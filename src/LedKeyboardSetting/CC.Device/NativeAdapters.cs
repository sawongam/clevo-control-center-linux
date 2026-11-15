using System;
using System.IO;
namespace CC.Device;

/// <summary>
/// Adapter for the vendor's native perkey_api.dll (USB HID ITE controller). Linux stub: no hidraw access.
/// Same method names / signatures as the original P/Invoke declarations. See PORT_NOTES.md for the
/// reports the original sends (CCENTER_PERKEY_LOG=/path logs each call instead of doing I/O).
/// </summary>
public static class PerkeyApi
{
    static void Log(string s)
    {
        var p = Environment.GetEnvironmentVariable("CCENTER_PERKEY_LOG");
        if (p != null) try { File.AppendAllText(p, s + "\n"); } catch { }
    }
    public static IntPtr InitPerkeyIo() { Log("InitPerkeyIo"); return IntPtr.Zero; }
    public static void SetLEDStatus(byte cmd, byte data0, byte data1, byte data2, byte data3)
        => Log($"SetLEDStatus cmd={cmd} d={data0},{data1},{data2},{data3}");
    /// <summary>Stub: returns 0 (failure) and leaves arr zeroed.</summary>
    public static int GetLedData(byte cmd, byte type, byte[] arr, byte size)
    { Log($"GetLedData cmd={cmd} type={type} size={size}"); return 0; }
}

/// <summary>Adapter for LineLEDAPI.dll (front light bar, X170). Linux stub.</summary>
public static class LineLEDAPI
{
    static void Log(string s)
    {
        var p = Environment.GetEnvironmentVariable("CCENTER_PERKEY_LOG");
        if (p != null) try { File.AppendAllText(p, s + "\n"); } catch { }
    }
    /// <summary>Stub: reports success so the UI is usable; no device I/O is performed.</summary>
    public static bool InitLineLEDIo() { Log("InitLineLEDIo"); return true; }
    public static int SetLightBarData62(byte cmd, byte[] data)
    { Log($"SetLightBarData62 cmd=0x{cmd:X2} data={Convert.ToHexString(data, 0, Math.Min(8, data.Length))}"); return 1; }
}
