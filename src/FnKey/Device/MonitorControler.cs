namespace FnKey.Device;

// Windows: SC_MONITORPOWER broadcast. Linux: the desktop/logind handles lid and screen blanking -> logged stub.
public class MonitorControler
{
	public void MonitorOn() { DebugLog.WriteDbgLog2Outlog("STUB MonitorOn (desktop handles DPMS on Linux)"); }

	public void MonitorOff() { DebugLog.WriteDbgLog2Outlog("STUB MonitorOff (desktop handles DPMS on Linux)"); }
}
