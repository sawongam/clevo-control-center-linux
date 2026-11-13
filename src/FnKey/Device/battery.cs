using System;
using System.IO;
using System.Linq;

namespace FnKey.Device;

// Replaces kernel32!GetSystemPowerStatus with /sys/class/power_supply (read only).
public class battery
{
	public PowerLineStatus _OLD_PowerLineStatus;

	private PowerLineStatus _line = PowerLineStatus.Unknown;

	private byte _percent = 255;

	public PowerLineStatus PowerLineStatus => _line;

	public PowerLineStatus OLD_PowerLineStatus
	{
		get { return _OLD_PowerLineStatus; }
		set { _OLD_PowerLineStatus = value; }
	}

	public float BatteryLifePercent => (int)_percent;

	public battery()
	{
		UpdatePowerInfo();
	}

	public void UpdatePowerInfo()
	{
		try
		{
			const string root = "/sys/class/power_supply";
			if (!Directory.Exists(root)) return;
			_line = PowerLineStatus.Unknown;
			foreach (var d in Directory.GetDirectories(root))
			{
				string type = Read(d + "/type");
				if (type == "Mains" || type == "USB")
				{
					if (Read(d + "/online") == "1") _line = PowerLineStatus.Online;
					else if (_line != PowerLineStatus.Online) _line = PowerLineStatus.Offline;
				}
				else if (type == "Battery" && int.TryParse(Read(d + "/capacity"), out var c)) _percent = (byte)Math.Min(100, c);
			}
		}
		catch { }
	}

	static string Read(string p) { try { return File.ReadAllText(p).Trim(); } catch { return ""; } }
}
