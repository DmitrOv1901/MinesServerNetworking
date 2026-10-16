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
    /// Кадр потреблён и отброшен. Поток кадров остаётся синхронным, соединение
    /// цело — решение о дисконнекте принимает хост (пороги у клиента и сервера
    /// разные).
    ///
    /// Сюда НЕ попадает битая длина кадра: после неё поток не восстановить, и
    /// буфер остаётся отравленным. Такое исключение выходит из приёмного пути
    /// наружу, и ловить его обязан хост — интерфейса для «соединение более не
    /// годно» здесь намеренно нет.
    /// </summary>
    public event Action<Exception> OnError;

    /// <summary>
    /// Счётчик для политики «N ошибок — дисконнект» на стороне хоста.
    /// </summary>
    public int DecodeErrorCount { get; }
}
