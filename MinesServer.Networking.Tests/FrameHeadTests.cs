using System.Buffers.Binary;
using MinesServer.Networking.Client.Packets;
using MinesServer.Networking.Connection;
using MinesServer.Networking.Exceptions;
using MinesServer.Networking.Server.Packets;

namespace MinesServer.Networking.Tests;

/// <summary>
/// Битое письмо должно само нести причину, иначе хост логирует «unknown packet
/// code» и не знает, какой код прислал клиент. Раньше hex летел в тексте
/// исключения из PacketBuffer, а ClientPacket.Decode его не добавлял вовсе — и
/// после перехода сессии на общий транспорт диагностика пропала бы целиком.
///
/// Ограничение обязательно: кадр достигает 4 МБ, и копировать его в исключение
/// на каждое битое письмо — вектор по памяти и по логам.
/// </summary>
internal class FrameHeadTests
{
    private static byte[] ClientFrame(byte code, int totalLength)
    {
        var frame = new byte[totalLength];
        BinaryPrimitives.WriteInt32LittleEndian(frame, totalLength);
        BinaryPrimitives.WriteUInt32LittleEndian(frame.AsSpan(sizeof(int)), 12345);
        frame[sizeof(int) + sizeof(uint)] = code;
        return frame;
    }

    [Test]
    public void UnknownClientCode_CarriesTheFrameItFailedOn()
    {
        var frame = ClientFrame(code: 200, totalLength: 9);

        var ex = Assert.Throws<PacketDecodeException>(() => ClientPacket.Decode(frame));
        var head = ex!.FrameHead;

        Assert.Multiple(() =>
        {
            Assert.That(head, Is.EqualTo(frame), "a short frame must arrive whole");
            // Без кода в дампе сообщение бесполезно: 200 здесь — и есть диагностика.
            Assert.That(head![8], Is.EqualTo((byte)200));
            Assert.That(ex.FrameHeadHex, Is.EqualTo(Convert.ToHexString(frame)));
        });
    }

    [Test]
    public void UnknownServerCode_CarriesTheFrameItFailedOn()
    {
        var frame = new byte[sizeof(int) + sizeof(ushort)];
        BinaryPrimitives.WriteInt32LittleEndian(frame, frame.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(sizeof(int)), 6000);

        var ex = Assert.Throws<PacketDecodeException>(() => ServerPacket.Decode(frame));

        Assert.That(ex!.FrameHead, Is.EqualTo(frame));
    }

    [Test]
    public void HeadIsCapped_ButTheHeaderSurvives()
    {
        // Крупный кадр: 9 байт заголовка (длина, timestamp, код) определяют пакет
        // полностью, а payload у битого кадра всё равно мусор.
        var frame = ClientFrame(code: 200, totalLength: 4096);

        var ex = Assert.Throws<PacketDecodeException>(() => ClientPacket.Decode(frame));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.FrameHead, Has.Length.EqualTo(PacketDecodeException.FrameHeadBytes));
            Assert.That(ex.FrameHead, Is.EqualTo(frame[..PacketDecodeException.FrameHeadBytes]));
            Assert.That(ex.FrameHeadHex, Has.Length.EqualTo(PacketDecodeException.FrameHeadBytes * 2));
        });
    }

    [Test]
    public void ShortFrameIsNotPaddedToTheCap()
    {
        // Обрезка по min(length, cap): кадр короче предела не раздувается.
        var frame = ClientFrame(code: 200, totalLength: 9);

        var ex = Assert.Throws<PacketDecodeException>(() => ClientPacket.Decode(frame));

        Assert.That(ex!.FrameHead, Has.Length.EqualTo(frame.Length));
    }

    [Test]
    public void BadFrameLength_CarriesTheUnreadTail()
    {
        var buffer = new PacketBuffer();
        var garbage = new byte[] { 2, 0, 0, 0, 0xDE, 0xAD, 0xBE, 0xEF };
        buffer.Put(garbage, garbage.Length);

        var ex = Assert.Throws<PacketDecodeException>(() => buffer.TryTake(out _));

        Assert.Multiple(() =>
        {
            Assert.That(ex!.FrameHead, Is.EqualTo(garbage));
            // Hex уехал из текста сообщения в байты: иначе хост не может его отдать
            // в Elastic отдельным полем.
            Assert.That(ex.Message, Does.Not.Contain("head="));
            Assert.That(ex.FrameHeadHex, Is.EqualTo("02000000DEADBEEF"));
        });
    }

    [Test]
    public void ExceptionWithoutAFrame_ReportsNoHead()
    {
        // Часть PacketDecodeException рождается без кадра (например, разбор
        // сохранённой программы): отсутствие дампа не должно выглядеть как пустой.
        var ex = new PacketDecodeException("Malformed program");

        Assert.Multiple(() =>
        {
            Assert.That(ex.FrameHead, Is.Null);
            Assert.That(ex.FrameHeadHex, Is.Empty);
        });
    }
}