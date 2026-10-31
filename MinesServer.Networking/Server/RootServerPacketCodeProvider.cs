using System;
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

namespace MinesServer.Networking.Server;

public class RootServerPacketCodeProvider : NetworkPacketCodeProvider<IRootServerPacket, ushort, RootServerPacketCodeProvider>
{
    static RootServerPacketCodeProvider()
    {
        Register<OutdatedClientPacket>();
        Register<TeleportPacket>();
        Register<OpenWindowPacket>();
        Register<CloseWindowPacket>();
        Register<LzmaPacket>();
        Register<DisconnectPacket>();
        Register<ReconnectPacket>();
        Register<BasketPacket>();
        Register<CurrencyPacket>();
        Register<GeologyPacket>();
        Register<HealthPacket>();
        Register<HideClanPacket>();
        Register<LevelPacket>();
        Register<OnlinePacket>();
        Register<PlayerInfoPacket>();
        Register<ShowClanPacket>();
        Register<DeselectItemPacket>();
        Register<InventoryPacket>();
        Register<SelectItemPacket>();
        Register<MissionArrowPacket>();
        Register<MissionProgressPacket>();
        Register<ReservedRootPacket21>();
        Register<AutoMineStatePacket>();
        Register<OpenURLPacket>();
        Register<PingPacket>();
        Register<MaxDepthPacket>();
        Register<AddStatusLinePacket>();
        Register<ClearStatusLinePacket>();
        Register<ClearStatusPacket>();
        Register<SkillProgressPacket>();
        Register<DailyBonusStatePacket>();
        Register<MissionInitPacket>();
        Register<LocalChatMessagePacket>();
        Register<ModalWindowPacket>();
        Register<WorldInitPacket>();
        Register<ClientConfigPacket>();
        Register<BreakpointHitPacket>();
        Register<RuntimeAssetPacket>();
        Register<HBPacket>();
        Register<RobotInfoPacket>();
        Register<LZ4Packet>();
        Register<UpdateProgramPacket>();
        Register<ProgramMemoryPacket>();
        Register<ProgramStatePacket>();
        Register<OpenProgrammatorPacket>();
        Register<AuthTokenPacket>();
        Register<ChatMutePacket>();
        Register<ChatMessageListPacket>();
        Register<ChatListPacket>();
        Register<MovementSpeedPacket>();
    }
}

internal readonly record struct ReservedRootPacket21(byte Payload) : IRootServerPacket<ReservedRootPacket21>
{
    public ushort PacketCode => RootServerPacketCodeProvider.Cache<ReservedRootPacket21>.Code;

    public int Size => sizeof(byte);

    public int Encode(Span<byte> output)
    {
        output[0] = Payload;
        return sizeof(byte);
    }

    public static ReservedRootPacket21 Decode(ReadOnlySpan<byte> input) => new(input[0]);
}
