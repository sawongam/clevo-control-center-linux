using System;
using CCenter.Platform;

namespace CPUOC30.Model;

/// <summary>
/// In-process replacement for CC30_BG.exe (CPUOC_Background). It listened on the in-log:
///  cpuoc_bg_104 start per-second OC info (wrote CPU freq/volt/util/temp/ram clock to Insyde page 6)  -> not needed: the UI reads Linux sources directly
///  cpuoc_bg_105 stop per-second info                                                                   -> no-op
///  cpuoc_bg_106 apply the Insyde-buffer settings (PL1/PL2/time/VR/offsets/ratios) to XTU              -> OC WRITE, gated, never applied on Linux
///  107/108/10a/10b/10c  (reset / apply from FnKey / BIOS defaults / ratio offsets)                      -> OC WRITES, gated, not applied
/// </summary>
internal static class CpuOcBackground
{
    public static void OnInLog(string log)
    {
        var t = log.ToLower();
        if (!t.Contains("cpuoc_bg_")) return;
        if (t.Contains("104") || t.Contains("105")) return;
        if (t.Contains("106") || t.Contains("107") || t.Contains("108") || t.Contains("10a") || t.Contains("10b") || t.Contains("10c"))
            HardwareGate.GuardOcWrite(() => DebugLog.Log("CPUOC_BG " + t + ": would apply OC info to XTU (not available on Linux)"));
    }
}
