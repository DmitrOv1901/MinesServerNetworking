using MinesServer.Utils;
using System;

namespace MinesServer.Networking.Server.Packets.Connection;

/// <summary>
/// Текстурный ресурс мира, передаваемый через RuntimeAssetPacket.
/// Имя файла также используется в настройках клеток и декалей.
/// </summary>
public readonly record struct WorldTextureDefinitionPacket(string Filename, string ETag)
{
    private string SafeFilename => Filename ?? string.Empty;
    private string SafeETag => ETag ?? string.Empty;

    public int Size =>
        sizeof(byte) * 2 + SafeFilename.Length + SafeETag.Length;

    public int Encode(Span<byte> output)
    {
        string filename = SafeFilename;
        string etag = SafeETag;
        if (filename.Length > byte.MaxValue || etag.Length > byte.MaxValue)
        {
            throw new InvalidOperationException("Texture filename and ETag must each fit in a one-byte length prefix.");
        }

        var writer = output.Writer();
        writer.WriteU1PrefixedASCII(filename);
        writer.WriteU1PrefixedASCII(etag);
        return writer.Position;
    }

    public static WorldTextureDefinitionPacket Decode(ReadOnlySpan<byte> input, out int bytesRead)
    {
        var reader = input.Reader();
        var definition = new WorldTextureDefinitionPacket(
            reader.ReadU1PrefixedASCII(out _),
            reader.ReadU1PrefixedASCII(out _));
        bytesRead = reader.Position;
        return definition;
    }
}
