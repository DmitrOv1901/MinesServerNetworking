using MinesServer.Networking.Server.Packets.GUI.Components;
using MinesServer.Networking.Server.Packets.GUI.Components.Containers;
using MinesServer.Networking.Server.Packets.GUI.Components.Input;
using MinesServer.Networking.Server.Packets.GUI.Components.Visual;

namespace MinesServer.Networking.Server.Packets.GUI;

public class GUIComponentPacketCodeProvider : NetworkPacketCodeProvider<IGUIComponentPacket, byte, GUIComponentPacketCodeProvider>
{
    static GUIComponentPacketCodeProvider()
    {
        Register<TextPacket>();
        Register<ImagePacket>();
        Register<ScrollViewerPacket>();
        Register<DockPanelPacket>();
        Register<GridPacket>();
        Register<IntDropdownPacket>();
        Register<StringDropdownPacket>();
        Register<SelectablePacket>();
        Register<TextBoxPacket>();
        Register<SliderPacket>();
        Register<PanelPacket>();
        Register<LinePacket>();
        Register<CanvasPacket>();
    }
}
