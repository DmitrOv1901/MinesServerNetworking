using MinesServer.Networking.Exceptions;
using MinesServer.Networking.Server.Packets.GUI.Components;
using MinesServer.Utils;
using System;

namespace MinesServer.Networking.Server.Packets.GUI;

/// <summary>
/// Живое обновление элемента окна (по attached-свойству "UI.Id").
/// WindowTag — ASCII-тег окна, ElementId — числовой id элемента,
/// Component — новый компонент, целиком заменяющий старый.
/// </summary>
public readonly record struct WindowUpdatePacket(
    string WindowTag,
    uint ElementId,
    IGUIComponentPacket Component) : IRootServerPacket<WindowUpdatePacket>
{
    public ushort PacketCode => RootServerPacketCodeProvider.Cache<WindowUpdatePacket>.Code;

    public readonly int Size =>
        sizeof(byte) // WindowTag.Length
        + WindowTag.Length // WindowTag (ASCII)
        + sizeof(uint) // ElementId
        + sizeof(byte) // Component packet code
        + Component.Size; // Component

    public static WindowUpdatePacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var tag = reader.ReadU1PrefixedASCII(out _);
        var elementId = reader.ReadU4();
        var packetCode = reader.ReadU1();
        if (!GUIComponentPacketCodeProvider.TryGetDecoder(packetCode, out var decoder))
            throw new PacketDecodeException("Invalid packet code");
        var component = decoder(reader.Remaining);
        return new(tag, elementId, component);
    }

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.WriteU1PrefixedASCII(WindowTag);
        writer.Write(ElementId);
        writer.Write(Component.PacketCode);
        writer.Write(Component);
        return writer.Position;
    }
}
