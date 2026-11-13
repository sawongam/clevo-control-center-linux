using System.Diagnostics;
namespace CC;
/// <summary>Original wrote to the Windows "OutLog" event log read by the CC tray/service. No such log on Linux:
/// the entries are only emitted to Debug output (and the RIPPLE_CC_MOCK_LOG-style trace is on the DCHU calls).</summary>
internal class DebugLog
{
	public static void WriteDbgLog2Outlog(string log) => Debug.WriteLine("OutLog: Debug BAT Utility : " + log);
	public static void WriteLog2Outlog(string log) => Debug.WriteLine("OutLog: " + log);
}
