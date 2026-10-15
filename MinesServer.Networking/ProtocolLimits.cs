namespace MinesServer.Networking;

/// <summary>
/// Единые пределы протокола.
///
/// Клиент и сервер обязаны разделять одни и те же константы: расхождение видно
/// только с одной стороны — та, что проверяет лимит первой, рвёт соединение там,
/// где другая ещё считает кадр валидным.
/// </summary>
public static class ProtocolLimits
{
    /// <summary>
    /// Максимальный размер одного кадра (payload пакета, без префикса длины).
    /// Кадр длиннее — отклоняется как <c>PacketDecodeException</c>.
    /// </summary>
    public const int MaxFrameLength = 4 * 1024 * 1024; /* 4M */

    /// <summary>
    /// Стартовая ёмкость буфера кадрирования. Растёт удвоением до
    /// <see cref="MaxFrameLength"/>, сжимается обратно при оттоке данных.
    /// </summary>
    public const int FrameBufferInitialCapacity = 8192; /* 8K */

    /// <summary>
    /// Максимальный размер payload после распаковки кадра LZ4/LZMA.
    ///
    /// Сжатый кадр сам по себе ограничен <see cref="MaxFrameLength"/>, поэтому
    /// честный распакованный payload тоже обязан в него укладываться: пакет,
    /// не помещающийся в кадр без сжатия, не может быть передан и в сжатом виде.
    ///
    /// Значение заявленного размера приходит с провода как <c>int32</c> и проверяется
    /// до аллокации — см. <c>LZ4Packet.Decode</c> и <c>LzmaPacket.Decode</c>.
    /// </summary>
    public const int MaxDecompressedLength = MaxFrameLength;
}