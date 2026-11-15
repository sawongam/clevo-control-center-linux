using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CCenter.Platform;

namespace GPUOverclocking;

public class RWIni
{
	private string path;
	private StringBuilder outStr = new StringBuilder(255);
	private string Default = "null";
	private int Size = 255;
	private string lang = "en-US";

	public RWIni()
	{
		path = AppContext.BaseDirectory;
		lang = Global.RW_REG.Check_language();
	}

	public string Translation(string Text)
	{
		Ini.GetPrivateProfileString(lang, Text, Default, outStr, Size, Path.Combine(path, "mtlanguage.ini"));
		return outStr.ToString();
	}

	public string Read_OEM(string section, string key)
	{
		Ini.GetPrivateProfileString(section, key, Default, outStr, Size, Path.Combine(path, "oem.ini"));
		return outStr.ToString();
	}

	public string Read_Custom(string section, string key)
	{
		Ini.GetPrivateProfileString(section, key, Default, outStr, Size, Path.Combine(path, "Custom.ini"));
		return outStr.ToString();
	}

	public string Read_Batery(string section, string key)
	{
		Ini.GetPrivateProfileString(section, key, Default, outStr, Size, Path.Combine(path, "Battery.ini"));
		return outStr.ToString();
	}
}
