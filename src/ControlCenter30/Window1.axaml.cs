using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using CCenter.Platform;

namespace ControlCenter30;

public partial class Window1 : Window
{
	private enum UWPDescriptDirection
	{
		Top = 1,
		Bottom,
		Left,
		Right
	}

	private enum AMDMode
	{
		Auto,
		Manual
	}

	private enum AMDConvert
	{
		VoltageToIndex,
		IndexToVoltage,
		FrequencyToIndex,
		IndexToFrequency
	}

	public class AMD_CCX
	{
		public string ProcessorNameString { get; set; }

		public int TurboFrequency { get; set; }

		public int BaseFrequency { get; set; }
	}

	private ArrayList supportArray = new ArrayList();

	private List<AppInfo> _appInfo = new List<AppInfo>();

	private bool isFnKeyOn = false;

	private bool isAMDinitOK = false;

	private bool isInstallDTTDriver = false;

	private string powerModeIconPath = "Image/ic_Entertainment_disable.png";

	private int powerModeValue = 3;

	private byte CustomID = 0;

	private DispatcherTimer LoadingTimer = new DispatcherTimer();

	private DispatcherTimer ClickTimer = new DispatcherTimer();

	private DispatcherTimer PollTimer = new DispatcherTimer();

	public EventHandler CloseUI;

	public EventHandler ReadPowerFromInsydeBuffer;

	public EventHandler MultiLanguageUpdate;

	public EventHandler ReloadUI;

	public EventHandler TurboFanEnable;

	public EventHandler TurboFanDisable;

	private language lang = language.Instance;

	private SMBIOS GetSMBIOS;

	private AMD_CCX ccx = null;

	private int Mode_From_BIOS = 0;

	private int Frequency_From_BIOS = 0;

	private float Voltage_From_BIOS = 0f;

	private double _dbRawWidth = 1068.0;

	private double _dbRawHeight = 692.0;

	private double maxW = 1068.0;

	private double maxH = 692.0;

	private Button[] _btns;

	public Window1()
	{
		ReadPowerModeInsydeBuffer();
		supportArray = App.SupportArray;
		GetAllAppInfo(supportArray);
		InitializeComponent();
		_btns = new[] { button_1, button_2, button_3, button_4, button_5, button_6, button_7, button_8 };
		WireEvents();
		GetSMBIOS = new SMBIOS();
		DebugLog.WriteDbgLog2Outlog("SMBIOS Family:" + GetSMBIOS.System.Family + "   Manufacturer:" + GetSMBIOS.System.ManufacturerName);
		LoadMultiLang();
		InitAppInfo(_appInfo);
		CenterWindowOnScreen();
		InitialEventLog();
		LoadingTimer.Interval = TimeSpan.FromMilliseconds(500.0);
		LoadingTimer.Tick += Loading_Tick;
		ClickTimer.Interval = TimeSpan.FromMilliseconds(300.0);
		ClickTimer.Tick += Click_Tick;
		CloseUI = (EventHandler)Delegate.Combine(CloseUI, new EventHandler(ClosingUIEvent));
		ReadPowerFromInsydeBuffer = (EventHandler)Delegate.Combine(ReadPowerFromInsydeBuffer, new EventHandler(ReadPowerModeInsydeBuffer));
		MultiLanguageUpdate = (EventHandler)Delegate.Combine(MultiLanguageUpdate, new EventHandler(UpdateMultiLanguageEvent));
		ReloadUI = (EventHandler)Delegate.Combine(ReloadUI, new EventHandler(ReloadUIEvent));
		TurboFanEnable = (EventHandler)Delegate.Combine(TurboFanEnable, new EventHandler(TurboFanEnableEvent));
		TurboFanDisable = (EventHandler)Delegate.Combine(TurboFanDisable, new EventHandler(TurboFanDisableEvent));
		UC_DialogBox.CancelEvent += UC_DialogBox_CancelEvent;
		UC_DialogBox.OKEvent += UC_DialogBox_OKEvent;
		UC_DialogBox_panel.CancelEvent += UC_DialogBox_panel_CancelEvent;
		UC_DialogBox_panel.OKEvent += UC_DialogBox_panel_OKEvent;
		SetSkinColor();
		DisableTooltipsForEPSON();
		Apps_FnKey.IsVisible = false;
		// FindInstalls (registry Uninstall keys) / FindDrivers (WMI Win32_PnPSignedDriver) look for the Intel
		// "Dynamic Tuning" driver; it does not exist on Linux, so isInstallDTTDriver stays false.
		Update_PerformanceMode_UI();
		InitMode();
		Load_windowStyle();
		if (IsSupportAMD_Manual_Mode())
		{
			InitAMD();
		}
		else
		{
			SetAMDItemVisibility(isVisible: false);
		}
		ApplyScreenshotPage();
		// FnKey tells CC30 about power mode changes through the event log (clevofnkey^202). When FnKey is a
		// separate process on Linux there is no such channel, so the persisted mode is polled instead.
		if (!AppHost.IsScreenshot)
		{
			PollTimer.Interval = TimeSpan.FromMilliseconds(1000.0);
			PollTimer.Tick += (_, _) =>
			{
				if (LoadingTimer.IsEnabled) return;
				int old = powerModeValue;
				var b = DCHU.ReadAppSettings(1, 1, 1);
				if (b[0] != old && b[0] <= 3) ReadPowerFromInsydeBuffer?.Invoke(null, null);
			};
			PollTimer.Start();
		}
	}

	private void WireEvents()
	{
		Opened += (_, _) => Window_ContentRendered(null, null);
		Closing += (_, e) => Window_Closing(null, e);
		button_1.Click += button_1_Click;
		button_2.Click += button_2_Click;
		button_3.Click += button_3_Click;
		button_4.Click += button_4_Click;
		button_5.Click += button_5_Click;
		button_6.Click += button_6_Click;
		button_7.Click += button_7_Click;
		button_8.Click += button_8_Click;
		CB_EnablePgUpDn.Click += CB_EnablePgUpDn_Click;
		BTN_back.Click += BTN_back_Click;
		RB_performance.Click += RB_performance_Click;
		RB_Entertainment.Click += RB_Entertainment_Click;
		RB_powerSaving.Click += RB_powerSaving_Click;
		RB_Quite.Click += RB_Quite_Click;
		CB_CPUDynamic.Click += CB_CPUDynamic_Click;
		Radio_AutoMode.Click += Radio_AutoMode_Click;
		Radio_ManualMode.Click += Radio_ManualMode_Click;
		ComboBox_SetVoltage.SelectionChanged += ComboBox_SetVoltage_SelectionChanged;
		ComboBox_SetFreq.SelectionChanged += ComboBox_SetFreq_SelectionChanged;
		CB_PanelLowRefreshRate.Click += CB_PanelLowRefreshRate_Click;
		CB_TurnOnTruboFan.Click += CB_TurnOnTruboFan_Click;
		RB_performance_newUI.Click += RB_performance_Click;
		CB_CPUDynamic_Copy.Click += CB_CPUDynamic_Click;
		RB_Entertainment_newUI.Click += RB_Entertainment_Click;
		RB_powerSaving_newUI.Click += RB_powerSaving_Click;
		CB_PanelLowRefreshRate_new.Click += CB_PanelLowRefreshRate_Click;
		RB_Quite_newUI.Click += RB_Quite_Click;
		BTN_close.Click += Button_Click_Close;
		BTN_min.Click += Button_Click_Minimize;
		title_move.PointerPressed += title_move_MouseLeftButtonDown;
	}

	// --page NAME for --screenshot: put the window into a specific state.
	private void ApplyScreenshotPage()
	{
		switch (AppHost.PageArg)
		{
		case "fnkey":
		case "oldmodes":
		case "amd":
			UpdateUI();
			break;
		case "dialog":
			UC_DialogBox.Show(0, 12);
			break;
		case "dialog2":
			UC_DialogBox_panel.Show(3, 13);
			break;
		case "dialog3":
			UC_DialogBox_panel.Show(3, 14);
			break;
		}
	}

	private void InitMode()
	{
		int num = 0;
		byte[] array = DCHU.ReadAppSettings(1, 27, 1);
		if (array[0] == 1)
		{
			Global.SupportBIT.MonitorLowRefreshRate = true;
		}
		Grid_NewMode_Quiet.IsVisible = false;
		Grid_NewMode_PowerSaving.IsVisible = false;
		Grid_NewMode_performance.IsVisible = false;
		Grid_NewMode_Entertainment.IsVisible = false;
		if (Global.SupportBIT.PowerModeBit_Quite)
		{
			Grid_NewMode_Quiet.IsVisible = true;
			num++;
		}
		if (Global.SupportBIT.PowerModeBit_PowerSaving)
		{
			Grid_NewMode_PowerSaving.IsVisible = true;
			num++;
			if (Global.SupportBIT.MonitorLowRefreshRate)
			{
				CB_PanelLowRefreshRate_new.IsVisible = true;
			}
		}
		if (Global.SupportBIT.PowerModeBit_Performance)
		{
			Grid_NewMode_performance.IsVisible = true;
			num++;
		}
		if (Global.SupportBIT.PowerModeBit_Entertainment)
		{
			Grid_NewMode_Entertainment.IsVisible = true;
			num++;
		}
		if (num == 0)
		{
			Grid_NewMode.IsVisible = false;
			if (Global.SupportBIT.DTT || Global.SupportBIT.TurboFan || Global.SupportBIT.MonitorLowRefreshRate)
			{
				VB_mode.Height = 550.0;
			}
		}
		else
		{
			Grid_Mode.IsVisible = false;
			Grid_NewMode.IsVisible = true;
			Grid_NewMode.Columns = num;
		}
	}

	private void InitAMD()
	{
		isAMDinitOK = false;
		ReadAMDManualModeFromBIOS();
		SetAMDItemVisibility(isVisible: true);
		ComboBox_SetVoltage.Items.Clear();
		for (int i = 0; i < 1000; i++)
		{
			float num = 1550000 - i * 25000;
			ComboBox_SetVoltage.Items.Add((num / 1000000f).ToString("0.000000"));
			if (num < 1000000f)
			{
				break;
			}
		}
		ComboBox_SetFreq.Items.Clear();
		int num2 = 0;
		while (true)
		{
			int num3 = 600 + num2 * 300;
			if (num3 >= ccx.TurboFrequency)
			{
				break;
			}
			ComboBox_SetFreq.Items.Insert(0, num3.ToString());
			num2++;
		}
		if (!ComboBox_SetFreq.Items.Contains(ccx.TurboFrequency.ToString()))
		{
			ComboBox_SetFreq.Items.Insert(0, ccx.TurboFrequency.ToString());
		}
		if (Frequency_From_BIOS > ccx.TurboFrequency || Frequency_From_BIOS < 600)
		{
			ComboBox_SetFreq.SelectedItem = "3600";
		}
		else
		{
			ComboBox_SetFreq.SelectedItem = Frequency_From_BIOS.ToString();
		}
		if (Voltage_From_BIOS > 1.55f || Voltage_From_BIOS < 0.975f)
		{
			ComboBox_SetVoltage.SelectedItem = "1.150000";
		}
		else
		{
			ComboBox_SetVoltage.SelectedItem = Voltage_From_BIOS.ToString("#0.000000");
		}
		UpdateUI_AMDMode(Mode_From_BIOS);
		isAMDinitOK = true;
	}

	private void SetAMDItemVisibility(bool isVisible)
	{
		if (isVisible)
		{
			VB_mode.Height = 530.0;
			grid_AMD_Mode.IsVisible = true;
		}
		else
		{
			grid_AMD_Mode.IsVisible = false;
		}
	}

	private void DisableTooltipsForEPSON()
	{
		byte[] wMIPackage = DCHU.GetWMIPackage(17);
		CustomID = wMIPackage[2];
		if (CustomID != 5)
		{
			return;
		}
		// WPF: ToolTip style with Visibility=Hidden on every app tile -> tooltips are never shown.
		NoTips = true;
		foreach (var b in _btns) ToolTip.SetTip(b, null);
	}

	private bool NoTips = false;

	private void SetSkinColor()
	{
		byte[] wMIPackage = DCHU.GetWMIPackage(17);
		byte b = wMIPackage[3];
		byte b2 = wMIPackage[4];
		byte b3 = wMIPackage[5];
		var source = Img.Load("Image/title.png");
		if (GetSMBIOS.System.Family.Contains("AORUS"))
		{
			source = Img.Load("Image/title_AORUS.png");
		}
		if (GetSMBIOS.System.ManufacturerName.Contains("MEDION"))
		{
			source = Img.Load("Image/MEDION/title_MEDION2.png");
		}
		if (App.custom_id == 4 && (App.clevo_id == 20723 || App.clevo_id == 20722 || App.clevo_id == 20725 || App.clevo_id == 28914 || App.clevo_id == 28916))
		{
			source = Img.Load("Image/GigaByteTitle.png");
		}
		Title_name.Source = source;
		if (b != 0 || b2 != 0 || b3 != 0)
		{
			CustomSkinColor.SetCustomSkinColor(b, b2, b3);
			Color val = Color.FromArgb(byte.MaxValue, b, b2, b3);
			Brush background = new SolidColorBrush(val);
			BTN_close.Background = background;
		}
	}

	private void Click_Tick(object sender, EventArgs e)
	{
		foreach (var b in _btns) b.IsEnabled = true;
		ClickTimer.Stop();
	}

	private void Loading_Tick(object sender, EventArgs e)
	{
		RB_Entertainment.IsEnabled = true;
		RB_performance.IsEnabled = true;
		RB_powerSaving.IsEnabled = true;
		RB_Quite.IsEnabled = true;
		Grid_NewMode.IsEnabled = true;
		Cursor = null;
		LoadingTimer.Stop();
	}

	private void SetTips(Control src, Control newUi, object tip)
	{
		ToolTip.SetTip(src, tip);
		ToolTip.SetShowDelay(src, 1000);
		if (newUi != null)
		{
			ToolTip.SetTip(newUi, tip);
			ToolTip.SetShowDelay(newUi, 1000);
		}
	}

	private void LoadMultiLang()
	{
		if (GetSMBIOS.System.ManufacturerName.Contains("MEDION"))
		{
			RB_performance.Content = lang.TranslationLanguage("_Performance_MEDION");
		}
		else
		{
			RB_performance.Content = lang.TranslationLanguage("_Performance");
		}
		if (Global.SupportBIT.PowerModeBit && Global.SupportBIT.PowerModeUI_ID == 1)
		{
			RB_Entertainment.Content = lang.TranslationLanguage("_Balanced");
		}
		else
		{
			RB_Entertainment.Content = lang.TranslationLanguage("_Entertainment");
		}
		RB_powerSaving.Content = lang.TranslationLanguage("_PowerSaving");
		RB_Quite.Content = lang.TranslationLanguage("_Quiet");
		labelBack.Content = lang.TranslationLanguage("_Back");
		SetTips(RB_performance, RB_performance_newUI, NewTextBlockForToolTip(lang.TranslationLanguage("_PerformanceSummary"), 300));
		SetTips(RB_Entertainment, RB_Entertainment_newUI, NewTextBlockForToolTip(lang.TranslationLanguage("_EntertainmentSummary"), 300));
		SetTips(RB_powerSaving, RB_powerSaving_newUI, NewTextBlockForToolTip(lang.TranslationLanguage("_PowerSavingSummary"), 0));
		SetTips(RB_Quite, RB_Quite_newUI, NewTextBlockForToolTip(lang.TranslationLanguage("_QuiteSummary"), 300));
		CB_PanelLowRefreshRate.Content = lang.TranslationLanguage("_LowRefreshRate");
		CB_CPUDynamic.Content = lang.TranslationLanguage("_EnableDBCPU");
		Radio_AutoMode.Content = lang.TranslationLanguage("_AutoMode");
		Radio_ManualMode.Content = lang.TranslationLanguage("_ManualMode");
		T_CPUVoltagelimit.Text = lang.TranslationLanguage("_CPUVoltageLimit");
		T_CPUFrequencylimit.Text = lang.TranslationLanguage("_CPUFrequencyLimit");
	}

	private void InitialEventLog()
	{
		DebugLog.OutLogWritten += outlog_EntryWritten;
	}

	private void outlog_EntryWritten(string message)
	{
		var thread = new Thread(DoWork);
		thread.Start(message);
	}

	private void DoWork(object param)
	{
		try
		{
			if (((string)param).ToLower().Contains("clevofnkey^202"))
			{
				ReadPowerFromInsydeBuffer?.Invoke(null, null);
			}
			else if (((string)param).ToLower().Contains("clevofnkey^204"))
			{
				CloseUI?.Invoke(null, null);
			}
			else if (((string)param).ToLower().Contains("clevofnkey^102"))
			{
				MultiLanguageUpdate?.Invoke(null, null);
			}
			else if (((string)param).ToLower().Contains("update") && ((string)param).Contains("cc30"))
			{
				ReloadUI?.Invoke(null, null);
			}
			else if (((string)param).ToLower().Contains("clevofan^113"))
			{
				string[] array = ((string)param).Split(new char[1] { '^' });
				if (array[2] == "6")
				{
					TurboFanDisable?.Invoke(null, null);
				}
				else
				{
					TurboFanEnable?.Invoke(null, null);
				}
			}
			else
			{
				if (!((string)param).ToLower().Contains("arg001"))
				{
					return;
				}
				string[] array2 = ((string)param).Split(new char[1] { '^' });
				if (!(array2[1] == "89") && !(array2[1] == "94") && !(array2[1] == "95") && !(array2[1] == "106"))
				{
					if (array2[1] == "107")
					{
						TurboFanDisable?.Invoke(null, null);
					}
					else if (array2[1] == "112")
					{
						TurboFanEnable?.Invoke(null, null);
					}
					else if (array2[1] == "143")
					{
						TurboFanEnable?.Invoke(null, null);
					}
				}
			}
		}
		catch (Exception ex)
		{
			DebugLog.WriteDbgLog2Outlog("CC3.0, DoWork Error: " + ex.Message);
		}
	}

	private void ReloadUIEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post(() =>
		{
			if (!isFnKeyOn)
			{
				supportArray.Clear();
				_appInfo.Clear();
				App.Init_WMI();
				supportArray = App.SupportArray;
				GetAllAppInfo(supportArray);
				InitAppInfo(_appInfo);
				DebugLog.WriteDbgLog2Outlog("CC30, Reload UI Done.");
			}
		});
	}

	private void UpdateMultiLanguageEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post(() =>
		{
			_appInfo.Clear();
			GetAllAppInfo(supportArray);
			InitAppInfo(_appInfo);
			LoadMultiLang();
		});
	}

	public void ClosingUIEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post(() =>
		{
			SetReadXTUInfo(0);
			SetCC30_APP_Launch(0);
			Environment.Exit(Environment.ExitCode);
		});
	}

	private void TurboFanEnableEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post(() =>
		{
			if (Global.SupportBIT.TurboFan)
			{
				CB_TurnOnTruboFan.IsEnabled = true;
			}
		});
	}

	private void TurboFanDisableEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post(() =>
		{
			CB_TurnOnTruboFan.IsEnabled = false;
		});
	}

	public void ReadPowerModeInsydeBuffer(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post(() =>
		{
			if (Mode_From_BIOS == 1 && IsSupportAMD_Manual_Mode())
			{
				InitAMD();
			}
			else
			{
				ReadPowerModeInsydeBuffer();
				UpdatePowerModeIconPath();
				CB_CPUDynamic.IsVisible = false;
				CB_TurnOnTruboFan.IsVisible = false;
				CB_PanelLowRefreshRate.IsVisible = false;
				switch (powerModeValue)
				{
				case 0:
					RB_Quite.IsChecked = true;
					RB_Quite_newUI.IsChecked = true;
					Update_QuiteMode_UI();
					break;
				case 1:
					RB_powerSaving.IsChecked = true;
					RB_powerSaving_newUI.IsChecked = true;
					Update_PowerSvingMode_UI();
					break;
				case 2:
					RB_performance.IsChecked = true;
					RB_performance_newUI.IsChecked = true;
					Update_PerformanceMode_UI();
					break;
				case 3:
					RB_Entertainment.IsChecked = true;
					RB_Entertainment_newUI.IsChecked = true;
					break;
				default:
					RB_Entertainment.IsChecked = true;
					RB_Entertainment_newUI.IsChecked = true;
					break;
				}
			}
		});
	}

	private (double w, double h) ScreenSize()
	{
		try
		{
			var s = Screens?.Primary;
			if (s != null && !AppHost.IsScreenshot) return (s.Bounds.Width / s.Scaling, s.Bounds.Height / s.Scaling);
		}
		catch { }
		return (1920.0, 1080.0);
	}

	private void CenterWindowOnScreen()
	{
		var (primaryScreenWidth, primaryScreenHeight) = ScreenSize();
		double width = Width;
		double height = Height;
		Position = new PixelPoint((int)(primaryScreenWidth / 2.0 - width / 2.0), (int)(primaryScreenHeight / 2.0 - height / 2.0));
	}

	private void GetAllAppInfo(ArrayList supportArray)
	{
		int num = 0;
		for (int i = 0; i < supportArray.Count; i++)
		{
			if (supportArray[i].Equals(1))
			{
				string[] strAppInfo = new string[5] { "", "", "", "", "" };
				AllApp(i, ref strAppInfo);
				num++;
				AppInfo appInfo = new AppInfo();
				appInfo.AppInfoIndex = num;
				appInfo.AppLabel = strAppInfo[0];
				appInfo.AppIconPath = strAppInfo[1];
				appInfo.AppDescription = strAppInfo[2];
				appInfo.AppURI = strAppInfo[3];
				appInfo.WindowName = strAppInfo[4];
				_appInfo.Add(appInfo);
			}
		}
	}

	private void AllApp(int appArrayPos, ref string[] strAppInfo)
	{
		switch (appArrayPos)
		{
		case 0:
			strAppInfo[0] = "Power Modes";
			strAppInfo[1] = powerModeIconPath;
			strAppInfo[2] = lang.TranslationLanguage("_PowerModesSummary");
			strAppInfo[3] = "clevofnkey:";
			strAppInfo[4] = "";
			break;
		case 1:
			strAppInfo[0] = "Flexikey";
			strAppInfo[1] = "Image/ic_flexikey.png";
			strAppInfo[2] = lang.TranslationLanguage("_FlexikeySummary");
			strAppInfo[3] = "clevoflexikey:";
			strAppInfo[4] = "ControlCenter";
			break;
		case 2:
			strAppInfo[0] = "Flexiaccess";
			strAppInfo[1] = "Image/ic_flexiaccess.png";
			strAppInfo[2] = lang.TranslationLanguage("_FA20Summary");
			strAppInfo[3] = "clevofa:";
			strAppInfo[4] = "FlexiAccess";
			break;
		case 3:
			strAppInfo[0] = "LED Keyboard";
			strAppInfo[1] = "Image/ic_led.png";
			strAppInfo[2] = lang.TranslationLanguage("_LEDKeyboardSummary");
			strAppInfo[3] = "clevoperkey:";
			strAppInfo[4] = "LED Keyboard Setting";
			break;
		case 4:
			strAppInfo[0] = "LED Keyboard";
			strAppInfo[1] = "Image/ic_led.png";
			strAppInfo[2] = lang.TranslationLanguage("_LEDKeyboardSummary");
			strAppInfo[3] = "clevokeyboard:";
			strAppInfo[4] = "LED Keyboard Setting";
			break;
		case 5:
			strAppInfo[0] = "GPU Overclocking";
			strAppInfo[1] = "Image/ic_gpu.png";
			strAppInfo[2] = lang.TranslationLanguage("_GPUSummary");
			strAppInfo[3] = "clevogpu:";
			strAppInfo[4] = "GPU Overclocking";
			break;
		case 6:
			strAppInfo[0] = "CPU Overclocking";
			strAppInfo[1] = "Image/ic_cpu.png";
			strAppInfo[2] = lang.TranslationLanguage("_CPUSummary");
			strAppInfo[3] = "clevocpu:";
			strAppInfo[4] = "CPU OverClocking";
			break;
		case 7:
			strAppInfo[0] = "FAN Speed Control";
			strAppInfo[1] = "Image/ic_fan.png";
			strAppInfo[2] = lang.TranslationLanguage("_FANSummary");
			strAppInfo[3] = "clevofan:";
			strAppInfo[4] = "Fan Speed Setting";
			break;
		case 8:
			strAppInfo[0] = "Energy Save";
			strAppInfo[1] = "Image/ic_energy_save_utility_300x300.png";
			strAppInfo[2] = lang.TranslationLanguage("_EnergySaveSummary");
			strAppInfo[3] = "clevoenergy:";
			strAppInfo[4] = "Energy Save";
			break;
		case 9:
			strAppInfo[0] = "Battery Utility";
			strAppInfo[1] = "Image/ic_battery_utility_300x300.png";
			strAppInfo[2] = lang.TranslationLanguage("_BatteryUtilitySummary");
			strAppInfo[3] = "clevobattery:";
			strAppInfo[4] = "Battery Utility";
			break;
		default:
			strAppInfo = null;
			break;
		}
	}

	private void SetTile(int i, Control val, AppInfo info)
	{
		var b = _btns[i];
		b.IsEnabled = true;
		b.Content = val;
		b.Tag = info.AppLabel;   // WPF: ContentStringFormat carries the label text into the template
		ToolTip.SetTip(b, NoTips ? null : info.AppDescription);
		b.IsVisible = true;
	}

	private void InitAppInfo(List<AppInfo> _appInfo)
	{
		for (int i = 0; i < _appInfo.Count; i++)
		{
			var val = new Image();
			val.Source = Img.Load(_appInfo[i].AppIconPath);
			if (i > 7) break;
			SetTile(i, val, _appInfo[i]);
			switch (i)
			{
			case 0:
				Grid_app.Columns = 1;
				break;
			case 1:
				Grid_app.Columns = 2;
				break;
			case 2:
				if (_appInfo.Count == 3)
				{
					button_3.Margin = new Thickness(5.0, 5.0, -512.0, 5.0);
				}
				break;
			case 4:
				Grid_app.Columns = 3;
				if (_appInfo.Count == 5)
				{
					button_5.Margin = new Thickness(5.0, 5.0, -339.6, 5.0);
				}
				break;
			case 6:
				Grid_app.Columns = 4;
				if (_appInfo.Count == 7)
				{
					button_7.Margin = new Thickness(5.0, 5.0, -253.5, 5.0);
				}
				break;
			}
		}
		if (Global.SupportBIT.UWP_PgUpDnKey)
		{
			G_UpDn_S2.IsVisible = true;
			if (_appInfo.Count == 2)
			{
				G_UpDn_S2.Margin = new Thickness(5.0, 5.0, -512.0, 5.0);
			}
			if (_appInfo.Count == 3)
			{
				button_3.Margin = new Thickness(5.0, 5.0, 5.0, 5.0);
				Grid_app.Columns = 2;
			}
			if (_appInfo.Count == 4)
			{
				Grid_app.Columns = 3;
				button_4.Margin = new Thickness(5.0, 5.0, -169.5, 5.0);
				G_UpDn_S2.Margin = new Thickness(174.5, 5.0, -339.0, 5.0);
			}
			Load_PgUpDnKey();
		}
	}

	private void Button_Click_Close(object sender, RoutedEventArgs e)
	{
		SetReadXTUInfo(0);
		SetCC30_APP_Launch(0);
		Close();
	}

	private void SetReadXTUInfo(int v)
	{
		byte[] bytes = BitConverter.GetBytes(v);
		DCHU.WriteAppSettings(6, 253, 1, bytes);
	}

	private void Button_Click_Minimize(object sender, RoutedEventArgs e)
	{
		WindowState = WindowState.Minimized;
	}

	private void CheckMode(int value)
	{
		switch (value)
		{
		case 0:
			RB_Quite.IsChecked = true;
			RB_Quite_newUI.IsChecked = true;
			Update_QuiteMode_UI();
			break;
		case 1:
			RB_powerSaving.IsChecked = true;
			RB_powerSaving_newUI.IsChecked = true;
			Update_PowerSvingMode_UI();
			break;
		case 2:
			if (Mode_From_BIOS != 1)
			{
				RB_performance.IsChecked = true;
				RB_performance_newUI.IsChecked = true;
				Update_PerformanceMode_UI();
			}
			break;
		case 3:
		default:
			RB_Entertainment.IsChecked = true;
			RB_Entertainment_newUI.IsChecked = true;
			break;
		}
	}

	private void UpdateUI()
	{
		if (!isFnKeyOn)
		{
			Grid_app.IsVisible = false;
			Grid_app.IsEnabled = false;
			Apps_FnKey.IsVisible = true;
			Apps_FnKey.IsEnabled = true;
			CB_CPUDynamic.IsVisible = false;
			CB_TurnOnTruboFan.IsVisible = false;
			ReadPowerModeInsydeBuffer();
			CheckMode(powerModeValue);
			isFnKeyOn = true;
		}
		else
		{
			Grid_app.IsVisible = true;
			Grid_app.IsEnabled = true;
			Apps_FnKey.IsVisible = false;
			Apps_FnKey.IsEnabled = false;
			ReadPowerModeInsydeBuffer();
			UpdatePowerModeIconPath();
			isFnKeyOn = false;
		}
	}

	private void BeginLoading()
	{
		Cursor = new Cursor(StandardCursorType.Wait);
		RB_Entertainment.IsEnabled = false;
		RB_performance.IsEnabled = false;
		RB_powerSaving.IsEnabled = false;
		RB_Quite.IsEnabled = false;
		Grid_NewMode.IsEnabled = false;
		LoadingTimer.Start();
	}

	private void RB_performance_Click(object sender, RoutedEventArgs e)
	{
		if (powerModeValue != 2)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^2");
			BeginLoading();
			CB_PanelLowRefreshRate.IsVisible = false;
		}
	}

	private void RB_Entertainment_Click(object sender, RoutedEventArgs e)
	{
		if (powerModeValue != 3)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^3");
			BeginLoading();
			CB_CPUDynamic.IsVisible = false;
			CB_PanelLowRefreshRate.IsVisible = false;
			CB_TurnOnTruboFan.IsVisible = false;
		}
	}

	private void RB_powerSaving_Click(object sender, RoutedEventArgs e)
	{
		if (powerModeValue != 1)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^1");
			BeginLoading();
			CB_CPUDynamic.IsVisible = false;
			CB_TurnOnTruboFan.IsVisible = false;
			CB_TurnOnTruboFan.IsVisible = false;
			Update_PowerSvingMode_UI();
		}
	}

	private void RB_Quite_Click(object sender, RoutedEventArgs e)
	{
		if (powerModeValue != 0)
		{
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^0");
			BeginLoading();
			CB_CPUDynamic.IsVisible = false;
			CB_PanelLowRefreshRate.IsVisible = false;
			CB_TurnOnTruboFan.IsVisible = false;
			Update_QuiteMode_UI();
		}
	}

	private void ReadPowerModeInsydeBuffer()
	{
		int num = -1;
		int num2 = 1;
		byte[] array = new byte[num2];
		int offset = 1;
		int page = 1;
		int num3 = DCHU.ReadAppSettings(page, offset, num2, ref array[0]);
		num = array[0];
		switch (num)
		{
		case 0:
			powerModeValue = 0;
			powerModeIconPath = "Image/ic_quite.png";
			break;
		case 1:
			powerModeValue = 1;
			powerModeIconPath = "Image/ic_power_saving.png";
			break;
		case 2:
			powerModeValue = 2;
			powerModeIconPath = "Image/ic_performance.png";
			break;
		case 3:
			powerModeValue = 3;
			powerModeIconPath = "Image/ic_entertainment.png";
			break;
		default:
			DebugLog.WriteDbgLog2Outlog("clovocc30, ReadPowerModeInsydeBuffer error, data=" + num);
			powerModeValue = 3;
			powerModeIconPath = "Image/ic_entertainment.png";
			break;
		}
	}

	private void UpdatePowerModeIconPath()
	{
		for (int i = 0; i < _appInfo.Count; i++)
		{
			if (_appInfo[i].AppURI.Contains("clevofnkey"))
			{
				_appInfo[i].AppIconPath = powerModeIconPath;
			}
		}
		InitAppInfo(_appInfo);
	}

	public int SetCC30_APP_Launch(int value)
	{
		int page = 1;
		int offset = 7;
		byte[] bytes = BitConverter.GetBytes(value);
		int length = 1;
		int num = 0;
		do
		{
			DCHU.WriteAppSettings(page, offset, length, bytes);
			byte[] array = DCHU.ReadAppSettings(page, offset, length);
			if (array[0].Equals(bytes[0]))
			{
				return 1;
			}
		}
		while (num++ < 3);
		return 1;
	}

	private void frame_MouseDown(int v)
	{
		int arrayPosition = v - 1;
		if (_appInfo[arrayPosition].AppURI.Contains("clevofnkey"))
		{
			UpdateUI();
		}
		else if (_appInfo[arrayPosition].AppURI.Contains("clevocpu"))
		{
			byte[] buffer = BitConverter.GetBytes(1);
			DCHU.WriteAppSettings(6, 253, 1, buffer);
			LaunchApp("clevocpu");
			ChangeFocus(_appInfo[v - 1].WindowName);
		}
		else
		{
			LaunchApp(_appInfo[arrayPosition].AppURI);
			ChangeFocus(_appInfo[v - 1].WindowName);
		}
	}

	// Original: only sends the launch command to the service (InLog), the service starts the UWP app.
	// Port: same log line, then AppHost.LaunchSibling for the mapped project (skipped when already running).
	private void LaunchApp(string appName)
	{
		Common.MapApp(appName, out var command, out var processName, out var project);
		DebugLog.WriteLog2InLog(command);
		if (project != null && !Common.IsProcessExist(project))
		{
			AppHost.LaunchSibling(project);
		}
	}

	private bool IsProcessExist(string processName) => Common.IsProcessExist(processName);

	private void Window_ContentRendered(object sender, EventArgs e)
	{
		Topmost = false;
	}

	private TextBlock NewTextBlockForToolTip(string text, int width)
	{
		TextBlock val = new TextBlock();
		val.Text = text;
		val.FontSize = 19.0;
		val.FontFamily = new FontFamily("Arial");
		val.TextWrapping = TextWrapping.Wrap;
		val.Foreground = Brushes.Black;
		val.HorizontalAlignment = HorizontalAlignment.Left;
		if (width != 0)
		{
			val.Width = width;
		}
		return val;
	}

	// SetForegroundWindow(FindWindow(null, windowName)) after Sleep(2000): no equivalent (and the Sleep froze the UI).
	private void ChangeFocus(string windowName)
	{
	}

	private void BTN_back_Click(object sender, RoutedEventArgs e)
	{
		UpdateUI();
	}

	private void Window_Closing(object sender, EventArgs e)
	{
		SetReadXTUInfo(0);
		SetCC30_APP_Launch(0);
	}

	private void title_move_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
	}

	private void ClickTile(int n)
	{
		var b = _btns[n - 1];
		b.IsEnabled = false;
		ClickTimer.Start();
		frame_MouseDown(n);
	}

	private void button_1_Click(object sender, RoutedEventArgs e) => ClickTile(1);

	private void button_2_Click(object sender, RoutedEventArgs e) => ClickTile(2);

	private void button_3_Click(object sender, RoutedEventArgs e) => ClickTile(3);

	private void button_4_Click(object sender, RoutedEventArgs e) => ClickTile(4);

	private void button_5_Click(object sender, RoutedEventArgs e) => ClickTile(5);

	private void button_6_Click(object sender, RoutedEventArgs e) => ClickTile(6);

	private void button_7_Click(object sender, RoutedEventArgs e) => ClickTile(7);

	private void button_8_Click(object sender, RoutedEventArgs e) => ClickTile(8);

	private void Update_PerformanceMode_UI()
	{
		CB_PanelLowRefreshRate.IsVisible = false;
		if (Global.SupportBIT.TurboFan)
		{
			byte[] array = DCHU.ReadAppSettings(4, 8, 1);
			byte[] array2 = DCHU.ReadAppSettings(4, 5, 1);
			if (Global.SupportBIT.DTT && !isInstallDTTDriver)
			{
				CB_TurnOnTruboFan.IsVisible = false;
				array[0] = 0;
				DCHU.WriteAppSettings(4, 8, 1, array);
				DCHU.WriteAppSettings(1, 32, 1, array);
				DebugLog.WriteDbgLog2Outlog("clevocc30^112^0");
				DCHU.SetWMI(121, 25, 2u);
				array2[0] = 0;
				DCHU.WriteAppSettings(4, 5, 1, array2);
			}
			else
			{
				CB_TurnOnTruboFan.IsVisible = true;
				if (array2[0] == 6)
				{
					CB_TurnOnTruboFan.IsEnabled = false;
				}
				else
				{
					CB_TurnOnTruboFan.IsEnabled = true;
				}
				CB_TurnOnTruboFan.IsChecked = array[0] == 1;
			}
		}
		else
		{
			if (!Global.SupportBIT.DTT)
			{
				return;
			}
			byte[] array3 = DCHU.ReadAppSettings(1, 32, 1);
			if (isInstallDTTDriver)
			{
				CB_CPUDynamic.IsChecked = array3[0] == 1;
				if (RB_performance.IsChecked == true)
				{
					CB_CPUDynamic.IsVisible = true;
				}
				CB_CPUDynamic_Copy.IsVisible = true;
			}
			else if (array3[0] == 1)
			{
				array3[0] = 0;
				DCHU.WriteAppSettings(1, 32, 1, array3);
			}
		}
	}

	private void Update_PowerSvingMode_UI()
	{
		if (Global.SupportBIT.MonitorLowRefreshRate)
		{
			CB_PanelLowRefreshRate.IsVisible = true;
			CB_PanelLowRefreshRate_new.IsVisible = true;
			byte[] array = DCHU.ReadAppSettings(1, 28, 1);
			CB_PanelLowRefreshRate.IsChecked = array[0] == 1;
		}
	}

	private void Update_QuiteMode_UI()
	{
		if (Global.SupportBIT.MonitorLowRefreshRate && CustomID == 5)
		{
			CB_PanelLowRefreshRate.IsVisible = true;
			byte[] array = DCHU.ReadAppSettings(1, 28, 1);
			CB_PanelLowRefreshRate.IsChecked = array[0] == 1;
		}
	}

	private void CB_CPUDynamic_Click(object sender, RoutedEventArgs e)
	{
		byte[] array = new byte[1];
		if (CB_CPUDynamic.IsChecked == true)
		{
			array[0] = 1;
			DCHU.WriteAppSettings(1, 32, 1, ref array[0]);
			DCHU.SetWMI(121, 25, 130u);
		}
		else
		{
			array[0] = 0;
			DCHU.WriteAppSettings(1, 32, 1, ref array[0]);
			DCHU.SetWMI(121, 25, 2u);
		}
	}

	private void Load_windowStyle()
	{
		var (primaryScreenWidth, primaryScreenHeight) = ScreenSize();
		float num = 1920f / (float)primaryScreenWidth;
		float num2 = 1080f / (float)primaryScreenHeight;
		maxW = Math.Round(_dbRawWidth / (double)num, 0);
		maxH = Math.Round(_dbRawHeight / (double)num2, 0);
		MaxWidth = maxW;
		MaxHeight = maxH;
		VB_main.Width = MaxWidth;
		VB_main.Height = MaxHeight;
		Width = MaxWidth;
		Height = MaxHeight;
	}

	private void Radio_ManualMode_Click(object sender, RoutedEventArgs e)
	{
		if (!grid_manalMode.IsEnabled)
		{
			UC_DialogBox.Show(0, 12);
		}
	}

	private void UC_DialogBox_CancelEvent()
	{
		UpdateUI_AMDMode(0);
		WriteAMDManualModeToBIOS(0);
		UC_DialogBox.IsVisible = false;
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	private void UC_DialogBox_OKEvent()
	{
		Dispatcher.UIThread.Post(() =>
		{
			Console.WriteLine("get ok event");
			UpdateUI_AMDMode(1);
			WriteAMDManualModeToBIOS(1);
			int powerMode = App.GetPowerMode();
			App.SetPreviousPowerMode(powerMode);
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^2");
			Thread.Sleep(500);
			DebugLog.WriteDbgLog2Outlog("clevocc30^10f^1");
			if (IsSupportAMD_Manual_Mode())
			{
				InitAMD();
			}
			UC_DialogBox.IsVisible = false;
			GC.Collect();
			GC.WaitForPendingFinalizers();
		});
	}

	private void UC_DialogBox_panel_CancelEvent()
	{
		Dispatcher.UIThread.Post(() =>
		{
			if (CB_PanelLowRefreshRate.IsChecked == true)
			{
				CB_PanelLowRefreshRate.IsChecked = false;
			}
			else
			{
				CB_PanelLowRefreshRate.IsChecked = true;
			}
			UC_DialogBox_panel.IsVisible = false;
			GC.Collect();
			GC.WaitForPendingFinalizers();
		});
	}

	private void UC_DialogBox_panel_OKEvent()
	{
		Dispatcher.UIThread.Post(() =>
		{
			byte[] array = new byte[1];
			if (CB_PanelLowRefreshRate.IsChecked == true)
			{
				array[0] = 1;
			}
			else
			{
				array[0] = 0;
			}
			DCHU.WriteAppSettings(1, 28, 1, array);
			DebugLog.WriteDbgLog2Outlog("clevocc30^114");
			UC_DialogBox_panel.IsVisible = false;
			GC.Collect();
			GC.WaitForPendingFinalizers();
		});
	}

	private void Radio_AutoMode_Click(object sender, RoutedEventArgs e)
	{
		UpdateUI_AMDMode(0);
		WriteAMDManualModeToBIOS(0);
		DebugLog.WriteDbgLog2Outlog("clevocc30^101^" + App.GetPreviousPowerMode());
		Thread.Sleep(500);
		DebugLog.WriteDbgLog2Outlog("clevocc30^10f^0");
		if (IsSupportAMD_Manual_Mode())
		{
			InitAMD();
		}
	}

	private void Button_SetVoltage_Click(object sender, RoutedEventArgs e)
	{
		WriteAMDManualModeToBIOS(1);
		DebugLog.WriteDbgLog2Outlog("clevocc30^101^2");
	}

	private void Button_SetFreq_Click(object sender, RoutedEventArgs e)
	{
		WriteAMDManualModeToBIOS(1);
		DebugLog.WriteDbgLog2Outlog("clevocc30^101^2");
	}

	// Original: HKLM\HARDWARE\DESCRIPTION\System\CentralProcessor\0 ProcessorNameString. Port: /proc/cpuinfo "model name".
	private static string ProcessorNameString()
	{
		var ov = Environment.GetEnvironmentVariable("CCENTER_CC30_CPU");
		if (ov != null) return ov;
		if (AppHost.PageArg == "amd") return "AMD Ryzen 5 3600 6-Core Processor";
		try
		{
			foreach (var l in File.ReadLines("/proc/cpuinfo"))
				if (l.StartsWith("model name")) return l.Substring(l.IndexOf(':') + 1).Trim();
		}
		catch { }
		return "None";
	}

	private bool IsSupportAMD_Manual_Mode()
	{
		int num = 0;
		string text = ProcessorNameString();
		if (text.Contains("AMD") && Global.SupportBIT.AMD_ManualMode)
		{
			text = text.ToLower();
			if (ccx == null)
			{
				ccx = new AMD_CCX();
				ccx.ProcessorNameString = text;
				if (text.Contains("3100"))
				{
					ccx.TurboFrequency = 3900;
					ccx.BaseFrequency = 3600;
				}
				else if (text.Contains("3300"))
				{
					ccx.TurboFrequency = 4300;
					ccx.BaseFrequency = 3800;
				}
				else if (text.Contains("3600x"))
				{
					ccx.TurboFrequency = 4400;
					ccx.BaseFrequency = 3800;
				}
				else if (text.Contains("3600"))
				{
					ccx.TurboFrequency = 4200;
					ccx.BaseFrequency = 3600;
				}
				else if (text.Contains("3700x"))
				{
					ccx.TurboFrequency = 4400;
					ccx.BaseFrequency = 3600;
				}
				else if (text.Contains("3900"))
				{
					ccx.TurboFrequency = 4300;
					ccx.BaseFrequency = 3100;
				}
				else
				{
					ccx.TurboFrequency = 4300;
					ccx.BaseFrequency = 3100;
					DebugLog.WriteDbgLog2Outlog("AMD Unknow ProcessorNameString= " + text);
				}
			}
			return true;
		}
		return false;
	}

	private void ReadAMDManualModeFromBIOS()
	{
		byte[] array = new byte[256];
		array[0] = 7;
		array[1] = 0;
		byte[] o_buffer = new byte[256];
		DCHU.SetWMIPackageEx(4, array, ref o_buffer);
		Mode_From_BIOS = o_buffer[2];
		Frequency_From_BIOS = AMDConverTable(AMDConvert.IndexToFrequency, o_buffer[3]);
		Voltage_From_BIOS = (float)AMDConverTable(AMDConvert.IndexToVoltage, o_buffer[4]) / 1000f;
		DebugLog.WriteDbgLog2Outlog("CC3.0, Read AMD Manual Mode From BIOS, mode=" + Mode_From_BIOS + ", frequency=" + Frequency_From_BIOS + ", index=" + o_buffer[3] + ", voltage=" + Voltage_From_BIOS + ", index=" + o_buffer[4]);
	}

	private void WriteAMDManualModeToBIOS(int mode)
	{
		float num = Convert.ToSingle(ComboBox_SetVoltage.SelectedItem);
		int input = (int)Convert.ToUInt32(ComboBox_SetFreq.SelectedItem);
		byte[] array = new byte[256];
		array[0] = 7;
		array[1] = 1;
		array[2] = Convert.ToByte(mode);
		int value = AMDConverTable(AMDConvert.FrequencyToIndex, input);
		array[3] = Convert.ToByte(value);
		int value2 = AMDConverTable(AMDConvert.VoltageToIndex, (int)(num * 1000f));
		array[4] = Convert.ToByte(value2);
		byte[] o_buffer = new byte[256];
		DCHU.SetWMIPackageEx(4, array, ref o_buffer);
		DebugLog.WriteDbgLog2Outlog("CC3.0, Write AMD Manual Mode To BIOS, mode=" + mode + ", frequency=" + input + ", index=" + value + ", voltage=" + num + ", index=" + value2);
	}

	private int AMDConverTable(AMDConvert action, int input)
	{
		int num = 0;
		switch (action)
		{
		case AMDConvert.FrequencyToIndex:
			num = ((input % 300 != 0) ? ((input - 600) / 300 + 2) : ((input - 600) / 300 + 1));
			break;
		case AMDConvert.IndexToFrequency:
			num = 300 * (input - 1) + 600;
			if (num > ccx.TurboFrequency)
			{
				num = ccx.TurboFrequency;
			}
			break;
		case AMDConvert.VoltageToIndex:
			num = (input - 975) / 25 + 1;
			break;
		case AMDConvert.IndexToVoltage:
			num = 25 * (input - 1) + 975;
			break;
		default:
			DebugLog.WriteDbgLog2Outlog("CC3.0, AMDConverTable, error action=" + action);
			break;
		}
		return num;
	}

	private void UpdateUI_AMDMode(int mode)
	{
		if (mode == 1)
		{
			Radio_ManualMode.IsChecked = true;
			Radio_AutoMode.IsChecked = false;
			VB_mode.Opacity = 0.5;
			VB_mode.IsEnabled = false;
			grid_manalMode.Opacity = 1.0;
			grid_manalMode.IsEnabled = true;
			RB_performance.IsChecked = false;
			RB_Entertainment.IsChecked = false;
			RB_powerSaving.IsChecked = false;
			RB_Quite.IsChecked = false;
		}
		else
		{
			Radio_ManualMode.IsChecked = false;
			Radio_AutoMode.IsChecked = true;
			VB_mode.Opacity = 1.0;
			VB_mode.IsEnabled = true;
			grid_manalMode.Opacity = 0.5;
			grid_manalMode.IsEnabled = false;
		}
	}

	private void ComboBox_SetVoltage_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (isAMDinitOK)
		{
			WriteAMDManualModeToBIOS(1);
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^2");
		}
	}

	private void ComboBox_SetFreq_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (isAMDinitOK)
		{
			WriteAMDManualModeToBIOS(1);
			DebugLog.WriteDbgLog2Outlog("clevocc30^101^2");
		}
	}

	public void SetFanMode(byte mode)
	{
		DCHU.SetWMI(121, 1, mode);
		if (mode == 7)
		{
			DCHU.WriteAppSettings(4, 8, 1, new byte[1] { 1 });
			return;
		}
		DCHU.WriteAppSettings(4, 5, 1, new byte[1] { mode });
		DCHU.WriteAppSettings(4, 8, 1, new byte[1] { 0 });
	}

	private void CB_PanelLowRefreshRate_Click(object sender, RoutedEventArgs e)
	{
		byte[] array = new byte[1];
		bool on = ((sender as ToggleButton)?.IsChecked) == true;
		if (sender == CB_PanelLowRefreshRate_new) { CB_PanelLowRefreshRate.IsChecked = on; }
		if (CB_PanelLowRefreshRate.IsChecked == true)
		{
			array[0] = 1;
			UC_DialogBox_panel.Show(3, 13);
		}
		else
		{
			array[0] = 0;
			UC_DialogBox_panel.Show(3, 14);
		}
	}

	private void CB_TurnOnTruboFan_Click(object sender, RoutedEventArgs e)
	{
		byte[] array = new byte[1];
		byte[] array2 = new byte[1];
		if (CB_TurnOnTruboFan.IsChecked == true)
		{
			array[0] = 1;
			if (Global.SupportBIT.DTT)
			{
				array2[0] = 1;
			}
			DebugLog.WriteDbgLog2Outlog("clevocc30^112^1");
		}
		else
		{
			array[0] = 0;
			array2[0] = 0;
			DebugLog.WriteDbgLog2Outlog("clevocc30^112^0");
		}
		DCHU.WriteAppSettings(4, 8, 1, array);
		DCHU.WriteAppSettings(1, 32, 1, array2);
		int data = 2 | (array[0] << 6) | (array2[0] << 7);
		DCHU.SetWMI(121, 25, (uint)data);
		DCHU.WriteAppSettings(4, 5, 1, new byte[1] { 0 });
	}

	private void CB_EnablePgUpDn_Click(object sender, RoutedEventArgs e)
	{
		byte[] array = new byte[1];
		if (CB_EnablePgUpDn.IsChecked == true)
		{
			I_PgUp.Opacity = 1.0;
			array[0] = 0;
		}
		else
		{
			I_PgUp.Opacity = 0.2;
			array[0] = 1;
		}
		DCHU.WriteAppSettings(1, 33, 1, array);
		if (Global.SupportBIT.SupportBIOSFeatureList_Version == 0)
		{
			DCHU.SetWMI(121, 45, array[0]);
			return;
		}
		byte[] array2 = new byte[256];
		byte[] o_buffer = new byte[256];
		array2[0] = 26;
		array2[1] = array[0];
		DCHU.SetWMIPackageEx(4, array2, ref o_buffer);
	}

	private void Load_PgUpDnKey()
	{
		byte[] array = new byte[1];
		array = DCHU.ReadAppSettings(1, 33, 1);
		if (array[0] == 1)
		{
			CB_EnablePgUpDn.IsChecked = false;
			I_PgUp.Opacity = 0.2;
		}
		else
		{
			CB_EnablePgUpDn.IsChecked = true;
			I_PgUp.Opacity = 1.0;
		}
		if (Global.SupportBIT.SupportBIOSFeatureList_Version == 0)
		{
			DCHU.SetWMI(121, 45, array[0]);
			return;
		}
		byte[] array2 = new byte[256];
		byte[] o_buffer = new byte[256];
		array2[0] = 26;
		array2[1] = array[0];
		DCHU.SetWMIPackageEx(4, array2, ref o_buffer);
	}
}
