using Genumerics;
#if !NET7_0_OR_GREATER
using System.Reflection;
using System.Reflection.Emit;
#endif
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Linq.Expressions;
using System;
using System.Collections.Generic;

namespace MinesServer.Networking
{
    public abstract class NetworkPacketCodeProvider<TPacketType, TIdentity, TSelf>
        where TPacketType : class, INetworkPacketBase
#if NET7_0_OR_GREATER
        where TIdentity : IBinaryInteger<TIdentity>
#else
        where TIdentity : unmanaged, IComparable, IFormattable, IConvertible, IComparable<TIdentity>, IEquatable<TIdentity>
#endif
        where TSelf : NetworkPacketCodeProvider<TPacketType, TIdentity, TSelf>, new()
    {
        public delegate TPacketType PacketDecoder(ReadOnlySpan<byte> input);

        private static TIdentity MaxIdentity =
#if NET7_0_OR_GREATER
            TIdentity.Zero;
#else
            Number.Zero<TIdentity>();
#endif

        static NetworkPacketCodeProvider()
        {
            _ = new TSelf();
        }

        private static readonly Dictionary<Type, TIdentity> _packetsMapping = new();

        private static readonly List<PacketDecoder> _decoders = new();

        protected static void Register<TPacket>() where TPacket : INetworkPacket<TPacket>, TPacketType
        {
            _packetsMapping.Add(typeof(TPacket), MaxIdentity);
#if NET7_0_OR_GREATER
            _decoders.Add(x => TPacket.Decode(x));
            MaxIdentity++;
#else
            var parameter = Expression.Parameter(typeof(ReadOnlySpan<byte>), "input");
            // For NAOT/IL2CPP this automatically falls back to the interpreted form.
            var deleg = Expression.Lambda<PacketDecoder>(
                Expression.TypeAs(
                    Expression.Call(
                        typeof(TPacket),
                        "Decode",
                        null,
                        parameter),
                    typeof(TPacketType)),
                parameter).Compile();
            _decoders.Add(deleg);
            MaxIdentity = Number.Add(MaxIdentity, Number.One<TIdentity>());
#endif
        }

        public static TIdentity GetPacketCode(TPacketType packet) => UnsafeGetPacketCode(packet.GetType());

        public static TIdentity GetPacketCode<TPacket>() where TPacket : TPacketType => Cache<TPacket>.Code;

        protected static TIdentity UnsafeGetPacketCode(Type packetType) => _packetsMapping[packetType];

        public static PacketDecoder GetDecoder(TIdentity identity)
        {
#if NET7_0_OR_GREATER
            return _decoders[int.CreateTruncating(identity)];
#else
            return _decoders[Number.Convert<TIdentity, int>(identity)];
#endif
        }

        public static bool TryGetDecoder(TIdentity identity, [NotNullWhen(true)] out PacketDecoder? decoder)
        {
            decoder = default;
            if (!IsValid(identity))
                return false;
            decoder = GetDecoder(identity);
            return true;
        }

        /// <summary>
        /// Нижняя граница включающая: код 0 соответствует первому
        /// зарегистрированному пакету. <c>IsPositive</c> — строго положительное
        /// число — отвергал ноль, и на net7+ не декодировались
        /// <c>ClientHelloPacket</c> и <c>OutdatedClientPacket</c>.
        /// </summary>
        public static bool IsValid(TIdentity identity) =>
#if NET7_0_OR_GREATER
            // Ветки обязаны оставаться разными: операторы IBinaryInteger дешевле
            // Number<T>, и сервер на net9.0 выжимает максимум из сетевого стека.
            identity >= TIdentity.Zero && identity < MaxIdentity;
#else
            Number.GreaterThanOrEqual(identity, Number.Zero<TIdentity>()) && Number.LessThan(identity, MaxIdentity);
#endif

        public static class Cache<TPacket> where TPacket : TPacketType
        {
            public static TIdentity Code { get; } = UnsafeGetPacketCode(typeof(TPacket));
        }
    }
}
