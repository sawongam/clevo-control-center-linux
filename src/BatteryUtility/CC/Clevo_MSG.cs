namespace CC;
/// <summary>Windows window-message IPC to the CC tray (FindWindow "HkeyTray"/SendMessage WM_COPYDATA).
/// The original class is instantiated nowhere in this app (Global.clevo_msg is never assigned); kept as a no-op.</summary>
public class Clevo_MSG
{
	public void SendMsg(int CMD, int DATA) { }
	public void SendMsg_WMI(int CMD, int DATA) { }
	public void GetMsg_WMI(int CMD) { }
	public void SendArrayToHkeyTray() { }
}
