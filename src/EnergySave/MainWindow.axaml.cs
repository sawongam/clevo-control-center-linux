using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CCenter.Platform;

namespace DesktopBridge_EnergySave;

public partial class MainWindow : Window
{
	private bool isWindowsFullscreen = false;
	private double maxW = 1920.0;
	private double maxH = 1080.0;
	/// <summary>WPF Window.Width/Height dependency properties (declared 1920x1080, only changed by a user resize).</summary>
	private double declW = 1920.0;
	private double declH = 1080.0;
	private float rateW;
	private float rateH;
	private static int nPowerPlanPage = 0;
	private static int nEnergySavePage = 1;
	private bool isUtilityStart = false;
	private int nPageSelect = 0;
	private IniFile ini;
	private DispatcherTimer dispatcherTimer;
	private DispatcherTimer SwitchTimer;
	private PowerTable dt = new PowerTable();
	private language lang = language.Instance;
	private bool isAC = true;
	private int WattLimit = 100;
	private double iAdapterV = 19.2;
	private int iPowerR = 10;
	private bool isSecond = true;
	private bool isLock = true;
	private bool isEveryday = false;
	private bool isSundayCheck = false;
	private bool isMondayCheck = false;
	private bool isTuesdayCheck = false;
	private bool isWednesdayCheck = false;
	private bool isThursdayCheck = false;
	private bool isFridayCheck = false;
	private bool isSaturdayCheck = false;
	private int EnableDayCount = 0;
	private bool isAuto = false;
	private bool isChargePressed = false;
	private bool isDischargePressed = false;
	private int nChargeLimit = 100;
	private int nDischargeLimit = 40;
	private ArrayList itemsList = new ArrayList();
	private List<EnergySaveInfo> SaveModeList = new List<EnergySaveInfo>();
	private int SelectItem = 0;
	private int RenameItem = 0;
	private int SaveModeCount = 0;
	private List<int> cmd = new List<int>();
	private DispatcherTimer BatteryTimer = new DispatcherTimer();
	private int nStopChargeLimitFromWMI_17 = 0;
	private int nStopDischargeLimitFromWMI_17 = 0;
	public int ClevoID = 0;
	public int L140CU = 5121;
	private decimal[] intArr = new decimal[5];
	private int intArrCount = 0;
	private Point point;
	private bool _isPressed = false;
	private bool _isPressed_2 = false;
	private double StartValue = 0.0;
	private double EndValue = 0.0;
	private bool _isPressed_3 = false;
	private bool _isPressed_4 = false;
	private double StartValue_2 = 0.0;
	private double EndValue_2 = 0.0;
	private bool fakeData = false;
	private int lastLineStatus = -1;

	/// <summary>GetProductdll.dll (PCI product id) has no Linux equivalent: 0 = unknown model (stub).</summary>
	public static int GetProductID_PCI() => 0;

	public MainWindow()
	{
		int li = Array.IndexOf(AppHost.Args, "--lang");
		if (li >= 0 && li + 1 < AppHost.Args.Length)
		{
			foreach (Clevo.CleDef.UI_Language l in Enum.GetValues(typeof(Clevo.CleDef.UI_Language)))
				if (language.Code(l) == AppHost.Args[li + 1] && l != Clevo.CleDef.UI_Language.system) { language.OverrideLanguage = (int)l; break; }
		}
		if (!AppHost.IsScreenshot) CheckAppExistOrNot();
		InitializeComponent();
		MessageBox.Owner = this;
		WindowFitSize();
		DCHU.GetWMI(70);
		// The mock backend answers 0 for this query; on real hardware the original check is kept.
		if (!Backend.IsMock && (DCHU.GetWMI(16) & 0x100) != 256)
		{
			MessageBox.Show("This UWP app is not suitable for this model.");
			Environment.Exit(Environment.ExitCode);
			return;
		}
		ClevoID = GetProductIDFromInsydeBuffer();
		WattLimit = GetWattLimit();
		GetChargeLimitFromWMI_17();
		Init_INIFile();
		InitEvents();
		Opened += Window_Loaded;
		SizeChanged += mainWindow_SizeChanged;
	}

	private void CheckAppExistOrNot()
	{
		string processName = Process.GetCurrentProcess().ProcessName;
		if (Process.GetProcessesByName(processName).GetUpperBound(0) > 0)
		{
			Environment.Exit(Environment.ExitCode);
		}
	}

	private int GetProductIDFromInsydeBuffer()
	{
		int page = 0;
		int offset = 0;
		int length = 2;
		int result = 0;
		try
		{
			byte[] array = DCHU.ReadAppSettings(page, offset, length);
			byte b = array[0];
			array[0] = array[1];
			array[1] = b;
			result = BitConverter.ToInt16(array, 0);
			if (ClevoID == 0)
			{
				ClevoID = GetProductID_PCI();
			}
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("EnergySave GetProductIDFromInsydeBuffer() Error, ProductID=" + result);
		}
		return result;
	}

	// ---- event wiring (IComponentConnector.Connect in the original) ----
	static void Down(InputElement el, Action<object, PointerPressedEventArgs> h) => el.PointerPressed += (s, e) => { if (e.GetCurrentPoint(el).Properties.IsLeftButtonPressed) h(s, e); };
	static void Up(InputElement el, Action<object, PointerReleasedEventArgs> h) => el.PointerReleased += (s, e) => { if (e.InitialPressMouseButton == MouseButton.Left) h(s, e); };
	static void Enter(InputElement el, Action<object, PointerEventArgs> h) => el.PointerEntered += (s, e) => h(s, e);
	static void Leave(InputElement el, Action<object, PointerEventArgs> h) => el.PointerExited += (s, e) => h(s, e);

	private void InitEvents()
	{
		mainGrid.PointerReleased += (s, e) => Grid_MouseLeftButtonUp();
		Apps_main.PointerPressed += (s, e) => { if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) Apps_main_MouseLeftButtonDown(s, e); };
		Apps_main.PointerMoved += (s, e) => { Apps_main_MouseMove(s, e); KnobsPointerMoved(e); };
		btnClose.Click += Button_Click_Close;
		btnWindow.Click += Button_Click_Window;
		btnMinimize.Click += Button_Click_Minimize;
		foreach (InputElement el in new InputElement[] { Image_PowerPlan, Image_PowerPlan_icon, Image_PowerPlan_dotted_line, Label_PowerPlan_Title, Label_PowerPlan_Summary })
		{
			Enter(el, Image_PowerPlan_MouseEnter); Leave(el, Image_PowerPlan_MouseLeave);
			Down(el, Image_PowerPlan_MouseLeftButtonDown); Up(el, Image_PowerPlan_MouseLeftButtonUp);
		}
		foreach (InputElement el in new InputElement[] { Image_EnergySave, Image_EnergySave_icon, Image_EnergySave_dotted_line, Label_EnergySave_Title, Label_EnergySave_Summary_left, Label_EnergySave_Summary_center, Label_EnergySave_Summary_right })
		{
			Enter(el, Image_EnergySave_MouseEnter); Leave(el, Image_EnergySave_MouseLeave);
			Down(el, Image_EnergySave_MouseLeftButtonDown); Up(el, Image_EnergySave_MouseLeftButtonUp);
		}
		Enter(Image_StartUtility, Image_StartUtility_MouseEnter); Leave(Image_StartUtility, Image_StartUtility_MouseLeave);
		Down(Image_StartUtility, Image_StartUtility_MouseLeftButtonDown); Up(Image_StartUtility, Image_StartUtility_MouseLeftButtonUp);
		Down(Image_Minute, Image_Minute_MouseLeftButtonDown);
		Down(Image_Second, Image_Second_MouseLeftButtonDown);
		Down(ZoomIn, ZoomIn_Click);
		Down(ZoomOut, ZoomOut_Click);
		Down(Image_EnergySave_lock, Image_EnergySave_lock_MouseLeftButtonDown);
		Enter(Image_EnergySave_lock, Image_EnergySave_lock_MouseEnter); Leave(Image_EnergySave_lock, Image_EnergySave_lock_MouseLeave);
		Down(Image_EnergySave_Everyday_Check, Image_EnergySave_Everyday_Check_MouseLeftButtonDown);
		Down(Image_EnergySave_Week_Check, Image_EnergySave_Week_Check_MouseLeftButtonDown);
		Down(Label_EnergySave_Sunday, Label_EnergySave_Sunday_MouseLeftButtonDown);
		Down(Label_EnergySave_Monday, Label_EnergySave_Monday_MouseLeftButtonDown);
		Down(Label_EnergySave_Tuesday, Label_EnergySave_Tuesday_MouseLeftButtonDown);
		Down(Label_EnergySave_Wednesday, Label_EnergySave_Wednesday_MouseLeftButtonDown);
		Down(Label_EnergySave_Thursday, Label_EnergySave_Thursday_MouseLeftButtonDown);
		Down(Label_EnergySave_Friday, Label_EnergySave_Friday_MouseLeftButtonDown);
		Down(Label_EnergySave_Saturday, Label_EnergySave_Saturday_MouseLeftButtonDown);
		Down(Image_EnergySave_save, Image_EnergySave_save_MouseLeftButtonDown);
		Down(Image_EnergySave_rename, Image_EnergySave_rename_MouseLeftButtonDown);
		Down(Image_EnergySave_add, Image_EnergySave_add_MouseLeftButtonDown);
		Down(Image_EnergySave_delete, Image_EnergySave_delete_MouseLeftButtonDown);
		ListBox_Save_Mode.SelectionChanged += ListBox_Save_Mode_SelectionChanged;
		Down(Image_EnergySave_auto, Image_EnergySave_auto_MouseLeftButtonDown);
		Down(Image_EnergySave_manually, Image_EnergySave_manually_MouseLeftButtonDown);
		Down(Image_EnergySave_charge, Image_EnergySave_charge_MouseLeftButtonDown); Up(Image_EnergySave_charge, Image_EnergySave_charge_MouseLeftButtonUp);
		Down(Image_EnergySave_discharge, Image_EnergySave_discharge_MouseLeftButtonDown); Up(Image_EnergySave_discharge, Image_EnergySave_discharge_MouseLeftButtonUp);
		Down(Ellipse_schedule1, Ellipse_schedule1_MouseLeftButtonDown);
		Down(Ellipse_schedule2, Ellipse_schedule2_MouseLeftButtonDown);
		Btn_Rename.Click += Yes_Click;
		Btn_Cancel.Click += No_Click;
		KeyDown += (s, e) => { if (e.Key == Key.Escape && Btn_Cancel.IsVisible) No_Click(s, e); };   // IsCancel="True"
		// thumbs: press on S/E thumb ellipse or its letter
		foreach (Control c in new Control[] { knob.ThumbEllipse, knob.ThumbText }) Down(c, (s, e) => { _isPressed = true; _isPressed_2 = false; });
		foreach (Control c in new Control[] { knob2.ThumbEllipse, knob2.ThumbText }) Down(c, (s, e) => { _isPressed = false; _isPressed_2 = true; });
		foreach (Control c in new Control[] { knob3.ThumbEllipse, knob3.ThumbText }) Down(c, (s, e) => { _isPressed_3 = true; _isPressed_4 = false; });
		foreach (Control c in new Control[] { knob4.ThumbEllipse, knob4.ThumbText }) Down(c, (s, e) => { _isPressed_3 = false; _isPressed_4 = true; });
		foreach (Control c in new Control[] { knob.ThumbEllipse, knob.ThumbText, knob2.ThumbEllipse, knob2.ThumbText, knob3.ThumbEllipse, knob3.ThumbText, knob4.ThumbEllipse, knob4.ThumbText })
			Up(c, (s, e) => { _isPressed = _isPressed_2 = _isPressed_3 = _isPressed_4 = false; });
	}

	private void Grid_MouseLeftButtonUp()
	{
		_isPressed = false;
		_isPressed_2 = false;
		_isPressed_3 = false;
		_isPressed_4 = false;
		isChargePressed = false;
		isDischargePressed = false;
	}

	/// <summary>
	/// In WPF the knob ring (a transparent Ellipse with a 150px stroke, r = 75..225) raised MouseMove; the schedule
	/// toggle ellipses (r &lt; 97.5) and the thumbs sit above it. Reproduced geometrically on the canvas move event.
	/// </summary>
	private void KnobsPointerMoved(PointerEventArgs e)
	{
		if (!Canvas_EnergySave.IsVisible || KnobBase.IsThumb(e.Source)) return;
		if (InRing(knob3, e)) { KnobMove2(e); return; }
		if (InRing(knob, e)) KnobMove1(e);
	}

	private static bool InRing(KnobBase k, PointerEventArgs e)
	{
		var p = e.GetPosition(k.Inner);
		double r = Math.Sqrt((p.X - 150) * (p.X - 150) + (p.Y - 150) * (p.Y - 150));
		return r >= 97.5 && r <= 225;
	}

	private void Window_Loaded(object sender, EventArgs e)
	{
		SwitchTimer = new DispatcherTimer();
		SwitchTimer.Tick += Switch_Tick;
		SwitchTimer.Interval = new TimeSpan(0, 0, 1);
		dispatcherTimer = new DispatcherTimer();
		dispatcherTimer.Tick += dispatcherTimer_Tick;
		dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
		dispatcherTimer.Start();
		SetDataTable();
		SetChart();
		mainChart.DataBind(dt);
		GetSaveModeCountFromIni();
		if (SaveModeCount > 0)
		{
			GetSaveModeListFromIni();
			GetEnergySaveStatus();
		}
		else
		{
			AddNew2SaveModeList("Default");
			SelectItem = 0;
			isUtilityStart = false;
		}
		GetSelectProfile();
		SetSaveModeListBox();
		UpdateEnergySaveUI();
		setSaveModeSelect(SelectItem);
		// SystemEvents.PowerModeChanged is emulated in Battery_Tick (AC state polling)
		if (PowerStatusAdapter.PowerLineStatus == 1) isAC = true;
		else if (PowerStatusAdapter.PowerLineStatus == 0) isAC = false;
		lastLineStatus = PowerStatusAdapter.PowerLineStatus;
		UpdateBatteryUI();
		BatteryTimer.Interval = TimeSpan.FromMilliseconds(1000);
		BatteryTimer.Tick += Battery_Tick;
		BatteryTimer.Start();
		if (AppHost.IsScreenshot && AppHost.PageArg != null) DispatcherTimer.RunOnce(ApplyPageArg, TimeSpan.FromMilliseconds(100));
	}

	private void UpdateBatteryUI()
	{
		int num = (int)(PowerStatusAdapter.BatteryLifePercent * 100f);
		Label_EnergySave_battery_power.Content = num + "%";
		if (num < 10)
		{
			Image_EnergySave_battery_cabacity.Height = 25.0;
			Canvas.SetTop(Image_EnergySave_battery_cabacity, 370.0);
		}
		else if (num == 100)
		{
			Image_EnergySave_battery_cabacity.Height = 304.0;
			Canvas.SetTop(Image_EnergySave_battery_cabacity, 100.0);
		}
		else
		{
			Image_EnergySave_battery_cabacity.Height = 25 + 31 * (num / 10 - 1);
			Canvas.SetTop(Image_EnergySave_battery_cabacity, 370 - 30 * (num / 10 - 1));
		}
	}

	private void Battery_Tick(object sender, EventArgs e)
	{
		UpdateBatteryUI();
		int ls = PowerStatusAdapter.PowerLineStatus;
		if (ls != lastLineStatus)   // SystemEvents_PowerModeChanged (StatusChange)
		{
			lastLineStatus = ls;
			if (ls == 1) isAC = true;
			else if (ls == 0) isAC = false;
		}
	}

	private void SetDataTable()
	{
		for (int i = 0; i < 60; i++)
		{
			PowerRow dataRow = dt.NewRow();
			dataRow["AC"] = 0;
			dt.Rows.Add(dataRow);
		}
	}

	private void SetChart()
	{
		mainChart.YMaximum = WattLimit;
	}

	private void mainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (WindowState == WindowState.Normal && !_sizing) { declW = e.NewSize.Width; declH = e.NewSize.Height; }
	}
	private bool _sizing = false;

	private void Apps_main_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		point = e.GetPosition(this);
		if (!isWindowsFullscreen)
		{
			double num = declW / 1920.0;
			double num2 = declH / 1080.0;
			if (point.Y < 81.0 * num2 && point.X > 506.0 * num && point.X < 1414.0 * num)
			{
				BeginMoveDrag(e);
			}
		}
	}

	private void Button_Click_Window(object sender, RoutedEventArgs e)
	{
		if (isWindowsFullscreen)
		{
			isWindowsFullscreen = false;
			WindowFitSize();
			btnWindow.Tag = Img.Load("Assets/ic_fullscreen.png");
		}
		else
		{
			isWindowsFullscreen = true;
			WindowFullSrceen();
			btnWindow.Tag = Img.Load("Assets/ic_window.png");
		}
	}

	private void WindowFitSize()
	{
		double scaling = 1.0, primaryScreenWidth = 1920, primaryScreenHeight = 1080;
		var scr = Screens?.Primary;
		if (scr != null) { scaling = scr.Scaling; primaryScreenWidth = scr.Bounds.Width / scaling; primaryScreenHeight = scr.Bounds.Height / scaling; }
		rateW = 2560f / (float)primaryScreenWidth;
		rateH = 1440f / (float)primaryScreenHeight;
		maxW = Math.Round(1920.0 / (double)rateW, 0);
		maxH = Math.Round(1080.0 / (double)rateH, 0);
		_sizing = true;
		this.WindowState = WindowState.Normal;
		this.MaxWidth = maxW + 20.0;
		this.MaxHeight = maxH + 18.0;
		this.MinWidth = maxW / 2.0;
		this.MinHeight = maxH / 2.0;
		mainViewbox.Width = maxW;
		mainViewbox.Height = maxH;
		double left = primaryScreenWidth / 2.0 - maxW / 2.0;
		double top = primaryScreenHeight / 2.0 - maxH / 2.0;
		if (left < 0.0) left = 0.0;
		if (top < 0.0) top = 0.0;
		WindowStartupLocation = WindowStartupLocation.Manual;
		Position = new PixelPoint((int)(left * scaling), (int)(top * scaling));
		this.Topmost = false;
		_sizing = false;
	}

	private void WindowFullSrceen()
	{
		_sizing = true;
		this.WindowState = WindowState.Normal;
		this.MaxWidth = double.PositiveInfinity;
		this.MaxHeight = double.PositiveInfinity;
		this.WindowState = WindowState.Maximized;
		mainViewbox.Width = declW;
		mainViewbox.Height = declH;
		_sizing = false;
	}

	private void GetChargeLimitFromWMI_17()
	{
		byte[] wMIPackage = DCHU.GetWMIPackage(17);
		nStopChargeLimitFromWMI_17 = wMIPackage[208];
		nStopDischargeLimitFromWMI_17 = wMIPackage[209];
		DebugLog.WriteDbgLog2Outlog("EnergySave GetChargeLimitFromWMI_17(), StopChargeLimit(0xD0)=" + nStopChargeLimitFromWMI_17 + ", StopDischargeLimit(0xD1)=" + nStopDischargeLimitFromWMI_17);
	}

	private void UpdateEnergySaveUI()
	{
		Label_PowerPlan_Title.Content = lang.TranslationLanguage("_PowerPlanConsumption");
		Label_EnergySave_Title.Content = lang.TranslationLanguage("_EnergySaveTitle");
		if (isSecond)
		{
			Label_Legend.Content = lang.TranslationLanguage("_PowerUsageSec");
		}
		else
		{
			Label_Legend.Content = lang.TranslationLanguage("_PowerUsageMin");
		}
		Label_Minute.Content = lang.TranslationLanguage("_Minute");
		Label_Second.Content = lang.TranslationLanguage("_Second");
		Label_Chart.Content = lang.TranslationLanguage("_PowerConsumptionMonitor");
		Label_AC.Content = lang.TranslationLanguage("_AC");
		Label_DC.Content = lang.TranslationLanguage("_DC");
		Label_EnergySave_Description.Text = lang.TranslationLanguage("_Description");
		if (isLock)
		{
			Label_EnergySave_lock.Content = lang.TranslationLanguage("_UnlockToModify");
		}
		else
		{
			Label_EnergySave_lock.Content = lang.TranslationLanguage("_LockToPreventChanges");
		}
		Label_EnergySave_Discharge.Content = lang.TranslationLanguage("_Schedule") + ": ";
		Label_EnergySave_Everyday.Content = lang.TranslationLanguage("_Everyday");
		Label_EnergySave_Week.Content = lang.TranslationLanguage("_WeekSpecified");
		Label_EnergySave_Sunday.Content = lang.TranslationLanguage("_Sun");
		Label_EnergySave_Monday.Content = lang.TranslationLanguage("_Mon");
		Label_EnergySave_Tuesday.Content = lang.TranslationLanguage("_Tue");
		Label_EnergySave_Wednesday.Content = lang.TranslationLanguage("_Wed");
		Label_EnergySave_Thursday.Content = lang.TranslationLanguage("_Thu");
		Label_EnergySave_Friday.Content = lang.TranslationLanguage("_Fri");
		Label_EnergySave_Saturday.Content = lang.TranslationLanguage("_Sat");
		Label_EnergySave_save.Content = lang.TranslationLanguage("_Save");
		Label_EnergySave_save_mode.Content = lang.TranslationLanguage("_ChooseAnEnergySaveProfile");
		Label_EnergySave_Warning.Content = lang.TranslationLanguage("_EnergySaveWarning");
		Label_EnergySave_battery_setting.Content = lang.TranslationLanguage("_BatterySetting");
		Label_EnergySave_auto.Content = lang.TranslationLanguage("_Auto");
		Label_EnergySave_manually.Content = lang.TranslationLanguage("_Manually");
		Btn_Rename.Content = lang.TranslationLanguage("_Rename");
		Btn_Cancel.Content = lang.TranslationLanguage("_Cancel");
		LoadPowerPlanScheme();
		SetSchedule1FromList(SaveModeList[SelectItem].Schedule1_Start, SaveModeList[SelectItem].Schedule1_End);
		SetSchedule2FromList(SaveModeList[SelectItem].Schedule2_Start, SaveModeList[SelectItem].Schedule2_End);
		SetScheduleOpacity();
		SetDischargeRoutine();
		SetBatterySetting();
		SetUtilityStart();
		SetEnergySaveSummary();
	}

	private void SetEnergySaveSummary()
	{
		string text = "";
		string text2 = "";
		string text3 = "";
		text = text + lang.TranslationLanguage("_ModeName") + ": " + SaveModeList[SelectItem].Name;
		if (SaveModeList[SelectItem].isSchedule1_Enable)
		{
			text += "\r\n";
			int schedule1_Start = SaveModeList[SelectItem].Schedule1_Start;
			int schedule1_End = SaveModeList[SelectItem].Schedule1_End;
			text = text + lang.TranslationLanguage("_Schedule1") + Hour24ConvertToHour12(schedule1_Start).ToString() + " - " + Hour24ConvertToHour12(schedule1_End).ToString();
		}
		if (SaveModeList[SelectItem].isSchedule2_Enable)
		{
			text += "\r\n";
			int schedule2_Start = SaveModeList[SelectItem].Schedule2_Start;
			int schedule2_End = SaveModeList[SelectItem].Schedule2_End;
			text = text + lang.TranslationLanguage("_Schedule2") + Hour24ConvertToHour12(schedule2_Start).ToString() + " - " + Hour24ConvertToHour12(schedule2_End).ToString();
		}
				Label_EnergySave_Summary_left.Content = text;
		int num = 0;
		bool flag = false;
		bool flag2 = false;
		if (SaveModeList[SelectItem].isEveryday)
		{
			text2 = text2 + lang.TranslationLanguage("_EnableDays") + lang.TranslationLanguage("_Everyday");
		}
		else
		{
			text2 += lang.TranslationLanguage("_EnableDays");
			if (SaveModeList[SelectItem].isSunday)
			{
				num++;
				text2 += lang.TranslationLanguage("_Sun");
				flag2 = true;
			}
			if (SaveModeList[SelectItem].isMonday)
			{
				num++;
				if (flag2)
				{
					text2 += ", ";
				}
				else
				{
					flag2 = true;
				}
				text2 += lang.TranslationLanguage("_Mon");
			}
			if (SaveModeList[SelectItem].isTuesday)
			{
				num++;
				if (flag2)
				{
					text2 += ", ";
				}
				else
				{
					flag2 = true;
				}
				text2 += lang.TranslationLanguage("_Tue");
			}
			if (SaveModeList[SelectItem].isWednesday)
			{
				if (num > 2 && !flag)
				{
					num = 0;
					text2 += "\r\n";
					flag = true;
				}
				if (flag2)
				{
					text2 += ", ";
				}
				else
				{
					flag2 = true;
				}
				text2 += lang.TranslationLanguage("_Wed");
				num++;
			}
			if (SaveModeList[SelectItem].isThursday)
			{
				if (num > 2 && !flag)
				{
					num = 0;
					text2 += "\r\n";
					flag = true;
				}
				if (flag2)
				{
					text2 += ", ";
				}
				else
				{
					flag2 = true;
				}
				text2 += lang.TranslationLanguage("_Thu");
				num++;
			}
			if (SaveModeList[SelectItem].isFriday)
			{
				if (num > 2 && !flag)
				{
					num = 0;
					text2 += "\r\n";
					flag = true;
				}
				if (flag2)
				{
					text2 += ", ";
				}
				else
				{
					flag2 = true;
				}
				text2 += lang.TranslationLanguage("_Fri");
				num++;
			}
			if (SaveModeList[SelectItem].isSaturday)
			{
				if (num > 2 && !flag)
				{
					num = 0;
					text2 += "\r\n";
					flag = true;
				}
				if (flag2)
				{
					text2 += ", ";
				}
				else
				{
					flag2 = true;
				}
				text2 += lang.TranslationLanguage("_Sat");
				num++;
			}
		}
		if (SaveModeList[SelectItem].isBatterySettingAuto)
		{
			text2 = ((nStopChargeLimitFromWMI_17 == 0 || nStopDischargeLimitFromWMI_17 == 0) ? (text2 + "\r\n" + lang.TranslationLanguage("_DischargeLimit") + "：95%") : (text2 + "\r\n" + lang.TranslationLanguage("_DischargeLimit") + "：" + nStopDischargeLimitFromWMI_17 + "%"));
		}
		else
		{
			text2 = text2 + "\r\n" + lang.TranslationLanguage("_DischargeLimit") + "：" + SaveModeList[SelectItem].DischargeLimit + "%";
		}
		Label_EnergySave_Summary_center.Content = text2;
		text3 = ((!SaveModeList[SelectItem].isBatterySettingAuto) ? (text3 + lang.TranslationLanguage("_BatterySetting") + lang.TranslationLanguage("_Manually")) : (text3 + lang.TranslationLanguage("_BatterySetting") + lang.TranslationLanguage("_Auto")));
		if (SaveModeList[SelectItem].isBatterySettingAuto)
		{
			text3 = ((nStopChargeLimitFromWMI_17 == 0 || nStopDischargeLimitFromWMI_17 == 0) ? (text3 + "\r\n" + lang.TranslationLanguage("_ChargeLimit") + "：100%") : (text3 + "\r\n" + lang.TranslationLanguage("_ChargeLimit") + "：" + nStopChargeLimitFromWMI_17 + "%"));
		}
		else
		{
			text3 = text3 + "\r\n" + lang.TranslationLanguage("_ChargeLimit") + "：" + SaveModeList[SelectItem].ChargeLimit + "%";
		}
		Label_EnergySave_Summary_right.Content = text3;
	}

	private string Hour24ConvertToHour12(int hour_24)
	{
		if (hour_24 == 0)
		{
			return "12 am";
		}
		if (hour_24 == 12)
		{
			return "12 pm";
		}
		if (hour_24 > 12)
		{
			return hour_24 - 12 + " pm";
		}
		if (hour_24 < 12 && hour_24 != 0)
		{
			return hour_24 + " am";
		}
		return "";
	}

	private void SetUtilityStart()
	{
		Bitmap val = null;
		if (isUtilityStart)
		{
			val = Img.Load("Assets/utility_on_normal.png");
			Image_StartUtility.Source = val;
		}
		else
		{
			val = Img.Load("Assets/utility_off_normal.png");
			Image_StartUtility.Source = val;
		}
	}

	private void SetScheduleOpacity()
	{
		if (SaveModeList[SelectItem].isSchedule1_Enable)
		{
			SetScheduleOpacity(1, 1.0);
		}
		else
		{
			SetScheduleOpacity(1, 0.3);
		}
		if (SaveModeList[SelectItem].isSchedule2_Enable)
		{
			SetScheduleOpacity(2, 1.0);
		}
		else
		{
			SetScheduleOpacity(2, 0.3);
		}
	}

	private void SetBatterySetting()
	{
		if (SaveModeList[SelectItem].isBatterySettingAuto)
		{
			Bitmap val = null;
			val = Img.Load("Assets/btn_checkbox_enable.png");
			Image_EnergySave_auto.Source = val;
			val = Img.Load("Assets/btn_checkbox_disable.png");
			Image_EnergySave_manually.Source = val;
			if (nStopChargeLimitFromWMI_17 != 0 && nStopDischargeLimitFromWMI_17 != 0)
			{
				SetBatteryLimit(nStopChargeLimitFromWMI_17, nStopDischargeLimitFromWMI_17);
			}
			else
			{
				SetBatteryLimit(100, 95);
			}
			isAuto = true;
		}
		else
		{
			Bitmap val2 = null;
			val2 = Img.Load("Assets/btn_checkbox_disable.png");
			Image_EnergySave_auto.Source = val2;
			val2 = Img.Load("Assets/btn_checkbox_enable.png");
			Image_EnergySave_manually.Source = val2;
			SetBatteryLimit(SaveModeList[SelectItem].ChargeLimit, SaveModeList[SelectItem].DischargeLimit);
			isAuto = false;
		}
	}

	private void SetBatteryLimit(int charge, int discharge)
	{
		switch (charge)
		{
		case 10:
			Canvas.SetTop(Image_EnergySave_charge, 353.0);
			Canvas.SetTop(Label_EnergySave_charge, 350.0);
			break;
		case 20:
			Canvas.SetTop(Image_EnergySave_charge, 325.0);
			Canvas.SetTop(Label_EnergySave_charge, 320.0);
			break;
		case 30:
			Canvas.SetTop(Image_EnergySave_charge, 295.0);
			Canvas.SetTop(Label_EnergySave_charge, 290.0);
			break;
		case 40:
			Canvas.SetTop(Image_EnergySave_charge, 265.0);
			Canvas.SetTop(Label_EnergySave_charge, 260.0);
			break;
		case 50:
			Canvas.SetTop(Image_EnergySave_charge, 236.0);
			Canvas.SetTop(Label_EnergySave_charge, 230.0);
			break;
		case 60:
			Canvas.SetTop(Image_EnergySave_charge, 206.0);
			Canvas.SetTop(Label_EnergySave_charge, 200.0);
			break;
		case 70:
			Canvas.SetTop(Image_EnergySave_charge, 176.0);
			Canvas.SetTop(Label_EnergySave_charge, 170.0);
			break;
		case 80:
			Canvas.SetTop(Image_EnergySave_charge, 146.0);
			Canvas.SetTop(Label_EnergySave_charge, 140.0);
			break;
		case 90:
			Canvas.SetTop(Image_EnergySave_charge, 117.0);
			Canvas.SetTop(Label_EnergySave_charge, 110.0);
			break;
		case 100:
			Canvas.SetTop(Image_EnergySave_charge, 85.0);
			Canvas.SetTop(Label_EnergySave_charge, 80.0);
			break;
		default:
			Canvas.SetTop(Image_EnergySave_charge, 85.0);
			Canvas.SetTop(Label_EnergySave_charge, 80.0);
			break;
		}
		switch (discharge)
		{
		case 10:
			Canvas.SetTop(Image_EnergySave_discharge, 353.0);
			Canvas.SetTop(Label_EnergySave_discharge, 350.0);
			break;
		case 20:
			Canvas.SetTop(Image_EnergySave_discharge, 325.0);
			Canvas.SetTop(Label_EnergySave_discharge, 320.0);
			break;
		case 30:
			Canvas.SetTop(Image_EnergySave_discharge, 295.0);
			Canvas.SetTop(Label_EnergySave_discharge, 290.0);
			break;
		case 40:
			Canvas.SetTop(Image_EnergySave_discharge, 265.0);
			Canvas.SetTop(Label_EnergySave_discharge, 260.0);
			break;
		case 50:
			Canvas.SetTop(Image_EnergySave_discharge, 236.0);
			Canvas.SetTop(Label_EnergySave_discharge, 230.0);
			break;
		case 60:
			Canvas.SetTop(Image_EnergySave_discharge, 206.0);
			Canvas.SetTop(Label_EnergySave_discharge, 200.0);
			break;
		case 70:
			Canvas.SetTop(Image_EnergySave_discharge, 176.0);
			Canvas.SetTop(Label_EnergySave_discharge, 170.0);
			break;
		case 80:
			Canvas.SetTop(Image_EnergySave_discharge, 146.0);
			Canvas.SetTop(Label_EnergySave_discharge, 140.0);
			break;
		case 90:
			Canvas.SetTop(Image_EnergySave_discharge, 117.0);
			Canvas.SetTop(Label_EnergySave_discharge, 110.0);
			break;
		case 95:
			Canvas.SetTop(Image_EnergySave_discharge, 101.0);
			Canvas.SetTop(Label_EnergySave_discharge, 95.0);
			break;
		default:
			Canvas.SetTop(Image_EnergySave_discharge, 85.0);
			Canvas.SetTop(Label_EnergySave_discharge, 80.0);
			break;
		}
		Label_EnergySave_charge.Content = charge + "% " + lang.TranslationLanguage("_ChargeLimit");
		Label_EnergySave_discharge.Content = discharge + "% " + lang.TranslationLanguage("_DischargeLimit");
	}

	private void SetSaveModeListBox()
	{
		ListBox_Save_Mode.Items.Clear();
		for (int i = 0; i < SaveModeList.Count; i++)
		{
			AddSaveModeItem(SaveModeList[i].Name);
		}
	}

	private void SetDischargeRoutine()
	{
		if (SaveModeList[SelectItem].isEveryday)
		{
			Bitmap val = null;
			val = Img.Load("Assets/btn_checkbox_enable.png");
			Image_EnergySave_Everyday_Check.Source = val;
			val = Img.Load("Assets/btn_checkbox_disable.png");
			Image_EnergySave_Week_Check.Source = val;
		}
		else
		{
			Bitmap val2 = null;
			val2 = Img.Load("Assets/btn_checkbox_disable.png");
			Image_EnergySave_Everyday_Check.Source = val2;
			val2 = Img.Load("Assets/btn_checkbox_enable.png");
			Image_EnergySave_Week_Check.Source = val2;
		}
		if (SaveModeList[SelectItem].isSunday)
		{
			ImageBrush val3 = new ImageBrush { Stretch = Stretch.Fill };
			val3.Source = Img.Load("Assets/btn_week_selected.png");
			isSundayCheck = true;
			Label_EnergySave_Sunday.Background = val3;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Sunday.Foreground = foreground;
			EnableDayCount++;
		}
		else
		{
			isSundayCheck = false;
			Label_EnergySave_Sunday.Background = null;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Sunday.Foreground = foreground2;
			EnableDayCount--;
		}
		if (SaveModeList[SelectItem].isMonday)
		{
			ImageBrush val4 = new ImageBrush { Stretch = Stretch.Fill };
			val4.Source = Img.Load("Assets/btn_week_selected.png");
			isMondayCheck = true;
			Label_EnergySave_Monday.Background = val4;
			SolidColorBrush foreground3 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Monday.Foreground = foreground3;
			EnableDayCount++;
		}
		else
		{
			isMondayCheck = false;
			Label_EnergySave_Monday.Background = null;
			SolidColorBrush foreground4 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Monday.Foreground = foreground4;
			EnableDayCount--;
		}
		if (SaveModeList[SelectItem].isTuesday)
		{
			ImageBrush val5 = new ImageBrush { Stretch = Stretch.Fill };
			val5.Source = Img.Load("Assets/btn_week_selected.png");
			isTuesdayCheck = true;
			Label_EnergySave_Tuesday.Background = val5;
			SolidColorBrush foreground5 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Tuesday.Foreground = foreground5;
			EnableDayCount++;
		}
		else
		{
			isTuesdayCheck = false;
			Label_EnergySave_Tuesday.Background = null;
			SolidColorBrush foreground6 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Tuesday.Foreground = foreground6;
			EnableDayCount--;
		}
		if (SaveModeList[SelectItem].isWednesday)
		{
			ImageBrush val6 = new ImageBrush { Stretch = Stretch.Fill };
			val6.Source = Img.Load("Assets/btn_week_selected.png");
			isWednesdayCheck = true;
			Label_EnergySave_Wednesday.Background = val6;
			SolidColorBrush foreground7 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Wednesday.Foreground = foreground7;
			EnableDayCount++;
		}
		else
		{
			isWednesdayCheck = false;
			Label_EnergySave_Wednesday.Background = null;
			SolidColorBrush foreground8 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Wednesday.Foreground = foreground8;
			EnableDayCount--;
		}
		if (SaveModeList[SelectItem].isThursday)
		{
			ImageBrush val7 = new ImageBrush { Stretch = Stretch.Fill };
			val7.Source = Img.Load("Assets/btn_week_selected.png");
			isThursdayCheck = true;
			Label_EnergySave_Thursday.Background = val7;
			SolidColorBrush foreground9 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Thursday.Foreground = foreground9;
			EnableDayCount++;
		}
		else
		{
			isThursdayCheck = false;
			Label_EnergySave_Thursday.Background = null;
			SolidColorBrush foreground10 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Thursday.Foreground = foreground10;
			EnableDayCount--;
		}
		if (SaveModeList[SelectItem].isFriday)
		{
			ImageBrush val8 = new ImageBrush { Stretch = Stretch.Fill };
			val8.Source = Img.Load("Assets/btn_week_selected.png");
			isFridayCheck = true;
			Label_EnergySave_Friday.Background = val8;
			SolidColorBrush foreground11 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Friday.Foreground = foreground11;
			EnableDayCount++;
		}
		else
		{
			isFridayCheck = false;
			Label_EnergySave_Friday.Background = null;
			SolidColorBrush foreground12 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Friday.Foreground = foreground12;
			EnableDayCount--;
		}
		if (SaveModeList[SelectItem].isSaturday)
		{
			ImageBrush val9 = new ImageBrush { Stretch = Stretch.Fill };
			val9.Source = Img.Load("Assets/btn_week_selected.png");
			isSaturdayCheck = true;
			Label_EnergySave_Saturday.Background = val9;
			SolidColorBrush foreground13 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Saturday.Foreground = foreground13;
			EnableDayCount++;
		}
		else
		{
			isSaturdayCheck = false;
			Label_EnergySave_Saturday.Background = null;
			SolidColorBrush foreground14 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Saturday.Foreground = foreground14;
			EnableDayCount--;
		}
	}

	private void Switch_Tick(object sender, EventArgs e)
	{
		SwitchTimer.Stop();
		EnableEnergySave(bEnable: false);
		cmd = GetAllCommandForWMI_118();
		SetEnergySaveInfo2EC(cmd);
		EnableEnergySave(bEnable: true);
		SetSaveModeList2Ini();
		SetSelectProfile();
		SetEnergySaveStatus();
		BatteryRefreshCommand(0, 0, 0);
		Thread.Sleep(2000);
		BatteryRefreshCommand(1, 0, 0);
	}

	private void dispatcherTimer_Tick(object sender, EventArgs e)
	{
		if (fakeData) return;
		if (dt.Rows.Count > 60)
		{
			dt.Rows.RemoveAt(0);
		}
		PowerRow dataRow = dt.NewRow();
		int num = 0;
		double num2 = 0.0;
		if (isAC)
		{
			intArrCount %= 5;
			short num3 = 5;
			short num4 = 139;
			short num5 = 1;
			short num6 = 177;
			int data = (num3 << 24) + (num4 << 16) + (num5 << 8) + num6;
			uint num7 = (uint)DCHU.SetWMIForTotalCurrent(65, (uint)data);
			num7 &= 0xFFFF;
			uint num8 = (num7 & 0xFF) << 8;
			uint num9 = (num7 & 0xFF00) >> 8;
			num7 = num8 + num9;
			data = 93126656 + (num5 << 8) + num6;
			uint num10 = (uint)DCHU.SetWMIForTotalCurrent(65, (uint)data);
			decimal num11 = (num10 & 0xFF) switch
			{
				10u => Convert.ToDecimal("0.1"), 
				20u => Convert.ToDecimal("0.2"), 
				5u => Convert.ToDecimal("0.05"), 
				_ => Convert.ToDecimal("0.2"), 
			};
			decimal num12 = Math.Round((decimal)num7 * 3m / 1023m, 2);
			decimal num13 = Convert.ToDecimal(19.2);
			decimal num14 = 0m;
			num14 = num12 / num11;
			decimal num15 = num14 * num13;
			intArr[intArrCount] = num15;
			num15 = Average();
			intArrCount++;
			num2 = Convert.ToDouble(num15);
		}
		else
		{
			int data2 = 92996017;
			uint num16 = (uint)DCHU.SetWMIForTotalCurrent(65, (uint)data2);
			data2 = 93061553;
			uint num17 = (uint)DCHU.SetWMIForTotalCurrent(65, (uint)data2);
			num16 &= 0xFF;
			num17 &= 0xFF;
			num2 = (num16 << 8) + num17;
		}
		double num18 = 485.0 - num2 * 330.0 / (double)WattLimit;
		Canvas.SetTop(Image_ConsumptionCurrentValue, num18);
		Label_ConsumptionCurrentValue.Content = $"{num2:#,0.#}" + " W";
		Canvas.SetTop(Label_ConsumptionCurrentValue, num18 - 8.0);
		if (isAC)
		{
			dataRow["AC"] = num2;
			dataRow["DC"] = 0;
		}
		else
		{
			dataRow["AC"] = 0;
			dataRow["DC"] = num2;
		}
		dt.Rows.Add(dataRow);
		mainChart.DataBind(dt);
		
	}

	private decimal Average()
	{
		decimal result = 0m;
		decimal num = 0m;
		decimal[] array = intArr;
		for (int i = 0; i < array.Length; i++)
		{
			int num2 = (int)array[i];
			num += (decimal)num2;
			result = num / 5m;
		}
		return result;
	}

	private void Apps_main_MouseMove(object sender, PointerEventArgs e)
	{
		point = e.GetPosition(this);
		double num = 0.0;
		double num2 = 0.0;
		num = declW / 1920.0;
		num2 = declH / 1080.0;
				if (!isWindowsFullscreen)
		{
			if (point.Y < 81.0 * num2 && point.X > 506.0 * num && point.X < 1414.0 * num)
			{
				this.Cursor = new Cursor(StandardCursorType.SizeAll);
			}
			else
			{
				this.Cursor = Cursor.Default;
			}
		}
		int num3 = 0;
		Point position = e.GetPosition(Canvas_EnergySave);
				if (isAuto)
		{
			Image_EnergySave_charge.Cursor = new Cursor(StandardCursorType.Arrow);
			Image_EnergySave_discharge.Cursor = new Cursor(StandardCursorType.Arrow);
			return;
		}
		Image_EnergySave_charge.Cursor = new Cursor(StandardCursorType.Hand);
		Image_EnergySave_discharge.Cursor = new Cursor(StandardCursorType.Hand);
		if (isChargePressed)
		{
			if (position.Y < 94.0)
			{
				num3 = nChargeLimit;
				nChargeLimit = 100;
				Canvas.SetTop(Image_EnergySave_charge, 85.0);
				Canvas.SetTop(Label_EnergySave_charge, 75.0);
			}
			else if (position.Y < 126.0 && position.Y >= 94.0)
			{
				num3 = nChargeLimit;
				nChargeLimit = 90;
				if (nChargeLimit <= nDischargeLimit)
				{
					nChargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_charge, 117.0);
				Canvas.SetTop(Label_EnergySave_charge, 105.0);
			}
			else if (position.Y < 155.0 && position.Y >= 126.0)
			{
				num3 = nChargeLimit;
				nChargeLimit = 80;
				if (nChargeLimit <= nDischargeLimit)
				{
					nChargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_charge, 146.0);
				Canvas.SetTop(Label_EnergySave_charge, 135.0);
			}
			else if (position.Y < 185.0 && position.Y >= 155.0)
			{
				num3 = nChargeLimit;
				nChargeLimit = 70;
				if (nChargeLimit <= nDischargeLimit)
				{
					nChargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_charge, 176.0);
				Canvas.SetTop(Label_EnergySave_charge, 165.0);
			}
			else if (position.Y >= 215.0)
			{
				num3 = nChargeLimit;
				nChargeLimit = 60;
				if (nChargeLimit <= nDischargeLimit)
				{
					nChargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_charge, 206.0);
				Canvas.SetTop(Label_EnergySave_charge, 195.0);
			}
			Label_EnergySave_charge.Content = nChargeLimit + "% Charge Limit";
						SaveModeList[SelectItem].ChargeLimit = nChargeLimit;
		}
		if (isDischargePressed)
		{
			if (position.Y < 126.0 && position.Y >= 94.0)
			{
				num3 = nDischargeLimit;
				nDischargeLimit = 90;
				if (nDischargeLimit >= nChargeLimit)
				{
					nDischargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_discharge, 117.0);
				Canvas.SetTop(Label_EnergySave_discharge, 105.0);
			}
			else if (position.Y < 155.0 && position.Y >= 126.0)
			{
				num3 = nDischargeLimit;
				nDischargeLimit = 80;
				if (nDischargeLimit >= nChargeLimit)
				{
					nDischargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_discharge, 146.0);
				Canvas.SetTop(Label_EnergySave_discharge, 135.0);
			}
			else if (position.Y < 185.0 && position.Y >= 155.0)
			{
				num3 = nDischargeLimit;
				nDischargeLimit = 70;
				if (nDischargeLimit >= nChargeLimit)
				{
					nDischargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_discharge, 176.0);
				Canvas.SetTop(Label_EnergySave_discharge, 165.5);
			}
			else if (position.Y < 215.0 && position.Y >= 185.0)
			{
				num3 = nDischargeLimit;
				nDischargeLimit = 60;
				if (nDischargeLimit >= nChargeLimit)
				{
					nDischargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_discharge, 206.0);
				Canvas.SetTop(Label_EnergySave_discharge, 195.0);
			}
			else if (position.Y < 245.0 && position.Y >= 215.0)
			{
				num3 = nDischargeLimit;
				nDischargeLimit = 50;
				if (nDischargeLimit >= nChargeLimit)
				{
					nDischargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_discharge, 236.0);
				Canvas.SetTop(Label_EnergySave_discharge, 225.0);
			}
			else if (position.Y >= 245.0)
			{
				num3 = nDischargeLimit;
				nDischargeLimit = 40;
				if (nDischargeLimit >= nChargeLimit)
				{
					nDischargeLimit = num3;
					return;
				}
				Canvas.SetTop(Image_EnergySave_discharge, 265.0);
				Canvas.SetTop(Label_EnergySave_discharge, 255.0);
			}
			Label_EnergySave_discharge.Content = nDischargeLimit + "% Discharge Limit";
						SaveModeList[SelectItem].DischargeLimit = nDischargeLimit;
		}
		UpdateEnergySaveUI();
	}

	private void Button_Click_Close(object sender, RoutedEventArgs e)
	{
		this.Close();
	}

	private void Button_Click_Minimize(object sender, RoutedEventArgs e)
	{
		this.WindowState = (WindowState)1;
	}

	private void Image_EnergySave_MouseEnter(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (nPageSelect == nPowerPlanPage)
		{
			val = Img.Load("Assets/btn_panel_hover.png");
			Image_EnergySave.Source = val;
		}
		else if (nPageSelect != nEnergySavePage)
		{
		}
	}

	private void Image_EnergySave_MouseLeave(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (nPageSelect == nPowerPlanPage)
		{
			val = Img.Load("Assets/btn_panel_normal.png");
			Image_EnergySave.Source = val;
		}
		else if (nPageSelect != nEnergySavePage)
		{
		}
	}

	private void Image_EnergySave_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		nPageSelect = nEnergySavePage;
	}

	private void Image_EnergySave_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		Bitmap val = null;
		val = Img.Load("Assets/btn_panel_selected.png");
		Image_EnergySave.Source = val;
		val = Img.Load("Assets/btn_panel_normal.png");
		Image_PowerPlan.Source = val;
		val = Img.Load("Assets/ic_energy_save_selected.png");
		Image_EnergySave_icon.Source = val;
		val = Img.Load("Assets/ic_power_plan_normal.png");
		Image_PowerPlan_icon.Source = val;
		ImageBrush val2 = new ImageBrush { Stretch = Stretch.Fill };
		val2.Source = Img.Load("Assets/bg_energy_save.png");
		Apps_main.Background = val2;
		Canvas_PowerPlan.IsVisible = false;
		Canvas_EnergySave.IsVisible = true;
		grid_Disable_background.IsVisible = true;
		isLock = true;
		val = Img.Load("Assets/btn_lock_normal.png");
		Image_EnergySave_lock.Source = val;
		Label_EnergySave_lock.Content = lang.TranslationLanguage("_UnlockToModify");
		grid_Disable_background.IsVisible = true;
		Image_EnergySave_save.IsVisible = false;
		Label_EnergySave_save.IsVisible = false;
		int num = (int)(PowerStatusAdapter.BatteryLifePercent * 100f);
		Label_EnergySave_battery_power.Content = num + "%";
		if (num < 10)
		{
			Image_EnergySave_battery_cabacity.Height = 25.0;
			Canvas.SetTop(Image_EnergySave_battery_cabacity, 370.0);
		}
		else if (num == 100)
		{
			Image_EnergySave_battery_cabacity.Height = 304.0;
			Canvas.SetTop(Image_EnergySave_battery_cabacity, 100.0);
		}
		else
		{
			Image_EnergySave_battery_cabacity.Height = 25 + 31 * (num / 10 - 1);
			Canvas.SetTop(Image_EnergySave_battery_cabacity, 370 - 30 * (num / 10 - 1));
		}
	}

	private void Image_StartUtility_MouseEnter(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (isUtilityStart)
		{
			val = Img.Load("Assets/utility_on_hover.png");
			Image_StartUtility.Source = val;
		}
		else
		{
			val = Img.Load("Assets/utility_off_hover.png");
			Image_StartUtility.Source = val;
		}
	}

	private void Image_StartUtility_MouseLeave(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (isUtilityStart)
		{
			val = Img.Load("Assets/utility_on_normal.png");
			Image_StartUtility.Source = val;
		}
		else
		{
			val = Img.Load("Assets/utility_off_normal.png");
			Image_StartUtility.Source = val;
		}
	}

	private void Image_StartUtility_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (isUtilityStart)
		{
			isUtilityStart = false;
		}
		else
		{
			isUtilityStart = true;
		}
	}

	private void Image_StartUtility_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		if (CheckScheduleOverlapOrNot())
		{
			MessageBox.Show("Two Schedules should not be overlapped. At least an hour gap between two schedules.");
			return;
		}
		Bitmap val = null;
		if (isUtilityStart)
		{
			val = Img.Load("Assets/utility_on_normal.png");
			Image_StartUtility.Source = val;
			EnableEnergySave(bEnable: false);
			cmd = GetAllCommandForWMI_118();
			SetEnergySaveInfo2EC(cmd);
			EnableEnergySave(bEnable: true);
			SetSaveModeList2Ini();
			SetSelectProfile();
		}
		else
		{
			val = Img.Load("Assets/utility_off_normal.png");
			Image_StartUtility.Source = val;
			EnableEnergySave(bEnable: false);
			SwitchTimer.Start();
		}
		SetEnergySaveStatus();
	}

	private static void BatteryRefreshCommand(int ACOFF, int Refresh, int FlexiCharge)
	{
		SetACOFF(ACOFF);
		SetBatteryRefresh(Refresh);
		SetBatteryFlexiDischange(FlexiCharge);
	}

	private static void SetACOFF(int value)
	{
		DCHU.SetWMI(121, 28, (uint)value);
	}

	private static void SetBatteryFlexiDischange(int value)
	{
		DCHU.SetWMI(118, 7, (uint)value);
	}

	private static void SetBatteryRefresh(int value)
	{
		DCHU.SetWMI(121, 29, (uint)value);
	}

	private void SetEnergySaveInfo2EC(List<int> cmd)
	{
		for (int i = 0; i < cmd.Count; i++)
		{
			DCHU.SetWMI(118, (uint)cmd[i]);
		}
	}

	private void EnableEnergySave(bool bEnable)
	{
		int num = 0;
		num = ((!bEnable) ? 83886080 : 83886081);
		DCHU.SetWMI(118, (uint)num);
	}

	private List<int> GetAllCommandForWMI_118()
	{
		List<int> list = new List<int>();
		int num = 0;
		DateTime now = DateTime.Now;
		int num2 = 0;
		switch (now.DayOfWeek.ToString())
		{
		case "Sunday":
			num2 = 0;
			break;
		case "Monday":
			num2 = 1;
			break;
		case "Tuesday":
			num2 = 2;
			break;
		case "Wednesday":
			num2 = 3;
			break;
		case "Thursday":
			num2 = 4;
			break;
		case "Friday":
			num2 = 5;
			break;
		case "Saturday":
			num2 = 6;
			break;
		}
		num2 <<= 17;
		num = 16777216;
		list.Add(num + now.Second + (now.Minute << 6) + (now.Hour << 12) + num2);
		num = 33554432;
		int num3 = 0;
		int num4 = 0;
		if (SaveModeList[SelectItem].isEveryday)
		{
			num3 = 255;
		}
		if (!SaveModeList[SelectItem].isEveryday && SaveModeList[SelectItem].isSunday)
		{
			num3++;
		}
		if (!SaveModeList[SelectItem].isEveryday && SaveModeList[SelectItem].isMonday)
		{
			num3 += 2;
		}
		if (!SaveModeList[SelectItem].isEveryday && SaveModeList[SelectItem].isTuesday)
		{
			num3 += 4;
		}
		if (!SaveModeList[SelectItem].isEveryday && SaveModeList[SelectItem].isWednesday)
		{
			num3 += 8;
		}
		if (!SaveModeList[SelectItem].isEveryday && SaveModeList[SelectItem].isThursday)
		{
			num3 += 16;
		}
		if (!SaveModeList[SelectItem].isEveryday && SaveModeList[SelectItem].isFriday)
		{
			num3 += 32;
		}
		if (!SaveModeList[SelectItem].isEveryday && SaveModeList[SelectItem].isSaturday)
		{
			num3 += 64;
		}
		if (SaveModeList[SelectItem].isBatterySettingAuto)
		{
			num4 = ((nStopChargeLimitFromWMI_17 == 0 || nStopDischargeLimitFromWMI_17 == 0) ? 95 : nStopDischargeLimitFromWMI_17);
		}
		else
		{
			num4 = SaveModeList[SelectItem].DischargeLimit;
		}
		list.Add(num + num3 + (num4 << 8));
		int num5 = 0;
		int num6 = 0;
		if (SaveModeList[SelectItem].isSchedule1_Enable)
		{
			num = 50331648;
			num5 = SaveModeList[SelectItem].Schedule1_Start;
			num6 = SaveModeList[SelectItem].Schedule1_End;
			num6++;
			list.Add(num + (num5 << 6) + (num6 << 17));
		}
		else
		{
			num = 50331648;
			list.Add(num);
		}
		int num7 = 0;
		int num8 = 0;
		if (SaveModeList[SelectItem].isSchedule2_Enable)
		{
			num = 67108864;
			num7 = SaveModeList[SelectItem].Schedule2_Start;
			num8 = SaveModeList[SelectItem].Schedule2_End;
			num8++;
			list.Add(num + (num7 << 6) + (num8 << 17));
		}
		else
		{
			num = 67108864;
			list.Add(num);
		}
		num = 100663296;
		int num9 = 0;
		int num10 = 0;
		if (SaveModeList[SelectItem].isBatterySettingAuto)
		{
			if (nStopChargeLimitFromWMI_17 != 0 && nStopDischargeLimitFromWMI_17 != 0)
			{
				num9 = nStopDischargeLimitFromWMI_17;
				num10 = nStopChargeLimitFromWMI_17;
			}
			else
			{
				num9 = 95;
				num10 = 100;
			}
		}
		else
		{
			num9 = SaveModeList[SelectItem].DischargeLimit;
			num10 = SaveModeList[SelectItem].ChargeLimit;
		}
		list.Add(num + num9 + (num10 << 8));
		return list;
	}

	private void Image_PowerPlan_MouseEnter(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (nPageSelect != nPowerPlanPage && nPageSelect == nEnergySavePage)
		{
			val = Img.Load("Assets/btn_panel_hover.png");
			Image_PowerPlan.Source = val;
		}
	}

	private void Image_PowerPlan_MouseLeave(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (nPageSelect != nPowerPlanPage && nPageSelect == nEnergySavePage)
		{
			val = Img.Load("Assets/btn_panel_normal.png");
			Image_PowerPlan.Source = val;
		}
	}

	private void Image_PowerPlan_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		nPageSelect = nPowerPlanPage;
	}

	private void Image_PowerPlan_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		Bitmap val = null;
		val = Img.Load("Assets/btn_panel_normal.png");
		Image_EnergySave.Source = val;
		val = Img.Load("Assets/btn_panel_selected.png");
		Image_PowerPlan.Source = val;
		val = Img.Load("Assets/ic_energy_save_normal.png");
		Image_EnergySave_icon.Source = val;
		val = Img.Load("Assets/ic_power_plan_selected.png");
		Image_PowerPlan_icon.Source = val;
		ImageBrush val2 = new ImageBrush { Stretch = Stretch.Fill };
		val2.Source = Img.Load("Assets/bg_power_plan.png");
		Apps_main.Background = val2;
		Canvas_PowerPlan.IsVisible = true;
		Canvas_EnergySave.IsVisible = false;
		grid_Disable_background.IsVisible = false;
		isLock = true;
		val = Img.Load("Assets/btn_lock_normal.png");
		Image_EnergySave_lock.Source = val;
		Label_EnergySave_lock.Content = lang.TranslationLanguage("_UnlockToModify");
		Image_EnergySave_save.IsVisible = false;
		Label_EnergySave_save.IsVisible = false;
		GetSaveModeCountFromIni();
		if (SaveModeCount > 0)
		{
			GetSaveModeListFromIni();
			GetEnergySaveStatus();
		}
		else
		{
			SaveModeList.Clear();
			AddNew2SaveModeList("Default");
			SelectItem = 0;
			isUtilityStart = false;
		}
		GetSelectProfile();
		SetSaveModeListBox();
		UpdateEnergySaveUI();
		setSaveModeSelect(SelectItem);
	}

	private void Image_Minute_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		Bitmap val = null;
		val = Img.Load("Assets/btn_checkbox_enable.png");
		Image_Minute.Source = val;
		val = Img.Load("Assets/btn_checkbox_disable.png");
		Image_Second.Source = val;
		Label_Legend.Content = lang.TranslationLanguage("_PowerUsageMin");
		dt.Clear();
		for (int i = 0; i < 60; i++)
		{
			PowerRow dataRow = dt.NewRow();
			dataRow["AC"] = 0;
			dt.Rows.Add(dataRow);
		}
		mainChart.DataBind(dt);
		Canvas.SetTop(Image_ConsumptionCurrentValue, 485.0);
		Label_ConsumptionCurrentValue.Content = $"{0:#,0.#}" + " W";
		Canvas.SetTop(Label_ConsumptionCurrentValue, 477.0);
		dispatcherTimer.Stop();
		dispatcherTimer = new DispatcherTimer();
		dispatcherTimer.Tick += dispatcherTimer_Tick;
		dispatcherTimer.Interval = new TimeSpan(0, 1, 0);
		dispatcherTimer.Start();
		isSecond = false;
	}

	private void Image_Second_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		Bitmap val = null;
		val = Img.Load("Assets/btn_checkbox_disable.png");
		Image_Minute.Source = val;
		val = Img.Load("Assets/btn_checkbox_enable.png");
		Image_Second.Source = val;
		Label_Legend.Content = lang.TranslationLanguage("_PowerUsageSec");
		dt.Clear();
		for (int i = 0; i < 60; i++)
		{
			PowerRow dataRow = dt.NewRow();
			dataRow["AC"] = 0;
			dt.Rows.Add(dataRow);
		}
		mainChart.DataBind(dt);
		Canvas.SetTop(Image_ConsumptionCurrentValue, 485.0);
		Label_ConsumptionCurrentValue.Content = $"{0:#,0.#}" + " W";
		Canvas.SetTop(Label_ConsumptionCurrentValue, 477.0);
		dispatcherTimer.Stop();
		dispatcherTimer = new DispatcherTimer();
		dispatcherTimer.Tick += dispatcherTimer_Tick;
		dispatcherTimer.Interval = new TimeSpan(0, 0, 1);
		dispatcherTimer.Start();
		isSecond = true;
	}

	private void LoadPowerPlanScheme()
	{
		Dictionary<Guid, PowerPlanInfo> allPowerSchemas = PowerSchemeHelper.GetAllPowerSchemas();
		int num = 0;
		string text = "";
		Guid powerActiveScheme = PowerSchemeHelper.GetPowerActiveScheme();
		foreach (KeyValuePair<Guid, PowerPlanInfo> item in allPowerSchemas)
		{
			if (item.Value.SchemeGuid == powerActiveScheme)
			{
				text = item.Value.FriendlyName;
			}
			num++;
		}
		Label_PowerPlan_Summary.Content = lang.TranslationLanguage("_CurrentPowerPlan") + text;
	}

	private void Ellipse_schedule1_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (SaveModeList[SelectItem].isSchedule1_Enable)
		{
			SaveModeList[SelectItem].isSchedule1_Enable = false;
			SetScheduleOpacity(1, 0.3);
		}
		else
		{
			SaveModeList[SelectItem].isSchedule1_Enable = true;
			SetScheduleOpacity(1, 1.0);
		}
		UpdateEnergySaveUI();
	}

	private void SetScheduleOpacity(int schedule, double opacity)
	{
		switch (schedule)
		{
		case 1:
			knob.Opacity = opacity;
			knob2.Opacity = opacity;
			Label_EnergySave_schedule_1_0.Opacity = opacity;
			Label_EnergySave_schedule_1_6.Opacity = opacity;
			Label_EnergySave_schedule_1_12.Opacity = opacity;
			Label_EnergySave_schedule_1_18.Opacity = opacity;
			Label_EnergySave_1.Opacity = opacity;
			Label_EnergySave_schedule1.Opacity = opacity;
			Label_EnergySave_schedule1_time.Opacity = opacity;
			break;
		case 2:
			knob3.Opacity = opacity;
			knob4.Opacity = opacity;
			Label_EnergySave_schedule_2_0.Opacity = opacity;
			Label_EnergySave_schedule_2_6.Opacity = opacity;
			Label_EnergySave_schedule_2_12.Opacity = opacity;
			Label_EnergySave_schedule_2_18.Opacity = opacity;
			Label_EnergySave_2.Opacity = opacity;
			Label_EnergySave_schedule2.Opacity = opacity;
			Label_EnergySave_schedule2_time.Opacity = opacity;
			break;
		}
	}

	private void SetSchedule1FromList(int startTime, int endTime)
	{
		ProgressCircle progressCircle = knob.Circle;
		ProgressCircle progressCircle2 = knob.Circle2;
		knob.Value = startTime;
		knob2.Value = endTime;
		progressCircle.Value = 4 * (int)knob2.Value + (int)knob2.Value / 6;
		progressCircle2.Value = 4 * (int)knob.Value + (int)knob.Value / 6;
		StartValue = startTime;
		EndValue = endTime;
		Label_EnergySave_schedule1_time.Content = startTime + " - " + endTime;
	}

	private void Ellipse_schedule2_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (SaveModeList[SelectItem].isSchedule2_Enable)
		{
			SaveModeList[SelectItem].isSchedule2_Enable = false;
			SetScheduleOpacity(2, 0.3);
		}
		else
		{
			SaveModeList[SelectItem].isSchedule2_Enable = true;
			SetScheduleOpacity(2, 1.0);
		}
		UpdateEnergySaveUI();
	}

	private void SetSchedule2FromList(int startTime, int endTime)
	{
		ProgressCircle progressCircle = knob3.Circle;
		ProgressCircle progressCircle2 = knob3.Circle2;
		knob3.Value = startTime;
		knob4.Value = endTime;
		progressCircle.Value = 4 * (int)knob4.Value + (int)knob4.Value / 6;
		progressCircle2.Value = 4 * (int)knob3.Value + (int)knob3.Value / 6;
		StartValue_2 = startTime;
		EndValue_2 = endTime;
		Label_EnergySave_schedule2_time.Content = startTime + " - " + endTime;
	}

	private void Image_EnergySave_lock_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		Bitmap val = null;
		if (isLock)
		{
			isLock = false;
			val = Img.Load("Assets/btn_unlock_normal.png");
			Image_EnergySave_lock.Source = val;
			Label_EnergySave_lock.Content = lang.TranslationLanguage("_LockToPreventChanges");
			grid_Disable_background.IsVisible = false;
			Image_EnergySave_save.IsVisible = true;
			Label_EnergySave_save.IsVisible = true;
			return;
		}
		isLock = true;
		val = Img.Load("Assets/btn_lock_normal.png");
		Image_EnergySave_lock.Source = val;
		Label_EnergySave_lock.Content = lang.TranslationLanguage("_UnlockToModify");
		grid_Disable_background.IsVisible = true;
		Image_EnergySave_save.IsVisible = false;
		Label_EnergySave_save.IsVisible = false;
		GetSaveModeCountFromIni();
		if (SaveModeCount > 0)
		{
			GetSaveModeListFromIni();
			GetEnergySaveStatus();
		}
		else
		{
			SaveModeList.Clear();
			AddNew2SaveModeList("Default");
			SelectItem = 0;
			isUtilityStart = false;
		}
		GetSelectProfile();
		SetSaveModeListBox();
		UpdateEnergySaveUI();
		setSaveModeSelect(SelectItem);
	}

	private void Image_EnergySave_lock_MouseEnter(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (isLock)
		{
			val = Img.Load("Assets/btn_lock_hover.png");
			Image_EnergySave_lock.Source = val;
		}
		else
		{
			val = Img.Load("Assets/btn_unlock_hover.png");
			Image_EnergySave_lock.Source = val;
		}
	}

	private void Image_EnergySave_lock_MouseLeave(object sender, PointerEventArgs e)
	{
		Bitmap val = null;
		if (isLock)
		{
			val = Img.Load("Assets/btn_lock_normal.png");
			Image_EnergySave_lock.Source = val;
		}
		else
		{
			val = Img.Load("Assets/btn_unlock_normal.png");
			Image_EnergySave_lock.Source = val;
		}
	}

	private void Image_EnergySave_Everyday_Check_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		isEveryday = true;
		Bitmap val = null;
		val = Img.Load("Assets/btn_checkbox_enable.png");
		Image_EnergySave_Everyday_Check.Source = val;
		val = Img.Load("Assets/btn_checkbox_disable.png");
		Image_EnergySave_Week_Check.Source = val;
		SaveModeList[SelectItem].isEveryday = isEveryday;
		UpdateEnergySaveUI();
	}

	private void Image_EnergySave_Week_Check_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		isEveryday = false;
		Bitmap val = null;
		val = Img.Load("Assets/btn_checkbox_disable.png");
		Image_EnergySave_Everyday_Check.Source = val;
		val = Img.Load("Assets/btn_checkbox_enable.png");
		Image_EnergySave_Week_Check.Source = val;
		SaveModeList[SelectItem].isEveryday = isEveryday;
		UpdateEnergySaveUI();
	}

	private void Label_EnergySave_Sunday_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		ImageBrush val = new ImageBrush { Stretch = Stretch.Fill };
		val.Source = Img.Load("Assets/btn_week_selected.png");
		if (isSundayCheck)
		{
			isSundayCheck = false;
			Label_EnergySave_Sunday.Background = null;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Sunday.Foreground = foreground;
		}
		else
		{
			isSundayCheck = true;
			Label_EnergySave_Sunday.Background = val;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Sunday.Foreground = foreground2;
		}
		SaveModeList[SelectItem].isSunday = isSundayCheck;
		UpdateEnergySaveUI();
	}

	private void Label_EnergySave_Monday_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		ImageBrush val = new ImageBrush { Stretch = Stretch.Fill };
		val.Source = Img.Load("Assets/btn_week_selected.png");
		if (isMondayCheck)
		{
			isMondayCheck = false;
			Label_EnergySave_Monday.Background = null;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Monday.Foreground = foreground;
		}
		else
		{
			isMondayCheck = true;
			Label_EnergySave_Monday.Background = val;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Monday.Foreground = foreground2;
		}
		SaveModeList[SelectItem].isMonday = isMondayCheck;
		UpdateEnergySaveUI();
	}

	private void Label_EnergySave_Tuesday_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		ImageBrush val = new ImageBrush { Stretch = Stretch.Fill };
		val.Source = Img.Load("Assets/btn_week_selected.png");
		if (isTuesdayCheck)
		{
			isTuesdayCheck = false;
			Label_EnergySave_Tuesday.Background = null;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Tuesday.Foreground = foreground;
		}
		else
		{
			isTuesdayCheck = true;
			Label_EnergySave_Tuesday.Background = val;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Tuesday.Foreground = foreground2;
		}
		SaveModeList[SelectItem].isTuesday = isTuesdayCheck;
		UpdateEnergySaveUI();
	}

	private void Label_EnergySave_Wednesday_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		ImageBrush val = new ImageBrush { Stretch = Stretch.Fill };
		val.Source = Img.Load("Assets/btn_week_selected.png");
		if (isWednesdayCheck)
		{
			isWednesdayCheck = false;
			Label_EnergySave_Wednesday.Background = null;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Wednesday.Foreground = foreground;
		}
		else
		{
			isWednesdayCheck = true;
			Label_EnergySave_Wednesday.Background = val;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Wednesday.Foreground = foreground2;
		}
		SaveModeList[SelectItem].isWednesday = isWednesdayCheck;
		UpdateEnergySaveUI();
	}

	private void Label_EnergySave_Thursday_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		ImageBrush val = new ImageBrush { Stretch = Stretch.Fill };
		val.Source = Img.Load("Assets/btn_week_selected.png");
		if (isThursdayCheck)
		{
			isThursdayCheck = false;
			Label_EnergySave_Thursday.Background = null;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Thursday.Foreground = foreground;
		}
		else
		{
			isThursdayCheck = true;
			Label_EnergySave_Thursday.Background = val;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Thursday.Foreground = foreground2;
		}
		SaveModeList[SelectItem].isThursday = isThursdayCheck;
		UpdateEnergySaveUI();
	}

	private void Label_EnergySave_Friday_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		ImageBrush val = new ImageBrush { Stretch = Stretch.Fill };
		val.Source = Img.Load("Assets/btn_week_selected.png");
		if (isFridayCheck)
		{
			isFridayCheck = false;
			Label_EnergySave_Friday.Background = null;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Friday.Foreground = foreground;
		}
		else
		{
			isFridayCheck = true;
			Label_EnergySave_Friday.Background = val;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Friday.Foreground = foreground2;
		}
		SaveModeList[SelectItem].isFriday = isFridayCheck;
		UpdateEnergySaveUI();
	}

	private void Label_EnergySave_Saturday_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		ImageBrush val = new ImageBrush { Stretch = Stretch.Fill };
		val.Source = Img.Load("Assets/btn_week_selected.png");
		if (isSaturdayCheck)
		{
			isSaturdayCheck = false;
			Label_EnergySave_Saturday.Background = null;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_Saturday.Foreground = foreground;
		}
		else
		{
			isSaturdayCheck = true;
			Label_EnergySave_Saturday.Background = val;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
			Label_EnergySave_Saturday.Foreground = foreground2;
		}
		SaveModeList[SelectItem].isSaturday = isSaturdayCheck;
		UpdateEnergySaveUI();
	}

	private void Image_EnergySave_auto_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		isAuto = true;
		Bitmap val = null;
		val = Img.Load("Assets/btn_checkbox_enable.png");
		Image_EnergySave_auto.Source = val;
		val = Img.Load("Assets/btn_checkbox_disable.png");
		Image_EnergySave_manually.Source = val;
		SaveModeList[SelectItem].isBatterySettingAuto = isAuto;
		UpdateEnergySaveUI();
	}

	private void Image_EnergySave_manually_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		isAuto = false;
		Bitmap val = null;
		val = Img.Load("Assets/btn_checkbox_disable.png");
		Image_EnergySave_auto.Source = val;
		val = Img.Load("Assets/btn_checkbox_enable.png");
		Image_EnergySave_manually.Source = val;
		SaveModeList[SelectItem].isBatterySettingAuto = isAuto;
		UpdateEnergySaveUI();
	}

	private void Image_EnergySave_save_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (CheckScheduleOverlapOrNot())
		{
			MessageBox.Show("Two Schedules should not be overlapped. At least an hour gap between two schedules.");
			return;
		}
		isUtilityStart = true;
		Bitmap val = null;
		val = Img.Load("Assets/utility_on_normal.png");
		Image_StartUtility.Source = val;
		EnableEnergySave(bEnable: false);
		BatteryRefreshCommand(0, 0, 0);
		Thread.Sleep(500);
		BatteryRefreshCommand(1, 0, 0);
		Thread.Sleep(1000);
		EnableEnergySave(bEnable: false);
		cmd = GetAllCommandForWMI_118();
		SetEnergySaveInfo2EC(cmd);
		EnableEnergySave(bEnable: true);
		SetSaveModeList2Ini();
		SetSelectProfile();
		SetEnergySaveStatus();
		MessageBox.Show("Save Completed");
	}

	private void Image_EnergySave_charge_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		isChargePressed = true;
	}

	private void Image_EnergySave_charge_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		isChargePressed = false;
	}

	private void Image_EnergySave_discharge_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		isDischargePressed = true;
	}

	private void Image_EnergySave_discharge_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		isDischargePressed = false;
	}

	private int GetSaveModeCount()
	{
		return ListBox_Save_Mode.Items.Count;
	}

	private void AddSaveModeItem(string content)
	{
		ListBoxItem val = new ListBoxItem();
		val.Content = content;
		val.FontSize = 20.0;
		SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
		val.Foreground = foreground;
		val.Height = 68.0;
		val.Width = 312.0;
		val.HorizontalAlignment = HorizontalAlignment.Center;
		val.HorizontalContentAlignment = HorizontalAlignment.Center;
		val.Theme = (Avalonia.Styling.ControlTheme)this.FindResource("ModeItem");
		ListBox_Save_Mode.Items.Add(val);
	}

	private void DeleteSaveModeItem()
	{
		if (ListBox_Save_Mode.Items.Count == 1)
		{
			MessageBox.Show("Last Save Mode");
			return;
		}
		SaveModeList.RemoveAt(SelectItem);
		ListBox_Save_Mode.Items.RemoveAt(ListBox_Save_Mode.Items.IndexOf(ListBox_Save_Mode.SelectedItem));
	}

	private void Image_EnergySave_delete_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		DeleteSaveModeItem();
	}

	private void ListBox_Save_Mode_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		int count = ListBox_Save_Mode.Items.Count;
		for (int i = 0; i < count; i++)
		{
			ListBoxItem val = (ListBoxItem)ListBox_Save_Mode.Items[i];
			if (val != null)
			{
				SolidColorBrush val2 = null;
				val2 = ((val != ListBox_Save_Mode.SelectedItem) ? new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue)) : new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, byte.MaxValue, (byte)237)));
				val.Foreground = val2;
			}
		}
		ListBoxItem val3 = (ListBoxItem)ListBox_Save_Mode.SelectedItem;
		if (val3 == null)
		{
			SelectItem = ListBox_Save_Mode.Items.Count - 1;
			setSaveModeSelect(SelectItem);
			val3 = (ListBoxItem)ListBox_Save_Mode.SelectedItem;
			if (val3 == null)
			{
				SelectItem = 0;
				setSaveModeSelect(SelectItem);
				val3 = (ListBoxItem)ListBox_Save_Mode.SelectedItem;
			}
		}
		else
		{
			SelectItem = ListBox_Save_Mode.Items.IndexOf(ListBox_Save_Mode.SelectedItem);
		}
		UpdateEnergySaveUI();
	}

	private void setSaveModeSelect(int pos)
	{
		int count = ListBox_Save_Mode.Items.Count;
		for (int i = 0; i < count; i++)
		{
			object obj = ListBox_Save_Mode.Items[i];
			ListBoxItem val = (ListBoxItem)((obj is ListBoxItem) ? obj : null);
			if (val != null && i == pos)
			{
				val.IsSelected = true;
				SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, byte.MaxValue, (byte)237));
				val.Foreground = foreground;
			}
		}
	}

	private void Image_EnergySave_add_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		string text = "Mode " + (ListBox_Save_Mode.Items.Count + 1);
		AddNew2SaveModeList(text);
		AddSaveModeItem(text);
	}

	private void Image_EnergySave_rename_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		RenameItem = SelectItem;
		TextBox_Rename.IsVisible = true;
		Btn_Rename.IsVisible = true;
		Btn_Cancel.IsVisible = true;
		grid_Disable_background.IsVisible = true;
		Image_EnergySave_lock.IsEnabled = false;
		object obj = ListBox_Save_Mode.Items[RenameItem];
		ListBoxItem val = (ListBoxItem)((obj is ListBoxItem) ? obj : null);
		TextBox_Rename.Text = val.Content.ToString();
	}

	private void Yes_Click(object sender, RoutedEventArgs e)
	{
		TextBox_Rename.IsVisible = false;
		Btn_Rename.IsVisible = false;
		Btn_Cancel.IsVisible = false;
		grid_Disable_background.IsVisible = false;
		Image_EnergySave_lock.IsEnabled = true;
		string text = TextBox_Rename.Text;
		ListBoxItem val = (ListBoxItem)ListBox_Save_Mode.SelectedItem;
		val.Content = text;
		SaveModeList[SelectItem].Name = text;
	}

	private void No_Click(object sender, RoutedEventArgs e)
	{
		TextBox_Rename.IsVisible = false;
		Btn_Rename.IsVisible = false;
		Btn_Cancel.IsVisible = false;
		grid_Disable_background.IsVisible = false;
		Image_EnergySave_lock.IsEnabled = true;
	}

	private void LockEnergySave()
	{
		Bitmap val = null;
		if (isLock)
		{
			val = Img.Load("Assets/btn_lock_normal.png");
			Image_EnergySave_lock.Source = val;
			grid_Disable_background.IsVisible = true;
		}
		else
		{
			val = Img.Load("Assets/btn_unlock_normal.png");
			Image_EnergySave_lock.Source = val;
			grid_Disable_background.IsVisible = false;
		}
	}

	private void Init_INIFile()
	{
		ini = new IniFile("EnergySave.ini", "C:\\ComboKey");
	}

	private int GetSaveModeCountFromIni()
	{
		SaveModeCount = Convert.ToInt32(ini.Read("Count", "MainSetting"));
		return SaveModeCount;
	}

	private int GetSelectProfile()
	{
		SelectItem = Convert.ToInt32(ini.Read("SelectProfile", "MainSetting"));
		if (SelectItem < 0)
		{
			SelectItem = 0;
		}
		return SelectItem;
	}

	private void SetSelectProfile()
	{
		ini.Write("SelectProfile", SelectItem.ToString(), "MainSetting");
	}

	private void GetEnergySaveStatus()
	{
		isUtilityStart = Convert.ToBoolean(ini.Read("EnergySaveStart", "MainSetting"));
	}

	private void SetEnergySaveStatus()
	{
		ini.Write("EnergySaveStart", Convert.ToString(isUtilityStart), "MainSetting");
	}

	private void GetSaveModeListFromIni()
	{
		SaveModeList.Clear();
		for (int i = 0; i < SaveModeCount; i++)
		{
			string text = "profile_" + i;
			EnergySaveInfo energySaveInfo = new EnergySaveInfo();
			energySaveInfo.Name = ini.Read(text, "MainSetting");
			energySaveInfo.isSchedule1_Enable = Convert.ToBoolean(ini.Read("Schedule1_Enable", text));
			energySaveInfo.isSchedule2_Enable = Convert.ToBoolean(ini.Read("Schedule2_Enable", text));
			energySaveInfo.Schedule1_Start = Convert.ToInt32(ini.Read("Schedule1_Start", text));
			energySaveInfo.Schedule1_End = Convert.ToInt32(ini.Read("Schedule1_End", text));
			energySaveInfo.Schedule2_Start = Convert.ToInt32(ini.Read("Schedule2_Start", text));
			energySaveInfo.Schedule2_End = Convert.ToInt32(ini.Read("Schedule2_End", text));
			energySaveInfo.isSunday = Convert.ToBoolean(ini.Read("SUN", text));
			energySaveInfo.isMonday = Convert.ToBoolean(ini.Read("MON", text));
			energySaveInfo.isTuesday = Convert.ToBoolean(ini.Read("TUE", text));
			energySaveInfo.isWednesday = Convert.ToBoolean(ini.Read("WED", text));
			energySaveInfo.isThursday = Convert.ToBoolean(ini.Read("THU", text));
			energySaveInfo.isFriday = Convert.ToBoolean(ini.Read("FRI", text));
			energySaveInfo.isSaturday = Convert.ToBoolean(ini.Read("SAT", text));
			energySaveInfo.isEveryday = Convert.ToBoolean(ini.Read("Everyday", text));
			energySaveInfo.isBatterySettingAuto = Convert.ToBoolean(ini.Read("AUTO", text));
			energySaveInfo.ChargeLimit = Convert.ToInt32(ini.Read("ChargeLimit", text));
			energySaveInfo.DischargeLimit = Convert.ToInt32(ini.Read("DischargeLimit", text));
			SaveModeList.Add(energySaveInfo);
		}
	}

	private void SetSaveModeList2Ini()
	{
		ini.Write("Count", SaveModeList.Count.ToString(), "MainSetting");
		for (int i = 0; i < SaveModeList.Count; i++)
		{
			string text = "profile_" + i;
			ini.Write(text, SaveModeList[i].Name, "MainSetting");
			ini.Write("Schedule1_Enable", Convert.ToString(SaveModeList[i].isSchedule1_Enable), text);
			ini.Write("Schedule2_Enable", Convert.ToString(SaveModeList[i].isSchedule2_Enable), text);
			ini.Write("Schedule1_Start", Convert.ToString(SaveModeList[i].Schedule1_Start), text);
			ini.Write("Schedule1_End", Convert.ToString(SaveModeList[i].Schedule1_End), text);
			ini.Write("Schedule2_Start", Convert.ToString(SaveModeList[i].Schedule2_Start), text);
			ini.Write("Schedule2_End", Convert.ToString(SaveModeList[i].Schedule2_End), text);
			ini.Write("SUN", Convert.ToString(SaveModeList[i].isSunday), text);
			ini.Write("MON", Convert.ToString(SaveModeList[i].isMonday), text);
			ini.Write("TUE", Convert.ToString(SaveModeList[i].isTuesday), text);
			ini.Write("WED", Convert.ToString(SaveModeList[i].isWednesday), text);
			ini.Write("THU", Convert.ToString(SaveModeList[i].isThursday), text);
			ini.Write("FRI", Convert.ToString(SaveModeList[i].isFriday), text);
			ini.Write("SAT", Convert.ToString(SaveModeList[i].isSaturday), text);
			ini.Write("AUTO", Convert.ToString(SaveModeList[i].isBatterySettingAuto), text);
			ini.Write("ChargeLimit", Convert.ToString(SaveModeList[i].ChargeLimit), text);
			ini.Write("DischargeLimit", Convert.ToString(SaveModeList[i].DischargeLimit), text);
			ini.Write("Everyday", Convert.ToString(SaveModeList[i].isEveryday), text);
		}
	}

	private void AddNew2SaveModeList(string name)
	{
		EnergySaveInfo energySaveInfo = new EnergySaveInfo();
		energySaveInfo.Name = name;
		energySaveInfo.isSchedule1_Enable = true;
		energySaveInfo.isSchedule2_Enable = true;
		energySaveInfo.Schedule1_Start = 0;
		energySaveInfo.Schedule1_End = 1;
		energySaveInfo.Schedule2_Start = 0;
		energySaveInfo.Schedule2_End = 1;
		energySaveInfo.isSunday = false;
		energySaveInfo.isMonday = false;
		energySaveInfo.isTuesday = false;
		energySaveInfo.isWednesday = false;
		energySaveInfo.isThursday = false;
		energySaveInfo.isFriday = false;
		energySaveInfo.isSaturday = false;
		energySaveInfo.isBatterySettingAuto = true;
		energySaveInfo.ChargeLimit = 100;
		energySaveInfo.DischargeLimit = 40;
		SaveModeList.Add(energySaveInfo);
	}

	public void SetWattLimit(int wattLimit)
	{
		Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\EnergySave", "WattLimit", wattLimit);
	}

	public int GetWattLimit()
	{
		int num = Convert.ToInt16(Reg.GetValue("HKEY_CURRENT_USER\\Software\\ControlCenter3.0\\EnergySave", "WattLimit", 250));
		if (ClevoID == L140CU)
		{
			if (num > 100 || num < 50)
			{
				num = 100;
			}
		}
		else if (num > 250 || num < 50)
		{
			num = 250;
		}
		return num;
	}

	private void ZoomOut_Click(object sender, PointerPressedEventArgs e)
	{
		WattLimit += 50;
		if (ClevoID == L140CU)
		{
			if (WattLimit > 100)
			{
				WattLimit = 100;
			}
		}
		else if (WattLimit > 250)
		{
			WattLimit = 250;
		}
		mainChart.YMaximum = WattLimit; mainChart.InvalidateVisual();
		SetWattLimit(WattLimit);
	}

	private void ZoomIn_Click(object sender, PointerPressedEventArgs e)
	{
		WattLimit -= 50;
		if (WattLimit < 50)
		{
			WattLimit = 50;
		}
		mainChart.YMaximum = WattLimit; mainChart.InvalidateVisual();
		SetWattLimit(WattLimit);
	}

	private bool CheckScheduleOverlapOrNot()
	{
		bool result = false;
		int schedule1_Start = SaveModeList[SelectItem].Schedule1_Start;
		int num = SaveModeList[SelectItem].Schedule1_End + 1;
		int schedule2_Start = SaveModeList[SelectItem].Schedule2_Start;
		int num2 = SaveModeList[SelectItem].Schedule2_End + 1;
		if (SaveModeList[SelectItem].isSchedule1_Enable && SaveModeList[SelectItem].isSchedule2_Enable)
		{
			result = (schedule1_Start - num2 <= 0 || num - schedule2_Start <= 0) && ((schedule1_Start - num2 >= 0 || num - schedule2_Start >= 0) ? true : false);
		}
		return result;
	}


	// ---- screenshot support: --page NAME ----
	private void ApplyPageArg()
	{
		string p = AppHost.PageArg ?? "";
		bool data = p.EndsWith("-data");
		if (data) p = p.Substring(0, p.Length - 5);
		Down0 = null;
		if (data) FillFake();
		switch (p)
		{
			case "energysave": GoEnergySave(); break;
			case "unlocked": GoEnergySave(); Image_EnergySave_lock_MouseLeftButtonDown(null, null); break;
			case "manual": GoEnergySave(); Image_EnergySave_lock_MouseLeftButtonDown(null, null); Image_EnergySave_manually_MouseLeftButtonDown(null, null); break;
			case "overlap":
				GoEnergySave(); Image_EnergySave_lock_MouseLeftButtonDown(null, null);
				SaveModeList[SelectItem].Schedule1_Start = 2; SaveModeList[SelectItem].Schedule1_End = 8;
				SaveModeList[SelectItem].Schedule2_Start = 6; SaveModeList[SelectItem].Schedule2_End = 14;
				UpdateEnergySaveUI(); Label_EnergySave_Warning.IsVisible = true; Image_EnergySave_save.IsEnabled = false;
				Image_EnergySave_save.Source = Img.Load("Assets/btn_save_pressed.png"); Label_EnergySave_save.Foreground = new SolidColorBrush(Color.FromRgb(159, 159, 159)); break;
			case "rename": GoEnergySave(); Image_EnergySave_lock_MouseLeftButtonDown(null, null); Image_EnergySave_rename_MouseLeftButtonDown(null, null); break;
			case "minute": Image_Minute_MouseLeftButtonDown(null, null); break;
			case "fullscreen": Button_Click_Window(null, null); break;
			case "msg-save": MessageBox.Show("Save Completed"); break;
			case "msg-overlap": MessageBox.Show("Two Schedules should not be overlapped. At least an hour gap between two schedules."); break;
			case "msg-last": MessageBox.Show("Last Save Mode"); break;
		}
	}
	private object Down0;
	private void GoEnergySave() { Image_EnergySave_MouseLeftButtonDown(null, null); Image_EnergySave_MouseLeftButtonUp(null, null); }
	private void FillFake()
	{
		dt.Clear();
		for (int i = 0; i < 60; i++)
		{
			var r = dt.NewRow();
			double v = 35 + 25 * Math.Sin(i / 6.0) + 10 * Math.Sin(i / 2.3);
			if (i < 40) { r["AC"] = v; r["DC"] = 0; } else { r["AC"] = 0; r["DC"] = v; }
			dt.Rows.Add(r);
		}
		mainChart.DataBind(dt);
		double w = 20.8;
		double top = 485.0 - w * 330.0 / WattLimit;
		Canvas.SetTop(Image_ConsumptionCurrentValue, top); Canvas.SetTop(Label_ConsumptionCurrentValue, top - 8.0);
		Label_ConsumptionCurrentValue.Content = $"{w:#,0.#}" + " W";
		fakeData = true;
	}
}
