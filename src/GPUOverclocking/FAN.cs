namespace GPUOverclocking;

// Reduced port of FAN.cs: the GPU OC app only ever stores its own version numbers in it
// (MainWindow.window_ContentRendered); the fan-control methods are never called from this app.
public class FAN
{
	public int FanAPPVersion_Major = 0;
	public int FanAPPVersion_Minor = 0;
	public int FanAPPVersion_Revision = 0;
	public int FanAPPVersion_Build = 0;
}
