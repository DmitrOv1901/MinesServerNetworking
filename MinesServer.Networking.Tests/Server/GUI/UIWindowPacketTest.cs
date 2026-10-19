using MinesServer.Networking.Server.Packets.GUI;
using MinesServer.Networking.Server.Packets.GUI.Components.Visual;

namespace MinesServer.Networking.Tests.Server.GUI;

internal class UIWindowPacketTest : RootServerPacketTest<UIWindowPacket>
{
    public override UIWindowPacket Packet => new(
        "market2",
        "Магазин: Кирка и акси",
        600,
        400,
        280,
        120,
        UIWindowPacket.AutoPosition,
        UIWindowPacket.AutoPosition,
        UIWindowPacket.DefaultFlags,
        new TextPacket
        {
            Text = "Привет, шахтёр!",
            AttachedProperties = [new("UI.Id", "1")],
        });
}
