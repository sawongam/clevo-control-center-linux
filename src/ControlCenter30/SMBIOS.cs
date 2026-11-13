using System;
using System.IO;

namespace ControlCenter30;

// Original: parsed the raw SMBIOS tables through WMI (MSSmBios_RawSMBiosTables) and exposed
// BIOS / System / BaseBoard / Processor / MemoryDevice structures.
// Linux port: the same fields are read from /sys/class/dmi/id/* (world-readable ones only).
// Mapping (only System.Family and System.ManufacturerName are consumed by the UI):
//   System.ManufacturerName <- sys_vendor      (NH5x_NH7xHP: "Dream Machines")
//   System.ProductName      <- product_name    ("NH5x_NH7xHP")
//   System.Version          <- product_version
//   System.SerialNumber     <- product_serial  (root only -> "")
//   System.Family           <- product_family  ("Not Applicable" on this board)
//   Board.*                 <- board_vendor / board_name / board_version / board_serial
//   BIOS.Vendor / Version   <- bios_vendor / bios_version
internal class SMBIOS
{
	public class BIOSInformation
	{
		public string Vendor { get; }
		public string Version { get; }
		public BIOSInformation(string vendor, string version) { Vendor = vendor; Version = version; }
	}

	public class SystemInformation
	{
		public string ManufacturerName { get; }
		public string ProductName { get; }
		public string Version { get; }
		public string SerialNumber { get; }
		public string Family { get; }
		public SystemInformation(string manufacturerName, string productName, string version, string serialNumber, string family)
		{
			ManufacturerName = manufacturerName; ProductName = productName; Version = version; SerialNumber = serialNumber; Family = family;
		}
	}

	public class BaseBoardInformation
	{
		public string ManufacturerName { get; }
		public string ProductName { get; }
		public string Version { get; }
		public string SerialNumber { get; }
		public BaseBoardInformation(string manufacturerName, string productName, string version, string serialNumber)
		{
			ManufacturerName = manufacturerName; ProductName = productName; Version = version; SerialNumber = serialNumber;
		}
	}

	private readonly BIOSInformation biosInformation;
	private readonly SystemInformation systemInformation;
	private readonly BaseBoardInformation baseBoardInformation;

	public BIOSInformation BIOS => biosInformation;

	public SystemInformation System => systemInformation;

	public BaseBoardInformation Board => baseBoardInformation;

	static string Dmi(string name)
	{
		// RIPPLE_CC_DMI_<NAME> lets screenshots/tests simulate other OEM strings (e.g. MEDION, AORUS).
		var ov = Environment.GetEnvironmentVariable("RIPPLE_CC_DMI_" + name.ToUpperInvariant());
		if (ov != null) return ov;
		try { return File.ReadAllText("/sys/class/dmi/id/" + name).Trim(); }
		catch { return ""; }
	}

	public SMBIOS()
	{
		biosInformation = new BIOSInformation(Dmi("bios_vendor"), Dmi("bios_version"));
		systemInformation = new SystemInformation(Dmi("sys_vendor"), Dmi("product_name"), Dmi("product_version"), Dmi("product_serial"), Dmi("product_family"));
		baseBoardInformation = new BaseBoardInformation(Dmi("board_vendor"), Dmi("board_name"), Dmi("board_version"), Dmi("board_serial"));
	}
}
