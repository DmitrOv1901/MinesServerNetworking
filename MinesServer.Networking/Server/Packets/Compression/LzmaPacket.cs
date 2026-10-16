using MinesServer.Networking.Exceptions;
using MinesServer.Utils;
using SharpCompress.Compressors.LZMA;
using System;
using System.Buffers;
using System.IO;
using System.Runtime.InteropServices;

namespace MinesServer.Networking.Server.Packets.Compression;

public readonly record struct LzmaPacket(IRootServerPacket Payload) : IRootServerPacket<LzmaPacket>, IUnreliableLengthPacket
{
    private static readonly LzmaEncoderProperties _propsInstance = LzmaEncoderProperties.Default;
    private static readonly byte[] _encoderProperties = new LzmaStream(_propsInstance, false, null).Properties;

    public ushort PacketCode => RootServerPacketCodeProvider.Cache<LzmaPacket>.Code;

    // There is no reliable way to predict the length of a compressed lzma payload, so assume the worst and treat the payload as uncompressed.
    // This shouldn't affect encoding and decoding.
    public int Size =>
        sizeof(int) + // Length Header
        sizeof(ushort) + // Packet code
        Payload.Size; // Payload

    public int Encode(Span<byte> output)
    {
        // Сжимается префикс записанного, а не весь арендованный буфер: Rent возвращает
        // больше запрошенного, и в заголовок ушла бы длина аренды вместо
        // фактически записанных данных.
        int uncompressedSize = sizeof(ushort) + Payload.Size;
        byte[] encoded = ArrayPool<byte>.Shared.Rent(uncompressedSize);
        try
        {
            var writer = encoded.AsSpan().Writer();
            writer.Write(RootServerPacketCodeProvider.GetPacketCode(Payload));
            writer.Advance(Payload.Encode(writer.Remaining));
            int written = writer.Position;

            using var outStream = new MemoryStream();
            using (var lzStream = new LzmaStream(_propsInstance, false, outStream))
                lzStream.Write(encoded, 0, written);

            var compressedBytes = outStream.GetBuffer().AsSpan(0, (int)outStream.Length);

            var outWriter = output.Writer();
            outWriter.Write(written);
            outWriter.WriteSpan(compressedBytes);

            return outWriter.Position;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(encoded);
        }
    }

    public static LzmaPacket Decode(ReadOnlySpan<byte> input)
    {
        // Размер идёт напрямую как int32 мимо Read4(), в отличие от LZ4, и уходит
        // в outSize LzmaStream — поэтому проверять надо здесь же.
        if (input.Length < sizeof(int))
            throw new PacketDecodeException($"LZMA frame is truncated: {input.Length} bytes, expected at least {sizeof(int)}");

        var declared = MemoryMarshal.Read<int>(input);
        if (declared < sizeof(ushort) || declared > ProtocolLimits.MaxDecompressedLength)
            throw new PacketDecodeException($"LZMA uncompressed size {declared} is out of range [0, {ProtocolLimits.MaxDecompressedLength}]");

        using var outStream = new MemoryStream();
        var compressedPayload = input[sizeof(int)..];

        unsafe
        {
            fixed (byte* pInput = &MemoryMarshal.GetReference(compressedPayload))
            {
                using var inStream = new UnmanagedMemoryStream(pInput, compressedPayload.Length);
                using var lzStream = new LzmaStream(_encoderProperties, inStream, -1, declared);
                lzStream.CopyTo(outStream);
            }
        }

        ReadOnlySpan<byte> decoded = outStream.GetBuffer().AsSpan(0, (int)outStream.Length);
        var reader = decoded.Reader();
        var code = reader.ReadU2();
        if (!RootServerPacketCodeProvider.TryGetDecoder(code, out var decoder))
            throw new PacketDecodeException("Unknown packet code");
        return new(decoder(reader.Remaining));
    }
}