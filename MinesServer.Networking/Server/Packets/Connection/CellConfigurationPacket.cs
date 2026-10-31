using MinesServer.Data;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MinesServer.Networking.Server.Packets.Connection;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public readonly record struct CellConfigurationPacket(
    CellConfigFlags Flags,
    CellRenderLayer DrawLayer,
    CellOutlineKind Outline,
    CellTextureAnchorKind TextureAnchor,
    CellSurfaceEffectKind SurfaceEffect,
    byte SurfaceEffectPalette,
    CellAnimationType ShaderAnimation,
    float FrameAnimationSpeed,
    CellDecalAtlasKind DecalAtlas,
    byte ReliefGroup,
    int MapColor) : IServerPacket<CellConfigurationPacket>
{
    public readonly int Size => Unsafe.SizeOf<CellConfigurationPacket>();

    public int Encode(Span<byte> output) => output.UnsafeWrite(this);

    public static CellConfigurationPacket Decode(ReadOnlySpan<byte> input) => input.UnsafeRead<CellConfigurationPacket>();
}
