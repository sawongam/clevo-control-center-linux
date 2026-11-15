using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CCenter.Platform;

namespace GPUOverclocking;

public class App : Application
{
	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	public override void OnFrameworkInitializationCompleted()
	{
		// WPF: StartUpPage property from the first command-line argument (never read by this app)
		if (Backend.IsMock) Backend.Current = new SeededMock(Backend.Current);
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d) d.MainWindow = new MainWindow();
		base.OnFrameworkInitializationCompleted();
	}
}

/// <summary>
/// Only used with the in-process simulator (no daemon / screenshot mode): the stock MockBackend answers 0 to the
/// capability query (cmd 16), which makes the original code show "This machine does not support." Seed what the
/// real NH5x_NH7xHP firmware reports (UWP_FanSetting 0x80 + UWP_GPUOC 0x20) and the "performance" situational
/// mode (AppSettings page 1 offset 1 = 2) in which the GPU OC controls are shown.
/// </summary>
internal sealed class SeededMock : IBackend
{
	readonly IBackend _in;
	public SeededMock(IBackend inner) { _in = inner; }
	public bool IsMock => _in.IsMock;
	public DchuResult Call(int func, uint arg, byte[] buf) => func == 16 && buf == null ? new DchuResult { Int = 0xB0 } : _in.Call(func, arg, buf);
	public byte[] AppGet(int page, int offset, int length)
	{
		var r = _in.AppGet(page, offset, length);
		if (page == 1 && offset == 1 && length >= 1 && !_modeSet) r[0] = (byte)Mode;
		return r;
	}
	bool _modeSet;
	public static int Mode = 2;
	public bool AppSet(int page, int offset, byte[] data) { if (page == 1 && offset == 1) _modeSet = true; return _in.AppSet(page, offset, data); }
}
