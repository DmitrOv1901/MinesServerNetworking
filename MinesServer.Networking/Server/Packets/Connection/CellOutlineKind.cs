namespace MinesServer.Networking.Server.Packets.Connection;

public enum CellOutlineKind : byte
{
    Pliant = 0,
    Wavy = 1,
    Rigid = 2,
    Round = 3,
    Wall = 4,
    Corner = 5,
    Door = 6,
}
