using MinesServer.Networking.Shared.Packets;
using MinesServer.Utils;
using System;
using System.Linq;
using System.Text;

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
        + sizeof(byte) // OnClickContext length prefix
        + Encoding.UTF8.GetByteCount(OnClickContext) // OnClickContext (UTF-8)
        + sizeof(byte) // AttachedProperties.Length
        + AttachedProperties.Sum(x => x.Size); // AttachedProperties

    public virtual int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.Write(Style is not null);
        if(Style is not null)
            writer.Write(Style.Value);
        // OnClickContext несёт action кнопок легаси-окон ("Продать:%M%", «Ордеры») —
        // кириллица. "ASCII"-вариант режет char до младшего байта: клиент получает
        // мусор, эхом возвращает его в ElementClickPacket.WindowTag, и серверный
        // MButton.ProcessButton тихо не матчится. UTF-8 с байтовым префиксом
        // побайтно совместим со старым форматом для ASCII-строк. Кодируем во
        // временный массив: WriteU1PrefixedUtf8 требует запас (len+1)*3 байта и
        // падает на точном буфере Size, а спановой перегрузки GetBytes нет на net481.
        byte[] actionBytes = Encoding.UTF8.GetBytes(OnClickContext);
        writer.Write((byte)actionBytes.Length);
        writer.WriteArray(actionBytes);
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
        OnClickContext = reader.ReadU1PrefixedUtf8(out _);
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
