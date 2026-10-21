using MinesServer.Networking.Server.Packets.GUI;
using MinesServer.Networking.Server.Packets.GUI.Components.Visual;

namespace MinesServer.Networking.Tests.Server.GUI;

internal class WindowUpdatePacketTest : RootServerPacketTest<WindowUpdatePacket>
{
    public override WindowUpdatePacket Packet => new(
        "market2",
        7,
        new TextPacket
        {
            Text = "Обновлённый текст: проверка",
            AttachedProperties = [new("UI.Id", "7")],
        });
}
