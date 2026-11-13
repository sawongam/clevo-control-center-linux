using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace CC;

public class RWIni
{
	private string path;

	private StringBuilder outStr = new StringBuilder(255);

	private string Default = "null";

	private int Size = 255;

	private string lang = "en-US";

	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => RippleCC.Platform.Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);

	

	public RWIni()
	{
		path = AppContext.BaseDirectory.TrimEnd('/');
		lang = Global.RW_REG.Check_language();
	}

	public string Translation(string Text)
	{
		GetPrivateProfileString(lang, Text, Default, outStr, Size, path + "\\mtlanguage.ini");
		return outStr.ToString();
	}

	public string Read_OEM(string section, string key)
	{
		GetPrivateProfileString(section, key, Default, outStr, Size, path + "\\oem.ini");
		return outStr.ToString();
	}

	public string Read_Custom(string section, string key)
	{
		GetPrivateProfileString(section, key, Default, outStr, Size, path + "\\Custom.ini");
		return outStr.ToString();
	}
}
