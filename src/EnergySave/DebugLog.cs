namespace DesktopBridge_EnergySave;

/// <summary>Windows EventLog debug output ("OutLog"/"PowerBiosServerLog") does not exist on Linux: no-ops.</summary>
internal class DebugLog
{
	public static bool IsOutLogSourceExist() => false;
	public static bool IsPowerBiosServerLogSourceExist() => false;
	public static bool WriteDbgLog2Outlog(string log) => false;
	public static bool WriteLog2InLog(string log) => false;
}
