using System;
using Avalonia.Threading;

namespace FnKey.Device;

public class GPU
{
	public string GPUName = "";

	public string GPUName2 = "";

	public string GetNV_Driver_version = "";

	public int GPUOCMax;

	public int GPUOCMin;

	public int MEMOCMax;

	public int MEMOCMin;

	public int GPUOCRange;

	public int MemoryOCRange;

	public int NVdeviceID;

	public int GPUOCMax2;

	public int GPUOCMin2;

	public int MEMOCMax2;

	public int MEMOCMin2;

	public int GPUOCRange2;

	public int MemoryOCRange2;

	public int NVdeviceID2;

	public int GPU_TotalNumber;

	public int SupportGPUOC_WMI;

	public int SupportGPUcustom_WMI;

	public int NVBaseClock = 1400;

	public int NVIDAI_Lock_Frequency;

	public int NVIDAI_Lock_Frequency2;

	private DispatcherTimer CheckNVIDIA_Ready;

	public int Check_NVReady_Count = 10;

	public int Default_CoreOC;

	public int Default_VRAMOC;

	public bool NVReady;

	public bool FirstRun;

	public int limitClock_entertainment;

	public int limitClock_performance;

	public int limitClock_powerSaving;

	public int limitClock_quiet;

	public int limitClock_temp;

	public int NV_Driver_Version;

	public void Init_GPU()
	{
		byte[] buffer = new byte[1];
		Global.dchu.WriteAppSettings(5, 9, 1, buffer);
		if (Global.features.SupportGPUcustom_WMI == 1 || Global.features.SupportGPUOC || Global.fan.FanCount > 1)
		{
			CheckNVIDIA_Ready = new DispatcherTimer();
			CheckNVIDIA_Ready.Tick += CheckNVIDIA_Ready_Tick;
			CheckNVIDIA_Ready.Interval = System.TimeSpan.FromMilliseconds(100);
			Notify.ToService("w_arg255^ControlGPU.exe^info"); // NVIDIA clock/offset helper: not ported (any future implementation must use HardwareGate.GuardOcWrite)
			CheckNVIDIA_Ready.Start();
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("NotSupportGPUOC");
		}
	}

	private void CheckNVIDIA_Ready_Tick(object sender, EventArgs e)
	{
		CheckNVIDIA_Ready.Interval = System.TimeSpan.FromMilliseconds(5000);
		CheckNVIDIA_Ready.Stop();
		Check_NVReady_Count--;
		if (Check_NVReady_Count >= 0 && GetGPU_Info() != 0)
		{
			CheckNVIDIA_Ready.Start();
		}
	}

	public int GetGPU_Info()
	{
		try
		{
			byte[] array = new byte[2];
			array = Global.dchu.ReadAppSettings(5, 0, 2);
			NVdeviceID = (array[1] << 8) + array[0];
			byte[] array2 = new byte[4];
			array2 = Global.dchu.ReadAppSettings(5, 6, 7);
			GPU_TotalNumber = array2[0];
			NVBaseClock = (array2[2] << 8) + array2[1];
			NVReady = Convert.ToBoolean(array2[3]);
			NV_Driver_Version = (array2[6] << 16) + (array2[5] << 8) + array2[4];
			workaround_GC6();
			if (!NVReady)
			{
				Notify.ToService("w_arg255^ControlGPU.exe^info"); // NVIDIA clock/offset helper: not ported (any future implementation must use HardwareGate.GuardOcWrite)
				return 9999;
			}
			LimitNVGPU_Clock();
			if (Global.features.SupportGPUcustom_WMI == 1)
			{
				Read_Custom_offset();
			}
			else
			{
				byte[] buffer = new byte[256];
				Global.dchu.WriteAppSettings(5, 0, 6, buffer);
			}
			if (Global.features.OCLoadDefault || FirstRun || Global.features.LoadDefault || (Global.features.SupportGPUcustom_WMI == 1 && !Global.features.SupportGPUOC))
			{
				byte[] array3 = new byte[8];
				byte[] array4 = new byte[4];
				array4 = Global.dchu.ReadAppSettings(5, 2, 4);
				array3[0] = array4[0];
				array3[1] = array4[1];
				array3[2] = array4[2];
				array3[3] = array4[3];
				array3[4] = array4[0];
				array3[5] = array4[1];
				array3[6] = array4[2];
				array3[7] = array4[3];
				Global.dchu.WriteAppSettings(5, 16, 8, array3);
				DebugLog.WriteDbgLog2Outlog("GPU Load Default");
				FirstRun = false;
			}
			byte[] array5 = Global.dchu.ReadAppSettings(1, 1, 1);
			NVReady = true;
			Load_GPU_forFn_3(array5[0]);
			return 0;
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("GPU Init error");
			return 8888;
		}
	}

	public void Load_GPU_forFn_3(int SituationalMode)
	{
		if (!NVReady)
		{
			return;
		}
		switch (SituationalMode)
		{
		case 0:
			if (Global.features.SupportLimitGPUClock)
			{
				Set_NVIDIA_Lock_Frequency(limitClock_quiet);
			}
			else
			{
				Set_NVIDIA_Lock_Frequency(0);
			}
			if (Global.features.SupportAllModeGPUOC)
			{
				Load_OC();
			}
			else
			{
				Load_OC_OFF();
			}
			break;
		case 1:
			if (Global.features.SupportLimitGPUClock && limitClock_powerSaving != 0)
			{
				Set_NVIDIA_Lock_Frequency(limitClock_powerSaving);
			}
			else
			{
				Set_NVIDIA_Lock_Frequency(NVBaseClock);
			}
			if (Global.features.SupportAllModeGPUOC)
			{
				Load_OC();
			}
			else
			{
				Load_OC_OFF();
			}
			break;
		case 2:
			Global.BAT_PowerStatus.UpdatePowerInfo();
			if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online)
			{
				if (Global.features.SupportLimitGPUClock)
				{
					Set_NVIDIA_Lock_Frequency(limitClock_performance);
				}
				else
				{
					Set_NVIDIA_Lock_Frequency(0);
				}
				Load_OC();
			}
			else
			{
				Load_OC_OFF();
			}
			break;
		case 3:
			if (Global.features.SupportLimitGPUClock)
			{
				Set_NVIDIA_Lock_Frequency(limitClock_entertainment);
			}
			else
			{
				Set_NVIDIA_Lock_Frequency(0);
			}
			if (Global.features.SupportAllModeGPUOC)
			{
				Load_OC();
			}
			else
			{
				Load_OC_OFF();
			}
			break;
		}
	}

	public void GPU_Turbo_off()
	{
		if (limitClock_temp == 0)
		{
			Set_NVIDIA_Lock_Frequency(NVBaseClock);
		}
		else
		{
			Set_NVIDIA_Lock_Frequency(limitClock_temp);
		}
	}

	public void Load_OC()
	{
		int num = 0;
		int num2 = 0;
		byte[] array = Global.dchu.ReadAppSettings(5, 16, 8);
		num = (((array[1] & 0x80) != 128) ? (array[0] + (array[1] << 8)) : (array[0] + (array[1] << 8) - 65535));
		num2 = (((array[3] & 0x80) != 128) ? (array[2] + (array[3] << 8)) : (array[2] + (array[3] << 8) - 65535));
		Set_Increment(0, num, num2);
		if (GPU_TotalNumber >= 2)
		{
			int num3 = 0;
			int num4 = 0;
			num3 = (((array[5] & 0x80) != 128) ? (array[4] + (array[5] << 8)) : (array[4] + (array[5] << 8) - 65535));
			num4 = (((array[7] & 0x80) != 128) ? (array[6] + (array[7] << 8)) : (array[6] + (array[7] << 8) - 65535));
			Set_Increment(1, num3, num4);
		}
	}

	public void Load_OC_OFF()
	{
		if (NVReady)
		{
			Set_Increment(0, 0, 0);
			if (GPU_TotalNumber >= 2)
			{
				Set_Increment(1, 0, 0);
			}
		}
	}

	private void Load_OC_Default()
	{
		Set_Increment(0, Default_CoreOC, Default_VRAMOC);
		if (GPU_TotalNumber >= 2)
		{
			Set_Increment(1, Default_CoreOC, Default_VRAMOC);
		}
	}

	public void Set_NVIDIA_Lock_Frequency(int Freq)
	{
		Notify.ToService("w_arg255^ControlGPU.exe^set_lock_frequency:" + Freq); // NVIDIA clock/offset helper: not ported (any future implementation must use HardwareGate.GuardOcWrite)
	}

	public void Set_Increment(int index, int CORE_Freq, int MEM_Freq)
	{
		Notify.ToService("w_arg255^ControlGPU.exe^set_increment:" + index + ":" + CORE_Freq + ":" + MEM_Freq); // NVIDIA clock/offset helper: not ported (any future implementation must use HardwareGate.GuardOcWrite)
	}

	public void Read_Custom_offset()
	{
		byte[] array = new byte[256];
		array = Global.dchu.GetWMIPackage(17);
		int num = array[48] + (array[49] << 8);
		int num2 = array[50] + (array[51] << 8);
		int num3 = array[64] + (array[65] << 8);
		int num4 = array[66] + (array[67] << 8);
		int num5 = array[80] + (array[81] << 8);
		int num6 = array[82] + (array[83] << 8);
		int num7 = array[96] + (array[97] << 8);
		int num8 = array[98] + (array[99] << 8);
		if (NVdeviceID == num || NVdeviceID == num2)
		{
			if ((array[35] & 0x80) == 128)
			{
				Default_CoreOC = array[52] + (array[53] << 8) - 65535;
			}
			else
			{
				Default_CoreOC = array[52] + (array[53] << 8);
			}
			if ((array[37] & 0x80) == 128)
			{
				Default_VRAMOC = array[54] + (array[55] << 8) - 65535;
			}
			else
			{
				Default_VRAMOC = array[54] + (array[55] << 8);
			}
		}
		else if (NVdeviceID == num3 || NVdeviceID == num4)
		{
			if ((array[45] & 0x80) == 128)
			{
				Default_CoreOC = array[68] + (array[69] << 8) - 65535;
			}
			else
			{
				Default_CoreOC = array[68] + (array[69] << 8);
			}
			if ((array[47] & 0x80) == 128)
			{
				Default_VRAMOC = array[70] + (array[71] << 8) - 65535;
			}
			else
			{
				Default_VRAMOC = array[70] + (array[71] << 8);
			}
		}
		else if (NVdeviceID == num5 || NVdeviceID == num6)
		{
			if ((array[55] & 0x80) == 128)
			{
				Default_CoreOC = array[84] + (array[85] << 8) - 65535;
			}
			else
			{
				Default_CoreOC = array[84] + (array[85] << 8);
			}
			if ((array[57] & 0x80) == 128)
			{
				Default_VRAMOC = array[86] + (array[87] << 8) - 65535;
			}
			else
			{
				Default_VRAMOC = array[86] + (array[87] << 8);
			}
		}
		else if (NVdeviceID == num7 || NVdeviceID == num8)
		{
			if ((array[65] & 0x80) == 128)
			{
				Default_CoreOC = array[100] + (array[101] << 8) - 65535;
			}
			else
			{
				Default_CoreOC = array[100] + (array[101] << 8);
			}
			if ((array[67] & 0x80) == 128)
			{
				Default_VRAMOC = array[102] + (array[103] << 8) - 65535;
			}
			else
			{
				Default_VRAMOC = array[102] + (array[103] << 8);
			}
		}
		else
		{
			Default_CoreOC = 0;
			Default_VRAMOC = 0;
		}
		byte[] array2 = new byte[6]
		{
			(byte)(NVdeviceID & 0xFF),
			(byte)((NVdeviceID >> 8) & 0xFF),
			0,
			0,
			0,
			0
		};
		if (Default_CoreOC < 0)
		{
			array2[2] = (byte)((Default_CoreOC - 1) & 0xFF);
			array2[3] = (byte)(Default_CoreOC - 1 >> 8);
		}
		else
		{
			array2[2] = (byte)(Default_CoreOC & 0xFF);
			array2[3] = (byte)(Default_CoreOC >> 8);
		}
		if (Default_VRAMOC < 0)
		{
			array2[4] = (byte)((Default_VRAMOC - 1) & 0xFF);
			array2[5] = (byte)(Default_VRAMOC - 1 >> 8);
		}
		else
		{
			array2[4] = (byte)(Default_VRAMOC & 0xFF);
			array2[5] = (byte)(Default_VRAMOC >> 8);
		}
		Global.dchu.WriteAppSettings(5, 0, 6, array2);
		DebugLog.WriteDbgLog2Outlog("Support GPU DefaultOC\nNVID = 0x" + NVdeviceID.ToString("X") + "\nDefault_CoreOC = " + Default_CoreOC + "\nDefault_VRAMOC = " + Default_VRAMOC + "\nRead WMI17 GPU Offset--------------------------------------------\nNVIDPrimary1 =" + array[49].ToString("X") + array[48].ToString("X") + "\nNVIDSecond1 =" + array[51].ToString("X") + array[50].ToString("X") + "\nCoreOC =" + array[53].ToString("X") + array[52].ToString("X") + "\nVRAMOC =" + array[55].ToString("X") + array[54].ToString("X") + "\n\nNVIDPrimary2 =" + array[65].ToString("X") + array[64].ToString("X") + "\nNVIDSecond2 =" + array[67].ToString("X") + array[66].ToString("X") + "\nCoreOC =" + array[69].ToString("X") + array[68].ToString("X") + "\nVRAMOC =" + array[71].ToString("X") + array[70].ToString("X") + "\n\nNVIDPrimary3 =" + array[81].ToString("X") + array[80].ToString("X") + "\nNVIDSecond3 =" + array[83].ToString("X") + array[82].ToString("X") + "\nCoreOC =" + array[85].ToString("X") + array[84].ToString("X") + "\nVRAMOC =" + array[87].ToString("X") + array[86].ToString("X") + "\n\nNVIDPrimary4 =" + array[97].ToString("X") + array[96].ToString("X") + "\nNVIDSecond4 =" + array[99].ToString("X") + array[98].ToString("X") + "\nCoreOC =" + array[101].ToString("X") + array[100].ToString("X") + "\nVRAMOC =" + array[103].ToString("X") + array[102].ToString("X"));
	}

	private void LimitNVGPU_Clock()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		array[0] = 9;
		Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
		if (Global.features.SupportLimitGPUClock)
		{
			limitClock_entertainment = o_buffer[2] + (o_buffer[3] << 8);
			limitClock_performance = o_buffer[4] + (o_buffer[5] << 8);
			limitClock_powerSaving = o_buffer[6] + (o_buffer[7] << 8);
			limitClock_quiet = o_buffer[8] + (o_buffer[9] << 8);
			DebugLog.WriteDbgLog2Outlog("Support limit NV GPU clock\nlimitClock_entertainment = " + limitClock_entertainment + "\nlimitClock_performance = " + limitClock_performance + "\nlimitClock_powerSaving = " + limitClock_powerSaving + "\nlimitClock_quiet =" + limitClock_quiet);
		}
		limitClock_temp = o_buffer[10] + (o_buffer[11] << 8);
	}

	private void workaround_GC6()
	{
		if (Global.features.SupportGC6Setting)
		{
			byte[] array = new byte[256];
			byte[] o_buffer = new byte[256];
			_ = new byte[24];
			array[0] = 10;
			Global.dchu.SetWMIPackageEx(4, array, ref o_buffer);
			Notify.ToService("w_arg255^ControlGPU.exe^GC6:" + o_buffer[1]); // NVIDIA clock/offset helper: not ported (any future implementation must use HardwareGate.GuardOcWrite)
		}
	}
}
