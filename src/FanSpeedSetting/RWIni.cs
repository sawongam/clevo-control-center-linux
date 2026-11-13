using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using RippleCC.Platform;
using System;
using System.Text;

namespace FanSpeedSetting;

public class RWIni
{
	private string path;

	private StringBuilder outStr = new StringBuilder(255);

	private string Default = "null";

	private int Size = 255;

	private string lang = "en-US";

	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);



	public RWIni()
	{
		path = AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar);
		lang = Global.RW_REG.Check_language();
	}

	public string Translation(string Text)
	{
		GetPrivateProfileString(lang, Text, Default, outStr, Size, Path.Combine(path, "mtlanguage.ini"));
		return outStr.ToString();
	}

	public string Read_OEM(string section, string key)
	{
		GetPrivateProfileString(section, key, Default, outStr, Size, Path.Combine(path, "oem.ini"));
		return outStr.ToString();
	}

	public string Read_Custom(string section, string key)
	{
		GetPrivateProfileString(section, key, Default, outStr, Size, Path.Combine(path, "Custom.ini"));
		return outStr.ToString();
	}

	public string Read_Batery(string section, string key)
	{
		GetPrivateProfileString(section, key, Default, outStr, Size, Path.Combine(path, "Battery.ini"));
		return outStr.ToString();
	}

	// Read_OEM_list (GetPrivateProfileSection) is not used by FanSpeedSetting
	public List<string> Read_OEM_list(string section) => null;
}
