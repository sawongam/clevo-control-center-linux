using System;

namespace FanSpeedSetting;

/// <summary>Original wrote to the Windows event log read by DCHUService; no equivalent on Linux. Optional stderr trace with RIPPLE_CC_DEBUGLOG=1.</summary>
internal class DebugLog
{
    static readonly bool On = Environment.GetEnvironmentVariable("RIPPLE_CC_DEBUGLOG") == "1";
    public static void WriteDbgLog2Outlog(string log) { if (On) Console.Error.WriteLine("DebugFan:" + log); }
    public static void SendOutlog_Event(string log) { if (On) Console.Error.WriteLine("clevofan^" + log); }
}
