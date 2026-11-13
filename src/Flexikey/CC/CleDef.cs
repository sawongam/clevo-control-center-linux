namespace Clevo;

internal class CleDef
{
	public enum MessageType
	{
		System,
		Function,
		HotkeyEvent,
		WMI_Execture
	}

	public enum KbPart
	{
		all,
		left,
		mid,
		right,
		touchpad,
		none,
		volume,
		logo
	}

	public enum KB_Mode
	{
		random,
		custom,
		breath,
		cycle,
		wave,
		dance,
		tempo,
		flash
	}

	public enum LEDKB_Command
	{
		SendDataArray = 22,
		SetKBLEDSleepTimer,
		SetLEDKBOnOff,
		SetKbBrightness,
		SetKB_Mode,
		WriteToReg,
		LoadDefault
	}

	public enum Kb_Cmd_Index
	{
		L_R = 1,
		L_G = 2,
		L_B = 3,
		M_R = 4,
		M_G = 5,
		M_B = 6,
		R_R = 7,
		R_G = 8,
		R_B = 9,
		Left = 19,
		Mid = 20,
		Right = 21,
		Tp = 22,
		Logo = 23,
		AutoLightBar = 24,
		Status = 25,
		Brightness = 26,
		Mode = 27,
		SleepLEDTimer = 28
	}

	public enum Kb_Sleep
	{
		OFF = 0,
		sec_15 = 1,
		sec_30 = 2,
		min_1 = 3,
		min_5 = 4,
		custom = 255
	}

	public enum Kb_Type
	{
		NoLed,
		White,
		RGB,
		Perkey
	}

	public enum Brightness
	{
		Disable,
		Lv1,
		Lv2,
		Lv3,
		Lv4,
		Lv5
	}

	public enum HkeyTrayEvent
	{
		HKTrayOnInit = 256,
		HKTrayReady = 257,
		SituationMode = 258,
		TouchPad = 259,
		CCD = 260,
		Airplane = 261,
		FlexiKey = 262,
		CapsLk = 263,
		ScrLk = 264,
		NumLk = 265,
		Fan = 266,
		Changed = 267,
		KeyboardOnInit = 512,
		KeyboardReady = 513,
		KeyboardStatus = 514,
		KbBrightness = 515,
		WhiteKbBrightness = 516,
		KbStatus = 517,
		PerKbBrightness = 518,
		FA_Close = 576,
		FA_Start = 577,
		FA_Connect = 578,
		FA_Disconnect = 579,
		FA_UpdateData = 580
	}

	public enum CCToHKTray
	{
		CameraStatus = 257,
		TouchPadStatus = 258,
		MSHybridStatus = 259,
		LcdOff = 260,
		FanMode = 261,
		WinKey = 262,
		Situation = 263,
		AirplaneMode = 264,
		CapsLck = 265,
		ScrLck = 266,
		NumLck = 267,
		UpdateLiveInfoOn = 268,
		UpdateLiveInfoOff = 269,
		HeadPhone = 270,
		CcReady = 271,
		CcClose = 272,
		MemoryVoltage = 273,
		KbBrightness = 526,
		KbSpeed = 527,
		SleepLEDTimer = 528,
		KbPerkeyClearAll = 529,
		KbPerkeyBrightness = 530,
		KbPerkeyStatic = 531,
		KbPerkeyBreath = 532,
		KbPerkeyBreathRandom = 533,
		KbPerkeyBlink = 534,
		KbPerkeyBlinkRandom = 535,
		KbPerkeyWave = 536,
		KbPerkeyRandom = 537,
		KbPerkeyScan = 538,
		KbPerkeySnake = 539,
		KbPerkeyRipple = 540,
		KbPerkeyRippleRandom = 541,
		KbSendDataArray = 542,
		KbRgbMode = 543,
		KbRgbLoadDefault = 544,
		FAClose = 576,
		FAStart = 577,
		FAConnect = 578,
		FADisconnect = 579,
		FAUpdateData = 580
	}

	public enum UI_Language : byte
	{
		system,
		en_us,
		en_gb,
		es,
		de,
		it,
		ja,
		ko,
		zh_cn,
		zh_tw,
		fr_fr,
		fr_ca,
		pt_br,
		pt_pt,
		tr_tr,
		th_th,
		vi_vn,
		ru,
		ja_ep
	}
}
