using System;
using System.IO;
using System.Text;
using CC.other_Class;
using CCenter.Platform;
namespace CC;

/// <summary>
/// DCHU package 7 ("WMI_7", battery pack data) as read by Page_Battery_Refresh.Read_package_WMI7/Read_battery_ini.
/// The real firmware/daemon answer is used as is. If the package is empty (design capacity 0: the in-process
/// simulator, or a daemon that does not serve it) the same layout is synthesised from /sys/class/power_supply and
/// the shipped Battery.ini (thresholds + criteria), which is what the Windows service/firmware provides.
/// Layout (from the original parser): [0..1] version, [2..3] manufacturer date, [4..5] cycle count,
/// [6..7] full charge capacity, [8..9] design capacity, [10..11] battery status (0x10 full, 0x20 fully discharged),
/// [12..15] PF status, [16..19] operation status, [20] stop-charging threshold,
/// [32..45] thresholds, [48..] Replace criteria '$'-terminated, [80..] Warning criteria '$'-terminated.
/// </summary>
internal static class BatteryPackage
{
	public static byte[] Read()
	{
		byte[] a = Global.insyde.GetWMIPackage(7);
		int dc = (a[9] << 8) + a[8];
		if (dc != 0) return a;
		return Synthesize();
	}

	static int IniInt(string sect, string key, int def)
	{
		var s = Global.RW_ini.Read_Batery(sect, key);
		return int.TryParse(s.Trim(), out var v) ? v : def;
	}
	static string IniStr(string sect, string key, string def)
	{
		var s = Global.RW_ini.Read_Batery(sect, key);
		return s == "null" || s.Trim() == "" ? def : s.Trim();
	}
	static void U16(byte[] b, int at, int v) { b[at] = (byte)v; b[at + 1] = (byte)(v >> 8); }

	static byte[] Synthesize()
	{
		var b = new byte[256];
		var node = battery.BatNode();
		int fcc = 0, dc = 0, cycles = 0; bool full = false; int cap = 100;
		if (node != null)
		{
			long f = battery.RdL(Path.Combine(node, "charge_full"), battery.RdL(Path.Combine(node, "energy_full"), 0));
			long d = battery.RdL(Path.Combine(node, "charge_full_design"), battery.RdL(Path.Combine(node, "energy_full_design"), 0));
			fcc = (int)(f / 1000); dc = (int)(d / 1000);
			cycles = (int)Math.Max(0, battery.RdL(Path.Combine(node, "cycle_count"), 0));
			full = battery.Rd(Path.Combine(node, "status")) == "Full";
			cap = (int)battery.RdL(Path.Combine(node, "capacity"), 100);
		}
		if (dc <= 0) { dc = 4000; fcc = 4000; }
		if (fcc <= 0) fcc = dc;
		DateTime made = DateTime.Now.AddYears(-1);
		U16(b, 0, 1);
		U16(b, 2, (made.Year - 1980) * 512 + made.Month * 32 + made.Day);
		U16(b, 4, cycles);
		U16(b, 6, fcc);
		U16(b, 8, dc);
		U16(b, 10, (full ? 0x10 : 0) | (cap <= 0 ? 0x20 : 0));
		b[20] = 5;   // stop-charging threshold (not exposed by sysfs)
		b[32] = (byte)IniInt("Threshold", "ReplaceThresholdCapacity", 65);
		U16(b, 33, IniInt("Threshold", "ReplaceThresholdCyclecount", 700));
		b[35] = (byte)IniInt("Threshold", "ReplaceThresholdManufacturerdate", 7);
		b[36] = (byte)IniInt("Threshold", "Replace1ThresholdCapacity", 70);
		U16(b, 37, IniInt("Threshold", "Replace1ThresholdCyclecount", 600));
		b[39] = (byte)IniInt("Threshold", "WarningThresholdCapacity", 80);
		U16(b, 40, IniInt("Threshold", "WarningThresholdCyclecount", 560));
		b[42] = (byte)IniInt("Threshold", "WarningThresholdManufacturerdate", 6);
		b[43] = (byte)IniInt("Threshold", "Warning1ThresholdCapacity", 85);
		U16(b, 44, IniInt("Threshold", "Warning1ThresholdCyclecount", 500));
		var rep = Encoding.ASCII.GetBytes(IniStr("Criteria", "Replace", "(A+B+C)+(G*H)") + "$");
		var war = Encoding.ASCII.GetBytes(IniStr("Criteria", "Warning", "(D+E+F)+(I*J)") + "$");
		Array.Copy(rep, 0, b, 48, Math.Min(32, rep.Length));
		Array.Copy(war, 0, b, 80, Math.Min(32, war.Length));
		return b;
	}
}
