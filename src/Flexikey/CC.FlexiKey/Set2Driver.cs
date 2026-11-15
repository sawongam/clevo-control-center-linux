using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CCenter.Platform;

namespace CC.FlexiKey;

/// <summary>
/// STUB of the Windows keyboard/mouse filter driver wrapper (kb.dll / HKFltrLib).
/// Same public method names as the original. Instead of LoadLibrary("kb.dll") + GetProcAddress(name)
/// every would-be native call is appended to <see cref="Calls"/> and to
/// ConfigDir/flexikey-driver.log (one line per call: function name + decoded arguments).
/// A later phase replaces <see cref="Native"/> with a uinput/evdev root daemon client. See PORT_NOTES.md.
/// </summary>
public class Set2Driver
{
	public struct ClevoKB { public int VK_Key; public int DownDelay; public int UpDelay; }
	public struct ClevoKeyEx { public int key; public int status; public int DelayTime; }
	public struct KeyboardFilterLog { public short KeyCode; public short Reserved; public short MakeCount; public short BreakCount; }
	public enum ClevoMouse { HKFLTRBTN_LEFT = 1, HKFLTRBTN_RIGHT = 4, HKFLTRBTN_MIDDLE = 0x10, HKFLTRBTN_BACKWARD = 0x40, HKFLTRBTN_FORWARD = 0x100 }

	/// <summary>Every call the UI would have made to kb.dll.</summary>
	public static readonly List<string> Calls = new List<string>();
	static readonly object Lock = new object();
	private bool bIsHidMouseExist = true;
	public bool IsHidMouseExist { set { bIsHidMouseExist = value; } }

	/// <summary>Stand-in for LoadLibrary+GetProcAddress+call. Returns true (success).</summary>
	static bool Native(string func, string args = "")
	{
		var line = func + "(" + args + ")";
		lock (Lock)
		{
			Calls.Add(line);
			try { File.AppendAllText(Path.Combine(CcPaths.ConfigDir, "flexikey-driver.log"), line + "\n"); } catch { }
		}
		return true;
	}
	static string KB(ClevoKB[] a) { var sb = new StringBuilder("["); foreach (var k in a) sb.Append($"{{vk={k.VK_Key},down={k.DownDelay},up={k.UpDelay}}}"); return sb.Append(']').ToString(); }
	static string KBn(ClevoKB[] a, int n) { var sb = new StringBuilder("["); for (int i = 0; i < n && i < a.Length; i++) sb.Append($"{{vk={a[i].VK_Key},down={a[i].DownDelay},up={a[i].UpDelay}}}"); return sb.Append(']').ToString(); }
	static string KX(ClevoKeyEx[] a) { var sb = new StringBuilder("["); foreach (var k in a) { if (k.key == 0 && k.status == 0 && k.DelayTime == 0) continue; sb.Append($"{{key={k.key},status={k.status},delay={k.DelayTime}}}"); } return sb.Append(']').ToString(); }

	public Set2Driver() { }

	public bool SetMacroKey(KeyInfo info)
	{
		if (info.Device == KeyGlobal.DeviceType.keyboard) return SetKbMacroKeyEx(info);
		if (info.Device == KeyGlobal.DeviceType.mouse) return SetMouseMacroKeyEx(info);
		return false;
	}
	public bool SetLaunchApp(KeyInfo info)
	{
		if (info.Device == KeyGlobal.DeviceType.keyboard) return SetKbLaunchApp(info);
		if (info.Device == KeyGlobal.DeviceType.mouse) return SetMouseLaunchApp(info);
		return false;
	}
	public bool SetTextMacro(KeyInfo info)
	{
		if (info.Device == KeyGlobal.DeviceType.keyboard) return SetKbTextMacro(info);
		if (info.Device == KeyGlobal.DeviceType.mouse) return SetMouseTextMacro(info);
		return false;
	}
	public bool SetKeyDisable(KeyInfo info)
	{
		if (info.Device == KeyGlobal.DeviceType.keyboard) return SetKbKeyDisable(info);
		if (info.Device == KeyGlobal.DeviceType.mouse) return SetMouseKeyDisable(info);
		return false;
	}

	public bool SetKbEnableFilter() => Native("EnableFilter");
	public bool SetKbDisableFilter() => Native("DisableFilter");
	public bool SetKbCleanFilter() => Native("ClearFilterMacro");
	public bool SetMouseEnableFilter() => Native("EnableFilter_Mouse");
	public bool SetMouseDisableFilter() => Native("DisableFilter_Mouse");
	public bool SetMouseCleanFilter() => Native("ClearFilterMacro_Mouse");
	public static bool StartKbSta() => Native("StartFilterLog");
	public static bool StopKbSta() => Native("StopFilterLog");
	/// <summary>Stub: CountFilterLog()/GetFilterLog(i) -> no statistics recorded.</summary>
	public static List<KbStaClass> GetKbStaLog() { Native("CountFilterLog"); return new List<KbStaClass>(); }

	private bool SetKbMacroKeyEx(KeyInfo keyInfo)
	{
		try
		{
			ClevoKB[] array = new ClevoKB[2];
			bool flag = keyInfo.IsKeypressDelay;
			array[0].VK_Key = keyInfo.KeyCode;
			ClevoKeyEx[] array2 = SetupMacroKeyEx(keyInfo.MacroKey, flag);
			Native("CreateMacroKeyEx", "Hotkey=" + KB(array) + ", Macrokey=" + KX(array2));
			array[0].VK_Key = 57463; array[1].VK_Key = keyInfo.KeyCode;
			Native("CreateMacroKeyEx", "Hotkey=" + KB(array) + ", Macrokey=" + KX(array2));
			array[0].VK_Key = 57464; array[1].VK_Key = keyInfo.KeyCode;
			Native("CreateMacroKeyEx", "Hotkey=" + KB(array) + ", Macrokey=" + KX(array2));
		}
		catch { return false; }
		return true;
	}
	private bool SetKbMacroKey(KeyInfo keyInfo)
	{
		try
		{
			ClevoKB[] array = new ClevoKB[2];
			array[0].VK_Key = keyInfo.KeyCode;
			ClevoKB[] array2 = SetupMacroKey(keyInfo.MacroKey, 16, keyInfo.IsKeypressDelay);
			Native("CreateMacroKey", "Hotkey=" + KB(array) + ", Macrokey=" + KB(array2));
		}
		catch { return false; }
		return true;
	}
	private bool SetKbTextMacro(KeyInfo keyInfo)
	{
		ClevoKB[] array = new ClevoKB[2];
		array[0].VK_Key = keyInfo.KeyCode;
		ClevoKB[] array2 = SetupMacroKey(keyInfo.StartKey, 6, bTimeRecord: false);
		ClevoKB[] array3 = SetupMacroKey(keyInfo.SendKey, 6, bTimeRecord: false);
		byte[] bytes = Encoding.Unicode.GetBytes(keyInfo.TextContent);
		string a = "Startkey=" + KB(array2) + ", mystring=UTF16LE[" + Convert.ToHexString(bytes) + "], Endkey=" + KB(array3);
		Native("CreateStringKey", "Hotkey=" + KB(array) + ", " + a);
		array[0].VK_Key = 57463; array[1].VK_Key = keyInfo.KeyCode;
		Native("CreateStringKey", "Hotkey=" + KB(array) + ", " + a);
		array[0].VK_Key = 57464; array[1].VK_Key = keyInfo.KeyCode;
		Native("CreateStringKey", "Hotkey=" + KB(array) + ", " + a);
		return true;
	}
	private bool SetKbLaunchApp(KeyInfo keyInfo)
	{
		try
		{
			ClevoKB[] array = new ClevoKB[2];
			array[0].VK_Key = keyInfo.KeyCode;
			int num = keyInfo.AppEventId;
			Native("CreateFilterSysEvent", "Hotkey=" + KB(array) + ", EventID=" + num);
			array[0].VK_Key = 57463; array[1].VK_Key = keyInfo.KeyCode;
			Native("CreateFilterSysEvent", "Hotkey=" + KB(array) + ", EventID=" + num);
			array[0].VK_Key = 57464; array[1].VK_Key = keyInfo.KeyCode;
			Native("CreateFilterSysEvent", "Hotkey=" + KB(array) + ", EventID=" + num);
		}
		catch { return false; }
		return true;
	}
	private bool SetKbKeyDisable(KeyInfo keyInfo)
	{
		try
		{
			ClevoKB[] array = new ClevoKB[2];
			ClevoKB[] array2 = new ClevoKB[16];
			array[0].VK_Key = keyInfo.KeyCode;
			Native("CreateMacroKey", "Hotkey=" + KB(array) + ", Macrokey=" + KBn(array2, 1));
			array[0].VK_Key = 57463; array[1].VK_Key = keyInfo.KeyCode;
			Native("CreateMacroKey", "Hotkey=" + KB(array) + ", Macrokey=" + KBn(array2, 1));
			array[0].VK_Key = 57464; array[1].VK_Key = keyInfo.KeyCode;
			Native("CreateMacroKey", "Hotkey=" + KB(array) + ", Macrokey=" + KBn(array2, 1));
		}
		catch { return false; }
		return true;
	}
	private bool SetMouseMacroKeyEx(KeyInfo keyInfo)
	{
		try
		{
			ClevoKeyEx[] array = SetupMacroKeyEx(keyInfo.MacroKey, keyInfo.IsKeypressDelay);
			Native("CreateMacroKey_MouseEx", "MyBtn=" + keyInfo.KeyCode + ", Macrokey=" + KX(array));
		}
		catch { return false; }
		return true;
	}
	private bool SetMouseMacroKey(KeyInfo keyInfo)
	{
		try
		{
			ClevoKB[] array = SetupMacroKey(keyInfo.MacroKey, 16, keyInfo.IsKeypressDelay);
			Native("CreateMacroKey_Mouse", "MyBtn=" + keyInfo.KeyCode + ", Macrokey=" + KB(array));
		}
		catch { return false; }
		return true;
	}
	private bool SetMouseTextMacro(KeyInfo keyInfo)
	{
		ClevoKB[] array = SetupMacroKey(keyInfo.StartKey, 6, bTimeRecord: false);
		ClevoKB[] array2 = SetupMacroKey(keyInfo.SendKey, 6, bTimeRecord: false);
		byte[] bytes = Encoding.Unicode.GetBytes(keyInfo.TextContent);
		Native("CreateStringKey_Mouse", "MyBtn=" + keyInfo.KeyCode + ", Startkey=" + KB(array) + ", mystring=UTF16LE[" + Convert.ToHexString(bytes) + "], Endkey=" + KB(array2));
		return true;
	}
	private bool SetMouseLaunchApp(KeyInfo keyInfo)
	{
		Native("CreateFilterSysEvent_Mouse", "MyBtn=" + keyInfo.KeyCode + ", EventID=" + keyInfo.AppEventId);
		return true;
	}
	private bool SetMouseKeyDisable(KeyInfo keyInfo)
	{
		try
		{
			ClevoKB[] array = new ClevoKB[16];
			Native("CreateMacroKey_Mouse", "MyBtn=" + keyInfo.KeyCode + ", Macrokey=" + KBn(array, 1));
		}
		catch { return false; }
		return true;
	}

	private ClevoKeyEx[] SetupMacroKeyEx(List<MacrokeyContent> macroKeys, bool bTimeRecord)
	{
		int num = 0;
		num = ((macroKeys.Count <= 32) ? macroKeys.Count : 32);
		ClevoKeyEx[] array = new ClevoKeyEx[255];
		array[0].key = macroKeys[0].KeyCode;
		array[0].status = 1;
		array[0].DelayTime = macroKeys[1].KeyDelay;
		int num2 = 1;
		for (int i = 1; i < num - 1; i++)
		{
			if (macroKeys[i].KeyCode == 0)
			{
				continue;
			}
			array[i].key = macroKeys[i].KeyCode;
			if (macroKeys[i].KeyMakeBreak == KeyGlobal.MakeBreak.KeyMake)
			{
				array[i].status = 1;
			}
			else
			{
				array[i].status = 0;
			}
			if (bTimeRecord)
			{
				if (i + 1 >= macroKeys.Count)
				{
					array[i].DelayTime = 0;
				}
				else
				{
					array[i].DelayTime = macroKeys[i + 1].KeyDelay;
				}
			}
			else
			{
				array[i].DelayTime = 0;
			}
			num2++;
		}
		array[num2].key = macroKeys[num2].KeyCode;
		array[num2].status = 0;
		array[num2].DelayTime = 0;
		return array;
	}

	private ClevoKB[] SetupMacroKey(List<MacrokeyContent> macroKeys, int length, bool bTimeRecord)
	{
		List<ClevoKB> list = new List<ClevoKB>();
		ClevoKB[] array = new ClevoKB[length];
		int num = 0;
		num = ((macroKeys.Count <= length * 2) ? macroKeys.Count : (length * 2));
		if (macroKeys[0].KeyCode == 0)
		{
			ClevoKB clevoKB = new ClevoKB
			{
				DownDelay = 0,
				UpDelay = 0,
				VK_Key = 0
			};
		}
		else
		{
			for (int i = 0; i < num; i++)
			{
				if (macroKeys[i].KeyMakeBreak != KeyGlobal.MakeBreak.KeyBreak)
				{
					ClevoKB item = new ClevoKB
					{
						VK_Key = macroKeys[i].KeyCode
					};
					if (i + 1 >= macroKeys.Count)
					{
						item.DownDelay = 0;
					}
					else
					{
						item.DownDelay = macroKeys[i + 1].KeyDelay;
					}
					if (i + 2 >= num)
					{
						item.UpDelay = 0;
					}
					else
					{
						item.UpDelay = macroKeys[i + 2].KeyDelay;
					}
					list.Add(item);
				}
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			array[j].VK_Key = list[j].VK_Key;
			if (bTimeRecord)
			{
				array[j].DownDelay = list[j].DownDelay;
				array[j].UpDelay = list[j].UpDelay;
			}
			else
			{
				array[j].DownDelay = 0;
				array[j].UpDelay = 0;
			}
		}
		return array;
	}

}
