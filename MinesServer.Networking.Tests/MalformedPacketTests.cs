using System.Buffers.Binary;
using MinesServer.Networking.Exceptions;
using MinesServer.Networking.Server.Packets;
using MinesServer.Networking.Server.Packets.GUI.Components.Input;
using MinesServer.Networking.Server.Packets.GUI;
using MinesServer.Networking.Server.Packets.Information;
using MinesServer.Networking.Client;
using MinesServer.Networking.Server;

namespace MinesServer.Networking.Tests;

/// <summary>
/// Негативные тесты на malformed-ввод.
///
/// До них протокол проверялся только в happy path: тесты пустых пакетов проходили
/// тривиально и одинаково проходили для настоящей реализации и для заглушки.
/// </summary>
internal class MalformedPacketTests
{
    [Test]
    public void ServerPacket_TruncatedPayload_IsRejected()
    {
        var frame = new ServerPacket(new MaxDepthPacket(500));
        var full = new byte[frame.Size];
        frame.Encode(full);

        // Обрезанный кадр: длина префикса обещает больше, чем реально пришло.
        var truncated = full.AsSpan(0, full.Length - 3).ToArray();
        BinaryPrimitives.WriteInt32LittleEndian(truncated, full.Length);

        Assert.Throws<PacketDecodeException>(() => ServerPacket.Decode(truncated));
    }

    [Test]
    public void ServerPacket_DeclaredLengthDoesNotMatchContent_IsRejected()
    {
        var frame = new ServerPacket(new MaxDepthPacket(500));
        var buffer = new byte[frame.Size];
        frame.Encode(buffer);

        BinaryPrimitives.WriteInt32LittleEndian(buffer, frame.Size + 1);
        Assert.Throws<PacketDecodeException>(() => ServerPacket.Decode(buffer));
    }

    [Test]
    public void ServerPacket_UnknownRootCode_IsRejected()
    {
        var frame = new byte[sizeof(int) + sizeof(ushort)];
        BinaryPrimitives.WriteInt32LittleEndian(frame, frame.Length);
        BinaryPrimitives.WriteUInt16LittleEndian(frame.AsSpan(sizeof(int)), 6000);

        Assert.Throws<PacketDecodeException>(() => ServerPacket.Decode(frame));
    }

    [Test]
    public void ClientPacket_UnknownRootCode_HasNoDecoder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(RootClientPacketCodeProvider.IsValid((byte)200), Is.False);
            Assert.That(RootClientPacketCodeProvider.TryGetDecoder((byte)200, out _), Is.False);
        });
    }

    /// <summary>
    /// Регрессия <c>Name = null!</c>: компонент без имени падал в <c>Size</c> и
    /// <c>Encode</c> с <see cref="NullReferenceException"/> на разыменовании
    /// <c>string.Length</c>. Вызывающая сторона не получала ни пакета, ни внятной
    /// причины — падало в произвольном месте.
    /// </summary>
    [Test]
    public void UnnamedInputComponent_ThrowsDescriptiveError()
    {
        var unnamed = new TextBoxPacket { DefaultValue = "" };

        // Size не падает: он на горячем пути и обязан вычислиться хоть как-то.
        // Диагностика приходит из Encode — это единственное место, где имя
        // действительно требуется.
        int size = unnamed.Size;
        Assert.That(size, Is.EqualTo(unnamed.Size), "Size must be computable without a name");

        var fromEncode = Assert.Throws<InvalidOperationException>(
            () => unnamed.Encode(new byte[size]));
        Assert.That(fromEncode!.Message, Does.Contain("Name"));
    }

    /// <summary>
    /// Имя обязательно, но <see cref="TextBoxPacket.Size"/> не должно это
    /// проверять: свойство вызывается на каждой отправке через
    /// <c>ServerPacket.Size</c>, и проверка внутри превратила бы его в метод с
    /// веткой выброса, то есть почти наверняка в неинлайнимый.
    /// </summary>
    [Test]
    public void UnnamedInputComponent_SizeIsCheapAndStable()
    {
        // Оба экземпляра безымянные: проверка в том, что Size вычислим и не падает, а
        // разница с именованным — это ровно префикс плюс длина имени.
        var unnamed = new TextBoxPacket { DefaultValue = "" };
        var named = new TextBoxPacket { Name = "abc", DefaultValue = "" };

        Assert.Multiple(() =>
        {
            Assert.That(unnamed.Size, Is.GreaterThan(0));
            // Префикс длины (1 байт) входит в Size при любом Name — он указывает, сколько
            // символов идёт следом, и при пустом имени тоже нужен.
            Assert.That(named.Size - unnamed.Size, Is.EqualTo("abc".Length),
                "a name must add exactly its own length; the prefix is counted either way");
        });
    }

    [Test]
    public void NamedInputComponent_EncodesNormally()
    {
        var named = new TextBoxPacket { Name = "nickname", DefaultValue = "abc" };

        var buffer = new byte[named.Size];
        int written = named.Encode(buffer);

        Assert.That(written, Is.EqualTo(named.Size));
        var decoded = (TextBoxPacket)GUIComponentPacketCodeProvider.GetDecoder((byte)8)(buffer.AsSpan(0, written));
        Assert.That(decoded.Name, Is.EqualTo("nickname"));
        Assert.That(decoded.DefaultValue, Is.EqualTo("abc"));
    }
}