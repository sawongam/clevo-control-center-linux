using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Threading;
using RippleCC.Platform;

namespace FanSpeedSetting;

public partial class MainWindow : Window
{
	private loading loadingWindwos;

	private double _dbRawWidth = 1920.0;

	private double _dbRawHeight = 1080.0;

	private double maxW = 1920.0;

	private double maxH = 1080.0;

	private bool Insyde_DCHU_Ready = true;

	private bool load_fail = false;

	double ScreenW { get { var s = Screens.Primary; return s == null ? 1920 : s.Bounds.Width / s.Scaling; } }

	double ScreenH { get { var s = Screens.Primary; return s == null ? 1080 : s.Bounds.Height / s.Scaling; } }

	public MainWindow()
	{
		InitializeComponent();
		try { Icon = new WindowIcon(Img.Load("image/fanspeedsetting/ic_fan.png")); } catch { }
		// original: loading splash window on its own STA thread
		try { loadingWindwos = new loading(); loadingWindwos.Show(); } catch { }
		Loaded += Window_Loaded;
		Opened += (s, e) => Dispatcher.UIThread.Post(() => window_ContentRendered(s, e), DispatcherPriority.Loaded);
		Closed += window_Closed;
		SizeChanged += window_SizeChanged;
		PropertyChanged += (_, a) => { if (a.Property == WindowStateProperty) window_StateChanged(this, EventArgs.Empty); };
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		try
		{
			CheckAppExistOrNot();
			Global.RW_REG = new RWReg();
			Global.RW_ini = new RWIni();
			Global.MultiLang = new language();
			LoadMultiLang();
			Global.support_bit = new Support_BIT();
			Global.insyde = new InsydeDriver();
			Global.fan = new FAN();
			Dispatcher.UIThread.Post(() => CloseWindowSafe(loadingWindwos));
		}
		catch
		{
			load_fail = true;
			UC_DialogBox.Show(0, 10);
		}
	}

	private void CloseWindowSafe(Window w)
	{
		if (w == null) return;
		if (Dispatcher.UIThread.CheckAccess()) w.Close(); else Dispatcher.UIThread.Invoke(w.Close);
	}

	// original: exits when another instance of the same process name runs
	private void CheckAppExistOrNot()
	{
		string processName = Process.GetCurrentProcess().ProcessName;
		if (processName != "dotnet" && Process.GetProcessesByName(processName).Length > 1 && !AppHost.IsScreenshot)
		{
			Environment.Exit(Environment.ExitCode);
		}
	}

	// Windows event-log driven notifications (FnKey app / service). No Linux source; the handlers are kept and callable.
	public void ExecEvent(int Event)
	{
		switch (Event)
		{
		case 94:
		case 95:
			Global.fan.Read_AntiDust_Status();
			page_system_monitor.UpdateUI_AntiDust_Status();
			break;
		case 106:
			page_system_monitor.RB_FAN_Maxq.IsChecked = true;
			break;
		case 107:
			page_system_monitor.RB_FAN_custom.IsChecked = true;
			break;
		case 112:
			page_system_monitor.RB_FAN_auto.IsChecked = true;
			break;
		case 143:
			page_system_monitor.RB_FAN_max.IsChecked = true;
			break;
		}
	}

	public void FnKeyEvent(string Event)
	{
		if (!(Event == "102"))
		{
			if (Event == "202")
			{
				page_system_monitor.ReadSituationalMode();
			}
		}
		else
		{
			page_system_monitor.LoadMultiLang();
			page_system_monitor.ReadSituationalMode();
			page_system_monitor.UC_FanTable_CPU.LoadMultiLang();
			page_system_monitor.UC_FanTable_GPU1.LoadMultiLang();
			page_system_monitor.UC_FanTable_GPU2.LoadMultiLang();
		}
	}

	public void CC30Event(string Event, string data)
	{
		Dispatcher.UIThread.Post(() =>
		{
			if (Event == "112")
			{
				if (data == "1") page_system_monitor.UpdateUI_TurboFanON(); else page_system_monitor.UpdateUI_TurboFanOFF();
			}
		});
	}

	private void window_ContentRendered(object sender, EventArgs e)
	{
		Load_windowStyle();
		Topmost = false;
		if (load_fail)
		{
			CloseWindowSafe(loadingWindwos);
			UC_DialogBox.Show(0, 10);
			return;
		}
		Version version = new Version(3, 93, 0, 0);
		DebugLog.WriteDbgLog2Outlog("VER:" + version);
		Global.fan.FanAPPVersion_Major = version.Major;
		Global.fan.FanAPPVersion_Minor = version.Minor;
		Global.fan.FanAPPVersion_Build = version.Build;
		Global.fan.FanAPPVersion_Revision = version.Revision;
		if (!Insyde_DCHU_Ready)
		{
			UC_DialogBox.Show(0, 10);
			return;
		}
		Global.insyde.Init_WMI();
		if (!Global.support_bit.UWP_FanSetting)
		{
			UC_DialogBox.Show(0, 10);
			return;
		}
		byte[] array = new byte[4];
		array = Global.insyde.GetAPPData(0, 12, 4);
		if (array[0] == 0)
		{
			Global.support_bit.ACPIBridge_Load_Default = true;
		}
		if (version.Major != array[0] || version.Minor != array[1] || version.Build != array[2] || version.Revision != array[3])
		{
			byte[] data = new byte[4]
			{
				(byte)Global.fan.FanAPPVersion_Major,
				(byte)Global.fan.FanAPPVersion_Minor,
				(byte)Global.fan.FanAPPVersion_Build,
				(byte)Global.fan.FanAPPVersion_Revision
			};
			Global.insyde.SetAPPData(0, 12, 4, data);
		}
		Init_Main_UI();
		LoadSkinColor();
		ApplyTestArgs();
	}

	private void LoadSkinColor()
	{
		byte[] array = new byte[256];
		array = Global.insyde.GetWMIPackage(17);
		byte b = array[3];
		byte b2 = array[4];
		byte b3 = array[5];
		Color val = ((b != 0 || b2 != 0 || b3 != 0) ? Color.FromArgb(byte.MaxValue, b, b2, b3) : Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 210));
		Brush fill = new SolidColorBrush(val);
		title_icon.Fill = fill;
		byte b4 = array[6];
		byte b5 = array[7];
		byte b6 = array[8];
		val = ((b4 != 0 || b5 != 0 || b6 != 0) ? Color.FromArgb(byte.MaxValue, b4, b5, b6) : Color.FromArgb(byte.MaxValue, 0, 168, byte.MaxValue));
		fill = new SolidColorBrush(val);
		page_system_monitor.info_GPU.Foreground = fill;
		byte b7 = array[6];
		byte b8 = array[7];
		byte b9 = array[8];
		val = ((b7 != 0 || b8 != 0 || b9 != 0) ? Color.FromArgb(byte.MaxValue, b7, b8, b9) : Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 210));
		fill = new SolidColorBrush(val);
		page_system_monitor.system_temperature_layer3.Fill = fill;
	}

	private void LoadMultiLang()
	{
	}

	private void Init_Main_UI()
	{
		page_system_monitor.Init_SystemMonitor_UI();
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	private void BTN_close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void BTN_min_Click(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void Check_FullSrceen_Click(object sender, RoutedEventArgs e)
	{
		if (Check_FullSrceen.IsChecked == true)
		{
			WindowFullSrceen();
		}
		else
		{
			WindowFitSize();
		}
	}

	void SetSizes(double w, double h)
	{
		VB_main.Width = w; VB_main.Height = h;
		r1.Width = w; r2.Width = w; r3.Width = w;
		r1.Height = h; r2.Height = h; r3.Height = h;
	}

	private void Load_windowStyle()
	{
		double primaryScreenWidth = ScreenW;
		double primaryScreenHeight = ScreenH;
		if (Global.RW_REG.Screen_Width != primaryScreenWidth)
		{
			Global.RW_REG.Screen_Width = primaryScreenWidth;
			Global.RW_REG.window_Width = 0;
		}
		if (Global.RW_REG.Screen_Height != primaryScreenHeight)
		{
			Global.RW_REG.Screen_Height = primaryScreenHeight;
			Global.RW_REG.window_Width = 0;
		}
		float num = 2560f / (float)primaryScreenWidth;
		float num2 = 1440f / (float)primaryScreenHeight;
		maxW = Math.Round(_dbRawWidth / (double)num, 0);
		maxH = Math.Round(_dbRawHeight / (double)num2, 0);
		MaxWidth = maxW;
		MaxHeight = maxH;
		SetSizes(MaxWidth, MaxHeight);
		if (Global.RW_REG.window_Width == 0)
		{
			Width = MaxWidth;
			Height = MaxHeight;
			Global.RW_REG.window_Width = (int)Width;
			Global.RW_REG.window_Height = (int)Height;
		}
		else
		{
			Width = Global.RW_REG.window_Width;
			Height = Global.RW_REG.window_Height;
		}
		VB_Style.Width = 500f / num;
		VB_Style.Height = 77f / num2;
		MinWidth = VB_Style.Width;
		MinHeight = VB_Style.Height + 200.0;
		Check_FullSrceen.IsChecked = false;
		VB_title.Width = 1905f / num;
		VB_title.Height = 81f / num2;
		// WPF: when the registry value is absent window_FullSrceen defaults to 1 (maximised). Screenshot mode keeps the windowed size.
		if (Global.RW_REG.window_FullSrceen == 1 && !AppHost.IsScreenshot)
		{
			MaxWidth = double.PositiveInfinity;
			MaxHeight = double.PositiveInfinity;
			Width = double.NaN;
			Height = double.NaN;
			WindowState = WindowState.Maximized;
			Check_FullSrceen.IsChecked = true;
			VB_main.Width = Width;
			VB_main.Height = Height;
			r1.Width = primaryScreenWidth; r2.Width = primaryScreenWidth; r3.Width = primaryScreenWidth;
			r1.Height = primaryScreenHeight; r2.Height = primaryScreenHeight; r3.Height = primaryScreenHeight;
		}
		else
		{
			Topmost = false;
		}
		double left = (primaryScreenWidth - Width) / 2.0;
		double top = (primaryScreenHeight - Height) / 2.0;
		if (double.IsNaN(left) || left < 0.0) left = 0.0;
		if (double.IsNaN(top) || top < 0.0) top = 0.0;
		var sc = Screens.Primary?.Scaling ?? 1.0;
		Position = new PixelPoint((int)(left * sc), (int)(top * sc));
	}

	private void WindowFitSize()
	{
		double primaryScreenWidth = ScreenW;
		double primaryScreenHeight = ScreenH;
		float num = 2560f / (float)primaryScreenWidth;
		float num2 = 1440f / (float)primaryScreenHeight;
		WindowState = WindowState.Normal;
		MaxWidth = maxW;
		MaxHeight = maxH;
		SetSizes(MaxWidth, MaxHeight);
		Width = Global.RW_REG.window_Width;
		Height = Global.RW_REG.window_Height;
		VB_Style.Width = 500f / num;
		VB_Style.Height = 77f / num2;
		MinWidth = VB_Style.Width;
		MinHeight = VB_Style.Height + 200.0;
		if (Position.X < 0 || Position.Y < 0) Position = new PixelPoint(Math.Max(0, Position.X), Math.Max(0, Position.Y));
		Check_FullSrceen.IsChecked = false;
		Global.RW_REG.window_FullSrceen = 0;
		Topmost = false;
	}

	private void WindowFullSrceen()
	{
		WindowState = WindowState.Normal;
		MaxWidth = double.PositiveInfinity;
		MaxHeight = double.PositiveInfinity;
		WindowState = WindowState.Maximized;
		VB_main.Width = Width; VB_main.Height = Height;
		r1.Width = Width; r2.Width = Width; r3.Width = Width;
		r1.Height = Height; r2.Height = Height; r3.Height = Height;
		Check_FullSrceen.IsChecked = true;
		Global.RW_REG.window_FullSrceen = 1;
	}

	private void window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (Global.RW_REG == null) return;
		if (Check_FullSrceen.IsChecked != true)
		{
			if (WindowState == WindowState.Maximized)
			{
				WindowFullSrceen();
				return;
			}
			if (!double.IsNaN(Width)) Global.RW_REG.window_Width = (int)Width;
			if (!double.IsNaN(Height)) Global.RW_REG.window_Height = (int)Height;
		}
	}

	private void window_StateChanged(object sender, EventArgs e)
	{
		if (Global.RW_REG == null) return;
		if (WindowState == WindowState.Maximized) Global.RW_REG.window_FullSrceen = 1; else Global.RW_REG.window_FullSrceen = 0;
	}

	private void title_move_PointerPressed(object sender, PointerPressedEventArgs e) => title_move_MouseLeftButtonDown(sender, e);

	private void title_move_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
		if (Width >= maxW - 1.0)
		{
			Width = Width - 0.5;
			Height = Height - 0.5;
		}
		BeginMoveDrag(e);
	}

	private void title_move_PointerMoved(object sender, PointerEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && WindowState == WindowState.Maximized)
		{
			WindowFitSize();
		}
	}

	private void window_Closed(object sender, EventArgs e)
	{
		(Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
	}

	// ---- hidden test hooks (screenshots / MOCK_LOG verification), not part of the original ----
	// --page auto|max|maxq|silent|custom|custom-gpu1|custom-gpu2|antidust
	// --act click:RB_FAN_max | save-curve | reset-curve | drag:CPU:2:x:y   (several, ';'-separated)
	void ApplyTestArgs()
	{
		var p = AppHost.PageArg;
		var a = page_system_monitor;
		if (!string.IsNullOrEmpty(p))
		{
			switch (p)
			{
			case "auto": a.RB_FAN_auto.IsChecked = true; break;
			case "max": a.RB_FAN_max.IsChecked = true; break;
			case "maxq": a.RB_FAN_Maxq.IsChecked = true; break;
			case "silent": a.RB_FAN_Silent.IsChecked = true; break;
			case "custom": a.RB_FAN_custom.IsChecked = true; break;
			case "custom-gpu1": a.RB_FAN_custom.IsChecked = true; a.RB_customFanGPU1.IsChecked = true; break;
			case "custom-gpu2": a.RB_FAN_custom.IsChecked = true; a.RB_customFanGPU2.IsChecked = true; break;
			case "antidust": a.TestShowAntiDust(); break;
			}
		}
		var i = Array.IndexOf(AppHost.Args, "--act");
		if (i >= 0 && i + 1 < AppHost.Args.Length) a.RunTestActions(AppHost.Args[i + 1]);
	}
}
