using System;
using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CC.other_Class;
using RippleCC.Platform;

namespace CC;

public partial class MainWindow : Window
{
	private loading loadingWindwos;

	private double _dbRawWidth = 1920.0;

	private double _dbRawHeight = 1080.0;

	private double maxW = 1920.0;

	private double maxH = 1080.0;

	private int Clevo_WMI_Command;

	private string CC_Path = "";

	public MainWindow()
	{
		InitializeComponent();
		SystemParameters.Init(this);
		// original: second process instance exits (CheckAppExistOrNot); loading window ran on its own STA thread
		if (!AppHost.IsScreenshot)
		{
			loadingWindwos = new loading();
			loadingWindwos.Show();
		}
		Loaded += Window_Loaded;
		ContentRendered_Hook();
		Closed += window_Closed;
		SizeChanged += window_SizeChanged;
		PropertyChanged += (s, e) => { if (e.Property == WindowStateProperty) window_StateChanged(s, EventArgs.Empty); };
		title_move.PointerPressed += title_move_MouseLeftButtonDown;
		title_move.PointerMoved += title_move_MouseMove;
		title_move_Copy.PointerPressed += title_move_MouseLeftButtonDown;
		title_move_Copy.PointerMoved += title_move_MouseMove;
		Check_FullSrceen.Click += Check_FullSrceen_Click;
		BTN_min.Click += BTN_min_Click;
		BTN_close.Click += BTN_close_Click;
	}

	// WPF Window.ContentRendered: raised once after the first render
	private void ContentRendered_Hook()
	{
		Opened += (_, _) => Dispatcher.UIThread.Post(() => window_ContentRendered(this, EventArgs.Empty), DispatcherPriority.Loaded);
	}

	private void CloseWindowSafe(Window w)
	{
		if (w == null) return;
		if (Dispatcher.UIThread.CheckAccess()) w.Close();
		else Dispatcher.UIThread.Invoke(w.Close);
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		IsVisible = false;
		Global.RW_REG = new RWReg();
		string subkey = "Software\\ControlCenter2.0\\UI";
		Reg.SetValue(subkey, "_created", 1);
		subkey = "Software\\ControlCenter2.0\\LED\\PKB";
		Reg.SetValue(subkey, "_created", 1);
		CC_Path = AppContext.BaseDirectory;
		Global.MultiLang = new language();
		Global.KB = new KBModule();
		if (Global.KB.GetKBType().Equals(0))
		{
			MessageBoxWin.Show("No support this module!!!");
			(Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
		}
		Color val = Global.KB.CustomMainSkin();
		if (val.R != 0 || val.G != 0 || val.B != 0)
		{
			title_icon.Fill = new SolidColorBrush(Color.FromArgb(byte.MaxValue, val.R, val.G, val.B));
		}
		Dispatcher.UIThread.Post(() =>
		{
			CloseWindowSafe(loadingWindwos);
		});
		page_led_device.StartUp();
		ScriptedClicks();
	}

	/// <summary>Dev/screenshot helper: "--click NAME,NAME" clicks the named controls (RadioButton/CheckBox/Button) one after another.</summary>
	private void ScriptedClicks()
	{
		int i = Array.IndexOf(AppHost.Args, "--click");
		if (i < 0 || i + 1 >= AppHost.Args.Length) return;
		var names = AppHost.Args[i + 1].Split(',');
		DispatcherTimer.RunOnce(() =>
		{
			foreach (var tok in names)
			{
				if (tok.Contains('='))   // NAME=VALUE: select the matching item of a ComboBox
				{
					var sl = page_led_device.FindControl<Slider>(tok.Split('=')[0]);
					if (sl != null) { sl.Value = double.Parse(tok.Split('=')[1]); Dispatcher.UIThread.RunJobs(); continue; }
					var cb = page_led_device.FindControl<ComboBox>(tok.Split('=')[0]);
					if (cb?.ItemsSource != null) foreach (var it in cb.ItemsSource) if (it.ToString() == tok.Split('=')[1]) cb.SelectedItem = it;
				}
				else
				{
					var c = page_led_device.FindControl<Control>(tok);
					if (c is ComboBox cbx) cbx.IsDropDownOpen = true;
					else if (c is RadioButton rb) rb.IsChecked = true;
					else if (c is Avalonia.Controls.Primitives.ToggleButton tb2) tb2.IsChecked = !(tb2.IsChecked == true);
					if (c is Button || c is Avalonia.Controls.Primitives.ToggleButton) c.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
				}
				Dispatcher.UIThread.RunJobs();
			}
		}, TimeSpan.FromMilliseconds(300));
	}

	private void window_ContentRendered(object sender, EventArgs e)
	{
		Load_windowStyle();
		IsVisible = true;
		Init_Main_UI();
		Topmost = false;
	}

	private void LoadCustomIcon()
	{
		string text = Path.Combine(CC_Path, "image", "logo.png");
		if (File.Exists(text)) Image_logo.Source = new Bitmap(text);
		text = Path.Combine(CC_Path, "image", "title.png");
		if (File.Exists(text)) Image_title.Source = new Bitmap(text);
	}

	/// <summary>Original WndProc(WM_COPYDATA 74) from HkeyTray: no hotkey tray on Linux. Kept for a future daemon hook.</summary>
	public void ExecMessageCommand(int cc_WParam, int command, byte[] array)
	{
		if (cc_WParam == 2 && array.Length < 5)
			Get_Hotkey_Info(command, array);
	}

	private void Get_Hotkey_Info(int FuncNumber, object FuncData)
	{
		byte[] array = (byte[])FuncData;
		Update_Event((FuncNumber << 8) | array[0], array[1], array[2], array[3]);
	}

	private void Update_Event(int case_index, byte value1, byte value2, byte value3)
	{
		Dispatcher.UIThread.Post(() =>
		{
			switch (case_index)
			{
			case 515:
				page_led_device.UpdateBrightnessStatus(value1);
				break;
			case 517:
				page_led_device.UpdateKbStatus(value1);
				break;
			case 518:
				page_led_device.UpdateBrightnessStatus(value1);
				break;
			case 516:
				page_led_device.UpdateBrightnessStatus(value1);
				break;
			}
		});
	}

	private void Init_Main_UI()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	private void BTN_close_Click(object sender, RoutedEventArgs e)
	{
		page_led_device.Close_UI();
		Close();
	}

	private void BTN_min_Click(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void Check_FullSrceen_Click(object sender, RoutedEventArgs e)
	{
		if (Check_FullSrceen.IsChecked == true) WindowFullSrceen();
		else WindowFitSize();
	}

	private static double Cur(double v) => double.IsNaN(v) ? 0 : v;

	private void Load_windowStyle()
	{
		double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
		double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
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
		VB_main.Width = MaxWidth;
		VB_main.Height = MaxHeight;
		r1.Width = MaxWidth;
		r1.Height = MaxHeight;
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
		if (Global.RW_REG.window_FullSrceen == 1)
		{
			MaxWidth = double.PositiveInfinity;
			MaxHeight = double.PositiveInfinity;
			// WPF: Width/Height = NaN + WindowState Maximized -> window fills the screen
			Width = primaryScreenWidth;
			Height = primaryScreenHeight;
			if (!AppHost.IsScreenshot) WindowState = WindowState.Maximized;
			Check_FullSrceen.IsChecked = true;
			VB_main.Width = Width;
			VB_main.Height = Height;
			r1.Width = primaryScreenWidth;
			r1.Height = primaryScreenHeight;
		}
		double left = (primaryScreenWidth - Width) / 2.0;
		double top = (primaryScreenHeight - Height) / 2.0;
		if (left < 0.0) left = 0.0;
		if (top < 0.0) top = 0.0;
		double sc = Screens?.Primary?.Scaling ?? 1.0;
		Position = new PixelPoint((int)(left * sc), (int)(top * sc));
	}

	private void WindowFitSize()
	{
		double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
		double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
		float num = 2560f / (float)primaryScreenWidth;
		float num2 = 1440f / (float)primaryScreenHeight;
		WindowState = WindowState.Normal;
		MaxWidth = maxW;
		MaxHeight = maxH;
		VB_main.Width = MaxWidth;
		VB_main.Height = MaxHeight;
		r1.Width = MaxWidth;
		r1.Height = MaxHeight;
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
		IsVisible = false;
		WindowState = WindowState.Normal;
		MaxWidth = double.PositiveInfinity;
		MaxHeight = double.PositiveInfinity;
		WindowState = WindowState.Maximized;
		IsVisible = true;
		VB_main.Width = Bounds.Width;
		VB_main.Height = Bounds.Height;
		r1.Width = Bounds.Width;
		r1.Height = Bounds.Height;
		Check_FullSrceen.IsChecked = true;
		Global.RW_REG.window_FullSrceen = 1;
	}

	private void window_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (Check_FullSrceen.IsChecked != true)
		{
			if (WindowState == WindowState.Maximized)
			{
				WindowFullSrceen();
				return;
			}
			Global.RW_REG.window_Width = (int)Width;
			Global.RW_REG.window_Height = (int)Height;
		}
	}

	private void window_StateChanged(object sender, EventArgs e)
	{
		if (WindowState == WindowState.Maximized) Global.RW_REG.window_FullSrceen = 1;
		else Global.RW_REG.window_FullSrceen = 0;
	}

	private void title_move_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
		if (Width >= maxW - 1.0)
		{
			Width -= 0.5;
			Height -= 0.5;
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
		page_led_device.Close_UI();
		(Application.Current.ApplicationLifetime as Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime)?.Shutdown();
	}
}
