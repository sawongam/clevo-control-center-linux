namespace CC.FlexiKey;
/// <summary>WMI Win32_BaseBoard / Win32_SoundDevice probes: no equivalent, both return false (never called by the UI).</summary>
public class GetClevoType
{
	public static bool IsEpson() => false;
	public static bool IsW67() => false;
}
