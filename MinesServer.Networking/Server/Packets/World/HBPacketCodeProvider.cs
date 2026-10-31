namespace MinesServer.Networking.Server.Packets.World;

public class HBPacketCodeProvider : NetworkPacketCodeProvider<IHBPacket, byte, HBPacketCodeProvider>
{
    static HBPacketCodeProvider()
    {
        Register<MapRegionPacket>();
        Register<BuildingPacket>();
        Register<RobotPositionPacket>();
        Register<SFXPacket>();
        Register<VFXPacket>();
        Register<RemoveBuildingPacket>();
    }
}
