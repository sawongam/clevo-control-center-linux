using System;

namespace FnKey.Device;

public class FAN
{
	public Global.FanArgs Fan_CPU = new Global.FanArgs();

	public Global.FanArgs Fan_GPU1 = new Global.FanArgs();

	public Global.FanArgs Fan_GPU2 = new Global.FanArgs();

	public int FanCount;

	public int InitFanMode;

	public int FanMode;

	public int FanOffset;

	public int TurboFanStatus;

	public int FanAPPVersion_Major;

	public int FanAPPVersion_Minor;

	public int FanAPPVersion_Revision;

	public int FanAPPVersion_Build;

	public int AntiDust_RunStatus;

	public int AntiDust_Scheduler_status;

	public int AntiDust_S_Day = 255;

	public int AntiDust_S_Resume = 1;

	public int AntiDust_S_Hour;

	public bool Support_MaxQ;

	public bool FanRusume;

	public bool SupportCustomFan = true;

	public void Read_WMI13()
	{
		byte[] array = new byte[256];
		array = Global.dchu.GetWMIPackage(13);
		FanCount = array[12];
		InitFanMode = array[14];
		if (InitFanMode == 5)
		{
			Support_MaxQ = true;
		}
		byte b = array[43];
		if (FanCount <= 1 || ((b >> 1) & 1) == 1)
		{
			SupportCustomFan = false;
		}
		if (!SupportCustomFan)
		{
			return;
		}
		Fan_CPU.T1 = array[16];
		Fan_CPU.D1 = (byte)Math.Round((double)(int)array[17] / 255.0 * 100.0, 0);
		Fan_CPU.T2 = (Fan_CPU.T2_Default = array[18]);
		Fan_CPU.D2 = (Fan_CPU.D2_Default = (byte)Math.Round((double)(int)array[19] / 255.0 * 100.0, 0));
		Fan_CPU.T3 = (Fan_CPU.T3_Default = array[20]);
		Fan_CPU.D3 = (Fan_CPU.D3_Default = (byte)Math.Round((double)(int)array[21] / 255.0 * 100.0, 0));
		Fan_CPU.T4 = 100;
		Fan_CPU.D4 = 100;
		if (Fan_CPU.D1 >= Fan_CPU.D2)
		{
			Fan_CPU.D2 = (byte)(Fan_CPU.D1 + 1);
			if (Fan_CPU.D2 >= Fan_CPU.D3)
			{
				Fan_CPU.D3 = (byte)(Fan_CPU.D2 + 1);
			}
			if (Fan_CPU.D3 >= Fan_CPU.D4)
			{
				Fan_CPU.D3 = (byte)(Fan_CPU.D4 - 1);
			}
		}
		if (Fan_CPU.D2 >= Fan_CPU.D3)
		{
			Fan_CPU.D3 = (byte)(Fan_CPU.D2 + 1);
			if (Fan_CPU.D3 >= Fan_CPU.D4)
			{
				Fan_CPU.D3 = (byte)(Fan_CPU.D4 - 1);
			}
			if (Fan_CPU.D2 >= Fan_CPU.D3)
			{
				Fan_CPU.D2 = (byte)(Fan_CPU.D3 - 1);
			}
		}
		if (Fan_CPU.D3 >= Fan_CPU.D4)
		{
			Fan_CPU.D3--;
			if (Fan_CPU.D2 >= Fan_CPU.D3)
			{
				Fan_CPU.D2 = (byte)(Fan_CPU.D3 - 1);
			}
		}
		Fan_CPU.D2_Default = Fan_CPU.D2;
		Fan_CPU.D3_Default = Fan_CPU.D3;
		Fan_GPU1.T1 = array[24];
		Fan_GPU1.D1 = (byte)Math.Round((double)(int)array[25] / 255.0 * 100.0, 0);
		Fan_GPU1.T2 = (Fan_GPU1.T2_Default = array[26]);
		Fan_GPU1.D2 = (Fan_GPU1.D2_Default = (byte)Math.Round((double)(int)array[27] / 255.0 * 100.0, 0));
		Fan_GPU1.T3 = (Fan_GPU1.T3_Default = array[28]);
		Fan_GPU1.D3 = (Fan_GPU1.D3_Default = (byte)Math.Round((double)(int)array[29] / 255.0 * 100.0, 0));
		Fan_GPU1.T4 = 100;
		Fan_GPU1.D4 = 100;
		if (Fan_GPU1.D1 >= Fan_GPU1.D2)
		{
			Fan_GPU1.D2 = (byte)(Fan_GPU1.D1 + 1);
			if (Fan_GPU1.D2 >= Fan_GPU1.D3)
			{
				Fan_GPU1.D3 = (byte)(Fan_GPU1.D2 + 1);
			}
			if (Fan_GPU1.D3 >= Fan_GPU1.D4)
			{
				Fan_GPU1.D3 = (byte)(Fan_GPU1.D4 - 1);
			}
		}
		if (Fan_GPU1.D2 >= Fan_GPU1.D3)
		{
			Fan_GPU1.D3 = (byte)(Fan_GPU1.D2 + 1);
			if (Fan_GPU1.D3 >= Fan_GPU1.D4)
			{
				Fan_GPU1.D3 = (byte)(Fan_GPU1.D4 - 1);
			}
			if (Fan_GPU1.D2 >= Fan_GPU1.D3)
			{
				Fan_GPU1.D2 = (byte)(Fan_GPU1.D3 - 1);
			}
		}
		if (Fan_GPU1.D3 >= Fan_GPU1.D4)
		{
			Fan_GPU1.D3--;
			if (Fan_GPU1.D2 >= Fan_GPU1.D3)
			{
				Fan_GPU1.D2 = (byte)(Fan_GPU1.D3 - 1);
			}
		}
		Fan_GPU1.D2_Default = Fan_GPU1.D2;
		Fan_GPU1.D3_Default = Fan_GPU1.D3;
		Fan_GPU2.T1 = array[32];
		Fan_GPU2.D1 = (byte)Math.Round((double)(int)array[33] / 255.0 * 100.0, 0);
		Fan_GPU2.T2 = (Fan_GPU2.T2_Default = array[34]);
		Fan_GPU2.D2 = (Fan_GPU2.D2_Default = (byte)Math.Round((double)(int)array[35] / 255.0 * 100.0, 0));
		Fan_GPU2.T3 = (Fan_GPU2.T3_Default = array[36]);
		Fan_GPU2.D3 = (Fan_GPU2.D3_Default = (byte)Math.Round((double)(int)array[37] / 255.0 * 100.0, 0));
		Fan_GPU2.T4 = 100;
		Fan_GPU2.D4 = 100;
		if (Fan_GPU2.D1 >= Fan_GPU2.D2)
		{
			Fan_GPU2.D2 = (byte)(Fan_GPU2.D1 + 1);
			if (Fan_GPU2.D2 >= Fan_GPU2.D3)
			{
				Fan_GPU2.D3 = (byte)(Fan_GPU2.D2 + 1);
			}
			if (Fan_GPU2.D3 >= Fan_GPU2.D4)
			{
				Fan_GPU2.D3 = (byte)(Fan_GPU2.D4 - 1);
			}
		}
		if (Fan_GPU2.D2 >= Fan_GPU2.D3)
		{
			Fan_GPU2.D3 = (byte)(Fan_GPU2.D2 + 1);
			if (Fan_GPU2.D3 >= Fan_GPU2.D4)
			{
				Fan_GPU2.D3 = (byte)(Fan_GPU2.D4 - 1);
			}
			if (Fan_GPU2.D2 >= Fan_GPU2.D3)
			{
				Fan_GPU2.D2 = (byte)(Fan_GPU2.D3 - 1);
			}
		}
		if (Fan_GPU2.D3 >= Fan_GPU2.D4)
		{
			Fan_GPU2.D3--;
			if (Fan_GPU2.D2 >= Fan_GPU2.D3)
			{
				Fan_GPU2.D2 = (byte)(Fan_GPU2.D3 - 1);
			}
		}
		Fan_GPU2.D2_Default = Fan_GPU2.D2;
		Fan_GPU2.D3_Default = Fan_GPU2.D3;
		string text = "";
		if (FanCount == 1)
		{
			text = "Get WMI 13 \nFan_CPU.T1=" + Fan_CPU.T1 + "\nFan_CPU.T2=" + Fan_CPU.T2 + "\nFan_CPU.T3=" + Fan_CPU.T3 + "\nFan_CPU.T4=" + Fan_CPU.T4 + "\nFan_CPU.T2_Default=" + Fan_CPU.T2_Default + "\nFan_CPU.T3_Default=" + Fan_CPU.T3_Default + "\nFan_CPU.D1=" + Fan_CPU.D1 + "\nFan_CPU.D2=" + Fan_CPU.D2 + "\nFan_CPU.D3=" + Fan_CPU.D3 + "\nFan_CPU.D4=" + Fan_CPU.D4 + "\nFan_CPU.D2_Default=" + Fan_CPU.D2_Default + "\nFan_CPU.D3_Default=" + Fan_CPU.D3_Default;
		}
		if (FanCount == 2)
		{
			text = text + "\n-------------------------------------------------\nFan_GPU1.T1=" + Fan_GPU1.T1 + "\nFan_GPU1.T2=" + Fan_GPU1.T2 + "\nFan_GPU1.T3=" + Fan_GPU1.T3 + "\nFan_GPU1.T4=" + Fan_GPU1.T4 + "\nFan_GPU1.T2_Default=" + Fan_GPU1.T2_Default + "\nFan_GPU1.T3_Default=" + Fan_GPU1.T3_Default + "\nFan_GPU1.D1=" + Fan_GPU1.D1 + "\nFan_GPU1.D2=" + Fan_GPU1.D2 + "\nFan_GPU1.D3=" + Fan_GPU1.D3 + "\nFan_GPU1.D4=" + Fan_GPU1.D4 + "\nFan_GPU1.D2_Default=" + Fan_GPU1.D2_Default + "\n Fan_GPU1.D3_Default=" + Fan_GPU1.D3_Default;
		}
		if (FanCount == 3)
		{
			text = text + "\n-------------------------------------------------\nFan_GPU2.T1=" + Fan_GPU2.T1 + "\nFan_GPU2.T2=" + Fan_GPU2.T2 + "\nFan_GPU2.T3=" + Fan_GPU2.T3 + "\nFan_GPU2.T4=" + Fan_GPU2.T4 + "\nFan_GPU2.T2_Default=" + Fan_GPU2.T2_Default + "\nFan_GPU2.T3_Default=" + Fan_GPU2.T3_Default + "\nFan_GPU2.D1=" + Fan_GPU2.D1 + "\nFan_GPU2.D2=" + Fan_GPU2.D2 + "\nFan_GPU2.D3=" + Fan_GPU2.D3 + "\nFan_GPU2.D4=" + Fan_GPU2.D4 + "\nFan_GPU2.D2_Default=" + Fan_GPU2.D2_Default + "\n Fan_GPU2.D3_Default=" + Fan_GPU2.D3_Default;
		}
		if (FanCount != 0)
		{
			DebugLog.WriteDbgLog2Outlog(text);
		}
	}

	public void SetCustomFanTable()
	{
		Load_Rxx();
		byte[] array = new byte[256];
		array[2] = Fan_CPU.T2;
		array[3] = (byte)Math.Round((double)(int)Fan_CPU.D2 / 100.0 * 255.0, 0);
		array[4] = Fan_CPU.T3;
		array[5] = (byte)Math.Round((double)(int)Fan_CPU.D3 / 100.0 * 255.0, 0);
		array[6] = Fan_GPU1.T2;
		array[7] = (byte)Math.Round((double)(int)Fan_GPU1.D2 / 100.0 * 255.0, 0);
		array[8] = Fan_GPU1.T3;
		array[9] = (byte)Math.Round((double)(int)Fan_GPU1.D3 / 100.0 * 255.0, 0);
		array[10] = Fan_GPU2.T2;
		array[11] = (byte)Math.Round((double)(int)Fan_GPU2.D2 / 100.0 * 255.0, 0);
		array[12] = Fan_GPU2.T3;
		array[13] = (byte)Math.Round((double)(int)Fan_GPU2.D3 / 100.0 * 255.0, 0);
		array[14] = (byte)(Fan_CPU.R12 >> 8);
		array[15] = (byte)(Fan_CPU.R12 & 0xFF);
		array[16] = (byte)(Fan_CPU.R23 >> 8);
		array[17] = (byte)(Fan_CPU.R23 & 0xFF);
		array[18] = (byte)(Fan_CPU.R34 >> 8);
		array[19] = (byte)(Fan_CPU.R34 & 0xFF);
		array[20] = (byte)(Fan_GPU1.R12 >> 8);
		array[21] = (byte)(Fan_GPU1.R12 & 0xFF);
		array[22] = (byte)(Fan_GPU1.R23 >> 8);
		array[23] = (byte)(Fan_GPU1.R23 & 0xFF);
		array[24] = (byte)(Fan_GPU1.R34 >> 8);
		array[25] = (byte)(Fan_GPU1.R34 & 0xFF);
		array[26] = (byte)(Fan_GPU2.R12 >> 8);
		array[27] = (byte)(Fan_GPU2.R12 & 0xFF);
		array[28] = (byte)(Fan_GPU2.R23 >> 8);
		array[29] = (byte)(Fan_GPU2.R23 & 0xFF);
		array[30] = (byte)(Fan_GPU2.R34 >> 8);
		array[31] = (byte)(Fan_GPU2.R34 & 0xFF);
		Global.dchu.SetWMIPackage(14, array);
	}

	public void Read_FanInfo()
	{
		byte[] array = new byte[256];
		array = Global.dchu.ReadAppSettings(4, 0, 256);
		InitFanMode = array[4];
		if (InitFanMode == 5)
		{
			Support_MaxQ = true;
		}
		FanMode = array[5];
		FanCount = array[6];
		if (!Global.features.SupportFanOffset)
		{
			FanOffset = 0;
		}
		else
		{
			FanOffset = array[7];
		}
		TurboFanStatus = array[8];
		Fan_CPU.D1 = array[16];
		Fan_CPU.D2 = array[17];
		Fan_CPU.D3 = array[18];
		Fan_CPU.D4 = 100;
		Fan_CPU.D2_Default = array[20];
		Fan_CPU.D3_Default = array[21];
		Fan_CPU.T1 = array[22];
		Fan_CPU.T2 = array[23];
		Fan_CPU.T3 = array[24];
		Fan_CPU.T4 = 100;
		Fan_CPU.T2_Default = array[26];
		Fan_CPU.T3_Default = array[27];
		Fan_CPU.R12 = (array[29] << 8) + array[28];
		Fan_CPU.R23 = (array[31] << 8) + array[30];
		Fan_CPU.R34 = (array[33] << 8) + array[32];
		Fan_GPU1.D1 = array[34];
		Fan_GPU1.D2 = array[35];
		Fan_GPU1.D3 = array[36];
		Fan_GPU1.D4 = 100;
		Fan_GPU1.D2_Default = array[38];
		Fan_GPU1.D3_Default = array[39];
		Fan_GPU1.T1 = array[40];
		Fan_GPU1.T2 = array[41];
		Fan_GPU1.T3 = array[42];
		Fan_GPU1.T4 = 100;
		Fan_GPU1.T2_Default = array[44];
		Fan_GPU1.T3_Default = array[45];
		Fan_GPU1.R12 = (array[47] << 8) + array[46];
		Fan_GPU1.R23 = (array[49] << 8) + array[48];
		Fan_GPU1.R34 = (array[51] << 8) + array[50];
		Fan_GPU2.D1 = array[52];
		Fan_GPU2.D2 = array[53];
		Fan_GPU2.D3 = array[54];
		Fan_GPU2.D4 = 100;
		Fan_GPU2.D2_Default = array[56];
		Fan_GPU2.D3_Default = array[57];
		Fan_GPU2.T1 = array[58];
		Fan_GPU2.T2 = array[59];
		Fan_GPU2.T3 = array[60];
		Fan_GPU2.T4 = 100;
		Fan_GPU2.T2_Default = array[62];
		Fan_GPU2.T3_Default = array[63];
		Fan_GPU2.R12 = (array[65] << 8) + array[64];
		Fan_GPU2.R23 = (array[67] << 8) + array[66];
		Fan_GPU2.R34 = (array[69] << 8) + array[68];
		string text = "";
		if (FanCount == 1)
		{
			text = "Get Reg APP Data \nFan_CPU.T1=" + Fan_CPU.T1 + "\nFan_CPU.T2=" + Fan_CPU.T2 + "\nFan_CPU.T3=" + Fan_CPU.T3 + "\nFan_CPU.T4=" + Fan_CPU.T4 + "\nFan_CPU.T2_Default=" + Fan_CPU.T2_Default + "\nFan_CPU.T3_Default=" + Fan_CPU.T3_Default + "\nFan_CPU.D1=" + Fan_CPU.D1 + "\nFan_CPU.D2=" + Fan_CPU.D2 + "\nFan_CPU.D3=" + Fan_CPU.D3 + "\nFan_CPU.D4=" + Fan_CPU.D4 + "\nFan_CPU.D2_Default=" + Fan_CPU.D2_Default + "\nFan_CPU.D3_Default=" + Fan_CPU.D3_Default;
		}
		if (FanCount == 2)
		{
			text = text + "\n-------------------------------------------------\nFan_GPU1.T1=" + Fan_GPU1.T1 + "\nFan_GPU1.T2=" + Fan_GPU1.T2 + "\nFan_GPU1.T3=" + Fan_GPU1.T3 + "\nFan_GPU1.T4=" + Fan_GPU1.T4 + "\nFan_GPU1.T2_Default=" + Fan_GPU1.T2_Default + "\nFan_GPU1.T3_Default=" + Fan_GPU1.T3_Default + "\nFan_GPU1.D1=" + Fan_GPU1.D1 + "\nFan_GPU1.D2=" + Fan_GPU1.D2 + "\nFan_GPU1.D3=" + Fan_GPU1.D3 + "\nFan_GPU1.D4=" + Fan_GPU1.D4 + "\nFan_GPU1.D2_Default=" + Fan_GPU1.D2_Default + "\n Fan_GPU1.D3_Default=" + Fan_GPU1.D3_Default;
		}
		if (FanCount == 3)
		{
			text = text + "\n-------------------------------------------------\nFan_GPU2.T1=" + Fan_GPU2.T1 + "\nFan_GPU2.T2=" + Fan_GPU2.T2 + "\nFan_GPU2.T3=" + Fan_GPU2.T3 + "\nFan_GPU2.T4=" + Fan_GPU2.T4 + "\nFan_GPU2.T2_Default=" + Fan_GPU2.T2_Default + "\nFan_GPU2.T3_Default=" + Fan_GPU2.T3_Default + "\nFan_GPU2.D1=" + Fan_GPU2.D1 + "\nFan_GPU2.D2=" + Fan_GPU2.D2 + "\nFan_GPU2.D3=" + Fan_GPU2.D3 + "\nFan_GPU2.D4=" + Fan_GPU2.D4 + "\nFan_GPU2.D2_Default=" + Fan_GPU2.D2_Default + "\n Fan_GPU2.D3_Default=" + Fan_GPU2.D3_Default;
		}
		if (FanCount != 0)
		{
			DebugLog.WriteDbgLog2Outlog(text);
		}
		if (Global.features.SupportAntiDust_Fan)
		{
			UpdateSystemTime();
			AntiDust_RunStatus = array[80];
			AntiDust_Scheduler_status = array[81];
			if (AntiDust_Scheduler_status == 0)
			{
				SetAntiDust_Scheduler(1, 127, 0);
				return;
			}
			AntiDust_S_Day = array[82] & 0x7F;
			AntiDust_S_Resume = array[82] >> 7;
			AntiDust_S_Hour = array[83];
		}
	}

	public void Load_Rxx()
	{
		Fan_CPU.R12 = (int)Math.Round((double)(Fan_CPU.D2 - Fan_CPU.D1) / (double)(Fan_CPU.T2 - Fan_CPU.T1) * 2.55 * 16.0, 0);
		Fan_CPU.R23 = (int)Math.Round((double)(Fan_CPU.D3 - Fan_CPU.D2) / (double)(Fan_CPU.T3 - Fan_CPU.T2) * 2.55 * 16.0, 0);
		Fan_CPU.R34 = (int)Math.Round((double)(Fan_CPU.D4 - Fan_CPU.D3) / (double)(Fan_CPU.T4 - Fan_CPU.T3) * 2.55 * 16.0, 0);
		Fan_GPU1.R12 = (int)Math.Round((double)(Fan_GPU1.D2 - Fan_GPU1.D1) / (double)(Fan_GPU1.T2 - Fan_GPU1.T1) * 2.55 * 16.0, 0);
		Fan_GPU1.R23 = (int)Math.Round((double)(Fan_GPU1.D3 - Fan_GPU1.D2) / (double)(Fan_GPU1.T3 - Fan_GPU1.T2) * 2.55 * 16.0, 0);
		Fan_GPU1.R34 = (int)Math.Round((double)(Fan_GPU1.D4 - Fan_GPU1.D3) / (double)(Fan_GPU1.T4 - Fan_GPU1.T3) * 2.55 * 16.0, 0);
		Fan_GPU2.R12 = (int)Math.Round((double)(Fan_GPU2.D2 - Fan_GPU2.D1) / (double)(Fan_GPU2.T2 - Fan_GPU2.T1) * 2.55 * 16.0, 0);
		Fan_GPU2.R23 = (int)Math.Round((double)(Fan_GPU2.D3 - Fan_GPU2.D2) / (double)(Fan_GPU2.T3 - Fan_GPU2.T2) * 2.55 * 16.0, 0);
		Fan_GPU2.R34 = (int)Math.Round((double)(Fan_GPU2.D4 - Fan_GPU2.D3) / (double)(Fan_GPU2.T4 - Fan_GPU2.T3) * 2.55 * 16.0, 0);
	}

	public void SetFanInfo()
	{
		byte[] array = new byte[256];
		array[0] = (byte)Global.fan.FanAPPVersion_Major;
		array[1] = (byte)Global.fan.FanAPPVersion_Minor;
		array[2] = (byte)Global.fan.FanAPPVersion_Build;
		array[3] = (byte)Global.fan.FanAPPVersion_Revision;
		array[4] = (byte)InitFanMode;
		array[5] = (byte)FanMode;
		array[6] = (byte)FanCount;
		array[7] = (byte)FanOffset;
		array[8] = (byte)TurboFanStatus;
		if (SupportCustomFan)
		{
			array[16] = Fan_CPU.D1;
			array[17] = Fan_CPU.D2;
			array[18] = Fan_CPU.D3;
			array[19] = 100;
			array[20] = Fan_CPU.D2_Default;
			array[21] = Fan_CPU.D3_Default;
			array[22] = Fan_CPU.T1;
			array[23] = Fan_CPU.T2;
			array[24] = Fan_CPU.T3;
			array[25] = 100;
			array[26] = Fan_CPU.T2_Default;
			array[27] = Fan_CPU.T3_Default;
			array[28] = (byte)(Fan_CPU.R12 & 0xFF);
			array[29] = (byte)(Fan_CPU.R12 >> 8);
			array[30] = (byte)(Fan_CPU.R23 & 0xFF);
			array[31] = (byte)(Fan_CPU.R23 >> 8);
			array[32] = (byte)(Fan_CPU.R34 & 0xFF);
			array[33] = (byte)(Fan_CPU.R34 >> 8);
			array[34] = Fan_GPU1.D1;
			array[35] = Fan_GPU1.D2;
			array[36] = Fan_GPU1.D3;
			array[37] = 100;
			array[38] = Fan_GPU1.D2_Default;
			array[39] = Fan_GPU1.D3_Default;
			array[40] = Fan_GPU1.T1;
			array[41] = Fan_GPU1.T2;
			array[42] = Fan_GPU1.T3;
			array[43] = 100;
			array[44] = Fan_GPU1.T2_Default;
			array[45] = Fan_GPU1.T3_Default;
			array[46] = (byte)(Fan_GPU1.R12 & 0xFF);
			array[47] = (byte)(Fan_GPU1.R12 >> 8);
			array[48] = (byte)(Fan_GPU1.R23 & 0xFF);
			array[49] = (byte)(Fan_GPU1.R23 >> 8);
			array[50] = (byte)(Fan_GPU1.R34 & 0xFF);
			array[51] = (byte)(Fan_GPU1.R34 >> 8);
			array[52] = Fan_GPU2.D1;
			array[53] = Fan_GPU2.D2;
			array[54] = Fan_GPU2.D3;
			array[55] = 100;
			array[56] = Fan_GPU2.D2_Default;
			array[57] = Fan_GPU2.D3_Default;
			array[58] = Fan_GPU2.T1;
			array[59] = Fan_GPU2.T2;
			array[60] = Fan_GPU2.T3;
			array[61] = 100;
			array[62] = Fan_GPU2.T2_Default;
			array[63] = Fan_GPU2.T3_Default;
			array[64] = (byte)(Fan_GPU2.R12 & 0xFF);
			array[65] = (byte)(Fan_GPU2.R12 >> 8);
			array[66] = (byte)(Fan_GPU2.R23 & 0xFF);
			array[67] = (byte)(Fan_GPU2.R23 >> 8);
			array[68] = (byte)(Fan_GPU2.R34 & 0xFF);
			array[69] = (byte)(Fan_GPU2.R34 >> 8);
			DebugLog.WriteDbgLog2Outlog("Set APP Data \nFan_CPU.T1=" + Fan_CPU.T1 + "\nFan_CPU.T2=" + Fan_CPU.T2 + "\nFan_CPU.T3=" + Fan_CPU.T3 + "\nFan_CPU.T4=" + Fan_CPU.T4 + "\nFan_CPU.T2_Default=" + Fan_CPU.T2_Default + "\nFan_CPU.T3_Default=" + Fan_CPU.T3_Default + "\nFan_CPU.D1=" + Fan_CPU.D1 + "\nFan_CPU.D2=" + Fan_CPU.D2 + "\nFan_CPU.D3=" + Fan_CPU.D3 + "\nFan_CPU.D4=" + Fan_CPU.D4 + "\nFan_CPU.D2_Default=" + Fan_CPU.D2_Default + "\nFan_CPU.D3_Default=" + Fan_CPU.D3_Default + "\n-------------------------------------------------\nFan_GPU1.T1=" + Fan_GPU1.T1 + "\nFan_GPU1.T2=" + Fan_GPU1.T2 + "\nFan_GPU1.T3=" + Fan_GPU1.T3 + "\nFan_GPU1.T4=" + Fan_GPU1.T4 + "\nFan_GPU1.T2_Default=" + Fan_GPU1.T2_Default + "\nFan_GPU1.T3_Default=" + Fan_GPU1.T3_Default + "\nFan_GPU1.D1=" + Fan_GPU1.D1 + "\nFan_GPU1.D2=" + Fan_GPU1.D2 + "\nFan_GPU1.D3=" + Fan_GPU1.D3 + "\nFan_GPU1.D4=" + Fan_GPU1.D4 + "\nFan_GPU1.D2_Default=" + Fan_GPU1.D2_Default + "\n Fan_GPU1.D3_Default=" + Fan_GPU1.D3_Default + "\n-------------------------------------------------\nFan_GPU2.T1=" + Fan_GPU2.T1 + "\nFan_GPU2.T2=" + Fan_GPU2.T2 + "\nFan_GPU2.T3=" + Fan_GPU2.T3 + "\nFan_GPU2.T4=" + Fan_GPU2.T4 + "\nFan_GPU2.T2_Default=" + Fan_GPU2.T2_Default + "\nFan_GPU2.T3_Default=" + Fan_GPU2.T3_Default + "\nFan_GPU2.D1=" + Fan_GPU2.D1 + "\nFan_GPU2.D2=" + Fan_GPU2.D2 + "\nFan_GPU2.D3=" + Fan_GPU2.D3 + "\nFan_GPU2.D4=" + Fan_GPU2.D4 + "\nFan_GPU2.D2_Default=" + Fan_GPU2.D2_Default + "\n Fan_GPU2.D3_Default=" + Fan_GPU2.D3_Default);
		}
		Global.dchu.WriteAppSettings(4, 0, 256, array);
	}

	public void SetFanOffset(byte offset)
	{
		int data = (int)Math.Round(255.0 * ((double)(int)offset / 100.0), 0);
		Global.dchu.SetWMI(121, 14, (uint)data);
		byte[] buffer = new byte[1] { offset };
		Global.dchu.WriteAppSettings(4, 7, 1, buffer);
	}

	public void Load_FanMode_Info()
	{
		Read_FanInfo();
		if (Fan_CPU.D1 == 0 && Fan_CPU.T1 == 0)
		{
			Global.dchu.SetWMI(121, 34, 1u);
			Read_WMI13();
			SetFanInfo();
		}
		SetFanOffset((byte)FanOffset);
		if (SupportCustomFan)
		{
			SetCustomFanTable();
		}
	}

	public void Load_FanMode_forFn_3(int SituationalMode)
	{
		byte[] array = new byte[1];
		switch (SituationalMode)
		{
		case 0:
			array[0] = 0;
			break;
		case 1:
			array[0] = 0;
			break;
		case 2:
			array[0] = 0;
			break;
		case 3:
			if (Support_MaxQ)
			{
				array[0] = 5;
			}
			else
			{
				array[0] = 0;
			}
			break;
		}
		Global.dchu.WriteAppSettings(4, 5, 1, array);
	}

	public void KeepFanMode(int SituationalMode)
	{
		if (FanMode == 6)
		{
			Global.dchu.SetWMI(121, 1, (uint)FanMode);
			byte[] buffer = new byte[1] { (byte)FanMode };
			Global.dchu.WriteAppSettings(4, 5, 1, buffer);
			DebugLog.WriteDbgLog2Outlog("SetFanMode: 6(Custom)");
		}
	}

	public void Fn1_ChangeFanMode(byte mode)
	{
		byte[] buffer = new byte[1] { mode };
		Global.dchu.WriteAppSettings(4, 5, 1, buffer);
	}

	public void RunAntiDustMode()
	{
		Global.dchu.SetWMI(121, 41, 1u);
		AntiDust_Now();
	}

	public void AntiDust_Now()
	{
		AntiDust_RunStatus = 1;
		byte[] buffer = new byte[1] { 1 };
		Global.dchu.WriteAppSettings(4, 80, 1, buffer);
	}

	public void AntiDust_Done()
	{
		AntiDust_RunStatus = 0;
		byte[] buffer = new byte[1] { 0 };
		Global.dchu.WriteAppSettings(4, 80, 1, buffer);
	}

	public void SetAntiDust_Scheduler(int Resume, int Day, int Hour)
	{
		int data = Day + (Resume << 7) + (Hour << 8);
		Global.dchu.SetWMI(121, 40, (uint)data);
		AntiDust_S_Day = Day;
		AntiDust_S_Resume = Resume;
		AntiDust_S_Hour = Hour;
		byte[] buffer = new byte[5]
		{
			1,
			(byte)(Day + (Resume << 7)),
			(byte)Hour,
			0,
			0
		};
		Global.dchu.WriteAppSettings(4, 81, 5, buffer);
	}

	public void UpdateSystemTime()
	{
		int dayOfWeek = (int)DateTime.Now.DayOfWeek;
		int data = DateTime.Now.Second + (DateTime.Now.Minute << 6) + (DateTime.Now.Hour << 12) + (dayOfWeek << 17);
		Global.dchu.SetWMI(118, 1, (uint)data);
	}
}
