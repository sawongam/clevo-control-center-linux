using System;
using System.IO;
namespace CC;
/// <summary>Replaces the WMI Win32_ComputerSystem/SMBIOS read of System.ManufacturerName.</summary>
public static class Smbios
{
    public static string ManufacturerName()
    {
        try { return File.ReadAllText("/sys/class/dmi/id/sys_vendor").Trim(); } catch { return "Notebook"; }
    }
}
