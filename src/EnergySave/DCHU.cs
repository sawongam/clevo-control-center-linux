using System;
using RippleCC.Platform;

namespace DesktopBridge_EnergySave;

internal class DCHU
{
	private static int GetDCHU_Data_Integer(int command, ref int data) => InsydeDCHU.GetDCHU_Data_Integer(command, ref data);

	private static int SetDCHU_Data(int command, byte[] buffer, int length) => InsydeDCHU.SetDCHU_Data(command, buffer, length);

	private static int GetDCHU_Data_Buffer(int command, ref byte buffer) => InsydeDCHU.GetDCHU_Data_Buffer(command, ref buffer);

	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);

	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);

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

	public static int SetWMIPackage(int command, byte[] buffer)
	{
		int num = 0;
		try
		{
			num = SetDCHU_Data(command, buffer, buffer.Length);
			DebugLog.WriteDbgLog2Outlog("SetWMI Package CMD=" + command);
		}
		catch
		{
			return 0;
		}
		return num;
	}

	public static int GetWMI(int command)
	{
		int data = 0;
		try
		{
			GetDCHU_Data_Integer(command, ref data);
			DebugLog.WriteDbgLog2Outlog("Get WMI=" + command + "  Data=" + data);
		}
		catch
		{
		}
		return data;
	}

	public static int SetWMIForTotalCurrent(int command, uint data)
	{
		byte[] array = new byte[256];
		int num = 99;
		try
		{
			array = BitConverter.GetBytes(data);
			byte[] outBuf = new byte[256];
			num = InsydeDCHU.SetDCHU_DataEx(command, array, array.Length, ref outBuf[0]) == 0 ? 0 : BitConverter.ToInt32(outBuf, 0);
		}
		catch
		{
			return 0;
		}
		return num;
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
}
