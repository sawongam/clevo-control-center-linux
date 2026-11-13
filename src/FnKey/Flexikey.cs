using System;

namespace FnKey;

// Flexikey (combo keys): only the persisted on/off flags are ported. The key-filter driver (kb.dll,
// Set2Driver, C:\ComboKey\profile.xml) is Windows-only -> Features.FlexikeyInit/SetKeybordMouseEnble are stubs.
public class Flexikey
{
	public void OnStart()
	{
		Set_Flexikey_kb_status(Get_Flexikey_kb_status());
		Set_Flexikey_mouse_status(Get_Flexikey_mouse_status());
	}

	public int Get_Flexikey_kb_status()
	{
		try { return Global.dchu.ReadAppSettings(1, 8, 1)[0]; } catch { return 0; }
	}

	public int Get_Flexikey_mouse_status()
	{
		try { return Global.dchu.ReadAppSettings(1, 9, 1)[0]; } catch { return 0; }
	}

	public int Set_Flexikey_kb_status(int value)
	{
		Global.dchu.WriteAppSettings(1, 8, 1, BitConverter.GetBytes(value));
		return 1;
	}

	public int Set_Flexikey_mouse_status(int value)
	{
		Global.dchu.WriteAppSettings(1, 9, 1, BitConverter.GetBytes(value));
		return 1;
	}
}
