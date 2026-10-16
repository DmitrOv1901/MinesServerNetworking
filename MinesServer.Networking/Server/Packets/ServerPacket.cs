using MinesServer.Networking.Exceptions;
using MinesServer.Utils;
using System;
using System.Runtime.InteropServices;

namespace MinesServer.Networking.Server.Packets;

public readonly record struct ServerPacket(IRootServerPacket Payload) : IServerPacket<ServerPacket>
{
    public int Size => 
        sizeof(int) + // Length
        sizeof(ushort) + // Packet code
        Payload.Size; // Payload

    public static ServerPacket Decode(ReadOnlySpan<byte> input)
    {
        try {
            var reader = input.Reader();
            var length = reader.Read4();
            var packetCode = reader.ReadU2();
            if (!RootServerPacketCodeProvider.TryGetDecoder(packetCode, out var decoder))
                throw new PacketDecodeException("Unknown packet code", input);
            var result = new ServerPacket(decoder(reader.Remaining));
            if (result.Payload is IUnreliableLengthPacket) {
                if (result.Size < length)
                    throw new PacketDecodeException("Invalid packet length", input);
            }
            else if(result.Size != length)
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
        writer.Write(Payload.PacketCode);
        writer.Write(Payload);
        var pos = writer.Position + sizeof(int);
        MemoryMarshal.Write(output, ref pos);
        return pos;
    }
}
