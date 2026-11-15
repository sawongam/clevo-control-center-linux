using System;
using CCenter.Platform;

namespace ControlCenter30;

// Original: P/Invoke wrapper around InsydeDCHU.dll. The extern declarations are redirected to
// CCenter.Platform.InsydeDCHU (same names/signatures); everything else is unchanged.
internal class DCHU
{
	private static int GetDCHU_Data_Integer(int command, ref int data) => InsydeDCHU.GetDCHU_Data_Integer(command, ref data);

	private static int SetDCHU_Data(int command, byte[] buffer, int length) => InsydeDCHU.SetDCHU_Data(command, buffer, length);

	private static int GetDCHU_Data_Buffer(int command, ref byte buffer) => InsydeDCHU.GetDCHU_Data_Buffer(command, ref buffer);

	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);

	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);

	public static int SetDCHU_DataEx(int command, byte[] buffer, int length, ref byte OutputBuffer) => InsydeDCHU.SetDCHU_DataEx(command, buffer, length, ref OutputBuffer);

	public static int SetWMIPackageEx(int command, byte[] buffer, ref byte[] o_buffer)
	{
		int num = 0;
		try
		{
			num = SetDCHU_DataEx(command, buffer, 256, ref o_buffer[0]);
		}
		catch
		{
			return 0;
		}
		return num;
	}

	public static int GetDCHU_Data(int command, ref int data)
	{
		return GetDCHU_Data_Integer(Convert.ToInt32(command), ref data);
	}

	public static int GetWMI(int command)
	{
		int data = 0;
		GetDCHU_Data_Integer(command, ref data);
		DebugLog.WriteDbgLog2Outlog("CC3.0, GetWMI " + command + ", data=" + data);
		return data;
	}

	public static int SetWMI(int command, int SubCommand, uint data)
	{
		byte[] array = new byte[256];
		try
		{
			array = BitConverter.GetBytes(data);
			array[3] = Convert.ToByte(SubCommand);
			SetDCHU_Data(command, array, array.Length);
			DebugLog.WriteDbgLog2Outlog("Set WMI=" + command + "  SubCMD=" + SubCommand + "  Data=" + data);
		}
		catch
		{
			return 0;
		}
		return 1;
	}

	public static int SetWMI(int command, uint data)
	{
		byte[] array = new byte[256];
		try
		{
			array = BitConverter.GetBytes(data);
			SetDCHU_Data(command, array, array.Length);
			DebugLog.WriteDbgLog2Outlog("Set WMI=" + command + "  Data=" + data);
		}
		catch
		{
			return 0;
		}
		return 1;
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

	public static byte[] GetWMIPackage(int command)
	{
		byte[] array = new byte[256];
		try
		{
			int dCHU_Data_Buffer = GetDCHU_Data_Buffer(command, ref array[0]);
			DebugLog.WriteDbgLog2Outlog("GetWMI Package CMD=" + command + "  result=" + dCHU_Data_Buffer);
		}
		catch
		{
		}
		return array;
	}

	public static bool IsXTUInfoRead()
	{
		byte[] array = new byte[4];
		ReadAppSettings(6, 253, 1, ref array[0]);
		if (BitConverter.ToInt32(array, 0) == 1)
		{
			return true;
		}
		return false;
	}

	public static void SetXTUInfoRead(int read)
	{
		byte[] bytes = BitConverter.GetBytes(read);
		WriteAppSettings(6, 253, 1, bytes);
	}

	public static void SetAMD_Auto_Manual_Mode(int mode)
	{
		byte[] bytes = BitConverter.GetBytes(mode);
		WriteAppSettings(1, 17, 1, bytes);
	}

	public static int ReadAMD_Auto_Manual_Mode()
	{
		byte[] array = new byte[4];
		ReadAppSettings(1, 17, 1, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static void WriteAMDFrequency(uint freq)
	{
		byte[] bytes = BitConverter.GetBytes(freq);
		WriteAppSettings(1, 18, 4, ref bytes[0]);
	}

	public static void WriteAMDVoltage(float voltage)
	{
		byte[] bytes = BitConverter.GetBytes(voltage);
		WriteAppSettings(1, 22, 4, ref bytes[0]);
	}

	public static int ReadAMDFrequency()
	{
		byte[] array = new byte[4];
		ReadAppSettings(1, 18, 4, ref array[0]);
		return BitConverter.ToInt32(array, 0);
	}

	public static float ReadAMDVoltage()
	{
		byte[] array = new byte[4];
		ReadAppSettings(1, 22, 4, ref array[0]);
		return BitConverter.ToSingle(array, 0);
	}
}
