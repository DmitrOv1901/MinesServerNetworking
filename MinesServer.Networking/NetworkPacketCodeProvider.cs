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
        /// Код 0 валиден: первому зарегистрированному пакету соответствует именно ноль.
        /// Ветка net7+ раньше использовала <see cref="IBinaryInteger{T}.IsPositive(T)"/>,
        /// то есть строго положительное число, и отвергала код 0 — из-за чего не
        /// декодировались <c>ClientHelloPacket</c> (рукопожатие) и
        /// <c>OutdatedClientPacket</c> (отказ по версии клиента) на net7+.
        /// Ветка net481 использовала <c>&gt;= Zero</c>, то есть расхождение жило
        /// внутри одного файла. Обе ветки приведены к <c>&gt;= Zero</c>.
        /// </summary>
        public static bool IsValid(TIdentity identity) =>
#if NET7_0_OR_GREATER
            // Современная ветка: операторы IBinaryInteger дешевле Number<T> из
            // Genumerics. Серверу нужен максимум производительности сетевого
            // стека, клиент на net481 считает через Number.
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
