using MinesServer.Utils;
using System;

namespace MinesServer.Networking.Server.Packets.GUI.Components.Visual;

/// <summary>
/// v2-кнопка: самостоятельный визуальный компонент (не TextPacket с OnClickContext).
/// Идентичность — целочисленный id в attached-свойстве "UI.Id": клиент шлёт
/// UIEventPacket(WindowTag, id, Click), сервер диспетчеризует по id без строк.
/// Легаси OnClickContext поддержан, но для v2-окон остаётся пустым.
/// </summary>
public record class ButtonPacket : GUIComponentPacket, IGUIComponentPacket<ButtonPacket>
{
    public override byte PacketCode => GUIComponentPacketCodeProvider.Cache<ButtonPacket>.Code;

    public string Label { get; init; } = "";
    public string Tooltip { get; init; } = "";
    public string IconURI { get; init; } = "";
    public ButtonStyleHints Hints { get; init; }

    public override int Size => base.Size
        + sizeof(ushort) // Label.Length
        + Label.Length * 2 // Label (UTF-16)
        + sizeof(ushort) // Tooltip.Length
        + Tooltip.Length * 2 // Tooltip (UTF-16)
        + sizeof(byte) // IconURI.Length
        + IconURI.Length // IconURI (ASCII)
        + sizeof(byte); // Hints

    public override int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.Advance(base.Encode(output));
        writer.WriteU2PrefixedUtf16(Label);
        writer.WriteU2PrefixedUtf16(Tooltip);
        writer.WriteU1PrefixedASCII(IconURI);
        writer.Write((byte)Hints);
        return writer.Position;
    }

    public static ButtonPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        return new(ref reader);
    }

    protected ButtonPacket(ref MemoryReader reader) : base(ref reader)
    {
        Label = reader.ReadU2PrefixedUtf16(out _);
        Tooltip = reader.ReadU2PrefixedUtf16(out _);
        IconURI = reader.ReadU1PrefixedASCII(out _);
        Hints = (ButtonStyleHints)reader.ReadU1();
    }

    public ButtonPacket() : base() { }
}
