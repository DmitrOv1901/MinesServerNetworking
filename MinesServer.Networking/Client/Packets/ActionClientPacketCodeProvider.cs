using MinesServer.Networking.Client.Packets.Actions;
using MinesServer.Networking.Client.Packets.Movement;

namespace MinesServer.Networking.Client.Packets;

public class ActionClientPacketCodeProvider : NetworkPacketCodeProvider<IActionClientPacket, byte, ActionClientPacketCodeProvider>
{
    static ActionClientPacketCodeProvider()
    {
        Register<BuildCyanPacket>();
        Register<BuildGrayPacket>();
        Register<BuildGreenPacket>();
        Register<BuildWhitePacket>();
        Register<BzPacket>();
        Register<GeoPacket>();
        Register<HealPacket>();
        Register<SuicidePacket>();
        Register<ToggleAutoDigPacket>();
        Register<UnmappedKeyPacket>();
        Register<ClickCellPacket>();
        Register<MovePacket>();
        Register<RotatePacket>();
    }
}
