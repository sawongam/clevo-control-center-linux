using System;
using System.Diagnostics;
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

	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => CCenter.Platform.Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);

	

	public language()
	{
		path = AppContext.BaseDirectory.TrimEnd('/') + "\\mtlanguage.ini";
		lang = Global.RW_REG.Check_language();
	}

	public string Translation(string Text)
	{
		GetPrivateProfileString(lang, Text, Default, outStr, Size, path);
		return outStr.ToString();
	}
}
