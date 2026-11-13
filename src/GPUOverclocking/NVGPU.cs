using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using RippleCC.Platform;

namespace GPUOverclocking;

/// <summary>
/// Linux replacement for NVGPU_DLL.dll (a NVAPI wrapper). Same method names/signatures as the
/// DllImports in Page_system_monitor.xaml.cs and ControlGPU.exe.
///
/// READS  : `nvidia-smi --query-gpu=... --format=csv,noheader,nounits` (read-only), cached ~1 s and refreshed on a
///          background thread so the UI thread never blocks. If the dGPU is runtime-suspended (sysfs
///          power/runtime_status) nvidia-smi is NOT run (it would wake the GPU) and Check_NVStatus() = false,
///          which makes the UI show "OFF" exactly like the Windows app does for a sleeping dGPU.
///          nvidia-smi missing / failing: Check_NVStatus() = false and static fallback constants are returned.
/// WRITES : every method that changes core/memory offsets, V/F points, clock locks etc. records the requested value
///          in memory (so the UI reads back what was set) and runs the hardware application step inside
///          HardwareGate.GuardOcWrite. The hardware step is a documented stub: see ApplyOffsetsToHardware.
/// </summary>
public static class NVGPU
{
	// ---------------------------------------------------------------- fallbacks / assumptions (see PORT_NOTES.md)
	const string FallbackName = "NVIDIA GeForce RTX 3060 Laptop GPU";
	const int FallbackBase = 1283;     // RTX 3060 Laptop (published base clock, MHz)
	const int FallbackBoost = 1703;    // RTX 3060 Laptop (published boost clock, MHz)
	const int FallbackMaxMem = 7001;   // GDDR6 14 Gbps as reported by NVAPI/nvidia-smi
	const int CoreRangeMin = -200, CoreRangeMax = 200;   // UNVERIFIED: real NVAPI limits not readable on Linux
	const int MemRangeMin = -500, MemRangeMax = 500;     // UNVERIFIED

	class Snap
	{
		public bool Ok;
		public string Name = FallbackName;
		public int Temp, Util, MemUtil, MemUsed, MemTotal, GClk, MClk, Fan = -1;
		public double Power;
		public int MaxG = 2100, MaxM = FallbackMaxMem;
		public string Driver = "0", DeviceId = "0x252010DE";
	}

	static readonly object Lock = new();
	static Snap _cur = new Snap();
	static DateTime _stamp = DateTime.MinValue;
	static bool _busy;
	static bool _extra;
	static int _gpu;

	// ------------------------------------------------------------------------------- helpers
	static bool Asleep()
	{
		try
		{
			foreach (var d in Directory.GetDirectories("/sys/bus/pci/devices"))
			{
				if (File.ReadAllText(d + "/vendor").Trim() != "0x10de") continue;
				if (!File.ReadAllText(d + "/class").Trim().StartsWith("0x03")) continue;
				var rs = d + "/power/runtime_status";
				if (File.Exists(rs) && File.ReadAllText(rs).Trim() == "suspended") return true;
			}
		}
		catch { }
		return false;
	}

	static string Smi(string query)
	{
		try
		{
			var psi = new ProcessStartInfo("nvidia-smi", "--query-gpu=" + query + " --format=csv,noheader,nounits")
			{ RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
			using var p = Process.Start(psi);
			var o = p.StandardOutput.ReadToEnd();
			if (!p.WaitForExit(3000)) { try { p.Kill(); } catch { } return null; }
			return p.ExitCode == 0 ? o.Trim() : null;
		}
		catch { return null; }
	}

	static int I(string s, int def = 0)
	{
		if (double.TryParse(s?.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var v)) return (int)Math.Round(v);
		return def;
	}

	static Snap Fake()
	{
		// development override, e.g. RIPPLE_GPU_FAKE="temp=75,util=60,clk=1650,mem=7001"
		var f = Environment.GetEnvironmentVariable("RIPPLE_GPU_FAKE");
		if (string.IsNullOrEmpty(f)) return null;
		var s = new Snap { Ok = true, Temp = 45, Util = 0, GClk = 210, MClk = 405 };
		foreach (var kv in f.Split(','))
		{
			var a = kv.Split('=');
			if (a.Length != 2) continue;
			int v = I(a[1]);
			switch (a[0]) { case "temp": s.Temp = v; break; case "util": s.Util = v; break; case "clk": s.GClk = v; break; case "mem": s.MClk = v; break; case "off": s.Ok = v == 0 ? false : true; break; }
		}
		return s;
	}

	static Snap Refresh()
	{
		var fk = Fake();
		if (fk != null) return fk;
		var s = new Snap();
		if (Asleep()) return s;                       // Ok=false -> "OFF"
		var line = Smi("name,temperature.gpu,utilization.gpu,utilization.memory,memory.used,memory.total,clocks.current.graphics,clocks.current.memory,power.draw,fan.speed");
		if (line == null) return s;
		var f = line.Split('\n')[0].Split(',');
		if (f.Length < 10) return s;
		s.Name = f[0].Trim();
		s.Temp = I(f[1]); s.Util = I(f[2]); s.MemUtil = I(f[3]); s.MemUsed = I(f[4]); s.MemTotal = I(f[5]);
		s.GClk = I(f[6]); s.MClk = I(f[7]); s.Power = double.TryParse(f[8].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out var pw) ? pw : 0;
		s.Fan = I(f[9], -1);
		s.Ok = true;
		if (!_extra)
		{
			var e = Smi("clocks.max.graphics,clocks.max.memory,driver_version,pci.device_id");
			if (e != null)
			{
				var g = e.Split('\n')[0].Split(',');
				if (g.Length >= 4) { _mg = I(g[0], 2100); _mm = I(g[1], FallbackMaxMem); _drv = g[2].Trim(); _dev = g[3].Trim(); }
				_extra = true;
			}
		}
		s.MaxG = _mg; s.MaxM = _mm; s.Driver = _drv; s.DeviceId = _dev;
		return s;
	}
	static int _mg = 2100, _mm = FallbackMaxMem; static string _drv = "0", _dev = "0x252010DE";

	static Snap Get()
	{
		lock (Lock)
		{
			bool first = _stamp == DateTime.MinValue;
			if (first)
			{
				_cur = Refresh(); _stamp = DateTime.UtcNow;
			}
			else if (!_busy && (DateTime.UtcNow - _stamp).TotalMilliseconds > 1000)
			{
				_busy = true;
				ThreadPool.QueueUserWorkItem(_ =>
				{
					var n = Refresh();
					lock (Lock) { _cur = n; _stamp = DateTime.UtcNow; _busy = false; }
				});
			}
			return _cur;
		}
	}

	static IntPtr Str(string s) => Marshal.StringToHGlobalUni(s);
	static IntPtr _nameP;

	// ============================================================================================= READS
	public static int InitGPU_API() => 0;                       // read: 0 = OK (ControlGPU also accepts 252)
	public static void CloseGPU_API() { }                       // read
	public static bool Check_NVStatus() => Get().Ok;            // read: dGPU awake
	public static bool Check_GPU_VRAM_Clock() => Get().Ok;      // read
	public static bool run_ReadOCinfo() => true;                // read
	public static bool Get_GPU_ALLInfo() => true;               // read
	public static bool CheckGPU_Thermal() => false;             // read: thermal-throttle flag (not available from nvidia-smi query used)
	public static int Get_GPU_Temp() => Get().Temp;             // read
	public static int Get_GPU_Util() => Get().Util;             // read
	public static int Get_GPU_Graphics_Clock() => Get().GClk;   // read
	public static int Get_GPU_Memory_Clock() => Get().MClk;     // read
	public static int Get_GPU_Processor_Clock() => Get().GClk;  // read
	public static void Get_GPU_Max_Clock() { }                  // read (filled an internal DLL field; unused by the apps)
	public static int Get_GPU_Boost_Clock() => FallbackBoost;   // read (constant: nvidia-smi has no boost-clock field on this driver)
	public static int Get_GPU_Base_Clock() => FallbackBase;     // read (constant)
	public static int Get_GPU_defaultMaxTemp() => 87;           // read (constant)
	public static int Get_GPU_Pstate() => Get().Ok ? (Get().Util > 0 ? 0 : 8) : 8;   // read (approximation)
	public static int Get_GPU_LF() => 0;                        // read (stub: V/F info not exposed)
	public static int Get_GPU_setF() => 0;                      // read (stub)
	public static int Get_GPU_Volt() => 0;                      // read (stub: no voltage via nvidia-smi)
	public static int Get_Volt_range() => 0;                    // read (stub)
	public static int Get_GPU_Pstate_Freq(int LevelIndex, int modeIndex) => 0;   // read (stub)
	public static int Get_GPU_Memory_boost() => Get().MaxM;     // read
	public static int Get_GPU_Memory_base() => 405;             // read (constant)
	public static int Get_Memory_OC_max() => Get().MaxM + _memOff[Math.Clamp(_gpu, 0, 1)];   // read
	public static int Get_GPU_Overclock_range() => _coreOff[Math.Clamp(_gpu, 0, 1)];          // read: current offset as set
	public static int Get_GPU_Overclock_Max() => CoreRangeMax;  // read
	public static int Get_GPU_Overclock_rangeMax() => CoreRangeMax;
	public static int Get_GPU_Overclock_rangeMin() => CoreRangeMin;
	public static int Get_Memory_range() => _memOff[Math.Clamp(_gpu, 0, 1)];                  // read: current offset as set
	public static int Get_Memory_range_max() => MemRangeMax;
	public static int Get_Memory_range_min() => MemRangeMin;
	public static IntPtr Get_GPU_name() { if (_nameP == IntPtr.Zero) _nameP = Str(Get().Name.StartsWith("NVIDIA ") ? Get().Name.Substring(7) : Get().Name); return _nameP; }   // read; NVAPI name has no vendor prefix (the UI prepends "NVIDIA ")
	public static IntPtr Get_GPU_name2() => Get_GPU_name();     // read
	public static int Get_GPU_TotalNumber() => 1;               // read (this laptop: single dGPU)
	public static int Set_GPU_Number(int number) { _gpu = number; return number == 0 ? 0 : 1; }   // selects which GPU later calls address (no hardware effect)
	public static int Get_NVDeviceID(int number)                // read
	{
		var d = Get().DeviceId.Replace("0x", "");
		return int.TryParse(d.Length >= 8 ? d.Substring(4) : d, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var v) ? v : 0x2520;
	}
	public static int Drvier_version()                          // read
	{
		var p = Get().Driver.Split('.');
		return p.Length >= 2 ? I(p[0]) * 100 + I(p[1]) : 0;
	}
	public static IntPtr Get_VFPointFile(string file_path) => IntPtr.Zero;   // read (stub: V/F point dump not available)

	// ============================================================================================= OC WRITES
	static readonly int[] _coreOff = new int[2];
	static readonly int[] _memOff = new int[2];
	static readonly int[] _lock = new int[2];

	/// <summary>
	/// Hardware application step (stub). A real Linux implementation needs: NVIDIA proprietary driver with
	/// Coolbits (e.g. 8 or 28) in an X11 session, then
	///   nvidia-settings -a "[gpu:0]/GPUGraphicsClockOffsetAllPerformanceLevels=&lt;core&gt;"
	///   nvidia-settings -a "[gpu:0]/GPUMemoryTransferRateOffsetAllPerformanceLevels=&lt;mem*2&gt;"
	/// (Wayland has no equivalent; NVML offsets need root). Clock lock: `nvidia-smi -lgc min,max` (root).
	/// Intentionally NOT implemented: the owner has OC writes gated until tested on the real hardware.
	/// </summary>
	static void ApplyOffsetsToHardware(string what, int index, int value)
	{
		if (Environment.GetEnvironmentVariable("GPUOC_DEBUG") == "1")
			Console.Error.WriteLine($"NVGPU(stub) would apply {what} gpu={index} value={value}");
	}

	static int Clamp(int v, int lo, int hi) => Math.Max(lo, Math.Min(hi, v));

	public static int Set_CoreOC(int index, int offset)         // OC WRITE
	{
		int i = Math.Clamp(index, 0, 1);
		_coreOff[i] = Clamp(offset, CoreRangeMin, CoreRangeMax);
		HardwareGate.GuardOcWrite(() => ApplyOffsetsToHardware("core offset", i, _coreOff[i]));
		return 0;
	}

	public static int Set_MEMOC(int index, int offset)          // OC WRITE
	{
		int i = Math.Clamp(index, 0, 1);
		_memOff[i] = Clamp(offset, MemRangeMin, MemRangeMax);
		HardwareGate.GuardOcWrite(() => ApplyOffsetsToHardware("memory offset", i, _memOff[i]));
		return 0;
	}

	public static int Set_VFOffset(int number, int vfPointIdx, int freqOffsetkHz)   // OC WRITE (V/F curve point)
	{
		HardwareGate.GuardOcWrite(() => ApplyOffsetsToHardware("vf point " + vfPointIdx + " kHz", number, freqOffsetkHz));
		return 0;
	}

	public static int Set_ManualMode(int number)                // OC WRITE (switches V/F control to manual)
	{
		HardwareGate.GuardOcWrite(() => ApplyOffsetsToHardware("manual mode", number, 1));
		return 0;
	}

	public static int Set_LinearMode(int number)                // OC WRITE
	{
		HardwareGate.GuardOcWrite(() => ApplyOffsetsToHardware("linear mode", number, 1));
		return 0;
	}

	public static int Lock_Frequency(int index, int Freq)       // OC WRITE (clock lock)
	{
		int i = Math.Clamp(index, 0, 1);
		_lock[i] = Freq;
		HardwareGate.GuardOcWrite(() => ApplyOffsetsToHardware("clock lock", i, Freq));
		return 0;
	}
}

/// <summary>Port of ControlGPU.exe (helper that the Windows service ran for the app).</summary>
public static class ControlGpu
{
	public static void Run(string arg)
	{
		if (arg.Contains("set_increment"))
		{
			var a = arg.Split(':');
			SetIncrement(Convert.ToInt16(a[1]), Convert.ToInt16(a[2]), Convert.ToInt16(a[3]));
		}
		else if (arg.Contains("set_lock_frequency"))
		{
			NVGPU.InitGPU_API();
			NVGPU.Lock_Frequency(0, Convert.ToInt16(arg.Split(':')[1]));
		}
		// "info" (writes GPU info to AppSettings page 5) and "GC6" (Windows registry EnableCoprocPowerControl)
		// are never requested by this app and are not ported.
	}

	public static void SetIncrement(int index, int core, int mem)
	{
		int r = NVGPU.InitGPU_API();
		if (r == 0 || r == 252)
		{
			NVGPU.Set_CoreOC(index, core);     // OC write (gated inside NVGPU)
			NVGPU.Set_MEMOC(index, mem);       // OC write (gated inside NVGPU)
		}
	}
}

/// <summary>SystemParameters.PowerLineStatus replacement.</summary>
public static class PowerLine
{
	public static bool? Force;   // dev/screenshot override
	public static bool IsOffline
	{
		get
		{
			if (Force.HasValue) return Force.Value;
			try
			{
				bool any = false;
				foreach (var d in Directory.GetDirectories("/sys/class/power_supply"))
				{
					var t = Path.Combine(d, "type");
					if (!File.Exists(t) || File.ReadAllText(t).Trim() != "Mains") continue;
					any = true;
					if (File.ReadAllText(Path.Combine(d, "online")).Trim() == "1") return false;
				}
				return any;
			}
			catch { return false; }
		}
	}
}
