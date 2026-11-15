using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CCenter.Platform;

namespace FanSpeedSetting;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        // Mock-only: the simulator does not model the BIOS feature words the app reads at start-up.
        if (Backend.IsMock) Backend.Current = new MockOverlay(Backend.Current);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
        {
            // WPF App.OnStartup: StartUpPage = args[0]; unused by this app.
            d.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}

/// <summary>
/// Wraps the shared MockBackend so the simulated laptop reports "fan setting supported" (WMI 16 bit 7)
/// and a neutral situational mode. Only active in mock mode. Anti-dust support can be simulated with
/// CCENTER_MOCK_ANTIDUST=1 (WMI 96 bit 7).
/// </summary>
sealed class MockOverlay : IBackend
{
    readonly IBackend _in;
    public MockOverlay(IBackend inner)
    {
        _in = inner;
        _in.AppSet(1, 1, new byte[] { 3 }); // situational mode: entertainment (oem.ini SituationalMode=3)
    }
    public bool IsMock => true;
    public DchuResult Call(int func, uint arg, byte[] buf)
    {
        if (buf == null && arg == 0)
        {
            if (func == 16) { Log(func); return new DchuResult { Int = 0xF1 }; }
            if (func == 96) { Log(func); return new DchuResult { Int = Environment.GetEnvironmentVariable("CCENTER_MOCK_ANTIDUST") == "1" ? 0x80u : 0u }; }
        }
        return _in.Call(func, arg, buf);
    }
    static void Log(int func)
    {
        var p = Environment.GetEnvironmentVariable("CCENTER_MOCK_LOG");
        if (p != null) try { System.IO.File.AppendAllText(p, $"func=0x{func:X2} arg=0x00000000 buf=-\n"); } catch { }
    }
    public byte[] AppGet(int page, int offset, int length) => _in.AppGet(page, offset, length);
    public bool AppSet(int page, int offset, byte[] data) => _in.AppSet(page, offset, data);
}
