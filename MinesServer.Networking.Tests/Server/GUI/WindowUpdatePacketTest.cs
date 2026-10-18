using MinesServer.Networking.Server.Packets.GUI;

namespace MinesServer.Networking.Tests.Server.GUI;

internal class WindowUpdatePacketTest : RootServerPacketTest<WindowUpdatePacket>
{
    public override WindowUpdatePacket Packet => new(
        "market2",
        7,
        UIPatchKind.Text,
        "Обновлённый текст: проверка");
}
