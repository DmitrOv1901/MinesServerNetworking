using System;

namespace MinesServer.Networking.Server.Packets.GUI.Components.Visual;

/// <summary>
/// Семантические стилевые подсказки v2-кнопки. Клиент маппит их в USS-классы
/// (packet-button--primary и т.д.) поверх базового .packet-button. Флаги можно
/// комбинировать (Primary | Tab). Значения стабильны: это wire-формат.
/// </summary>
[Flags]
public enum ButtonStyleHints : byte
{
    None = 0,
    Primary = 1,
    Danger = 2,
    Ghost = 4,
    Success = 8,
    Tab = 16,
}
