using System;
using System.Collections;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CCenter.Platform;

namespace ControlCenter30;

public partial class App : Application
{
	public static ArrayList SupportArray = new ArrayList();

	private static int AppCount = 0;

	private static int kb_type = 0;

	// FlexiAccess is a Windows UWP companion that has no Linux port -> never installed.
	private static bool isFA30Installed = false;

	public static int custom_id = 0;

	public static int clevo_id = 0;

	private const int PAGE_SIZE = 256;

	private byte[] _wmi13 = null;

	static FileStream instanceLock;

	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
		{
			if (!App_Startup(d)) { Environment.Exit(0); }
		}
		base.OnFrameworkInitializationCompleted();
	}

	private bool App_Startup(IClassicDesktopStyleApplicationLifetime life)
	{
		if (Backend.IsMock) Backend.Current = new ProfileBackend(Backend.Current);
		if (!CheckAppExistOrNot()) return false;
		CheckFA30Installed();
		GetWMIFlag(70, DCHU.GetWMI(70));
		kb_type = GetWmi13andReturnKBType();
		custom_id = GetWmi17andReturnCustomID();
		clevo_id = GetCLEVO_ID();
		Init_WMI();
		SetAppVersion();
		if (AppCount == 0)
		{
			Msg.Show("OSD Only");
			return false;
		}
		ServiceBridge.Install();
		Global.REventlog.InitialEventLog();
		Window w;
		if (Global.SupportBIT.PowerModeUI_ID == 2)
		{
			var window_MOUSE = new Window_MOUSE();
			window_MOUSE.Show();
			window_MOUSE.Topmost = false;
			w = window_MOUSE;
		}
		else
		{
			var window = new Window1();
			window.Show();
			window.Topmost = false;
			w = window;
		}
		life.MainWindow = w;
		SetCC30_APP_Launch(1);
		return true;
	}

	private bool CheckFA30Installed()
	{
		return isFA30Installed;
	}

	// Original: Process.GetProcessesByName(self).Length > 1 -> exit. Port: exclusive lock file.
	private bool CheckAppExistOrNot()
	{
		if (AppHost.IsScreenshot) return true;
		try
		{
			var dir = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR") ?? Path.GetTempPath();
			instanceLock = new FileStream(Path.Combine(dir, "ccenter-controlcenter30.lock"), FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			return true;
		}
		catch (IOException) { return false; }
		catch { return true; }
	}

	private void SetAppVersion()
	{
		int value = 1;
		int value2 = 7;
		int value3 = 0;
		int value4 = 0;
		byte[] bytes = BitConverter.GetBytes(value);
		DCHU.WriteAppSettings(0, 4, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(value2);
		DCHU.WriteAppSettings(0, 5, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(value3);
		DCHU.WriteAppSettings(0, 6, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(value4);
		DCHU.WriteAppSettings(0, 7, 1, ref bytes[0]);
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

	private int GetWmi13andReturnKBType()
	{
		_wmi13 = new byte[256];
		DCHU.GetDCHU_Data(13, ref _wmi13[0]);
		return _wmi13[15] & 0xF;
	}

	private int GetWmi17andReturnCustomID()
	{
		byte[] wMIPackage = DCHU.GetWMIPackage(17);
		return wMIPackage[2];
	}

	private static int GetWmi122Bit23()
	{
		int data = 0;
		DCHU.GetDCHU_Data(122, ref data);
		if ((data & 0x800000) == 8388608)
		{
			return 1;
		}
		return 0;
	}

	public static void GetWMIFlag(int command, int WMI_Data)
	{
		switch (command)
		{
		case 16:
			SupportArray.Clear();
			if ((WMI_Data & 1) == 1)
			{
				SupportArray.Add(1);
				AppCount++;
			}
			else
			{
				SupportArray.Add(0);
			}
			if ((WMI_Data & 2) == 2)
			{
				SupportArray.Add(1);
				AppCount++;
				Global.SupportBIT.APP_Flexikey = true;
			}
			else
			{
				SupportArray.Add(0);
			}
			if ((WMI_Data & 4) == 4 && isFA30Installed)
			{
				SupportArray.Add(1);
				AppCount++;
			}
			else
			{
				SupportArray.Add(0);
			}
			if ((WMI_Data & 8) == 8 || (WMI_Data & 0x10) == 16)
			{
				if (kb_type == 0 || kb_type == 4)
				{
					SupportArray.Add(0);
					SupportArray.Add(0);
				}
				else
				{
					SupportArray.Add(0);
					SupportArray.Add(1);
					AppCount++;
					Global.SupportBIT.APP_LEDKeyboardSetting = true;
				}
			}
			else
			{
				SupportArray.Add(0);
				SupportArray.Add(0);
			}
			if ((WMI_Data & 0x20) == 32)
			{
				SupportArray.Add(1);
				AppCount++;
			}
			else
			{
				SupportArray.Add(0);
			}
			if ((WMI_Data & 0x40) == 64 && Global.SupportBIT.CPUOC)
			{
				SupportArray.Add(1);
				AppCount++;
			}
			else
			{
				SupportArray.Add(0);
			}
			if ((WMI_Data & 0x80) == 128)
			{
				SupportArray.Add(1);
				AppCount++;
				Global.SupportBIT.APP_FanSpeedSetting = true;
			}
			else
			{
				SupportArray.Add(0);
			}
			if ((WMI_Data & 0x100) == 256)
			{
				SupportArray.Add(1);
				AppCount++;
			}
			else
			{
				SupportArray.Add(0);
			}
			if ((WMI_Data & 0x200) == 512)
			{
				SupportArray.Add(1);
				AppCount++;
			}
			else
			{
				SupportArray.Add(0);
			}
			if (((WMI_Data >> 10) & 1) == 1)
			{
				Global.SupportBIT.UWP_PgUpDnKey = true;
			}
			break;
		case 122:
			if (((WMI_Data >> 23) & 1) == 1)
			{
				Global.SupportBIT.CPUOC = true;
			}
			break;
		case 96:
			if (((WMI_Data >> 12) & 1) == 1)
			{
				Global.SupportBIT.DTT = true;
			}
			if (((WMI_Data >> 13) & 1) == 1)
			{
				Global.SupportBIT.AMD_ManualMode = true;
			}
			break;
		}
	}

	public static int GetPowerMode()
	{
		int num = -1;
		int num2 = 1;
		byte[] array = new byte[num2];
		int offset = 1;
		int page = 1;
		int num3 = DCHU.ReadAppSettings(page, offset, num2, ref array[0]);
		return array[0];
	}

	public static void SetPreviousPowerMode(int mode)
	{
		byte[] bytes = BitConverter.GetBytes(mode);
		DCHU.WriteAppSettings(1, 30, 1, ref bytes[0]);
	}

	public static int GetPreviousPowerMode()
	{
		int num = -1;
		int num2 = 1;
		byte[] array = new byte[num2];
		int offset = 30;
		int page = 1;
		int num3 = DCHU.ReadAppSettings(page, offset, num2, ref array[0]);
		return array[0];
	}

	public static int GetCLEVO_ID()
	{
		int num = 0;
		int num2 = 2;
		byte[] array = new byte[num2];
		int offset = 0;
		int page = 0;
		int num3 = DCHU.ReadAppSettings(page, offset, num2, ref array[0]);
		return array[1] + (array[0] << 8);
	}

	public static void Init_WMI()
	{
		byte[] array = new byte[256];
		array = DCHU.ReadAppSettings(7, 0, 256);
		Global.SupportBIT.SupportBIOSFeatureList_Version = (array[0] << 8) + array[1];
		switch (Global.SupportBIT.SupportBIOSFeatureList_Version)
		{
		case 0:
			BIOSSpecialFeature_v0_0(array);
			break;
		case 256:
			BIOSSpecialFeature_v1_0(array);
			break;
		default:
			BIOSSpecialFeature_v0_0(array);
			break;
		}
	}

	private static void ApplyPowerModeBits(byte[] offset)
	{
		if ((offset[17] & 0xF) != 0)
		{
			Global.SupportBIT.PowerModeBit = true;
			if ((offset[17] & 1) == 1)
			{
				Global.SupportBIT.PowerModeBit_Quite = true;
			}
			if (((offset[17] >> 1) & 1) == 1)
			{
				Global.SupportBIT.PowerModeBit_PowerSaving = true;
			}
			if (((offset[17] >> 2) & 1) == 1)
			{
				Global.SupportBIT.PowerModeBit_Performance = true;
			}
			if (((offset[17] >> 3) & 1) == 1)
			{
				Global.SupportBIT.PowerModeBit_Entertainment = true;
			}
		}
		Global.SupportBIT.PowerModeUI_ID = (offset[17] >> 4) & 0xF;
	}

	private static void BIOSSpecialFeature_v0_0(byte[] offset)
	{
		GetWMIFlag(122, DCHU.GetWMI(122));
		GetWMIFlag(16, DCHU.GetWMI(16));
		GetWMIFlag(96, DCHU.GetWMI(96));
		ApplyPowerModeBits(offset);
	}

	private static void BIOSSpecialFeature_v1_0(byte[] offset)
	{
		int num = (offset[5] << 24) + (offset[4] << 16) + (offset[3] << 8) + offset[2];
		int wMI_Data = (offset[9] << 24) + (offset[8] << 16) + (offset[7] << 8) + offset[6];
		int wMI_Data2 = (offset[11] << 8) + offset[10];
		int wMI_Data3 = (offset[14] << 8) + offset[13];
		GetWMIFlag(122, wMI_Data);
		GetWMIFlag(16, wMI_Data3);
		GetWMIFlag(96, wMI_Data2);
		if (((offset[16] >> 4) & 1) == 1)
		{
			Global.SupportBIT.TurboFan = true;
		}
		ApplyPowerModeBits(offset);
	}
}
