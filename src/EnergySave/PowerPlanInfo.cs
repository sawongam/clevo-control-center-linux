using System;

namespace DesktopBridge_EnergySave;

public class PowerPlanInfo
{
	public string FriendlyName;

	public Guid SchemeGuid;

	public object Tag;

	public bool Set()
	{
		return PowerSchemeHelper.SetPowerScheme(SchemeGuid);
	}
}
