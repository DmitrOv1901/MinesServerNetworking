using MinesServer.Networking.Exceptions;
using MinesServer.Networking.Server.Packets.GUI.Components;
using MinesServer.Utils;
using System;

namespace MinesServer.Networking.Server.Packets.GUI;

/// <summary>
/// v2-окно: сервер-авторитарный аналог OpenWindowPacket. Отличия: собственный
/// заголовок с хромом (title bar, drag, resize, крестик по Closable), лимиты
/// размеров и позиция. Тег окна — ASCII (сервер генерирует его сам), заголовок
/// — UTF-16 (кириллица). Content — любой IGUIComponentPacket (обычно CanvasPacket,
/// чтобы дочерние элементы позиционировались по координатам через Canvas.*).
/// Повторная отправка с тем же WindowTag заменяет содержимое живого окна (rebuild).
/// </summary>
public readonly record struct UIWindowPacket(
    string WindowTag,
    string Title,
    ushort Width,
    ushort Height,
    ushort MinWidth,
    ushort MinHeight,
    short PosX,
    short PosY,
    UIWindowFlags Flags,
    IGUIComponentPacket Content) : IRootServerPacket<UIWindowPacket>
{
    /// <summary>Спецзначение позиции: клиент центрирует окно сам.</summary>
    public const short AutoPosition = short.MinValue;

    public static readonly UIWindowFlags DefaultFlags =
        UIWindowFlags.Draggable | UIWindowFlags.Resizable | UIWindowFlags.Closable | UIWindowFlags.AutoCenter;

    public ushort PacketCode => RootServerPacketCodeProvider.Cache<UIWindowPacket>.Code;

    public readonly int Size =>
        sizeof(byte) // WindowTag.Length
        + WindowTag.Length // WindowTag (ASCII)
        + sizeof(ushort) // Title.Length
        + Title.Length * 2 // Title (UTF-16)
        + sizeof(ushort) // Width
        + sizeof(ushort) // Height
        + sizeof(ushort) // MinWidth
        + sizeof(ushort) // MinHeight
        + sizeof(short) // PosX
        + sizeof(short) // PosY
        + sizeof(byte) // Flags
        + sizeof(byte) // Content packet code
        + Content.Size; // Content

    public static UIWindowPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var tag = reader.ReadU1PrefixedASCII(out _);
        var title = reader.ReadU2PrefixedUtf16(out _);
        var width = reader.ReadU2();
        var height = reader.ReadU2();
        var minWidth = reader.ReadU2();
        var minHeight = reader.ReadU2();
        var posX = reader.Read2();
        var posY = reader.Read2();
        var flags = (UIWindowFlags)reader.ReadU1();
        var packetCode = reader.ReadU1();
        if (!GUIComponentPacketCodeProvider.TryGetDecoder(packetCode, out var decoder))
            throw new PacketDecodeException("Invalid packet code");
        var content = decoder(reader.Remaining);
        return new(tag, title, width, height, minWidth, minHeight, posX, posY, flags, content);
    }

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.WriteU1PrefixedASCII(WindowTag);
        writer.WriteU2PrefixedUtf16(Title);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write(MinWidth);
        writer.Write(MinHeight);
        writer.Write(PosX);
        writer.Write(PosY);
        writer.Write((byte)Flags);
        writer.Write(Content.PacketCode);
        writer.Write(Content);
        return writer.Position;
    }
}
