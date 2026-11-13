using System;
using System.Globalization;
using RippleCC.Platform;

namespace CC;

public class RWReg
{
	const string UI = "HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\";
	const string FA = "HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\FA";
	const string WMI = "HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\";
	public int HeadPhone;

	public int window_FullScreen
	{
		get { var t = Reg.GetString(UI, "window_FullScreen", "null"); return t == "null" ? 1 : Convert.ToInt16(t); }
		set { Reg.SetValue(UI, "window_FullScreen", value.ToString()); }
	}
	public int window_Color
	{
		get { var t = Reg.GetString(UI, "window_Color", "null"); return t == "null" ? 10617087 : Convert.ToInt32(t); }
		set { Reg.SetValue(UI, "window_Color", value.ToString()); }
	}
	public int FlexiKey_KB_Enable { get { return 1; } set { } }
	public int FlexiKey_Mouse_Enable { get { return 1; } set { } }
	public int FlexiAccessStatus
	{
		get { return Convert.ToInt32(Reg.GetString(FA, "FlexiAccessStatus", "0")); }
		set { Reg.SetValue(FA, "FlexiAccessStatus", value); }
	}
	public string FA_ClientName { get { return Reg.GetString(FA, "ClientName", ""); } set { Reg.SetValue(FA, "ClientName", value); } }
	public string FA_Port { get { return Reg.GetString(FA, "Port", ""); } set { Reg.SetValue(FA, "Port", value); } }
	public string FA_IP { get { return Reg.GetString(FA, "IP", ""); } set { Reg.SetValue(FA, "IP", value); } }
	public int Support_Page_SystemMonitor() { return Convert.ToInt32(Reg.GetString(UI, "Support_Page_SystemMonitor", "0")); }
	public int WMI_122() { return Convert.ToInt32(Reg.GetString(WMI, "WMI_122", "0")); }
	public int Get_GPUSwitchStatus() { return Convert.ToInt16(Reg.GetString(WMI, "WMI_84", "0")); }
	public int KB_language() { return Convert.ToInt32(Reg.GetString(UI, "KB_language", "0")); }
	private void WriteLanguage(string strLang) { Reg.SetValue(UI, "language", strLang); }

	public string Check_language()
	{
		string text = "";
		text = Reg.GetString("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "language", "");
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
		Global.Flexikey_LED_Language = 0;
		return text;
	}
}
