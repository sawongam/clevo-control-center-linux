using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FnKey;

public class DCHU
{
	public static int GetDCHU_Data_Integer(int command, ref int data) => CCenter.Platform.InsydeDCHU.GetDCHU_Data_Integer(command, ref data);
	public static int SetDCHU_Data(int command, byte[] buffer, int length) => CCenter.Platform.InsydeDCHU.SetDCHU_Data(command, buffer, length);
	public static int SetDCHU_DataEx(int command, byte[] buffer, int length, ref byte OutputBuffer) => CCenter.Platform.InsydeDCHU.SetDCHU_DataEx(command, buffer, length, ref OutputBuffer);
	public static int GetDCHU_Data_Buffer(int command, ref byte buffer) => CCenter.Platform.InsydeDCHU.GetDCHU_Data_Buffer(command, ref buffer);
	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => CCenter.Platform.InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);
	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer) => CCenter.Platform.InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);

	public int GetWMI(int command)
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

	public byte[] ReadAppSettings(int page, int offset, int length)
	{
		byte[] array = new byte[length];
		try
		{
			ReadAppSettings(page, offset, length, ref array[0]);
		}
		catch
		{
		}
		return array;
	}

	public int WriteAppSettings(int page, int offset, int length, byte[] buffer)
	{
		try
		{
			return WriteAppSettings(page, offset, length, ref buffer[0]);
		}
		catch
		{
			return 0;
		}
	}

	public int SetWMI(int command, uint data)
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

	public int SetWMI(int command, int SubCommand, uint data)
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

	public byte[] GetWMIPackage(int command)
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

	public int SetWMIPackage(int command, byte[] buffer)
	{
		int num = 0;
		try
		{
			num = SetDCHU_Data(command, buffer, buffer.Length);
			DebugLog.WriteDbgLog2Outlog("SetWMI Package CMD=" + command);
			return num;
		}
		catch
		{
			return 0;
		}
	}

	public void SetH2RAM(uint Index, uint Data, uint Mask)
	{
		uint num = 0u;
		SetWMI(107, Index);
		num = GetWMIPackage(115)[0];
		uint num2 = 0u;
		if (Data == 0)
		{
			num2 = num & ~Mask;
		}
		else
		{
			uint num3 = 0u;
			Mask = ~Mask;
			num3 = num & Mask;
			num2 = Data | num3;
		}
		SetWMI(117, num2);
	}

	public uint GetH2RAM(uint Index)
	{
		SetWMI(107, Index);
		return GetWMIPackage(115)[0];
	}

	public byte[] Read_ECRAM(uint index, byte length)
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		byte[] array2 = new byte[length];
		array[0] = 1;
		array[1] = (byte)((index >> 8) & 0xFF);
		array[2] = (byte)(index & 0xFF);
		array[3] = length;
		array[4] = 0;
		array[5] = 0;
		array[6] = 177;
		SetWMIPackageEx(4, array, ref o_buffer);
		for (int i = 0; i < length; i++)
		{
			array2[i] = o_buffer[1 + i];
		}
		return array2;
	}

	public void Write_ECRAM(uint index, uint Data, uint Mask)
	{
		byte[] array = Read_ECRAM(index, 2);
		uint num = (uint)((array[0] + (array[1] << 8)) | Data);
		uint num2 = ~Mask | Data;
		num &= num2;
		byte[] array2 = new byte[256];
		byte[] o_buffer = new byte[256];
		array2[0] = 1;
		array2[1] = (byte)((index >> 8) & 0xFF);
		array2[2] = (byte)(index & 0xFF);
		array2[3] = 1;
		array2[4] = (byte)(num & 0xFF);
		array2[5] = (byte)(num >> 8);
		array2[6] = 178;
		SetWMIPackageEx(4, array2, ref o_buffer);
	}

	public int SetWMIPackageEx(int command, byte[] buffer, ref byte[] o_buffer)
	{
		int num = 0;
		try
		{
			_ = new byte[256];
			return SetDCHU_DataEx(command, buffer, 256, ref o_buffer[0]);
		}
		catch
		{
			return 0;
		}
	}

	public string GetECVersion()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		byte[] array2 = new byte[24];
		array[0] = 1;
		array[1] = 0;
		array[2] = 0;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 222;
		SetWMIPackageEx(4, array, ref o_buffer);
		array[0] = 1;
		array[1] = 1;
		array[2] = 0;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 222;
		SetWMIPackageEx(4, array, ref o_buffer);
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
		SetWMIPackageEx(4, array, ref o_buffer);
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
		SetWMIPackageEx(4, array, ref o_buffer);
		array2[10] = o_buffer[1];
		array2[11] = o_buffer[2];
		array2[12] = o_buffer[3];
		array2[13] = o_buffer[4];
		array2[14] = o_buffer[5];
		return ("1." + Encoding.ASCII.GetString(array2)).Split(new char[1] { '$' })[0].Trim(new char[1]);
	}

	public void GetECoff()
	{
		byte[] array = new byte[3];
		array = Read_ECRAM(1585u, 3);
		DebugLog.WriteDbgLog2Outlog("EC GetOff\n0x631 Data=0x" + Convert.ToString(array[0], 16) + "\n0x632 Data= 0x" + Convert.ToString(array[1], 16) + "\n0x633 Data=0x" + Convert.ToString(array[2], 16));
	}
}
