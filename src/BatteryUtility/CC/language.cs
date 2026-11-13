using System;
using System.Globalization;
using System.IO;
using System.Text;
using RippleCC.Platform;
namespace CC;
public class language
{
	private string path;
	private StringBuilder outStr = new StringBuilder(255);
	private string Default = "null";
	private int Size = 255;
	private string lang = "en-US";
	/// <summary>PORT: --lang xx on the command line forces the language (used for screenshots).</summary>
	static string ForcedLang()
	{
		var a = AppHost.Args; int i = Array.IndexOf(a, "--lang");
		return i >= 0 && i + 1 < a.Length ? a[i + 1] : null;
	}
	public language()
	{
		path = Path.Combine(AppContext.BaseDirectory, "mtlanguage.ini");
		lang = ForcedLang() ?? Global.RW_REG.Check_language();
	}
	public string Translation(string Text)
	{
		Ini.GetPrivateProfileString(lang, Text, Default, outStr, Size, path);
		return outStr.ToString();
	}
	public void Load_language()
	{
		lang = ForcedLang() ?? Check_language();
	}
	public string Check_language()
	{
		byte[] aPPData = Global.insyde.GetAPPData(1, 2, 1);
		switch (aPPData[0])
		{
		case 1: return "en-us";
		case 3: return "es";
		case 4: return "de";
		case 5: return "it";
		case 6: return Global.support_bit.CustomerID == 5 ? "ja-ep" : "ja";
		case 7: return "ko";
		case 8: return "zh-cn";
		case 9: return "zh-tw";
		case 10: return "fr-fr";
		case 11: return "fr-ca";
		case 12: return "pt-br";
		case 13: return "pt-pt";
		case 14: return "tr-tr";
		case 15: return "th-th";
		case 16: return "vi-vn";
		case 17: return "ru";
		case 18: return "ja-ep";
		}
		string text2 = CultureInfo.CurrentCulture.Name.ToLower();
		if (text2 == "ja")
			return Global.support_bit.CustomerID == 5 ? "ja-ep" : "ja";
		return "en-us";
	}
}
