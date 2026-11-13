using System;
using System.IO;
using System.Linq;

namespace FnKey.Device;

// Replaces powrprof (PowerRead/WriteAC/DCValueIndex of the video-brightness setting) with /sys/class/backlight.
// Read: percentage of max_brightness. Write: STUB (needs root/udev rule or the desktop's own API; on Linux
// the desktop environment already owns brightness) - the call is logged only.
public class Brightness
{
	public static string BacklightDir()
	{
		try
		{
			const string root = "/sys/class/backlight";
			if (!Directory.Exists(root)) return null;
			// firmware > platform > raw, as the kernel documents for userspace
			foreach (var kind in new[] { "firmware", "platform", "raw" })
				foreach (var d in Directory.GetDirectories(root))
					if (Read(d + "/type") == kind) return d;
			return Directory.GetDirectories(root).FirstOrDefault();
		}
		catch { return null; }
	}

	static string Read(string p) { try { return File.ReadAllText(p).Trim(); } catch { return ""; } }

	public int Read_Brightness()
	{
		var d = BacklightDir();
		if (d == null) return 0;
		if (int.TryParse(Read(d + "/brightness"), out var b) && int.TryParse(Read(d + "/max_brightness"), out var m) && m > 0)
			return (int)Math.Round(b * 100.0 / m);
		return 0;
	}

	public void Write_Brightness(int value)
	{
		DebugLog.WriteDbgLog2Outlog("STUB Write_Brightness(" + value + ") - backlight is owned by the desktop environment on Linux");
	}
}
