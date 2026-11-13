using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Layout;
using RippleCC.Platform;
using System.Globalization;
using CC.other_Class;


namespace CC;

public class RWReg
{
	public int window_FullSrceen
	{
		get
		{
			string text = Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_FullSrceen", "null").ToString();
			if (text == "null")
			{
				return 1;
			}
			return Convert.ToInt16(text);
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_FullSrceen", value.ToString());
		}
	}

	public int window_Color
	{
		get
		{
			string text = Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Color", "null").ToString();
			if (text == "null")
			{
				return 10617087;
			}
			return Convert.ToInt32(text);
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Color", value.ToString());
		}
	}

	public int window_Width
	{
		get
		{
			return Convert.ToInt16(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Width", 0).ToString());
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Width", value);
		}
	}

	public int window_Height
	{
		get
		{
			return Convert.ToInt16(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Height", 0).ToString());
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "window_Height", value);
		}
	}

	public double Screen_Width
	{
		get
		{
			return Convert.ToInt16(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "Screen_Width", 0));
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "Screen_Width", value);
		}
	}

	public double Screen_Height
	{
		get
		{
			return Convert.ToInt16(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "Screen_Height", 0));
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "Screen_Height", value);
		}
	}

	public int IsFirstRun
	{
		get
		{
			return Convert.ToInt16(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "FirstRun", "1"));
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\LED", "FirstRun", value);
		}
	}

	public string Check_language()
	{
		string text = "";
		text = InsydeAcpiProt.ReadAcpiUILanguage() switch
		{
			0 => CultureInfo.CurrentCulture.Name.ToLower(), 
			1 => "en-us", 
			2 => "en-us", 
			3 => "es", 
			4 => "de", 
			5 => "it", 
			6 => "ja", 
			7 => "ko", 
			8 => "zh-cn", 
			9 => "zh-tw", 
			10 => "fr-fr", 
			11 => "fr-ca", 
			12 => "pt-br", 
			13 => "pt-pt", 
			14 => "tr-tr", 
			15 => "th-th", 
			16 => "vi-vn", 
			17 => "ru", 
			18 => "ja-ep", 
			_ => "en-us", 
		};
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
		else if (text.Contains("ja-ep"))
		{
			text = "ja-ep";
			Global.Flexikey_LED_Language = 2;
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
		return Convert.ToInt32(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "KB_language", "0"));
	}

	private void WriteLanguage(string strLang)
	{
		Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "language", strLang);
	}

	public int WMI_122()
	{
		return Convert.ToInt32(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\WMI\\", "WMI_122", "0").ToString());
	}
}
