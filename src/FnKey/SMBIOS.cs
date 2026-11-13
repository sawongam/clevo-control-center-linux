using System.IO;

namespace FnKey;

// Replaces the WMI/SMBIOS table reader; only System.ManufacturerName is used by FnKey.
internal class SMBIOS
{
	public class SystemInfo
	{
		public string ManufacturerName { get; private set; }

		public SystemInfo()
		{
			try { ManufacturerName = File.ReadAllText("/sys/class/dmi/id/sys_vendor").Trim(); }
			catch { ManufacturerName = "Notebook"; }
		}
	}

	public SystemInfo System = new SystemInfo();
}
