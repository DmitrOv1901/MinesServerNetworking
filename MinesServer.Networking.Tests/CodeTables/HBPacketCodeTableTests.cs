using System.Buffers.Binary;
using MinesServer.Networking.Server.Packets.World;

namespace MinesServer.Networking.Tests.CodeTables;

/// <summary>
/// Снапшот вложенной таблицы кодов hub-потока (0–5).
///
/// <c>HBPacket</c> — RT-кадр, payload которого список под-пакетов со своим
/// 1-байтовым кодом; перестановка Register посередине молча меняет смысл всех
/// последующих кодов.
/// </summary>
internal class HBPacketCodeTableTests
{
    [Test]
    public void HBPacketCodes_MatchRegistrationOrder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(HBPacketCodeProvider.GetPacketCode<MapRegionPacket>(), Is.EqualTo((byte)0));
            Assert.That(HBPacketCodeProvider.GetPacketCode<BuildingPacket>(), Is.EqualTo((byte)1));
            Assert.That(HBPacketCodeProvider.GetPacketCode<RobotPositionPacket>(), Is.EqualTo((byte)2));
            Assert.That(HBPacketCodeProvider.GetPacketCode<AudioPacket>(), Is.EqualTo((byte)3));
            Assert.That(HBPacketCodeProvider.GetPacketCode<VFXPacket>(), Is.EqualTo((byte)4));
            Assert.That(HBPacketCodeProvider.GetPacketCode<RemoveBuildingPacket>(), Is.EqualTo((byte)5));
        });
    }

    [Test]
    public void CodeZero_IsValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(HBPacketCodeProvider.IsValid((byte)0), Is.True);
            Assert.That(HBPacketCodeProvider.TryGetDecoder((byte)0, out var decoder), Is.True);
            Assert.That(decoder, Is.Not.Null);
        });
    }

    [Test]
    public void UnknownCode_HasNoDecoder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(HBPacketCodeProvider.IsValid((byte)6), Is.False);
            Assert.That(HBPacketCodeProvider.TryGetDecoder((byte)6, out _), Is.False);
        });
    }

    /// <summary>
    /// Регрессия «HBPacket size mismatch: claimed N, encoded N + Count»:
    /// <c>Size</c> не учитывал 1 байт кода каждого события, поэтому для кадра с
    /// несколькими событиями фактический размер кодирования превышал заявленный.
    /// </summary>
    [Test]
    public void MultiEventHub_FrameSizeMatchesEncoding()
    {
        var region = new MapRegionPacket(
            10, 20, 1, 1,
            new[] { Data.CellType.Unloaded, Data.CellType.Unloaded, Data.CellType.Unloaded, Data.CellType.Unloaded });
        var hub = new HBPacket(new IHBPacket[]
        {
            region,
            new RemoveBuildingPacket(42, 43),
            new BuildingPacket(1, 2, Data.BuildingType.Gun, 0, 0),
        });
        var frame = new global::MinesServer.Networking.Server.Packets.ServerPacket(hub);

        var buffer = new byte[frame.Size + 64];
        int actual = frame.Encode(buffer);

        Assert.That(actual, Is.EqualTo(frame.Size), "encoded hub frame size must equal ServerPacket.Size");
        int prefix = BinaryPrimitives.ReadInt32LittleEndian(buffer);
        Assert.That(prefix, Is.EqualTo(actual), "frame length prefix must match the actually encoded size");

        var decoded = (HBPacket)global::MinesServer.Networking.Server.Packets.ServerPacket
            .Decode(buffer.AsSpan(0, actual)).Payload;
        Assert.That(decoded, Is.EqualTo(hub));
    }
}