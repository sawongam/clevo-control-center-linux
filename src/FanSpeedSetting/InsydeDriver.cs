using System;
using CCenter.Platform;
using System.Text;

namespace FanSpeedSetting;

public class InsydeDriver
{
	// original: [DllImport("InsydeDCHU.dll")] externs -> redirected to CCenter.Platform.InsydeDCHU (same signatures)
	public static int GetDCHU_Data_Integer(int command, ref int data) => InsydeDCHU.GetDCHU_Data_Integer(command, ref data);

	public static int SetDCHU_Data(int command, byte[] buffer, int length) => InsydeDCHU.SetDCHU_Data(command, buffer, length);

	public static int SetDCHU_DataEx(int command, byte[] buffer, int length, ref byte OutputBuffer) => InsydeDCHU.SetDCHU_DataEx(command, buffer, length, ref OutputBuffer);

	public static int GetDCHU_Data_Buffer(int command, ref byte buffer) => InsydeDCHU.GetDCHU_Data_Buffer(command, ref buffer);

	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);

	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);

	public int SetWMI(int command, uint data)
	{
		byte[] array = new byte[4];
		array = BitConverter.GetBytes(data);
		int result = SetDCHU_Data(command, array, 4);
		DebugLog.WriteDbgLog2Outlog("SetWMI CMD=" + command + " Data=" + data);
		return result;
	}

	public int SetWMI(int command, int SubCommand, uint data)
	{
		byte[] array = new byte[4];
		array = BitConverter.GetBytes(data);
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

	public int SetWMIPackageEx(int command, byte[] buffer, ref byte[] o_buffer)
	{
		int num = 0;
		try
		{
			byte[] array = new byte[256];
			num = SetDCHU_DataEx(command, buffer, 256, ref o_buffer[0]);
		}
		catch
		{
			return 0;
		}
		return num;
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
		int dCHU_Data_Buffer = GetDCHU_Data_Buffer(command, ref array[0]);
		return array;
	}

	public byte[] GetAPPData(int page, int Index, int length)
	{
		byte[] array = new byte[length];
		int num = ReadAppSettings(page, Index, length, ref array[0]);
		return array;
	}

	public int SetAPPData(int page, int Index, int length, byte[] Data)
	{
		return WriteAppSettings(page, Index, length, ref Data[0]);
	}

	public void Init_WMI()
	{
		int wMI = GetWMI(70);
		byte[] array = new byte[256];
		array = Global.insyde.GetAPPData(7, 0, 256);
		switch ((array[0] << 8) + array[1])
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

	private void BIOSSpecialFeature_v0_0(byte[] offset)
	{
		int wMI = GetWMI(16);
		if ((wMI & 1) == 1)
		{
			Global.support_bit.UWP_PowerMode = true;
		}
		if ((wMI & 2) == 2)
		{
			Global.support_bit.UWP_Flexikey = true;
		}
		if ((wMI & 4) == 4)
		{
			Global.support_bit.UWP_FlexiAccess = true;
		}
		if ((wMI & 8) == 8)
		{
			Global.support_bit.UWP_PerkeyKB = true;
		}
		if ((wMI & 0x10) == 16)
		{
			Global.support_bit.UWP_RGBKB = true;
		}
		if ((wMI & 0x20) == 32)
		{
			Global.support_bit.UWP_GPUOC = true;
		}
		if ((wMI & 0x40) == 64)
		{
			Global.support_bit.UWP_CPUOC = true;
		}
		if ((wMI & 0x80) == 128)
		{
			Global.support_bit.UWP_FanSetting = true;
		}
		int wMI2 = GetWMI(122);
		if (((wMI2 >> 15) & 1) == 1)
		{
			Global.support_bit.FanLess = true;
		}
		int wMI3 = GetWMI(96);
		if (((wMI3 >> 7) & 1) == 1)
		{
			Global.support_bit.AntiDust_Fan = true;
		}
		if (((wMI3 >> 10) & 1) == 1)
		{
			Global.support_bit.FanOffset = false;
		}
	}

	private void BIOSSpecialFeature_v1_0(byte[] offset)
	{
		int num = (offset[5] << 24) + (offset[4] << 16) + (offset[3] << 8) + offset[2];
		int num2 = (offset[9] << 24) + (offset[8] << 16) + (offset[7] << 8) + offset[6];
		int num3 = (offset[11] << 8) + offset[10];
		int num4 = (offset[14] << 8) + offset[13];
		if ((num4 & 1) == 1)
		{
			Global.support_bit.UWP_PowerMode = true;
		}
		if ((num4 & 2) == 2)
		{
			Global.support_bit.UWP_Flexikey = true;
		}
		if ((num4 & 4) == 4)
		{
			Global.support_bit.UWP_FlexiAccess = true;
		}
		if ((num4 & 8) == 8)
		{
			Global.support_bit.UWP_PerkeyKB = true;
		}
		if ((num4 & 0x10) == 16)
		{
			Global.support_bit.UWP_RGBKB = true;
		}
		if ((num4 & 0x20) == 32)
		{
			Global.support_bit.UWP_GPUOC = true;
		}
		if ((num4 & 0x40) == 64)
		{
			Global.support_bit.UWP_CPUOC = true;
		}
		if ((num4 & 0x80) == 128)
		{
			Global.support_bit.UWP_FanSetting = true;
		}
		if (((num2 >> 15) & 1) == 1)
		{
			Global.support_bit.FanLess = true;
		}
		if (((num3 >> 7) & 1) == 1)
		{
			Global.support_bit.AntiDust_Fan = true;
		}
		if (((num3 >> 10) & 1) == 1)
		{
			Global.support_bit.FanOffset = false;
		}
		if (((num3 >> 12) & 1) == 1)
		{
			Global.support_bit.DTT = true;
		}
		if (((offset[16] >> 4) & 1) == 1)
		{
			Global.support_bit.TurboFan = true;
		}
	}

	public string GetECVersion()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		byte[] array2 = new byte[24];
		string text = "1.";
		array[0] = 1;
		array[1] = 0;
		array[2] = 0;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 222;
		int num = SetWMIPackageEx(4, array, ref o_buffer);
		array[0] = 1;
		array[1] = 1;
		array[2] = 0;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 222;
		num = SetWMIPackageEx(4, array, ref o_buffer);
		array2[0] = o_buffer[1];
		array2[1] = o_buffer[2];
		array2[2] = o_buffer[3];
		array2[3] = o_buffer[4];
		array2[4] = o_buffer[5];
		array[0] = 1;
		array[1] = 2;
		array[2] = 0;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 222;
		num = SetWMIPackageEx(4, array, ref o_buffer);
		array2[5] = o_buffer[1];
		array2[6] = o_buffer[2];
		array2[7] = o_buffer[3];
		array2[8] = o_buffer[4];
		array2[9] = o_buffer[5];
		array[0] = 1;
		array[1] = 3;
		array[2] = 0;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 222;
		num = SetWMIPackageEx(4, array, ref o_buffer);
		array2[10] = o_buffer[1];
		array2[11] = o_buffer[2];
		array2[12] = o_buffer[3];
		array2[13] = o_buffer[4];
		array2[14] = o_buffer[5];
		text += Encoding.ASCII.GetString(array2);
		string[] array3 = text.Split(new char[1] { '$' });
		return array3[0].Trim(new char[1]);
	}
}
