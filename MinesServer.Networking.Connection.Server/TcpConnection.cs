using MinesServer.Networking.Client.Packets;
using MinesServer.Networking.Server.Packets;
using MinesServer.Networking.Shared;
using NetCoreServer;

namespace MinesServer.Networking.Connection.Server;

public class TcpConnection : TcpSession, IClientConnection
{
    private readonly PacketBuffer _buffer = new();

    bool _isDisconnecting = false;

    private int _decodeErrorCount;

    public TcpConnection(TcpServer server) : base(server)
    {
    }

    public int DecodeErrorCount => _decodeErrorCount;

    protected override void OnReceived(byte[] buffer, long offset, long size)
    {
        base.OnReceived(buffer, offset, size);
        _buffer.Put(buffer, (int)offset, (int)size);
        while (_buffer.TryTake(out var completeBuffer))
        {
            ClientPacket packet;
            try
            {
                packet = ClientPacket.Decode(completeBuffer);
            }
            catch (Exception ex)
            {
                // continue, а не return: кадр уже потреблён, и выход отбросил бы
                // всё, что уже лежит в буфере.
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

    protected override void OnConnecting()
    {
        base.OnConnecting();
        _onConnecting?.Invoke();
    }

    protected override void OnDisconnected()
    {
        base.OnDisconnected();
        _isDisconnecting = false;
        _onDisconnected?.Invoke();
    }

    public ConnectionStatus ConnectionStatus
    {
        get {
            if(IsConnected)
                return ConnectionStatus.Connected;
            return _isDisconnecting ? ConnectionStatus.Disconnecting : ConnectionStatus.Disconnected;
        }
    }


    private event Action<ClientPacket> _onReceived;

    event Action<ClientPacket> IConnection<ClientPacket, ServerPacket>.OnReceived
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

    event Action<Exception> IConnection<ClientPacket, ServerPacket>.OnError
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

    int IConnection<ClientPacket, ServerPacket>.DecodeErrorCount => _decodeErrorCount;

    private event Action _onConnected;

    event Action IConnection<ClientPacket, ServerPacket>.OnConnected
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

    event Action IConnection<ClientPacket, ServerPacket>.OnConnecting
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

    private event Action _onDisconnected;

    event Action IConnection<ClientPacket, ServerPacket>.OnDisconnected
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

    private event Action _onDisconnecting;
    event Action IConnection<ClientPacket, ServerPacket>.OnDisconnecting
    {
        add
        {
            _onDisconnecting += value;
        }

        remove
        {
            _onDisconnecting -= value;
        }
    }

    public void Connect()
    {
        // Connect() в IConnection нужен только клиенту: сессию создаёт TcpServer.
        throw new NotSupportedException($"{nameof(TcpConnection)} is the server side of the transport and cannot initiate a connection. Use {nameof(TcpServer)} instead.");
    }

    public void SendAsync(ServerPacket packet)
    {
        Span<byte> span = stackalloc byte[packet.Size];
        packet.Encode(span);
        SendAsync(span);
    }

    protected override void OnDisconnecting()
    {
        base.OnDisconnecting();
        _isDisconnecting = true;
        _onDisconnecting?.Invoke();
    }

    void IConnection<ClientPacket, ServerPacket>.Disconnect()
    {
        Disconnect();
    }
}
