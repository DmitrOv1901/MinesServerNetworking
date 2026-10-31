using MinesServer.Networking.Server.Packets;
using MinesServer.Networking.Server.Packets.Movement;

namespace MinesServer.Networking.Tests.Server;

internal class ServerPacketTest : PacketTest<ServerPacket>
{
    public override ServerPacket Packet => new(new TeleportPacket(123, 456));
}
