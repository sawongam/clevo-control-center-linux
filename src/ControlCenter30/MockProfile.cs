using System;
using CCenter.Platform;

namespace ControlCenter30;

/// <summary>
/// Only active when the platform backend is the in-process simulator (no daemon / --screenshot).
/// CCenter.Platform's MockBackend returns zeros for the "BIOS feature" reads CC30 uses to decide which tiles to show
/// (WMI 16/96/122, WMI package 17, AppSettings page 0 bytes 0-1 and page 7). With all zeros the original would show
/// the "OSD Only" message and exit, so this overlay adds a plausible NH5x_NH7xHP feature set on top.
/// None of these values were read from the real laptop (see PORT_NOTES.md); the real daemon is never wrapped.
/// Overrides (hex): CCENTER_CC30_WMI16, CCENTER_CC30_WMI96, CCENTER_CC30_WMI122, CCENTER_CC30_P7_17 (page 7 byte 17),
/// CCENTER_CC30_CUSTOM (custom id), CCENTER_CC30_SKIN (RRGGBB), CCENTER_CC30_CLEVOID (decimal), CCENTER_CC30_CPU (cpu name).
/// </summary>
internal sealed class ProfileBackend : IBackend
{
	readonly IBackend inner;
	public int Wmi16 = 0x3E9;     // bit0 power modes, bit3 LED keyboard, bit5 GPU OC, bit6 CPU OC, bit7 fan, bit8 energy save, bit9 battery
	public int Wmi96 = 0x0000;
	public int Wmi122 = 0x800000; // bit23: CPU OC supported
	public int P7_17 = 0x0F;      // low nibble: modes (quiet|powersave|performance|entertainment); high nibble: UI id
	public int CustomId = 0;
	public byte[] Skin = new byte[3];
	public int ClevoId = 0;

	public ProfileBackend(IBackend inner)
	{
		this.inner = inner;
		Wmi16 = Env("CCENTER_CC30_WMI16", Wmi16);
		Wmi96 = Env("CCENTER_CC30_WMI96", Wmi96);
		Wmi122 = Env("CCENTER_CC30_WMI122", Wmi122);
		P7_17 = Env("CCENTER_CC30_P7_17", P7_17);
		CustomId = Env("CCENTER_CC30_CUSTOM", CustomId);
		ClevoId = int.TryParse(Environment.GetEnvironmentVariable("CCENTER_CC30_CLEVOID"), out var cid) ? cid : ClevoId;
		var pg = AppHost.PageArg ?? "";
		if (pg.StartsWith("apps") && int.TryParse(pg.Substring(4), out var n))
		{
			int[] bits = { 0x1, 0x2, 0x8, 0x20, 0x40, 0x80, 0x100, 0x200 };
			Wmi16 = 0;
			for (int i = 0; i < n && i < bits.Length; i++) Wmi16 |= bits[i];
		}
		if (pg == "pgupdn") Wmi16 |= 0x400;
		if (pg == "mouse") P7_17 = 0x2D;
		if (pg == "amd") Wmi96 |= 0x2000;
		if (pg == "oldmodes") P7_17 = 0;
		var skin = Environment.GetEnvironmentVariable("CCENTER_CC30_SKIN");
		if (skin != null && skin.Length == 6) Skin = Convert.FromHexString(skin);
	}

	static int Env(string n, int def)
	{
		var v = Environment.GetEnvironmentVariable(n);
		return v != null && int.TryParse(v, System.Globalization.NumberStyles.HexNumber, null, out var r) ? r : def;
	}

	public bool IsMock => true;

	public DchuResult Call(int func, uint arg, byte[] buf)
	{
		var r = inner.Call(func, arg, buf);
		if (buf != null) return r;
		switch (func)
		{
		case 16: return new DchuResult { Int = (ulong)Wmi16 };
		case 96: return new DchuResult { Int = (ulong)Wmi96 };
		case 122: return new DchuResult { Int = (ulong)(uint)Wmi122 };
		case 17:
			var b = new byte[256];
			b[2] = (byte)CustomId; b[3] = Skin[0]; b[4] = Skin[1]; b[5] = Skin[2];
			return new DchuResult { Buf = b };
		}
		return r;
	}

	public byte[] AppGet(int page, int offset, int length)
	{
		var d = inner.AppGet(page, offset, length);
		if (page == 7 && length == 256 && offset == 0 && d[17] == 0) { d[17] = (byte)P7_17; }
		else if (page == 7 && offset <= 17 && offset + length > 17 && d[17 - offset] == 0) d[17 - offset] = (byte)P7_17;
		if (page == 0 && offset == 0 && length >= 2 && ClevoId != 0) { d[0] = (byte)(ClevoId >> 8); d[1] = (byte)ClevoId; }
		return d;
	}

	public bool AppSet(int page, int offset, byte[] data) => inner.AppSet(page, offset, data);
}
