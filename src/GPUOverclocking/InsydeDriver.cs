using System;
using RippleCC.Platform;

namespace GPUOverclocking;

public class InsydeDriver
{
	// [DllImport("InsydeDCHU.dll")] externs redirected to RippleCC.Platform.InsydeDCHU (same signatures)
	public static int GetDCHU_Data_Integer(int command, ref int data) => InsydeDCHU.GetDCHU_Data_Integer(command, ref data);
	public static int SetDCHU_Data(int command, byte[] buffer, int length) => InsydeDCHU.SetDCHU_Data(command, buffer, length);
	public static int GetDCHU_Data_Buffer(int command, ref byte buffer) => InsydeDCHU.GetDCHU_Data_Buffer(command, ref buffer);
	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);
	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);

	public int SetWMI(int command, uint data)
	{
		byte[] array = BitConverter.GetBytes(data);
		int result = SetDCHU_Data(command, array, 4);
		DebugLog.WriteDbgLog2Outlog("SetWMI CMD=" + command + " Data=" + data);
		return result;
	}

	public int SetWMI(int command, int SubCommand, uint data)
	{
		byte[] array = BitConverter.GetBytes(data);
		array[3] = Convert.ToByte(SubCommand);
		int result = SetDCHU_Data(command, array, 4);
		DebugLog.WriteDbgLog2Outlog("SetWMI CMD=" + command + " SubCMD=" + SubCommand + " Data=" + data);
		return result;
	}

	public int SetWMIPackage(int command, byte[] buffer)
	{
		int result = SetDCHU_Data(command, buffer, 256);
		DebugLog.WriteDbgLog2Outlog("SetWMI Package CMD=" + command);
		return result;
	}

	public int GetWMI(int command)
	{
		int data = 0;
		GetDCHU_Data_Integer(command, ref data);
		DebugLog.WriteDbgLog2Outlog("GetWMI CMD=" + command + " Data=" + data);
		return data;
	}

	public byte[] GetWMIPackage(int command)
	{
		byte[] array = new byte[256];
		int r = GetDCHU_Data_Buffer(command, ref array[0]);
		DebugLog.WriteDbgLog2Outlog("GetWMI Package CMD=" + command + " result=" + r);
		return array;
	}

	public byte[] GetAPPData(int page, int Index, int length)
	{
		byte[] array = new byte[length];
		int num = ReadAppSettings(page, Index, length, ref array[0]);
		DebugLog.WriteDbgLog2Outlog("GetAPPData(" + page + "," + Index + " ," + length + ")Data=" + array[0]);
		return array;
	}

	public int SetAPPData(int page, int Index, int length, byte[] Data)
	{
		int result = WriteAppSettings(page, Index, length, ref Data[0]);
		DebugLog.WriteDbgLog2Outlog("SetAPPData page=" + page + " Index=" + Index + " length=" + length);
		return result;
	}

	public void Init_WMI()
	{
		int wMI = GetWMI(70);
		int wMI2 = GetWMI(16);
		if ((wMI2 & 1) == 1) Global.support_bit.UWP_FnKey = true;
		if ((wMI2 & 2) == 2) Global.support_bit.UWP_Flexikey = true;
		if ((wMI2 & 4) == 4) Global.support_bit.UWP_FlexiAccess = true;
		if ((wMI2 & 8) == 8) Global.support_bit.UWP_PerkeyKB = true;
		if ((wMI2 & 0x10) == 16) Global.support_bit.UWP_RGBKB = true;
		if ((wMI2 & 0x20) == 32) Global.support_bit.UWP_GPUOC = true;
		if ((wMI2 & 0x40) == 64) Global.support_bit.UWP_CPUOC = true;
		if ((wMI2 & 0x80) == 128) Global.support_bit.UWP_FanSetting = true;
		int wMI3 = GetWMI(122);
		if ((wMI3 & 0x8000) == 32768) Global.support_bit.FanLess = true;
	}
}
