using System;
using System.Reflection;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CCenter.Platform;
namespace CC;

public partial class MainWindow : Window
{
	private loading loadingWindwos;
	private double _dbRawWidth = 1920.0;
	private double _dbRawHeight = 1080.0;
	private double maxW = 1920.0;
	private double maxH = 1080.0;
	private bool load_fail = false;
	private bool Insyde_DCHU_Ready = true;
	public int APPVersion_Major = 0, APPVersion_Minor = 0, APPVersion_Revision = 0, APPVersion_Build = 0;
	private bool _started;

	static string PageName => AppHost.PageArg ?? "";

	public MainWindow()
	{
		InitializeComponent();
		loadingWindwos = new loading();
		loadingWindwos.Show();
		Opened += (_, _) => { if (!_started) { _started = true; Window_Loaded(); } };
		Closed += (_, _) => window_Closed();
		SizeChanged += window_SizeChanged;
		PropertyChanged += (s, e) => { if (e.Property == WindowStateProperty) window_StateChanged(); };
	}

	private void CloseWindowSafe(Window w) { try { w.Close(); } catch { } }

	private void Window_Loaded()
	{
		try
		{
			Global.RW_REG = new RWReg();
			Global.RW_ini = new RWIni();
			Global.MultiLang = new language();
			LoadCustomIcon();
			Global.support_bit = new Support_BIT();
			Global.insyde = new InsydeDriver();
			if (PageName != "loading")
				Dispatcher.UIThread.Post(() => CloseWindowSafe(loadingWindwos));
		}
		catch
		{
			load_fail = true;
			UC_DialogBox.Show(0, 10);
			return;
		}
		Dispatcher.UIThread.Post(window_ContentRendered, DispatcherPriority.Render);
	}

	private void window_ContentRendered()
	{
		if (load_fail)
		{
			CloseWindowSafe(loadingWindwos);
			UC_DialogBox.Show(0, 10);
			return;
		}
		Load_windowStyle();
		Topmost = false;
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		DebugLog.WriteDbgLog2Outlog("VER:" + version);
		APPVersion_Major = version.Major;
		APPVersion_Minor = version.Minor;
		APPVersion_Build = version.Build;
		APPVersion_Revision = version.Revision;
		if (!Insyde_DCHU_Ready)
		{
			UC_DialogBox.Show(0, 10);
			return;
		}
		Global.insyde.Init_WMI();
		if (!Global.support_bit.UWP_BatteryUtility)
		{
			UC_DialogBox.Show(0, 10);
			return;
		}
		byte[] array = new byte[4];
		array = Global.insyde.GetAPPData(0, 36, 4);
		if (version.Major != array[0] || version.Minor != array[1] || version.Build != array[2] || version.Revision != array[3])
		{
			byte[] array2 = new byte[256];
			array2[0] = (byte)APPVersion_Major;
			array2[1] = (byte)APPVersion_Minor;
			array2[2] = (byte)APPVersion_Build;
			array2[3] = (byte)APPVersion_Revision;
			Global.insyde.SetAPPData(0, 36, 4, array2);
		}
		page_battery_refresh.IsVisible = true;
		page_battery_refresh.init();
		main_windowStyle_forBAT.IsVisible = true;
		main_windowStyle.IsVisible = false;
		LoadSkinColor();
		page_battery_refresh.Open_UI();
		title_move.IsEnabled = false;
		if (PageName.Length > 0 && PageName != "main" && PageName != "loading" && PageName != "unsupported")
			page_battery_refresh.DebugState(PageName);
	}

	private void LoadSkinColor()
	{
		byte[] array = Global.insyde.GetWMIPackage(17);
		byte b = array[3], b2 = array[4], b3 = array[5];
		Color val = (b != 0 || b2 != 0 || b3 != 0) ? Color.FromArgb(255, b, b2, b3) : Color.FromArgb(255, 255, 0, 210);
		title_icon.Fill = new SolidColorBrush(val);
	}

	// Events 85/90/91 from the firmware (OutLog EventLog / WM_COPYDATA) have no Linux source: the page polls instead.

	private void LoadCustomIcon()
	{
		// original: optional "<CC_Path>\image\logo.png" / "title.png" overrides (CC_Path is "" -> relative to cwd)
		string text = "image/logo.png";
		if (System.IO.File.Exists(text)) Image_logo.Source = new Avalonia.Media.Imaging.Bitmap(text);
		text = "image/title.png";
		if (System.IO.File.Exists(text)) Image_title.Source = new Avalonia.Media.Imaging.Bitmap(text);
	}

	private void BTN_close_Click(object sender, RoutedEventArgs e)
	{
		DebugLog.WriteLog2Outlog("clevobattery^207^0");
		Close();
	}
	private void BTN_min_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }
	private void Check_FullSrceen_Click(object sender, RoutedEventArgs e)
	{
		var cb = (CheckBox)sender;
		if (cb.IsChecked == true) WindowFullSrceen(); else WindowFitSize();
	}

	static (double w, double h) Screen()
	{
		var s = Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime l
			? l.MainWindow?.Screens?.Primary : null;
		if (s == null) return (1920, 1080);
		return (s.Bounds.Width / s.Scaling, s.Bounds.Height / s.Scaling);
	}

	private void Load_windowStyle()
	{
		var (primaryScreenWidth, primaryScreenHeight) = Screen();
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
			VB_main.Width = double.NaN;   // = this.Width (NaN) -> fills the window
			VB_main.Height = double.NaN;
		}
		else
		{
			Topmost = false;
			Position = new PixelPoint((int)Math.Max(0, (primaryScreenWidth - Width) / 2.0), (int)Math.Max(0, (primaryScreenHeight - Height) / 2.0));
		}
	}

	private void WindowFitSize()
	{
		var (primaryScreenWidth, primaryScreenHeight) = Screen();
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
			if (WindowState == WindowState.Maximized) { WindowFullSrceen(); return; }
			Global.RW_REG.window_Width = (int)Width;
			Global.RW_REG.window_Height = (int)Height;
		}
	}

	private void window_StateChanged()
	{
		if (Global.RW_REG == null) return;
		Global.RW_REG.window_FullSrceen = WindowState == WindowState.Maximized ? 1 : 0;
	}

	private void title_move_PointerPressed(object sender, PointerPressedEventArgs e)
	{
		if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
		if (!double.IsNaN(Width) && Width >= maxW - 1.0)
		{
			Width = Width - 0.5;
			Height = Height - 0.5;
		}
		BeginMoveDrag(e);
	}

	private void title_move_PointerMoved(object sender, PointerEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && WindowState == WindowState.Maximized)
			WindowFitSize();
	}

	private void window_Closed()
	{
		page_battery_refresh.Close_UI();
		(Application.Current?.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
	}
}
