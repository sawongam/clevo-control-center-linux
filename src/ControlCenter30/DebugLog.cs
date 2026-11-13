using System;
using System.IO;

namespace ControlCenter30;

/// <summary>
/// Original: Windows Event Log IPC ("OutLog" read by FnKey/DCHUService, "PowerBiosServerLog" read by the service).
/// Linux port: an in-process bus. Messages for the service side (clevocc30^...) are executed directly by
/// <see cref="ServiceBridge"/>; every message is also appended to $RIPPLE_CC_LOG when set.
/// </summary>
internal class DebugLog
{
	public static event Action<string> OutLogWritten;

	static readonly string LogPath = Environment.GetEnvironmentVariable("RIPPLE_CC_LOG");

	static void File_(string kind, string log)
	{
		if (LogPath == null) return;
		try { File.AppendAllText(LogPath, $"{DateTime.Now:HH:mm:ss.fff} {kind} {log}\n"); } catch { }
	}

	public static bool IsOutLogSourceExist() => true;

	public static bool IsPowerBiosServerLogSourceExist() => true;

	public static bool WriteDbgLog2Outlog(string log)
	{
		File_("OUT", log);
		try { OutLogWritten?.Invoke(log); } catch (Exception ex) { Console.Error.WriteLine("OutLog handler: " + ex); }
		return true;
	}

	public static bool WriteLog2InLog(string log)
	{
		File_("IN ", log);
		return true;
	}
}
