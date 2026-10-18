using MinesServer.Networking.Client;
using MinesServer.Networking.Client.Packets;
using MinesServer.Networking.Client.Packets.Actions;
using MinesServer.Networking.Client.Packets.Chat;
using MinesServer.Networking.Client.Packets.Connection;
using MinesServer.Networking.Client.Packets.GUI;
using MinesServer.Networking.Client.Packets.Inventory;
using MinesServer.Networking.Client.Packets.Programmator;
using MinesServer.Networking.Client.Packets.Utilities;

namespace MinesServer.Networking.Tests.CodeTables;

/// <summary>
/// Снапшот таблицы кодов клиентских пакетов (0–28).
///
/// Таблица уже расходилась с серверной: клиент добавил
/// <c>OpenProgramListClickPacket</c>, сервер о нём не знал, коды после 18
/// сдвинулись, и клиентские пакеты сервер декодировал как чужие.
/// </summary>
internal class RootClientPacketCodeTableTests
{
    [Test]
    public void ClientPacketCodes_MatchRegistrationOrder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<ClientHelloPacket>(), Is.EqualTo((byte)0));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<QueryChatHistoryPacket>(), Is.EqualTo((byte)1));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<ElementClickPacket>(), Is.EqualTo((byte)2));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<SendChatMessagePacket>(), Is.EqualTo((byte)3));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<SendLocalChatMessagePacket>(), Is.EqualTo((byte)4));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<ChangeChatColorPacket>(), Is.EqualTo((byte)5));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<DeleteProgramClickPacket>(), Is.EqualTo((byte)6));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<PauseProgramPacket>(), Is.EqualTo((byte)7));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<ProgramStepInPacket>(), Is.EqualTo((byte)8));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<ProgramStepOutPacket>(), Is.EqualTo((byte)9));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<ProgramStepOverPacket>(), Is.EqualTo((byte)10));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<QueryProgramMemoryPacket>(), Is.EqualTo((byte)11));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<RenameProgramClickPacket>(), Is.EqualTo((byte)12));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<SaveProgramPacket>(), Is.EqualTo((byte)13));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<StartProgramPacket>(), Is.EqualTo((byte)14));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<StopProgramPacket>(), Is.EqualTo((byte)15));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<PongPacket>(), Is.EqualTo((byte)16));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<OpenClanClickPacket>(), Is.EqualTo((byte)17));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<OpenHelpClickPacket>(), Is.EqualTo((byte)18));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<OpenProgramListClickPacket>(), Is.EqualTo((byte)19));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<OpenSettingsClickPacket>(), Is.EqualTo((byte)20));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<DeselectItemPacket>(), Is.EqualTo((byte)21));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<SelectItemPacket>(), Is.EqualTo((byte)22));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<UseItemPacket>(), Is.EqualTo((byte)23));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<ActionClientPacket>(), Is.EqualTo((byte)24));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<RuntimeAssetRequestPacket>(), Is.EqualTo((byte)25));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<DropBoxClickPacket>(), Is.EqualTo((byte)26));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<OpenBuildingsClickPacket>(), Is.EqualTo((byte)27));
            Assert.That(RootClientPacketCodeProvider.GetPacketCode<UIEventPacket>(), Is.EqualTo((byte)28));
        });
    }

    /// <summary>
    /// Код 0 валиден: <c>IsPositive</c> отвергал ноль, и рукопожатие
    /// (<c>ClientHelloPacket</c>) не декодировалось ни на одной из веток сборки.
    /// </summary>
    [Test]
    public void CodeZero_IsValidAndDecodes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RootClientPacketCodeProvider.IsValid((byte)0), Is.True);
            Assert.That(RootClientPacketCodeProvider.TryGetDecoder((byte)0, out var decoder), Is.True);
            Assert.That(decoder, Is.Not.Null);
        });
    }

    [Test]
    public void UnknownCode_HasNoDecoder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RootClientPacketCodeProvider.IsValid((byte)29), Is.False);
            Assert.That(RootClientPacketCodeProvider.TryGetDecoder((byte)29, out _), Is.False);
        });
    }
}