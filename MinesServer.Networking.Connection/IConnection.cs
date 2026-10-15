using MinesServer.Networking.Shared;
using System;

namespace MinesServer.Networking.Connection;

public interface IConnection<TReceivePacket, TSendPacket> : IDisposable
    where TReceivePacket : INetworkPacket<TReceivePacket>
    where TSendPacket : INetworkPacket<TSendPacket>
{
    public void Connect();
    public void Disconnect();
    public void SendAsync(TSendPacket packet);
    public ConnectionStatus ConnectionStatus { get; }
    public event Action<TReceivePacket> OnReceived;
    public event Action OnConnected;
    public event Action OnDisconnected;
    public event Action OnDisconnecting;
    public event Action OnConnecting;

    /// <summary>
    /// Кадр не удалось декодировать.
    ///
    /// Событие существует потому, что защита от битых кадров — часть контракта
    /// транспорта, а не прикладного кода. Без него каждый потребитель
    /// <see cref="IConnection{TReceivePacket, TSendPacket}"/> обязан заново
    /// реализовать try/catch вокруг декодера, а забытый обработчик роняет
    /// receive-loop исключением.
    ///
    /// Событие означает «кадр потреблён и отброшен», а не «соединение мертво»:
    /// поток кадров после него остаётся синхронным. Решение о дисконнекте
    /// принимает хост — счётчик нарушений и порог живут в прикладном коде,
    /// потому что у клиента и сервера политика разная.
    /// </summary>
    public event Action<Exception> OnError;

    /// <summary>
    /// Сколько кадров не удалось декодировать с момента установления соединения.
    /// Хост использует это для политики «N ошибок — дисконнект».
    /// </summary>
    public int DecodeErrorCount { get; }
}
