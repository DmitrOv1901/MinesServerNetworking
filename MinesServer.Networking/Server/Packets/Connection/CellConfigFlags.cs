using System;

namespace MinesServer.Networking.Server.Packets.Connection;

[Flags]
public enum CellConfigFlags : byte
{
    None = 0,
    Passable = 1 << 0,
    Breakable = 1 << 1,
    DropsShadow = 1 << 2,
    ReceivesShadow = 1 << 3,
    Blending = 1 << 4,
    Glowing = 1 << 5,
    Emission = 1 << 6,
}
