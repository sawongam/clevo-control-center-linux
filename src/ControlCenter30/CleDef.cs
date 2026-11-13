namespace Clevo;

public class CleDef
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
		logo,
		lightbar
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
		flash,
		all
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
		TP_R = 10,
		TP_G = 11,
		TP_B = 12,
		Logo_R = 13,
		Logo_G = 14,
		Logo_B = 15,
		LightBar_R = 16,
		LightBar_G = 17,
		LightBar_B = 18,
		Left = 19,
		Mid = 20,
		Right = 21,
		Tp = 22,
		Logo = 23,
		AutoLightBar = 24,
		Status = 25,
		Brightness = 26,
		Mode = 27,
		SleepLEDTimer = 28,
		All_R = 30,
		All_G = 31,
		All_B = 32
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
		NoLed = 0,
		White = 1,
		RGB = 2,
		Perkey = 3,
		RGB15Color = 6,
		RGB15ColorCustom = 22
	}

	public enum Brightness
	{
		Off,
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
		BatteryFull = 268,
		Docking = 269,
		ACOut = 270,
		FN_3 = 271,
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

	public enum ModeType
	{
		quiet,
		pwrsaving,
		performance,
		entertainment
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
		BatteryRefresh = 285,
		HeadPhonePollingOn = 288,
		HeadPhonePollingOff = 289,
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
		KbPerkeyReactive = 545,
		KbPerkeyReactiveRandom = 546,
		KbSelectBootEffect = 547,
		FAClose = 576,
		FAStart = 577,
		FAConnect = 578,
		FADisconnect = 579,
		FAUpdateData = 580
	}

	public enum AppSetting
	{
		PowerMode = 257,
		UI_language = 258,
		ShowOSD = 259,
		CameraStatus = 262,
		CC30AppLaunch = 263,
		Major = 512,
		Minor = 513,
		Rev = 514,
		KBType = 516,
		KBLanguage = 517,
		SupportOverrideBootEffect = 518,
		OverrideBootEffect = 519,
		LedKBAppLaunch = 520,
		DefEffect = 528,
		DefKBSpeed = 529,
		DefKBBrightness = 530,
		DefSleepTimerTrigger = 531,
		DefSleepTimerHour = 532,
		DefSleepTimerMinute = 533,
		DefSleepTimerSecond = 534,
		DefColorRed = 535,
		DefColorGreen = 536,
		DefColorBlue = 537,
		Effect = 544,
		StaticStatus = 545,
		KBSleep = 546,
		KBBrightness = 547,
		KBSleepTimerTrigger = 548,
		KBSleepTimerHour = 549,
		KBSleepTimerMinute = 550,
		KBSleepTimerSecond = 551,
		RippleStatus = 560,
		RippleRed = 561,
		RippleGreen = 562,
		RippleBlue = 563,
		BreathStatus = 564,
		BreathRed = 565,
		BreathGreen = 566,
		BreathBlue = 567,
		BlinkStatus = 568,
		BlinkRed = 569,
		BlinkGreen = 570,
		BlinkBlue = 571,
		Status = 592,
		AllRed = 593,
		AllGreen = 594,
		AllBlue = 595,
		LeftStatus = 596,
		LeftRed = 597,
		LeftGreen = 598,
		LeftBlue = 599,
		MidStatus = 600,
		MidRed = 601,
		MidGreen = 602,
		MidBlue = 603,
		RightStatus = 604,
		RightRed = 605,
		RightGreen = 606,
		RightBlue = 607,
		TPStatus = 608,
		TPRed = 609,
		TPGreen = 610,
		TPBlue = 611,
		LogoStatus = 612,
		LogoRed = 613,
		LogoGreen = 614,
		LogoBlue = 615,
		LightBarStatus = 616,
		LightBarRed = 617,
		LightBarGreen = 618,
		LightBarBlue = 619,
		R0C0Red = 640
	}

	public enum System
	{
		WM_POWERBROADCAST = 536,
		PBT_APMSUSPEND = 4,
		PBT_APMRESUMESUSPEND = 7,
		PBT_APMRESUMEAUTOMATIC = 18
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

	public enum Customer
	{
		Standard,
		Tronic5,
		MCJ,
		Seager,
		Gigabyte,
		EPSON,
		Santech,
		POSITIVO
	}

	public enum ClevoID
	{
		NH5xAx = 20720,
		NL40CU1 = 16577,
		NH5xDx = 34101,
		NH7xDx = 34102,
		NH5xDx_G = 35381,
		NH7xDx_G = 35382,
		NP5xDx = 34408,
		NV40ME_MB = 16408,
		NLxxRU = 42240,
		NR40BU = 33320,
		L140CU = 5121,
		NS50MU = 20897,
		NS50MU_2 = 20898,
		NH5xDP = 20723,
		NH70Ex_Y = 28914,
		NH70Ex_Y_2 = 28916,
		NH50Ex_Y = 20722,
		NH50Ex_Y_2 = 20725
	}
}
