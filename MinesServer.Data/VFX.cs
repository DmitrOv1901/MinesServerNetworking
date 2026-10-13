namespace MinesServer.Data;

public enum VFX : byte
{
    Bz,
    Death,
    BombTick,
    Boom,
    Dizz,
    EMI,
    TpIn,
    TpOut,
    NoHp,
    NoHpSmall,
    Smoke,
    Volcano,
    C190,
    Crystal,
    Pull, // Animates a sprite from the player to a point in the world
    Push, // Animates a sprite from a point in the world to the player
    Heal,
    Hurt,
    GunShot,
    PlaceDeny
}
