namespace MinesServer.Data;

public enum CellAnimationType : byte
{
    // Значения сериализуются в CellConfigurationPacket и дублируются в шейдере террейна.
    None = 0, // Без анимации; например, у Empty.
    Blinking = 1, // Мигание; используется GrayAcid.
    Shimmer = 2, // Мерцание; используется PurpleAcid.
    Rainbow = 3, // Перелив радужных цветов; в текущей конфигурации клеток не используется.
    Legacy2Test = 4,
    Legacy3Test = 5,
    Legacy4Test = 6,
    Legacy5Test = 7,
    Legacy6Test = 8,
    Legacy7Test = 9,
    Legacy8Test = 10,
    Molten = 11, // Анимация расплавленной поверхности; используется Lava.
    Faceted = 12, // Переливчатая гранёная поверхность; используется Green.
    Prismatic = 13, // Призматический перелив; например, у XGreen.
}
