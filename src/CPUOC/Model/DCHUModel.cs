using System;
using CPUOC.Model;
using RippleCC.Platform;

namespace CPUOC30.Model;

internal class DCHUModel
{
	public enum Customer
	{
		Standard,
		Tronic5,
		MCJ,
		Seager,
		Gigabyte,
		EPSON,
		Santech
	}

	// Original read these from Insyde page 6 (written once a second by CC30_BG.exe from XTU monitors).
	// Here they are read directly from Linux sources (read-only), see CPUOC30.LinuxSensors.
	public int GetCPUFrequency()
	{
		return CPUOC30.LinuxSensors.CpuMHz();
	}

	public string GetCPUVoltage()
	{
		return CPUOC30.LinuxSensors.CpuVoltage();
	}

	/// <summary>BG did CalCPUTemp("0", EC package 0x0C byte 18) = identity; fall back to hwmon coretemp when the EC read is empty.</summary>
	public int GetCPUTemperature()
	{
		byte[] array = DCHU.GetWMIPackage(12);
		int t = array[18];
		if (t <= 0) t = CPUOC30.LinuxSensors.CpuTempHwmon();
		return t < 0 ? 0 : t;
	}

	public int GetCPUUtilization()
	{
		return CPUOC30.LinuxSensors.CpuUsage();
	}

	public int GetMemoryFrequency()
	{
		return CPUOC30.LinuxSensors.MemoryMHz();
	}

	static bool Forced => Environment.GetEnvironmentVariable("RIPPLE_CC_CPUOC_FORCE") == "1";

	public bool IsSupport_CPUOC_WMI16()
	{
		if (Forced) return true;
		int data = 0;
		DCHU.GetDCHU_Data(16, ref data);
		byte[] bytes = BitConverter.GetBytes(data);
		if ((bytes[0] & 0x40) == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsSupport_CPUOC_Wmi122Bit23()
	{
		if (Forced) return true;
		int data = 0;
		DCHU.GetDCHU_Data(122, ref data);
		if ((data & 0x800000) == 0)
		{
			return false;
		}
		return true;
	}

	public bool IsControlCenterControlCPU_UnderVolting()
	{
		int data = 0;
		DCHU.GetDCHU_Data(96, ref data);
		if ((data & 0x40) == 64)
		{
			return false;
		}
		return true;
	}

	public byte GetCustomId()
	{
		byte[] array = new byte[256];
		array = DCHU.GetWMIPackage(17);
		return array[2];
	}

	public byte GetUILanguage()
	{
		int page = 1;
		int offset = 2;
		int length = 1;
		try
		{
			byte[] array = DCHU.ReadAppSettings(page, offset, length);
			if (array[0] == 6 && GetCustomId() == 5)
			{
				return 18;
			}
			return array[0];
		}
		catch (Exception ex)
		{
			DebugLog.WriteDbgLog2Outlog("Get UILanguage Error, Message=" + ex.Message);
		}
		return 0;
	}

	public int GetFullScreen()
	{
		return DCHU.GetFullScreen();
	}

	public void SetFullScreen(int fullScreen)
	{
		DCHU.SetFullScreen(fullScreen);
	}

	public int GetPowerMode()
	{
		int num = 1;
		byte[] array = new byte[num];
		int offset = 1;
		int page = 1;
		DCHU.ReadAppSettings(page, offset, num, ref array[0]);
		return array[0];
	}

	public void SetIsSaved(int isSaved)
	{
		byte[] bytes = BitConverter.GetBytes(isSaved);
		DCHU.WriteAppSettings(6, 32, 1, ref bytes[0]); // OC settings flag: gated inside DCHU.WriteAppSettings
	}

	public int GetIsSaved()
	{
		byte[] array = new byte[4];
		DCHU.ReadAppSettings(6, 32, 1, ref array[0]);
		int result = BitConverter.ToInt32(array, 0);
		DebugLog.WriteDbgLog2Outlog("IsSaved= " + result);
		return result;
	}

	public void SetPowerLimit1CurrentValue(int pl1)
	{
		DCHU.SetPowerLimit1CurrentValue(pl1);
	}

	public void SetPowerLimitTimeCurrentValue(float plTime)
	{
		DCHU.SetPowerLimitTimeCurrentValue(plTime);
	}

	public void SetPowerLimit2CurrentValue(int pl2)
	{
		DCHU.SetPowerLimit2CurrentValue(pl2);
	}

	public void SetCPUVR(int vr)
	{
		DCHU.SetCPUVR(vr);
	}

	public void SetCPUVRAuto(bool auto)
	{
		if (auto)
		{
			DCHU.SetCPUVRAuto(1);
		}
		else
		{
			DCHU.SetCPUVRAuto(0);
		}
	}

	public void SetCoreVoltage(int offset)
	{
		DCHU.SetCoreVoltage(offset);
	}

	public void SetOverrideVoltage(float overRide)
	{
		DCHU.SetOverrideVoltage(overRide);
	}

	public void SetGraphicsVoltage(int gt)
	{
		if (GetCustomId() == 1)
		{
			DCHU.SetGraphicsVoltage(gt);
		}
	}

	public void SetCoreRatio(int id, int ratio)
	{
		DCHU.SetCoreRatio(id, ratio);
	}
}
