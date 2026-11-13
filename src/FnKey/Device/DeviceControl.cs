using System.IO;

namespace FnKey.Device;

// Replaces SetupAPI enumeration. FoundI2CHID: any HID device on the I2C bus (bus id 0018) = I2C precision touchpad.
public class DeviceControl
{
	public bool FoundI2CHID()
	{
		try
		{
			const string root = "/sys/bus/hid/devices";
			if (!Directory.Exists(root)) return false;
			foreach (var d in Directory.GetDirectories(root))
				if (Path.GetFileName(d).StartsWith("0018:")) return true;
		}
		catch { }
		return false;
	}
}
