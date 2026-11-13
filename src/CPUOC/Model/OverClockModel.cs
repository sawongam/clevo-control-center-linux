using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CPUOC.Model;
using CPUOC30;
using Clevo;
using GalaSoft.MvvmLight;
using RippleCC.Platform;

namespace CPUOC30.Model;

internal class OverClockModel : ObservableObject
{
	public struct ClevoMemory
	{
		public ulong Capacity;

		public string Manufacturer;

		public int Speed;
	}

	public struct XMP
	{
		public uint tCL;

		public uint tRCD_tRP;

		public uint tRAS;

		public uint tCWL;

		public uint tFAW;

		public uint tREFI;

		public uint tRFC;

		public uint tRRD;

		public uint tRTP;

		public uint tWR;

		public uint tWTR;

		public uint tVoltage;
	}

	public struct MemoryData
	{
		public ushort version;

		public ushort Voltage_Uint;

		public ushort Memory_Current_Voltage;

		public ushort Memory_Default_Voltage;

		public List<ushort> Memory_Voltage_Support_Value;

		public XMP[] Memory_XMP_Profile;

		public string size;

		public int speed_max;

		public int current_freq_speed;

		public int Current_usage;
	}

	private DCHUModel dchuModel = new DCHUModel();

	private ClevoMemory MyClevoMemory;

	public MemoryData myMemoryData;

	public string model_name;

	public int turbo_min;

	public int turbo_max;

	public int memory_speed_max;

	public decimal reference;

	private ushort Memory_Version_From_WMI18;

	private ushort Memory_Voltage_Uint_From_WMI18;

	private ushort Memory_Current_Voltage_From_WMI18;

	private ushort Memory_Default_Voltage_From_WMI18;

	private List<ushort> Memory_Voltage_Support_Value_From_WMI18;

	private XMP[] Memory_XMP_Profile_From_WMI18;

	private int PL1_Default_From_WMI18;

	private int PL2_Default_From_WMI18;

	private int CPU_default_voltage_offset_From_WMI18;

	private int CPU_default_voltage_GT_offset_From_WMI18;

	private uint PLTime_Default_From_WMI18;

	private uint VR_Default_From_WMI18;

	private bool Default_VRAuto_From_WMI18;

	private uint default_Ratio1_From_WMI18;

	private uint default_Ratio2_From_WMI18;

	private uint default_Ratio3_From_WMI18;

	private uint default_Ratio4_From_WMI18;

	private uint default_Ratio5_From_WMI18;

	private uint default_Ratio6_From_WMI18;

	private uint default_Ratio7_From_WMI18;

	private uint default_Ratio8_From_WMI18;

	private uint default_Ratio9_From_WMI18;

	private uint default_Ratio10_From_WMI18;

	private int PL1_Min_From_XTU;

	private int PL1_Default_From_XTU;

	private float PLTime_Default_From_XTU;

	private int PL2_Default_From_XTU;

	private bool CPU_VR_Auto_Default_From_XTU;

	private int CPU_VR_Default_From_XTU;

	private int Voltage_Override_Default_From_XTU;

	public int Default_PL1;

	public float Default_PLTime;

	public int Default_PL2;

	public bool Default_CPU_VR_Auto;

	public int Default_CPU_VR;

	public int Default_Voltage_Offset;

	public float Default_Voltage_Override;

	public int Default_Voltage_GT;

	public Dictionary<decimal, decimal> Default_Ratio = new Dictionary<decimal, decimal>();

	public int Current_PL1;

	public float Current_PLTime;

	public int Current_PL2;

	public bool Current_CPU_VR_Auto;

	public int Current_CPU_VR;

	public int Current_Voltage_Offset;

	public float Current_Voltage_Override;

	public int Current_Voltage_GT;

	public Dictionary<decimal, decimal> Current_Ratio = new Dictionary<decimal, decimal>();

	public List<ClientTuningControl> TuningControls { get; set; }

	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);

	public OverClockModel()
	{
		SetAppVersion();
		TuningControls = XtuStub.Controls.FindAll(x => x.Enabled);
	}

	private void SetAppVersion()
	{
		int major = Assembly.GetExecutingAssembly().GetName().Version.Major;
		int minor = Assembly.GetExecutingAssembly().GetName().Version.Minor;
		int build = Assembly.GetExecutingAssembly().GetName().Version.Build;
		int revision = Assembly.GetExecutingAssembly().GetName().Version.Revision;
		byte[] bytes = BitConverter.GetBytes(major);
		DCHU.WriteAppSettings(0, 28, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(minor);
		DCHU.WriteAppSettings(0, 29, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(build);
		DCHU.WriteAppSettings(0, 30, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(revision);
		DCHU.WriteAppSettings(0, 31, 1, ref bytes[0]);
	}

	public void Initial()
	{
		GetMemoryInfo();
		Read_package_WMI18();
		Read_Value_FromXTU();
		Sorting_Default_Info();
		Sorting_Current_Info();
		foreach (KeyValuePair<decimal, decimal> item in Current_Ratio)
		{
			if (item.Key == 29m)
			{
				turbo_max = (int)(item.Value * reference);
			}
		}
	}

	public List<string> GetPlatformCompatibilityIssues()
	{
		return new List<string>();
	}

	// Win32_PhysicalMemory (WMI) -> /proc/meminfo; speed is not readable without root (dmidecode), stubbed as DDR4-3200
	private void GetMemoryInfo()
	{
		ulong num = (ulong)LinuxSensors.MemTotalMB();
		MyClevoMemory.Speed = 3200;
		MyClevoMemory.Capacity = num;
		myMemoryData.size = Math.Round((double)num / 1024.0, 2).ToString("#0.##");
	}

	private void Read_package_WMI18()
	{
		string text = "";
		uint num = 0u;
		int num2 = 0;
		try
		{
			byte[] array = new byte[256];
			DCHU.GetDCHU_Data_Buffer(18, ref array[0]);
			Memory_Version_From_WMI18 = (ushort)((array[1] << 8) + array[0]);
			uint num3 = (uint)((array[11] << 24) + (array[10] << 16) + (array[9] << 8) + array[8]);
			if (num3 > 1000000)
			{
				PL1_Default_From_WMI18 = Convert.ToInt32(num3 / 1000000);
			}
			else if (num3 > 1000)
			{
				PL1_Default_From_WMI18 = Convert.ToInt32(num3 / 1000);
			}
			else
			{
				PL1_Default_From_WMI18 = Convert.ToInt32(num3);
			}
			text = text + "CPUOC, Read_package_WMI18, PL1 temp=" + num3 + ", ocInfo.PL1_Default_From_WMI18=" + PL1_Default_From_WMI18 + "\r\n";
			num3 = (uint)((array[15] << 24) + (array[14] << 16) + (array[13] << 8) + array[12]);
			if (num3 > 1000000)
			{
				PL2_Default_From_WMI18 = Convert.ToInt32(num3 / 1000000);
			}
			else if (num3 > 1000)
			{
				PL2_Default_From_WMI18 = Convert.ToInt32(num3 / 1000);
			}
			else
			{
				PL2_Default_From_WMI18 = Convert.ToInt32(num3);
			}
			text = text + "CPUOC, Read_package_WMI18, PL2 temp=" + num3 + ", ocInfo.PL2_Default_From_WMI18=" + PL2_Default_From_WMI18 + "\r\n";
			Memory_Voltage_Uint_From_WMI18 = (ushort)((array[17] << 8) + array[16]);
			Memory_Current_Voltage_From_WMI18 = (ushort)((array[19] << 8) + array[18]);
			Memory_Default_Voltage_From_WMI18 = (ushort)((array[21] << 8) + array[20]);
			Memory_Voltage_Support_Value_From_WMI18 = new List<ushort>();
			for (int i = 22; i < 46; i += 2)
			{
				ushort num4 = (ushort)((array[i + 1] << 8) + array[i]);
				if (num4 != 0)
				{
					Memory_Voltage_Support_Value_From_WMI18.Add(num4);
				}
			}
			Memory_XMP_Profile_From_WMI18 = new XMP[4];
			int[] array2 = new int[4] { 48, 80, 112, 144 };
			for (int j = 0; j < 4; j++)
			{
				int num5 = array2[j];
				Memory_XMP_Profile_From_WMI18[j].tCL = (ushort)((array[num5 + 1] << 8) + array[num5]);
				Memory_XMP_Profile_From_WMI18[j].tRCD_tRP = (ushort)((array[num5 + 3] << 8) + array[num5 + 2]);
				Memory_XMP_Profile_From_WMI18[j].tRAS = (ushort)((array[num5 + 5] << 8) + array[num5 + 4]);
				Memory_XMP_Profile_From_WMI18[j].tCWL = (ushort)((array[num5 + 7] << 8) + array[num5 + 6]);
				Memory_XMP_Profile_From_WMI18[j].tFAW = (ushort)((array[num5 + 9] << 8) + array[num5 + 8]);
				Memory_XMP_Profile_From_WMI18[j].tREFI = (ushort)((array[num5 + 11] << 8) + array[num5 + 10]);
				Memory_XMP_Profile_From_WMI18[j].tRFC = (ushort)((array[num5 + 13] << 8) + array[num5 + 12]);
				Memory_XMP_Profile_From_WMI18[j].tRRD = (ushort)((array[num5 + 15] << 8) + array[num5 + 14]);
				Memory_XMP_Profile_From_WMI18[j].tRTP = (ushort)((array[num5 + 17] << 8) + array[num5 + 16]);
				Memory_XMP_Profile_From_WMI18[j].tWR = (ushort)((array[num5 + 19] << 8) + array[num5 + 18]);
				Memory_XMP_Profile_From_WMI18[j].tWTR = (ushort)((array[num5 + 21] << 8) + array[num5 + 20]);
				num = (ushort)((array[num5 + 23] << 8) + array[num5 + 22]);
				if (num == 0)
				{
					num = 1200u;
				}
				Memory_XMP_Profile_From_WMI18[j].tVoltage = num;
			}
			num3 = (uint)((array[177] << 8) + array[176]);
			if (((array[177] >> 7) & 1) == 1)
			{
				uint cPU_default_voltage_offset_From_WMI = num3 ^ 0x8000;
				CPU_default_voltage_offset_From_WMI18 = (int)cPU_default_voltage_offset_From_WMI;
			}
			else
			{
				CPU_default_voltage_offset_From_WMI18 = (int)num3 * -1;
			}
			text = text + "CPUOC_BG, Read_package_WMI18, CPU Volt Offset Default temp=" + num3 + ", ocInfo.CPU_default_voltage_offset_From_WMI18=" + CPU_default_voltage_offset_From_WMI18 + "\r\n";
			num3 = (uint)((array[181] << 8) + array[180]);
			CPU_default_voltage_GT_offset_From_WMI18 = (int)num3 * -1;
			text = text + "CPUOC, Read_package_WMI18, GT Volt Offset Default temp=" + num3 + ", ocInfo.CPU_default_voltage_GT_offset_From_WMI18=" + CPU_default_voltage_GT_offset_From_WMI18 + "\r\n";
			num3 = (PLTime_Default_From_WMI18 = (uint)((array[220] << 8) + array[219]));
			text = text + "CPUOC, Read_package_WMI18, PL Time temp=" + num3 + ", ocInfo.PLTime_Default_From_WMI18=" + PLTime_Default_From_WMI18 + "\r\n";
			num3 = (VR_Default_From_WMI18 = (uint)((array[224] << 24) + (array[223] << 16) + (array[222] << 8) + array[221]));
			text = text + "CPUOC, Read_package_WMI18, VR temp=" + num3 + ", ocInfo.VR_Default_From_WMI18=" + VR_Default_From_WMI18 + "\r\n";
			if (VR_Default_From_WMI18 == uint.MaxValue)
			{
				Default_VRAuto_From_WMI18 = true;
			}
			num3 = array[208];
			default_Ratio1_From_WMI18 = num3;
			num3 = array[209];
			default_Ratio2_From_WMI18 = num3;
			num3 = array[210];
			default_Ratio3_From_WMI18 = num3;
			num3 = array[211];
			default_Ratio4_From_WMI18 = num3;
			num3 = array[213];
			default_Ratio5_From_WMI18 = num3;
			num3 = array[214];
			default_Ratio6_From_WMI18 = num3;
			num3 = array[215];
			default_Ratio7_From_WMI18 = num3;
			num3 = array[216];
			default_Ratio8_From_WMI18 = num3;
			num3 = array[217];
			default_Ratio9_From_WMI18 = num3;
			num3 = array[218];
			default_Ratio10_From_WMI18 = num3;
		}
		catch (IOException ex)
		{
			if (ex.Source != null)
			{
				DebugLog.WriteDbgLog2Inlog("CPUOC, Read_package_WMI18 IOException: " + ex.Message);
			}
			throw;
		}
	}

	private void Read_Value_FromXTU()
	{
		List<decimal> list = new List<decimal>();
		model_name = LinuxSensors.CpuBrand();
		decimal num = ((TuningControls.Find((ClientTuningControl x) => x.Id == 0) == null) ? ((decimal)GetCPU_Max_NonTurbo()) : TuningControls.Find((ClientTuningControl x) => x.Id == 0).ActiveValue);
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 1) != null)
		{
			reference = TuningControls.Find((ClientTuningControl x) => x.Id == 1).ActiveValue;
		}
		else if (true) // XTU monitor 15 (bclk): stub 100 MHz
		{
			reference = Convert.ToDecimal("100");
		}
		if (reference == 0m)
		{
			reference = 100m;
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 0) != null)
		{
			turbo_min = (int)Math.Round(reference * num, 0);
		}
		else
		{
			turbo_min = (int)num / 100 * 100;
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 48) != null)
		{
			list = TuningControls.Find((ClientTuningControl x) => x.Id == 48).SupportedValues;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 48).DefaultValue;
			List<decimal> list2 = new List<decimal>();
			for (int num2 = 0; num2 < list.Count; num2++)
			{
				if (list[num2].ToString().Equals("79228162514264337593543950335"))
				{
					list.RemoveAt(num2);
				}
			}
			PL1_Min_From_XTU = (int)list[0];
			PL1_Default_From_XTU = (int)defaultValue;
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 48 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 66) != null)
		{
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 66).DefaultValue;
			PLTime_Default_From_XTU = (int)defaultValue;
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 66 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 49) != null)
		{
			if (TuningControls.Find((ClientTuningControl x) => x.Id == 47) != null)
			{
				decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 47).DefaultValue;
				PL2_Default_From_XTU = (int)defaultValue;
			}
			else
			{
				DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 47 == null");
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 49 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 102) != null)
		{
			list = TuningControls.Find((ClientTuningControl x) => x.Id == 102).SupportedValues;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 102).DefaultValue;
			bool cPU_VR_Auto_Default_From_XTU = false;
			for (int num3 = 0; num3 < list.Count; num3++)
			{
				if (list[num3].ToString().Equals("79228162514264337593543950335"))
				{
					cPU_VR_Auto_Default_From_XTU = true;
				}
			}
			CPU_VR_Auto_Default_From_XTU = cPU_VR_Auto_Default_From_XTU;
			CPU_VR_Default_From_XTU = (int)defaultValue;
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 102 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 2) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 2).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 2).DefaultValue;
			if (activeValue == 0m)
			{
				Voltage_Override_Default_From_XTU = 0;
			}
			else
			{
				Voltage_Override_Default_From_XTU = (int)activeValue;
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x02 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 29) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 29).ActiveValue;
			list = TuningControls.Find((ClientTuningControl x) => x.Id == 29).SupportedValues;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 29).DefaultValue;
			if (default_Ratio1_From_WMI18 != 0)
			{
				Default_Ratio.Add(29m, default_Ratio1_From_WMI18);
				turbo_max = (int)Math.Round(reference * (decimal)default_Ratio1_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(29m, defaultValue);
				turbo_max = (int)Math.Round(reference * defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 29 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 30) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 30).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 30).DefaultValue;
			if (default_Ratio2_From_WMI18 != 0)
			{
				Default_Ratio.Add(30m, default_Ratio2_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(30m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 30 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 31) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 31).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 31).DefaultValue;
			if (default_Ratio3_From_WMI18 != 0)
			{
				Default_Ratio.Add(31m, default_Ratio3_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(31m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 31 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 32) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 32).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 32).DefaultValue;
			if (default_Ratio4_From_WMI18 != 0)
			{
				Default_Ratio.Add(32m, default_Ratio4_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(32m, activeValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 32 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 42) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 42).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 42).DefaultValue;
			if (default_Ratio5_From_WMI18 != 0)
			{
				Default_Ratio.Add(42m, default_Ratio5_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(42m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x2A == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 43) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 43).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 43).DefaultValue;
			if (default_Ratio6_From_WMI18 != 0)
			{
				Default_Ratio.Add(43m, default_Ratio6_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(43m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x2B == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 96) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 96).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 96).DefaultValue;
			if (default_Ratio7_From_WMI18 != 0)
			{
				Default_Ratio.Add(96m, default_Ratio7_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(96m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x60 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 97) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 97).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 97).DefaultValue;
			if (default_Ratio8_From_WMI18 != 0)
			{
				Default_Ratio.Add(97m, default_Ratio8_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(97m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x61 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 107) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 107).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 107).DefaultValue;
			if (default_Ratio9_From_WMI18 != 0)
			{
				Default_Ratio.Add(107m, default_Ratio9_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(107m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x6b == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 108) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 108).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 108).DefaultValue;
			if (default_Ratio10_From_WMI18 != 0)
			{
				Default_Ratio.Add(107m, default_Ratio9_From_WMI18);
			}
			else
			{
				Default_Ratio.Add(107m, defaultValue);
			}
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x6c == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4500) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4500).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4500).DefaultValue;
			Default_Ratio.Add(4500m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x1194 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4501) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4501).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4501).DefaultValue;
			Default_Ratio.Add(4501m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x1195 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4502) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4502).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4502).DefaultValue;
			Default_Ratio.Add(4502m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x1196 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4503) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4503).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4503).DefaultValue;
			Default_Ratio.Add(4503m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x1197 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4504) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4504).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4504).DefaultValue;
			Default_Ratio.Add(4504m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x1198 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4505) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4505).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4505).DefaultValue;
			Default_Ratio.Add(4505m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x1199 == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4506) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4506).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4506).DefaultValue;
			Default_Ratio.Add(4506m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x119a == null");
		}
		if (TuningControls.Find((ClientTuningControl x) => x.Id == 4507) != null)
		{
			decimal activeValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4507).ActiveValue;
			decimal defaultValue = TuningControls.Find((ClientTuningControl x) => x.Id == 4507).DefaultValue;
			Default_Ratio.Add(4507m, defaultValue);
		}
		else
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, LoadTuningControl, x.Id == 0x119b == null");
		}
	}

	private int GetCPU_Max_NonTurbo()
	{
		return 2300; // CIM_Processor.MaxClockSpeed; only used when XTU control 0 is missing
	}

	private void Sorting_Default_Info()
	{
		if (PL1_Default_From_WMI18 != 0)
		{
			Default_PL1 = PL1_Default_From_WMI18;
		}
		else
		{
			Default_PL1 = PL1_Default_From_XTU;
		}
		if (PLTime_Default_From_WMI18 != 0)
		{
			Default_PLTime = PLTime_Default_From_WMI18;
		}
		else
		{
			Default_PLTime = PLTime_Default_From_XTU;
		}
		if (PL2_Default_From_WMI18 != 0)
		{
			Default_PL2 = PL2_Default_From_WMI18;
		}
		else
		{
			Default_PL2 = PL2_Default_From_XTU;
		}
		if (Default_VRAuto_From_WMI18)
		{
			Default_CPU_VR_Auto = true;
		}
		else
		{
			Default_CPU_VR_Auto = CPU_VR_Auto_Default_From_XTU;
		}
		if (VR_Default_From_WMI18 != 0)
		{
			Default_CPU_VR = (int)VR_Default_From_WMI18;
		}
		else
		{
			Default_CPU_VR = CPU_VR_Default_From_XTU;
		}
		if (CPU_default_voltage_offset_From_WMI18 != 0)
		{
			Default_Voltage_Offset = CPU_default_voltage_offset_From_WMI18;
		}
		else
		{
			Default_Voltage_Offset = 0;
		}
		Default_Voltage_Override = Voltage_Override_Default_From_XTU;
		Default_Voltage_GT = CPU_default_voltage_GT_offset_From_WMI18;
	}

	private void Sorting_Current_Info()
	{
		int isSaved = dchuModel.GetIsSaved();
		if (isSaved == 1)
		{
			Current_PL1 = DCHU.GetPowerLimit1CurrentValue();
			Current_PL2 = DCHU.GetPowerLimit2CurrentValue();
			Current_PLTime = DCHU.GetPowerLimitTimeValue();
			if (DCHU.GetCPUVRAuto() == 1)
			{
				Current_CPU_VR_Auto = true;
			}
			else
			{
				Current_CPU_VR_Auto = false;
			}
			Current_CPU_VR = DCHU.GetCPUVRCurrentValue();
			Current_Voltage_Offset = DCHU.GetCPUVoltageOffset();
			Current_Voltage_Override = DCHU.GetOverrideVoltageValue();
			Current_Voltage_GT = DCHU.GetGraphicsVoltage();
			GetCurrentRatioFromInsydeBuffer();
		}
		else
		{
			Current_PL1 = Default_PL1;
			Current_PL2 = Default_PL2;
			Current_PLTime = Default_PLTime;
			Current_CPU_VR_Auto = Default_CPU_VR_Auto;
			Current_CPU_VR = Default_CPU_VR;
			Current_Voltage_Offset = Default_Voltage_Offset;
			Current_Voltage_Override = Default_Voltage_Override;
			Current_Voltage_GT = Default_Voltage_GT;
			Current_Ratio = Default_Ratio;
		}
	}

	private void GetCurrentRatioFromInsydeBuffer()
	{
		int num = 0;
		num = DCHU.GetCPUCoreRatioCurrentValue(29);
		Current_Ratio.Add(29m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(30);
		Current_Ratio.Add(30m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(31);
		Current_Ratio.Add(31m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(32);
		Current_Ratio.Add(32m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(42);
		Current_Ratio.Add(42m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(43);
		Current_Ratio.Add(43m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(96);
		Current_Ratio.Add(96m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(97);
		Current_Ratio.Add(97m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(107);
		Current_Ratio.Add(107m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(108);
		Current_Ratio.Add(108m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4500);
		Current_Ratio.Add(4500m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4501);
		Current_Ratio.Add(4501m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4502);
		Current_Ratio.Add(4502m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4503);
		Current_Ratio.Add(4503m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4504);
		Current_Ratio.Add(4504m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4505);
		Current_Ratio.Add(4505m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4506);
		Current_Ratio.Add(4506m, num);
		num = DCHU.GetCPUCoreRatioCurrentValue(4507);
		Current_Ratio.Add(4507m, num);
	}

	public int GetCPUTotalUsage()
	{
		return LinuxSensors.CpuUsage();
	}

	public List<decimal> GetScaleDataList(int dataType)
	{
		List<decimal> list = new List<decimal>();
		list.Clear();
		switch (dataType)
		{
		case 1:
		{
			int pL1_Min_From_XTU = PL1_Min_From_XTU;
			int num4 = 330;
			for (int num5 = pL1_Min_From_XTU; num5 <= num4; num5++)
			{
				decimal item2 = Convert.ToDecimal(num5);
				list.Add(item2);
			}
			break;
		}
		case 2:
		{
			List<decimal> supportedValues = new List<decimal>();
			supportedValues = TuningControls.Find((ClientTuningControl x) => x.Id == 66).SupportedValues;
			if (supportedValues == null)
			{
				DebugLog.WriteDbgLog2Inlog("CPUOC, GetDataList, x.Id == 66 == null");
				return null;
			}
			for (int num9 = 0; num9 < supportedValues.Count; num9++)
			{
				if (supportedValues[num9].ToString() != "79228162514264337593543950335")
				{
					list.Add(supportedValues[num9]);
				}
			}
			break;
		}
		case 3:
		{
			int num10 = -50;
			int num11 = 255;
			for (int num12 = num10; num12 <= num11; num12++)
			{
				decimal item4 = Convert.ToDecimal(num12);
				list.Add(item4);
			}
			break;
		}
		case 4:
		{
			int num6 = 100;
			int num7 = -200;
			for (int num8 = num7; num8 <= num6; num8 += 10)
			{
				decimal item3 = Convert.ToDecimal(num8);
				list.Add(item3);
			}
			break;
		}
		case 5:
		{
			int num = 100;
			int num2 = -200;
			for (int num3 = num2; num3 <= num; num3 += 10)
			{
				decimal item = Convert.ToDecimal(num3);
				list.Add(item);
			}
			break;
		}
		case 6:
			if (TuningControls.Find((ClientTuningControl x) => x.Id == 29) != null)
			{
				List<decimal> supportedValues = TuningControls.Find((ClientTuningControl x) => x.Id == 29).SupportedValues;
				foreach (decimal item5 in supportedValues)
				{
					list.Add(item5);
				}
			}
			else if (TuningControls.Find((ClientTuningControl x) => x.Id == 116) != null)
			{
				List<decimal> supportedValues = TuningControls.Find((ClientTuningControl x) => x.Id == 116).SupportedValues;
				foreach (decimal item6 in supportedValues)
				{
					list.Add(item6);
				}
			}
			else
			{
				DebugLog.WriteDbgLog2Outlog("CPUOC, GetDataList dataType unknown");
			}
			break;
		}
		return list;
	}

	public List<string> GetScaleDataList()
	{
		List<string> list = new List<string>();
		list.Clear();
		float num = 2f;
		float num2 = 0.5f;
		float num3 = num2 * 10f;
		float num4 = num * 10f;
		for (float num5 = num3; num5 <= num4; num5++)
		{
			float num6 = num5 / 10f;
			string item = Convert.ToString(num6);
			list.Add(item);
		}
		list.Insert(0, TranslationLanguage("_Default"));
		return list;
	}

	// XTU3SERVICE does not exist on Linux: XtuStub stands in for it, so it is always "present and running"
	public bool IsXTUServiceExist()
	{
		return true;
	}

	public bool IsXTUServiceRunning()
	{
		return true;
	}

	public string TranslationLanguage(string Text)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = "null";
		int bufferSize = 255;
		string filePath = Path.Combine(AppContext.BaseDirectory, "mtlanguage.ini");
		GetPrivateProfileString((CleDef.UI_Language)dchuModel.GetUILanguage() switch
		{
			CleDef.UI_Language.system => CultureInfo.CurrentCulture.Name.ToLower(), 
			CleDef.UI_Language.en_us => "en-us", 
			CleDef.UI_Language.en_gb => "en-us", 
			CleDef.UI_Language.es => "es", 
			CleDef.UI_Language.de => "de", 
			CleDef.UI_Language.it => "it", 
			CleDef.UI_Language.ja => "ja", 
			CleDef.UI_Language.ko => "ko", 
			CleDef.UI_Language.zh_cn => "zh-cn", 
			CleDef.UI_Language.zh_tw => "zh-tw", 
			CleDef.UI_Language.fr_fr => "fr-fr", 
			CleDef.UI_Language.fr_ca => "fr-ca", 
			CleDef.UI_Language.pt_br => "pt-br", 
			CleDef.UI_Language.pt_pt => "pt-pt", 
			CleDef.UI_Language.tr_tr => "tr-tr", 
			CleDef.UI_Language.th_th => "th-th", 
			CleDef.UI_Language.vi_vn => "vi-vn", 
			CleDef.UI_Language.ru => "ru", 
			CleDef.UI_Language.ja_ep => "ja-ep", 
			_ => "en-us", 
		}, Text, def, stringBuilder, bufferSize, filePath);
		if (stringBuilder.ToString().Equals("null"))
		{
			GetPrivateProfileString("en-us", Text, def, stringBuilder, bufferSize, filePath);
		}
		return stringBuilder.ToString();
	}
}
