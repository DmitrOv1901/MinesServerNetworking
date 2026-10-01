using MinesServer.Networking.Client.Packets.Actions;
using MinesServer.Networking.Client.Packets.Chat;
using MinesServer.Networking.Client.Packets.Connection;
using MinesServer.Networking.Client.Packets.GUI;
using MinesServer.Networking.Client.Packets.Inventory;
using MinesServer.Networking.Client.Packets.Programmator;
using MinesServer.Networking.Client.Packets.Utilities;

namespace MinesServer.Networking.Client;

public class RootClientPacketCodeProvider : NetworkPacketCodeProvider<IRootClientPacket, byte, RootClientPacketCodeProvider>
{
    static RootClientPacketCodeProvider()
    {
        Register<ClientHelloPacket>();
        Register<QueryChatHistoryPacket>();
        Register<ElementClickPacket>();
        Register<SendChatMessagePacket>();
        Register<SendLocalChatMessagePacket>();
        Register<ChangeChatColorPacket>();
        Register<DeleteProgramClickPacket>();
        Register<PauseProgramPacket>();
        Register<ProgramStepInPacket>();
        Register<ProgramStepOutPacket>();
        Register<ProgramStepOverPacket>();
        Register<QueryProgramMemoryPacket>();
        Register<RenameProgramClickPacket>();
        Register<SaveProgramPacket>();
        Register<StartProgramPacket>();
        Register<StopProgramPacket>();
        Register<PongPacket>();
        Register<OpenClanClickPacket>();
        Register<OpenHelpClickPacket>();
        Register<OpenProgramListClickPacket>();
        Register<OpenSettingsClickPacket>();
        Register<DeselectItemPacket>();
        Register<SelectItemPacket>();
        Register<UseItemPacket>();
        Register<ActionClientPacket>();
        Register<RuntimeAssetRequestPacket>();
        Register<DropBoxClickPacket>();
        Register<BldsClickPacket>();
    }
}
