using MinesServer.Networking.Server.Packets.Information;

namespace MinesServer.Networking.Tests.Server.Information;

internal class ClientConfigTest : RootServerPacketTest<ClientConfigPacket>
{
    public override ClientConfigPacket Packet => new(
        [new("w", "mu"), new("a", "ml"), new("s", "md"), new("d", "mr"), new("W", "ru"), new("A", "rl"), new("S", "rd"), new("D", "rr")],
        ["tex1.png", "tex2.jpg"]);
}
