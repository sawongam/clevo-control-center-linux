namespace ControlCenter30;

public class SupportBit
{
	public enum Customer
	{
		Standard,
		Tronic5,
		MCJ,
		Seager,
		Gigabyte,
		EPSON,
		Santech,
		POSITIVO,
		NEXCO,
		HANSUNG
	}

	public int SupportBIOSFeatureList_Version = 0;

	public bool CPUOC = false;

	public bool DTT = false;

	public bool AMD_ManualMode = false;

	public bool UWP_PgUpDnKey = false;

	public bool TurboFan = false;

	public bool PowerModeBit = false;

	public bool PowerModeBit_Quite = false;

	public bool PowerModeBit_PowerSaving = false;

	public bool PowerModeBit_Performance = false;

	public bool PowerModeBit_Entertainment = false;

	public int PowerModeUI_ID = 0;

	public bool MonitorLowRefreshRate = false;

	public bool APP_FanSpeedSetting = false;

	public bool APP_LEDKeyboardSetting = false;

	public bool APP_Flexikey = false;
}
