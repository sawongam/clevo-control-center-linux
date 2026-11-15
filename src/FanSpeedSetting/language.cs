using System;
using System.Diagnostics;
using System.IO;
using CCenter.Platform;
using System.Text;

namespace FanSpeedSetting;

public class language
{
	private string path;

	private StringBuilder outStr = new StringBuilder(255);

	private string Default = "null";

	private int Size = 255;

	private string lang = "en-US";

	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);


	public language()
	{
		path = Path.Combine(AppContext.BaseDirectory, "mtlanguage.ini");
	}

	public string Translation(string Text)
	{
		GetPrivateProfileString(lang, Text, Default, outStr, Size, path);
		return outStr.ToString();
	}

	public void Load_language()
	{
		lang = Check_language();
	}

	public string Check_language()
	{
		string text = "";
		byte[] aPPData = Global.insyde.GetAPPData(1, 2, 1);
		if (aPPData[0] == 1)
		{
			return "en-us";
		}
		if (aPPData[0] == 3)
		{
			return "es";
		}
		if (aPPData[0] == 4)
		{
			return "de";
		}
		if (aPPData[0] == 5)
		{
			return "it";
		}
		if (aPPData[0] == 6)
		{
			if (Global.support_bit.CustomerID == 5)
			{
				return "ja-ep";
			}
			return "ja";
		}
		if (aPPData[0] == 7)
		{
			return "ko";
		}
		if (aPPData[0] == 8)
		{
			return "zh-cn";
		}
		if (aPPData[0] == 9)
		{
			return "zh-tw";
		}
		if (aPPData[0] == 10)
		{
			return "fr-fr";
		}
		if (aPPData[0] == 11)
		{
			return "fr-ca";
		}
		if (aPPData[0] == 12)
		{
			return "pt-br";
		}
		if (aPPData[0] == 13)
		{
			return "pt-pt";
		}
		if (aPPData[0] == 14)
		{
			return "tr-tr";
		}
		if (aPPData[0] == 15)
		{
			return "th-th";
		}
		if (aPPData[0] == 16)
		{
			return "vi-vn";
		}
		if (aPPData[0] == 17)
		{
			return "ru";
		}
		if (aPPData[0] == 18)
		{
			return "ja-ep";
		}
		return "en-us";
	}
}
