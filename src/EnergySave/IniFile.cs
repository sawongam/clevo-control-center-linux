using System.IO;
using System.Reflection;
using CCenter.Platform;
using System.Text;

namespace DesktopBridge_EnergySave;

internal class IniFile
{
	private string Path;

	private string EXE = Assembly.GetExecutingAssembly().GetName().Name;

	private static long WritePrivateProfileString(string Section, string Key, string Value, string FilePath) => Ini.WritePrivateProfileString(Section, Key, Value, FilePath) ? 1 : 0;

	private static int GetPrivateProfileString(string Section, string Key, string Default, StringBuilder RetVal, int Size, string FilePath) => Ini.GetPrivateProfileString(Section, Key, Default, RetVal, Size, FilePath);

	public IniFile(string FileName, string IniPath = null)
	{
		string text = CcPaths.Map(IniPath + "\\" + FileName);
		if (File.Exists(text))
		{
			Path = new FileInfo(text ?? (EXE + ".ini")).FullName.ToString();
			return;
		}
		DirectoryInfo directoryInfo = new DirectoryInfo(CcPaths.Map(IniPath));
		if (!directoryInfo.Exists)
		{
			Directory.CreateDirectory(CcPaths.Map(IniPath));
		}
		File.Create(text).Dispose();
		Path = new FileInfo(text ?? (EXE + ".ini")).FullName.ToString();
	}

	public string Read(string Key, string Section = null)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		GetPrivateProfileString(Section ?? EXE, Key, "", stringBuilder, 255, Path);
		if (stringBuilder.Length == 0)
		{
			return "-1";
		}
		return stringBuilder.ToString();
	}

	public void Write(string Key, string Value, string Section = null)
	{
		WritePrivateProfileString(Section ?? EXE, Key, Value, Path);
	}

	public void DeleteKey(string Key, string Section = null)
	{
		Write(Key, null, Section ?? EXE);
	}

	public void DeleteSection(string Section = null)
	{
		Write(null, null, Section ?? EXE);
	}

	public bool KeyExists(string Key, string Section = null)
	{
		return Read(Key, Section).Length > 0;
	}

	public bool FileExists(string FileName, string IniPath = null)
	{
		string path = CcPaths.Map(IniPath + "\\" + FileName);
		if (File.Exists(path))
		{
			return true;
		}
		return false;
	}
}
