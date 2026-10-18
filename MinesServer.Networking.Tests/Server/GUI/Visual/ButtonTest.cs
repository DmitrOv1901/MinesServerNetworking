using MinesServer.Networking.Server.Packets.GUI.Components.Visual;

namespace MinesServer.Networking.Tests.Server.GUI.Visual;

internal class ButtonTest : PacketTest<ButtonPacket>
{
    public override ButtonPacket Packet => new()
    {
        Label = "Продать всё",
        Tooltip = "Продаёт все ресурсы: проверка",
        IconURI = "ftp://localhost:21/sell.png",
        Hints = ButtonStyleHints.Primary | ButtonStyleHints.Tab,
        AttachedProperties = [new("UI.Id", "13")],
    };
}
