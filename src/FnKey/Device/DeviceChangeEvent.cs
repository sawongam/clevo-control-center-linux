namespace FnKey.Device;

// Windows: WM_DEVICECHANGE watcher (USB buffalo workaround). Not applicable on Linux.
public class DeviceChangeEvent
{
	public void Support_EPSON_Workaround_USB_Buffalo() { DebugLog.WriteDbgLog2Outlog("STUB Support_EPSON_Workaround_USB_Buffalo (Windows-only)"); }
}
