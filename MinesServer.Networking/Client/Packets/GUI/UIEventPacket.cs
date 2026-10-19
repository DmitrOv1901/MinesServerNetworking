using MinesServer.Networking.Shared.Packets;
using MinesServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesServer.Networking.Client.Packets.GUI;

/// <summary>
/// Событие элемента v2-окна (клиент → сервер). Элемент адресуется
/// целочисленным id (attached-свойство "UI.Id"), окно — тегом-идентификатором,
/// по которому сервер сверяет, с тем же ли окном работает клиент.
/// Тег v2-окна ASCII по контракту (его генерирует сервер): пишется
/// WriteU1PrefixedASCII, как в UIWindowPacket/WindowUpdatePacket.
/// </summary>
public readonly record struct UIEventPacket(
    string WindowTag,
    uint ElementId,
    UIEventType EventType,
    IReadOnlyList<StringPairPacket> Context) : IRootClientPacket<UIEventPacket>
{
    public byte PacketCode => RootClientPacketCodeProvider.Cache<UIEventPacket>.Code;

    public int Size =>
        sizeof(byte) // WindowTag length prefix
        + WindowTag.Length // WindowTag (ASCII)
        + sizeof(uint) // ElementId
        + sizeof(byte) // EventType
        + Context.Sum(x => x.Size); // Context

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.WriteU1PrefixedASCII(WindowTag);
        writer.Write(ElementId);
        writer.Write((byte)EventType);
        foreach (var element in Context)
            writer.Write(element);
        return writer.Position;
    }

    public static UIEventPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var tag = reader.ReadU1PrefixedASCII(out _);
        var elementId = reader.ReadU4();
        var eventType = (UIEventType)reader.ReadU1();
        var context = new List<StringPairPacket>();
        while (reader.CanRead)
        {
            var pair = StringPairPacket.Decode(reader.Remaining);
            context.Add(pair);
            reader.Advance(pair.Size);
        }
        return new(tag, elementId, eventType, context);
    }

    public bool Equals(UIEventPacket other) =>
        WindowTag == other.WindowTag &&
        ElementId == other.ElementId &&
        EventType == other.EventType &&
        Context.SequenceEqual(other.Context);
}
