using MinesServer.Networking.Exceptions;
using MinesServer.Utils;
using System;
using System.Runtime.InteropServices;

namespace MinesServer.Networking.Client.Packets;

public readonly record struct ClientPacket(uint Timestamp, IRootClientPacket Data) : IClientPacket<ClientPacket>
{
    public int Size =>
        sizeof(int) + // Length
        sizeof(uint) + // Timestamp
        sizeof(byte) + // Packet code
        Data.Size; // Payload

    public static ClientPacket Decode(ReadOnlySpan<byte> input)
    {
        try
        {
            var reader = input.Reader();
            var length = reader.Read4();
            var timestamp = reader.ReadU4();
            var packetCode = reader.ReadU1();
            if (!RootClientPacketCodeProvider.TryGetDecoder(packetCode, out var decoder))
                throw new PacketDecodeException("Unknown packet code", input);
            var result = new ClientPacket(timestamp, decoder(reader.Remaining));
            if (result.Data is IUnreliableLengthPacket)
            {
                if (result.Size < length)
                    throw new PacketDecodeException("Invalid packet length", input);
            }
            else if (result.Size != length)
                throw new PacketDecodeException("Invalid packet length", input);
            return result;
        }
        catch (PacketDecodeException) { throw; }
        catch (Exception e)
        {
            throw new PacketDecodeException("Packet failed to decode, potentially malicious payload", e, input);
        }
    }

    public int Encode(Span<byte> output)
    {
        var writer = output[sizeof(int)..].Writer();
        writer.Write(Timestamp);
        writer.Write(Data.PacketCode);
        writer.Write(Data);
        var pos = writer.Position + sizeof(int);
        MemoryMarshal.Write(output, ref pos);
        return pos;
    }
}
