using System;
using System.Globalization;
using CCenter.Platform;
namespace CC;
/// <summary>Registry-backed settings (HKCU\Software\ControlCenter2.0\...). Only the members this app uses are ported;
/// storage goes through CCenter.Platform.Reg (JSON file in ~/.config/ccenter/registry.json).</summary>
public class RWReg
{
	const string UI = "HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\";
	const string BU = "HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\BatteryUtility";
	public string UI_language = "en-US";

	static short I16(string key, string name, object def) => Convert.ToInt16(Reg.GetValue(key, name, def));

	public int window_FullSrceen
	{
		get { return 1; }   // original getter is a constant 1 (always starts maximised)
		set { Reg.SetValue(UI, "window_FullSrceen", value.ToString()); }
	}
	public int window_Width
	{
		get { return I16(UI, "window_Width", 0); }
		set { Reg.SetValue(UI, "window_Width", value); }
	}
	public int window_Height
	{
		get { return I16(UI, "window_Height", 0); }
		set { Reg.SetValue(UI, "window_Height", value); }
	}
	public string last_RefreshDate
	{
		get { return Convert.ToString(Reg.GetValue(BU, "RefreshDate", "null")); }
		set { Reg.SetValue(BU, "RefreshDate", value); }
	}
	public int OngoingStatus
	{
		get { return I16(BU, "OngoingStatus", "0"); }
		set { Reg.SetValue(BU, "OngoingStatus", value); }
	}
	public int Refresh_ElapsedTime_ss
	{
		get { return I16(BU, "Refresh_ElapsedTime_ss", "0"); }
		set { Reg.SetValue(BU, "Refresh_ElapsedTime_ss", value); }
	}
	public int Refresh_ElapsedTime_mm
	{
		get { return I16(BU, "Refresh_ElapsedTime_mm", "0"); }
		set { Reg.SetValue(BU, "Refresh_ElapsedTime_mm", value); }
	}
	public int Refresh_ElapsedTime_hh
	{
		get { return I16(BU, "Refresh_ElapsedTime_hh", "0"); }
		set { Reg.SetValue(BU, "Refresh_ElapsedTime_hh", value); }
	}
	public int BAT_log_count
	{
		get { return I16(BU, "BAT_log_count", "0"); }
		set { Reg.SetValue(BU, "BAT_log_count", value); }
	}

	public string Check_language()
	{
		string text = "";
		text = Convert.ToString(Reg.GetValue(UI, "language", ""));
		if (text == "")
		{
			text = CultureInfo.CurrentCulture.Name.ToLower();
			WriteLanguage(text);
		}
		if (text.Contains("en"))
		{
			Global.Flexikey_LED_Language = text == "en-us" ? 0 : 1;
			text = "en-us";
		}
		else if (text.Contains("es")) { text = "es"; Global.Flexikey_LED_Language = 1; }
		else if (text.Contains("de")) { text = "de"; Global.Flexikey_LED_Language = 1; }
		else if (text.Contains("it")) { text = "it"; Global.Flexikey_LED_Language = 1; }
		else if (text.Contains("ja")) { text = "ja"; Global.Flexikey_LED_Language = 2; }
		else if (text.Contains("ko")) { text = "ko"; Global.Flexikey_LED_Language = 0; }
		else if (text.Contains("zh-cn")) { text = "zh-cn"; Global.Flexikey_LED_Language = 0; }
		else if (text.Contains("zh-tw")) { text = "zh-tw"; Global.Flexikey_LED_Language = 0; }
		else if (text.Contains("fr-fr") || text.Contains("zh-fr")) { text = "fr-fr"; Global.Flexikey_LED_Language = 1; }
		else if (text.Contains("fr-ca") || text.Contains("zh-ca")) { text = "fr-ca"; Global.Flexikey_LED_Language = 1; }
		else if (text.Contains("pt-br") || text.Contains("zh-br")) { text = "pt-br"; Global.Flexikey_LED_Language = 3; }
		else if (text.Contains("pt-pt") || text.Contains("zh-pt")) { text = "pt-pt"; Global.Flexikey_LED_Language = 1; }
		else if (text.Contains("tr-tr") || text.Contains("zh-tr")) { text = "tr-tr"; Global.Flexikey_LED_Language = 1; }
		else if (text.Contains("th-th") || text.Contains("zh-th")) { text = "th-th"; Global.Flexikey_LED_Language = 0; }
		else if (text.Contains("vi-vn") || text.Contains("zh-vn")) { text = "vi-vn"; Global.Flexikey_LED_Language = 0; }
		else if (text.Contains("ru")) { text = "ru"; Global.Flexikey_LED_Language = 1; }
		else { text = "en-us"; Global.Flexikey_LED_Language = 0; }
		return text;
	}

	private void WriteLanguage(string strLang) => Reg.SetValue(UI, "language", strLang);
}
