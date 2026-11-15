using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using CCenter.Platform;

namespace DesktopBridge_EnergySave;

/// <summary>
/// Linux adapter for powrprof.dll (same class / method names as the Windows helper).
/// The three stock Windows plans are mapped onto power-profiles-daemon profiles:
///   Balanced = balanced, High performance = performance, Power saver = power-saver.
/// Active plan: `powerprofilesctl get` when present, otherwise the stored value (Reg, "stored only, not applied").
/// </summary>
internal class PowerSchemeHelper
{
	public static readonly Guid Balanced = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");
	public static readonly Guid HighPerformance = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
	public static readonly Guid PowerSaver = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
	const string RegKey = "HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\EnergySave";

	private class PowrProfHelper
	{
		static readonly (Guid g, string name, string profile)[] Plans =
		{
			(Balanced, "Balanced", "balanced"),
			(HighPerformance, "High performance", "performance"),
			(PowerSaver, "Power saver", "power-saver"),
		};

		static string Ppd(params string[] args)
		{
			try
			{
				var psi = new ProcessStartInfo("powerprofilesctl") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
				foreach (var a in args) psi.ArgumentList.Add(a);
				using var p = Process.Start(psi);
				string o = p.StandardOutput.ReadToEnd().Trim();
				if (!p.WaitForExit(2000)) { try { p.Kill(); } catch { } return null; }
				return p.ExitCode == 0 ? o : null;
			}
			catch { return null; }
		}

		public static Guid GetPowerSchemeGuid(uint index) => index < Plans.Length ? Plans[index].g : Guid.Empty;

		public static string GetPowerSchemeFriendlyName(Guid schemeGuid)
		{
			foreach (var p in Plans) if (p.g == schemeGuid) return p.name;
			return null;
		}

		public static bool SetPowerScheme(Guid schemeGuid)
		{
			foreach (var p in Plans)
				if (p.g == schemeGuid)
				{
					Reg.SetValue(RegKey, "PowerPlan", schemeGuid.ToString());   // stored
					Ppd("set", p.profile);                                       // applied only if powerprofilesctl exists
					return true;
				}
			return false;
		}

		public static Guid GetPowerActiveScheme()
		{
			string cur = Ppd("get");
			if (cur != null) foreach (var p in Plans) if (p.profile == cur) return p.g;
			if (Guid.TryParse(Reg.GetString(RegKey, "PowerPlan", ""), out var g)) return g;
			return Balanced;
		}
	}

	public static bool SetPowerScheme(Guid schemeGuid)
	{
		Guid powerActiveScheme = PowrProfHelper.GetPowerActiveScheme();
		if (powerActiveScheme == schemeGuid) return true;
		PowrProfHelper.SetPowerScheme(schemeGuid);
		powerActiveScheme = PowrProfHelper.GetPowerActiveScheme();
		return powerActiveScheme == schemeGuid;
	}

	public static Dictionary<Guid, PowerPlanInfo> GetAllPowerSchemas()
	{
		uint num = 0u;
		Dictionary<Guid, PowerPlanInfo> dictionary = new Dictionary<Guid, PowerPlanInfo>(3);
		Guid powerSchemeGuid;
		do
		{
			powerSchemeGuid = PowrProfHelper.GetPowerSchemeGuid(num);
			if (powerSchemeGuid != Guid.Empty)
			{
				dictionary.Add(powerSchemeGuid, new PowerPlanInfo { SchemeGuid = powerSchemeGuid, FriendlyName = PowrProfHelper.GetPowerSchemeFriendlyName(powerSchemeGuid) });
			}
			num++;
		}
		while (powerSchemeGuid != Guid.Empty);
		return dictionary;
	}

	public static Guid GetPowerActiveScheme() => PowrProfHelper.GetPowerActiveScheme();
}

/// <summary>System.Windows.Forms.SystemInformation.PowerStatus replacement (/sys/class/power_supply).
/// Dev overrides: CCENTER_ES_BATTERY=0..100, CCENTER_ES_AC=0|1.</summary>
internal static class PowerStatusAdapter
{
	const string Root = "/sys/class/power_supply";

	/// <summary>0..1, like PowerStatus.BatteryLifePercent (1 when no battery).</summary>
	public static float BatteryLifePercent
	{
		get
		{
			var o = Environment.GetEnvironmentVariable("CCENTER_ES_BATTERY");
			if (int.TryParse(o, out var ov)) return ov / 100f;
			try
			{
				foreach (var d in Directory.GetDirectories(Root))
					if (File.Exists(d + "/type") && File.ReadAllText(d + "/type").Trim() == "Battery" && File.Exists(d + "/capacity"))
						return int.Parse(File.ReadAllText(d + "/capacity").Trim()) / 100f;
			}
			catch { }
			return 1f;
		}
	}

	/// <summary>0 = Offline, 1 = Online, 255 = Unknown (PowerLineStatus).</summary>
	public static int PowerLineStatus
	{
		get
		{
			var o = Environment.GetEnvironmentVariable("CCENTER_ES_AC");
			if (int.TryParse(o, out var ov)) return ov != 0 ? 1 : 0;
			try
			{
				foreach (var d in Directory.GetDirectories(Root))
					if (File.Exists(d + "/type") && File.ReadAllText(d + "/type").Trim() == "Mains" && File.Exists(d + "/online"))
						return File.ReadAllText(d + "/online").Trim() == "1" ? 1 : 0;
			}
			catch { }
			return 255;
		}
	}
}
