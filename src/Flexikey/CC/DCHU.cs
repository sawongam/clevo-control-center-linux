using System;
using CCenter.Platform;

namespace CC;

internal class DCHU
{
	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);
	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);

	public static int SetWMIPackageEx(int command, byte[] buffer, ref byte[] o_buffer)
	{
		try { return InsydeDCHU.SetDCHU_DataEx(command, buffer, 256, ref o_buffer[0]); }
		catch { return 0; }
	}
	public static int GetDCHU_Data(int command, ref int data) => InsydeDCHU.GetDCHU_Data_Integer(Convert.ToInt32(command), ref data);
	public static int GetDCHU_Data(int command, ref byte buffer) => InsydeDCHU.GetDCHU_Data_Buffer(command, ref buffer);
	public static byte[] GetWMIPackage(int command)
	{
		byte[] array = new byte[256];
		try { InsydeDCHU.GetDCHU_Data_Buffer(command, ref array[0]); } catch { }
		return array;
	}
	public static int WriteAppSettings(int page, int offset, int length, byte[] buffer)
	{
		try { return InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer[0]); } catch { return 0; }
	}
	public static byte[] ReadAppSettings(int page, int offset, int length)
	{
		byte[] array = new byte[length];
		try { InsydeDCHU.ReadAppSettings(page, offset, length, ref array[0]); } catch { }
		return array;
	}
	public static int ReadAcpiUILanguage()
	{
		byte[] array = new byte[1];
		InsydeDCHU.ReadAppSettings(258 / 256, 258 % 256, 1, ref array[0]);
		return array[0];
	}
}
