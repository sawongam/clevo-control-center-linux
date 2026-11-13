namespace CC.FlexiKey;

public class KeyGlobal
{
	public enum KbColorPart
	{
		kball,
		left,
		mid,
		right,
		touchpad,
		lightbar,
		allpart,
		a_cover
	}

	public enum KeyActionType
	{
		none,
		keypress,
		launch,
		text,
		disable
	}

	public enum DeviceType
	{
		none,
		keyboard,
		mouse
	}

	public enum MakeBreak
	{
		KeyMake,
		KeyBreak
	}

	public enum StaLevel
	{
		Sta1 = 1,
		Sta2 = 2,
		Sta3 = 3,
		Sta4 = 4,
		Sta5 = 5,
		StaDisable = 9
	}

	public static HookKeysTable hookKeysTable;

	public static int MacroKeyMax = 24;
}
