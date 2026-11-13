namespace FanSpeedSetting;

/// <summary>
/// Original used user32 window messages (HkeyTray, ClevoMsg_ControlCenter). Not present on Linux; no-ops.
/// FanSpeedSetting never calls these (kept for source parity).
/// </summary>
public class Clevo_MSG
{
    public Clevo_MSG() { }
    public void SendMsg(int CMD, int DATA) { }
    public void SendMsg_WMI(int CMD, int DATA) { }
    public void GetMsg_WMI(int CMD) { }
    public void SendArrayToHkeyTray() { }
    public void SendToHKTray(int cmd) { }
    public void SendToHKTray(int cmd, byte arr1) { }
    public void SendToHKTray(int cmd, byte arr1, byte arr2) { }
    public void SendToHKTray(int cmd, byte arr1, byte arr2, byte arr3) { }
    public void SendToHKTray(int cmd, byte arr1, byte arr2, byte arr3, byte arr4) { }
}
