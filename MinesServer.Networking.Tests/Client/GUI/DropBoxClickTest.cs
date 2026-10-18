using MinesServer.Networking.Client.Packets.GUI;

namespace MinesServer.Networking.Tests.Client.GUI;

internal class DropBoxClickTest : RootClientPacketTest<DropBoxClickPacket>
{
    public override DropBoxClickPacket Packet => new();
}
