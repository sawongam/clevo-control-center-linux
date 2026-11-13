using System;
using System.Threading;
using System.Threading.Tasks;

namespace ControlCenter30;

/// <summary>
/// The Windows build does not talk to the hardware for power modes itself: it writes "clevocc30^101^N" into the
/// OutLog event log and FnKey.exe (Form1.OnChangePowerMode -> Features.SetPowerMode) does the DCHU calls, then
/// answers "clevofnkey^202". There is no service on Linux, so this class performs the hardware part of that
/// hand-off directly (see PORT_NOTES.md for what is intentionally not replicated: GPU clock lock/OC, Windows
/// power plans, ASPM, XTU, panel refresh rate).
/// </summary>
internal static class ServiceBridge
{
	static readonly object Gate = new object();
	static bool installed;
	/// <summary>Windows: checkDriverInstall.isInstall_DTT_Driver() (Intel Dynamic Tuning driver). Never present on Linux.</summary>
	public static bool IsInstallDTTDriver = false;
	static int hwPState = -1;

	public static void Install()
	{
		if (installed) return;
		installed = true;
		DebugLog.OutLogWritten += OnOutLog;
	}

	static void OnOutLog(string msg)
	{
		var a = msg.Split('^');
		if (a.Length < 2 || a[0] != "clevocc30") return;
		switch (a[1])
		{
		case "101":
			if (a.Length >= 3 && byte.TryParse(a[2], out var mode))
				Task.Run(() => ChangePowerMode(mode));
			break;
		case "10f":
			// FnKey: UpdatePowerModeUI(amdMode) - tray icon only (AMD manual mode). Nothing to do.
			break;
		case "114":
			// FnKey: SetPanelRefreshRate(-1) (Windows display driver API). Not available on Linux.
			break;
		}
	}

	static bool SupportHW_P_state()
	{
		if (hwPState < 0)
		{
			int data = 0;
			DCHU.GetDCHU_Data(122, ref data);
			hwPState = ((data & 0x40000000) == 1073741824) ? 1 : 0;
		}
		return hwPState == 1;
	}

	// FnKey Form1.OnChangePowerMode (hardware relevant subset)
	static void ChangePowerMode(byte modeType)
	{
		lock (Gate)
		{
			SetPowerMode(modeType);
			DebugLog.WriteDbgLog2Outlog("Set PowerMode: " + (Common.ModeType)modeType);
			if (!SupportHW_P_state())
			{
				if (modeType == 3 || modeType == 0) SetEnergyStarMode(2u);
				if (modeType == 2) SetEnergyStarMode(0u);
				if (modeType == 1) SetEnergyStarMode(1u);
			}
			// Fan.Load_FanMode_forFn_3 (Support_MaxQ not evaluated -> always 0, see PORT_NOTES)
			DCHU.WriteAppSettings(4, 5, 1, new byte[1] { 0 });
			DebugLog.WriteDbgLog2Outlog("clevofnkey^202");
		}
	}

	// FnKey Features.SetPowerMode
	static bool SetPowerMode(uint mode)
	{
		int page = 1;
		int offset = 1;
		byte[] bytes = BitConverter.GetBytes(mode);
		int length = 1;
		DCHU.WriteAppSettings(page, offset, length, bytes);
		if (mode == 2)
		{
			uint num = 2u;
			byte[] array = DCHU.ReadAppSettings(1, 32, 1);
			if (array[0] == 1)
			{
				if (IsInstallDTTDriver)
				{
					num |= 0x80;
				}
				else
				{
					array[0] = 0;
					DCHU.WriteAppSettings(1, 32, 1, array);
				}
			}
			byte[] array2 = DCHU.ReadAppSettings(4, 8, 1);
			if (array2[0] == 1)
			{
				if (Global.SupportBIT.DTT)
				{
					if (IsInstallDTTDriver)
					{
						num |= 0x40;
					}
					else
					{
						array2[0] = 0;
						DCHU.WriteAppSettings(4, 8, 1, array2);
						array[0] = 0;
						DCHU.WriteAppSettings(1, 32, 1, array);
						num = 2u;
					}
				}
				else
				{
					num |= 0x40;
				}
			}
			DCHU.SetWMI(121, 25, num);
		}
		else
		{
			DCHU.SetWMI(121, 25, mode);
		}
		return true;
	}

	static bool SetEnergyStarMode(uint mode)
	{
		DCHU.SetWMI(79, mode);
		DebugLog.WriteDbgLog2Outlog("SetEnergyStarMode: " + mode);
		return true;
	}
}
