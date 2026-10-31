using MinesServer.Data;
using MinesServer.Networking.Shared.Packets;
using MinesServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MinesServer.Networking.Server.Packets.World;

public readonly record struct SFXPacket(SFX EffectType, ushort TargetBotId, ushort X, ushort Y, IReadOnlyList<StringPairPacket> Parameters) : IHBPacket<SFXPacket>
{
    public byte PacketCode => HBPacketCodeProvider.Cache<SFXPacket>.Code;

    public int Size =>
        sizeof(SFX) + // EffectType
        sizeof(ushort) + // TargetBotId
        sizeof(ushort) + // X
        sizeof(ushort) + // Y
        sizeof(byte) + // Parameters.Length
        Parameters.Sum(x => x.Size); // Parameters

    public int Encode(Span<byte> output)
    {
        var writer = output.Writer();
        writer.Write(EffectType);
        writer.Write(TargetBotId);
        writer.Write(X);
        writer.Write(Y);
        writer.Write((byte)Parameters.Count);
        foreach (var param in Parameters)
            writer.Write(param);
        return writer.Position;
    }

    public static SFXPacket Decode(ReadOnlySpan<byte> input)
    {
        var reader = input.Reader();
        var effect = reader.Read<SFX>();
        var botid = reader.ReadU2();
        var x = reader.ReadU2();
        var y = reader.ReadU2();
        var parameters = new StringPairPacket[reader.ReadU1()];
        for (int i = 0; i < parameters.Length; i++)
        {
            var item = StringPairPacket.Decode(reader.Remaining);
            reader.Advance(item.Size);
            parameters[i] = item;
        }
        return new(effect, botid, x, y, parameters);
    }

    public bool Equals(SFXPacket other) =>
        EffectType == other.EffectType &&
        TargetBotId == other.TargetBotId &&
        X == other.X &&
        Y == other.Y &&
        Parameters.SequenceEqual(other.Parameters);
}
