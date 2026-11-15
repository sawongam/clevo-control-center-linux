using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using CCenter.Platform;

namespace GPUOverclocking;

// Not ported (Windows-only plumbing, no effect on the UI): the ACPI bridge structs/IOCTLs and WM_COPYDATA WndProc
// (never connected: OnSourceInitialized is empty in the original), the Windows EventLog listener (service
// notifications "SetGPUOC"/"clevofnkey^102/202"), XTU service check. See PORT_NOTES.md.
public partial class MainWindow : Window
{
	private loading loadingWindwos;
	private double _dbRawWidth = 1920.0;
	private double _dbRawHeight = 1080.0;
	private double maxW = 1920.0;
	private double maxH = 1080.0;
	private bool Insyde_DCHU_Ready = true;
	private bool load_fail = false;

	public MainWindow()
	{
		InitializeComponent();
		Opened += (s, e) => OnOpened();
		Closed += (s, e) => window_Closed(s, e);
		SizeChanged += (s, e) => window_SizeChanged(s, e);
		PropertyChanged += (s, e) => { if (e.Property == WindowStateProperty) window_StateChanged(s, EventArgs.Empty); };
		Check_FullSrceen.Click += (s, e) => Check_FullSrceen_Click(s, e);
		Check_FullSrceen1.Click += (s, e) => Check_FullSrceen_Click(s, e);
		BTN_min.Click += (s, e) => BTN_min_Click();
		BTN_min1.Click += (s, e) => BTN_min_Click();
		BTN_close.Click += (s, e) => Close();
		BTN_close1.Click += (s, e) => Close();
		foreach (var p in new[] { title_move2, title_move, title_move_Copy })
		{
			p.PointerPressed += title_move_MouseLeftButtonDown;
			p.PointerMoved += title_move_MouseMove;
		}
		grip.PointerPressed += (s, e) => { if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginResizeDrag(WindowEdge.SouthEast, e); };
	}

	private void OnOpened()
	{
		if (!AppHost.IsScreenshot)
		{
			loadingWindwos = new loading();
			loadingWindwos.Show();
		}
		Window_Loaded();
		Dispatcher.UIThread.Post(() => window_ContentRendered(), DispatcherPriority.Loaded);
	}

	private void CloseLoading()
	{
		try { loadingWindwos?.Close(); } catch { }
		loadingWindwos = null;
	}

	private void Window_Loaded()
	{
		try
		{
			CheckAppExistOrNot();
			Global.RW_REG = new RWReg();
			Global.RW_ini = new RWIni();
			Global.MultiLang = new language();
			Global.support_bit = new Support_BIT();
			Global.insyde = new InsydeDriver();
			Global.fan = new FAN();
			Dispatcher.UIThread.Post(CloseLoading);
		}
		catch
		{
			load_fail = true;
			UC_DialogBox.Show(0, 10);
		}
	}

	private void CheckAppExistOrNot()
	{
		string processName = Process.GetCurrentProcess().ProcessName;
		if (processName == "GPUOverclocking" && Process.GetProcessesByName(processName).Length > 1)
			Environment.Exit(Environment.ExitCode);
	}

	private void window_ContentRendered()
	{
		if (load_fail)
		{
			CloseLoading();
			UC_DialogBox.Show(0, 10);
			return;
		}
		Load_windowStyle();
		Topmost = false;
		Version version = typeof(MainWindow).Assembly.GetName().Version ?? new Version(1, 0, 0, 0);
		DebugLog.WriteDbgLog2Outlog("VER:" + version);
		Global.fan.FanAPPVersion_Major = version.Major;
		Global.fan.FanAPPVersion_Minor = version.Minor;
		Global.fan.FanAPPVersion_Build = Math.Max(version.Build, 0);
		Global.fan.FanAPPVersion_Revision = Math.Max(version.Revision, 0);
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
		byte[] array = Global.insyde.GetAPPData(0, 24, 4);
		int build = Math.Max(version.Build, 0), rev = Math.Max(version.Revision, 0);
		if (version.Major != array[0] || version.Minor != array[1] || build != array[2] || rev != array[3])
		{
			byte[] data = new byte[4] { (byte)version.Major, (byte)version.Minor, (byte)build, (byte)rev };
			Global.insyde.SetAPPData(0, 24, 4, data);   // app-version bookkeeping, not an OC write
		}
		Init_Main_UI();
		LoadSkinColor();
		AppHost_PageHook();
	}

	// --page NAME for screenshots (dev aid, not in the original)
	private void AppHost_PageHook()
	{
		var pg = AppHost.PageArg;
		if (string.IsNullOrEmpty(pg)) return;
		foreach (var part in pg.Split('+'))
			page_system_monitor.DevPage(part);
	}

	private void LoadSkinColor()
	{
		byte[] array = Global.insyde.GetWMIPackage(17);
		byte b = array[3];
		byte b2 = array[4];
		byte b3 = array[5];
		Color val = ((b != 0 || b2 != 0 || b3 != 0) ? Color.FromArgb(byte.MaxValue, b, b2, b3) : Color.FromArgb(byte.MaxValue, byte.MaxValue, 0, 210));
		title_icon.Fill = new SolidColorBrush(val);
		byte b4 = array[6];
		byte b5 = array[7];
		byte b6 = array[8];
		val = ((b4 != 0 || b5 != 0 || b6 != 0) ? Color.FromArgb(byte.MaxValue, b4, b5, b6) : Color.FromArgb(byte.MaxValue, 0, 168, byte.MaxValue));
		page_system_monitor.info_GPU.Foreground = new SolidColorBrush(val);
	}

	private void Init_Main_UI()
	{
		page_system_monitor.Init_SystemMonitor_UI();
		page_system_monitor.Open_UI();
	}

	private void BTN_min_Click()
	{
		WindowState = WindowState.Minimized;
	}

	private void Check_FullSrceen_Click(object sender, EventArgs e)
	{
		if (Check_FullSrceen.IsChecked == true) WindowFullSrceen(); else WindowFitSize();
	}

	// WPF SystemParameters.PrimaryScreenWidth/Height are in DIPs
	private (double w, double h) ScreenDips()
	{
		var scr = Screens?.Primary;
		if (scr == null) return (1920.0, 1080.0);
		return (scr.Bounds.Width / scr.Scaling, scr.Bounds.Height / scr.Scaling);
	}

	private void Load_windowStyle()
	{
		var (primaryScreenWidth, primaryScreenHeight) = ScreenDips();
		float num = 2560f / (float)primaryScreenWidth;
		float num2 = 1440f / (float)primaryScreenHeight;
		maxW = Math.Round(_dbRawWidth / (double)num, 0);
		maxH = Math.Round(_dbRawHeight / (double)num2, 0);
		MaxWidth = maxW;
		MaxHeight = maxH;
		VB_main.Width = MaxWidth;
		VB_main.Height = MaxHeight;
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
		if (Global.RW_REG.window_FullSrceen == 1)
		{
			MaxWidth = double.PositiveInfinity;
			MaxHeight = double.PositiveInfinity;
			Width = double.NaN;
			Height = double.NaN;
			WindowState = WindowState.Maximized;
			Check_FullSrceen.IsChecked = true;
			VB_main.Width = double.NaN;
			VB_main.Height = double.NaN;
		}
		else
		{
			Topmost = false;
		}
		double left = (primaryScreenWidth - Width) / 2.0;
		double top = (primaryScreenHeight - Height) / 2.0;
		if (!double.IsNaN(left) && !double.IsNaN(top))
		{
			if (left < 0.0) left = 0.0;
			if (top < 0.0) top = 0.0;
			var sc = Screens?.Primary?.Scaling ?? 1.0;
			Position = new PixelPoint((int)(left * sc), (int)(top * sc));
		}
	}

	private void WindowFitSize()
	{
		var (primaryScreenWidth, primaryScreenHeight) = ScreenDips();
		float num = 2560f / (float)primaryScreenWidth;
		float num2 = 1440f / (float)primaryScreenHeight;
		WindowState = WindowState.Normal;
		MaxWidth = maxW;
		MaxHeight = maxH;
		VB_main.Width = MaxWidth;
		VB_main.Height = MaxHeight;
		Width = Global.RW_REG.window_Width;
		Height = Global.RW_REG.window_Height;
		VB_Style.Width = 500f / num;
		VB_Style.Height = 77f / num2;
		MinWidth = VB_Style.Width;
		MinHeight = VB_Style.Height + 200.0;
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
		VB_main.Width = double.NaN;
		VB_main.Height = double.NaN;
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
			if (Width > 0 && !double.IsNaN(Width))
			{
				Global.RW_REG.window_Width = (int)Width;
				Global.RW_REG.window_Height = (int)Height;
			}
		}
	}

	private void window_StateChanged(object sender, EventArgs e)
	{
		if (Global.RW_REG == null) return;
		Global.RW_REG.window_FullSrceen = WindowState == WindowState.Maximized ? 1 : 0;
	}

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

	private void title_move_MouseMove(object sender, PointerEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && WindowState == WindowState.Maximized)
		{
			WindowFitSize();
		}
	}

	private void window_Closed(object sender, EventArgs e)
	{
		page_system_monitor.Close_UI();
		(Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
	}
}
