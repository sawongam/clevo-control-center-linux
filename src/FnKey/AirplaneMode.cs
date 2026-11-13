using System;
using System.IO;
using System.Threading;

namespace FnKey;

// Port of FnKey.AirplaneMode. Windows watches the RadioManagement registry key (RegistryMonitor);
// Linux polls rfkill's soft/hard state every 2 s and re-runs InitAirPlaneLed() when it changes.
public class AirplaneMode
{
	private bool Disable_Airplane;

	private System.Threading.Timer poll;

	private bool lastState;

	public AirplaneMode()
	{
		InitAirPlaneLed();
		lastState = Rfkill.AirplaneOn();
		poll = new System.Threading.Timer(_ =>
		{
			try
			{
				bool s = Rfkill.AirplaneOn();
				if (s != lastState) { lastState = s; InitAirPlaneLed(); }
			}
			catch { }
		}, null, 2000, 2000);
	}

	public void InitAirPlaneLed()
	{
		if (!CheckWirelessDevice())
		{
			Global.features.SetAirplaneHotkey(0);
			DebugLog.WriteDbgLog2Outlog("DisabletAirplaneMode");
			Global.dchu.SetWMI(121, 26, 0u);
			return;
		}
		Global.features.SetAirplaneHotkey(1);
		DebugLog.WriteDbgLog2Outlog("SetAirplaneMode");
		byte[] array = new byte[4];
		if (Rfkill.AirplaneOn())
		{
			Global.dchu.SetWMI(121, 26, 1u);
			array[1] = 1;
		}
		else
		{
			Global.dchu.SetWMI(121, 26, 0u);
			array[1] = 0;
		}
	}

	public bool CheckWirelessDevice()
	{
		try
		{
			if (Rfkill.WlanCount() == 0)
			{
				Disable_Airplane = true;
				return false;
			}
			Disable_Airplane = false;
			return true;
		}
		catch
		{
			Disable_Airplane = true;
			return false;
		}
	}
}
