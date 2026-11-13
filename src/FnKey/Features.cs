using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Clevo;
using FnKey.Device;
using RippleCC.Platform;

namespace FnKey;

// Port of FnKey.Features. Methods that only use DCHU / AppSettings are copied unchanged from the original.
// Windows-only parts (registry radio state, keybd_event, SendInput, WMI, powercfg, display modes, AMD ryzen.dll,
// XTU) are replaced by Linux adapters or by documented stubs (see PORT_NOTES.md section "Features stubs").
public class Features
{
	public struct AMD_Ryzen
	{
		public uint PPT;

		public uint TDC;

		public uint EDC;
	}

	public struct AMD_Mode
	{
		public AMD_Ryzen Default;
		public AMD_Ryzen Thermal_1;
		public AMD_Ryzen Thermal_2;
		public AMD_Ryzen Thermal_3;
		public AMD_Ryzen Thermal_4;
		public AMD_Ryzen TotalPower_1;
		public AMD_Ryzen Entertainment_Thermal_1;
		public AMD_Ryzen Entertainment_Thermal_2;
		public AMD_Ryzen Entertainment;
		public AMD_Ryzen Performance;
		public AMD_Ryzen Pwrsaving;
		public AMD_Ryzen Quiet;
		public AMD_Ryzen DC;
		public AMD_Ryzen DC1;
		public AMD_Ryzen DC2;
	}

	public bool LoadDefault;
	public int PCbeep;
	public bool OCLoadDefault;
	public int DefaultPowerConverationMode = 3;
	public int EnergyStar15C;
	public int BatteryChargeControl;
	public bool SupportFanLess;
	public bool SupportTpSmbios;
	public bool SupportLogoColor;
	public bool SupportWmi3dHp;
	public bool SupportMSHybrid_dGPUSwicth;
	public bool SupportEthernetLED;
	public bool SupportEss;
	public int WakeUpLanState;
	public bool SupportModernStandby;
	public bool OLED_Panel;
	public byte CustomerID;
	public bool AMD_CPU;
	public int NBC_Status = 99;
	public bool GPU_Turbo_Off;
	public bool AMD_TotalPower_Limit;
	public bool AMD_Thermal_Limit;
	public int ClevoID;
	public int DCMode_PowerMode;
	public bool Arg001ByDCHUServiceSend;
	public bool BatteryUtilityRefresh;
	public int SupportUWPAppsflags;
	public int SupportUWPAppsCount;
	public bool SupportPowerMode;
	public bool SupportFlexikey;
	public bool SupportFlexiAccess;
	public bool SupportGPUOC;
	public bool SupportCPUOC_WMI16Bit6;
	public bool SupportFanSpeedSetting;
	public bool SupportEnergySave;
	public bool SupportBatteryUtility;
	public bool SupportDisable_PgUpDnKey;
	public bool CPUCore1WithDefaultValue;
	public int SupportBIOSFeatureList_Version;
	public bool SupportLimitGPUClock;
	public bool SupportGC6Setting;
	public bool SupportBatterySaverSetting;
	public bool SupportDisplayTimeNever;
	public bool DisableFnF2TouchPadHook;
	public bool SupportRescanCardReader;
	public bool DisableLAN_PowerSavingMode;
	public bool SupportRescanPS2Mouse;
	public bool SupportMEPowerGatingDisable;
	public bool SupportPCI5Enable;
	public bool SupportSetDeviceSavePower;
	public bool SupportASPMControl;
	public bool SupportTurboFan;
	public bool SupportAMDCC30PowerMode;
	public bool SupportBATLowControl;
	public bool SupportPowerModeBit;
	public bool SupportPowerModeBit_Quite;
	public bool SupportPowerModeBit_PowerSaving;
	public bool SupportPowerModeBit_Performance;
	public bool SupportPowerModeBit_Entertainment;
	public int SupportPowerModeUI_ID;
	public bool SupportMSHybrid_dGPU_iGPUSwicth;
	public bool SupportRemoveHDRLimit;
	public bool SupportFix165MHzPanelS3ResumeScreenFlicker;
	public bool SupportFixUCSI_DeviceYellowMarkWorkaround;
	public bool SupportExchangePanelRefreshRate = true;
	public bool DefaultEnableExchangePanelRefreshRate = true;
	public bool SupportMovieTVnoise;
	public bool SupportMovieTVnoise_ASPM;
	public bool SupportSetPowerPlan;
	public bool SupportDTT_iSST_DeviceYellowMarkWorkaround;
	public bool SupportCheckRealtekLANSavePower;
	public bool SupportRebootRescanCardReader;
	public bool NoSupportHW_KB_Type;
	public bool CheckUnigine;
	public bool SupportNewMode;
	public bool SupportSandTPScanCode = true;
	public bool SupportAllModeGPUOC;
	public bool SupportAntiDust_Fan;
	public bool SupportPerkeyDefaultStaticColor;
	public bool SupportDisablePCIE_ASPM;
	public bool SupportFanOffset = true;
	public bool SupportLightBar_X170;
	public bool SupportDTT;
	public bool AMD_Manual_Mode;
	public bool FanSpeedSupport;
	public bool SupportGamingGroup;
	public bool Support3dHeadphone;
	public bool CustomFanSupport;
	public bool WakeUpLanSupport;
	public bool SupportCheckNotAirplaneOsd;
	public bool SupportUsbHubReset;
	public bool SupportBootChangeCPUVoltage;
	public bool SupportTpColor;
	public bool SupportLightbar;
	public bool Support6WhKb;
	public bool FanExists;
	public bool SupportCPUOC_WMI122Bit23;
	public bool SupportXMP;
	public int SupportGPUOC_WMI;
	public int SupportGPUcustom_WMI;
	public int SupportKBLEDsleep_WMI;
	public bool SupportHW_P_state;
	public string System_ManufacturerName = "Notebook";
	public bool SupportMonitorLowRefreshRate;
	public bool IsLowRefreshRateChecked;
	public bool SupportI2C_TP;
	public bool RunNVSRSolution;
	public MonitorControler MyMonitorControler = new MonitorControler();
	private CheckDrivers checkDriverInstall = new CheckDrivers();
	public static Guid guidBalance = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");
	public static Guid guidPowerSaver = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");
	public static Guid guidHighPerformance = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");
	public AMD_Mode clevo_AMD_Mode;
	public uint AMD_PPT_Now = 999u;
	public uint AMD_TDC_Now = 999u;
	public uint AMD_EDC_Now = 999u;
	public int Mode_From_BIOS;
	public int Frequency_From_BIOS;
	public float Voltage_From_BIOS;

	public int ACMode_PowerMode
	{
		get
		{
			int page = 1;
			int offset = 16;
			int length = 1;
			try
			{
				return Global.dchu.ReadAppSettings(page, offset, length)[0];
			}
			catch
			{
				DebugLog.WriteDbgLog2Outlog("ACMode_PowerMode read error");
			}
			return 0;
		}
		set
		{
			int page = 1;
			int offset = 16;
			byte[] bytes = BitConverter.GetBytes(value);
			int length = 1;
			Global.dchu.WriteAppSettings(page, offset, length, bytes);
		}
	}

	// ---------------------------------------------------------------- Linux adapters (replace Windows API use)

	public bool GetI2C_TP()
	{
		// Windows: HKCU\...\PrecisionTouchPad\Status Enabled. Linux: desktop-specific (libinput / gsettings / KDE);
		// persisted state of the last toggle done through SetI2C_TP is used (AppSettings page 1 off 10 = what Send_Win_Ctrl_F24 wrote).
		try { return Reg.GetInt("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\PrecisionTouchPad\\Status", "Enabled", 1) == 1; }
		catch { return true; }
	}

	public bool SetI2C_TP(bool status)
	{
		int num = GetI2C_TP() ? 1 : 0;
		if (!status && num == 1)
		{
			Send_Win_Ctrl_F24(0);
		}
		if (status && num == 0)
		{
			Send_Win_Ctrl_F24(1);
		}
		return true;
	}

	// Windows: synthesizes Win+Ctrl+F24 (the PTP toggle shortcut) with keybd_event. Linux: STUB - there is no such
	// shortcut; the desktop (KDE/GNOME) already handles the touchpad key natively. State bookkeeping is kept.
	private void Send_Win_Ctrl_F24(int status)
	{
		Reg.SetValue("HKEY_CURRENT_USER\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\PrecisionTouchPad\\Status", "Enabled", status);
		if (status == 0)
		{
			DebugLog.WriteDbgLog2Inlog("clevofnkey^206^0");
		}
		else
		{
			DebugLog.WriteDbgLog2Inlog("clevofnkey^206^1");
		}
		int page = 1;
		int offset = 10;
		byte[] bytes = BitConverter.GetBytes(status);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
	}

	public void MonitorControl(bool status)
	{
		if (status)
		{
			MyMonitorControler.MonitorOn();
		}
		else
		{
			MyMonitorControler.MonitorOff();
		}
	}

	public void SetScreenOff()
	{
		DebugLog.WriteDbgLog2Outlog("STUB Set Screen Off (Windows: synthetic mouse click + scrnsave.scr /s)");
	}

	public void Insyde_AirplaneMode_Control()
	{
		// Windows: HKLM\...\RadioManagement\SystemRadioState (1 = airplane on) and radio API SetAirplaneMode(). Linux: rfkill.
		if (!Rfkill.AirplaneOn())
		{
			Global.dchu.SetWMI(121, 26, 1u);
			Rfkill.SetAirplaneMode(1);
		}
		else
		{
			Global.dchu.SetWMI(121, 26, 0u);
			Rfkill.SetAirplaneMode(0);
		}
	}

	public void GetInfo()
	{
		string text = "None";
		try { text = File.ReadAllText("/sys/class/dmi/id/bios_version").Trim(); } catch { }
		DebugLog.WriteDbgLog2Outlog("BIOSVersion: " + text);
		DebugLog.WriteDbgLog2Outlog("ECVersion:" + Global.dchu.GetECVersion());
		Global.dchu.GetECoff();
	}

	public bool IsUSBKeyboardExist()
	{
		// Windows: Win32_Keyboard WMI rows containing "USB". Linux: any input device on the USB bus with a keyboard handler.
		try
		{
			foreach (var d in Directory.GetDirectories("/sys/class/input", "input*"))
			{
				string name = File.Exists(d + "/name") ? File.ReadAllText(d + "/name") : "";
				string phys = File.Exists(d + "/phys") ? File.ReadAllText(d + "/phys") : "";
				if (phys.StartsWith("usb-") && name.ToLower().Contains("keyboard")) return true;
			}
		}
		catch { }
		return false;
	}

	public void SetBrightnessFromWMI17(int Event)
	{
		LCDBrightness lCDBrightness = new LCDBrightness();
		int num = 0;
		byte[] array = new byte[256];
		array = Global.dchu.GetWMIPackage(17);
		switch (Event)
		{
		case 78:
			num = array[210];
			break;
		case 79:
			num = array[211];
			if (num == 0)
			{
				num = 10;
			}
			break;
		case 48:
			num = array[212];
			if (num == 0)
			{
				num = 20;
			}
			break;
		case 49:
			num = array[213];
			if (num == 0)
			{
				num = 30;
			}
			break;
		case 50:
			num = array[214];
			if (num == 0)
			{
				num = 40;
			}
			break;
		case 51:
			num = array[215];
			if (num == 0)
			{
				num = 50;
			}
			break;
		case 52:
			num = array[216];
			if (num == 0)
			{
				num = 60;
			}
			break;
		case 53:
			num = array[217];
			if (num == 0)
			{
				num = 70;
			}
			break;
		case 54:
			num = array[218];
			if (num == 0)
			{
				num = 80;
			}
			break;
		case 55:
			num = array[219];
			if (num == 0)
			{
				num = 90;
			}
			break;
		case 56:
			num = array[220];
			if (num == 0)
			{
				num = 100;
			}
			break;
		default:
			DebugLog.WriteDbgLog2Outlog("SetBrightnessFromWMI17, Unknown Event:" + Event);
			break;
		}
		int num2 = 0;
		do
		{
			num2++;
			lCDBrightness.SetBrightness(num);
			DebugLog.WriteDbgLog2Outlog("SetBrightnessFromWMI17, Event: " + Event + ", brightness: " + num + ", Count=" + num2);
			LCDBrightness.Brightness brightness = lCDBrightness.GetBrightness();
			Global.BAT_PowerStatus.UpdatePowerInfo();
			if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online)
			{
				if (brightness.AC == num)
				{
					break;
				}
			}
			else if (brightness.DC == num)
			{
				break;
			}
			Thread.Sleep(100);
		}
		while (num2 <= 2);
	}

	// ---------------------------------------------------------------- Windows-only: stubs (kept so Form1 flow is unchanged)

	public void Rotation180() { DebugLog.WriteDbgLog2Outlog("STUB Rotation180 (Windows ChangeDisplaySettingsEx; on Linux use the desktop's display settings)"); }
	public bool FlexikeyInit() { DebugLog.WriteDbgLog2Outlog("STUB FlexikeyInit (kb.dll/profile.xml key filter is Windows-only)"); return true; }
	public void SetKeybordMouseEnble(bool IsEnable) { DebugLog.WriteDbgLog2Outlog("STUB SetKeybordMouseEnble(" + IsEnable + ") (Windows-only key filter driver)"); }
	public void CheckExecuteCOUWP() { DebugLog.WriteDbgLog2Outlog("OnStart, CheckExecuteCOUWP (UWP package check; n/a on Linux)"); }
	public void CheckModernStandbySupport()
	{
		// Windows: "powercfg /a" lists S0 Low Power Idle. Linux: /sys/power/mem_sleep default [s2idle] = modern standby.
		try
		{
			string ms = File.ReadAllText("/sys/power/mem_sleep");
			Global.features.SupportModernStandby = ms.Contains("[s2idle]");
		}
		catch { Global.features.SupportModernStandby = false; }
		if (Global.features.SupportModernStandby) DebugLog.WriteDbgLog2Outlog("SupportModernStandby");
	}
	public bool NvsrSolutionCheck() { return false; }  // AuxSwitchCC30.dll NVSR panel workaround (Windows-only)
	public bool OLED_Panel_Check() { return false; }   // EDID registry scan (Windows-only); Linux: would read /sys/class/drm/*/edid
	public void RunNvsrWorkaround() { }
	public void KillAllApps() { DebugLog.WriteDbgLog2Outlog("STUB KillAllApps"); }
	public void SetPanelRefreshRate(int preMode) { DebugLog.WriteDbgLog2Outlog("STUB SetPanelRefreshRate(" + preMode + ") (Windows display-mode switch)"); }
	public bool ReadLowRefreshRateCheckedFlag() { return Global.dchu.ReadAppSettings(1, 30, 1)[0] == 1; }
	public void CheckPanelSupportLowRefreshRate() { }
	public void CheckIfSupportLowRefreshRate() { }
	public void Load_PgUpDnKey() { DebugLog.WriteDbgLog2Outlog("STUB Load_PgUpDnKey (Windows key filter)"); }
	public void CheckRealtekLANSavePower() { }
	public void EnergySaveInit() { DebugLog.WriteDbgLog2Outlog("STUB EnergySaveInit (EnergySave app is ported separately)"); }
	public void UpdateWolStatus(bool IsShutdown) { DebugLog.WriteDbgLog2Outlog("STUB UpdateWolStatus (Windows registry/powercfg)"); }
	public bool IsLaunchFnKeyNeedToUpdate() { return false; }
	public void CopyLaunchFnKey() { }
	public void WMIDeviceChangeDetect() { }
	public void RegisterOOBENotification() { }
	public bool IsWindows11() { return false; }
	public void SetXTU1_0() { DebugLog.WriteDbgLog2Outlog("STUB SetXTU1_0 (Intel XTU - Windows-only; CPU OC is handled by the CPU_OC port and HardwareGate)"); }
	public int SetXTU2_0() { DebugLog.WriteDbgLog2Outlog("STUB SetXTU2_0 (Intel XTU - Windows-only)"); return 0; }
	public bool SetXTUService(int value) { return true; }
	public void CheckAndExecuteCPUOC_Background() { }
	public void XTUTimerTick1_0() { }
	public void XTUTimerTick2_0() { }
	public void ReleasePROC_HOT() { }
	public void SetWakeUpOnLan() { DebugLog.WriteDbgLog2Outlog("STUB SetWakeUpOnLan (Windows registry/powercfg; Linux: ethtool wol)"); }
	// Beeper: original loops nosound.wav to keep the audio path awake. Not needed on Linux.
	public void LoopBeeperStart() { DebugLog.WriteDbgLog2Outlog("STUB LoopBeeperStart"); }
	public void LoopBeeperStop() { DebugLog.WriteDbgLog2Outlog("STUB LoopBeeperStop"); }
	private enum AMDConvert { IndexToFrequency, IndexToVoltage }
	private int AMDConverTable(AMDConvert action, int input) { return input; } // ryzen index tables: AMD models only

	public bool IsPCCExist()
	{
		try { return System.Diagnostics.Process.GetProcessesByName("PowerConsumptionControl").Length > 0; } catch { return false; }
	}

	public bool IsProcessExist(string processName)
	{
		try { return System.Diagnostics.Process.GetProcessesByName(processName).Length > 0; } catch { return false; }
	}

	public string GetOEM_ini_Data(string section, string name)
	{
		// original reads C:\Program Files (x86)\ControlCenter\oem.ini
		string result = null;
		StringBuilder sb = new StringBuilder(255);
		Ini.GetPrivateProfileString(section, name, "null", sb, 255, Path.Combine(AppContext.BaseDirectory, "oem.ini"));
		if (sb.ToString() != "null") result = sb.ToString();
		return result;
	}

	// AMD Ryzen (ryzen.dll / ryzenmaster SDK): only reachable on ClevoID 0x50F0/0x50F4 AMD models. Not applicable to this Intel laptop.
	public void AMD_CPU_Check() { }
	public void AMD_init() { DebugLog.WriteDbgLog2Outlog("STUB AMD_init (ryzen.dll, AMD models only)"); }
	public void AMD_init_2_0() { DebugLog.WriteDbgLog2Outlog("STUB AMD_init_2_0 (ryzen.dll, AMD models only)"); }
	public void InstallAMDDriver() { }
	public void AMDManualModeLoadDefault() { }
	public void SetAMD_PowerConsumption(AMD_Ryzen r) { DebugLog.WriteDbgLog2Outlog("STUB SetAMD_PowerConsumption PPT=" + r.PPT + " TDC=" + r.TDC + " EDC=" + r.EDC + " (ryzen.dll; guarded as OC write when implemented)"); }
	public void RestoreAMD_PowerConsumption() { DebugLog.WriteDbgLog2Outlog("STUB RestoreAMD_PowerConsumption"); }
	public void AMD_PowerConsumption_Thermal(AMD_Ryzen r) { SetAMD_PowerConsumption(r); }

	// ---------------------------------------------------------------- unchanged from the original (DCHU / AppSettings only)


	public bool SetPowerMode(uint mode)
	{
		int page = 1;
		int offset = 1;
		byte[] bytes = BitConverter.GetBytes(mode);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
		if (mode == 2)
		{
			uint num = 2u;
			page = 1;
			offset = 32;
			byte[] array = Global.dchu.ReadAppSettings(1, 32, 1);
			if (array[0] == 1)
			{
				if (checkDriverInstall.isInstall_DTT_Driver())
				{
					num |= 0x80;
				}
				else
				{
					array[0] = 0;
					Global.dchu.WriteAppSettings(1, 32, 1, array);
				}
			}
			byte[] array2 = new byte[1];
			array2 = Global.dchu.ReadAppSettings(4, 8, 1);
			if (array2[0] == 1)
			{
				if (Global.features.SupportDTT)
				{
					if (checkDriverInstall.isInstall_DTT_Driver())
					{
						num |= 0x40;
					}
					else
					{
						array2[0] = 0;
						Global.dchu.WriteAppSettings(4, 8, 1, array2);
						array[0] = 0;
						Global.dchu.WriteAppSettings(1, 32, 1, array);
						num = 2u;
					}
				}
				else
				{
					num |= 0x40;
				}
			}
			Global.dchu.SetWMI(121, 25, num);
		}
		else
		{
			Global.dchu.SetWMI(121, 25, mode);
		}
		return true;
	}


	public bool SetEnergyStarMode(uint mode)
	{
		Global.dchu.SetWMI(79, mode);
		DebugLog.WriteDbgLog2Outlog("SetEnergyStarMode: " + mode);
		return true;
	}


	public int GetPowerMode()
	{
		int page = 1;
		int offset = 1;
		int length = 1;
		try
		{
			return Global.dchu.ReadAppSettings(page, offset, length)[0];
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("AppSettings error");
		}
		return 0;
	}


	public int GetCCD()
	{
		int page = 1;
		int offset = 6;
		int length = 1;
		try
		{
			return Global.dchu.ReadAppSettings(page, offset, length)[0];
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("AppSettings error");
		}
		return 0;
	}


	public int SetCCD(int value)
	{
		DebugLog.WriteDbgLog2Outlog("SetCCD" + value);
		int page = 1;
		int offset = 6;
		byte[] bytes = BitConverter.GetBytes(value);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
		Global.dchu.SetWMI(34, (uint)value);
		return 1;
	}


	public void WriteCCDValue(int value)
	{
		DebugLog.WriteDbgLog2Outlog("WriteCCD" + value);
		int page = 1;
		int offset = 6;
		byte[] bytes = BitConverter.GetBytes(value);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
	}


	public int GetLedKB_APP_Launch()
	{
		int page = 2;
		int offset = 8;
		int length = 1;
		try
		{
			return Global.dchu.ReadAppSettings(page, offset, length)[0];
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("AppSettings error");
		}
		return 0;
	}


	public int SetLedKB_APP_Launch(int value)
	{
		int page = 2;
		int offset = 8;
		byte[] bytes = BitConverter.GetBytes(value);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
		return 1;
	}


	public int GetCC30_APP_Launch()
	{
		int page = 1;
		int offset = 7;
		int length = 1;
		try
		{
			return Global.dchu.ReadAppSettings(page, offset, length)[0];
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("AppSettings error");
		}
		return 0;
	}


	public int SetCC30_APP_Launch(int value)
	{
		int page = 1;
		int offset = 7;
		byte[] bytes = BitConverter.GetBytes(value);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
		return 1;
	}


	public void BatteryLowControl_Setting()
	{
		if (!SupportBATLowControl)
		{
			return;
		}
		Global.BAT_PowerStatus.UpdatePowerInfo();
		if (Global.BAT_PowerStatus.PowerLineStatus != PowerLineStatus.Online)
		{
			int page = 1;
			int offset = 13;
			byte[] array = Global.dchu.ReadAppSettings(page, offset, 2);
			byte b = array[0];
			byte b2 = (byte)Global.BAT_PowerStatus.BatteryLifePercent;
			byte b3 = (byte)Global.brightness.Read_Brightness();
			if (Global.brightness.Read_Brightness() > 0 && b != 0)
			{
				b = b3;
			}
			else if (b == 0)
			{
				b = b3;
				Global.brightness.Write_Brightness(0);
				DebugLog.WriteDbgLog2Outlog("battery low Set brightness=0");
			}
			array[0] = b;
			array[1] = b2;
			Global.dchu.WriteAppSettings(page, offset, 2, array);
		}
	}


	public void BatteryLowControl_Restore()
	{
		if (!SupportBATLowControl)
		{
			return;
		}
		int page = 1;
		int offset = 13;
		byte[] array = Global.dchu.ReadAppSettings(page, offset, 1);
		if (array[0] != 0)
		{
			byte b = (byte)Global.brightness.Read_Brightness();
			if (b == 0)
			{
				Global.brightness.Write_Brightness(array[0]);
				DebugLog.WriteDbgLog2Outlog("battery low Restore brightness=" + array[0]);
			}
			else
			{
				Global.brightness.Write_Brightness(b);
				DebugLog.WriteDbgLog2Outlog("battery low Restore brightness=" + b);
			}
			array[0] = 0;
			Global.dchu.WriteAppSettings(page, offset, 1, array);
		}
	}


	public void BatteryLowControl_Check()
	{
		if (!SupportBATLowControl)
		{
			return;
		}
		int page = 1;
		int offset = 13;
		byte[] array = Global.dchu.ReadAppSettings(page, offset, 2);
		byte b = array[0];
		byte b2 = array[1];
		if (b != 0)
		{
			Global.BAT_PowerStatus.UpdatePowerInfo();
			if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online || Global.BAT_PowerStatus.BatteryLifePercent > (float)(int)b2)
			{
				BatteryLowControl_Restore();
			}
		}
	}


	public void Load_SituationalMode(int mode)
	{
		uint data = 0u;
		byte[] bytes = BitConverter.GetBytes(mode);
		Global.dchu.WriteAppSettings(1, 0, 1, bytes);
		switch (mode)
		{
		case 0:
			data = 3u;
			break;
		case 1:
			data = 1u;
			break;
		case 2:
			data = 0u;
			break;
		case 3:
			data = 2u;
			break;
		}
		Global.dchu.SetWMI(121, 25, data);
	}


	public void SetAirplaneHotkey(int data)
	{
		Global.dchu.SetWMI(19, (uint)data);
	}


	public void GPUSwitch(int value)
	{
		Global.dchu.SetWMI(121, 11, (uint)value);
	}


	public void GPUSwitch_new(byte value)
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		_ = new byte[24];
		array[0] = 22;
		array[1] = value;
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
	}


	public void DoWMIFlag(int command, int WMI_Data)
	{
		switch (command)
		{
		case 18:
		case 19:
		case 20:
		case 21:
		case 22:
		case 23:
		case 24:
		case 25:
		case 26:
		case 27:
		case 28:
		case 29:
		case 30:
		case 31:
		case 32:
		case 33:
		case 34:
		case 35:
		case 36:
		case 37:
		case 38:
		case 39:
		case 40:
		case 41:
		case 42:
		case 43:
		case 44:
		case 45:
		case 46:
		case 47:
		case 48:
		case 49:
		case 50:
		case 51:
		case 52:
		case 53:
		case 54:
		case 55:
		case 56:
		case 57:
		case 58:
		case 59:
		case 61:
			_ = 61;
			break;
		case 70:
			if ((WMI_Data & 1) == 1)
			{
				CallingVariations.SetUSB(0);
				CallingVariations.ReloadActivePowerPlan();
			}
			if ((WMI_Data & 0x1000) == 4096)
			{
				Global.features.LoadDefault = true;
				DebugLog.WriteDbgLog2Outlog("LoadDefault = true");
			}
			else
			{
				Global.features.LoadDefault = false;
				DebugLog.WriteDbgLog2Outlog("LoadDefault = false");
			}
			if ((WMI_Data & 0x2000) == 8192)
			{
				try
				{
					Global.features.LoopBeeperStop();
					Global.features.PCbeep = 1;
					CallingVariations.pc_beep_Mute(p1: true);
				}
				catch
				{
				}
			}
			if ((WMI_Data & 0x20000) == 131072)
			{
				Global.features.OCLoadDefault = true;
			}
			break;
		case 82:
			Global.features.DefaultPowerConverationMode = WMI_Data >> 4;
			Global.features.DefaultPowerConverationMode = Global.features.DefaultPowerConverationMode & 3;
			if ((WMI_Data & 0x200000) == 2097152)
			{
				Global.features.EnergyStar15C = 1;
			}
			if ((WMI_Data & 0x8000000) == 134217728)
			{
				Global.features.BatteryChargeControl = 1;
			}
			break;
		case 122:
			if ((WMI_Data & 1) == 1)
			{
				Global.features.FanSpeedSupport = true;
			}
			else
			{
				Global.features.FanSpeedSupport = false;
			}
			if ((WMI_Data & 2) == 2)
			{
				Global.features.SupportGamingGroup = true;
			}
			else
			{
				Global.features.SupportGamingGroup = false;
			}
			if ((WMI_Data & 4) == 4)
			{
				Global.features.Support3dHeadphone = true;
			}
			else
			{
				Global.features.Support3dHeadphone = false;
			}
			if ((WMI_Data & 8) == 8)
			{
				Global.features.SupportTpColor = true;
			}
			else
			{
				Global.features.SupportTpColor = false;
			}
			if ((WMI_Data & 0x10) == 16)
			{
				Global.features.CustomFanSupport = true;
			}
			else
			{
				Global.features.CustomFanSupport = false;
			}
			if ((WMI_Data & 0x20) == 32)
			{
				Global.features.WakeUpLanSupport = true;
			}
			else
			{
				Global.features.WakeUpLanSupport = false;
			}
			if ((WMI_Data & 0x40) == 64)
			{
				Global.features.SupportCheckNotAirplaneOsd = true;
			}
			if ((WMI_Data & 0x100) == 256)
			{
				Global.features.SupportUsbHubReset = true;
			}
			else
			{
				Global.features.SupportUsbHubReset = false;
			}
			if ((WMI_Data & 0x200) == 512)
			{
				Global.features.SupportBootChangeCPUVoltage = true;
			}
			if ((WMI_Data & 0x800) == 2048)
			{
				Global.features.SupportTpColor = true;
			}
			else
			{
				Global.features.SupportTpColor = false;
			}
			if ((WMI_Data & 0x1000) == 4096)
			{
				Global.features.SupportLightbar = true;
			}
			else
			{
				Global.features.SupportLightbar = false;
			}
			if ((WMI_Data & 0x4000) == 16384)
			{
				Global.features.Support6WhKb = true;
			}
			else
			{
				Global.features.Support6WhKb = false;
			}
			if ((WMI_Data & 0x8000) == 32768)
			{
				Global.features.SupportFanLess = true;
			}
			else
			{
				Global.features.SupportFanLess = false;
			}
			if ((WMI_Data & 0x10000) == 65536)
			{
				Global.features.FanExists = false;
			}
			if ((WMI_Data & 0x20000) == 131072)
			{
				Global.features.SupportTpSmbios = true;
			}
			if ((WMI_Data & 0x40000) == 262144)
			{
				Global.features.SupportLogoColor = true;
			}
			if ((WMI_Data & 0x80000) == 524288)
			{
				Global.features.SupportWmi3dHp = true;
			}
			if ((WMI_Data & 0x100000) == 1048576)
			{
				Global.features.SupportMSHybrid_dGPUSwicth = true;
			}
			if ((WMI_Data & 0x200000) == 2097152)
			{
				Global.features.SupportEthernetLED = true;
			}
			if ((WMI_Data & 0x400000) == 4194304)
			{
				Global.features.SupportEss = true;
			}
			if ((WMI_Data & 0x800000) == 8388608)
			{
				Global.features.SupportCPUOC_WMI122Bit23 = true;
			}
			else
			{
				Global.features.SupportCPUOC_WMI122Bit23 = false;
			}
			if ((WMI_Data & 0x1000000) == 16777216)
			{
				Global.features.SupportXMP = true;
			}
			else
			{
				Global.features.SupportXMP = false;
			}
			if (((WMI_Data >> 26) & 1) == 1)
			{
				Global.features.SupportGPUOC_WMI = 1;
			}
			else
			{
				Global.features.SupportGPUOC_WMI = 0;
			}
			if (((WMI_Data >> 27) & 1) == 1)
			{
				Global.features.SupportGPUcustom_WMI = 1;
			}
			else
			{
				Global.features.SupportGPUcustom_WMI = 0;
			}
			if (((WMI_Data >> 28) & 1) == 1)
			{
				Global.features.SupportKBLEDsleep_WMI = 1;
			}
			else
			{
				Global.features.SupportKBLEDsleep_WMI = 0;
			}
			if ((WMI_Data & 0x40000000) == 1073741824)
			{
				Global.features.SupportHW_P_state = true;
			}
			else
			{
				Global.features.SupportHW_P_state = false;
			}
			break;
		case 96:
			if ((WMI_Data & 1) == 1)
			{
				Global.features.NoSupportHW_KB_Type = true;
			}
			else
			{
				Global.features.NoSupportHW_KB_Type = false;
			}
			if ((WMI_Data & 2) == 2)
			{
				Global.features.CheckUnigine = true;
			}
			else
			{
				Global.features.CheckUnigine = false;
			}
			if (((WMI_Data >> 2) & 1) == 1)
			{
				Global.features.SupportNewMode = true;
			}
			else
			{
				Global.features.SupportNewMode = false;
			}
			if (((WMI_Data >> 4) & 1) == 1)
			{
				Global.features.SupportSandTPScanCode = false;
			}
			else
			{
				Global.features.SupportSandTPScanCode = true;
			}
			if ((WMI_Data & 0x20) == 32)
			{
				Global.features.SupportAllModeGPUOC = true;
			}
			if (((WMI_Data >> 7) & 1) == 1)
			{
				Global.features.SupportAntiDust_Fan = true;
			}
			else
			{
				Global.features.SupportAntiDust_Fan = false;
			}
			if (((WMI_Data >> 8) & 1) == 1)
			{
				Global.features.SupportPerkeyDefaultStaticColor = true;
			}
			if (((WMI_Data >> 9) & 1) == 1)
			{
				Global.features.SupportDisablePCIE_ASPM = true;
			}
			else
			{
				Global.features.SupportDisablePCIE_ASPM = false;
			}
			if (((WMI_Data >> 10) & 1) == 1)
			{
				Global.features.SupportFanOffset = false;
			}
			else
			{
				Global.features.SupportFanOffset = true;
			}
			if (((WMI_Data >> 11) & 1) == 1)
			{
				Global.features.SupportLightBar_X170 = true;
			}
			else
			{
				Global.features.SupportLightBar_X170 = false;
			}
			if (((WMI_Data >> 12) & 1) == 1)
			{
				Global.features.SupportDTT = true;
			}
			else
			{
				Global.features.SupportDTT = false;
			}
			if (((WMI_Data >> 13) & 1) == 1)
			{
				Global.features.AMD_Manual_Mode = true;
			}
			else
			{
				Global.features.AMD_Manual_Mode = false;
			}
			break;
		case 60:
			if ((WMI_Data & 0x20) == 32)
			{
				Global.features.WakeUpLanState = 1;
			}
			else
			{
				Global.features.WakeUpLanState = 0;
			}
			Global.features.SetWakeUpOnLan();
			break;
		case 16:
		{
			SupportUWPAppsCount = 1;
			SupportUWPAppsflags = WMI_Data;
			if ((WMI_Data & 1) == 1)
			{
				Global.features.SupportPowerMode = true;
				SupportUWPAppsCount++;
			}
			else
			{
				Global.features.SupportPowerMode = false;
			}
			if ((WMI_Data & 2) == 2)
			{
				Global.features.SupportFlexikey = true;
				SupportUWPAppsCount++;
			}
			else
			{
				Global.features.SupportFlexikey = false;
			}
			if ((WMI_Data & 4) == 4)
			{
				Global.features.SupportFlexiAccess = true;
			}
			else
			{
				Global.features.SupportFlexiAccess = false;
			}
			if ((WMI_Data & 8) == 8 || (WMI_Data & 0x10) == 16)
			{
				SupportUWPAppsCount++;
			}
			if ((WMI_Data & 0x20) == 32)
			{
				Global.features.SupportGPUOC = true;
				SupportUWPAppsCount++;
			}
			else
			{
				Global.features.SupportGPUOC = false;
			}
			if ((WMI_Data & 0x40) == 64)
			{
				Global.features.SupportCPUOC_WMI16Bit6 = true;
				SupportUWPAppsCount++;
			}
			else
			{
				Global.features.SupportCPUOC_WMI16Bit6 = false;
			}
			if ((WMI_Data & 0x80) == 128)
			{
				Global.features.SupportFanSpeedSetting = true;
				SupportUWPAppsCount++;
			}
			else
			{
				Global.features.SupportFanSpeedSetting = false;
			}
			if ((WMI_Data & 0x100) == 256)
			{
				Global.features.SupportEnergySave = true;
				SupportUWPAppsCount++;
			}
			else
			{
				Global.features.SupportEnergySave = false;
			}
			if ((WMI_Data & 0x200) == 512)
			{
				Global.features.SupportBatteryUtility = true;
				SupportUWPAppsCount++;
			}
			else
			{
				Global.features.SupportBatteryUtility = false;
			}
			if (((WMI_Data >> 10) & 1) == 1)
			{
				Global.features.SupportDisable_PgUpDnKey = true;
			}
			else
			{
				Global.features.SupportDisable_PgUpDnKey = false;
			}
			byte[] bytes = BitConverter.GetBytes(WMI_Data);
			Global.dchu.WriteAppSettings(0, 252, bytes.Count(), bytes);
			break;
		}
		}
	}


	public void Get_Features_list()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		_ = new byte[24];
		array[0] = 8;
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
		SupportBIOSFeatureList_Version = (o_buffer[0] << 8) + o_buffer[1];
		DebugLog.WriteDbgLog2Outlog("SetWMI Package CMD=4 Sub-8 Version=" + o_buffer[0] + "." + o_buffer[1]);
		Global.dchu.WriteAppSettings(7, 0, 256, o_buffer);
		switch (SupportBIOSFeatureList_Version)
		{
		case 0:
			BIOSSpecialFeature_v0_0(o_buffer);
			break;
		case 256:
			BIOSSpecialFeature_v1_0(o_buffer);
			break;
		default:
			BIOSSpecialFeature_v0_0(o_buffer);
			break;
		}
	}


	private void BIOSSpecialFeature_v0_0(byte[] offset)
	{
		Global.features.DoWMIFlag(82, Global.dchu.GetWMI(82));
		Global.features.DoWMIFlag(122, Global.dchu.GetWMI(122));
		Global.features.DoWMIFlag(96, Global.dchu.GetWMI(96));
		Global.features.DoWMIFlag(60, Global.dchu.GetWMI(60));
		Global.features.DoWMIFlag(16, Global.dchu.GetWMI(16));
		if ((offset[13] & 1) == 1)
		{
			SupportLimitGPUClock = true;
		}
		if (((offset[13] >> 1) & 1) == 1)
		{
			SupportGC6Setting = true;
			DebugLog.WriteDbgLog2Inlog("SupportGC6Setting");
		}
		if ((offset[17] & 0xF) != 0)
		{
			SupportPowerModeBit = true;
			if ((offset[17] & 1) == 1)
			{
				SupportPowerModeBit_Quite = true;
			}
			if (((offset[17] >> 1) & 1) == 1)
			{
				SupportPowerModeBit_PowerSaving = true;
			}
			if (((offset[17] >> 2) & 1) == 1)
			{
				SupportPowerModeBit_Performance = true;
			}
			if (((offset[17] >> 3) & 1) == 1)
			{
				SupportPowerModeBit_Entertainment = true;
			}
		}
		SupportPowerModeUI_ID = offset[17] >> 4;
		if (((offset[19] >> 4) & 1) == 1)
		{
			SupportCheckRealtekLANSavePower = true;
		}
	}


	private void BIOSSpecialFeature_v1_0(byte[] offset)
	{
		int num = (offset[5] << 24) + (offset[4] << 16) + (offset[3] << 8) + offset[2];
		int num2 = (offset[9] << 24) + (offset[8] << 16) + (offset[7] << 8) + offset[6];
		int num3 = (offset[11] << 8) + offset[10];
		int num4 = (offset[14] << 8) + offset[13];
		DebugLog.WriteDbgLog2Outlog("WMI=4 Sub-8 Data\noffset2~5,WMI82=" + num + "\noffset6~9,WMI122=" + num2 + "\noffset10~11,WMI96=" + num3 + "\noffset12,WMI60=" + offset[12] + "\noffset13~14,WMI16=" + num4 + "\noffset15=" + offset[15] + "\noffset16=" + offset[16] + "     All data save to AppSettings 0x700");
		if (num + num2 + num3 + num4 == 0)
		{
			DoWMIFlag(82, Global.dchu.GetWMI(82));
			DoWMIFlag(122, Global.dchu.GetWMI(122));
			DoWMIFlag(96, Global.dchu.GetWMI(96));
			DoWMIFlag(60, Global.dchu.GetWMI(60));
			DoWMIFlag(16, Global.dchu.GetWMI(16));
		}
		else
		{
			DoWMIFlag(82, num);
			DoWMIFlag(122, num2);
			DoWMIFlag(96, num3);
			DoWMIFlag(60, offset[12]);
			DoWMIFlag(16, num4);
		}
		if ((offset[15] & 1) == 1)
		{
			SupportLimitGPUClock = true;
		}
		if (((offset[15] >> 1) & 1) == 1)
		{
			SupportGC6Setting = true;
		}
		if (((offset[15] >> 2) & 1) == 1)
		{
			SupportBatterySaverSetting = true;
		}
		if (((offset[15] >> 3) & 1) == 1)
		{
			DisableFnF2TouchPadHook = true;
		}
		if (((offset[15] >> 4) & 1) == 1)
		{
			SupportRescanCardReader = true;
		}
		if (((offset[15] >> 5) & 1) == 1)
		{
			DisableLAN_PowerSavingMode = true;
		}
		if (((offset[15] >> 6) & 1) == 1)
		{
			SupportDisplayTimeNever = true;
		}
		if (((offset[15] >> 7) & 1) == 1)
		{
			SupportRescanPS2Mouse = true;
		}
		if ((offset[16] & 1) == 1)
		{
			SupportMEPowerGatingDisable = true;
		}
		if (((offset[16] >> 1) & 1) == 1)
		{
			SupportPCI5Enable = true;
		}
		if (((offset[16] >> 2) & 1) == 1)
		{
			SupportSetDeviceSavePower = true;
		}
		if (((offset[16] >> 3) & 1) == 1)
		{
			ReadASPMControlStatus();
			SupportASPMControl = true;
		}
		if (((offset[16] >> 4) & 1) == 1)
		{
			SupportTurboFan = true;
		}
		if (((offset[16] >> 5) & 1) == 1)
		{
			SupportAMDCC30PowerMode = true;
		}
		if (((offset[16] >> 6) & 1) == 1)
		{
			SupportBATLowControl = true;
		}
		if ((offset[17] & 0xF) != 0)
		{
			SupportPowerModeBit = true;
			if ((offset[17] & 1) == 1)
			{
				SupportPowerModeBit_Quite = true;
			}
			if (((offset[17] >> 1) & 1) == 1)
			{
				SupportPowerModeBit_PowerSaving = true;
			}
			if (((offset[17] >> 2) & 1) == 1)
			{
				SupportPowerModeBit_Performance = true;
			}
			if (((offset[17] >> 3) & 1) == 1)
			{
				SupportPowerModeBit_Entertainment = true;
			}
		}
		SupportPowerModeUI_ID = offset[17] >> 4;
		if ((offset[18] & 1) == 1)
		{
			SupportMSHybrid_dGPU_iGPUSwicth = true;
		}
		if (((offset[18] >> 1) & 1) == 1)
		{
			SupportRemoveHDRLimit = true;
		}
		if (((offset[18] >> 2) & 1) == 1)
		{
			SupportFix165MHzPanelS3ResumeScreenFlicker = true;
		}
		if (((offset[18] >> 3) & 1) == 1)
		{
			SupportFixUCSI_DeviceYellowMarkWorkaround = true;
		}
		if (((offset[18] >> 5) & 1) == 1)
		{
			SupportExchangePanelRefreshRate = false;
		}
		if (((offset[18] >> 6) & 1) == 1)
		{
			SupportMovieTVnoise = true;
		}
		if (((offset[18] >> 7) & 1) == 1)
		{
			DefaultEnableExchangePanelRefreshRate = false;
		}
		if ((offset[19] & 1) == 1)
		{
			SupportMovieTVnoise_ASPM = true;
		}
		if (((offset[19] >> 1) & 1) == 1)
		{
			Read_PwerPlanTable();
			SupportSetPowerPlan = true;
		}
		if (((offset[19] >> 2) & 1) == 1)
		{
			SupportDTT_iSST_DeviceYellowMarkWorkaround = true;
		}
		if (((offset[19] >> 4) & 1) == 1)
		{
			SupportCheckRealtekLANSavePower = true;
		}
		if (((offset[19] >> 5) & 1) == 1)
		{
			SupportRebootRescanCardReader = true;
		}
	}


	public void ReadASPMControlStatus()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		_ = new byte[24];
		array[0] = 15;
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
		Global.PerformanceMode.ASPM_AC = o_buffer[1];
		Global.PerformanceMode.ASPM_DC = o_buffer[2];
		Global.EntertainmentMode.ASPM_AC = o_buffer[3];
		Global.EntertainmentMode.ASPM_DC = o_buffer[4];
		Global.QuietMode.ASPM_AC = o_buffer[5];
		Global.QuietMode.ASPM_DC = o_buffer[6];
		Global.PwrsavingeMode.ASPM_AC = o_buffer[7];
		Global.PwrsavingeMode.ASPM_DC = o_buffer[8];
		DebugLog.WriteDbgLog2Outlog("SupportASPMControl\nEntertainmentMode ACPM AC=" + Global.EntertainmentMode.ASPM_AC + " DC=" + Global.EntertainmentMode.ASPM_DC + "\nPerformanceMode ACPM AC=" + Global.PerformanceMode.ASPM_AC + " DC=" + Global.PerformanceMode.ASPM_DC + "\nPwrsavingeMode ACPM AC=" + Global.PwrsavingeMode.ASPM_AC + " DC=" + Global.PwrsavingeMode.ASPM_DC + "\nQuietMode ACPM AC=" + Global.QuietMode.ASPM_AC + " DC=" + Global.QuietMode.ASPM_DC);
	}


	private void Read_PwerPlanTable()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		_ = new byte[24];
		array[0] = 29;
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
		Global.PerformanceMode.PowerPlan = o_buffer[1];
		Global.EntertainmentMode.PowerPlan = o_buffer[2];
		Global.QuietMode.PowerPlan = o_buffer[3];
		Global.PwrsavingeMode.PowerPlan = o_buffer[4];
		DebugLog.WriteDbgLog2Outlog("SupportSetPowerPlan\nEntertainmentMode PowerPlan=" + Global.EntertainmentMode.PowerPlan + "\nPerformanceMode PowerPlan=" + Global.PerformanceMode.PowerPlan + "\nPwrsavingeMode PowerPlan=" + Global.PwrsavingeMode.PowerPlan + "\nQuietMode PowerPlan=" + Global.QuietMode.PowerPlan);
	}


	public void Set_PowerPlan(int mode)
	{
		switch (mode)
		{
		case 0:
			CallingVariations.SetActivePowerPlanGUID(guidBalance);
			DebugLog.WriteDbgLog2Outlog("SetPowerPlan = Balance");
			break;
		case 1:
			CallingVariations.SetActivePowerPlanGUID(guidHighPerformance);
			DebugLog.WriteDbgLog2Outlog("SetPowerPlan = HighPerformance");
			break;
		case 2:
			CallingVariations.SetActivePowerPlanGUID(guidPowerSaver);
			DebugLog.WriteDbgLog2Outlog("SetPowerPlan = PowerSaver");
			break;
		}
	}


	public byte GetBatterySaver_Status()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		array[0] = 13;
		array[1] = 0;
		array[2] = 0;
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
		byte result = o_buffer[0];
		DebugLog.WriteDbgLog2Outlog("GetBatterySaverStatus=" + result);
		return result;
	}


	public void SetBatterySaver_Status(byte status)
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		array[0] = 13;
		array[1] = 1;
		array[2] = status;
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
		DebugLog.WriteDbgLog2Outlog("SetBatterySaverStatus=" + status);
	}


	public void SetUILanguage(byte value)
	{
		int page = 1;
		int offset = 2;
		byte[] bytes = BitConverter.GetBytes((short)value);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
	}


	public byte GetUILanguage()
	{
		int page = 1;
		int offset = 2;
		int length = 1;
		try
		{
			byte[] array = Global.dchu.ReadAppSettings(page, offset, length);
			if (array[0] == 0)
			{
				array[0] = 1;
				Global.features.SetUILanguage(array[0]);
			}
			return array[0];
		}
		catch
		{
			Global.features.SetUILanguage(1);
		}
		return 1;
	}


	public void SetShowOSD(byte value)
	{
		int page = 1;
		int offset = 3;
		byte[] bytes = BitConverter.GetBytes((short)value);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
	}


	public byte GetShowOSD()
	{
		int page = 1;
		int offset = 3;
		int length = 1;
		try
		{
			return Global.dchu.ReadAppSettings(page, offset, length)[0];
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("AppSettings error");
		}
		return 0;
	}


	public void SetUILanguageAutoDetect(byte value)
	{
		int page = 1;
		int offset = 4;
		byte[] bytes = BitConverter.GetBytes((short)value);
		int length = 1;
		Global.dchu.WriteAppSettings(page, offset, length, bytes);
	}


	public byte GetUILanguageAutoDetect()
	{
		int page = 1;
		int offset = 4;
		int length = 1;
		try
		{
			return Global.dchu.ReadAppSettings(page, offset, length)[0];
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("AppSettings error");
		}
		return 0;
	}


	public void GetCustomerID()
	{
		byte[] array = new byte[256];
		array = Global.dchu.GetWMIPackage(17);
		Global.features.CustomerID = array[2];
		DebugLog.WriteDbgLog2Outlog("GetCustomerID=" + array[2]);
	}


	public void GetWMI18Package()
	{
		byte[] array = new byte[256];
		array = Global.dchu.GetWMIPackage(18);
		if (array[208] != 0)
		{
			Global.features.CPUCore1WithDefaultValue = true;
		}
		else
		{
			Global.features.CPUCore1WithDefaultValue = false;
		}
		DebugLog.WriteDbgLog2Outlog("_Data_WMI18[0xD0]=" + array[208] + ", CPUCore1WithDefaultValue=" + Global.features.CPUCore1WithDefaultValue);
	}


	public byte GetDefaultPowerMode()
	{
		_ = new byte[256];
		return Global.dchu.GetWMIPackage(17)[13];
	}


	public void GetDCModePowerMode()
	{
		byte[] array = new byte[256];
		array = Global.dchu.GetWMIPackage(17);
		Global.features.DCMode_PowerMode = (array[13] >> 7) & 1;
		DebugLog.WriteDbgLog2Outlog("DCPowerMode=" + DCMode_PowerMode);
	}


	public string TranslationLanguage(string Text)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = "null";
		int bufferSize = 255;
		string filePath = (filePath = Path.Combine(AppContext.BaseDirectory, "mtlanguage.ini"));
		Ini.GetPrivateProfileString((CleDef.UI_Language)Global.features.GetUILanguage() switch
		{
			CleDef.UI_Language.system => CultureInfo.CurrentCulture.Name.ToLower(), 
			CleDef.UI_Language.en_us => "en-us", 
			CleDef.UI_Language.en_gb => "en-us", 
			CleDef.UI_Language.es => "es", 
			CleDef.UI_Language.de => "de", 
			CleDef.UI_Language.it => "it", 
			CleDef.UI_Language.ja => "ja", 
			CleDef.UI_Language.ko => "ko", 
			CleDef.UI_Language.zh_cn => "zh-cn", 
			CleDef.UI_Language.zh_tw => "zh-tw", 
			CleDef.UI_Language.fr_fr => "fr-fr", 
			CleDef.UI_Language.fr_ca => "fr-ca", 
			CleDef.UI_Language.pt_br => "pt-br", 
			CleDef.UI_Language.pt_pt => "pt-pt", 
			CleDef.UI_Language.tr_tr => "tr-tr", 
			CleDef.UI_Language.th_th => "th-th", 
			CleDef.UI_Language.vi_vn => "vi-vn", 
			CleDef.UI_Language.ru => "ru", 
			CleDef.UI_Language.ja_ep => "ja-ep", 
			_ => "en-us", 
		}, Text, def, stringBuilder, bufferSize, filePath);
		return stringBuilder.ToString();
	}


	public bool CheckI2C_TP()
	{
		DeviceControl deviceControl = new DeviceControl();
		SupportI2C_TP = deviceControl.FoundI2CHID();
		if (SupportI2C_TP)
		{
			if (!SupportSandTPScanCode)
			{
				Global.dchu.SetH2RAM(219u, 4u, 4u);
				DebugLog.WriteDbgLog2Outlog("EC Control I2C T/P");
				return false;
			}
			DebugLog.WriteDbgLog2Outlog("CC30 Control I2C T/P");
			return SupportI2C_TP;
		}
		return false;
	}


	public bool Check_Arg001_byDCHUServiceSend()
	{
		int page = 1;
		int offset = 12;
		if (Global.dchu.ReadAppSettings(page, offset, 1)[0] == 1)
		{
			Arg001ByDCHUServiceSend = true;
			DebugLog.WriteDbgLog2Outlog("Arg001Event by DCHUService Send");
		}
		else
		{
			/* Windows: starts the ACPI WAIT_EVENT thread (Arg001Event). Linux: events come from IEventSource. */
			Arg001ByDCHUServiceSend = false;
			DebugLog.WriteDbgLog2Outlog("Arg001Event by FnKey Send");
		}
		return Arg001ByDCHUServiceSend;
	}


	public void Log_Battery_PowerStatus()
	{
		DebugLog.WriteDbgLog2Outlog("Log_Battery_PowerStatus\r\n, BatteryLifePercent = " + Global.BAT_PowerStatus.BatteryLifePercent + "%\r\n, PowerLineStatus = " + Global.BAT_PowerStatus.PowerLineStatus.ToString() + "\r\n, OLD_PowerLineStatus = " + Global.BAT_PowerStatus.OLD_PowerLineStatus.ToString() + "\r\n, DCMode_PowerMode = " + Global.features.DCMode_PowerMode);
	}


	public int GetAMDManualMode()
	{
		byte[] array = new byte[256];
		array[0] = 7;
		array[1] = 0;
		byte[] o_buffer = new byte[256];
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
		Mode_From_BIOS = o_buffer[2];
		Frequency_From_BIOS = AMDConverTable(AMDConvert.IndexToFrequency, o_buffer[3]);
		Voltage_From_BIOS = (float)AMDConverTable(AMDConvert.IndexToVoltage, o_buffer[4]) / 1000f;
		return Mode_From_BIOS;
	}


	public static void SetIsSavedFlag(int v)
	{
		byte[] bytes = BitConverter.GetBytes(v);
		DCHU.WriteAppSettings(6, 32, 1, ref bytes[0]);
	}


	public static int GetIsSavedFlag()
	{
		byte[] array = new byte[4];
		DCHU.ReadAppSettings(6, 32, 1, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

}
