using System;
using System.Globalization;
using RippleCC.Platform;

namespace GPUOverclocking;

// Port of the members of RWReg that this app uses (window geometry, language, GPU OC limits).
// Registry -> RippleCC.Platform.Reg (JSON file). The keyboard/fan/battery members were never called here.
public class RWReg
{
	private const string UI = "HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\";
	private const string GPU = "HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\GPU";
	public string UI_language = "en-US";

	public int window_FullSrceen
	{
		get
		{
			string text = (Reg.GetValue(UI, "window_FullSrceen", "null") ?? "null").ToString();
			if (text == "null") return 1;
			return Convert.ToInt16(text);
		}
		set { Reg.SetValue(UI, "window_FullSrceen", value.ToString()); }
	}

	public int window_Color
	{
		get
		{
			string text = (Reg.GetValue(UI, "window_Color", "null") ?? "null").ToString();
			if (text == "null") return 10617087;
			return Convert.ToInt32(text);
		}
		set { Reg.SetValue(UI, "window_Color", value.ToString()); }
	}

	public int window_Width
	{
		get { return Convert.ToInt16((Reg.GetValue(UI, "window_Width", 0) ?? 0).ToString()); }
		set { Reg.SetValue(UI, "window_Width", value); }
	}

	public int window_Height
	{
		get { return Convert.ToInt16((Reg.GetValue(UI, "window_Height", 0) ?? 0).ToString()); }
		set { Reg.SetValue(UI, "window_Height", value); }
	}

	public void Init()
	{
	}

	public string Check_language()
	{
		string text = Convert.ToString(Reg.GetValue(UI, "language", ""));
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

	private void WriteLanguage(string strLang)
	{
		Reg.SetValue(UI, "language", strLang);
	}

	public int GPU_CoreINC_MAX() => Convert.ToInt16(Reg.GetValue(GPU, "GPUOCMax", "0"));
	public int GPU_CoreINC_MIN() => Convert.ToInt16(Reg.GetValue(GPU, "GPUOCMin", "0"));
	public int GPU_VRAMINC_MAX() => Convert.ToInt16(Reg.GetValue(GPU, "MEMOCMax", "0"));
	public int GPU_VRAMINC_MIN() => Convert.ToInt16(Reg.GetValue(GPU, "MEMOCMin", "0"));
}
