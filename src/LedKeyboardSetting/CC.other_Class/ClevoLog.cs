using System;
using System.IO;
namespace CC.other_Class;

/// <summary>Original wrote to the Windows EventLog "PowerBIOSServer_Out" (read by DCHUService). No service on Linux: optional file log.</summary>
internal class ClevoLog
{
    public static void Write(string str)
    {
        var p = Environment.GetEnvironmentVariable("CCENTER_KB_LOG");
        if (p != null) try { File.AppendAllText(p, str.Replace("\n", " | ") + "\n"); } catch { }
    }
}
