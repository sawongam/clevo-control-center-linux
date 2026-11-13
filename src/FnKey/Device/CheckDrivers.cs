namespace FnKey.Device;

// Windows: looks for the Intel DTT driver in the Uninstall registry keys. No equivalent on Linux -> not installed.
public class CheckDrivers
{
	public bool isInstall_DTT_Driver() => false;
}
