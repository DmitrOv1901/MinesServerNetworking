namespace MinesServer.Networking.Server.Packets.GUI;

/// <summary>
/// Точечное изменение одного элемента живого v2-окна без его переоткрытия.
/// Элемент адресуется целочисленным id из attached-свойства "UI.Id" (см. v2 builder).
/// Payload трактуется по Kind: Text/Value — строка; Enabled/Visible — "0"/"1";
/// Position — "x;y"; Size — "w;h"; StyleClass — имя класса.
/// </summary>
public enum UIPatchKind : byte
{
    Text = 0,
    Value = 1,
    Enabled = 2,
    Visible = 3,
    Position = 4,
    Size = 5,
    StyleClass = 6,
}
