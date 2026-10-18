using MinesServer.Networking.Shared.Packets;
using MinesServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MinesServer.Networking.Client.Packets.GUI;

public readonly record struct ElementClickPacket(string WindowTag, int ElementIndex, IReadOnlyList<StringPairPacket> Context) : IRootClientPacket<ElementClickPacket>
{
    public byte PacketCode => RootClientPacketCodeProvider.Cache<ElementClickPacket>.Code;

    public int Size =>
        sizeof(byte) // WindowTag length prefix
        + Encoding.UTF8.GetByteCount(WindowTag) // WindowTag (UTF-8)
        + sizeof(int) // ElementIndex
        + Context.Sum(x => x.Size); // Context

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        // WindowTag несёт action кнопки легаси-окон ("Продать:%M%") — кириллица.
        // "ASCII"-вариант писателя режет каждый char до младшего байта: сервер
        // получает мусор, MButton.ProcessButton не матчится и кнопки рынка
        // молча умирают. UTF-8 с байтовым префиксом побайтно совпадает со старым
        // форматом для ASCII-строк, так что протокол остаётся совместимым.
        // Байты кодируем во временный массив: WriteU1PrefixedUtf8 требует запас
        // (len+1)*3 байта и на точном буфере Size кидает ArgumentOutOfRangeException,
        // а спановой перегрузки GetBytes нет на net481.
        byte[] tagBytes = Encoding.UTF8.GetBytes(WindowTag);
        writer.Write((byte)tagBytes.Length);
        writer.WriteArray(tagBytes);
        writer.Write(ElementIndex);
        foreach (var element in Context)
            writer.Write(element);
        return writer.Position;
    }

    public static ElementClickPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var tag = reader.ReadU1PrefixedUtf8(out _);
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
