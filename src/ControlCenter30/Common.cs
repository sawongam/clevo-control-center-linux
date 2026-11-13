using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace ControlCenter30;

public class Common
{
	public enum ModeType
	{
		quiet,
		pwrsaving,
		performance,
		entertainment
	}

	public int powerModeValue = 3;

	// Original: Launcher.LaunchUriAsync(new Uri(appName + ":")) for the UWP bridge apps, after the
	// "w_arg253^launchapp^..." command was sent to the service.
	// Port: AppHost.LaunchSibling(<project name>). Mapping of the URI scheme names:
	//   clevokeyboard / clevoperkey -> LedKeyboardSetting     clevoflexikey -> Flexikey
	//   clevofan -> FanSpeedSetting                           clevofa (FlexiAccess) -> none (not ported)
	//   clevocpu -> CPUOC     clevogpu -> GPUOverclocking     clevoenergy -> EnergySave     clevobattery -> BatteryUtility
	internal static void MapApp(string appName, out string command, out string processName, out string project)
	{
		command = "";
		processName = "";
		project = null;
		if (appName.ToLower().Contains("clevokeyboard") || appName.ToLower().Contains("clevoperkey"))
		{
			command = "w_arg253^launchapp^CLEVOCO.LedKeyboardSetting_6h6z29zh29qx0^no";
			processName = "LedKeyboardSetting";
			project = "LedKeyboardSetting";
		}
		else if (appName.ToLower().Contains("clevoflexikey"))
		{
			command = "w_arg253^launchapp^CLEVOCO.Flexikey_6h6z29zh29qx0^no";
			processName = "CC";
			project = "Flexikey";
		}
		else if (appName.ToLower().Contains("clevofan"))
		{
			command = "w_arg253^launchapp^CLEVOCO.504814C03D814_6h6z29zh29qx0^no";
			processName = "FanSpeedSetting";
			project = "FanSpeedSetting";
		}
		else if (appName.ToLower().Contains("clevofa"))
		{
			command = "w_arg253^launchapp^CLEVOCO.FlexiAccess_6h6z29zh29qx0^no";
			processName = "FA30";
		}
		else if (appName.ToLower().Contains("clevocpu"))
		{
			command = "w_arg253^launchapp^CLEVOCO.CPUOverclocking_6h6z29zh29qx0^no";
			processName = "CPUOC2.0";
			project = "CPUOC";
		}
		else if (appName.ToLower().Contains("clevogpu"))
		{
			command = "w_arg253^launchapp^CLEVOCO.GPUOverclocking_6h6z29zh29qx0^no";
			processName = "GPUOverclocking";
			project = "GPUOverclocking";
		}
		else if (appName.ToLower().Contains("clevoenergy"))
		{
			command = "w_arg253^launchapp^CLEVOCO.EnergySaveUtility_6h6z29zh29qx0^no";
			processName = "DesktopBridge_EnergySave";
			project = "EnergySave";
		}
		else if (appName.ToLower().Contains("clevobattery"))
		{
			command = "w_arg253^launchapp^CLEVOCO.BatteryPackUtility_6h6z29zh29qx0^no";
			processName = "BatteryPackUtility";
			project = "BatteryUtility";
		}
	}

	public void LaunchApp(string appName)
	{
		MapApp(appName, out var command, out var processName, out var project);
		DebugLog.WriteLog2InLog(command);
		Thread.Sleep(1000);
		if (!IsProcessExist(project ?? processName))
		{
			if (project != null) RippleCC.Platform.AppHost.LaunchSibling(project);
		}
	}

	/// <summary>True when a process whose command line mentions the sibling project name is running (Linux: scans /proc).</summary>
	internal static bool IsProcessExist(string processName)
	{
		if (string.IsNullOrEmpty(processName)) return false;
		try
		{
			int self = Environment.ProcessId;
			foreach (var dir in Directory.GetDirectories("/proc"))
			{
				var name = Path.GetFileName(dir);
				if (!int.TryParse(name, out var pid) || pid == self) continue;
				string cmd;
				try { cmd = File.ReadAllText(Path.Combine(dir, "cmdline")); } catch { continue; }
				foreach (var tok in cmd.Split('\0'))
				{
					var f = Path.GetFileName(tok);
					if (f == processName || f == processName + ".dll") return true;
				}
			}
		}
		catch { }
		return false;
	}

	public void Close_UI()
	{
		SetCC30_APP_Launch(0);
	}

	public int SetCC30_APP_Launch(int value)
	{
		int page = 1;
		int offset = 7;
		byte[] bytes = BitConverter.GetBytes(value);
		int length = 1;
		int num = 0;
		do
		{
			DCHU.WriteAppSettings(page, offset, length, bytes);
			byte[] array = DCHU.ReadAppSettings(page, offset, length);
			if (array[0].Equals(bytes[0]))
			{
				return 1;
			}
		}
		while (num++ < 3);
		return 1;
	}

	public void ReadPowerModeInsydeBuffer()
	{
		int num = -1;
		int num2 = 1;
		byte[] array = new byte[num2];
		int offset = 1;
		int page = 1;
		int num3 = DCHU.ReadAppSettings(page, offset, num2, ref array[0]);
		num = array[0];
		switch (num)
		{
		case 0:
			powerModeValue = 0;
			break;
		case 1:
			powerModeValue = 1;
			break;
		case 2:
			powerModeValue = 2;
			break;
		case 3:
			powerModeValue = 3;
			break;
		default:
			DebugLog.WriteDbgLog2Outlog("clovocc30, ReadPowerModeInsydeBuffer error, data=" + num);
			powerModeValue = 3;
			break;
		}
	}
}
