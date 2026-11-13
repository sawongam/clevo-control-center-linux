using System;
using System.Globalization;

namespace FanSpeedSetting;

public class RWReg
{
	public string UI_language = "en-US";

	private const int CPU_REMOTE = 18;

	private const int GPU1_REMOTE = 21;

	private const int GPU2_REMOTE = 24;

	private int cpu_temp;

	private int gpu1_temp;

	private int gpu2_temp;

	public double barkup_VB_CPUFan_H;

	public double barkup_VB_CPUFan_W;

	public double barkup_VB_CPU_Temp_W;

	public double barkup_VB_CPU_Temp_H;

	public string CurrentMode;

	public string CurrentMode2;

	public int AirplaneMode;

	public int TouchPad;

	public int CCD;

	public int WhiteLedKB_Brightness;

	public int HeadPhone;

	public int KBSleepTimer;

	public int KBSleepTimerOn;

	public bool SupportNewMode = false;

	public int window_FullSrceen
	{
		get
		{
			string text = Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "window_FullSrceen", "null"));
			if (text == "null")
			{
				return 1;
			}
			return Convert.ToInt16(text);
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "window_FullSrceen", value.ToString());
		}
	}

	public int window_Color
	{
		get
		{
			string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Color", "null").ToString();
			if (text == "null")
			{
				return 10617087;
			}
			return Convert.ToInt32(text);
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Color", value.ToString());
		}
	}

	public int First_run
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "First_run", 0));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "First_run", value, RegistryValueKind.DWord);
		}
	}

	public string ECVersion
	{
		get
		{
			return Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\", "ECVersion", "1."));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\", "ECVersion", value, RegistryValueKind.String);
		}
	}

	public int window_Width
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "window_Width", 0));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "window_Width", value, RegistryValueKind.DWord);
		}
	}

	public int window_Height
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "window_Height", 0));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "window_Height", value, RegistryValueKind.DWord);
		}
	}

	public double Screen_Width
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "Screen_Width", 0));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "Screen_Width", value, RegistryValueKind.DWord);
		}
	}

	public double Screen_Height
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "Screen_Height", 0));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "Screen_Height", value, RegistryValueKind.DWord);
		}
	}

	public int UAC_Behavior
	{
		get
		{
			string text = Registry.GetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "ConsentPromptBehaviorAdmin", "5").ToString();
			if (text == "null")
			{
				return 5;
			}
			return Convert.ToInt32(text);
		}
		set
		{
			Registry.SetValue("HKEY_LOCAL_MACHINE\\SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Policies\\System", "ConsentPromptBehaviorAdmin", value, RegistryValueKind.DWord);
		}
	}

	public int GPUOC_Freq
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUOC_Freq", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUOC_Freq", value, RegistryValueKind.DWord);
		}
	}

	public int GPUOC2_Freq
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUOC2_Freq", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUOC2_Freq", value, RegistryValueKind.DWord);
		}
	}

	public int GPUMOC_Freq
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUMOC_Freq", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUMOC_Freq", value, RegistryValueKind.DWord);
		}
	}

	public int GPUMOC2_Freq
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUMOC2_Freq", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUMOC2_Freq", value, RegistryValueKind.DWord);
		}
	}

	public byte FanOCMode
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC", "FanOCMode", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC", "FanOCMode", value, RegistryValueKind.DWord);
		}
	}

	public bool SupportFanLess
	{
		get
		{
			return Convert.ToBoolean(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0", "SupportFanLess", null));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0", "SupportFanLess", value);
		}
	}

	public bool SupportMaxqFan => Convert.ToBoolean(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\", "SupportMaxQ_FAN", null));

	public int PerkeySpeed
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KBSpeed", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KBSpeed", value, RegistryValueKind.DWord);
		}
	}

	public int KBStatus
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KBStatus", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KBStatus", value, RegistryValueKind.DWord);
		}
	}

	public int KBbrightness
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KBbrightness", "3"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KBbrightness", value, RegistryValueKind.DWord);
		}
	}

	public int KB_RGB_Mode
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KB_RGB_Mode", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KB_RGB_Mode", value, RegistryValueKind.DWord);
		}
	}

	public int Perkey_EffectMode
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "EffectMode", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "EffectMode", value, RegistryValueKind.DWord);
		}
	}

	public int Perkey_BlinkStatus
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "BlinkStatus", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "BlinkStatus", value, RegistryValueKind.DWord);
		}
	}

	public byte Blink_R
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Blink_R", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Blink_R", value, RegistryValueKind.DWord);
		}
	}

	public byte Blink_G
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Blink_G", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Blink_G", value, RegistryValueKind.DWord);
		}
	}

	public byte Blink_B
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Blink_B", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Blink_B", value, RegistryValueKind.DWord);
		}
	}

	public int Perkey_BreathStatus
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "BreathStatus", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "BreathStatus", value, RegistryValueKind.DWord);
		}
	}

	public byte Breath_R
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Breath_R", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Breath_R", value, RegistryValueKind.DWord);
		}
	}

	public byte Breath_G
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Breath_G", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Breath_G", value, RegistryValueKind.DWord);
		}
	}

	public byte Breath_B
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Breath_B", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Breath_B", value, RegistryValueKind.DWord);
		}
	}

	public int Perkey_RippleStatus
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "RippleStatus", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "RippleStatus", value, RegistryValueKind.DWord);
		}
	}

	public byte Ripple_R
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Ripple_R", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Ripple_R", value, RegistryValueKind.DWord);
		}
	}

	public byte Ripple_G
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Ripple_G", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Ripple_G", value, RegistryValueKind.DWord);
		}
	}

	public byte Ripple_B
	{
		get
		{
			return Convert.ToByte(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Ripple_B", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "Ripple_B", value, RegistryValueKind.DWord);
		}
	}

	public int Perkey_StaticStatus
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "StaticStatus", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\", "StaticStatus", value, RegistryValueKind.DWord);
		}
	}

	public int FlexiKey_KB_Enable
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ComboKey\\", "KBEnable", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ComboKey", "KBEnable", value.ToString());
		}
	}

	public int FlexiKey_Mouse_Enable
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ComboKey\\", "MouseEnable", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ComboKey", "MouseEnable", value.ToString());
		}
	}

	public int FlexiAccessStatus
	{
		get
		{
			return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "FlexiAccessStatus", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "FlexiAccessStatus", value, RegistryValueKind.DWord);
		}
	}

	public string FA_ClientName
	{
		get
		{
			return Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "ClientName", ""));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "ClientName", value, RegistryValueKind.String);
		}
	}

	public string FA_Port
	{
		get
		{
			return Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "Port", ""));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "Port", value, RegistryValueKind.String);
		}
	}

	public string FA_IP
	{
		get
		{
			return Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "IP", ""));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA", "IP", value, RegistryValueKind.String);
		}
	}

	public string last_RefreshDate
	{
		get
		{
			return Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "RefreshDate", "null"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "RefreshDate", value, RegistryValueKind.String);
		}
	}

	public int OngoingStatus
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "OngoingStatus", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "OngoingStatus", value, RegistryValueKind.DWord);
		}
	}

	public int Refresh_ElapsedTime_ss
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "Refresh_ElapsedTime_ss", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "Refresh_ElapsedTime_ss", value, RegistryValueKind.DWord);
		}
	}

	public int Refresh_ElapsedTime_mm
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "Refresh_ElapsedTime_mm", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "Refresh_ElapsedTime_mm", value, RegistryValueKind.DWord);
		}
	}

	public int Refresh_ElapsedTime_hh
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "Refresh_ElapsedTime_hh", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "Refresh_ElapsedTime_hh", value, RegistryValueKind.DWord);
		}
	}

	public int BAT_log_count
	{
		get
		{
			return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "BAT_log_count", "0"));
		}
		set
		{
			Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility", "BAT_log_count", value, RegistryValueKind.DWord);
		}
	}


	public void Init()
	{
	}

	public int Support_Page_SystemMonitor()
	{
		return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "Support_Page_SystemMonitor", "0"));
	}

	public int Support_Page_LEDDevice()
	{
		return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "Support_Page_LEDDevice", "0"));
	}

	public string Check_language()
	{
		string text = "";
		text = Convert.ToString(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "language", ""));
		if (text == "")
		{
			text = CultureInfo.CurrentCulture.Name.ToLower();
			WriteLanguage(text);
		}
		if (text.Contains("en"))
		{
			if (text == "en-us")
			{
				Global.Flexikey_LED_Language = 0;
			}
			else
			{
				Global.Flexikey_LED_Language = 1;
			}
			text = "en-us";
		}
		else if (text.Contains("es"))
		{
			text = "es";
			Global.Flexikey_LED_Language = 1;
		}
		else if (text.Contains("de"))
		{
			text = "de";
			Global.Flexikey_LED_Language = 1;
		}
		else if (text.Contains("it"))
		{
			text = "it";
			Global.Flexikey_LED_Language = 1;
		}
		else if (text.Contains("ja"))
		{
			text = "ja";
			Global.Flexikey_LED_Language = 2;
		}
		else if (text.Contains("ko"))
		{
			text = "ko";
			Global.Flexikey_LED_Language = 0;
		}
		else if (text.Contains("zh-cn"))
		{
			text = "zh-cn";
			Global.Flexikey_LED_Language = 0;
		}
		else if (text.Contains("zh-tw"))
		{
			text = "zh-tw";
			Global.Flexikey_LED_Language = 0;
		}
		else if (text.Contains("fr-fr") || text.Contains("zh-fr"))
		{
			text = "fr-fr";
			Global.Flexikey_LED_Language = 1;
		}
		else if (text.Contains("fr-ca") || text.Contains("zh-ca"))
		{
			text = "fr-ca";
			Global.Flexikey_LED_Language = 1;
		}
		else if (text.Contains("pt-br") || text.Contains("zh-br"))
		{
			text = "pt-br";
			Global.Flexikey_LED_Language = 3;
		}
		else if (text.Contains("pt-pt") || text.Contains("zh-pt"))
		{
			text = "pt-pt";
			Global.Flexikey_LED_Language = 1;
		}
		else if (text.Contains("tr-tr") || text.Contains("zh-tr"))
		{
			text = "tr-tr";
			Global.Flexikey_LED_Language = 1;
		}
		else if (text.Contains("th-th") || text.Contains("zh-th"))
		{
			text = "th-th";
			Global.Flexikey_LED_Language = 0;
		}
		else if (text.Contains("vi-vn") || text.Contains("zh-vn"))
		{
			text = "vi-vn";
			Global.Flexikey_LED_Language = 0;
		}
		else if (text.Contains("ru"))
		{
			text = "ru";
			Global.Flexikey_LED_Language = 1;
		}
		else
		{
			text = "en-us";
			Global.Flexikey_LED_Language = 0;
		}
		return text;
	}

	public int KB_language()
	{
		return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "KB_language", "0"));
	}

	private void WriteLanguage(string strLang)
	{
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\Fan\\", "language", strLang);
	}

	public int GPU_CoreINC_MAX()
	{
		return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUOCMax", "0"));
	}

	public int GPU_CoreINC_MIN()
	{
		return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "GPUOCMin", "0"));
	}

	public int GPU_VRAMINC_MAX()
	{
		return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "MEMOCMax", "0"));
	}

	public int GPU_VRAMINC_MIN()
	{
		return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU", "MEMOCMin", "0"));
	}

	public int WMI_6()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_6", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int WMI_8()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_8", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int WMI_69()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_69", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int WMI_70()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_70", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int WMI_122()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_122", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int Get_GPUSwitchStatus()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_84", "0").ToString();
		return Convert.ToInt16(value);
	}

	public int Get_TP()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_42", "0").ToString();
		return Convert.ToInt16(value);
	}

	public int Get_CCD()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_34", "0").ToString();
		return Convert.ToInt16(value);
	}

	public int Get_LeftWinKey()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0", "LeftWinKey", "1").ToString();
		return Convert.ToInt32(value);
	}

	public bool Get_DisableAirplane()
	{
		return Convert.ToBoolean(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0", "Disable_Airplane", null));
	}

	public bool Get_SupportDocking()
	{
		return Convert.ToBoolean(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0", "SupportDocking", null));
	}

	public int Get_DockingStatus()
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0", "DockingStatus", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int Get_Fan_Table_duty(int FanIndex, int DutyIndex)
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan" + FanIndex, "D" + DutyIndex, "null").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public void Set_Fan_Table_duty(int FanIndex, int DutyIndex, int value)
	{
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan" + FanIndex, "D" + DutyIndex, value, RegistryValueKind.DWord);
	}

	public int Get_Fan_Table_temp(int FanIndex, int TempIndex)
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan" + FanIndex, "T" + TempIndex, "null").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public void Set_Fan_Table_temp(int FanIndex, int TempIndex, int value)
	{
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan" + FanIndex, "T" + TempIndex, value, RegistryValueKind.DWord);
	}

	public int Get_Fan_Table_duty_default(int FanIndex, int DutyIndex)
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan" + FanIndex, "Default_D" + DutyIndex, "null").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int Get_Fan_Table_temp_default(int FanIndex, int TempIndex)
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan" + FanIndex, "Default_T" + TempIndex, "null").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int CPUFan_duty()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan1", "duty", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int GPUFan_duty()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan2", "duty", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int GPUFan2_duty()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan3", "duty", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int CPUFan_rpm()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan1", "rpm", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int GPUFan_rpm()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan2", "rpm", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int GPUFan2_rpm()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC\\Fan3", "rpm", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int FanMode()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC", "FanMode", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int FanOffset()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC", "FanOffset", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	public int FanCount()
	{
		string text = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\EC", "FanCount", "0").ToString();
		if (text == "null")
		{
			return 0;
		}
		return Convert.ToInt32(text);
	}

	// LoadCurrentMode (Registry.OpenSubKey) is not used by FanSpeedSetting; omitted

	public int KBType()
	{
		return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "KBType", "0"));
	}

	public bool SupportSelectBootEffect()
	{
		if (Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\", "SupportKBSelectBootEffect", "0")) == 0)
		{
			return false;
		}
		return true;
	}

	public int SelectBootEffect()
	{
		return Convert.ToInt32(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\", "KBSelectBootEffect", "0"));
	}

	public void Set_KB_RGB(string part, int R, int G, int B)
	{
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\" + part, "R", R, RegistryValueKind.DWord);
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\" + part, "G", G, RegistryValueKind.DWord);
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\" + part, "B", B, RegistryValueKind.DWord);
	}

	public int Get_KB_R(string part)
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED" + part, "R", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int Get_KB_G(string part)
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED" + part, "G", "0").ToString();
		return Convert.ToInt32(value);
	}

	public int Get_KB_B(string part)
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED" + part, "B", "0").ToString();
		return Convert.ToInt32(value);
	}

	public void Set_Perkey_Static_RGB(string id, byte R, byte G, byte B)
	{
		int num = (R << 16) + (G << 8) + B;
		Registry.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\Static", id, num, RegistryValueKind.DWord);
	}

	public int Get_Perkey_Static_RGB(string id)
	{
		string value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED\\PKB\\Static", id, "0").ToString();
		return Convert.ToInt32(value);
	}

	public int PerKey_Type()
	{
		return Convert.ToInt16(Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "PerKey_Type", "0"));
	}

	public string GetTDP()
	{
		return "0";
	}

	public int CalCPUTemp(string CPUTDP, int set_remote)
	{
		double num = 0.0;
		if (CPUTDP == "84W" || CPUTDP == "88W")
		{
			num = ((set_remote <= 60) ? ((double)(set_remote - 1)) : ((double)(set_remote - 12) * 0.33 + 44.0));
		}
		else
		{
			num = CPUTDP switch
			{
				"65W" => (set_remote <= 50) ? ((double)(set_remote - 1)) : ((double)(set_remote - 35) * 0.41 + 43.7), 
				"91W" => (set_remote <= 50) ? ((double)(set_remote - 5)) : ((double)(set_remote - 9) * 0.22 + 43.7), 
				"35W" => (set_remote <= 32) ? ((double)set_remote) : ((double)set_remote * 0.5 + 16.0), 
				"47W" => (set_remote <= 26) ? ((double)set_remote) : ((double)set_remote * 0.5 + 13.0), 
				_ => set_remote, 
			};
		}
		return (int)Math.Round(num, 0);
	}

	public void UpdateWMI12()
	{
		object value = Registry.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\wmi", "WMI_12", "");
		byte[] array = (byte[])value;
		cpu_temp = CalCPUTemp(GetTDP(), array[18]);
		gpu1_temp = array[21];
		gpu2_temp = array[24];
	}

	public int CpuTemp()
	{
		return cpu_temp;
	}

	public int GpuTemp(int num)
	{
		return num switch
		{
			0 => gpu1_temp, 
			1 => gpu2_temp, 
			_ => 0, 
		};
	}
}
