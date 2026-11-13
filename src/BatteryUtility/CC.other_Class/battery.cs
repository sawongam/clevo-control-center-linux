using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
namespace CC.other_Class;

/// <summary>
/// Replaces kernel32 GetSystemPowerStatus (via /sys/class/power_supply) and powerlife.dll / powercfg
/// (the "BatteryUtility" power plan that disables all sleep/idle timers during a refresh).
/// Power plan replacement: while a refresh is running a `systemd-inhibit` child holds an idle+sleep
/// inhibitor lock (no system power settings are modified); Delete_ releases it.
/// </summary>
internal class battery
{
	private struct SystemPowerStatus
	{
		public PowerLineStatus PowerLineStatus;
		public BatteryChargeStatus BatteryChargeStatus;
		public byte BatteryLifePercent;
		public byte Reserved;
		public int BatteryLifeRemaining;
		public int BatteryFullLifeTime;
	}
	private SystemPowerStatus _powerStatus;
	private static Process _inhibitor;

	public PowerLineStatus PowerLineStatus => _powerStatus.PowerLineStatus;
	public BatteryChargeStatus BatteryChargeStatus => _powerStatus.BatteryChargeStatus;
	public float BatteryLifePercent => (int)_powerStatus.BatteryLifePercent;
	public int BatteryLifeRemaining => _powerStatus.BatteryLifeRemaining;
	public int BatteryFullLifeTime => _powerStatus.BatteryFullLifeTime;

	const string PS = "/sys/class/power_supply";

	// ---- raw sysfs access (shared with the package-7 adapter) ----
	public static string BatNode()
	{
		try
		{
			string first = null;
			foreach (var d in Directory.GetDirectories(PS))
			{
				if (Rd(Path.Combine(d, "type")) != "Battery") continue;
				if (Rd(Path.Combine(d, "present")) == "0") continue;
				first ??= d;
				if (Path.GetFileName(d).StartsWith("BAT")) return d;
			}
			return first;
		}
		catch { return null; }
	}
	public static string Rd(string f)
	{
		try { return File.ReadAllText(f).Trim(); } catch { return null; }
	}
	public static long RdL(string f, long def = -1) => long.TryParse(Rd(f), out var v) ? v : def;
	public static bool AcOnlineRaw()
	{
		try
		{
			foreach (var d in Directory.GetDirectories(PS))
			{
				var t = Rd(Path.Combine(d, "type"));
				if (t == "Mains" || t == "USB" || t == "USB_PD" || t == "USB_C")
					if (Rd(Path.Combine(d, "online")) == "1") return true;
			}
		}
		catch { }
		return false;
	}

	public battery() { UpdatePowerInfo(); }

	public void UpdatePowerInfo()
	{
		var s = new SystemPowerStatus { PowerLineStatus = PowerLineStatus.Offline, BatteryLifeRemaining = -1, BatteryFullLifeTime = -1 };
		bool forceNone = Environment.GetEnvironmentVariable("RIPPLE_BAT_NONE") == "1";
		var node = forceNone ? null : BatNode();
		if (forceNone)
		{
			s.PowerLineStatus = PowerLineStatus.Online;
			s.BatteryChargeStatus = BatteryChargeStatus.NoSystemBattery;
			s.BatteryLifePercent = 255;
		}
		else if (node == null)
		{
			// no battery node: plausible fixed values (AC online, full)
			s.PowerLineStatus = PowerLineStatus.Online;
			s.BatteryLifePercent = 100;
			s.BatteryChargeStatus = BatteryChargeStatus.High;
		}
		else
		{
			s.PowerLineStatus = AcOnlineRaw() ? PowerLineStatus.Online : PowerLineStatus.Offline;
			long cap = RdL(Path.Combine(node, "capacity"), -1);
			if (cap < 0)
			{
				long now = RdL(Path.Combine(node, "charge_now"), RdL(Path.Combine(node, "energy_now"), -1));
				long full = RdL(Path.Combine(node, "charge_full"), RdL(Path.Combine(node, "energy_full"), -1));
				cap = (now >= 0 && full > 0) ? now * 100 / full : 100;
			}
			s.BatteryLifePercent = (byte)Math.Clamp(cap, 0, 100);
			BatteryChargeStatus f = 0;
			if (cap > 66) f |= BatteryChargeStatus.High;
			if (cap < 33) f |= BatteryChargeStatus.Low;
			if (cap < 5) f |= BatteryChargeStatus.Critical;
			if (Rd(Path.Combine(node, "status")) == "Charging") f |= BatteryChargeStatus.Charging;
			s.BatteryChargeStatus = f;
		}
		_powerStatus = s;
	}

	// ---- power plan replacement ----
	public void Import_BatteryUtility_PowerPlan()
	{
		try
		{
			if (_inhibitor != null && !_inhibitor.HasExited) return;
			if (Avalonia.Application.Current != null && RippleCC.Platform.AppHost.IsScreenshot) return;
			var psi = new ProcessStartInfo("systemd-inhibit",
				"--what=idle:sleep --who=BatteryPackUtility --why=\"Battery refresh in progress\" --mode=block sleep infinity")
			{ UseShellExecute = false, CreateNoWindow = true };
			_inhibitor = Process.Start(psi);
		}
		catch { Console.WriteLine("Import_BatteryUtility_POwerPlan F3"); }
	}

	public void Delete_BatteryUtility_PowerPlan()
	{
		try
		{
			if (_inhibitor != null && !_inhibitor.HasExited) { _inhibitor.Kill(true); _inhibitor.Dispose(); }
			_inhibitor = null;
		}
		catch { Console.WriteLine("Delete_BatteryUtility_PowerPlan F2"); }
	}
}
