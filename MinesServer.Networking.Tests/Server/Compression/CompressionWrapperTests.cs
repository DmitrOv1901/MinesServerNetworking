using System.Buffers.Binary;
using MinesServer.Networking.Exceptions;
using MinesServer.Networking.Server.Packets.Compression;
using MinesServer.Networking.Server.Packets.GUI;
using MinesServer.Networking.Server.Packets.GUI.Components;
using MinesServer.Networking.Server.Packets.GUI.Components.Containers;
using MinesServer.Networking.Server.Packets.GUI.Components.Visual;

namespace MinesServer.Networking.Tests.Server.Compression;

/// <summary>
/// Общие проверки обёрток сжатия: заголовок обязан совпадать с фактически
/// сжатыми данными, а объявленный размер распаковки — проверяться до аллокации.
///
/// Round-trip с мусорным хвостом проходил (декодер хвост игнорирует), поэтому
/// нужен прямой тест на заголовок.
/// </summary>
internal class CompressionWrapperTests
{
    private static OpenWindowPacket Payload(string text) => new("TestWindow", 50, 50, new DockPanelPacket
    {
        Children = [new TextPacket { Text = text }]
    });

    /// <summary>
    /// Payload крупнее прежнего порога stackalloc (1 МБ).
    ///
    /// Один <see cref="TextPacket"/> не годится: его текст идёт с префиксом ushort,
    /// максимум 65535 символов.
    /// </summary>
    private static OpenWindowPacket LargePayload()
    {
        var children = new List<IGUIComponentPacket>();
        var line = new string('a', 20_000);
        for (int i = 0; i < 64; i++)
            children.Add(new TextPacket { Text = line });

        return new OpenWindowPacket("BigWindow", 50, 50, new DockPanelPacket { Children = children });
    }

    /// <summary>
    /// Заголовок = длина фактически закодированного prefix'а (код пакета + payload),
    /// а не длина арендованного буфера.
    /// </summary>
    [Test]
    public void LZ4_HeaderEqualsActualCompressedInputSize()
    {
        var packet = new LZ4Packet(Payload("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
        var buffer = new byte[packet.Size];
        int written = packet.Encode(buffer);

        int declared = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        int expected = sizeof(ushort) + packet.Payload.Size;

        Assert.Multiple(() =>
        {
            Assert.That(declared, Is.EqualTo(expected),
                "header must hold the actually written input size, not the rented buffer length");
            Assert.That(written, Is.LessThanOrEqualTo(packet.Size));
        });
    }

    [Test]
    public void LZMA_HeaderEqualsActualCompressedInputSize()
    {
        var packet = new LzmaPacket(Payload("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa"));
        var buffer = new byte[packet.Size];
        int written = packet.Encode(buffer);

        int declared = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        int expected = sizeof(ushort) + packet.Payload.Size;

        Assert.Multiple(() =>
        {
            Assert.That(declared, Is.EqualTo(expected),
                "header must hold the actually written input size, not the ArrayPool rent length");
            Assert.That(written, Is.LessThanOrEqualTo(packet.Size));
        });
    }

    /// <summary>
    /// Починка префикса не должна была сломать декодирование.
    /// </summary>
    [Test]
    public void LZ4_RoundTripsPayload()
    {
        var packet = new LZ4Packet(Payload("round trip payload"));
        var buffer = new byte[packet.Size];
        packet.Encode(buffer);

        var decoded = LZ4Packet.Decode(buffer);
        Assert.That(decoded.Payload, Is.EqualTo(packet.Payload));
    }

    [Test]
    public void LZMA_RoundTripsPayload()
    {
        var packet = new LzmaPacket(Payload("round trip payload"));
        var buffer = new byte[packet.Size];
        packet.Encode(buffer);

        var decoded = LzmaPacket.Decode(buffer);
        Assert.That(decoded.Payload, Is.EqualTo(packet.Payload));
    }

    /// <summary>
    /// Кадр в пару байт, объявляющий ~2 ГБ. Без проверки заголовка LZ4 домножает
    /// его (<c>MaximumOutputSize</c>), а LZMA отдаёт напрямую в <c>outSize</c> —
    /// обе аллоцируют по запросу из провода.
    ///
    /// Обе обёртки декодируются на клиенте, то есть удалённый игрок на них не
    /// влияет: вектор — MITM (в протоколе нет ни TLS, ни подписи).
    /// </summary>
    [Test]
    public void LZ4_OversizedHeaderIsRejectedBeforeAllocation()
    {
        var bomb = new byte[sizeof(int) + 8];
        BinaryPrimitives.WriteInt32LittleEndian(bomb, int.MaxValue);

        var ex = Assert.Throws<PacketDecodeException>(() => LZ4Packet.Decode(bomb));
        Assert.That(ex!.Message, Does.Contain("out of range"));
    }

    [Test]
    public void LZMA_OversizedHeaderIsRejectedBeforeAllocation()
    {
        var bomb = new byte[sizeof(int) + 8];
        BinaryPrimitives.WriteInt32LittleEndian(bomb, int.MaxValue);

        var ex = Assert.Throws<PacketDecodeException>(() => LzmaPacket.Decode(bomb));
        Assert.That(ex!.Message, Does.Contain("out of range"));
    }

    [Test]
    public void LZ4_HeaderJustOverLimitIsRejected()
    {
        var frame = new byte[sizeof(int) + 8];
        BinaryPrimitives.WriteInt32LittleEndian(frame, ProtocolLimits.MaxDecompressedLength + 1);

        Assert.Throws<PacketDecodeException>(() => LZ4Packet.Decode(frame));
    }

    [Test]
    public void LZMA_HeaderJustOverLimitIsRejected()
    {
        var frame = new byte[sizeof(int) + 8];
        BinaryPrimitives.WriteInt32LittleEndian(frame, ProtocolLimits.MaxDecompressedLength + 1);

        Assert.Throws<PacketDecodeException>(() => LzmaPacket.Decode(frame));
    }

    [Test]
    public void LZ4_NegativeHeaderIsRejected()
    {
        var frame = new byte[sizeof(int) + 8];
        BinaryPrimitives.WriteInt32LittleEndian(frame, -1);

        Assert.Throws<PacketDecodeException>(() => LZ4Packet.Decode(frame));
    }

    [Test]
    public void LZMA_NegativeHeaderIsRejected()
    {
        var frame = new byte[sizeof(int) + 8];
        BinaryPrimitives.WriteInt32LittleEndian(frame, -1);

        Assert.Throws<PacketDecodeException>(() => LzmaPacket.Decode(frame));
    }

    [Test]
    public void LZ4_TruncatedHeaderIsRejected()
    {
        Assert.Throws<PacketDecodeException>(() => LZ4Packet.Decode(new byte[2]));
    }

    [Test]
    public void LZMA_TruncatedHeaderIsRejected()
    {
        Assert.Throws<PacketDecodeException>(() => LzmaPacket.Decode(new byte[2]));
    }

    /// <summary>
    /// Регрессия stackalloc: порог был 4 МБ при стеке 1 МБ, то есть payload
    /// крупнее 1 МБ переполнял стек.
    /// </summary>
    [Test]
    public void LZ4_LargePayloadBeyondStackallocThreshold_DoesNotOverflowStack()
    {
        var payload = LargePayload();
        var packet = new LZ4Packet(payload);

        Assert.That(packet.Size, Is.GreaterThan(1024 * 1024),
            "payload must exceed the old stackalloc threshold of 1 MB");

        var buffer = new byte[packet.Size];
        int written = packet.Encode(buffer);

        var decoded = LZ4Packet.Decode(buffer.AsSpan(0, written));
        Assert.That(decoded.Payload, Is.EqualTo(payload));
    }

    [Test]
    public void LZMA_LargePayloadRoundTrips()
    {
        var payload = LargePayload();
        var packet = new LzmaPacket(payload);

        var buffer = new byte[packet.Size];
        int written = packet.Encode(buffer);

        var decoded = LzmaPacket.Decode(buffer.AsSpan(0, written));
        Assert.That(decoded.Payload, Is.EqualTo(payload));
    }

    /// <summary>
    /// Сжимаемый payload обязан ужиматься: иначе мусорный хвост съедает всю
    /// экономию.
    /// </summary>
    [Test]
    public void LZ4_CompressiblePayloadActuallyShrinks()
    {
        var packet = new LZ4Packet(Payload(new string('a', 32 * 1024)));
        var buffer = new byte[packet.Size];
        int written = packet.Encode(buffer);

        Assert.That(written, Is.LessThan(packet.Payload.Size / 2),
            "a highly compressible payload must compress well below its raw size");
    }

    [Test]
    public void LZMA_CompressiblePayloadActuallyShrinks()
    {
        var packet = new LzmaPacket(Payload(new string('a', 32 * 1024)));
        var buffer = new byte[packet.Size];
        int written = packet.Encode(buffer);

        Assert.That(written, Is.LessThan(packet.Payload.Size / 2),
            "a highly compressible payload must compress well below its raw size");
    }
}