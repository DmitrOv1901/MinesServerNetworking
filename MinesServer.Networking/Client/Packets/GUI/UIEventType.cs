namespace MinesServer.Networking.Client.Packets.GUI;

/// <summary>
/// Тип события элемента v2-окна. Идентификаторы стабильны: это wire-формат.
/// Click — клик кнопки/вкладки; ValueChanged — слайдер/dropdown/toggle/textfield
/// (значение в Context под ключом "value"); Submitted — Enter в текстовом поле;
/// WindowClosed — ESC/крестик (шлётся только если у окна был Closable);
/// WindowMoved/WindowResized — игрок перетащил/растянул окно (координаты в Context).
/// </summary>
public enum UIEventType : byte
{
    Click = 0,
    ValueChanged = 1,
    Submitted = 2,
    WindowClosed = 3,
    WindowMoved = 4,
    WindowResized = 5,
}
