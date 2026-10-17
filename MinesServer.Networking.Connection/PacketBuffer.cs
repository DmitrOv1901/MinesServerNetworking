using MinesServer.Networking.Exceptions;
using System;
using System.Runtime.CompilerServices;

namespace MinesServer.Networking.Connection;

public class PacketBuffer
{
    /// <summary>
    /// Единый для клиента и сервера: расхождение означает, что одна сторона рвёт
    /// соединение там, где другая ещё считает кадр валидным.
    /// </summary>
    public const int DefaultMaxLength = ProtocolLimits.MaxFrameLength;

    public const int DefaultInitialCapacity = ProtocolLimits.FrameBufferInitialCapacity;

    private byte[] _buffer;
    private int _readPos;
    private int _writePos;
    private readonly int _initialCapacity;
    private readonly int _maxLength;
    private const int ShrinkFactor = 4;

    public PacketBuffer(int initialCapacity = DefaultInitialCapacity, int maxLength = DefaultMaxLength)
    {
        _initialCapacity = initialCapacity;
        _maxLength = maxLength;
        _buffer = new byte[initialCapacity];
        _readPos = 0;
        _writePos = 0;
    }

    public void Put(byte[] data, int length) => Put(data, 0, length);

    /// <summary>
    /// offset обязателен: NetCoreServer отдаёт принятые данные в реюзнутом
    /// массиве, где полезные данные не обязаны лежать с нуля.
    /// </summary>
    public void Put(byte[] data, int offset, int length)
    {
        if (length == 0) return;
        if (offset < 0 || (uint)offset > (uint)data.Length || (uint)length > (uint)(data.Length - offset))
            throw new ArgumentOutOfRangeException(nameof(offset), $"Range [{offset}, {offset + (long)length}) is out of data bounds (length {data.Length}).");

        EnsureCapacity(length);
        Unsafe.CopyBlock(ref _buffer[_writePos], ref data[offset], (uint)length);
        _writePos += length;
    }

    public bool TryTake(out byte[] packet)
    {
        packet = null;
        int available = _writePos - _readPos;

        if (available < 4) return false;

        // По позиции чтения, а не по нулю буфера: начало кадра не обязано
        // совпадать с началом буфера.
        int packetLength = Unsafe.ReadUnaligned<int>(ref _buffer[_readPos]);

        if (packetLength < 4)
            throw new PacketDecodeException($"Packet length is too small: {packetLength}", FrameHead());

        if (packetLength > _maxLength)
            throw new PacketDecodeException($"Packet length is too long: {packetLength} (limit {_maxLength})", FrameHead());

        if (available < packetLength) return false;

        packet = new byte[packetLength];
        Unsafe.CopyBlock(ref packet[0], ref _buffer[_readPos], (uint)packetLength);
        _readPos += packetLength;

        CompactBuffer();
        return true;
    }

    /// <summary>
    /// Непрочитанный хвост: когда длину кадра прочитать уже нельзя, на проводе
    /// видно только мусор из реюзнутого массива, и без дампа сообщение
    /// бесполезно. Сами байты едут в исключении — хост логирует
    /// <see cref="PacketDecodeException.FrameHead"/>.
    ///
    /// Ровно <c>_writePos - _readPos</c>, а не остаток ёмкости: за хвостом лежит
    /// содержимое прошлых кадров, и в диагностику оно попадать не должно.
    /// </summary>
    private ReadOnlySpan<byte> FrameHead() => _buffer.AsSpan(_readPos, _writePos - _readPos);

    private void EnsureCapacity(int appendLength)
    {
        int totalRequired = _writePos + appendLength;
        if (totalRequired <= _buffer.Length) return;

        int currentDataLength = _writePos - _readPos;
        int newCapacity = Math.Max(_buffer.Length * 2, currentDataLength + appendLength);

        byte[] newBuffer = new byte[newCapacity];
        if (currentDataLength > 0)
            Unsafe.CopyBlock(ref newBuffer[0], ref _buffer[_readPos], (uint)currentDataLength);

        _buffer = newBuffer;
        _readPos = 0;
        _writePos = currentDataLength;
    }

    private void CompactBuffer()
    {
        int dataLength = _writePos - _readPos;

        if (_readPos == 0)
        {
            TryShrinkBuffer();
            return;
        }

        if (dataLength > 0)
            Unsafe.CopyBlock(ref _buffer[0], ref _buffer[_readPos], (uint)dataLength);

        _readPos = 0;
        _writePos = dataLength;

        TryShrinkBuffer();
    }

    private void TryShrinkBuffer()
    {
        int currentCapacity = _buffer.Length;
        int desiredCapacity = Math.Max(
            _initialCapacity,
            Math.Max(_writePos * 2, _initialCapacity)
        );

        if (currentCapacity > desiredCapacity * ShrinkFactor)
        {
            byte[] newBuffer = new byte[desiredCapacity];
            Unsafe.CopyBlock(ref newBuffer[0], ref _buffer[0], (uint)_writePos);
            _buffer = newBuffer;
        }
    }
}
    