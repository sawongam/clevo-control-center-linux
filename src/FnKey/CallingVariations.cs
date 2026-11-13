using System;
using System.Collections.Generic;
using System.IO;

namespace FnKey;

// Replaces the native helper DLLs imported by CallingVariations.cs (GetProductdll.dll, powerlife.dll,
// AudioControlDLL.dll, wlandll.dll ...). Each member keeps its original name/signature; what Linux does is
// documented per member. Everything power-plan/ASPM/brightness related is owned by the desktop / TLP /
// power-profiles-daemon on Linux and is therefore a logged no-op.
internal static class CallingVariations
{
	public struct NtPowerInformation
	{
		public int AcOnLine;
	}

	static void Stub(string what) => DebugLog.WriteDbgLog2Outlog("STUB " + what + " (Windows power-plan API; handled by the desktop/OS on Linux)");

	/// <summary>GetProductdll!GetProductID_PCI = Clevo product id. Linux: env FNKEY_CLEVO_ID (hex) else 0 (generic Intel branch).
	/// TODO(owner): the real value of this laptop is not known yet; read it on Windows (Form1 logs "ClevoID: xxxx") or derive from DMI.</summary>
	public static int GetProductID_PCI()
	{
		var e = Environment.GetEnvironmentVariable("FNKEY_CLEVO_ID");
		if (!string.IsNullOrEmpty(e) && int.TryParse(e.Replace("0x", ""), System.Globalization.NumberStyles.HexNumber, null, out var v)) return v;
		return 0;
	}

	public static NtPowerInformation GetNtPowerInformation()
	{
		Global.BAT_PowerStatus.UpdatePowerInfo();
		return new NtPowerInformation { AcOnLine = (Global.BAT_PowerStatus.PowerLineStatus == FnKey.Device.PowerLineStatus.Online ? 1 : 0) };
	}

	public static byte GetActivePowerPlan() { return 0; }
	public static byte GetLIDACTIONAC(byte plan) { return 0; }  // 0 = do nothing: logind's HandleLidSwitch owns lid actions on Linux
	public static byte GetLIDACTIONDC(byte plan) { return 0; }
	public static void MySystemShutDown(int p1) { Stub("MySystemShutDown(" + p1 + ")"); }
	public static int SetUSB(int a) { Stub("SetUSB(" + a + ")"); return 0; }
	public static byte ReloadActivePowerPlan() { return 0; }
	public static int Set_ASPM(int status) { Stub("Set_ASPM(" + status + ")"); return 0; }
	public static int Set_ASPMEx(int status, int PowerMode) { Stub("Set_ASPMEx(" + status + "," + PowerMode + ")"); return 0; }
	public static int SetActivePowerPlanGUID(Guid g) { Stub("SetActivePowerPlanGUID(" + g + ")"); return 0; }
	public static int SetACCriticalBatteryDoNothing() { return 0; }
	public static int SetLowBatteryLevel(int v) { return 0; }
	public static int SetCriticalBatteryLevel(int v) { return 0; }
	public static int SetReservelBatteryLevel(int v) { return 0; }
	public static void SetCriticalBatteryActionWithGuid(int action, Guid guid) { }
	public static List<Guid> ListAllPowerScheme() { return new List<Guid>(); }
	public static void SetModernStandbyConnectivityInStandby(int status) { Stub("SetModernStandbyConnectivityInStandby(" + status + ")"); }
	public static void SetVideoTimeoutAC(int Number, short value) { Stub("SetVideoTimeoutAC"); }
	public static void SetVideoTimeoutDC(int Number, short value) { Stub("SetVideoTimeoutDC"); }
	public static bool pc_beep_Mute(bool p1) { Stub("pc_beep_Mute(" + p1 + ")"); return true; }
}
