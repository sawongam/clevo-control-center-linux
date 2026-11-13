using System;
using System.Globalization;
using RippleCC.Platform;
using System.Text;

namespace CC.FlexiKey;

public class UiStringTable
{
	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);

	public static bool GetSupport3LedKB()
	{
		if (Reg.GetString("HKEY_CURRENT_USER\\Software\\hotkey\\", "Support3LedKB", "0") == "1")
		{
			return true;
		}
		return false;
	}

	public static bool GetSupportTpColor()
	{
		if (Reg.GetString("HKEY_CURRENT_USER\\Software\\hotkey\\", "SupportTpColor", "0") == "1")
		{
			return true;
		}
		return false;
	}

	public static bool GetSupportLightbar()
	{
		if (Reg.GetString("HKEY_CURRENT_USER\\Software\\hotkey\\", "SupportLightbar", "0") == "1")
		{
			return true;
		}
		return false;
	}

	public static bool GetSupportLogoColor()
	{
		if (Reg.GetString("HKEY_CURRENT_USER\\Software\\hotkey\\", "SupportLogoColor", "0") == "1")
		{
			return true;
		}
		return false;
	}

	public static string GetLanguage()
	{
		string text = Reg.GetString("HKEY_CURRENT_USER\\Software\\ComboKey\\", "language", "");
		if (text == "")
		{
			return CultureInfo.CurrentCulture.Name.ToLower();
		}
		return text;
	}

	public static HookKeysTable.KeyLanguage GetLayout(string lang)
	{
		if (lang.Contains("en-gb"))
		{
			return HookKeysTable.KeyLanguage.uk;
		}
		if (lang.Contains("en-") || lang.Contains("zh-") || lang.Contains("ko") || lang.Contains("th") || lang.Contains("vi"))
		{
			return HookKeysTable.KeyLanguage.usa;
		}
		if (lang.Contains("ja"))
		{
			return HookKeysTable.KeyLanguage.jp;
		}
		if (lang.Contains("pt-br"))
		{
			return HookKeysTable.KeyLanguage.br;
		}
		return HookKeysTable.KeyLanguage.uk;
	}

	public static string UpdateLbl(string path, string name)
	{
		string text = GetLanguage();
		if (text.Contains("en-"))
		{
			text = "en";
		}
		else if (text.Contains("es"))
		{
			text = "es";
		}
		else if (text.Contains("de"))
		{
			text = "de";
		}
		else if (text.Contains("it"))
		{
			text = "it";
		}
		else if (text.Contains("ja"))
		{
			text = "ja";
		}
		else if (text.Contains("ko"))
		{
			text = "ko";
		}
		else if (text.Contains("th-th"))
		{
			text = "th-th";
		}
		else if (text.Contains("tr-tr"))
		{
			text = "tr-tr";
		}
		else if (text.Contains("vi-vn"))
		{
			text = "vi-vn";
		}
		string text2 = "null";
		StringBuilder stringBuilder = new StringBuilder(1024);
		string def = "null";
		int bufferSize = 1024;
		GetPrivateProfileString(text, name, def, stringBuilder, bufferSize, path);
		text2 = stringBuilder.ToString();
		if (text2 == "null")
		{
			GetPrivateProfileString("en", name, def, stringBuilder, bufferSize, path);
			text2 = stringBuilder.ToString();
		}
		return text2;
	}

	public static string UpdateBacklightLbl(string path, string name)
	{
		string text = GetLanguage();
		if (text.Contains("en-"))
		{
			text = "en";
		}
		else if (text.Contains("es"))
		{
			text = "es";
		}
		else if (text.Contains("de"))
		{
			text = "de";
		}
		else if (text.Contains("it"))
		{
			text = "it";
		}
		else if (text.Contains("ja"))
		{
			text = "ja";
		}
		else if (text.Contains("ko"))
		{
			text = "ko";
		}
		else if (text.Contains("th-th"))
		{
			text = "th-th";
		}
		else if (text.Contains("tr-tr"))
		{
			text = "tr-tr";
		}
		else if (text.Contains("vi-vn"))
		{
			text = "vi-vn";
		}
		string text2 = "null";
		StringBuilder stringBuilder = new StringBuilder(1024);
		string def = "null";
		int bufferSize = 1024;
		GetPrivateProfileString(text, name, def, stringBuilder, bufferSize, path);
		text2 = stringBuilder.ToString();
		if (text2 == "null")
		{
			GetPrivateProfileString("en", name, def, stringBuilder, bufferSize, path);
			text2 = stringBuilder.ToString();
		}
		return text2;
	}
}
