namespace MinesServer.Data;

public enum BuildingType : ushort
{
    None = ' ',
    Teleport = 'T',
    Resp = 'R',
    Gun = 'G',
    Market = 'M',
    Up = 'U',
    Storage = 'L', // Почему L... Load?
    Craft = 'F', // Почему F... Factory?
    Vulkan = 'Q', // Почему Q...
    Spot = 'O', // Почему O...
    Levi = 'W', // Почему W... WarMachine?
    Jobs = 'J',
    Target = 'Y', // Почему Y...
    FLAGBLYAT = 'D', // Почему D...
    Port = 'P',
    Bomb = 'B',
    Sign = 'I',
    Observatory = 'P',
    /// <summary>
    /// Не совсем понимаю для чего это используется у мячина. Но типа обработчик в клиенте есть так что пусть будет и тут.
    /// </summary>
    Invalid = 'C',
    BombShop = 'b',
    Beacon = 'Z', // Почему Z...
    Clans = 'c',
    Science = 'N', // Почему N... NauchniyCentr?
    AutoDigger = 'A',
    ChunkUpdater = 'V',
    GeoStorage = 'S' // Почему S... Хранение геокристаллов, Storage занят 'L'
}
