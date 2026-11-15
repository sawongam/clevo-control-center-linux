using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using CCenter.Platform;

namespace CPUOC30;

/// <summary>Minimal stand-in for Intel's ClientTuningControl (IntelOverclockingSDK.dll).</summary>
public class ClientTuningControl
{
    public uint Id;
    public bool Enabled = true;
    public bool RequiresReboot;
    public decimal ActiveValue;
    public decimal DefaultValue;
    public List<decimal> SupportedValues = new();
}

/// <summary>
/// Replacement for the Intel XTU SDK + XTU3SERVICE (Windows only, MSR/MCHBAR access).
/// Controls are STUBBED with plausible values for an i7-11800H (8C/16T, 45 W): see PORT_NOTES.md.
/// Reads return the stub; Tune() is an overclock write and goes through HardwareGate and, even when
/// unlocked, is only logged: this port never talks to MSRs / XTU.
/// </summary>
public static class XtuStub
{
    public static readonly List<ClientTuningControl> Controls = Build();
    public const decimal MaxDecimal = decimal.MaxValue; // XTU "auto" marker

    static List<decimal> Range(int a, int b) { var l = new List<decimal>(); for (int i = a; i <= b; i++) l.Add(i); return l; }

    static List<ClientTuningControl> Build()
    {
        var l = new List<ClientTuningControl>();
        // id 0: non-turbo (base) ratio 23 x 100 MHz = 2300 MHz; id 1: reference clock
        l.Add(new() { Id = 0, ActiveValue = 23, DefaultValue = 23 });
        l.Add(new() { Id = 1, ActiveValue = 100, DefaultValue = 100 });
        // id 2: core voltage override (0 = default)
        l.Add(new() { Id = 2, ActiveValue = 0, DefaultValue = 0 });
        // id 48: PL1 (W), 47: PL2, 49: PL2 enable, 66: PL time (s), 102: ICCmax (A*4) incl. "auto" marker
        var pl1 = Range(10, 330);
        l.Add(new() { Id = 48, ActiveValue = 45, DefaultValue = 45, SupportedValues = pl1 });
        l.Add(new() { Id = 47, ActiveValue = 107, DefaultValue = 107, SupportedValues = Range(10, 330) });
        l.Add(new() { Id = 49, ActiveValue = 1, DefaultValue = 1 });
        var plt = new List<decimal>();
        foreach (var v in new[] { 1, 2, 3, 4, 5, 6, 7, 8, 10, 12, 14, 16, 20, 24, 28, 32, 40, 48, 56, 64, 96, 128 }) plt.Add(v);
        plt.Add(MaxDecimal);
        l.Add(new() { Id = 66, ActiveValue = 28, DefaultValue = 28, SupportedValues = plt });
        var vr = Range(0, 255); vr.Add(MaxDecimal);
        l.Add(new() { Id = 102, ActiveValue = 0, DefaultValue = 0, SupportedValues = vr });
        // per-core-count turbo ratios (id 29/30/31/32/42/43/96/97 = 1..8 active cores); no E-cores on Tiger Lake-H
        var ratios = new (uint id, int r)[] { (29, 46), (30, 46), (31, 45), (32, 45), (42, 43), (43, 43), (96, 42), (97, 42) };
        foreach (var (id, r) in ratios) l.Add(new() { Id = id, ActiveValue = r, DefaultValue = r, SupportedValues = Range(8, 48) });
        return l;
    }

    public static bool PlatformCompatible(out List<string> issues) { issues = new List<string>(); return true; }

    /// <summary>XTU "tune": OC write. Never touches hardware in this port.</summary>
    public static bool Tune(uint id, decimal value)
    {
        bool done = false;
        HardwareGate.GuardOcWrite(() => { CPUOC30.Model.DebugLog.Log($"XTU Tune id={id} value={value} (no XTU on Linux: not applied)"); done = true; });
        return done;
    }
}

/// <summary>Read-only Linux sources replacing the XTU monitors / Windows performance counters / EC reads.</summary>
public static class LinuxSensors
{
    static ulong _idle, _total; static bool _haveCpu;

    public static string CpuBrand()
    {
        try
        {
            foreach (var l in File.ReadLines("/proc/cpuinfo"))
                if (l.StartsWith("model name")) return l.Substring(l.IndexOf(':') + 1).Trim();
        }
        catch { }
        return "11th Gen Intel(R) Core(TM) i7-11800H @ 2.30GHz";
    }

    /// <summary>Average current core frequency in MHz (XTU monitor id 5 x 1000).</summary>
    public static int CpuMHz()
    {
        try
        {
            var vals = new List<double>();
            foreach (var d in Directory.GetDirectories("/sys/devices/system/cpu", "cpu[0-9]*"))
            {
                var f = Path.Combine(d, "cpufreq", "scaling_cur_freq");
                if (File.Exists(f) && double.TryParse(File.ReadAllText(f).Trim(), out var khz)) vals.Add(khz / 1000.0);
            }
            if (vals.Count > 0) return (int)Math.Round(vals.Average());
        }
        catch { }
        try
        {
            var v = new List<double>();
            foreach (var l in File.ReadLines("/proc/cpuinfo"))
                if (l.StartsWith("cpu MHz") && double.TryParse(l.Substring(l.IndexOf(':') + 1).Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var m)) v.Add(m);
            if (v.Count > 0) return (int)Math.Round(v.Average());
        }
        catch { }
        return 0;
    }

    /// <summary>% processor time, first call returns 0 like the original NextSample logic.</summary>
    public static int CpuUsage()
    {
        try
        {
            var p = File.ReadLines("/proc/stat").First().Split(' ', StringSplitOptions.RemoveEmptyEntries).Skip(1).Select(ulong.Parse).ToArray();
            ulong idle = p[3] + (p.Length > 4 ? p[4] : 0), total = 0; foreach (var v in p.Take(8)) total += v;
            int res = 0;
            if (_haveCpu && total > _total) res = (int)Math.Round(100.0 * (1.0 - (double)(idle - _idle) / (total - _total)));
            _idle = idle; _total = total; _haveCpu = true;
            return Math.Clamp(res, 0, 100);
        }
        catch { return 0; }
    }

    /// <summary>coretemp package temperature (read-only hwmon), -1 if unavailable.</summary>
    public static int CpuTempHwmon()
    {
        try
        {
            foreach (var h in Directory.GetDirectories("/sys/class/hwmon"))
            {
                var n = Path.Combine(h, "name");
                if (File.Exists(n) && File.ReadAllText(n).Trim() == "coretemp")
                {
                    var t = Path.Combine(h, "temp1_input");
                    if (File.Exists(t)) return (int)Math.Round(int.Parse(File.ReadAllText(t).Trim()) / 1000.0);
                }
            }
        }
        catch { }
        return -1;
    }

    static readonly Random Rnd = new();
    /// <summary>STUB: CPU core voltage (XTU monitor 0xA4000004) needs MSR 0x198; plausible idle value with jitter.</summary>
    public static string CpuVoltage() => Math.Round(0.8 + Rnd.NextDouble() * 0.12, 4).ToString("#0.#####", CultureInfo.InvariantCulture);

    /// <summary>STUB: memory clock (XTU monitor 17) needs MCHBAR; DDR4-3200 laptop DIMMs.</summary>
    public static int MemoryMHz() => 3200;

    public static long MemTotalMB()
    {
        try { foreach (var l in File.ReadLines("/proc/meminfo")) if (l.StartsWith("MemTotal:")) return long.Parse(l.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1]) / 1024; } catch { }
        return 0;
    }

    /// <summary>1 = on AC, 0 = on battery, 255 = unknown (SystemInformation.PowerStatus.PowerLineStatus).</summary>
    public static int PowerLine()
    {
        if (AppHost.IsScreenshot) return AppHost.PageArg == "unplugged" ? 0 : 1;
        try
        {
            foreach (var d in Directory.GetDirectories("/sys/class/power_supply"))
            {
                var t = Path.Combine(d, "type");
                if (File.Exists(t) && File.ReadAllText(t).Trim() == "Mains")
                    return File.ReadAllText(Path.Combine(d, "online")).Trim() == "1" ? 1 : 0;
            }
        }
        catch { }
        return 255;
    }
}

/// <summary>Seeds the in-process simulator so the UI passes the capability checks (screenshots / dev). Not used with the real daemon.</summary>
public static class SeedMock
{
    public static void Apply()
    {
        if (!Backend.IsMock) return;
        var b = new byte[256]; b[0] = 0x40;                 // WMI16: CPU OC supported
        Backend.Current.Call(16, 0, b);
        var c = new byte[256]; c[2] = 0x80;                 // WMI122 bit 23
        Backend.Current.Call(122, 0, c);
        if (AppHost.IsScreenshot)
        {
            byte mode = AppHost.PageArg is "perf" or "unplugged" or "ratio" or "power" or "vr" or "voltage" or "dialog" ? (byte)2
                      : AppHost.PageArg switch { "quiet" => (byte)0, "powersave" => (byte)1, "entertainment" => (byte)3, _ => (byte)0 };
            Backend.Current.AppSet(1, 1, new[] { mode });
        }
    }
}
