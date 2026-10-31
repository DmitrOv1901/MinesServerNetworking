using MinesServer.Data;
using MinesServer.Networking.Server.Packets.Connection;

namespace MinesServer.Networking.Tests.Server.Connection;

internal class CellConfigurationTest : PacketTest<CellConfigurationPacket>
{
    public override CellConfigurationPacket Packet => new(
        Flags: CellConfigFlags.Passable | CellConfigFlags.Breakable,
        DrawLayer: CellRenderLayer.Foreground,
        Outline: CellOutlineKind.Wavy,
        TextureAnchor: CellTextureAnchorKind.Cell,
        SurfaceEffect: CellSurfaceEffectKind.Faceted,
        SurfaceEffectPalette: 2,
        ShaderAnimation: CellAnimationType.Shimmer,
        FrameAnimationSpeed: 6f,
        DecalAtlas: CellDecalAtlasKind.Rock,
        ReliefGroup: 2,
        MapColor: unchecked((int)0xFF112233));

    [Test]
    public void WireLayoutMatchesProtocolContract()
    {
        var packet = Packet;
        Assert.That(packet.Size, Is.EqualTo(17));
        var bytes = new byte[packet.Size];
        Assert.That(packet.Encode(bytes), Is.EqualTo(packet.Size));
        Assert.That(CellConfigurationPacket.Decode(bytes), Is.EqualTo(packet));
    }
}
