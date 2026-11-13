namespace FnKey;

// X170 light bar via LineLEDAPI.dll (USB). Windows-only native library -> stub (only reached when WMI 96 bit 11 is set).
public class LightBar_X170
{
	public void Init() { DebugLog.WriteDbgLog2Outlog("STUB LightBar_X170.Init (LineLEDAPI.dll)"); }
}
