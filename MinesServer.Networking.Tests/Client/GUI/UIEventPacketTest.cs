using MinesServer.Networking.Client.Packets.GUI;

namespace MinesServer.Networking.Tests.Client.GUI;

internal class UIEventPacketTest : RootClientPacketTest<UIEventPacket>
{
    public override UIEventPacket Packet => new(
        "TestWindow",
        42,
        UIEventType.ValueChanged,
        [
            new("value", "0.5"),
            new("anotherKey", "anotherValue with unicode: проверка")
        ]);
}
