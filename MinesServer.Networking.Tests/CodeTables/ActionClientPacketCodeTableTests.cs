using MinesServer.Networking.Client.Packets;
using MinesServer.Networking.Client.Packets.Actions;
using MinesServer.Networking.Client.Packets.Movement;

namespace MinesServer.Networking.Tests.CodeTables;

/// <summary>
/// Снапшот таблицы кодов игровых действий (0–13).
///
/// Адресует вообще все игровые действия, и перестановка Register молча отправила
/// бы «копать» в «повернуться» — это выглядело бы как «сервер иногда не копает».
/// </summary>
internal class ActionClientPacketCodeTableTests
{
    [Test]
    public void ActionPacketCodes_MatchRegistrationOrder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<BuildCyanPacket>(), Is.EqualTo((byte)0));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<BuildGrayPacket>(), Is.EqualTo((byte)1));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<BuildGreenPacket>(), Is.EqualTo((byte)2));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<BuildWhitePacket>(), Is.EqualTo((byte)3));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<BzPacket>(), Is.EqualTo((byte)4));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<GeoPacket>(), Is.EqualTo((byte)5));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<HealPacket>(), Is.EqualTo((byte)6));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<SuicidePacket>(), Is.EqualTo((byte)7));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<ToggleAgressionPacket>(), Is.EqualTo((byte)8));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<ToggleAutoDigPacket>(), Is.EqualTo((byte)9));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<UnmappedKeyPacket>(), Is.EqualTo((byte)10));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<ClickCellPacket>(), Is.EqualTo((byte)11));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<MovePacket>(), Is.EqualTo((byte)12));
            Assert.That(ActionClientPacketCodeProvider.GetPacketCode<RotatePacket>(), Is.EqualTo((byte)13));
        });
    }

    [Test]
    public void CodeZero_IsValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ActionClientPacketCodeProvider.IsValid((byte)0), Is.True);
            Assert.That(ActionClientPacketCodeProvider.TryGetDecoder((byte)0, out var decoder), Is.True);
            Assert.That(decoder, Is.Not.Null);
        });
    }

    [Test]
    public void UnknownCode_HasNoDecoder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ActionClientPacketCodeProvider.IsValid((byte)14), Is.False);
            Assert.That(ActionClientPacketCodeProvider.TryGetDecoder((byte)14, out _), Is.False);
        });
    }
}