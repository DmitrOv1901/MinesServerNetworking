using MinesServer.Networking.Server.Packets.GUI;
using MinesServer.Networking.Server.Packets.GUI.Components;
using MinesServer.Networking.Server.Packets.GUI.Components.Containers;
using MinesServer.Networking.Server.Packets.GUI.Components.Input;
using MinesServer.Networking.Server.Packets.GUI.Components.Visual;

namespace MinesServer.Networking.Tests.CodeTables;

/// <summary>
/// Снапшот таблицы кодов компонентов окон (0–12).
///
/// Адресует отрисовку всех серверных окон целиком, и перестановка Register не даёт
/// исключения — окно просто рисуется не теми элементами.
/// </summary>
internal class GUIComponentPacketCodeTableTests
{
    [Test]
    public void GUIComponentCodes_MatchRegistrationOrder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<TextPacket>(), Is.EqualTo((byte)0));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<ImagePacket>(), Is.EqualTo((byte)1));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<ScrollViewerPacket>(), Is.EqualTo((byte)2));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<DockPanelPacket>(), Is.EqualTo((byte)3));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<GridPacket>(), Is.EqualTo((byte)4));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<IntDropdownPacket>(), Is.EqualTo((byte)5));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<StringDropdownPacket>(), Is.EqualTo((byte)6));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<SelectablePacket>(), Is.EqualTo((byte)7));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<TextBoxPacket>(), Is.EqualTo((byte)8));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<SliderPacket>(), Is.EqualTo((byte)9));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<PanelPacket>(), Is.EqualTo((byte)10));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<LinePacket>(), Is.EqualTo((byte)11));
            Assert.That(GUIComponentPacketCodeProvider.GetPacketCode<CanvasPacket>(), Is.EqualTo((byte)12));
        });
    }

    [Test]
    public void CodeZero_IsValid()
    {
        Assert.Multiple(() =>
        {
            Assert.That(GUIComponentPacketCodeProvider.IsValid((byte)0), Is.True);
            Assert.That(GUIComponentPacketCodeProvider.TryGetDecoder((byte)0, out var decoder), Is.True);
            Assert.That(decoder, Is.Not.Null);
        });
    }

    [Test]
    public void UnknownCode_HasNoDecoder()
    {
        Assert.Multiple(() =>
        {
            Assert.That(GUIComponentPacketCodeProvider.IsValid((byte)13), Is.False);
            Assert.That(GUIComponentPacketCodeProvider.TryGetDecoder((byte)13, out _), Is.False);
        });
    }
}