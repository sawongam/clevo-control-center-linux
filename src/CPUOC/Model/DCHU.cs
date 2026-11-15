using System;
using System.Runtime.InteropServices;
using CCenter.Platform;

namespace CPUOC.Model;

internal class DCHU
{
	private static int GetDCHU_Data_Integer(int command, ref int data) => InsydeDCHU.GetDCHU_Data_Integer(command, ref data);
	public static int SetDCHU_Data(int command, byte[] buffer, int length) => InsydeDCHU.SetDCHU_Data(command, buffer, length);
	public static int GetDCHU_Data_Buffer(int command, ref byte buffer) => InsydeDCHU.GetDCHU_Data_Buffer(command, ref buffer);
	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);
	/// <summary>Raw page write. Page 6 offsets 32..84 hold the OC settings (PL1/PL2/VR/voltage/ratios): every such write is an OC write and is gated.</summary>
	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer)
	{
		bool oc = page == 6 && offset >= 32 && offset <= 84;
		if (!oc) return InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);
		int r = 0; int len = length; var copy = System.Runtime.InteropServices.MemoryMarshal.CreateSpan(ref buffer, len).ToArray();
		HardwareGate.GuardOcWrite(() => r = InsydeDCHU.WriteAppSettings(page, offset, len, ref copy[0]));
		return r;
	}

	public static int SetWMI(int command, uint data)
	{
		byte[] array = new byte[256];
		try
		{
			array = BitConverter.GetBytes(data);
			SetDCHU_Data(command, array, array.Length);
		}
		catch
		{
			return 0;
		}
		return 1;
	}

	public static int GetDCHU_Data(int command, ref int data)
	{
		return GetDCHU_Data_Integer(Convert.ToInt32(command), ref data);
	}

	public static int SetDCHU_Data(string strData)
	{
		int command = Convert.ToInt32(strData);
		byte[] array = new byte[256];
		array[0] = Convert.ToByte(strData);
		return SetDCHU_Data(command, array, strData.Length);
	}

	public static int GetDCHU_Data(int command, ref byte buffer)
	{
		return GetDCHU_Data_Buffer(command, ref buffer);
	}

	public static int WriteAppSettings(int page, int offset, int length, byte[] buffer)
	{
		int result;
		try
		{
			result = WriteAppSettings(page, offset, length, ref buffer[0]);
		}
		catch
		{
			return 0;
		}
		return result;
	}

	public static byte[] ReadAppSettings(int page, int offset, int length)
	{
		byte[] array = new byte[length];
		try
		{
			int num = ReadAppSettings(page, offset, length, ref array[0]);
		}
		catch
		{
		}
		return array;
	}

	public static int GetPowerLimit1CurrentValue()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 33, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static float GetPowerLimitTimeValue()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 41, 4, ref array[0]);
		return BitConverter.ToSingle(array, 0);
	}

	public static int GetPowerLimit2CurrentValue()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 37, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static int GetCPUVRAuto()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 74, 1, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static int GetCPUVRCurrentValue()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 49, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static int GetCPUVoltageOffset()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 61, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static int GetGraphicsVoltage()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 69, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static float GetOverrideVoltageValue()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 65, 4, ref array[0]);
		return BitConverter.ToSingle(array, 0);
	}

	public static int GetCPUCoreRatioCurrentValue(int id)
	{
		byte[] array = new byte[4];
		switch (id)
		{
		case 29:
			ReadAppSettings(6, 53, 1, ref array[0]);
			break;
		case 30:
			ReadAppSettings(6, 54, 1, ref array[0]);
			break;
		case 31:
			ReadAppSettings(6, 55, 1, ref array[0]);
			break;
		case 32:
			ReadAppSettings(6, 56, 1, ref array[0]);
			break;
		case 42:
			ReadAppSettings(6, 57, 1, ref array[0]);
			break;
		case 43:
			ReadAppSettings(6, 58, 1, ref array[0]);
			break;
		case 96:
			ReadAppSettings(6, 59, 1, ref array[0]);
			break;
		case 97:
			ReadAppSettings(6, 60, 1, ref array[0]);
			break;
		case 107:
			ReadAppSettings(6, 75, 1, ref array[0]);
			break;
		case 108:
			ReadAppSettings(6, 76, 1, ref array[0]);
			break;
		case 4500:
			ReadAppSettings(6, 77, 1, ref array[0]);
			break;
		case 4501:
			ReadAppSettings(6, 78, 1, ref array[0]);
			break;
		case 4502:
			ReadAppSettings(6, 79, 1, ref array[0]);
			break;
		case 4503:
			ReadAppSettings(6, 80, 1, ref array[0]);
			break;
		case 4504:
			ReadAppSettings(6, 81, 1, ref array[0]);
			break;
		case 4505:
			ReadAppSettings(6, 82, 1, ref array[0]);
			break;
		case 4506:
			ReadAppSettings(6, 83, 1, ref array[0]);
			break;
		case 4507:
			ReadAppSettings(6, 84, 1, ref array[0]);
			break;
		}
		return BitConverter.ToInt32(array, 0);
	}

	public static int GetCPUVoltageMode()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 73, 1, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static void SetCPUVoltageMode(int mode)
	{
		byte[] bytes = BitConverter.GetBytes(mode);
		byte[] array = new byte[4];
		WriteAppSettings(6, 73, 1, ref bytes[0]);
	}

	public static void SetCoreRatio(int id, int ratio)
	{
		byte[] bytes = BitConverter.GetBytes(ratio);
		switch (id)
		{
		case 29:
			WriteAppSettings(6, 53, 1, ref bytes[0]);
			break;
		case 30:
			WriteAppSettings(6, 54, 1, ref bytes[0]);
			break;
		case 31:
			WriteAppSettings(6, 55, 1, ref bytes[0]);
			break;
		case 32:
			WriteAppSettings(6, 56, 1, ref bytes[0]);
			break;
		case 42:
			WriteAppSettings(6, 57, 1, ref bytes[0]);
			break;
		case 43:
			WriteAppSettings(6, 58, 1, ref bytes[0]);
			break;
		case 96:
			WriteAppSettings(6, 59, 1, ref bytes[0]);
			break;
		case 97:
			WriteAppSettings(6, 60, 1, ref bytes[0]);
			break;
		case 107:
			WriteAppSettings(6, 75, 1, ref bytes[0]);
			break;
		case 108:
			WriteAppSettings(6, 76, 1, ref bytes[0]);
			break;
		case 4500:
			WriteAppSettings(6, 77, 1, ref bytes[0]);
			break;
		case 4501:
			WriteAppSettings(6, 78, 1, ref bytes[0]);
			break;
		case 4502:
			WriteAppSettings(6, 79, 1, ref bytes[0]);
			break;
		case 4503:
			WriteAppSettings(6, 80, 1, ref bytes[0]);
			break;
		case 4504:
			WriteAppSettings(6, 81, 1, ref bytes[0]);
			break;
		case 4505:
			WriteAppSettings(6, 82, 1, ref bytes[0]);
			break;
		case 4506:
			WriteAppSettings(6, 83, 1, ref bytes[0]);
			break;
		case 4507:
			WriteAppSettings(6, 84, 1, ref bytes[0]);
			break;
		}
	}

	public static void SetIsSavedFlag(int v)
	{
		byte[] bytes = BitConverter.GetBytes(v);
		WriteAppSettings(6, 32, 1, ref bytes[0]);
	}

	public static void SetPowerLimit1CurrentValue(int PL1)
	{
		byte[] bytes = BitConverter.GetBytes(PL1);
		WriteAppSettings(6, 33, 4, ref bytes[0]);
	}

	public static void SetPowerLimitTimeCurrentValue(float PL_Time)
	{
		byte[] bytes = BitConverter.GetBytes(PL_Time);
		WriteAppSettings(6, 41, 4, ref bytes[0]);
	}

	public static void SetPowerLimit2CurrentValue(int PL2)
	{
		byte[] bytes = BitConverter.GetBytes(PL2);
		WriteAppSettings(6, 37, 4, ref bytes[0]);
	}

	public static void SetCPUVR(int VR_Limit)
	{
		byte[] bytes = BitConverter.GetBytes(VR_Limit);
		WriteAppSettings(6, 49, 4, ref bytes[0]);
	}

	public static void SetCoreVoltage(int voltage)
	{
		byte[] bytes = BitConverter.GetBytes(voltage);
		WriteAppSettings(6, 61, 4, ref bytes[0]);
	}

	public static void SetGraphicsVoltage(int voltage)
	{
		byte[] bytes = BitConverter.GetBytes(voltage);
		WriteAppSettings(6, 69, 4, ref bytes[0]);
	}

	public static byte[] GetWMIPackage(int command)
	{
		byte[] array = new byte[256];
		try
		{
			int dCHU_Data_Buffer = GetDCHU_Data_Buffer(command, ref array[0]);
		}
		catch
		{
		}
		return array;
	}

	public static void SetCPUVRAuto(int auto)
	{
		byte[] bytes = BitConverter.GetBytes(auto);
		WriteAppSettings(6, 74, 1, ref bytes[0]);
	}

	public static void SetVoltageMode(int mode)
	{
		byte[] bytes = BitConverter.GetBytes(mode);
		WriteAppSettings(6, 73, 1, bytes);
	}

	public static void SetOverrideVoltage(float OverRide)
	{
		byte[] bytes = BitConverter.GetBytes(OverRide);
		WriteAppSettings(6, 65, 4, ref bytes[0]);
	}

	public static int GetCPU_Freq()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 9, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static double GetCPU_Voltage()
	{
		byte[] array = new byte[8];
		ReadAppSettings(6, 13, 8, ref array[0]);
		return BitConverter.ToDouble(array, 0);
	}

	public static int GetCPU_Utilization()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 21, 1, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static int GetRAMClock()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 23, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static int GetCPU_Temperature()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 22, 1, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static void SetFullScreen(int fullScreen)
	{
		byte[] bytes = BitConverter.GetBytes(fullScreen);
		WriteAppSettings(6, 31, 4, ref bytes[0]);
	}

	public static int GetFullScreen()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 31, 1, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}
}
