using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using RippleCC.Platform;

namespace ControlCenter30;

public partial class Window_MOUSE : Window
{
	private language lang = language.Instance;

	private DispatcherTimer LoadingTimer = new DispatcherTimer();

	public Window_MOUSE()
	{
		InitializeComponent();
		Closing += (_, _) => Window_Closing();
		BTN_FanSpeedSetting.Click += BTN_FanSpeedSetting_Click;
		BTN_LEDKeyboard.Click += BTN_LEDKeyboard_Click;
		BTN_Flexikey.Click += BTN_Flexikey_Click;
		RB_performance.Click += RB_performance_Click;
		RB_Entertainment.Click += RB_Entertainment_Click;
		RB_Quiet.Click += RB_Quiet_Click;
		title_move.PointerPressed += title_move_MouseLeftButtonDown;
		Check_FullSrceen.Click += Check_FullSrceen_Click;
		BTN_min.Click += BTN_min_Click;
		BTN_close.Click += BTN_close_Click;
		if (!Global.SupportBIT.APP_FanSpeedSetting)
		{
			BTN_FanSpeedSetting.IsVisible = false;
		}
		if (!Global.SupportBIT.APP_LEDKeyboardSetting)
		{
			BTN_LEDKeyboard.IsVisible = false;
		}
		if (!Global.SupportBIT.APP_Flexikey)
		{
			BTN_Flexikey.IsVisible = false;
		}
		if (!Global.SupportBIT.PowerModeBit_Performance)
		{
			RB_performance.IsVisible = false;
		}
		UpdatePowerMode();
		LoadMultiLang();
		Global.REventlog.Arg001Event += REventlog_Arg001Event;
		Global.REventlog.FnKeyEvent += REventlog_FnKeyEvent;
		Global.REventlog.FanEvent += REventlog_FanEvent;
		LoadingTimer.Interval = TimeSpan.FromMilliseconds(1000.0);
		LoadingTimer.Tick += LoadingTimer_Tick;
		Opened += (_, _) => Topmost = false;
	}

	private void REventlog_FanEvent(string EventID)
	{
	}

	private void REventlog_FnKeyEvent(string EventID)
	{
		Dispatcher.UIThread.Post(() =>
		{
			switch (EventID)
			{
			case "102":
				LoadMultiLang();
				break;
			case "202":
				UpdatePowerMode();
				break;
			case "204":
				Global.CommonCode.Close_UI();
				Close();
				break;
			}
		});
	}

	private void REventlog_Arg001Event(int EventID)
	{
	}

	private void LoadMultiLang()
	{
		RB_performance.Content = lang.TranslationLanguage("_Performance_MCJ");
		RB_Entertainment.Content = lang.TranslationLanguage("_Balanced");
		RB_Quiet.Content = lang.TranslationLanguage("_Quiet_MCJ");
		WP_PowerMode_text.Text = lang.TranslationLanguage("_PowerModes_MCJ");
		BTN_FanSpeedSetting.Content = lang.TranslationLanguage("_FanSpeedSetting_MCJ");
		BTN_LEDKeyboard.Content = lang.TranslationLanguage("_LEDKeyboard_MCJ");
		RB_performance.Tag = lang.TranslationLanguage("_PerformanceSummary_MCJ");
		RB_Entertainment.Tag = lang.TranslationLanguage("_BalanceSummary_MCJ");
		RB_Quiet.Tag = lang.TranslationLanguage("_QuiteSummary_MCJ");
	}

	private void BTN_FanSpeedSetting_Click(object sender, RoutedEventArgs e)
	{
		Global.CommonCode.LaunchApp("clevofan");
		Loading();
	}

	private void BTN_LEDKeyboard_Click(object sender, RoutedEventArgs e)
	{
		Global.CommonCode.LaunchApp("clevokeyboard");
		Loading();
	}

	private void BTN_Flexikey_Click(object sender, RoutedEventArgs e)
	{
		Global.CommonCode.LaunchApp("clevoflexikey");
		Loading();
	}

	private void Window_Closing()
	{
		Global.CommonCode.Close_UI();
	}

	private void UpdatePowerMode()
	{
		Dispatcher.UIThread.Post(() =>
		{
			Global.CommonCode.ReadPowerModeInsydeBuffer();
			switch (Global.CommonCode.powerModeValue)
			{
			case 0:
				RB_Quiet.IsChecked = true;
				break;
			case 2:
				RB_performance.IsChecked = true;
				break;
			case 3:
				RB_Entertainment.IsChecked = true;
				break;
			default:
				RB_Entertainment.IsChecked = true;
				break;
			}
		});
	}

	private void RB_performance_Click(object sender, RoutedEventArgs e)
	{
		if (Global.CommonCode.powerModeValue != 2)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^2");
			Global.CommonCode.powerModeValue = 2;
			Loading();
		}
	}

	private void RB_Entertainment_Click(object sender, RoutedEventArgs e)
	{
		if (Global.CommonCode.powerModeValue != 3)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^3");
			Global.CommonCode.powerModeValue = 3;
			Loading();
		}
	}

	private void RB_Quiet_Click(object sender, RoutedEventArgs e)
	{
		if (Global.CommonCode.powerModeValue != 0)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^0");
			Global.CommonCode.powerModeValue = 0;
			Loading();
		}
	}

	private void RB_PowerSaving_Click(object sender, RoutedEventArgs e)
	{
		if (Global.CommonCode.powerModeValue != 1)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^0");
			Global.CommonCode.powerModeValue = 1;
			Loading();
		}
	}

	private void Loading()
	{
		Cursor = new Cursor(StandardCursorType.Wait);
		main.IsEnabled = false;
		LoadingTimer.Start();
	}

	private void LoadingTimer_Tick(object sender, EventArgs e)
	{
		Cursor = null;
		main.IsEnabled = true;
		LoadingTimer.Stop();
	}

	private void BTN_min_Click(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void BTN_close_Click(object sender, RoutedEventArgs e)
	{
		Close();
	}

	private void Check_FullSrceen_Click(object sender, RoutedEventArgs e)
	{
		if (Check_FullSrceen.IsChecked == true)
		{
			WindowState = WindowState.Normal;
		}
		else
		{
			WindowState = WindowState.Maximized;
		}
	}

	private void title_move_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
	}
}
