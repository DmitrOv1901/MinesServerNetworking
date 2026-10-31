namespace MinesServer.Networking.Server.Packets.Connection;

public enum CellRenderLayer : byte
{
    Foreground = 0,
    Background = 1,
    Underlay = 2,
    WorldEdge = 3,
}
