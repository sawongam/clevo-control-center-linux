using System.Collections.Generic;
using CPUOC30.Model;
using GalaSoft.MvvmLight;

namespace CPUOC30.ViewModel;

public class IndexViewModel : ViewModelBase
{
	private enum ScaleDataListType
	{
		PL1PL2 = 1,
		PLTime,
		CPU_VR,
		Voltage_Offset,
		Voltage_Graphics,
		Core_Ratio,
		Voltage_Override
	}

	private string model_name;

	private int turbo_min;

	private int turbo_max;

	private int memory_speed_max;

	private decimal reference;

	private int pl1;

	private int pl2;

	private float pl_time;

	private bool cpu_vr_auto;

	private int cpu_vr;

	private int voltage_offset;

	private float voltage_override;

	private int voltage_gt;

	private int p_core_1;

	private int p_core_2;

	private int p_core_3;

	private int p_core_4;

	private int p_core_5;

	private int p_core_6;

	private int p_core_7;

	private int p_core_8;

	private int p_core_9;

	private int p_core_10;

	private int e_core_1;

	private int e_core_2;

	private int e_core_3;

	private int e_core_4;

	private int e_core_5;

	private int e_core_6;

	private int e_core_7;

	private int e_core_8;

	private int cpu_frequency_per_second;

	private string voltage_per_second;

	private int temperature_per_second;

	private int cpu_usage_per_second;

	private int memory_frequency_per_second;

	public List<decimal> PL1PL2DataList = new List<decimal>();

	public List<decimal> PLTimeDataList = new List<decimal>();

	public List<decimal> VRDataList = new List<decimal>();

	public List<decimal> VoltageOffsetDataList = new List<decimal>();

	public List<decimal> GraphicsVoltageOffsetDataList = new List<decimal>();

	public List<string> OverrideVoltageDataList = new List<string>();

	public List<decimal> CPUCoreDataList = new List<decimal>();

	public bool IsControlCenterControlCPU_UnderVolting;

	public byte Custom_Id;

	private OverClockModel overClockModel = new OverClockModel();

	private DCHUModel dchuModel = new DCHUModel();

	public string Model_Name
	{
		get
		{
			return model_name;
		}
		set
		{
			model_name = value;
			RaisePropertyChanged(() => Model_Name);
		}
	}

	public int Turbo_Min
	{
		get
		{
			return turbo_min;
		}
		set
		{
			turbo_min = value;
			RaisePropertyChanged(() => Turbo_Min);
		}
	}

	public int Turbo_Max
	{
		get
		{
			return turbo_max;
		}
		set
		{
			turbo_max = value;
			RaisePropertyChanged(() => Turbo_Max);
		}
	}

	public int Memory_Speed_Max
	{
		get
		{
			return memory_speed_max;
		}
		set
		{
			memory_speed_max = value;
			RaisePropertyChanged(() => Memory_Speed_Max);
		}
	}

	public decimal Reference
	{
		get
		{
			return reference;
		}
		set
		{
			reference = value;
			RaisePropertyChanged(() => Reference);
		}
	}

	public int PL1
	{
		get
		{
			return pl1;
		}
		set
		{
			pl1 = value;
			RaisePropertyChanged(() => PL1);
		}
	}

	public int Default_PL1 { get; set; }

	public int PL2
	{
		get
		{
			return pl2;
		}
		set
		{
			pl2 = value;
			RaisePropertyChanged(() => PL2);
		}
	}

	public int Default_PL2 { get; set; }

	public float PL_Time
	{
		get
		{
			return pl_time;
		}
		set
		{
			pl_time = value;
			RaisePropertyChanged(() => PL_Time);
		}
	}

	public float Default_PLTime { get; set; }

	public bool CPU_VR_Auto
	{
		get
		{
			return cpu_vr_auto;
		}
		set
		{
			cpu_vr_auto = value;
			RaisePropertyChanged(() => CPU_VR_Auto);
		}
	}

	public bool Default_CPU_VR_Auto { get; set; }

	public int CPU_VR
	{
		get
		{
			return cpu_vr;
		}
		set
		{
			cpu_vr = value;
			RaisePropertyChanged(() => CPU_VR);
		}
	}

	public int Default_CPU_VR { get; set; }

	public int Voltage_Offset
	{
		get
		{
			return voltage_offset;
		}
		set
		{
			voltage_offset = value;
			RaisePropertyChanged(() => Voltage_Offset);
		}
	}

	public int Default_Voltage_Offset { get; set; }

	public float Voltage_Override
	{
		get
		{
			return voltage_override;
		}
		set
		{
			voltage_override = value;
			RaisePropertyChanged(() => Voltage_Override);
		}
	}

	public float Default_Voltage_Override { get; set; }

	public int Voltage_GT
	{
		get
		{
			return voltage_gt;
		}
		set
		{
			voltage_gt = value;
			RaisePropertyChanged(() => Voltage_GT);
		}
	}

	public int Default_Voltage_GT { get; set; }

	public int P_Core_1
	{
		get
		{
			return p_core_1;
		}
		set
		{
			p_core_1 = value;
			RaisePropertyChanged(() => P_Core_1);
		}
	}

	public int P_Core_2
	{
		get
		{
			return p_core_2;
		}
		set
		{
			p_core_2 = value;
			RaisePropertyChanged(() => P_Core_2);
		}
	}

	public int P_Core_3
	{
		get
		{
			return p_core_3;
		}
		set
		{
			p_core_3 = value;
			RaisePropertyChanged(() => P_Core_3);
		}
	}

	public int P_Core_4
	{
		get
		{
			return p_core_4;
		}
		set
		{
			p_core_4 = value;
			RaisePropertyChanged(() => P_Core_4);
		}
	}

	public int P_Core_5
	{
		get
		{
			return p_core_5;
		}
		set
		{
			p_core_5 = value;
			RaisePropertyChanged(() => P_Core_5);
		}
	}

	public int P_Core_6
	{
		get
		{
			return p_core_6;
		}
		set
		{
			p_core_6 = value;
			RaisePropertyChanged(() => P_Core_6);
		}
	}

	public int P_Core_7
	{
		get
		{
			return p_core_7;
		}
		set
		{
			p_core_7 = value;
			RaisePropertyChanged(() => P_Core_7);
		}
	}

	public int P_Core_8
	{
		get
		{
			return p_core_8;
		}
		set
		{
			p_core_8 = value;
			RaisePropertyChanged(() => P_Core_8);
		}
	}

	public int P_Core_9
	{
		get
		{
			return p_core_9;
		}
		set
		{
			p_core_9 = value;
			RaisePropertyChanged(() => P_Core_9);
		}
	}

	public int P_Core_10
	{
		get
		{
			return p_core_10;
		}
		set
		{
			p_core_10 = value;
			RaisePropertyChanged(() => P_Core_10);
		}
	}

	public int E_Core_1
	{
		get
		{
			return e_core_1;
		}
		set
		{
			e_core_1 = value;
			RaisePropertyChanged(() => E_Core_1);
		}
	}

	public int E_Core_2
	{
		get
		{
			return e_core_2;
		}
		set
		{
			e_core_2 = value;
			RaisePropertyChanged(() => E_Core_2);
		}
	}

	public int E_Core_3
	{
		get
		{
			return e_core_3;
		}
		set
		{
			e_core_3 = value;
			RaisePropertyChanged(() => E_Core_3);
		}
	}

	public int E_Core_4
	{
		get
		{
			return e_core_4;
		}
		set
		{
			e_core_4 = value;
			RaisePropertyChanged(() => E_Core_4);
		}
	}

	public int E_Core_5
	{
		get
		{
			return e_core_5;
		}
		set
		{
			e_core_5 = value;
			RaisePropertyChanged(() => E_Core_5);
		}
	}

	public int E_Core_6
	{
		get
		{
			return e_core_6;
		}
		set
		{
			e_core_6 = value;
			RaisePropertyChanged(() => E_Core_6);
		}
	}

	public int E_Core_7
	{
		get
		{
			return e_core_7;
		}
		set
		{
			e_core_7 = value;
			RaisePropertyChanged(() => E_Core_7);
		}
	}

	public int E_Core_8
	{
		get
		{
			return e_core_8;
		}
		set
		{
			e_core_8 = value;
			RaisePropertyChanged(() => E_Core_8);
		}
	}

	public int CPU_Frequency_Per_Second
	{
		get
		{
			return cpu_frequency_per_second;
		}
		set
		{
			cpu_frequency_per_second = value;
			RaisePropertyChanged(() => CPU_Frequency_Per_Second);
		}
	}

	public string Voltage_Per_Second
	{
		get
		{
			return voltage_per_second;
		}
		set
		{
			voltage_per_second = value;
			RaisePropertyChanged(() => Voltage_Per_Second);
		}
	}

	public int Temperature_Per_Second
	{
		get
		{
			return temperature_per_second;
		}
		set
		{
			temperature_per_second = value;
			RaisePropertyChanged(() => Temperature_Per_Second);
		}
	}

	public int CPU_Usage_Per_Second
	{
		get
		{
			return cpu_usage_per_second;
		}
		set
		{
			cpu_usage_per_second = value;
			RaisePropertyChanged(() => CPU_Usage_Per_Second);
		}
	}

	public int Memory_Frequency_Per_Second
	{
		get
		{
			return memory_frequency_per_second;
		}
		set
		{
			memory_frequency_per_second = value;
			RaisePropertyChanged(() => Memory_Frequency_Per_Second);
		}
	}

	public int IsFullScreen
	{
		get
		{
			return dchuModel.GetFullScreen();
		}
		set
		{
			dchuModel.SetFullScreen(value);
		}
	}

	public void Initial()
	{
		overClockModel.Initial();
		Model_Name = overClockModel.model_name;
		Turbo_Min = overClockModel.turbo_min;
		Turbo_Max = overClockModel.turbo_max;
		Memory_Speed_Max = overClockModel.memory_speed_max;
		Reference = overClockModel.reference;
		PL1 = overClockModel.Current_PL1;
		PL2 = overClockModel.Current_PL2;
		PL_Time = overClockModel.Current_PLTime;
		CPU_VR_Auto = overClockModel.Current_CPU_VR_Auto;
		CPU_VR = overClockModel.Current_CPU_VR;
		Voltage_Offset = overClockModel.Current_Voltage_Offset;
		Voltage_Override = overClockModel.Current_Voltage_Override;
		Voltage_GT = overClockModel.Current_Voltage_GT;
		GetCoreRatioFromModel();
		GetPerSecondInfo();
		PL1PL2DataList = GetScaleDataList(1);
		PLTimeDataList = GetScaleDataList(2);
		VRDataList = GetScaleDataList(3);
		VoltageOffsetDataList = GetScaleDataList(4);
		GraphicsVoltageOffsetDataList = GetScaleDataList(5);
		CPUCoreDataList = GetScaleDataList(6);
		OverrideVoltageDataList = GetScaleDataList();
		IsControlCenterControlCPU_UnderVolting = dchuModel.IsControlCenterControlCPU_UnderVolting();
		Custom_Id = dchuModel.GetCustomId();
	}

	public void GetDefaultInfo()
	{
		PL1 = overClockModel.Default_PL1;
		PL2 = overClockModel.Default_PL2;
		PL_Time = overClockModel.Default_PLTime;
		CPU_VR_Auto = overClockModel.Default_CPU_VR_Auto;
		CPU_VR = overClockModel.Default_CPU_VR;
		Voltage_Offset = overClockModel.Default_Voltage_Offset;
		Voltage_Override = overClockModel.Default_Voltage_Override;
		Voltage_GT = overClockModel.Default_Voltage_GT;
		GetDefaultRatioFromModel();
	}

	public List<string> GetPlatformCompatibilityIssues()
	{
		return overClockModel.GetPlatformCompatibilityIssues();
	}

	private void GetDefaultRatioFromModel()
	{
		foreach (KeyValuePair<decimal, decimal> item in overClockModel.Default_Ratio)
		{
			if (item.Key == 29m)
			{
				P_Core_1 = (int)item.Value;
			}
			if (item.Key == 30m)
			{
				P_Core_2 = (int)item.Value;
			}
			if (item.Key == 31m)
			{
				P_Core_3 = (int)item.Value;
			}
			if (item.Key == 32m)
			{
				P_Core_4 = (int)item.Value;
			}
			if (item.Key == 42m)
			{
				P_Core_5 = (int)item.Value;
			}
			if (item.Key == 43m)
			{
				P_Core_6 = (int)item.Value;
			}
			if (item.Key == 96m)
			{
				P_Core_7 = (int)item.Value;
			}
			if (item.Key == 97m)
			{
				P_Core_8 = (int)item.Value;
			}
			if (item.Key == 107m)
			{
				P_Core_9 = (int)item.Value;
			}
			if (item.Key == 108m)
			{
				P_Core_10 = (int)item.Value;
			}
			if (item.Key == 4500m)
			{
				E_Core_1 = (int)item.Value;
			}
			if (item.Key == 4501m)
			{
				E_Core_2 = (int)item.Value;
			}
			if (item.Key == 4502m)
			{
				E_Core_3 = (int)item.Value;
			}
			if (item.Key == 4503m)
			{
				E_Core_4 = (int)item.Value;
			}
			if (item.Key == 4504m)
			{
				E_Core_5 = (int)item.Value;
			}
			if (item.Key == 4505m)
			{
				E_Core_6 = (int)item.Value;
			}
			if (item.Key == 4506m)
			{
				E_Core_7 = (int)item.Value;
			}
			if (item.Key == 4507m)
			{
				E_Core_8 = (int)item.Value;
			}
		}
	}

	private void GetCoreRatioFromModel()
	{
		foreach (KeyValuePair<decimal, decimal> item in overClockModel.Current_Ratio)
		{
			if (item.Key == 29m)
			{
				P_Core_1 = (int)item.Value;
			}
			if (item.Key == 30m)
			{
				P_Core_2 = (int)item.Value;
			}
			if (item.Key == 31m)
			{
				P_Core_3 = (int)item.Value;
			}
			if (item.Key == 32m)
			{
				P_Core_4 = (int)item.Value;
			}
			if (item.Key == 42m)
			{
				P_Core_5 = (int)item.Value;
			}
			if (item.Key == 43m)
			{
				P_Core_6 = (int)item.Value;
			}
			if (item.Key == 96m)
			{
				P_Core_7 = (int)item.Value;
			}
			if (item.Key == 97m)
			{
				P_Core_8 = (int)item.Value;
			}
			if (item.Key == 107m)
			{
				P_Core_9 = (int)item.Value;
			}
			if (item.Key == 108m)
			{
				P_Core_10 = (int)item.Value;
			}
			if (item.Key == 4500m)
			{
				E_Core_1 = (int)item.Value;
			}
			if (item.Key == 4501m)
			{
				E_Core_2 = (int)item.Value;
			}
			if (item.Key == 4502m)
			{
				E_Core_3 = (int)item.Value;
			}
			if (item.Key == 4503m)
			{
				E_Core_4 = (int)item.Value;
			}
			if (item.Key == 4504m)
			{
				E_Core_5 = (int)item.Value;
			}
			if (item.Key == 4505m)
			{
				E_Core_6 = (int)item.Value;
			}
			if (item.Key == 4506m)
			{
				E_Core_7 = (int)item.Value;
			}
			if (item.Key == 4507m)
			{
				E_Core_8 = (int)item.Value;
			}
		}
	}

	public void GetPerSecondInfo()
	{
		cpu_frequency_per_second = dchuModel.GetCPUFrequency();
		voltage_per_second = dchuModel.GetCPUVoltage();
		temperature_per_second = dchuModel.GetCPUTemperature();
		memory_frequency_per_second = dchuModel.GetMemoryFrequency();
		cpu_usage_per_second = overClockModel.GetCPUTotalUsage();
		if (cpu_usage_per_second < 0)
		{
			cpu_usage_per_second = dchuModel.GetCPUUtilization();
		}
	}

	private List<decimal> GetScaleDataList(int dataType)
	{
		return overClockModel.GetScaleDataList(dataType);
	}

	private List<string> GetScaleDataList()
	{
		return overClockModel.GetScaleDataList();
	}

	public int CheckSupportCPUOC()
	{
		if (!dchuModel.IsSupport_CPUOC_WMI16())
		{
			return -1;
		}
		if (!dchuModel.IsSupport_CPUOC_Wmi122Bit23())
		{
			return -2;
		}
		return 0;
	}

	public int CheckXTUServiceStatus()
	{
		if (!overClockModel.IsXTUServiceExist())
		{
			return -1;
		}
		if (!overClockModel.IsXTUServiceRunning())
		{
			return -2;
		}
		return 0;
	}

	public string TranslationLanguage(string Text)
	{
		return overClockModel.TranslationLanguage(Text);
	}

	public int GetPowerMode()
	{
		return dchuModel.GetPowerMode();
	}

	public void SetXTUInfoToInsydeBuffer()
	{
		dchuModel.SetIsSaved(1);
		dchuModel.SetPowerLimit1CurrentValue(PL1);
		dchuModel.SetPowerLimitTimeCurrentValue(PL_Time);
		dchuModel.SetPowerLimit2CurrentValue(PL2);
		dchuModel.SetCPUVRAuto(CPU_VR_Auto);
		dchuModel.SetCPUVR(CPU_VR);
		dchuModel.SetCoreVoltage(Voltage_Offset);
		dchuModel.SetOverrideVoltage(Voltage_Override);
		dchuModel.SetGraphicsVoltage(Voltage_GT);
		dchuModel.SetCoreRatio(29, P_Core_1);
		dchuModel.SetCoreRatio(30, P_Core_2);
		dchuModel.SetCoreRatio(31, P_Core_3);
		dchuModel.SetCoreRatio(32, P_Core_4);
		dchuModel.SetCoreRatio(42, P_Core_5);
		dchuModel.SetCoreRatio(43, P_Core_6);
		dchuModel.SetCoreRatio(96, P_Core_7);
		dchuModel.SetCoreRatio(97, P_Core_8);
		dchuModel.SetCoreRatio(107, P_Core_9);
		dchuModel.SetCoreRatio(108, P_Core_10);
		dchuModel.SetCoreRatio(4500, E_Core_1);
		dchuModel.SetCoreRatio(4501, E_Core_2);
		dchuModel.SetCoreRatio(4502, E_Core_3);
		dchuModel.SetCoreRatio(4503, E_Core_4);
		dchuModel.SetCoreRatio(4504, E_Core_5);
		dchuModel.SetCoreRatio(4505, E_Core_6);
		dchuModel.SetCoreRatio(4506, E_Core_7);
		dchuModel.SetCoreRatio(4507, E_Core_8);
		DebugLog.WriteDbgLog2Inlog("cpuoc_bg_106");
	}

	public void SetIsSaved(int save)
	{
		dchuModel.SetIsSaved(save);
	}
}
