using MinesServer.Networking.Exceptions;
using System;
using System.Runtime.CompilerServices;
using System.Text;

namespace MinesServer.Networking.Connection;

public class PacketBuffer
{
    /// <summary>
    /// Предел кадра по умолчанию. Единый для клиента и сервера — расхождение
    /// значений означает, что одна сторона разрывает соединение там, где другая
    /// ещё считает кадр валидным.
    /// </summary>
    public const int DefaultMaxLength = ProtocolLimits.MaxFrameLength;

    /// <summary>
    /// Стартовая ёмкость по умолчанию.
    /// </summary>
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
    /// Кладёт <paramref name="length"/> байт из <paramref name="data"/>, начиная с
    /// <paramref name="offset"/>.
    ///
    /// offset обязателен: NetCoreServer отдаёт принятые данные в реюзнутом массиве,
    /// где полезные данные не обязаны лежать с нулевой позиции. Копирование всегда
    /// с нуля подмешивает мусор и рассинхронизирует поток кадров.
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

        // Длина читается по текущей позиции чтения, а не по нулю буфера. Чтение
        // из _buffer[0] работало только потому, что CompactBuffer() безусловно
        // обнуляет _readPos после каждого успешного Take: инвариант «на входе в
        // TryTake _readPos == 0» держался на соглашении о порядке вызовов, а не
        // на коде. Любой путь, оставляющий _readPos > 0 при частичном чтении,
        // молча рассинхронизировал бы поток кадров.
        int packetLength = Unsafe.ReadUnaligned<int>(ref _buffer[_readPos]);

        if (packetLength < 4)
            throw new PacketDecodeException($"Packet length is too small: {packetLength}; head={HexHead(32)}");

        if (packetLength > _maxLength)
            throw new PacketDecodeException($"Packet length is too long: {packetLength} (limit {_maxLength}); head={HexHead(32)}");

        if (available < packetLength) return false;

        packet = new byte[packetLength];
        Unsafe.CopyBlock(ref packet[0], ref _buffer[_readPos], (uint)packetLength);
        _readPos += packetLength;

        CompactBuffer();
        return true;
    }

    /// <summary>
    /// Hex-дамп первых байт непрочитанного хвоста буфера.
    ///
    /// Единственный способ понять, что прислал противоположный конец, когда
    /// длину кадра читать уже нельзя: на проводе видно только мусор из
    /// реюзнутого массива, а число в префиксе говорит лишь «что-то сломалось».
    /// Диагностика из TryTake попадает в лог хоста вместе с исключением.
    /// </summary>
    private string HexHead(int maxBytes)
    {
        int available = _writePos - _readPos;
        int count = maxBytes < available ? maxBytes : available;
        if (count <= 0) return string.Empty;

        var sb = new StringBuilder(count * 3);
        for (int i = 0; i < count; i++)
        {
            if (i > 0) sb.Append(' ');
            sb.Append(_buffer[_readPos + i].ToString("X2"));
        }
        return sb.ToString();
    }

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
    