using MinesServer.Data;
using MinesServer.Networking.Server.Packets.Connection;

namespace MinesServer.Networking.Tests.Server.Connection;

internal class WorldInitTest : RootServerPacketTest<WorldInitPacket>
{
    public override WorldInitPacket Packet => new("test", "Тестовый мир", 10, 10,
        [
            new(
                Flags: CellConfigFlags.None,
                DrawLayer: CellRenderLayer.Background,
                Outline: CellOutlineKind.Pliant,
                TextureAnchor: CellTextureAnchorKind.Cell,
                SurfaceEffect: CellSurfaceEffectKind.Plain,
                SurfaceEffectPalette: 0,
                ShaderAnimation: CellAnimationType.None,
                FrameAnimationSpeed: 6f,
                DecalAtlas: CellDecalAtlasKind.None,
                ReliefGroup: 0,
                MapColor: 0),
            new(
                Flags: CellConfigFlags.Passable | CellConfigFlags.DropsShadow,
                DrawLayer: CellRenderLayer.Foreground,
                Outline: CellOutlineKind.Rigid,
                TextureAnchor: CellTextureAnchorKind.World,
                SurfaceEffect: CellSurfaceEffectKind.Prismatic,
                SurfaceEffectPalette: 1,
                ShaderAnimation: CellAnimationType.Prismatic,
                FrameAnimationSpeed: 0f,
                DecalAtlas: CellDecalAtlasKind.Rock,
                ReliefGroup: 1,
                MapColor: unchecked((int)0xFF00FF00)),
        ],
        [
            [1, 2, 3],
            [4, 5, 6]
        ],
        [new WorldTextureDefinitionPacket("Cells/Rock.png", "0123456789abcdef0123456789abcdef")]);
}
