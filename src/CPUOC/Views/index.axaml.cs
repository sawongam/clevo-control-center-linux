using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CPUOC30.Model;
using CPUOC30.ViewModel;
using CCenter.Platform;

namespace CPUOC30.View;

public partial class index : Window
{
	private const int FULLSCREEN = 1;

	private DispatcherTimer UpdatePerSecond_timer = new DispatcherTimer();
	private int OLD_CPU_clock = 0;
	private int Current_CPU_clock_offset = 0;
	private Point point;
	private float rateW = 0f;
	private float rateH = 0f;
	private double _dbRawWidth = 0.0;
	private double _dbRawHeight = 0.0;
	private double maxW = 1920.0;
	private double maxH = 1080.0;
	private bool isWindowsFullscreen = false;
	private bool IsInitFinished = false;
	private int powerMode = -1;
	private int lastPowerSetting = -1;
	private int lastPowerLine = -2;

	static readonly IBrush Grey = new SolidColorBrush(Color.FromArgb(255, 102, 102, 102));
	static readonly IBrush White = Brushes.White;

	IndexViewModel VM => (IndexViewModel)DataContext;

	static string CbText(ComboBox c) => c.SelectedItem?.ToString() ?? "";
	static void CbSetText(ComboBox c, string text)
	{
		if (c.ItemsSource == null) return;
		foreach (var o in c.ItemsSource) if (o?.ToString() == text) { c.SelectedItem = o; return; }
	}

	public index() { InitializeComponent(); }

	public index(IndexViewModel vm)
	{
		DataContext = vm;
		InitializeComponent();
		Closing += mainWindow_Closing;
		_dbRawWidth = Width;
		_dbRawHeight = Height;
		CheckFullScreen();
		LoadMultiLang();
		Task.Run(() =>
		{
			IsInitFinished = false;
			DebugLog.WriteDbgLog2Inlog("cpuoc_bg_104");
		}).ContinueWith(_ => Dispatcher.UIThread.Post(() =>
		{
			VM.Initial();
			UpdateMainUI();
			UpdatePerSecond_timer.Interval = TimeSpan.FromSeconds(1.0);
			UpdatePerSecond_timer.Tick += UpdatePerSecond_Tick;
			UpdatePerSecond_timer.Stop();
			UpdatePerSecond_timer.Start();
			DebugLog.WriteDbgLog2Inlog("Close_Loading");
			IsInitFinished = true;
			ApplyScreenshotPage();
		}));
	}

	/// <summary>--page NAME: open on a given panel / dialog (screenshots only).</summary>
	void ApplyScreenshotPage()
	{
		if (!AppHost.IsScreenshot) return;
		switch (AppHost.PageArg)
		{
			case "power": power_limit_MouseLeftButtonDown(null, null); break;
			case "vr": CPU_VR_MouseLeftButtonDown(null, null); break;
			case "voltage": CPU_Voltage_MouseLeftButtonDown(null, null); break;
			case "dialog": Apps_DialogBox.IsVisible = true; break;
		}
	}

	private void UpdateMainUI()
	{
		SwitchPowerMode();
		if (VM.Turbo_Min < 0) CPU_TurboMin_TEXTBOX.Text = "0";
		else CPU_TurboMin_TEXTBOX.Text = VM.Turbo_Min.ToString();
		if (VM.Turbo_Max.Equals(0)) CPU_TurboMax_TEXTBOX.Text = "N/A";
		else CPU_TurboMax_TEXTBOX.Text = VM.Turbo_Max.ToString();
		UpdateUIPerSecond();
		UpdateUI_ScaleDataList();
		UpdateUI_XTUInfo();
		SetSaveBtnEnable(isEnable: false);
		CheckPowerStatus();
	}

	/// <summary>Plug-in-power gating (SystemInformation.PowerStatus / SystemEvents.PowerModeChanged).</summary>
	private void ApplyUnplugged()
	{
		current_power_mode_LABEL.Content = VM.TranslationLanguage("_PlugInPower");
		switch_power_mode_description_LABEL.IsVisible = false;
		Apps_CPU_VR_Limit.IsVisible = false;
		Apps_CPU_Voltage.IsVisible = false;
		Apps_CPU_Ratio_Override.IsVisible = false;
		Apps_Power_Limit.IsVisible = false;
		Apps_PowerMode_Description.IsVisible = true;
		SetSaveBtnEnable(isEnable: false);
		SetDefaultBtnEnable(isEnable: false);
		SetNav(-1, false);
	}

	private void SystemEvents_PowerModeChanged()
	{
		int line = LinuxSensors.PowerLine();
		if (line == 1) SwitchPowerMode();
		else if (line == 0) ApplyUnplugged();
	}

	private void CheckPowerStatus()
	{
		int line = LinuxSensors.PowerLine();
		lastPowerLine = line;
		if (line == 1) SwitchPowerMode();
		else if (line == 0) ApplyUnplugged();
	}

	private void UpdateUI_XTUInfo()
	{
		decimal num = VM.PL1;
		comboBox_CPUPL1Minc.SelectedItem = num;
		num = VM.PL2;
		comboBox_CPUPL2Minc.SelectedItem = num;
		num = (decimal)VM.PL_Time;
		if (num > VM.PLTimeDataList[VM.PLTimeDataList.Count - 1]) num = VM.PLTimeDataList[VM.PLTimeDataList.Count - 1];
		comboBox_CPUPLTMinc.SelectedItem = num;
		num = VM.CPU_VR;
		if (VM.CPU_VR_Auto)
		{
			check_VRAuto.IsVisible = true;
			check_VRAuto.IsChecked = true;
			if (num.ToString() == "79228162514264337593543950335") comboBox_CPUVRMinc.SelectedItem = VM.VRDataList[0];
			else comboBox_CPUVRMinc.SelectedItem = num;
			grid_VRlimit.IsVisible = false;
		}
		else
		{
			check_VRAuto.IsChecked = false;
			comboBox_CPUVRMinc.SelectedItem = num;
		}
		if (VM.IsControlCenterControlCPU_UnderVolting)
		{
			num = VM.Voltage_Offset;
			comboBox_CPUAdaptive_Offset.SelectedItem = num;
			num = VM.Voltage_GT;
			comboBox_CPUAdaptive_Graphics.SelectedItem = num;
			num = (decimal)VM.Voltage_Override;
			string text = Convert.ToString(num);
			if (text.Equals("0")) comboBox_CPUAdaptive_override.SelectedItem = VM.TranslationLanguage("_Default");
			else comboBox_CPUAdaptive_override.SelectedItem = text;
			BTN_CPUAdaptive.IsChecked = true;
			CPUAdaptive_offset_value.IsVisible = true;
			CPUAdaptive_override_value.IsVisible = true;
			SP_CPUAdaptive_Graphics.IsVisible = VM.Custom_Id == 1;
		}
		else
		{
			CPU_Voltage_IMAGE.IsVisible = false;
			CPU_Voltage_LABEL.IsVisible = false;
			Apps_CPU_Voltage.IsVisible = false;
		}
		int num2 = 0;
		bool flag = false;
		bool flag2 = false;
		var pCores = new (int val, ComboBox cb, StackPanel sp)[]
		{
			(VM.P_Core_1, comboBox_CPURatioMinc1, SP_ratio1), (VM.P_Core_2, comboBox_CPURatioMinc2, SP_ratio2),
			(VM.P_Core_3, comboBox_CPURatioMinc3, SP_ratio3), (VM.P_Core_4, comboBox_CPURatioMinc4, SP_ratio4),
			(VM.P_Core_5, comboBox_CPURatioMinc5, SP_ratio5), (VM.P_Core_6, comboBox_CPURatioMinc6, SP_ratio6),
			(VM.P_Core_7, comboBox_CPURatioMinc7, SP_ratio7), (VM.P_Core_8, comboBox_CPURatioMinc8, SP_ratio8),
			(VM.P_Core_9, comboBox_CPURatioMinc9, SP_ratio9), (VM.P_Core_10, comboBox_CPURatioMinc10, SP_ratio10),
		};
		foreach (var (val, cb, sp) in pCores)
		{
			if ((decimal)val > 0m) { cb.SelectedItem = (decimal)val; num2++; flag = true; }
			else SP_ratioOverride.Children.Remove(sp);
		}
		var eCores = new (int val, ComboBox cb, StackPanel sp)[]
		{
			(VM.E_Core_1, comboBox_CPURatioMincE1, SP_ratioE1), (VM.E_Core_2, comboBox_CPURatioMincE2, SP_ratioE2),
			(VM.E_Core_3, comboBox_CPURatioMincE3, SP_ratioE3), (VM.E_Core_4, comboBox_CPURatioMincE4, SP_ratioE4),
			(VM.E_Core_5, comboBox_CPURatioMincE5, SP_ratioE5), (VM.E_Core_6, comboBox_CPURatioMincE6, SP_ratioE6),
			(VM.E_Core_7, comboBox_CPURatioMincE7, SP_ratioE7), (VM.E_Core_8, comboBox_CPURatioMincE8, SP_ratioE8),
		};
		foreach (var (val, cb, sp) in eCores)
		{
			if ((decimal)val > 0m) { cb.SelectedItem = (decimal)val; num2++; flag2 = true; }
			else SP_ratioOverride.Children.Remove(sp);
		}
		if (!flag) SP_ratioOverride.Children.Remove(SP_ratioTitle_P);
		if (!flag2) SP_ratioOverride.Children.Remove(SP_ratioTitle_E);
		SP_CPU_Ratio_OverrideList.Height = num2 * 100 + 100;
	}

	private void UpdateUI_ScaleDataList()
	{
		var vm = VM;
		SL_powerlimit1.DataContext = vm.PL1PL2DataList;
		SL_powerlimit1.Maximum = vm.PL1PL2DataList.Count;
		T_PL1Min.Text = vm.PL1PL2DataList[0].ToString();
		T_PL1Max.Text = vm.PL1PL2DataList[vm.PL1PL2DataList.Count - 1].ToString();
		comboBox_CPUPL1Minc.ItemsSource = vm.PL1PL2DataList;
		SL_powerlimit2.DataContext = SL_powerlimit1.DataContext;
		SL_powerlimit2.Maximum = SL_powerlimit1.Maximum;
		T_PL2Min.Text = T_PL1Min.Text;
		T_PL2Max.Text = T_PL1Max.Text;
		comboBox_CPUPL2Minc.ItemsSource = comboBox_CPUPL1Minc.ItemsSource;
		SL_powerlimitTime.DataContext = vm.PLTimeDataList;
		SL_powerlimitTime.Maximum = vm.PLTimeDataList.Count;
		T_PLTMin.Text = vm.PLTimeDataList[0].ToString();
		T_PLTMax.Text = vm.PLTimeDataList[vm.PLTimeDataList.Count - 1].ToString();
		comboBox_CPUPLTMinc.ItemsSource = vm.PLTimeDataList;
		SL_VR.DataContext = vm.VRDataList;
		SL_VR.Maximum = vm.VRDataList.Count;
		T_CVRMin.Text = vm.VRDataList[0].ToString();
		T_CVRMax.Text = vm.VRDataList[vm.VRDataList.Count - 1].ToString();
		comboBox_CPUVRMinc.ItemsSource = vm.VRDataList;
		SL_CPUAdaptive_offset.Maximum = vm.VoltageOffsetDataList.Count;
		T_CPUAdaptive_offsetMin.Text = vm.VoltageOffsetDataList[0].ToString();
		T_CPUAdaptive_offsetMax.Text = vm.VoltageOffsetDataList[vm.VoltageOffsetDataList.Count - 1].ToString();
		comboBox_CPUAdaptive_Offset.ItemsSource = vm.VoltageOffsetDataList;
		SL_CPUAdaptive_Graphics.Maximum = vm.GraphicsVoltageOffsetDataList.Count;
		T_CPUAdaptive_GraphicsMin.Text = vm.GraphicsVoltageOffsetDataList[0].ToString();
		T_CPUAdaptive_GraphicsMax.Text = vm.GraphicsVoltageOffsetDataList[vm.GraphicsVoltageOffsetDataList.Count - 1].ToString();
		comboBox_CPUAdaptive_Graphics.ItemsSource = vm.GraphicsVoltageOffsetDataList;
		SL_CPUAdaptive_override.Maximum = vm.OverrideVoltageDataList.Count;
		T_CPUAdaptive_overrideMin.Text = vm.OverrideVoltageDataList[0].ToString();
		T_CPUAdaptive_overrideMax.Text = vm.OverrideVoltageDataList[vm.OverrideVoltageDataList.Count - 1].ToString();
		comboBox_CPUAdaptive_override.ItemsSource = vm.OverrideVoltageDataList;
		SL_ratio1.Maximum = vm.CPUCoreDataList.Count;
		T_ratioIncMin1.Text = vm.CPUCoreDataList[0].ToString();
		T_ratioIncMax1.Text = vm.CPUCoreDataList[vm.CPUCoreDataList.Count - 1].ToString();
		comboBox_CPURatioMinc1.ItemsSource = vm.CPUCoreDataList;
		var sls = new Slider[] { SL_ratio2, SL_ratio3, SL_ratio4, SL_ratio5, SL_ratio6, SL_ratio7, SL_ratio8, SL_ratio9, SL_ratio10, SL_ratioE1, SL_ratioE2, SL_ratioE3, SL_ratioE4, SL_ratioE5, SL_ratioE6, SL_ratioE7, SL_ratioE8 };
		var mins = new TextBlock[] { T_ratioIncMin2, T_ratioIncMin3, T_ratioIncMin4, T_ratioIncMin5, T_ratioIncMin6, T_ratioIncMin7, T_ratioIncMin8, T_ratioIncMin9, T_ratioIncMin10, T_ratioIncMinE1, T_ratioIncMinE2, T_ratioIncMinE3, T_ratioIncMinE4, T_ratioIncMinE5, T_ratioIncMinE6, T_ratioIncMinE7, T_ratioIncMinE8 };
		var maxs = new TextBlock[] { T_ratioIncMax2, T_ratioIncMax3, T_ratioIncMax4, T_ratioIncMax5, T_ratioIncMax6, T_ratioIncMax7, T_ratioIncMax8, T_ratioIncMax9, T_ratioIncMax10, T_ratioIncMaxE1, T_ratioIncMaxE2, T_ratioIncMaxE3, T_ratioIncMaxE4, T_ratioIncMaxE5, T_ratioIncMaxE6, T_ratioIncMaxE7, T_ratioIncMaxE8 };
		var cbs = new ComboBox[] { comboBox_CPURatioMinc2, comboBox_CPURatioMinc3, comboBox_CPURatioMinc4, comboBox_CPURatioMinc5, comboBox_CPURatioMinc6, comboBox_CPURatioMinc7, comboBox_CPURatioMinc8, comboBox_CPURatioMinc9, comboBox_CPURatioMinc10, comboBox_CPURatioMincE1, comboBox_CPURatioMincE2, comboBox_CPURatioMincE3, comboBox_CPURatioMincE4, comboBox_CPURatioMincE5, comboBox_CPURatioMincE6, comboBox_CPURatioMincE7, comboBox_CPURatioMincE8 };
		for (int i = 0; i < sls.Length; i++)
		{
			sls[i].Maximum = SL_ratio1.Maximum;
			mins[i].Text = T_ratioIncMin1.Text;
			maxs[i].Text = T_ratioIncMax1.Text;
			cbs[i].ItemsSource = vm.CPUCoreDataList;
		}
	}

	private void UpdateUI_CPUTemperature() { }

	private void SwitchPowerMode()
	{
		powerMode = VM.GetPowerMode();
		lastPowerSetting = powerMode;
		switch (powerMode)
		{
		case 0:
		case 1:
		case 3:
			current_power_mode_LABEL.Content = VM.TranslationLanguage(powerMode == 0 ? "_Quiet" : powerMode == 1 ? "_PowerSaving" : "_Entertainment");
			switch_power_mode_description_LABEL.Content = VM.TranslationLanguage("_FN3");
			switch_power_mode_description_LABEL.IsVisible = true;
			Apps_CPU_VR_Limit.IsVisible = false;
			Apps_CPU_Voltage.IsVisible = false;
			Apps_CPU_Ratio_Override.IsVisible = false;
			Apps_Power_Limit.IsVisible = false;
			Apps_PowerMode_Description.IsVisible = true;
			SetSaveBtnEnable(isEnable: false);
			SetDefaultBtnEnable(isEnable: false);
			SetNav(-1, false);
			break;
		case 2:
			Apps_CPU_VR_Limit.IsVisible = false;
			Apps_CPU_Voltage.IsVisible = false;
			Apps_CPU_Ratio_Override.IsVisible = true;
			Apps_Power_Limit.IsVisible = false;
			Apps_PowerMode_Description.IsVisible = false;
			SetDefaultBtnEnable(isEnable: true);
			SetSaveBtnEnable(isEnable: false);
			SetNav(0, true);
			break;
		default:
			current_power_mode_LABEL.Content = "error: power mode=" + powerMode;
			break;
		}
	}

	/// <summary>Left menu: selected = 0 ratio, 1 power limit, 2 VR, 3 voltage, 4 cache ratio, -1 none. Cursor Hand when enabled.</summary>
	private void SetNav(int selected, bool enabled)
	{
		string Pic(string n, bool sel) => "Assets/btn_" + n + (sel ? "_selected.png" : "_normal.png");
		CPU_ratio_IMAGE.Source = Img.Load(Pic("cpu_ratio_override", selected == 0));
		cache_ratio_IMAGE.Source = Img.Load(Pic("cache_ratio_override", selected == 4));
		CPU_Voltage_IMAGE.Source = Img.Load(Pic("cpu_voltage", selected == 3));
		CPU_VR_IMAGE.Source = Img.Load(Pic("cpu_vr_limit", selected == 2));
		power_limit_IMAGE.Source = Img.Load(Pic("power_limit", selected == 1));
		var cur = new Cursor(enabled ? StandardCursorType.Hand : StandardCursorType.Arrow);
		foreach (Control c in new Control[] { CPU_ratio_IMAGE, cache_ratio_IMAGE, CPU_Voltage_IMAGE, CPU_VR_IMAGE, power_limit_IMAGE, power_limit_LABEL, CPU_VR_LABEL, CPU_Voltage_LABEL, cache_ratio_LABEL, CPU_ratio_LABEL }) c.Cursor = cur;
		power_limit_LABEL.Foreground = selected == 1 ? White : Grey;
		CPU_VR_LABEL.Foreground = selected == 2 ? White : Grey;
		CPU_Voltage_LABEL.Foreground = selected == 3 ? White : Grey;
		cache_ratio_LABEL.Foreground = Grey;
		CPU_ratio_LABEL.Foreground = selected == 0 ? White : Grey;
	}

	private void SetSaveBtnEnable(bool isEnable)
	{
		if (isEnable)
		{
			save_IMAGE.Source = Img.Load("Assets/btn_save_normal.png");
			save_IMAGE.IsEnabled = true;
			save_LABEL.Foreground = White;
			save_LABEL.IsEnabled = true;
		}
		else
		{
			save_IMAGE.Source = Img.Load("Assets/btn_save_pressed.png");
			save_IMAGE.IsEnabled = false;
			save_LABEL.Foreground = Grey;
			save_LABEL.IsEnabled = false;
		}
	}

	private void SetDefaultBtnEnable(bool isEnable)
	{
		if (isEnable)
		{
			default_IMAGE.Source = Img.Load("Assets/btn_default_normal.png");
			default_IMAGE.IsEnabled = true;
			default_LABEL.Foreground = White;
			default_LABEL.IsEnabled = true;
		}
		else
		{
			default_IMAGE.Source = Img.Load("Assets/btn_default_pressed.png");
			default_IMAGE.IsEnabled = false;
			default_LABEL.Foreground = Grey;
			default_LABEL.IsEnabled = false;
		}
	}

	private void CheckFullScreen()
	{
		if (VM.IsFullScreen == 1)
		{
			WindowFullSrceen();
			btnWindow.Background = new ImageBrush(Img.Load("Assets/ic_window.png"));
			isWindowsFullscreen = true;
		}
		else
		{
			WindowFitSize();
			CenterWindowOnScreen();
			btnWindow.Background = new ImageBrush(Img.Load("Assets/ic_fullscreen.png"));
			isWindowsFullscreen = false;
		}
	}

	(double w, double h, double scale) PrimaryScreen()
	{
		var s = Screens?.Primary;
		if (s == null) return (1920, 1080, 1);
		return (s.Bounds.Width / s.Scaling, s.Bounds.Height / s.Scaling, s.Scaling);
	}

	private void CenterWindowOnScreen()
	{
		var (sw, sh, scale) = PrimaryScreen();
		double width = Width;
		double height = Height;
		WindowStartupLocation = WindowStartupLocation.Manual;
		Position = new PixelPoint((int)((sw / 2.0 - width / 2.0) * scale), (int)((sh / 2.0 - height / 2.0) * scale));
		Topmost = false;
	}

	private void WindowFitSize()
	{
		var (sw, sh, _) = PrimaryScreen();
		rateW = 2560f / (float)sw;
		rateH = 1440f / (float)sh;
		maxW = Math.Round(_dbRawWidth / (double)rateW, 0);
		maxH = Math.Round(_dbRawHeight / (double)rateH, 0);
		MaxWidth = maxW;
		MaxHeight = maxH;
		mainViewbox.Width = MaxWidth;
		mainViewbox.Height = MaxHeight;
		WindowState = WindowState.Normal;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
		InvalidateVisual();
		isWindowsFullscreen = false;
	}

	private void WindowFullSrceen()
	{
		IsVisible = false;
		WindowState = WindowState.Normal;
		MaxWidth = double.PositiveInfinity;
		MaxHeight = double.PositiveInfinity;
		WindowState = WindowState.Maximized;
		IsVisible = true;
		var (sw, sh, _) = PrimaryScreen();
		Width = sw;
		Height = sh;
		mainViewbox.Width = sw;
		mainViewbox.Height = sh;
		isWindowsFullscreen = true;
	}

	private void UpdatePerSecond_Tick(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post(() => { UpdateUIPerSecond(); PollExternalChanges(); });
	}

	/// <summary>Replaces the EventLog notifications (FnKey power-mode change "clevofnkey^202", AC plug events) by polling.</summary>
	private void PollExternalChanges()
	{
		int line = LinuxSensors.PowerLine();
		if (line != lastPowerLine) { lastPowerLine = line; SystemEvents_PowerModeChanged(); return; }
		if (line == 0) return;
		int pm = VM.GetPowerMode();
		if (pm != lastPowerSetting) SwitchPowerMode();
	}

	private void UpdateUIPerSecond()
	{
		VM.GetPerSecondInfo();
		UpdateUI_CPU_freq(VM.CPU_Frequency_Per_Second);
		CPU_voltage_TEXTBOX.Text = VM.Voltage_Per_Second;
		UpdateUI_Memory_Clock(VM.Memory_Frequency_Per_Second);
		UpdateUI_CPU_Temperature(VM.Temperature_Per_Second);
		UpdateUI_CPUUtiliztion(VM.CPU_Usage_Per_Second);
	}

	private void UpdateUI_CPU_freq(int Current_CPU_clock)
	{
		if (OLD_CPU_clock != Current_CPU_clock)
		{
			Current_CPU_clock_offset = (Current_CPU_clock - OLD_CPU_clock) / 19;
			CPU_Core_TEXTBOX.Text = Current_CPU_clock.ToString();
			CPU_Core_Reflection_TEXTBOX.Text = Current_CPU_clock.ToString();
			int n = Current_CPU_clock >= VM.Turbo_Max ? 12
				: (Current_CPU_clock != 0 && VM.Turbo_Max != 0)
					? ((Current_CPU_clock < VM.Turbo_Min || Current_CPU_clock >= VM.Turbo_Max) ? Current_CPU_clock / (VM.Turbo_Min / 10) : 11)
					: 0;
			cpu_clock_IMAGE.Source = Img.Load("Assets/clock_" + n + ".png");
		}
	}

	private void UpdateUI_Memory_Clock(int clock)
	{
		if (RAMClock_TEXTBOX.Text == clock.ToString()) return;
		RamClock_0_Bar_IMAGE.IsVisible = false;
		RamClock_405_Bar_IMAGE.IsVisible = false;
		RamClock_810_Bar_IMAGE.IsVisible = false;
		RamClock_2999_Bar_IMAGE.IsVisible = false;
		RamClock_4006_Bar_IMAGE.IsVisible = false;
		RamClock_Full_Bar_IMAGE.IsVisible = false;
		if (clock <= 405) RamClock_405_Bar_IMAGE.IsVisible = true;
		else if (clock <= 810) RamClock_810_Bar_IMAGE.IsVisible = true;
		else if (clock < VM.Memory_Speed_Max) RamClock_2999_Bar_IMAGE.IsVisible = true;
		else if (clock - VM.Memory_Speed_Max < 10) RamClock_4006_Bar_IMAGE.IsVisible = true;
		else RamClock_Full_Bar_IMAGE.IsVisible = true;
		RAMClock_TEXTBOX.Text = clock.ToString();
	}

	private void UpdateUI_CPU_Temperature(int CPU_Temp_new)
	{
		double num = 135.0 + (double)CPU_Temp_new * 1.8;
		var rt = new RotateTransform(num);
		cpu_needle_layer1_IMAGE.RenderTransform = rt;
		cpu_needle_layer2_IMAGE.RenderTransform = rt;
		cpu_temperature_TEXTBOX.Text = CPU_Temp_new.ToString();
	}

	private void UpdateUI_CPUUtiliztion(int util)
	{
		if (CPU_util_TEXTBOX.Text == util.ToString()) return;
		CPU_util_TEXTBOX.Text = util.ToString();
		if (util < 0) util = 0;
		if (util > 100) util = 100;
		cpu_unilization_bar_IMAGE.Source = Img.Load("Assets/utilization_" + util + ".png");
	}

	private void LoadMultiLang()
	{
		string T(string k) => VM.TranslationLanguage(k);
		TextBox_CPU_Voltage.Text = T("_CPUVoltage");
		TextBox_Utilization.Text = T("_Utilization");
		TextBox_CPUSetting.Text = T("_CPUSetting");
		CPU_ratio_LABEL.Content = T("_CPURatioOverride");
		power_limit_LABEL.Content = T("_PowerLimit");
		CPU_Voltage_LABEL.Content = T("_CPUVoltage");
		CPU_VR_LABEL.Content = T("_CPUVRLimit");
		cache_ratio_LABEL.Content = T("_CacheRationOverride");
		TextBox_Memory_Clock.Text = T("_MemoryClock");
		T_CPU_PL1.Text = T("_PowerLimit1");
		T_CPU_PLT.Text = T("_PowerLimitTime");
		T_CPU_PL2.Text = T("_PowerLimit2");
		BTN_CPUAdaptive.Content = T("_Adaptive");
		T_CPUAdaptive_overrideINC.Text = T("_CoreAndCacheVoltage");
		T_CPUAdaptive_OffsetINC.Text = T("_CoreAndCacheOffset");
		T_CPUAdaptive_GraphicsINC.Text = T("_GraphicsOffset");
		check_VRAuto.Content = T("_AUTOMATIC");
		T_CPU_VRINC.Text = T("_VRLimit");
		TextBox_TurboMax.Text = T("_TurboMax");
		TextBox_CoreBase.Text = T("_CoreBase");
		TextBox_Temperature.Text = T("_Temperature");
		default_LABEL.Content = T("_Default");
		save_LABEL.Content = T("_Save");
		T_Dialog_info.Text = T("_MsgCPUOC");
		BTN_OK.Content = T("_OK");
		BTN_Cancel.Content = T("_Cancel");
	}

	private void Button_Click_Close(object sender, RoutedEventArgs e) => Close();

	private void Button_Click_Minimize(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

	private void Button_Click_Window(object sender, RoutedEventArgs e)
	{
		if (isWindowsFullscreen)
		{
			WindowFitSize();
			CenterWindowOnScreen();
			btnWindow.Background = new ImageBrush(Img.Load("Assets/ic_fullscreen.png"));
			isWindowsFullscreen = false;
		}
		else
		{
			WindowFullSrceen();
			btnWindow.Background = new ImageBrush(Img.Load("Assets/ic_window.png"));
			isWindowsFullscreen = true;
		}
	}

	private void CheckSaveButtonStatus()
	{
		if (IsInitFinished && powerMode == 2) SetSaveBtnEnable(isEnable: true);
	}

	private void UpdateUI_DefaultInfo()
	{
		VM.GetDefaultInfo();
		CbSetText(comboBox_CPUPL1Minc, VM.PL1.ToString());
		CbSetText(comboBox_CPUPLTMinc, VM.PL_Time.ToString());
		CbSetText(comboBox_CPUPL2Minc, VM.PL2.ToString());
		if (VM.CPU_VR_Auto) check_VRAuto.IsChecked = true;
		else
		{
			check_VRAuto.IsChecked = false;
			CbSetText(comboBox_CPUVRMinc, VM.CPU_VR.ToString());
			grid_VRlimit.IsVisible = true;
		}
		if (VM.IsControlCenterControlCPU_UnderVolting)
		{
			CbSetText(comboBox_CPUAdaptive_Offset, VM.Voltage_Offset.ToString());
			if (VM.Voltage_Override == 0f) comboBox_CPUAdaptive_override.SelectedItem = VM.TranslationLanguage("_Default");
			else CbSetText(comboBox_CPUAdaptive_override, VM.Voltage_Override.ToString());
			CbSetText(comboBox_CPUAdaptive_Graphics, VM.Voltage_GT.ToString());
			SP_CPUAdaptive_Graphics.IsVisible = VM.Custom_Id == 1;
		}
		CbSetText(comboBox_CPURatioMinc1, VM.P_Core_1.ToString());
		VM.Turbo_Max = (int)((decimal)VM.P_Core_1 * VM.Reference);
		CPU_TurboMax_TEXTBOX.Text = VM.Turbo_Max.ToString();
		CbSetText(comboBox_CPURatioMinc2, VM.P_Core_2.ToString());
		CbSetText(comboBox_CPURatioMinc3, VM.P_Core_3.ToString());
		CbSetText(comboBox_CPURatioMinc4, VM.P_Core_4.ToString());
		CbSetText(comboBox_CPURatioMinc5, VM.P_Core_5.ToString());
		CbSetText(comboBox_CPURatioMinc6, VM.P_Core_6.ToString());
		CbSetText(comboBox_CPURatioMinc7, VM.P_Core_7.ToString());
		CbSetText(comboBox_CPURatioMinc8, VM.P_Core_8.ToString());
		CbSetText(comboBox_CPURatioMinc9, VM.P_Core_9.ToString());
		CbSetText(comboBox_CPURatioMinc10, VM.P_Core_10.ToString());
		CbSetText(comboBox_CPURatioMincE1, VM.E_Core_1.ToString());
		CbSetText(comboBox_CPURatioMincE2, VM.E_Core_2.ToString());
		CbSetText(comboBox_CPURatioMincE3, VM.E_Core_3.ToString());
		CbSetText(comboBox_CPURatioMincE4, VM.E_Core_4.ToString());
		CbSetText(comboBox_CPURatioMincE5, VM.E_Core_5.ToString());
		CbSetText(comboBox_CPURatioMincE6, VM.E_Core_6.ToString());
		CbSetText(comboBox_CPURatioMincE7, VM.E_Core_7.ToString());
		CbSetText(comboBox_CPURatioMincE8, VM.E_Core_8.ToString());
	}

	/// <summary>DEFAULT: restores the defaults in the UI and writes them to the Insyde buffer (OC write, gated) + cpuoc_bg_106 style apply.</summary>
	private void default_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		DebugLog.WriteDbgLog2Outlog("CPUOC, Set Default");
		UpdateUI_DefaultInfo();
		VM.SetXTUInfoToInsydeBuffer();
		SetSaveBtnEnable(isEnable: false);
		VM.SetIsSaved(0);
	}

	private void save_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		Apps_DialogBox.IsVisible = true;
	}

	private void ShowPanel(int which)
	{
		if (powerMode != 2) return;
		Apps_CPU_VR_Limit.IsVisible = which == 2;
		Apps_CPU_Voltage.IsVisible = which == 3;
		Apps_CPU_Ratio_Override.IsVisible = which == 0;
		Apps_Power_Limit.IsVisible = which == 1;
		string Pic(string n, bool sel) => "Assets/btn_" + n + (sel ? "_selected.png" : "_normal.png");
		CPU_ratio_IMAGE.Source = Img.Load(Pic("cpu_ratio_override", which == 0));
		cache_ratio_IMAGE.Source = Img.Load(Pic("cache_ratio_override", which == 4));
		CPU_Voltage_IMAGE.Source = Img.Load(Pic("cpu_voltage", which == 3));
		CPU_VR_IMAGE.Source = Img.Load(Pic("cpu_vr_limit", which == 2));
		power_limit_IMAGE.Source = Img.Load(Pic("power_limit", which == 1));
		power_limit_LABEL.Foreground = which == 1 ? White : Grey;
		CPU_VR_LABEL.Foreground = which == 2 ? White : Grey;
		CPU_Voltage_LABEL.Foreground = which == 3 ? White : Grey;
		CPU_ratio_LABEL.Foreground = which == 0 ? White : Grey;
	}

	private void CPU_ratio_MouseLeftButtonDown(object sender, PointerPressedEventArgs e) => ShowPanel(0);
	private void CPU_VR_MouseLeftButtonDown(object sender, PointerPressedEventArgs e) => ShowPanel(2);
	private void CPU_Voltage_MouseLeftButtonDown(object sender, PointerPressedEventArgs e) => ShowPanel(3);
	private void power_limit_MouseLeftButtonDown(object sender, PointerPressedEventArgs e) => ShowPanel(1);
	private void cache_ratio_MouseLeftButtonDown(object sender, PointerPressedEventArgs e) => ShowPanel(4);

	private void check_VRAuto_Click(object sender, RoutedEventArgs e)
	{
		if (check_VRAuto.IsChecked == false) grid_VRlimit.IsVisible = true;
		else if (check_VRAuto.IsChecked == true) grid_VRlimit.IsVisible = false;
		SetSaveBtnEnable(isEnable: true);
	}

	private void SL_VR_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) { comboBox_CPUVRMinc.SelectedIndex = (int)SL_VR.Value; CheckSaveButtonStatus(); }
	private void comboBox_CPUVRMinc_SelectionChanged(object sender, SelectionChangedEventArgs e) { SL_VR.Value = comboBox_CPUVRMinc.SelectedIndex; CheckSaveButtonStatus(); }
	private void SL_CPUoverride_override_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) { comboBox_CPUAdaptive_override.SelectedIndex = (int)SL_CPUAdaptive_override.Value; CheckSaveButtonStatus(); }
	private void comboBox_CPUoverride_override_SelectionChanged(object sender, SelectionChangedEventArgs e) { SL_CPUAdaptive_override.Value = comboBox_CPUAdaptive_override.SelectedIndex; CheckSaveButtonStatus(); }
	private void SL_CPUAdaptive_CPUcache_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) { comboBox_CPUAdaptive_Offset.SelectedIndex = (int)SL_CPUAdaptive_offset.Value; CheckSaveButtonStatus(); }
	private void comboBox_CPUAdaptive_CPUcache_SelectionChanged(object sender, SelectionChangedEventArgs e) { SL_CPUAdaptive_offset.Value = comboBox_CPUAdaptive_Offset.SelectedIndex; CheckSaveButtonStatus(); }
	private void SL_CPUAdaptive_Graphics_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) { comboBox_CPUAdaptive_Graphics.SelectedIndex = (int)SL_CPUAdaptive_Graphics.Value; CheckSaveButtonStatus(); }
	private void comboBox_CPUAdaptive_Graphics_SelectionChanged(object sender, SelectionChangedEventArgs e) { SL_CPUAdaptive_Graphics.Value = comboBox_CPUAdaptive_Graphics.SelectedIndex; CheckSaveButtonStatus(); }

	private void SL_powerlimit1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_powerlimit1.Value > SL_powerlimit2.Value) SL_powerlimit2.Value = SL_powerlimit1.Value;
		comboBox_CPUPL1Minc.SelectedIndex = (int)SL_powerlimit1.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPUPL1Minc_SelectionChanged(object sender, SelectionChangedEventArgs e) { SL_powerlimit1.Value = comboBox_CPUPL1Minc.SelectedIndex; CheckSaveButtonStatus(); }
	private void SL_powerlimitTime_ValueChanged(object sender, RangeBaseValueChangedEventArgs e) { comboBox_CPUPLTMinc.SelectedIndex = (int)SL_powerlimitTime.Value; CheckSaveButtonStatus(); }
	private void comboBox_CPUPLTMinc_SelectionChanged(object sender, SelectionChangedEventArgs e) { SL_powerlimitTime.Value = comboBox_CPUPLTMinc.SelectedIndex; CheckSaveButtonStatus(); }
	private void SL_powerlimit2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_powerlimit2.Value < SL_powerlimit1.Value) SL_powerlimit1.Value = SL_powerlimit2.Value;
		comboBox_CPUPL2Minc.SelectedIndex = (int)SL_powerlimit2.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPUPL2Minc_SelectionChanged(object sender, SelectionChangedEventArgs e) { SL_powerlimit2.Value = comboBox_CPUPL2Minc.SelectedIndex; CheckSaveButtonStatus(); }

	private void mainWindow_Closing(object sender, WindowClosingEventArgs e)
	{
		DebugLog.WriteDbgLog2Inlog("cpuoc_bg_105");
	}

	private void Apps_2_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) return;
		point = e.GetPosition(this);
		double num = title.Height / (double)rateH;
		double num2 = title.Width / (double)rateW;
		double num4 = Apps_main.Width / (double)rateW;
		if (!isWindowsFullscreen && point.Y < num && point.X > num4 / 2.0 - num2 / 2.0 && point.X < num4 / 2.0 - num2 / 2.0 + num2)
			BeginMoveDrag(e);
	}

	private void Apps_main_MouseMove(object sender, PointerEventArgs e)
	{
		point = e.GetPosition(this);
		double num = title.Height / (double)rateH;
		double num2 = title.Width / (double)rateW;
		double num4 = Apps_main.Width / (double)rateW;
		if (!isWindowsFullscreen)
		{
			if (point.Y < num && point.X > num4 / 2.0 - num2 / 2.0 && point.X < num4 / 2.0 - num2 / 2.0 + num2) Cursor = new Cursor(StandardCursorType.SizeAll);
			else Cursor = null;
		}
	}

	private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
	{
		Apps_DialogBox.IsVisible = false;
	}

	private void BTN_OK_Click(object sender, RoutedEventArgs e)
	{
		Dispatcher.UIThread.Post(() =>
		{
			DebugLog.WriteDbgLog2Outlog("CPUOC, Set Save");
			var vm = VM;
			vm.PL1 = Convert.ToInt32(CbText(comboBox_CPUPL1Minc));
			vm.PL_Time = Convert.ToInt32(CbText(comboBox_CPUPLTMinc));
			vm.PL2 = Convert.ToInt32(CbText(comboBox_CPUPL2Minc));
			vm.CPU_VR = Convert.ToInt32(CbText(comboBox_CPUVRMinc));
			vm.CPU_VR_Auto = check_VRAuto.IsChecked == true;
			vm.P_Core_1 = ToI(comboBox_CPURatioMinc1); vm.P_Core_2 = ToI(comboBox_CPURatioMinc2);
			vm.P_Core_3 = ToI(comboBox_CPURatioMinc3); vm.P_Core_4 = ToI(comboBox_CPURatioMinc4);
			vm.P_Core_5 = ToI(comboBox_CPURatioMinc5); vm.P_Core_6 = ToI(comboBox_CPURatioMinc6);
			vm.P_Core_7 = ToI(comboBox_CPURatioMinc7); vm.P_Core_8 = ToI(comboBox_CPURatioMinc8);
			vm.P_Core_9 = ToI(comboBox_CPURatioMinc9); vm.P_Core_10 = ToI(comboBox_CPURatioMinc10);
			vm.E_Core_1 = ToI(comboBox_CPURatioMincE1); vm.E_Core_2 = ToI(comboBox_CPURatioMincE2);
			vm.E_Core_3 = ToI(comboBox_CPURatioMincE3); vm.E_Core_4 = ToI(comboBox_CPURatioMincE4);
			vm.E_Core_5 = ToI(comboBox_CPURatioMincE5); vm.E_Core_6 = ToI(comboBox_CPURatioMincE6);
			vm.E_Core_7 = ToI(comboBox_CPURatioMincE7); vm.E_Core_8 = ToI(comboBox_CPURatioMincE8);
			vm.Voltage_Offset = ToI(comboBox_CPUAdaptive_Offset);
			if (CbText(comboBox_CPUAdaptive_override).Equals(vm.TranslationLanguage("_Default"))) vm.Voltage_Override = 0f;
			else vm.Voltage_Override = Convert.ToSingle(CbText(comboBox_CPUAdaptive_override), System.Globalization.CultureInfo.CurrentCulture);
			if (vm.Custom_Id == 1) vm.Voltage_GT = ToI(comboBox_CPUAdaptive_Graphics);
			vm.Turbo_Max = (int)((decimal)vm.P_Core_1 * vm.Reference);
			CPU_TurboMax_TEXTBOX.Text = vm.Turbo_Max.ToString();
			vm.SetXTUInfoToInsydeBuffer();
			SetSaveBtnEnable(isEnable: false);
			Apps_DialogBox.IsVisible = false;
		});
	}

	static int ToI(ComboBox c)
	{
		var t = CbText(c);
		return t.Length == 0 ? 0 : Convert.ToInt32(t);
	}
}
