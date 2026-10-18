using System;

namespace MinesServer.Networking.Server.Packets.GUI;

/// <summary>
/// Поведенческие флаги v2-окна (UIWindowPacket). По умолчанию серверное
/// окно draggable + resizable + closable: ESC и крестик закрывают его.
/// Closable гасится принудительными модалками (игрок обязан нажать кнопку).
/// </summary>
[Flags]
public enum UIWindowFlags : byte
{
    None = 0,
    Draggable = 1,
    Resizable = 2,
    Closable = 4,
    AutoCenter = 8,
}
