using MinesServer.Utils;
using System;

namespace MinesServer.Networking.Server.Packets.GUI.Components;

public abstract record class InputComponentPacket<TValue> : GUIComponentPacket, IInputComponentPacket<TValue> where TValue : notnull
{
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Имя элемента. Обязательно: <see cref="INamedComponentPacket"/> требует имени,
    /// а по проводу оно идёт единственным экземпляром на компонент, поэтому
    /// без него компонент не идентифицируем.
    ///
    /// Инициализатор <c>null!</c> остаётся намеренно — иначе компонент без
    /// имени нельзя было бы даже собрать и поймать ошибку в Encode с внятным
    /// текстом. Проверка стоит в <see cref="Encode"/> и в <see cref="Size"/>.
    /// </summary>
    public string Name { get; init; } = null!;
    public abstract TValue DefaultValue { get; init; }

    /// <summary>
    /// Размер компонента. Вызывается на каждой отправке (через
    /// <c>ServerPacket.Size</c>), поэтому остаётся коротким инлайнимым
    /// выражением: проверки имени здесь нет — она в <see cref="Encode"/>.
    ///
    /// <c>Name?.Length ?? 0</c> вместо <c>Name.Length</c>: без имени
    /// <c>Size</c> обязан посчитать хоть что-то, а не упасть с
    /// <c>NullReferenceException</c>. Кодирование всё равно откажет в
    /// <see cref="EnsureNamed"/>.
    /// </summary>
    public override int Size => base.Size
        + sizeof(bool) // IsEnabled
        + sizeof(byte) // Name.Length
        + (Name?.Length ?? 0); // Name

    public override int Encode(Span<byte> output)
    {
        EnsureNamed();
        var writer = output.Writer();
        writer.Advance(base.Encode(output));
        writer.WriteU1PrefixedASCII(Name);
        writer.Write(IsEnabled);
        return writer.Position;
    }

    /// <summary>
    /// Без этой проверки компонент без имени падал в <c>Size</c> или
    /// <c>Encode</c> с <see cref="NullReferenceException"/> на разыменовании
    /// <see cref="string.Length"/>, и вызывающая сторона не получала ни пакета,
    /// ни внятной причины.
    /// </summary>
    private void EnsureNamed()
    {
        if (Name is null)
            throw new InvalidOperationException($"{GetType().Name}: Name must be set before encoding an input component.");
    }

    protected InputComponentPacket(ref MemoryReader reader) : base(ref reader)
    {
        Name = reader.ReadU1PrefixedASCII(out _);
        IsEnabled = reader.Read<bool>();
    }

    protected InputComponentPacket() : base() { }

    public virtual bool Equals(InputComponentPacket<TValue>? other) =>
        base.Equals(other) &&
        other.Name == Name &&
        other.IsEnabled == IsEnabled &&
        other.DefaultValue.Equals(DefaultValue);
}
