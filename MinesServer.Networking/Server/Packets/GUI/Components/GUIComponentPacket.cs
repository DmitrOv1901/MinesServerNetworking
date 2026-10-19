using MinesServer.Networking.Shared.Packets;
using MinesServer.Utils;
using System;
using System.Linq;

namespace MinesServer.Networking.Server.Packets.GUI.Components;

public abstract record class GUIComponentPacket : IGUIComponentPacket
{
    public abstract byte PacketCode { get; }

    public GUIStylePacket? Style { get; init; }
    public string OnClickContext { get; init; } = "";
    public StringPairPacket[] AttachedProperties { get; init; } = Array.Empty<StringPairPacket>();

    public virtual int Size =>
        sizeof(bool) // Style.HasValue
        + (Style?.Size ?? 0) // Style.Value
        + sizeof(byte) // OnClickContext.Length
        + OnClickContext.Length // OnClickContext
        + sizeof(byte) // AttachedProperties.Length
        + AttachedProperties.Sum(x => x.Size); // AttachedProperties

    public virtual int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.Write(Style is not null);
        if(Style is not null)
            writer.Write(Style.Value);
        writer.WriteU1PrefixedASCII(OnClickContext);
        writer.Write((byte)AttachedProperties.Length);
        foreach (var prop in AttachedProperties)
            writer.Write(prop);
        return writer.Position;
    }

    protected GUIComponentPacket(ref MemoryReader reader)
    {
        var hasStyle = reader.Read<bool>();
        if (hasStyle)
        {
            Style = GUIStylePacket.Decode(reader.Remaining);
            reader.Advance(Style.Value.Size);
        }
        OnClickContext = reader.ReadU1PrefixedASCII(out _);
        AttachedProperties = new StringPairPacket[reader.ReadU1()];
        for (int i = 0; i < AttachedProperties.Length; i++)
        {
            AttachedProperties[i] = StringPairPacket.Decode(reader.Remaining);
            reader.Advance(AttachedProperties[i].Size);
        }
    }

    protected GUIComponentPacket() { }

    public virtual bool Equals(GUIComponentPacket? other) =>
        Style == other?.Style &&
        OnClickContext == other?.OnClickContext &&
        AttachedProperties.SequenceEqual(other?.AttachedProperties);
}
