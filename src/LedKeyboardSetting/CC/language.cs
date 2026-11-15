using CCenter.Platform;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CC;

public class language
{
	private string path;

	private StringBuilder outStr = new StringBuilder(255);

	private string Default = "null";

	private int Size = 255;

	private string lang = "en-US";

	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);

	private static long WritePrivateProfileString(string section, string key, string val, string filePath) => Ini.WritePrivateProfileString(section, key, val, filePath) ? 1 : 0;

	public language()
	{
		path = AppContext.BaseDirectory.TrimEnd('/') + "\\mtlanguage.ini";
		lang = Global.RW_REG.Check_language();
	}

	public void Update_lang()
	{
		lang = Global.RW_REG.Check_language();
	}

	public string Translation(string Text)
	{
		GetPrivateProfileString(lang, Text, Default, outStr, Size, path);
		return outStr.ToString();
	}
}
