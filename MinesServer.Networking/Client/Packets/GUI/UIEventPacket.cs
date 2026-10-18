using MinesServer.Networking.Shared.Packets;
using MinesServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesServer.Networking.Client.Packets.GUI;

/// <summary>
/// Событие элемента v2-окна (клиент → сервер). Замена строковых action для
/// окон, построенных UIWindowPacket: элемент адресуется целочисленным id
/// (attached-свойство "UI.Id"), окно — ASCII-тегом. WindowTag пишем UTF-8 с
/// байтовым префиксом вручную — по тем же причинам, что и в ElementClickPacket:
/// WriteU1PrefixedUtf8 требует запас (len+1)*3 и падает на точном буфере Size.
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
        + Encoding.UTF8.GetByteCount(WindowTag) // WindowTag (UTF-8)
        + sizeof(uint) // ElementId
        + sizeof(byte) // EventType
        + Context.Sum(x => x.Size); // Context

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        byte[] tagBytes = Encoding.UTF8.GetBytes(WindowTag);
        writer.Write((byte)tagBytes.Length);
        writer.WriteArray(tagBytes);
        writer.Write(ElementId);
        writer.Write((byte)EventType);
        foreach (var element in Context)
            writer.Write(element);
        return writer.Position;
    }

    public static UIEventPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var tag = reader.ReadU1PrefixedUtf8(out _);
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
