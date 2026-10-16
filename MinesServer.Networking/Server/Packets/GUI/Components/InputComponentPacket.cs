using MinesServer.Utils;
using System;

namespace MinesServer.Networking.Server.Packets.GUI.Components;

public abstract record class InputComponentPacket<TValue> : GUIComponentPacket, IInputComponentPacket<TValue> where TValue : notnull
{
    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// Обязательно по <see cref="INamedComponentPacket"/>: по проводу имя идёт
    /// единственным экземпляром, без него компонент не идентифицируем.
    ///
    /// Инициализатор <c>null!</c> остаётся намеренно — иначе компонент без имени
    /// нельзя собрать, чтобы поймать ошибку в <see cref="Encode"/>.
    /// </summary>
    public string Name { get; init; } = null!;
    public abstract TValue DefaultValue { get; init; }

    /// <summary>
    /// Проверки имени здесь нет намеренно: свойство вызывается на каждой
    /// отправке через <c>ServerPacket.Size</c>, и проверка внутри сделала бы его
    /// неинлайнимым. Диагностику даёт <see cref="EnsureNamed"/> — кодирование всё
    /// равно происходит один раз на пакет.
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
    /// Без проверки компонент без имени падал бы с
    /// <see cref="NullReferenceException"/> на разыменовании
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
