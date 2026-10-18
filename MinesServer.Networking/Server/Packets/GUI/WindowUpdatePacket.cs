using MinesServer.Utils;
using System;

namespace MinesServer.Networking.Server.Packets.GUI;

/// <summary>
/// Живое обновление элемента v2-окна (WindowUpdatePacket-патч по UI.Id).
/// WindowTag — ASCII-тег окна, ElementId — числовой id элемента.
/// </summary>
public readonly record struct WindowUpdatePacket(
    string WindowTag,
    uint ElementId,
    UIPatchKind Kind,
    string Payload) : IRootServerPacket<WindowUpdatePacket>
{
    public ushort PacketCode => RootServerPacketCodeProvider.Cache<WindowUpdatePacket>.Code;

    public readonly int Size =>
        sizeof(byte) // WindowTag.Length
        + WindowTag.Length // WindowTag (ASCII)
        + sizeof(uint) // ElementId
        + sizeof(byte) // Kind
        + sizeof(ushort) // Payload.Length
        + Payload.Length * 2; // Payload (UTF-16)

    public static WindowUpdatePacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        return new(
            reader.ReadU1PrefixedASCII(out _),
            reader.ReadU4(),
            (UIPatchKind)reader.ReadU1(),
            reader.ReadU2PrefixedUtf16(out _));
    }

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.WriteU1PrefixedASCII(WindowTag);
        writer.Write(ElementId);
        writer.Write((byte)Kind);
        writer.WriteU2PrefixedUtf16(Payload);
        return writer.Position;
    }
}
