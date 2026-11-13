using FnKey.Device;

namespace FnKey;

public class Global
{
	public class FanArgs
	{
		public byte T1;

		public byte T2;

		public byte T3;

		public byte T4;

		public byte T2_Default;

		public byte T3_Default;

		public byte D1;

		public byte D2;

		public byte D3;

		public byte D4;

		public byte D2_Default;

		public byte D3_Default;

		public int R12;

		public int R23;

		public int R34;

		public int rpm;

		public int local;

		public int remote;

		public byte duty;
	}

	public enum FnKeyStatus
	{
		FirstRun = 0,
		Init = 1,
		Sleep = 2,
		Resume = 3,
		Ready = 4,
		Unknow = 99
	}

	public enum RefreshStatus
	{
		PowerModeChange = 0,
		ACDC = 1,
		UI_Check_Event = 2,
		FirstRun = 3,
		OnStart = 4,
		OnResume = 5,
		Done = 99
	}

	public static DCHU dchu = new DCHU();

	public static ModeBox PerformanceMode = new ModeBox();

	public static ModeBox EntertainmentMode = new ModeBox();

	public static ModeBox PwrsavingeMode = new ModeBox();

	public static ModeBox QuietMode = new ModeBox();

	public static Features features = new Features();

	public static KBModule KB = new KBModule();

	public static FAN fan = new FAN();

	public static GPU gpu = new GPU();

	public static RWReg RW_REG = new RWReg();

	public static Flexikey flexikey = new Flexikey();

	public static battery BAT_PowerStatus = new battery();

	public static Brightness brightness = new Brightness();

	public static DeviceControl DevControl = new DeviceControl();

	public static DeviceChangeEvent DevEvent = new DeviceChangeEvent();

	public static FnKeyStatus fnKeyStatus;

	public static RefreshStatus refreshRateStatus;
}
