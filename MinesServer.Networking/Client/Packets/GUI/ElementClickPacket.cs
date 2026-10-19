using MinesServer.Networking.Shared.Packets;
using MinesServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesServer.Networking.Client.Packets.GUI;

public readonly record struct ElementClickPacket(string WindowTag, int ElementIndex, IReadOnlyList<StringPairPacket> Context) : IRootClientPacket<ElementClickPacket>
{
    public byte PacketCode => RootClientPacketCodeProvider.Cache<ElementClickPacket>.Code;

    public int Size =>
        sizeof(byte) // WindowTag.Length
        + WindowTag.Length // WindowTag
        + sizeof(int) // ElementIndex
        + Context.Sum(x => x.Size); // Context

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.WriteU1PrefixedASCII(WindowTag);
        writer.Write(ElementIndex);
        foreach (var element in Context)
            writer.Write(element);
        return writer.Position;
    }

    public static ElementClickPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var tag = reader.ReadU1PrefixedASCII(out _);
        var index = reader.Read4();
        var context = new List<StringPairPacket>();
        while(reader.CanRead)
        {
            var pair = StringPairPacket.Decode(reader.Remaining);
            context.Add(pair);
            reader.Advance(pair.Size);
        }
        return new(tag, index, context);
    }

    public bool Equals(ElementClickPacket other) =>
        WindowTag == other.WindowTag &&
        ElementIndex == other.ElementIndex &&
        Context.SequenceEqual(other.Context);
}
