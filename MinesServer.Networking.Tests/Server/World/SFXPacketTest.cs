using MinesServer.Networking.Server.Packets.World;

namespace MinesServer.Networking.Tests.Server.World;

internal class SFXPacketTest : PacketTest<SFXPacket>
{
    public override SFXPacket Packet => new(
        Data.SFX.Boom,
        303,
        50,
        60,
        [
            new("test", "ASDasdasd")
        ]);
}
