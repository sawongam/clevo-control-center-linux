using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using CCenter.Platform;

namespace CC;

public partial class MainWindow : Window
{
	private loading loadingWindwos;
	private double _dbRawWidth;
	private double _dbRawHeight;

	public MainWindow()
	{
		// CheckAppExistOrNot(): single instance (process count) - skipped, see PORT_NOTES.md
		int data = 0;
		DCHU.GetDCHU_Data(70, ref data);
		DCHU.GetDCHU_Data(16, ref data);
		// original: if ((data & 2) == 0) { MessageBox "This UWP app is not suitable for this model."; Exit }
		// Not enforced on Linux: the capability bit cannot be verified without the real firmware (see PORT_NOTES.md).
		SetAppVersion();
		Global.FlexikeyInitialFlag = true;
		InitializeComponent();
		_dbRawWidth = Width;
		_dbRawHeight = Height;
		// loading splash (WPF showed it during start-up, closed in ContentRendered)
		if (!AppHost.IsScreenshot || AppHost.PageArg == "loading") { loadingWindwos = new loading(); loadingWindwos.Show(); }
		AddHandler(KeyDownEvent, (s, e) => { if (Test1.Class1.Active != null && Test1.Class1.Active.Feed(e, true)) e.Handled = true; }, RoutingStrategies.Tunnel);
		AddHandler(KeyUpEvent, (s, e) => { if (Test1.Class1.Active != null && Test1.Class1.Active.Feed(e, false)) e.Handled = true; }, RoutingStrategies.Tunnel);
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		Global.RW_REG = new RWReg();
		page_extra_setting.FlexikeySetColor();
		if (Global.RW_REG.window_FullScreen == 1) WindowFullSreceen();
		else { WindowFitSize(); Check_FullSrceen.IsChecked = false; }
		var scr = Screens.Primary;
		if (scr != null && WindowState == WindowState.Normal)
		{
			double sw = scr.Bounds.Width / scr.Scaling, sh = scr.Bounds.Height / scr.Scaling;
			Position = new PixelPoint((int)((sw - Width) / 2.0 * scr.Scaling), (int)((sh - Height) / 2.0 * scr.Scaling));
		}
		Global.RW_ini = new RWIni();
		Global.MultiLang = new language();
		SetSkinColor();
		Opacity = 0; // Visibility.Hidden until ContentRendered
		Dispatcher.UIThread.Post(() => window_ContentRendered(this, EventArgs.Empty), DispatcherPriority.Background);
	}

	private void SetSkinColor()
	{
		byte[] wMIPackage = DCHU.GetWMIPackage(17);
		byte b = wMIPackage[3], b2 = wMIPackage[4], b3 = wMIPackage[5];
		if (b != 0 || b2 != 0 || b3 != 0)
			title_icon.Fill = new SolidColorBrush(Color.FromArgb(255, b, b2, b3));
	}

	private void window_ContentRendered(object sender, EventArgs e)
	{
		Global.support_bit = new Support_BIT();
		Global.clevo_msg = new Clevo_MSG();
		Init_Main_UI();
		loadingWindwos?.Close();
		Opacity = 1;
		Topmost = false;
		Simulate_FlexiKey_Check();
		if (AppHost.IsScreenshot) ApplyScreenshotPage(AppHost.PageArg);
	}

	/// <summary>--page NAME for screenshots (not in the original).</summary>
	void ApplyScreenshotPage(string p) { ScreenshotPages.Apply(this, p); }

	private void Init_Main_UI() { page_flexikey.Init(); }

	private void BTN_close_Click(object sender, RoutedEventArgs e) { Close(); }
	private void BTN_min_Click(object sender, RoutedEventArgs e) { WindowState = WindowState.Minimized; }

	private void Check_FullSrceen_Click(object sender, RoutedEventArgs e)
	{
		if (Check_FullSrceen.IsChecked == true) WindowFullSreceen(); else WindowFitSize();
	}

	private void WindowFitSize()
	{
		var scr = Screens.Primary;
		double primaryScreenWidth = scr != null ? scr.Bounds.Width / scr.Scaling : 2560;
		double primaryScreenHeight = scr != null ? scr.Bounds.Height / scr.Scaling : 1440;
		float num = 2560f / (float)primaryScreenWidth;
		float num2 = 1440f / (float)primaryScreenHeight;
		Width = _dbRawWidth / (double)num;
		Height = _dbRawHeight / (double)num2;
		VB_Style.Width = 500f / num;
		VB_Style.Height = 77f / num2;
		VB_title.Width = 1905f / num;
		VB_title.Height = 81f / num2;
		r1.Width = Width;
		r1.Height = Height;
		WindowState = WindowState.Normal;
		title_move.Cursor = new Cursor(StandardCursorType.SizeAll);
		Global.RW_REG.window_FullScreen = 0;
	}

	private void WindowFullSreceen()
	{
		Global.RW_REG.window_FullScreen = 1;
		if (!AppHost.IsScreenshot) WindowState = WindowState.Maximized;
		title_move.Cursor = new Cursor(StandardCursorType.Arrow);
		r1.Width = Width;
		r1.Height = Height;
	}

	private void title_move_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && WindowState == WindowState.Normal) BeginMoveDrag(e);
	}

	private void Simulate_FlexiKey_Check()
	{
		page_extra_setting.Close_UI();
		page_flexikey.Open_UI();
		grid_backgroundImage.IsVisible = false;
	}

	private void window_Closed(object sender, EventArgs e)
	{
		(Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
	}

	private void SetAppVersion()
	{
		int value = 7, value2 = 0, value3 = 0;
		byte[] bytes = BitConverter.GetBytes(1);
		DCHU.WriteAppSettings(0, 28, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(value);
		DCHU.WriteAppSettings(0, 29, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(value2);
		DCHU.WriteAppSettings(0, 30, 1, ref bytes[0]);
		bytes = BitConverter.GetBytes(value3);
		DCHU.WriteAppSettings(0, 31, 1, ref bytes[0]);
	}
}
