using System;
using System.IO;
using System.Linq;

namespace FnKey;

// Replaces wlandll.dll / HKLM RadioManagement\SystemRadioState / Windows radio API (SetAirplaneMode).
// Read side: /sys/class/rfkill (no privileges). Write side: STUB (writing /dev/rfkill needs the 'rfkill' ACL or
// NetworkManager's `nmcli radio all off`; on Linux the desktop already handles the airplane key natively).
public static class Rfkill
{
	const string Root = "/sys/class/rfkill";

	public record Radio(string Name, string Type, bool SoftBlocked, bool HardBlocked);

	public static Radio[] List()
	{
		try
		{
			if (!Directory.Exists(Root)) return Array.Empty<Radio>();
			return Directory.GetDirectories(Root).Select(d => new Radio(
				Read(d + "/name"), Read(d + "/type"), Read(d + "/soft") == "1", Read(d + "/hard") == "1")).ToArray();
		}
		catch { return Array.Empty<Radio>(); }
	}

	static string Read(string p) { try { return File.ReadAllText(p).Trim(); } catch { return ""; } }

	/// <summary>Windows SystemRadioState == 1: every wlan/bluetooth radio is blocked.</summary>
	public static bool AirplaneOn()
	{
		var r = List().Where(x => x.Type == "wlan" || x.Type == "bluetooth" || x.Type == "wwan").ToArray();
		return r.Length > 0 && r.All(x => x.SoftBlocked || x.HardBlocked);
	}

	/// <summary>Number of WLAN adapters (replaces wifi.GetWLANCounter()).</summary>
	public static int WlanCount() => List().Count(x => x.Type == "wlan");

	public static void SetAirplaneMode(int status)
	{
		DebugLog.WriteDbgLog2Outlog("STUB SetAirplaneMode(" + status + ") - not performed; use `rfkill " + (status == 1 ? "block all" : "unblock all") + "` / the desktop applet");
	}
}
