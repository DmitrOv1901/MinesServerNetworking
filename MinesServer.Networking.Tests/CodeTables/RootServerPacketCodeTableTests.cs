using MinesServer.Networking.Server;
using MinesServer.Networking.Server.Packets;
using MinesServer.Networking.Server.Packets.Chat;
using MinesServer.Networking.Server.Packets.Compression;
using MinesServer.Networking.Server.Packets.Connection;
using MinesServer.Networking.Server.Packets.GUI;
using MinesServer.Networking.Server.Packets.Information;
using MinesServer.Networking.Server.Packets.Information.StatusPanel;
using MinesServer.Networking.Server.Packets.Inventory;
using MinesServer.Networking.Server.Packets.Mission;
using MinesServer.Networking.Server.Packets.Movement;
using MinesServer.Networking.Server.Packets.Programmator;
using MinesServer.Networking.Server.Packets.Utilities;
using MinesServer.Networking.Server.Packets.World;

namespace MinesServer.Networking.Tests.CodeTables;

/// <summary>
/// Снапшот таблицы кодов серверных пакетов (0–50).
///
/// Код — позиция вызова Register, нигде не записанная явно, поэтому перестановка
/// молча перенумеровывает протокол, и падение приходит не в тестах, а в игре.
///
/// Таблицы живут здесь, а не в репозитории сервера: пока они были там, протокол
/// можно было сломать здесь, и падал бы сервер.
/// </summary>
internal class RootServerPacketCodeTableTests
{
    [Test]
    public void ServerPacketCodes_MatchRegistrationOrder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<OutdatedClientPacket>(), Is.EqualTo((ushort)0));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<TeleportPacket>(), Is.EqualTo((ushort)1));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<OpenWindowPacket>(), Is.EqualTo((ushort)2));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<CloseWindowPacket>(), Is.EqualTo((ushort)3));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<LzmaPacket>(), Is.EqualTo((ushort)4));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<DisconnectPacket>(), Is.EqualTo((ushort)5));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ReconnectPacket>(), Is.EqualTo((ushort)6));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<BasketPacket>(), Is.EqualTo((ushort)7));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<CurrencyPacket>(), Is.EqualTo((ushort)8));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<GeologyPacket>(), Is.EqualTo((ushort)9));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<HealthPacket>(), Is.EqualTo((ushort)10));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<HideClanPacket>(), Is.EqualTo((ushort)11));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<LevelPacket>(), Is.EqualTo((ushort)12));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<OnlinePacket>(), Is.EqualTo((ushort)13));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<PlayerInfoPacket>(), Is.EqualTo((ushort)14));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ShowClanPacket>(), Is.EqualTo((ushort)15));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<DeselectItemPacket>(), Is.EqualTo((ushort)16));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<InventoryPacket>(), Is.EqualTo((ushort)17));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<SelectItemPacket>(), Is.EqualTo((ushort)18));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<MissionArrowPacket>(), Is.EqualTo((ushort)19));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<MissionProgressPacket>(), Is.EqualTo((ushort)20));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<AggressionStatePacket>(), Is.EqualTo((ushort)21));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<AutoMineStatePacket>(), Is.EqualTo((ushort)22));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<OpenURLPacket>(), Is.EqualTo((ushort)23));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<PingPacket>(), Is.EqualTo((ushort)24));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<MaxDepthPacket>(), Is.EqualTo((ushort)25));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<AddStatusLinePacket>(), Is.EqualTo((ushort)26));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ClearStatusLinePacket>(), Is.EqualTo((ushort)27));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ClearStatusPacket>(), Is.EqualTo((ushort)28));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<SkillProgressPacket>(), Is.EqualTo((ushort)29));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<DailyBonusStatePacket>(), Is.EqualTo((ushort)30));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<MissionInitPacket>(), Is.EqualTo((ushort)31));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<LocalChatMessagePacket>(), Is.EqualTo((ushort)32));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ModalWindowPacket>(), Is.EqualTo((ushort)33));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<WorldInitPacket>(), Is.EqualTo((ushort)34));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ClientConfigPacket>(), Is.EqualTo((ushort)35));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<BreakpointHitPacket>(), Is.EqualTo((ushort)36));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<RuntimeAssetPacket>(), Is.EqualTo((ushort)37));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<HBPacket>(), Is.EqualTo((ushort)38));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<RobotInfoPacket>(), Is.EqualTo((ushort)39));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<LZ4Packet>(), Is.EqualTo((ushort)40));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<UpdateProgramPacket>(), Is.EqualTo((ushort)41));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ProgramMemoryPacket>(), Is.EqualTo((ushort)42));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ProgramStatePacket>(), Is.EqualTo((ushort)43));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<OpenProgrammatorPacket>(), Is.EqualTo((ushort)44));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<AuthTokenPacket>(), Is.EqualTo((ushort)45));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ChatMutePacket>(), Is.EqualTo((ushort)46));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ChatMessageListPacket>(), Is.EqualTo((ushort)47));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<ChatListPacket>(), Is.EqualTo((ushort)48));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<MovementSpeedPacket>(), Is.EqualTo((ushort)49));
            Assert.That(RootServerPacketCodeProvider.GetPacketCode<WindowUpdatePacket>(), Is.EqualTo((ushort)50));
        });
    }

    /// <summary>
    /// Код 0 валиден: <c>IsPositive</c> отвергал ноль, и отказ по версии клиента
    /// (<c>OutdatedClientPacket</c>) нельзя было декодировать.
    /// </summary>
    [Test]
    public void CodeZero_IsValidAndDecodes()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RootServerPacketCodeProvider.IsValid((ushort)0), Is.True);
            Assert.That(RootServerPacketCodeProvider.TryGetDecoder((ushort)0, out var decoder), Is.True);
            Assert.That(decoder, Is.Not.Null);
        });
    }

    [Test]
    public void UnknownCode_HasNoDecoder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RootServerPacketCodeProvider.IsValid((ushort)51), Is.False);
            Assert.That(RootServerPacketCodeProvider.TryGetDecoder((ushort)51, out _), Is.False);
        });
    }
}