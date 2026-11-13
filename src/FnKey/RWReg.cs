using System;
using RippleCC.Platform;

namespace FnKey;

public class RWReg
{
	public int IsFirstRun
	{
		get
		{
			return Convert.ToInt16(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\FnKey", "FirstRun", "0"));
		}
		set
		{
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\FnKey", "FirstRun", value);
		}
	}
}
