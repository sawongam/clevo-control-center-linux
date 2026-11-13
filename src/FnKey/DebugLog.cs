using System;
using System.IO;

namespace FnKey;

// Replaces the Windows EventLog writes ("OutLog"/"PowerBiosServerLog" via DCHUService).
// Out-log text lines are plain diagnostics; on Linux they go to stderr when FNKEY_LOG=1 (or a file path).
// Lines that on Windows were *messages to other CC apps / the service* (clevofnkey^NNN, w_arg254^...) are
// recorded through Notify.* so PORT_NOTES can list them; they are no-ops here (see Notify).
internal class DebugLog
{
	static readonly string Target = Environment.GetEnvironmentVariable("FNKEY_LOG");
	static readonly object Gate = new object();

	static void Emit(string tag, string log)
	{
		if (string.IsNullOrEmpty(Target) || Target == "0") return;
		string line = DateTime.Now.ToString("HH:mm:ss.fff") + " [" + tag + "] " + log;
		lock (Gate)
		{
			if (Target == "1") Console.Error.WriteLine(line);
			else { try { File.AppendAllText(Target, line + "\n"); } catch { } }
		}
	}

	public static void WriteDbgLog2Outlog(string log) => Emit("out", log);

	public static void WriteDbgLog2Inlog(string log) => Emit("in", log);
}

/// <summary>
/// The Windows app talks to the other Control Center apps / DCHUService by writing text lines to the
/// event log ("clevofnkey^201", "w_arg254^C:\...\cc30wk.exe -dlp"...). None of those consumers exist on Linux,
/// so these are logged only (visible with FNKEY_LOG=1).
/// </summary>
internal static class Notify
{
	public static void ToApps(string code) => DebugLog.WriteDbgLog2Outlog("clevofnkey^" + code + "   (notification to LedKeyboardSetting/CC30 UI; no consumer on Linux)");
	public static void ToService(string text) => DebugLog.WriteDbgLog2Inlog(text + "   (request to DCHUService; Windows-only helper, no-op on Linux)");
}
