using System;

namespace FnKey;

// Replaces \\.\LCD IOCTL_VIDEO_{QUERY,SET}_DISPLAY_BRIGHTNESS. AC and DC levels do not exist on Linux:
// both report the current backlight percentage; SetBrightness is a logged STUB (see Device/Brightness.cs).
public class LCDBrightness
{
	public class Brightness
	{
		public byte AC;

		public byte DC;
	}

	public Brightness GetBrightness()
	{
		int v = new FnKey.Device.Brightness().Read_Brightness();
		return new Brightness { AC = (byte)v, DC = (byte)v };
	}

	public bool SetBrightness(int brightness)
	{
		return SetBrightness(brightness, brightness);
	}

	public bool SetBrightness(int acBrightness, int dcBrightness)
	{
		DebugLog.WriteDbgLog2Outlog("STUB LCDBrightness.SetBrightness(" + acBrightness + "," + dcBrightness + ") - not performed on Linux");
		return false;
	}
}
