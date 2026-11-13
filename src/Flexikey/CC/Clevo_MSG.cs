using System;
using System.IO;
using RippleCC.Platform;

namespace CC;

/// <summary>
/// Windows: WM_COPYDATA messages to the "HkeyTray" window. No such window on Linux: each message is
/// recorded in ConfigDir/flexikey-hktray.log (cmd, data bytes) so a later phase can route it.
/// </summary>
public class Clevo_MSG
{
	public static readonly System.Collections.Generic.List<string> Sent = new();
	public void SendMsg(int CMD, int DATA) { Log($"SendMsg CMD={CMD} DATA={DATA}"); }
	public void SendMsg_WMI(int CMD, int DATA) { Log($"SendMsg_WMI CMD={CMD} DATA={DATA}"); }
	public void GetMsg_WMI(int CMD) { Log($"GetMsg_WMI CMD={CMD}"); }
	public void SendArrayToHkeyTray() { Log("SendArrayToHkeyTray"); }
	public void SendToHKTray(int cmd) { Send(cmd, 0, 0, 0, 0); }
	public void SendToHKTray(int cmd, byte arr1) { Send(cmd, arr1, 0, 0, 0); }
	public void SendToHKTray(int cmd, byte arr1, byte arr2) { Send(cmd, arr1, arr2, 0, 0); }
	public void SendToHKTray(int cmd, byte arr1, byte arr2, byte arr3) { Send(cmd, arr1, arr2, arr3, 0); }
	public void SendToHKTray(int cmd, byte arr1, byte arr2, byte arr3, byte arr4) { Send(cmd, arr1, arr2, arr3, arr4); }
	void Send(int cmd, byte a, byte b, byte c, byte d)
		=> Log($"SendToHKTray dwData={(byte)((cmd >> 8) & 0xFF)} data=[{(byte)(cmd & 0xFF)},{a},{b},{c},{d}]");
	static void Log(string s)
	{
		Sent.Add(s);
		try { File.AppendAllText(Path.Combine(CcPaths.ConfigDir, "flexikey-hktray.log"), s + "\n"); } catch { }
	}
}
