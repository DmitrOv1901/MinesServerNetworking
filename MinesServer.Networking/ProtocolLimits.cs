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
    /// Максимальный размер кадра. Считается вместе с 4-байтовым префиксом длины —
    /// ровно то, что <c>TryTake</c> сравнивает с этим значением.
    /// </summary>
    public const int MaxFrameLength = 4 * 1024 * 1024; /* 4M */

    /// 8 КБ — типичный кадр лезет без перевыделения; логистика роста в
    /// <c>PacketBuffer</c>.
    /// </summary>
    public const int FrameBufferInitialCapacity = 8192; /* 8K */

    /// <summary>
    /// Равен <see cref="MaxFrameLength"/>: пакет, не влезающий в кадр без сжатия,
    /// не передать и в сжатом виде. Проверяется до аллокации, потому что размер
    /// приходит с провода как <c>int32</c> (см. <c>LZ4Packet.Decode</c>,
    /// <c>LzmaPacket.Decode</c>).
    /// </summary>
    public const int MaxDecompressedLength = MaxFrameLength;
}