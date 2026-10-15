using System.Buffers.Binary;
using MinesServer.Networking.Connection;
using MinesServer.Networking.Exceptions;

namespace MinesServer.Networking.Tests.Connection;

/// <summary>
/// Тесты кадрирования. До них у <see cref="PacketBuffer"/> не было ни одного
/// теста, хотя именно он держит синхронизацию потока кадров.
/// </summary>
internal class PacketBufferTests
{
    private static void Feed(PacketBuffer buffer, byte[] frame) => buffer.Put(frame, frame.Length);

    private static void Feed(PacketBuffer buffer, byte[] data, int offset, int length) => buffer.Put(data, offset, length);

    /// <summary>
    /// Собирает кадр: префикс длины — это полный размер кадра, включая сам
    /// префикс, то есть ровно <c>buffer.Length</c> (так же считает
    /// <c>ServerPacket.Encode</c>). TryTake возвращает кадр целиком, вместе с
    /// префиксом.
    /// </summary>
    private static byte[] Frame(params byte[] payload)
    {
        var frame = new byte[sizeof(int) + payload.Length];
        BinaryPrimitives.WriteInt32LittleEndian(frame, frame.Length);
        payload.CopyTo(frame, sizeof(int));
        return frame;
    }

    private static byte[] FrameOfLength(int length)
    {
        var frame = new byte[length];
        BinaryPrimitives.WriteInt32LittleEndian(frame, length);
        for (int i = sizeof(int); i < length; i++)
            frame[i] = (byte)(i & 0xFF);
        return frame;
    }

    private static List<byte[]> Drain(PacketBuffer buffer)
    {
        var taken = new List<byte[]>();
        while (buffer.TryTake(out var packet))
            taken.Add(packet);
        return taken;
    }

    [Test]
    public void SingleFrame_IsTaken()
    {
        var buffer = new PacketBuffer();
        var frame = Frame([1, 2, 3, 4, 5]);
        Feed(buffer, frame);

        Assert.That(buffer.TryTake(out var packet), Is.True);
        Assert.That(packet, Is.EqualTo(frame), "taken frame must include its length prefix");
        Assert.That(buffer.TryTake(out _), Is.False, "buffer must be empty after the only frame is taken");
    }

    /// <summary>
    /// Регрессия чтения длины кадра из <c>_buffer[0]</c> вместо
    /// <c>_buffer[_readPos]</c>.
    ///
    /// С багом два кадра в одном Put декодировались как один и тот же первый кадр:
    /// TryTake брал длину из нулевой позиции, но копировал тело из <c>_readPos</c>.
    /// В бою это выглядело бы как «клиент иногда повторяет действие».
    /// </summary>
    [Test]
    public void TwoFramesInOnePut_AreTakenSeparatelyAndInOrder()
    {
        var buffer = new PacketBuffer();
        var first = Frame([0xAA, 0xAA, 0xAA, 0xAA, 0xAA]);
        var second = Frame([0xBB, 0xBB, 0xBB]);
        Feed(buffer, first);
        Feed(buffer, second);

        var taken = Drain(buffer);

        Assert.Multiple(() =>
        {
            Assert.That(taken, Has.Count.EqualTo(2));
            Assert.That(taken[0], Is.EqualTo(first));
            Assert.That(taken[1], Is.EqualTo(second));
        });
    }

    [Test]
    public void ThreeFramesWithDifferentLengths_AreTakenInOrder()
    {
        var buffer = new PacketBuffer();
        var first = Frame([1]);
        var second = Frame([2, 2, 2, 2, 2, 2, 2]);
        var third = Frame([3, 3]);
        Feed(buffer, first);
        Feed(buffer, second);
        Feed(buffer, third);

        var taken = Drain(buffer);

        Assert.Multiple(() =>
        {
            Assert.That(taken, Has.Count.EqualTo(3));
            Assert.That(taken[0], Is.EqualTo(first));
            Assert.That(taken[1], Is.EqualTo(second));
            Assert.That(taken[2], Is.EqualTo(third));
        });
    }

    [Test]
    public void PartialFrame_IsNotTakenUntilComplete()
    {
        var buffer = new PacketBuffer();
        var frame = FrameOfLength(64);

        Feed(buffer, frame.AsSpan(0, 20).ToArray());
        Assert.That(buffer.TryTake(out _), Is.False, "incomplete frame must not be taken");

        Feed(buffer, frame.AsSpan(20, 44).ToArray());
        Assert.That(buffer.TryTake(out var packet), Is.True);
        Assert.That(packet, Is.EqualTo(frame));
    }

    /// <summary>
    /// Кадр приходит двумя сетевыми чтениями так, что второе заканчивается
    /// посреди следующего кадра. Это единственный путь, где TryTake вызывается
    /// при непустой позиции чтения, не сбросив её предварительно, — и именно
    /// он ловил чтение длины из <c>_buffer[0]</c>.
    /// </summary>
    [Test]
    public void SplitFrameEndingInsideNextFrame_IsResynchronised()
    {
        var buffer = new PacketBuffer();
        var first = Frame([0xCC, 0xDD, 0xEE]);
        var second = Frame([0x11, 0x22]);

        // Первое чтение — весь первый кадр плюс первые 4 байта второго.
        Feed(buffer, first.Concat(second.AsSpan(0, 4).ToArray()).ToArray());
        Assert.That(buffer.TryTake(out var takenFirst), Is.True);
        Assert.That(takenFirst, Is.EqualTo(first));

        // Второе чтение — остаток второго кадра.
        Feed(buffer, second.AsSpan(4).ToArray());
        Assert.That(buffer.TryTake(out var takenSecond), Is.True);
        Assert.That(takenSecond, Is.EqualTo(second));
    }

    [Test]
    public void PartialHeader_IsNotTakenUntilComplete()
    {
        var buffer = new PacketBuffer();
        var frame = Frame([4, 5, 6, 7]);

        Feed(buffer, frame.AsSpan(0, 2).ToArray()); // меньше префикса длины
        Assert.That(buffer.TryTake(out _), Is.False);

        Feed(buffer, frame.AsSpan(2).ToArray());
        Assert.That(buffer.TryTake(out var packet), Is.True);
        Assert.That(packet, Is.EqualTo(frame));
    }

    /// <summary>
    /// NetCoreServer отдаёт принятые данные в реюзнутом массиве, поэтому offset
    /// обязателен: копирование всегда с нуля подмешивает мусор.
    /// </summary>
    [Test]
    public void Put_WithNonZeroOffset_CopiesOnlyRequestedRange()
    {
        var buffer = new PacketBuffer();
        var dirty = new byte[128];
        for (int i = 0; i < dirty.Length; i++)
            dirty[i] = 0xEE;

        var frame = Frame([0x11, 0x22, 0x33]);
        frame.CopyTo(dirty, 40);

        Feed(buffer, dirty, 40, frame.Length);

        Assert.That(buffer.TryTake(out var packet), Is.True);
        Assert.That(packet, Is.EqualTo(frame));
    }

    [Test]
    public void Put_WithOutOfRangeOffset_IsRejected()
    {
        var buffer = new PacketBuffer();
        Assert.Throws<ArgumentOutOfRangeException>(() => Feed(buffer, new byte[8], 4, 8));
    }

    /// <summary>
    /// Проверка лимита живёт в TryTake, а не в Put: Put только складывает
    /// данные, кадр проверяется при попытке выделить из буфера.
    /// </summary>
    [Test]
    public void FrameLongerThanLimit_IsRejected()
    {
        var buffer = new PacketBuffer();
        var oversized = new byte[sizeof(int)];
        BinaryPrimitives.WriteInt32LittleEndian(oversized, PacketBuffer.DefaultMaxLength + 1);
        Feed(buffer, oversized);

        Assert.Throws<PacketDecodeException>(() => buffer.TryTake(out _));
    }

    [Test]
    public void DeclaredLengthBelowPrefix_IsRejected()
    {
        var buffer = new PacketBuffer();
        Feed(buffer, new byte[] { 2, 0, 0, 0, 9, 9 });

        Assert.Throws<PacketDecodeException>(() => buffer.TryTake(out _));
    }

    [Test]
    public void EmptyPut_IsNoOp()
    {
        var buffer = new PacketBuffer();
        Feed(buffer, Array.Empty<byte>());
        Assert.That(buffer.TryTake(out _), Is.False);
    }

    /// <summary>
    /// Буфер перевыделяется при росте кадра, и непрочитанные данные обязаны
    /// пережить перевыделение вместе с позицией чтения.
    /// </summary>
    [Test]
    public void BufferGrowth_KeepsPendingFrameIntact()
    {
        var buffer = new PacketBuffer(initialCapacity: 16, maxLength: 1024 * 1024);

        // Кадр крупнее начальной ёмкости — EnsureCapacity обязан перенести данные.
        var big = FrameOfLength(8192);
        Feed(buffer, big);

        Assert.That(buffer.TryTake(out var packet), Is.True);
        Assert.That(packet, Is.EqualTo(big));
    }

    /// <summary>
    /// Кадр приходит двумя сетевыми чтениями, и между ними буфер перевыделяется
    /// по EnsureCapacity. Позиция чтения и непрочитанные данные обязаны пережить
    /// перевыделение.
    /// </summary>
    [Test]
    public void PartialFrameSpanningBufferGrowth_IsResynchronised()
    {
        var buffer = new PacketBuffer(initialCapacity: 16, maxLength: 1024 * 1024);

        var first = FrameOfLength(4096);
        var second = FrameOfLength(64);

        Feed(buffer, first.AsSpan(0, 1000).ToArray());  // расширяет буфер
        Feed(buffer, first.AsSpan(1000).ToArray());     // дописывает остаток + второй кадр
        Feed(buffer, second);

        var taken = Drain(buffer);

        Assert.Multiple(() =>
        {
            Assert.That(taken, Has.Count.EqualTo(2));
            Assert.That(taken[0], Is.EqualTo(first));
            Assert.That(taken[1], Is.EqualTo(second));
        });
    }

    [Test]
    public void DefaultLimit_MatchesProtocolLimits()
    {
        Assert.Multiple(() =>
        {
            Assert.That(PacketBuffer.DefaultMaxLength, Is.EqualTo(ProtocolLimits.MaxFrameLength));
            Assert.That(PacketBuffer.DefaultMaxLength, Is.EqualTo(4 * 1024 * 1024));
            Assert.That(PacketBuffer.DefaultInitialCapacity, Is.EqualTo(ProtocolLimits.FrameBufferInitialCapacity));
        });
    }
}