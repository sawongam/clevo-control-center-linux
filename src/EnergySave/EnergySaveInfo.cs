namespace DesktopBridge_EnergySave;

internal class EnergySaveInfo
{
	public string Name = "";

	public bool isSchedule1_Enable = true;

	public int Schedule1_Start = 0;

	public int Schedule1_End = 0;

	public bool isSchedule2_Enable = true;

	public int Schedule2_Start = 0;

	public int Schedule2_End = 0;

	public bool isEveryday = true;

	public bool isSunday = false;

	public bool isMonday = false;

	public bool isTuesday = false;

	public bool isWednesday = false;

	public bool isThursday = false;

	public bool isFriday = false;

	public bool isSaturday = false;

	public bool isBatterySettingAuto = true;

	public int ChargeLimit = 10;

	public int DischargeLimit = 90;
}
