using K4os.Compression.LZ4;
using MinesServer.Networking.Exceptions;
using MinesServer.Utils;
using System;
using System.Buffers;

namespace MinesServer.Networking.Server.Packets.Compression;

public readonly record struct LZ4Packet(IRootServerPacket Payload) : IRootServerPacket<LZ4Packet>, IUnreliableLengthPacket
{
    public ushort PacketCode => RootServerPacketCodeProvider.Cache<LZ4Packet>.Code;

    /// <summary>
    /// Верхняя граница: префикс длины плюс худший случай сжатия. Оценка включает
    /// <see cref="ushort"/> кода пакета, который пишется в поток перед payload'ом.
    /// </summary>
    public int Size =>
        sizeof(int) + // Uncompressed size
        LZ4Codec.MaximumOutputSize(sizeof(ushort) + Payload.Size);

    public int Encode(Span<byte> output)
    {
        // Сжимается префикс записанного, а не весь арендованный буфер: иначе в
        // поток уходит неинициализированный хвост, который декодер игнорирует.
        var uncompressedSize = sizeof(ushort) + Payload.Size;
        byte[] rented = ArrayPool<byte>.Shared.Rent(uncompressedSize);
        try
        {
            var encw = rented.AsSpan().Writer();
            encw.Write(Payload.PacketCode);
            encw.Advance(Payload.Encode(encw.Remaining));
            int written = encw.Position;

            var writer = output.Writer();
            writer.Write(written);
            // ArrayPool, а не stackalloc: порог stackalloc был 4 МБ при
            // дефолтном размере стека 1 МБ, то есть переполнял его.
            int compressed = LZ4Codec.Encode(rented.AsSpan(0, written), writer.Remaining, LZ4Level.L00_FAST);
            return writer.Position + compressed;
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(rented);
        }
    }

    public static LZ4Packet Decode(ReadOnlySpan<byte> input)
    {
        if (input.Length < sizeof(int))
            throw new PacketDecodeException($"LZ4 frame is truncated: {input.Length} bytes, expected at least {sizeof(int)}");

        var reader = input.Reader();
        var declared = reader.Read4();

        // До MaximumOutputSize: он домножает объявленное (n + n/255 + 16), поэтому
        // лимитировать надо сам заголовок, иначе кадр в несколько байт форсирует
        // аллокацию на гигабайты.
        if (declared < sizeof(ushort) || declared > ProtocolLimits.MaxDecompressedLength)
            throw new PacketDecodeException($"LZ4 uncompressed size {declared} is out of range [0, {ProtocolLimits.MaxDecompressedLength}]");

        var len = LZ4Codec.MaximumOutputSize(declared);
        byte[] decoded = ArrayPool<byte>.Shared.Rent(len);
        try
        {
            var encoded = decoded.AsSpan(0, len);
            LZ4Codec.Decode(reader.Remaining, encoded);
            var innerReader = encoded.Reader();
            var code = innerReader.ReadU2();
            if (!RootServerPacketCodeProvider.TryGetDecoder(code, out var decoder))
                throw new PacketDecodeException("Unknown packet code");
            return new(decoder(innerReader.Remaining));
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(decoded);
        }
    }
}
