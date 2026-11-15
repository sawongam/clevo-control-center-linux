using System;
using System.IO;

namespace CPUOC30.Model;

/// <summary>
/// The Windows app talked to CC30_BG.exe through the Windows Event Log ("cpuoc_bg_104" ...).
/// Here the in-log is dispatched in-process to <see cref="CpuOcBackground"/>; the out-log is dropped (stderr with CCENTER_MOCK_LOG).
/// </summary>
public class DebugLog
{
    public static bool IsOutLogSourceExist() => false;
    public static bool IsPowerBiosServerLogSourceExist() => false;
    public static bool WriteDbgLog2Outlog(string log) { Trace("OUT " + log); return true; }
    public static bool WriteDbgLog2Inlog(string log) { Trace("IN  " + log); CpuOcBackground.OnInLog(log); return true; }
    static void Trace(string s)
    {
        var p = Environment.GetEnvironmentVariable("CCENTER_MOCK_LOG");
        if (p != null) try { File.AppendAllText(p, "cpuoc-log: " + s + "\n"); } catch { }
    }
    public static void Log(string s) => Trace(s);
}
