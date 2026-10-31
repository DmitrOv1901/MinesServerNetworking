using MinesServer.Utils;
using System;
using System.Linq;

namespace MinesServer.Networking.Server.Packets.Connection;

public readonly record struct WorldInitPacket(string CodeName, string DisplayName, ushort Width, ushort Height, CellConfigurationPacket[] Cells, byte[][] TileGroups, WorldTextureDefinitionPacket[]? TextureManifest = null) : IRootServerPacket<WorldInitPacket>
{
    public ushort PacketCode => RootServerPacketCodeProvider.Cache<WorldInitPacket>.Code;

    public int Size =>
        sizeof(byte) + // CodeName.Length
        CodeName.Length + // CodeName
        sizeof(byte) + // DisplayName.Length
        DisplayName.Length * 2 + // DisplayName
        sizeof(ushort) + // Width
        sizeof(ushort) + // Height
        sizeof(byte) + // TileGroups.Length
        sizeof(byte) * TileGroups.Length + // TileGroups[].Length
        TileGroups.Sum(x => x.Length) + // TileGroups[]
        sizeof(ushort) + // TextureManifest.Length
        (TextureManifest?.Sum(x => x.Size) ?? 0) + // TextureManifest
        sizeof(ushort) + // Cells.Length
        Cells.Sum(x => x.Size); // Cells

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        WorldTextureDefinitionPacket[] textures = TextureManifest ?? Array.Empty<WorldTextureDefinitionPacket>();
        if (textures.Length > ushort.MaxValue || Cells.Length > ushort.MaxValue)
        {
            throw new InvalidOperationException("WorldInitPacket contains too many texture definitions or cell configurations.");
        }

        writer.WriteU1PrefixedASCII(CodeName);
        writer.WriteU1PrefixedUtf16(DisplayName);
        writer.Write(Width);
        writer.Write(Height);
        writer.Write((byte)TileGroups.Length);
        foreach (var group in TileGroups)
            writer.WriteU1PrefixedArray(group);
        writer.Write((ushort)textures.Length);
        foreach (WorldTextureDefinitionPacket texture in textures)
            writer.Advance(texture.Encode(output[writer.Position..]));
        writer.Write((ushort)Cells.Length);
        foreach (CellConfigurationPacket cell in Cells)
            writer.Advance(cell.Encode(output[writer.Position..]));
        return writer.Position;
    }

    public static WorldInitPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var codeName = reader.ReadU1PrefixedASCII(out _);
        var name = reader.ReadU1PrefixedUtf16(out _);
        var width = reader.ReadU2();
        var height = reader.ReadU2();
        var tileGroups = new byte[reader.ReadU1()][];
        for (int i = 0; i < tileGroups.Length; i++)
            tileGroups[i] = reader.ReadU1PrefixedArray<byte>(out _);
        var textures = new WorldTextureDefinitionPacket[reader.ReadU2()];
        for (int i = 0; i < textures.Length; i++)
        {
            textures[i] = WorldTextureDefinitionPacket.Decode(reader.Remaining, out int bytesRead);
            reader.Advance(bytesRead);
        }
        var cellCount = reader.ReadU2();
        var renderConfig = new CellConfigurationPacket[cellCount];
        for (int i = 0; i < renderConfig.Length; i++)
        {
            renderConfig[i] = CellConfigurationPacket.Decode(reader.Remaining, out int bytesRead);
            reader.Advance(bytesRead);
        }
        return new WorldInitPacket(
            codeName,
            name,
            width,
            height,
            renderConfig,
            tileGroups,
            textures);
    }

    public bool Equals(WorldInitPacket other) =>
        CodeName == other.CodeName &&
        DisplayName == other.DisplayName &&
        Width == other.Width &&
        Height == other.Height &&
        Cells.SequenceEqual(other.Cells) &&
        (TextureManifest ?? Array.Empty<WorldTextureDefinitionPacket>()).SequenceEqual(other.TextureManifest ?? Array.Empty<WorldTextureDefinitionPacket>()) &&
        TileGroups.Length == other.TileGroups.Length &&
        TileGroups.SelectMany(x => x).SequenceEqual(other.TileGroups.SelectMany(x => x));
}
