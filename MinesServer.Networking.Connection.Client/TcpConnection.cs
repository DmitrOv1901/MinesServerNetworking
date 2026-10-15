using MinesServer.Networking.Client.Packets;
using MinesServer.Networking.Server.Packets;
using MinesServer.Networking.Shared;
using NetCoreServer;
using System;
using System.Net;

namespace MinesServer.Networking.Connection.Client;

public class TcpConnection : TcpClient, IServerConnection
{
    private readonly PacketBuffer _buffer = new();

    private bool _isDisconnecting = false;

    private int _decodeErrorCount;

    public TcpConnection(IPAddress address, int port) : base(address, port)
    {
    }

    public int DecodeErrorCount => _decodeErrorCount;

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        base.OnReceived(buffer, offset, size);
        _buffer.Put(buffer, (int)offset, (int)size);
        while (_buffer.TryTake(out var completeBuffer))
        {
            ServerPacket packet;
            try
            {
                packet = ServerPacket.Decode(completeBuffer);
            }
            catch (Exception ex)
            {
                // Кадр уже потреблён буфером, поэтому цикл продолжается: поток
                // кадров остаётся синхронным. Выход из метода здесь отбросил бы
                // все кадры, уже лежащие в буфере.
                _decodeErrorCount++;
                _onError?.Invoke(ex);
                continue;
            }
            _onReceived?.Invoke(packet);
        }
    }

    protected override void OnConnected()
    {
        base.OnConnected();
        _onConnected?.Invoke();
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        _isDisconnecting = false;
        _onDisconnected?.Invoke();
    }

    public ConnectionStatus ConnectionStatus
    {
        get
        {
            if (IsConnecting)
                return ConnectionStatus.Connecting;
            if (IsConnected)
                return ConnectionStatus.Connected;
            return _isDisconnecting ? ConnectionStatus.Disconnecting : ConnectionStatus.Disconnected;
        }
    }


    private event Action<ServerPacket> _onReceived;

    event Action<ServerPacket> IConnection<ServerPacket, ClientPacket>.OnReceived
    {
        add
        {
            _onReceived += value;
        }

        remove
        {
            _onReceived -= value;
        }
    }

    private event Action<Exception> _onError;

    event Action<Exception> IConnection<ServerPacket, ClientPacket>.OnError
    {
        add
        {
            _onError += value;
        }

        remove
        {
            _onError -= value;
        }
    }

    int IConnection<ServerPacket, ClientPacket>.DecodeErrorCount => _decodeErrorCount;

    private event Action _onConnected;

    event Action IConnection<ServerPacket, ClientPacket>.OnConnected
    {
        add
        {
            _onConnected += value;
        }

        remove
        {
            _onConnected -= value;
        }
    }

    private event Action _onConnecting;

    event Action IConnection<ServerPacket, ClientPacket>.OnConnecting
    {
        add
        {
            _onConnecting += value;
        }

        remove
        {
            _onConnecting -= value;
        }
    }

    public event Action OnDisconnecting;

    private event Action _onDisconnected;

    event Action IConnection<ServerPacket, ClientPacket>.OnDisconnected
    {
        add
        {
            _onDisconnected += value;
        }

        remove
        {
            _onDisconnected -= value;
        }
    }

    

    public void SendAsync(ClientPacket packet)
    {
        var buffer = new byte[packet.Size];
        Span<byte> span = buffer.AsSpan();
        packet.Encode(span);
        SendAsync(buffer);
    }

    void IConnection<ServerPacket, ClientPacket>.Disconnect()
    {
        _isDisconnecting = true;
        OnDisconnecting?.Invoke();
        Disconnect();
    }

    void IConnection<ServerPacket, ClientPacket>.Connect()
    {
        _onConnecting?.Invoke();
        Connect();
    }
}
