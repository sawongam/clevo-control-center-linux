using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Threading;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Clevo;
using FnKey.Device;
using CCenter.Platform;

namespace FnKey;

// WinForms-compatible Timer facade (original code uses System.Windows.Forms.Timer: Interval in ms, Start/Stop/Enabled, Tick).
public class Timer
{
	private readonly DispatcherTimer t = new DispatcherTimer();
	private int ms = 100;
	public event EventHandler Tick;
	public Timer() { t.Tick += (s, e) => Tick?.Invoke(this, EventArgs.Empty); }
	public int Interval { get { return ms; } set { ms = value; t.Interval = TimeSpan.FromMilliseconds(value); } }
	public bool Enabled { get { return t.IsEnabled; } set { if (value) Start(); else Stop(); } }
	public void Start() { t.Interval = TimeSpan.FromMilliseconds(ms); t.Stop(); t.Start(); }
	public void Stop() { t.Stop(); }
}

// Port of FnKey.Form1 minus the WinForms/tray UI: the hot-key controller.
// Input: Handle("Arg001^<notify value>") exactly like the original outlog_EntryWritten text protocol.
public class Form1
{
	private OsdForm osd;

	private AirplaneMode airplane;

	private int Mode;

	private int NextMode = 1;

	private bool IgnoreDCHUEvent;

	private bool After_RestartResume;

	private bool After_Resume;

	public Timer ResumeDelayTimer = new Timer();

	public Timer IgnoreEventTimer = new Timer();

	public Timer UpdateStatusTimer = new Timer();

	public Timer NHxxAC_AF_DelayTimer = new Timer();

	public Timer DoResume = new Timer();

	public static int nLastEvent = 0;

	public static bool bIsRyzenMasterStart = false;

	public static bool bIsGPUTurboOn = false;

	public static Guid guidBalance = new Guid("381b4222-f694-41f0-9685-ff5bb260df2e");

	public static Guid guidPowerSaver = new Guid("a1841308-3541-4fab-bc81-f71556f20b4a");

	public static Guid guidHighPerformance = new Guid("8c5e7fda-e8bf-4a96-9a85-a6e23a8c635c");

	private Timer acPoll = new Timer();

	private Timer XTU_timer = new Timer();

	public Form1(OsdForm osdForm)
	{
		osd = osdForm;
	}

	/// <summary>Replaces Form1_Load + OnStart. skipInit = no DCHU writes at startup (OSD/event handling only).</summary>
	public void Start(bool skipInit)
	{
		ResumeDelayTimer.Interval = 2000;
		ResumeDelayTimer.Tick += ResumeDelayTimer_Tick;
		IgnoreEventTimer.Interval = 2000;
		IgnoreEventTimer.Tick += IgnoreEventTimer_Tick;
		UpdateStatusTimer.Interval = 1000;
		UpdateStatusTimer.Tick += UpdateStatusTimer_Tick;
		NHxxAC_AF_DelayTimer.Interval = 5000;
		NHxxAC_AF_DelayTimer.Tick += NHxxAC_AF_DelayTimer_Tick;
		DoResume.Interval = 3000;
		DoResume.Tick += DoResume_Tick;
		if (skipInit)
		{
			Global.fnKeyStatus = Global.FnKeyStatus.Ready;   // no SetupMenuItem: AppSettings may be uninitialised (ShowOSD=0) without OnFirstRun
			return;
		}
		try { Form1_Load(); } catch (Exception ex) { DebugLog.WriteDbgLog2Outlog("Form1_Load error: " + ex.Message); Global.fnKeyStatus = Global.FnKeyStatus.Ready; }
		// SystemEvents.PowerModeChanged(StatusChange) replacement: poll AC state
		acPoll.Interval = 2000;
		acPoll.Tick += (s, e) => { Global.BAT_PowerStatus.UpdatePowerInfo(); if (Global.BAT_PowerStatus.PowerLineStatus != Global.BAT_PowerStatus.OLD_PowerLineStatus && Global.BAT_PowerStatus.PowerLineStatus != PowerLineStatus.Unknown) OnPowerLineChanged(); };
		acPoll.Start();
	}

	private void Form1_Load()
	{
		Global.features.RegisterOOBENotification();
		Global.features.ClevoID = CallingVariations.GetProductID_PCI();
		DebugLog.WriteDbgLog2Outlog("ClevoID: " + Global.features.ClevoID.ToString("x"));
		byte[] bytes = BitConverter.GetBytes(Global.features.ClevoID);
		byte b = bytes[0];
		bytes[0] = bytes[1];
		bytes[1] = b;
		Global.dchu.WriteAppSettings(0, 0, 2, bytes);
		Global.features.AMD_CPU_Check();
		Version version = Assembly.GetExecutingAssembly().GetName().Version;
		byte[] array = Global.dchu.ReadAppSettings(0, 8, 4);
		Global.features.DoWMIFlag(70, Global.dchu.GetWMI(70));
		if (array[0] != version.Major || array[1] != version.Minor || array[2] != version.Build || array[3] != version.Revision || Global.features.LoadDefault)
		{
			CallingVariations.Set_ASPM(2);
			DebugLog.WriteDbgLog2Inlog("restore PCI-E ASPM Setting");
			if ((array[0] == 0 && array[1] == 0 && array[2] == 0 && array[3] == 0) || Global.RW_REG.IsFirstRun.Equals(0) || Global.features.LoadDefault)
			{
				if (AllowFirstRun) { OnFirstRun(); Global.KB.FirstRun(); } else DebugLog.WriteDbgLog2Outlog("first run skipped (opt-in: --first-run or FNKEY_FIRST_RUN=1)");
			}
			Global.RW_REG.IsFirstRun = 1;
			Global.gpu.FirstRun = true;
		}
		DebugLog.WriteDbgLog2Outlog("Fnkey Version: " + version);
		array = new byte[4] { (byte)version.Major, (byte)version.Minor, (byte)version.Build, (byte)version.Revision };
		Global.dchu.WriteAppSettings(0, 8, 4, array);
		airplane = new AirplaneMode();
		OnStart();
		Global.BAT_PowerStatus.UpdatePowerInfo();
		Global.BAT_PowerStatus.OLD_PowerLineStatus = Global.BAT_PowerStatus.PowerLineStatus;
		Global.features.Log_Battery_PowerStatus();
		Global.features.WMIDeviceChangeDetect();
	}

	private void OnStart()
	{
		try
		{
			Global.refreshRateStatus = Global.RefreshStatus.OnStart;
			Global.fnKeyStatus = Global.FnKeyStatus.Init;
			DebugLog.WriteDbgLog2Outlog("OnStart");
			Global.features.GetInfo();
			Global.features.Get_Features_list();
			Global.features.GetCustomerID();
			Global.features.CheckExecuteCOUWP();
			if (Global.features.CustomerID == 5)
			{
				Global.features.SupportBATLowControl = true;
			}
			switch (Global.features.ClevoID)
			{
			case 20724:
				Global.features.InstallAMDDriver();
				Global.features.AMD_init_2_0();
				break;
			case 20720:
				Global.features.InstallAMDDriver();
				Global.features.AMDManualModeLoadDefault();
				Global.features.AMD_init();
				break;
			case 20897:
			case 20898:
				Notify.ToService("w_arg254^cc30wk.exe -utp");
				if (Global.features.CustomerID == 5)
				{
					Global.DevEvent.Support_EPSON_Workaround_USB_Buffalo();
				}
				break;
			case 16464:
			case 16577:
			case 16593:
			case 16720:
			case 16721:
				Notify.ToService("w_arg254^ImeMode.exe");
				break;
			case 42240:
			case 42241:
				if (Global.features.SupportBIOSFeatureList_Version == 0)
				{
					Notify.ToService("w_arg254^cc30wk.exe -dlp");
				}
				break;
			case 5121:
				if (Global.features.CustomerID == 5)
				{
					CallingVariations.SetModernStandbyConnectivityInStandby(0);
				}
				break;
			case 26085:
			case 26087:
			case 26597:
			case 26599:
				if (Global.features.SupportBIOSFeatureList_Version == 0)
				{
					CallingVariations.SetVideoTimeoutAC(1, 0);
					CallingVariations.SetVideoTimeoutDC(1, 0);
				}
				break;
			}
			Global.features.BatteryLowControl_Check();
			if (Global.features.SupportDisplayTimeNever)
			{
				CallingVariations.SetVideoTimeoutAC(1, 0);
				CallingVariations.SetVideoTimeoutDC(1, 0);
			}
			if (Global.features.SupportMEPowerGatingDisable) Notify.ToService("w_arg254^ME_PG_DISABLED.exe");
			if (Global.features.DisableLAN_PowerSavingMode) Notify.ToService("w_arg254^cc30wk.exe -dlp");
			if (Global.features.SupportRescanPS2Mouse) Notify.ToService("w_arg254^cc30wk.exe -fpm");
			if (Global.features.SupportPCI5Enable) Notify.ToService("w_arg254^PCI5X17_R.exe");
			if (Global.features.SupportSetDeviceSavePower) Notify.ToService("w_arg254^MSPower_Device.exe");
			if (Global.features.SupportRemoveHDRLimit) Notify.ToService("w_arg254^cc30wk.exe -rdl");
			if (Global.features.SupportMovieTVnoise || Global.features.SupportMovieTVnoise_ASPM) Notify.ToService("w_arg254^MovieRuntime.exe");
			if (Global.features.SupportDisable_PgUpDnKey) Global.features.Load_PgUpDnKey();
			if (Global.features.SupportRebootRescanCardReader) Notify.ToService("w_arg254^cc30wk.exe -rc");
			if (Global.features.SupportCheckRealtekLANSavePower) Global.features.CheckRealtekLANSavePower();
			Global.features.GetDCModePowerMode();
			Global.features.CheckI2C_TP();
			Global.features.CheckModernStandbySupport();
			Global.fan.Load_FanMode_Info();
			Global.gpu.Init_GPU();
			Global.features.SetCCD(Global.features.GetCCD());
			Global.features.SetCC30_APP_Launch(0);
			Global.features.NvsrSolutionCheck();
			Global.features.OLED_Panel_Check();
			SMBIOS sMBIOS = new SMBIOS();
			Global.features.System_ManufacturerName = sMBIOS.System.ManufacturerName;
			DebugLog.WriteDbgLog2Outlog("OnStart, KB.Init");
			Global.KB.Init();
			if (Global.features.SupportFlexikey)
			{
				Global.flexikey.Set_Flexikey_kb_status(0);
				Global.flexikey.Set_Flexikey_mouse_status(0);
				Global.features.FlexikeyInit();
				Global.features.SetKeybordMouseEnble(IsEnable: false);
			}
			Global.features.Check_Arg001_byDCHUServiceSend();
			if (Global.features.SupportEnergySave) Global.features.EnergySaveInit();
			After_RestartResume = true;
			SetupMenuItem();
			Global.features.CheckIfSupportLowRefreshRate();
			Global.BAT_PowerStatus.UpdatePowerInfo();
			if (Global.features.DCMode_PowerMode != 0)
			{
				if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
				{
					ChangePowerMode(CleDef.ModeType.pwrsaving);
				}
				else
				{
					ChangePowerMode((CleDef.ModeType)Global.features.ACMode_PowerMode);
				}
			}
			else
			{
				ChangePowerMode((CleDef.ModeType)Global.features.GetPowerMode());
			}
			Global.fnKeyStatus = Global.FnKeyStatus.Ready;
			DebugLog.WriteDbgLog2Outlog("OnReady");
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("OnStartError");
		}
		Global.fnKeyStatus = Global.FnKeyStatus.Ready;
		Notify.ToService("w_arg255^gfe_start.bat");
	}

	// Tray-menu state sync: only the OSD on/off flag matters without a tray (ShowOSD app setting, page 1 off 3).
	private void SetupMenuItem()
	{
		osd.IsSupportOSD = Global.features.GetShowOSD() == 1;
	}

	/// <summary>Tray menu "OSD" toggle.</summary>
	public void ToggleOsdSetting()
	{
		if (Global.features.GetShowOSD() == 0) { Global.features.SetShowOSD(1); osd.IsSupportOSD = true; }
		else { Global.features.SetShowOSD(0); osd.IsSupportOSD = false; }
	}

	public void Mode_click(CleDef.ModeType modeType)
	{
		Global.refreshRateStatus = Global.RefreshStatus.PowerModeChange;
		Global.features.ACMode_PowerMode = (byte)modeType;
		ChangePowerMode(modeType);
	}

	private void ChangePowerMode(CleDef.ModeType type)
	{
		new Thread(OnChangePowerMode).Start(type);
	}

	private void Change_Notify_Icon(CleDef.ModeType type) { }

	private void UpdatePowerModeUI(byte amdMode)
	{
		Global.features.SupportPowerMode = amdMode != 1;
	}

	private void SuspendState(int kind) { DebugLog.WriteDbgLog2Outlog("STUB SetSuspendState(" + kind + ") - logind handles lid/suspend on Linux"); }

	private void LaunchApp(string appName)
	{
		if (appName.ToLower().Contains("cc30")) AppHost.LaunchSibling("ControlCenter30");
	}

	private void DefaultLaunch(string uri)
	{
		if (uri.StartsWith("clevokeyboard")) AppHost.LaunchSibling("LedKeyboardSetting");
		else if (uri.StartsWith("clevocc30")) AppHost.LaunchSibling("ControlCenter30");
	}

	/// <summary>Public entry: one notify value from the firmware (low byte = event, bytes above = data).</summary>
	public void HandleNotify(int value) => Handle("Arg001^" + value);

	public void Handle(string message)
	{
		Dispatcher.UIThread.Invoke(() => HandleMessage(message));
	}

	public void Sleep() => OnSleep();

	public void Resume() => OnResume();

	public void Close()
	{
		DebugLog.WriteDbgLog2Outlog("OnClosing");
		Global.KB.UpdateCurrentOverrideBootEffect();
		Global.KB.SetOverrideBootEffect(Global.KB.OverrideBootEffect);
		(Avalonia.Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Shutdown();
	}


	private void HandleMessage(string message)
	{
		Dispatcher.UIThread.Invoke((Action)(() =>
		{
			if (message.Equals("Set AMD OK"))
			{
				DebugLog.WriteDbgLog2Outlog("Received Set AMD OK, nLastEvent = 0");
				nLastEvent = 0;
			}
			string[] array = message.Split(new char[1] { '^' });
			if (array.Length != 1)
			{
				if (array[0] == "clevokeyboard")
				{
					string text = array[1];
					if (!(text == "201") && text == "202")
					{
						if (array[2] == "5")
						{
							Global.KB.Mode = 5;
							Global.KB.UpdatePerkeyRippleColor();
							Global.KB.UpdatePerkeyRippleStatus();
							Global.KB.StartHook(status: true);
						}
						else if (array[2] == "11")
						{
							Global.KB.Mode = 11;
							Global.KB.UpdatePerkeyFnColor();
							Global.KB.StartHook(status: true);
						}
						else
						{
							Global.KB.StartHook(status: false);
						}
					}
				}
				else if (array[0] == "clevocc30")
				{
					switch (array[1])
					{
					case "101":
						try
						{
							Global.refreshRateStatus = Global.RefreshStatus.PowerModeChange;
							byte b = Convert.ToByte(array[2]);
							Global.features.ACMode_PowerMode = b;
							ChangePowerMode((CleDef.ModeType)b);
							break;
						}
						catch
						{
							break;
						}
					case "107":
						IgnoreDCHUEvent = false;
						IgnoreEventTimer.Stop();
						break;
					case "10f":
					{
						byte amdMode = Convert.ToByte(array[2]);
						UpdatePowerModeUI(amdMode);
						break;
					}
					case "114":
						Global.features.IsLowRefreshRateChecked = Global.features.ReadLowRefreshRateCheckedFlag();
						Global.refreshRateStatus = Global.RefreshStatus.UI_Check_Event;
						Global.features.SetPanelRefreshRate(-1);
						break;
					}
				}
				else if (array[0] == "clevobattery")
				{
					if (array[1] == "207")
					{
						if (array[2] == "1")
						{
							DebugLog.WriteDbgLog2Outlog("BatteryUtilityRefresh=  ture");
							Global.features.BatteryUtilityRefresh = true;
						}
						else
						{
							DebugLog.WriteDbgLog2Outlog("BatteryUtilityRefresh=  false");
							Global.features.BatteryUtilityRefresh = false;
							DebugLog.WriteDbgLog2Outlog("EnergySave Init");
							Global.features.EnergySaveInit();
						}
					}
				}
				else if (array[0] == "clevoenergy")
				{
					if (array[1] == "208" && Global.features.SupportEnergySave)
					{
						Global.features.EnergySaveInit();
					}
				}
				else if (array[0] == "dchu")
				{
					switch (array[1])
					{
					case "10e":
						try
						{
							DebugLog.WriteDbgLog2Outlog("DCHU Service notify FnKey Close");
							Close();
							break;
						}
						catch (Exception ex)
						{
							DebugLog.WriteDbgLog2Outlog("DCHU Service notify FnKey Close, exception:" + ex.Message);
							break;
						}
					case "110":
						Global.features.Check_Arg001_byDCHUServiceSend();
						break;
					case "111":
						Global.features.Check_Arg001_byDCHUServiceSend();
						break;
					}
				}
				else if (array[0] == "Arg001")
				{
					int num = Convert.ToInt32(array[1]) & 0xFF;
					int e_Data = Convert.ToInt32(array[1]) >> 8;
					Arg001_GetEvent(num, e_Data);
				}
			}
		}));
	}


	private void Arg001_GetEvent(int Event, int E_Data)
	{
		try
		{
			Dispatcher.UIThread.Invoke((Action)(() =>
			{
				switch (Event)
				{
				case 65:
					DebugLog.WriteDbgLog2Outlog("Get EC PROC_HOT");
					if (nLastEvent == 0)
					{
						nLastEvent = 65;
						NHxxAC_AF_DelayTimer.Interval = 10;
						NHxxAC_AF_DelayTimer.Start();
						DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer.Interval= 10ms");
					}
					else if (nLastEvent == 65)
					{
						DebugLog.WriteDbgLog2Outlog("Same QEvent");
					}
					else
					{
						nLastEvent = 65;
						NHxxAC_AF_DelayTimer.Stop();
						NHxxAC_AF_DelayTimer.Interval = 5000;
						NHxxAC_AF_DelayTimer.Start();
						DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer.Interval= 5s");
					}
					DebugLog.WriteDbgLog2Outlog("nLastEvent=" + nLastEvent + ", NHxxAC_AF_DelayTimer.Enabled=" + NHxxAC_AF_DelayTimer.Enabled);
					if (Global.features.ClevoID == 20720 || (Global.features.ClevoID == 20724 && bIsGPUTurboOn))
					{
						Global.gpu.GPU_Turbo_off();
						bIsGPUTurboOn = false;
						DebugLog.WriteDbgLog2Outlog("GPU Turbo off");
					}
					break;
				case 66:
					Global.features.AMD_TotalPower_Limit = false;
					if (nLastEvent == 0)
					{
						nLastEvent = 66;
						NHxxAC_AF_DelayTimer.Interval = 10;
						NHxxAC_AF_DelayTimer.Start();
						DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer.Interval= 10ms");
					}
					else if (nLastEvent == 66)
					{
						DebugLog.WriteDbgLog2Outlog("Same QEvent");
					}
					else
					{
						nLastEvent = 66;
						NHxxAC_AF_DelayTimer.Stop();
						NHxxAC_AF_DelayTimer.Interval = 5000;
						NHxxAC_AF_DelayTimer.Start();
						DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer.Interval= 5s");
					}
					DebugLog.WriteDbgLog2Outlog("nLastEvent=" + nLastEvent + ", NHxxAC_AF_DelayTimer.Enabled=" + NHxxAC_AF_DelayTimer.Enabled);
					if (Global.features.ClevoID == 20720 || (Global.features.ClevoID == 20724 && !bIsGPUTurboOn))
					{
						Global.gpu.Load_GPU_forFn_3(3);
						bIsGPUTurboOn = true;
						DebugLog.WriteDbgLog2Outlog("GPU Turbo on");
					}
					break;
				case 67:
					Global.features.AMD_Thermal_Limit = false;
					if (nLastEvent == 0)
					{
						nLastEvent = 67;
						NHxxAC_AF_DelayTimer.Interval = 10;
						NHxxAC_AF_DelayTimer.Start();
						DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer.Interval= 10ms");
					}
					else if (nLastEvent == 67)
					{
						DebugLog.WriteDbgLog2Outlog("Same QEvent");
					}
					else
					{
						nLastEvent = 67;
						NHxxAC_AF_DelayTimer.Stop();
						NHxxAC_AF_DelayTimer.Interval = 5000;
						NHxxAC_AF_DelayTimer.Start();
						DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer.Interval= 5s");
					}
					DebugLog.WriteDbgLog2Outlog("nLastEvent=" + nLastEvent + ", NHxxAC_AF_DelayTimer.Enabled=" + NHxxAC_AF_DelayTimer.Enabled);
					break;
				case 68:
					nLastEvent = 68;
					NHxxAC_AF_DelayTimer.Stop();
					NHxxAC_AF_DelayTimer.Interval = 10;
					NHxxAC_AF_DelayTimer.Start();
					DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer.Interval= 10ms, nLastEvent = " + nLastEvent + ", NHxxAC_AF_DelayTimer.Enabled=" + NHxxAC_AF_DelayTimer.Enabled);
					break;
				case 69:
					Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.TotalPower_1);
					Global.features.AMD_TotalPower_Limit = true;
					DebugLog.WriteDbgLog2Outlog("Get TotoalPowerLimit");
					break;
				case 70:
					if (Global.features.GetPowerMode() == 3)
					{
						Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.Entertainment_Thermal_1);
					}
					break;
				case 71:
					if (Global.features.GetPowerMode() == 3)
					{
						Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.Entertainment_Thermal_2);
					}
					break;
				case 89:
					Global.refreshRateStatus = Global.RefreshStatus.PowerModeChange;
					if (!IgnoreDCHUEvent && Global.fnKeyStatus == Global.FnKeyStatus.Ready && Global.features.SupportPowerMode && (Global.features.ClevoID != 20720 || Global.features.GetAMDManualMode() != 1))
					{
						Mode = NextMode;
						Global.features.ACMode_PowerMode = Mode;
						switch (Mode)
						{
						case 0:
							osd.ShowOsd(OsdForm.OsdImage.osd_quiet2);
							ChangePowerMode(CleDef.ModeType.quiet);
							break;
						case 1:
							osd.ShowOsd(OsdForm.OsdImage.osd_powerSaving2);
							ChangePowerMode(CleDef.ModeType.pwrsaving);
							break;
						case 2:
							if (Global.features.System_ManufacturerName.Contains("MEDION"))
							{
								osd.ShowOsd(OsdForm.OsdImage.osd_Gaming);
							}
							else
							{
								osd.ShowOsd(OsdForm.OsdImage.osd_peformance2);
							}
							ChangePowerMode(CleDef.ModeType.performance);
							break;
						case 3:
							if (Global.features.SupportPowerModeBit && (Global.features.SupportPowerModeUI_ID == 1 || Global.features.SupportPowerModeUI_ID == 2))
							{
								osd.ShowOsd(OsdForm.OsdImage.osd_balanced);
							}
							else
							{
								osd.ShowOsd(OsdForm.OsdImage.osd_entetainment2);
							}
							ChangePowerMode(CleDef.ModeType.entertainment);
							break;
						}
						IgnoreDCHUEvent = true;
						IgnoreEventTimer.Interval = 1000;
						IgnoreEventTimer.Start();
						if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.ClevoID == 30485)
						{
							Global.features.SetXTU2_0();
						}
						else if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.SupportCPUOC_WMI122Bit23)
						{
							Global.features.SetXTU1_0();
						}
					}
					break;
				case 246:
					osd.ShowOsd(OsdForm.OsdImage.osd_camera_off);
					Global.features.WriteCCDValue(0);
					break;
				case 247:
					osd.ShowOsd(OsdForm.OsdImage.osd_camera_on);
					Global.features.WriteCCDValue(1);
					break;
				case 252:
					if (!Global.features.SupportI2C_TP)
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_off);
					}
					else if (Global.features.GetI2C_TP())
					{
						Global.features.SetI2C_TP(status: false);
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_off);
					}
					else
					{
						Global.features.SetI2C_TP(status: true);
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_on);
					}
					break;
				case 253:
					if (!Global.features.SupportI2C_TP)
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_on);
					}
					else if (Global.features.GetI2C_TP())
					{
						Global.features.SetI2C_TP(status: false);
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_off);
					}
					else
					{
						Global.features.SetI2C_TP(status: true);
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_on);
					}
					break;
				case 93:
					Thread.Sleep(100);
					if (Global.features.GetI2C_TP())
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_on);
					}
					else
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_touchpad_off);
					}
					break;
				case 149:
					if (Global.features.SupportUWPAppsflags != 0 && !IgnoreDCHUEvent)
					{
						if (Global.features.GetCC30_APP_Launch() == 0)
						{
							LaunchApp("clevocc30");
							IgnoreDCHUEvent = true;
							IgnoreEventTimer.Interval = 1000;
							IgnoreEventTimer.Start();
						}
						else
						{
							DebugLog.WriteDbgLog2Outlog("clevofnkey^204");
							Thread.Sleep(100);
							DebugLog.WriteDbgLog2Outlog("clevofnkey^204");
							IgnoreEventTimer.Interval = 500;
							IgnoreDCHUEvent = true;
							IgnoreEventTimer.Start();
						}
					}
					break;
				case 192:
				{
					CallingVariations.NtPowerInformation ntPowerInformation = CallingVariations.GetNtPowerInformation();
					byte activePowerPlan = CallingVariations.GetActivePowerPlan();
					int lIDACTIONAC = CallingVariations.GetLIDACTIONAC(activePowerPlan);
					int lIDACTIONDC = CallingVariations.GetLIDACTIONDC(activePowerPlan);
					if (ntPowerInformation.AcOnLine == 1)
					{
						switch (lIDACTIONAC)
						{
						case 1:
							if (Global.features.SupportModernStandby)
							{
								DebugLog.WriteDbgLog2Outlog("do Lid Modern Standby, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
								Global.features.MonitorControl(status: false);
							}
							else
							{
								DebugLog.WriteDbgLog2Outlog("do Lid S3, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
								SuspendState(0);
							}
							break;
						case 2:
							DebugLog.WriteDbgLog2Outlog("do Lid S4, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
							SuspendState(1);
							break;
						case 3:
							DebugLog.WriteDbgLog2Outlog("do Lid ShutDown, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
							CallingVariations.MySystemShutDown(2);
							break;
						}
					}
					else
					{
						switch (lIDACTIONDC)
						{
						case 1:
							if (Global.features.SupportModernStandby)
							{
								DebugLog.WriteDbgLog2Outlog("do Lid Modern Standby, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
								Global.features.MonitorControl(status: false);
							}
							else
							{
								DebugLog.WriteDbgLog2Outlog("do Lid S3, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
								SuspendState(0);
							}
							break;
						case 2:
							DebugLog.WriteDbgLog2Outlog("do Lid S4, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
							SuspendState(1);
							break;
						case 3:
							DebugLog.WriteDbgLog2Outlog("do Lid ShutDown, SYSData.AcOnLine=" + ntPowerInformation.AcOnLine + ", LIDACTIONAC = " + lIDACTIONAC + ", LIDACTIONDC=" + lIDACTIONDC);
							CallingVariations.MySystemShutDown(2);
							break;
						}
					}
					break;
				}
				case 84:
					Global.features.RunNvsrWorkaround();
					if (Global.features.OLED_Panel)
					{
						Global.dchu.SetWMI(94, 1u);
					}
					break;
				case 123:
					if (Global.features.SupportFlexikey)
					{
						Dispatcher.UIThread.Invoke((Action)(() =>
						{
							if (Global.flexikey.Get_Flexikey_kb_status() == 1 || Global.flexikey.Get_Flexikey_mouse_status() == 1)
							{
								osd.ShowOsd(OsdForm.OsdImage.osd_combo_off);
								Global.flexikey.Set_Flexikey_kb_status(0);
								Global.flexikey.Set_Flexikey_mouse_status(0);
								Global.features.FlexikeyInit();
								Global.features.SetKeybordMouseEnble(IsEnable: false);
							}
							else
							{
								osd.ShowOsd(OsdForm.OsdImage.osd_combo_on);
								Global.flexikey.Set_Flexikey_kb_status(1);
								Global.flexikey.Set_Flexikey_mouse_status(1);
								Global.features.FlexikeyInit();
								Global.features.SetKeybordMouseEnble(IsEnable: true);
							}
						}));
						DebugLog.WriteDbgLog2Outlog("clevofnkey^205");
						Thread.Sleep(100);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^205");
					}
					break;
				case 124:
					Global.features.Rotation180();
					break;
				case 131:
					if (!IgnoreDCHUEvent && (Global.KB.typeRule == 3 || Global.KB.typeRule == 2 || Global.KB.typeRule == 6 || Global.KB.typeRule == 22 || Global.KB.typeRule == 1))
					{
						if (Global.features.GetLedKB_APP_Launch() == 0)
						{
							DefaultLaunch("clevokeyboard:");
							IgnoreDCHUEvent = true;
							IgnoreEventTimer.Interval = 1000;
							IgnoreEventTimer.Start();
						}
						else
						{
							DebugLog.WriteDbgLog2Outlog("clevofnkey^203");
							Thread.Sleep(100);
							DebugLog.WriteDbgLog2Outlog("clevofnkey^203");
							IgnoreDCHUEvent = true;
							IgnoreEventTimer.Interval = 500;
							IgnoreEventTimer.Start();
						}
					}
					break;
				case 132:
				{
					DebugLog.WriteDbgLog2Outlog("STUB control.exe /name Microsoft.Recovery (Windows recovery settings)");
					break;
				}
				case 138:
					if (Global.KB.typeRule == 22)
					{
						int num2 = Global.KB.UpdateCurrentStatus();
						int num3 = Global.KB.UpdateCurrentBrightness();
						if (num2 == 0)
						{
							Global.KB.SetBrightnessLevel(1);
							osd.ShowWhiteKbfor5(1);
						}
						else
						{
							switch (num3)
							{
							case 1:
								Global.KB.SetBrightnessLevel(2);
								osd.ShowWhiteKbfor5(2);
								break;
							case 2:
								Global.KB.SetBrightnessLevel(3);
								osd.ShowWhiteKbfor5(3);
								break;
							case 3:
								Global.KB.SetBrightnessLevel(4);
								osd.ShowWhiteKbfor5(4);
								break;
							case 4:
								Global.KB.SetBrightnessLevel(0);
								osd.ShowWhiteKbfor5(0);
								break;
							}
						}
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
						Thread.Sleep(100);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					}
					else if (Global.KB.typeRule == 1)
					{
						Global.KB.GetWhiteBrightnessNow();
						if (Global.features.Support6WhKb)
						{
							osd.ShowWhiteKbfor6(Global.KB.GetBrightness());
						}
						else
						{
							switch (Global.KB.GetBrightness())
							{
							case 0:
								osd.ShowOsd(OsdForm.OsdImage.osd_backlight_off);
								break;
							case 1:
								osd.ShowOsd(OsdForm.OsdImage.osd_backlight_low);
								break;
							case 2:
								osd.ShowOsd(OsdForm.OsdImage.osd_backlight_high);
								break;
							}
						}
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
						Thread.Sleep(100);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					}
					break;
				case 32:
					Global.KB.SetBrightnessDown();
					osd.ShowOsd(OsdForm.OsdImage.osd_backlight_low);
					DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					Thread.Sleep(100);
					DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					break;
				case 33:
					Global.KB.SetBrightnessUp();
					osd.ShowOsd(OsdForm.OsdImage.osd_backlight_high);
					DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					Thread.Sleep(100);
					DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					break;
				case 63:
					Global.KB.UpdateCurrentStatus();
					DebugLog.WriteDbgLog2Outlog("KB Status=" + Global.KB.Status);
					if (Global.KB.Status == 0)
					{
						Global.KB.Status = 1;
						osd.ShowOsd(OsdForm.OsdImage.osd_backlight_on);
					}
					else
					{
						Global.KB.Status = 0;
						osd.ShowOsd(OsdForm.OsdImage.osd_backlight_off);
					}
					Global.KB.SetStatus();
					DebugLog.WriteDbgLog2Outlog("clevofnkey^20A");
					Thread.Sleep(100);
					DebugLog.WriteDbgLog2Outlog("clevofnkey^20A");
					break;
				case 139:
					Global.KB.UpdateCurrentStatus();
					if (Global.KB.LeftStatus == 0)
					{
						Global.KB.LeftStatus = 1;
					}
					else
					{
						Global.KB.LeftStatus = 0;
					}
					Global.KB.SetStatus();
					break;
				case 140:
					Global.KB.UpdateCurrentStatus();
					if (Global.KB.MidStatus == 0)
					{
						Global.KB.MidStatus = 1;
					}
					else
					{
						Global.KB.MidStatus = 0;
					}
					Global.KB.SetStatus();
					break;
				case 141:
					Global.KB.UpdateCurrentStatus();
					if (Global.KB.RightStatus == 0)
					{
						Global.KB.RightStatus = 1;
					}
					else
					{
						Global.KB.RightStatus = 0;
					}
					Global.KB.SetStatus();
					break;
				case 129:
					if (Global.KB.typeRule == 3 || Global.KB.typeRule == 2 || Global.KB.typeRule == 6 || Global.KB.typeRule == 22)
					{
						Global.KB.SetBrightnessDown();
						osd.ShowOsd(OsdForm.OsdImage.osd_backlight_low);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
						Thread.Sleep(100);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					}
					break;
				case 130:
					if (Global.KB.typeRule == 3 || Global.KB.typeRule == 2 || Global.KB.typeRule == 6 || Global.KB.typeRule == 22)
					{
						Global.KB.SetBrightnessUp();
						osd.ShowOsd(OsdForm.OsdImage.osd_backlight_high);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
						Thread.Sleep(100);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					}
					break;
				case 133:
					Global.features.Insyde_AirplaneMode_Control();
					break;
				case 158:
				case 159:
					if (Global.KB.typeRule == 3 || Global.KB.typeRule == 2 || Global.KB.typeRule == 6 || Global.KB.typeRule == 22)
					{
						Global.KB.UpdateCurrentStatus();
						if (Global.KB.Status == 0)
						{
							Global.KB.Status = 1;
							osd.ShowOsd(OsdForm.OsdImage.osd_backlight_on);
						}
						else
						{
							Global.KB.Status = 0;
							osd.ShowOsd(OsdForm.OsdImage.osd_backlight_off);
						}
						Global.KB.SetStatus();
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
						Thread.Sleep(100);
						DebugLog.WriteDbgLog2Outlog("clevofnkey^201");
					}
					break;
				case 166:
					osd.ShowOsd(OsdForm.OsdImage.osd_docking_on);
					break;
				case 167:
					osd.ShowOsd(OsdForm.OsdImage.osd_docking_off);
					break;
				case 184:
					osd.ShowOsd(OsdForm.OsdImage.osd_gps_off);
					break;
				case 185:
					osd.ShowOsd(OsdForm.OsdImage.osd_gps_on);
					break;
				case 199:
					if (!Global.features.IsUSBKeyboardExist())
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_num_off);
					}
					break;
				case 200:
					if (!Global.features.IsUSBKeyboardExist())
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_num_on);
					}
					break;
				case 201:
					osd.ShowOsd(OsdForm.OsdImage.osd_caps_off);
					break;
				case 202:
					osd.ShowOsd(OsdForm.OsdImage.osd_caps_on);
					break;
				case 203:
					osd.ShowOsd(OsdForm.OsdImage.osd_scr_off);
					break;
				case 204:
					osd.ShowOsd(OsdForm.OsdImage.osd_scr_on);
					break;
				case 222:
					osd.ShowOsd(OsdForm.OsdImage.osd_silent_on);
					break;
				case 223:
					osd.ShowOsd(OsdForm.OsdImage.osd_silent_off);
					break;
				case 234:
					osd.ShowOsd(OsdForm.OsdImage.osd_3g_off);
					break;
				case 235:
					osd.ShowOsd(OsdForm.OsdImage.osd_3g_on);
					break;
				case 301:
					osd.ShowOsd(OsdForm.OsdImage.osd_flexiaccess_on);
					break;
				case 302:
					osd.ShowOsd(OsdForm.OsdImage.osd_flexiaccess_off);
					break;
				case 303:
					osd.ShowOsd(OsdForm.OsdImage.osd_gc_on);
					break;
				case 304:
					osd.ShowOsd(OsdForm.OsdImage.osd_gc_off);
					break;
				case 64:
					osd.ShowOsd(OsdForm.OsdImage.osd_fan_speed_AntiDustRuning);
					break;
				case 107:
					osd.ShowOsd(OsdForm.OsdImage.osd_fan_speed_Custom);
					Global.fan.Fn1_ChangeFanMode(6);
					break;
				case 106:
					osd.ShowOsd(OsdForm.OsdImage.osd_fan_speed_MaxQ);
					Global.fan.Fn1_ChangeFanMode(5);
					break;
				case 112:
					if (Global.features.GetPowerMode() == 0)
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_fan_speed_silent);
					}
					else
					{
						osd.ShowOsd(OsdForm.OsdImage.osd_fan_speed_auto);
					}
					Global.fan.Fn1_ChangeFanMode(0);
					break;
				case 143:
					osd.ShowOsd(OsdForm.OsdImage.osd_fan_speed_max);
					Global.fan.Fn1_ChangeFanMode(1);
					break;
				case 186:
				{
					int num = E_Data;
					if (((num >> 7) & 1) == 1)
					{
						num &= 0x7F;
						Global.brightness.Write_Brightness(num);
					}
					Global.features.LoopBeeperStart();
					try
					{
						if (Global.features.PCbeep == 1)
						{
							CallingVariations.pc_beep_Mute(p1: false);
						}
						if (Global.features.SupportBATLowControl)
						{
							byte[] array2 = Global.dchu.Read_ECRAM(1361u, 1);
							bool flag = Convert.ToBoolean((array2[0] >> 5) & 1);
							bool flag2 = Convert.ToBoolean((array2[0] >> 4) & 1);
							if (flag | flag2)
							{
								Global.features.BatteryLowControl_Setting();
							}
						}
						break;
					}
					catch
					{
						break;
					}
				}
				case 187:
					Global.features.LoopBeeperStop();
					try
					{
						if (Global.features.PCbeep == 1)
						{
							CallingVariations.pc_beep_Mute(p1: true);
						}
						break;
					}
					catch
					{
						break;
					}
				case 59:
					Global.BAT_PowerStatus.UpdatePowerInfo();
					if (Global.BAT_PowerStatus.PowerLineStatus != PowerLineStatus.Online && !Global.features.BatteryUtilityRefresh)
					{
						Global.dchu.SetWMI(121, 44, 2u);
						Global.features.BatteryLowControl_Setting();
					}
					break;
				case 60:
					if (!Global.features.BatteryUtilityRefresh)
					{
						Global.dchu.SetWMI(121, 44, 3u);
						Global.features.BatteryLowControl_Restore();
					}
					break;
				case 215:
					if (Global.features.OLED_Panel)
					{
						Global.features.SetScreenOff();
					}
					else if (!Global.features.DisableFnF2TouchPadHook)
					{
						DebugLog.WriteDbgLog2Inlog("clevofnkey^1ff");
					}
					break;
				case 96:
					Global.gpu.GPU_Turbo_off();
					DebugLog.WriteDbgLog2Outlog("GPU Turbo off");
					break;
				case 97:
				{
					byte[] array = Global.dchu.ReadAppSettings(1, 1, 1);
					Global.gpu.Load_GPU_forFn_3(array[0]);
					DebugLog.WriteDbgLog2Outlog("GPU Turbo on");
					break;
				}
				case 72:
					osd.ShowOsd(OsdForm.OsdImage.osd_Fn_lock_disable);
					break;
				case 73:
					osd.ShowOsd(OsdForm.OsdImage.osd_Fn_lock_enable);
					break;
				case 78:
					Global.features.SetBrightnessFromWMI17(78);
					break;
				case 79:
					Global.features.SetBrightnessFromWMI17(79);
					break;
				case 48:
					Global.features.SetBrightnessFromWMI17(48);
					break;
				case 49:
					Global.features.SetBrightnessFromWMI17(49);
					break;
				case 50:
					Global.features.SetBrightnessFromWMI17(50);
					break;
				case 51:
					Global.features.SetBrightnessFromWMI17(51);
					break;
				case 52:
					Global.features.SetBrightnessFromWMI17(52);
					break;
				case 53:
					Global.features.SetBrightnessFromWMI17(53);
					break;
				case 54:
					Global.features.SetBrightnessFromWMI17(54);
					break;
				case 55:
					Global.features.SetBrightnessFromWMI17(55);
					break;
				case 56:
					Global.features.SetBrightnessFromWMI17(56);
					break;
				case 57:
					Global.KB.Change_ToggleRGBColor();
					DebugLog.WriteDbgLog2Outlog("clevofnkey^209");
					Thread.Sleep(100);
					DebugLog.WriteDbgLog2Outlog("clevofnkey^209");
					break;
				case 58:
					osd.ShowOsd(OsdForm.OsdImage.osd_fan_speed_Turbo);
					break;
				}
			}));
		}
		catch
		{
		}
	}


	// Port: the destructive first-run path (zero all AppSettings pages, fan defaults, ...) is opt-in.
	internal static bool AllowFirstRun => Environment.GetCommandLineArgs().Contains("--first-run") || Environment.GetEnvironmentVariable("FNKEY_FIRST_RUN") == "1";

	private void OnFirstRun()
	{
		for (int i = 0; i < 16; i++)
		{
			byte[] array = new byte[256];
			Global.dchu.WriteAppSettings(i, 0, array.Length, array);
		}
		Global.dchu.SetWMI(121, 34, 1u);
		DebugLog.WriteDbgLog2Outlog("Fnkey Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
		_ = new byte[4]
		{
			(byte)Assembly.GetExecutingAssembly().GetName().Version.Major,
			(byte)Assembly.GetExecutingAssembly().GetName().Version.Minor,
			(byte)Assembly.GetExecutingAssembly().GetName().Version.Build,
			(byte)Assembly.GetExecutingAssembly().GetName().Version.Revision
		};
		Global.fnKeyStatus = Global.FnKeyStatus.FirstRun;
		DebugLog.WriteDbgLog2Outlog("OnFirstRun");
		Global.features.SetCCD(1);
		Global.features.DoWMIFlag(16, Global.dchu.GetWMI(16));
		byte defaultPowerMode = Global.features.GetDefaultPowerMode();
		defaultPowerMode &= 0x7F;
		byte b = 0;
		b = defaultPowerMode switch
		{
			0 => (byte)3, 
			1 => (byte)2, 
			2 => (byte)1, 
			3 => (byte)0, 
			_ => (byte)3, 
		};
		Global.features.ACMode_PowerMode = b;
		Global.features.SetPowerMode(b);
		Global.features.SetUILanguageAutoDetect(1);
		string text = CultureInfo.CurrentCulture.Name.ToLower();
		if (text.Contains("es-"))
		{
			text = "es";
		}
		if (text.Contains("de-"))
		{
			text = "de";
		}
		if (text.Contains("it-"))
		{
			text = "it";
		}
		if (text.Contains("ja-"))
		{
			text = ((Global.features.CustomerID != 5) ? "ja" : "ja-ep");
		}
		if (text.Contains("ko-"))
		{
			text = "ko";
		}
		if (text.Contains("ru-"))
		{
			text = "ru";
		}
		int num = 0;
		Global.features.SetUILanguage((byte)(text switch
		{
			"en-us" => 1u, 
			"en-gb" => 2u, 
			"es" => 3u, 
			"de" => 4u, 
			"it" => 5u, 
			"ja" => 6u, 
			"ko" => 7u, 
			"zh-cn" => 8u, 
			"zh-tw" => 9u, 
			"fr-fr" => 10u, 
			"fr-ca" => 11u, 
			"pt-br" => 12u, 
			"pt-pt" => 13u, 
			"tr-tr" => 14u, 
			"th-th" => 15u, 
			"vi-vn" => 16u, 
			"ru" => 17u, 
			"ja-ep" => 18u, 
			_ => 1u, 
		}));
		Global.features.SetShowOSD(1);
		Global.refreshRateStatus = Global.RefreshStatus.FirstRun;
		Global.features.CheckPanelSupportLowRefreshRate();
		DebugLog.WriteDbgLog2Outlog("SupportMonitorLowRefreshRate=" + Global.features.SupportMonitorLowRefreshRate + ", IsLowRefreshRateChecked = " + Global.features.IsLowRefreshRateChecked);
	}


	private void OnSleep()
	{
		DoResume.Stop();
		ResumeDelayTimer.Stop();
		UpdateStatusTimer.Stop();
		try
		{
			int clevoID = Global.features.ClevoID;
			if ((uint)(clevoID - 34101) <= 1u)
			{
				CallingVariations.Set_ASPM(0);
			}
			Global.fnKeyStatus = Global.FnKeyStatus.Sleep;
			DebugLog.WriteDbgLog2Outlog("OnSleep");
			Global.dchu.GetWMI(72);
			Global.KB.UpdateCurrentOverrideBootEffect();
			Global.KB.SetOverrideBootEffect(Global.KB.OverrideBootEffect);
			if (Global.KB.GetKBType().Equals(3))
			{
				Global.KB.StartHook(status: false);
			}
			Global.features.UpdateWolStatus(IsShutdown: false);
			if (Global.features.SupportCPUOC_WMI16Bit6)
			{
				XTU_timer.Stop();
			}
			if (Global.features.SupportFlexikey)
			{
				Global.flexikey.Set_Flexikey_kb_status(0);
				Global.flexikey.Set_Flexikey_mouse_status(0);
				Global.features.FlexikeyInit();
				Global.features.SetKeybordMouseEnble(IsEnable: false);
				DebugLog.WriteDbgLog2Outlog("clevofnkey^205");
				Thread.Sleep(100);
				DebugLog.WriteDbgLog2Outlog("clevofnkey^205");
			}
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("OnSleepError");
		}
	}


	private void OnResume()
	{
		Global.refreshRateStatus = Global.RefreshStatus.OnResume;
		DoResume.Stop();
		ResumeDelayTimer.Stop();
		UpdateStatusTimer.Stop();
		DoResume.Start();
	}


	private void DoResume_Tick(object sender, EventArgs e)
	{
		switch (Global.features.ClevoID)
		{
		case 20720:
			DebugLog.WriteDbgLog2Inlog("w_arg254^" + AppContext.BaseDirectory + "\\cc30wk.exe -fh");
			break;
		case 42240:
		case 42241:
		case 42496:
			if (Global.features.SupportBIOSFeatureList_Version == 0)
			{
				DebugLog.WriteDbgLog2Inlog("w_arg254^" + AppContext.BaseDirectory + "\\cc30wk.exe -rc");
			}
			break;
		}
		if (Global.features.SupportRescanCardReader)
		{
			DebugLog.WriteDbgLog2Inlog("w_arg254^" + AppContext.BaseDirectory + "\\cc30wk.exe -rc");
		}
		if (Global.features.SupportFixUCSI_DeviceYellowMarkWorkaround)
		{
			DebugLog.WriteDbgLog2Inlog("w_arg254^" + AppContext.BaseDirectory + "\\cc30wk.exe -ucsi");
		}
		if (Global.features.SupportDTT_iSST_DeviceYellowMarkWorkaround)
		{
			DebugLog.WriteDbgLog2Inlog("w_arg254^" + AppContext.BaseDirectory + "\\cc30wk.exe -dtt_isst");
		}
		DoResume.Stop();
		ResumeDelayTimer.Stop();
		UpdateStatusTimer.Stop();
		try
		{
			Global.fnKeyStatus = Global.FnKeyStatus.Resume;
			DebugLog.WriteDbgLog2Outlog("OnResume");
			Global.features.SetCCD(Global.features.GetCCD());
			Global.features.DoWMIFlag(70, Global.dchu.GetWMI(70));
			Global.features.Get_Features_list();
			if (Global.features.LoadDefault)
			{
				DebugLog.WriteDbgLog2Outlog("OnFirstRun after OnResume");
				Global.features.KillAllApps();
				if (AllowFirstRun) { OnFirstRun(); Global.KB.FirstRun(); } else DebugLog.WriteDbgLog2Outlog("first run skipped (opt-in: --first-run or FNKEY_FIRST_RUN=1)");
				Global.RW_REG.IsFirstRun = 1;
				Global.gpu.FirstRun = true;
				Features.SetIsSavedFlag(0);
			}
			SetupMenuItem();
			if (Global.features.GetCC30_APP_Launch() == 1)
			{
				DebugLog.WriteDbgLog2Outlog("Fnkey, update cc30");
			}
			Global.features.BatteryLowControl_Check();
			Global.features.CheckI2C_TP();
			Global.fan.Load_FanMode_Info();
			airplane.InitAirPlaneLed();
			ResumeDelayTimer.Start();
			After_RestartResume = true;
			After_Resume = true;
			Global.BAT_PowerStatus.UpdatePowerInfo();
			if (Global.features.DCMode_PowerMode != 0)
			{
				if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Offline)
				{
					ChangePowerMode(CleDef.ModeType.pwrsaving);
				}
				else
				{
					ChangePowerMode((CleDef.ModeType)Global.features.ACMode_PowerMode);
				}
			}
			else
			{
				ChangePowerMode((CleDef.ModeType)Global.features.GetPowerMode());
			}
			Global.features.Log_Battery_PowerStatus();
			UpdateStatusTimer.Start();
			DebugLog.WriteDbgLog2Outlog("OnResume \r\n, SupportCPUOC_WMI16Bit6=" + Global.features.SupportCPUOC_WMI16Bit6 + "\r\n, SupportCPUOC_WMI122Bit23=" + Global.features.SupportCPUOC_WMI122Bit23);
			if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.ClevoID == 30485)
			{
				Global.features.SetXTU2_0();
			}
			else if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.SupportCPUOC_WMI122Bit23)
			{
				Global.features.SetXTU1_0();
			}
			DebugLog.WriteDbgLog2Outlog("OnReady");
		}
		catch
		{
			DebugLog.WriteDbgLog2Outlog("OnResumeError");
			UpdateStatusTimer.Start();
		}
	}


	private void ResumeDelayTimer_Tick(object sender, EventArgs e)
	{
		ResumeDelayTimer.Stop();
		Global.KB.Init();
		Global.features.RunNvsrWorkaround();
		Thread.Sleep(1000);
		Global.features.RunNvsrWorkaround();
		Thread.Sleep(2000);
		Global.features.RunNvsrWorkaround();
	}


	private void UpdateStatusTimer_Tick(object sender, EventArgs e)
	{
		UpdateStatusTimer.Stop();
		Thread.Sleep(1000);
		Global.fnKeyStatus = Global.FnKeyStatus.Ready;
	}


	private void IgnoreEventTimer_Tick(object sender, EventArgs e)
	{
		IgnoreDCHUEvent = false;
		IgnoreEventTimer.Stop();
	}


	private void NHxxAC_AF_DelayTimer_Tick(object sender, EventArgs e)
	{
		NHxxAC_AF_DelayTimer.Stop();
		if (Global.features.IsPCCExist())
		{
			NHxxAC_AF_DelayTimer.Start();
			return;
		}
		switch (nLastEvent)
		{
		case 65:
			NHxxACAF_EC_PROC_HOT_65();
			break;
		case 66:
			NHxxACAF_TotalPower_Release_66();
			break;
		case 67:
			NHxxACAF_Thermal_Event_Release_Release_67();
			break;
		case 68:
			NHxxACAF_Thermal_Limit_68();
			break;
		default:
			DebugLog.WriteDbgLog2Outlog("NHxxAC_AF_DelayTimer Error, nLastEvent=" + nLastEvent);
			Global.features.RestoreAMD_PowerConsumption();
			break;
		}
	}


	private void OnChangePowerMode(object a)
	{
		lock (this)
		{
			int powerMode = Global.features.GetPowerMode();
			CleDef.ModeType modeType = (CleDef.ModeType)a;
			Global.features.SetPowerMode((byte)modeType);
			if (!After_Resume)
			{
				Global.gpu.Load_GPU_forFn_3((byte)modeType);
			}
			DebugLog.WriteDbgLog2Outlog("Set PowerMode: " + modeType);
			if (!Global.features.SupportHW_P_state)
			{
				if (modeType == CleDef.ModeType.entertainment || modeType == CleDef.ModeType.quiet)
				{
					Global.features.SetEnergyStarMode(2u);
				}
				if (modeType == CleDef.ModeType.performance)
				{
					Global.features.SetEnergyStarMode(0u);
				}
				if (modeType == CleDef.ModeType.pwrsaving)
				{
					Global.features.SetEnergyStarMode(1u);
				}
			}
			Change_Notify_Icon(modeType);
			switch (modeType)
			{
			case CleDef.ModeType.quiet:
				if (Global.features.SupportPowerModeBit)
				{
					if (Global.features.SupportPowerModeBit_Performance)
					{
						NextMode = 2;
					}
					else if (Global.features.SupportPowerModeBit_Entertainment)
					{
						NextMode = 3;
					}
					else if (Global.features.SupportPowerModeBit_PowerSaving)
					{
						NextMode = 1;
					}
				}
				else
				{
					NextMode = 2;
				}
				if (Global.features.SupportASPMControl)
				{
					CallingVariations.Set_ASPMEx(Global.QuietMode.ASPM_AC, 1);
					CallingVariations.Set_ASPMEx(Global.QuietMode.ASPM_DC, 0);
					DebugLog.WriteDbgLog2Outlog("ASPM AC=" + Global.QuietMode.ASPM_AC + " DC=" + Global.QuietMode.ASPM_DC);
				}
				else if (Global.features.SupportDisablePCIE_ASPM)
				{
					CallingVariations.Set_ASPM(2);
				}
				if (Global.features.ClevoID == 20720 || Global.features.ClevoID == 20724)
				{
					CallingVariations.SetActivePowerPlanGUID(guidBalance);
					CallingVariations.SetACCriticalBatteryDoNothing();
					CallingVariations.Set_ASPMEx(2, 1);
					CallingVariations.Set_ASPMEx(2, 0);
				}
				if (!After_RestartResume && Global.features.SupportSetPowerPlan)
				{
					Global.features.Set_PowerPlan(Global.QuietMode.PowerPlan);
				}
				CallingVariations.ReloadActivePowerPlan();
				break;
			case CleDef.ModeType.pwrsaving:
				if (Global.features.SupportPowerModeBit)
				{
					if (Global.features.SupportPowerModeBit_Quite)
					{
						NextMode = 0;
					}
					else if (Global.features.SupportPowerModeBit_Performance)
					{
						NextMode = 2;
					}
					else if (Global.features.SupportPowerModeBit_Entertainment)
					{
						NextMode = 3;
					}
				}
				else
				{
					NextMode = 0;
				}
				if (Global.features.SupportASPMControl)
				{
					CallingVariations.Set_ASPMEx(Global.PwrsavingeMode.ASPM_AC, 1);
					CallingVariations.Set_ASPMEx(Global.PwrsavingeMode.ASPM_DC, 0);
					DebugLog.WriteDbgLog2Outlog("ASPM AC=" + Global.PwrsavingeMode.ASPM_AC + " DC=" + Global.PwrsavingeMode.ASPM_DC);
				}
				else if (Global.features.SupportDisablePCIE_ASPM)
				{
					CallingVariations.Set_ASPM(2);
				}
				if (Global.features.ClevoID == 20720 || Global.features.ClevoID == 20724)
				{
					CallingVariations.SetActivePowerPlanGUID(guidPowerSaver);
					CallingVariations.SetACCriticalBatteryDoNothing();
					CallingVariations.Set_ASPMEx(2, 1);
					CallingVariations.Set_ASPMEx(2, 0);
				}
				if (!After_RestartResume && Global.features.SupportSetPowerPlan)
				{
					Global.features.Set_PowerPlan(Global.PwrsavingeMode.PowerPlan);
				}
				CallingVariations.ReloadActivePowerPlan();
				break;
			case CleDef.ModeType.performance:
				if (Global.features.SupportPowerModeBit)
				{
					if (Global.features.SupportPowerModeBit_Entertainment)
					{
						NextMode = 3;
					}
					else if (Global.features.SupportPowerModeBit_PowerSaving)
					{
						NextMode = 1;
					}
					else if (Global.features.SupportPowerModeBit_Quite)
					{
						NextMode = 0;
					}
				}
				else
				{
					NextMode = 3;
				}
				if (Global.features.SupportASPMControl)
				{
					CallingVariations.Set_ASPMEx(Global.PerformanceMode.ASPM_AC, 1);
					CallingVariations.Set_ASPMEx(Global.PerformanceMode.ASPM_DC, 0);
					DebugLog.WriteDbgLog2Outlog("ASPM AC=" + Global.PerformanceMode.ASPM_AC + " DC=" + Global.PerformanceMode.ASPM_DC);
				}
				else if (Global.features.SupportDisablePCIE_ASPM)
				{
					CallingVariations.Set_ASPM(0);
				}
				if (Global.features.ClevoID == 20720)
				{
					CallingVariations.SetActivePowerPlanGUID(guidBalance);
					CallingVariations.SetACCriticalBatteryDoNothing();
					if (Global.features.SupportDisablePCIE_ASPM)
					{
						CallingVariations.Set_ASPMEx(0, 1);
					}
					else
					{
						CallingVariations.Set_ASPMEx(2, 1);
					}
					CallingVariations.Set_ASPMEx(2, 0);
				}
				else if (Global.features.ClevoID == 20724)
				{
					CallingVariations.SetActivePowerPlanGUID(guidBalance);
					CallingVariations.SetACCriticalBatteryDoNothing();
					CallingVariations.Set_ASPM(0);
				}
				if (!After_RestartResume && Global.features.SupportSetPowerPlan)
				{
					Global.features.Set_PowerPlan(Global.PerformanceMode.PowerPlan);
				}
				CallingVariations.ReloadActivePowerPlan();
				break;
			case CleDef.ModeType.entertainment:
				if (Global.features.SupportPowerModeBit)
				{
					if (Global.features.SupportPowerModeBit_PowerSaving)
					{
						NextMode = 1;
					}
					else if (Global.features.SupportPowerModeBit_Quite)
					{
						NextMode = 0;
					}
					else if (Global.features.SupportPowerModeBit_Performance)
					{
						NextMode = 2;
					}
				}
				else
				{
					NextMode = 1;
				}
				if (Global.features.SupportASPMControl)
				{
					CallingVariations.Set_ASPMEx(Global.EntertainmentMode.ASPM_AC, 1);
					CallingVariations.Set_ASPMEx(Global.EntertainmentMode.ASPM_DC, 0);
					DebugLog.WriteDbgLog2Outlog("ASPM AC=" + Global.EntertainmentMode.ASPM_AC + " DC=" + Global.EntertainmentMode.ASPM_DC);
				}
				else if (Global.features.SupportDisablePCIE_ASPM)
				{
					CallingVariations.Set_ASPM(2);
				}
				if (Global.features.ClevoID == 20720)
				{
					CallingVariations.SetActivePowerPlanGUID(guidBalance);
					CallingVariations.SetACCriticalBatteryDoNothing();
					CallingVariations.Set_ASPMEx(2, 0);
					CallingVariations.Set_ASPMEx(2, 1);
				}
				else if (Global.features.ClevoID == 20724)
				{
					CallingVariations.SetActivePowerPlanGUID(guidBalance);
					CallingVariations.SetACCriticalBatteryDoNothing();
					CallingVariations.Set_ASPM(0);
				}
				if (!After_RestartResume && Global.features.SupportSetPowerPlan)
				{
					Global.features.Set_PowerPlan(Global.EntertainmentMode.PowerPlan);
				}
				CallingVariations.ReloadActivePowerPlan();
				break;
			}
			if (Global.features.ClevoID == 20720 || Global.features.ClevoID == 20724)
			{
				Global.features.AMD_PPT_Now = 999u;
				Global.features.AMD_TDC_Now = 999u;
				Global.features.AMD_EDC_Now = 999u;
				Global.features.RestoreAMD_PowerConsumption();
				CallingVariations.SetLowBatteryLevel(15);
				CallingVariations.SetCriticalBatteryLevel(10);
				CallingVariations.SetReservelBatteryLevel(12);
				CallingVariations.ReloadActivePowerPlan();
			}
			Global.fan.Load_FanMode_forFn_3((byte)modeType);
			if (After_RestartResume)
			{
				Global.fan.KeepFanMode((byte)modeType);
				After_RestartResume = false;
			}
			if (After_Resume)
			{
				After_Resume = false;
			}
			DebugLog.WriteDbgLog2Outlog("clevofnkey^202");
			if (Global.fnKeyStatus != Global.FnKeyStatus.Init)
			{
				if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.ClevoID == 30485)
				{
					Global.features.SetXTU2_0();
				}
				else if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.SupportCPUOC_WMI122Bit23)
				{
					Global.features.SetXTU1_0();
				}
				Global.features.SetPanelRefreshRate(powerMode);
			}
		}
	}


	private void OnPowerLineChanged()
	{
		if (Global.features.ClevoID == 20720 || Global.features.ClevoID == 20724 || Global.fnKeyStatus != Global.FnKeyStatus.Ready)
		{
			return;
		}
		Dispatcher.UIThread.Invoke((Action)(() =>
		{
			Global.BAT_PowerStatus.UpdatePowerInfo();
			if (Global.features.DCMode_PowerMode != 0)
			{
				if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online && Global.BAT_PowerStatus.OLD_PowerLineStatus != PowerLineStatus.Online)
				{
					Global.BAT_PowerStatus.OLD_PowerLineStatus = PowerLineStatus.Online;
					DebugLog.WriteDbgLog2Outlog("Event AC In");
					ChangePowerMode((CleDef.ModeType)Global.features.ACMode_PowerMode);
				}
				else if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Offline && Global.BAT_PowerStatus.OLD_PowerLineStatus != PowerLineStatus.Offline)
				{
					Global.BAT_PowerStatus.OLD_PowerLineStatus = PowerLineStatus.Offline;
					DebugLog.WriteDbgLog2Outlog("Event ac out_support DC PowerMode");
					if (Global.features.ClevoID == 16577 && Global.features.CustomerID == 7)
					{
						ChangePowerMode(CleDef.ModeType.entertainment);
					}
					else
					{
						ChangePowerMode(CleDef.ModeType.pwrsaving);
					}
				}
			}
			else
			{
				if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online && Global.BAT_PowerStatus.OLD_PowerLineStatus != PowerLineStatus.Online)
				{
					Global.BAT_PowerStatus.OLD_PowerLineStatus = PowerLineStatus.Online;
					DebugLog.WriteDbgLog2Outlog("event ac in");
					Global.features.BatteryLowControl_Check();
					if (Global.features.GetPowerMode() == 2)
					{
						Global.gpu.Load_GPU_forFn_3(2);
						if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.ClevoID == 30485)
						{
							Global.features.SetXTU2_0();
						}
						else if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.SupportCPUOC_WMI122Bit23)
						{
							Global.features.SetXTU1_0();
						}
					}
					else if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.ClevoID == 30485)
					{
						Global.features.SetXTU2_0();
					}
				}
				else if (Global.BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Offline && Global.BAT_PowerStatus.OLD_PowerLineStatus != PowerLineStatus.Offline)
				{
					Global.BAT_PowerStatus.OLD_PowerLineStatus = PowerLineStatus.Offline;
					DebugLog.WriteDbgLog2Outlog("event ac out");
					if (Global.features.GetPowerMode() == 2)
					{
						Global.gpu.Load_OC_OFF();
						if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.ClevoID == 30485)
						{
							Global.features.SetXTU2_0();
						}
						else if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.SupportCPUOC_WMI122Bit23)
						{
							Global.features.SetXTU1_0();
						}
					}
					else if (Global.features.SupportCPUOC_WMI16Bit6 && Global.features.ClevoID == 30485)
					{
						Global.features.SetXTU2_0();
					}
				}
				Global.refreshRateStatus = Global.RefreshStatus.ACDC;
				Global.features.SetPanelRefreshRate(-1);
			}
			Global.features.Log_Battery_PowerStatus();
		}));
	}


	private static void NHxxACAF_EC_PROC_HOT_65()
	{
		Global.features.AMD_PPT_Now = 0u;
		Global.features.AMD_TDC_Now = 0u;
		Global.features.AMD_EDC_Now = 0u;
		if (Global.BAT_PowerStatus.BatteryLifePercent > 51f)
		{
			Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.DC);
		}
		else
		{
			Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.DC1);
		}
	}


	private static void NHxxACAF_TotalPower_Release_66()
	{
		if (!Global.features.AMD_TotalPower_Limit && !Global.features.AMD_Thermal_Limit)
		{
			Global.features.RestoreAMD_PowerConsumption();
			DebugLog.WriteDbgLog2Outlog("Release TotoalPowerLimit");
		}
		else if (!Global.features.AMD_TotalPower_Limit && Global.features.AMD_Thermal_Limit)
		{
			Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.Thermal_1);
		}
		else if (Global.features.AMD_TotalPower_Limit && !Global.features.AMD_Thermal_Limit)
		{
			Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.TotalPower_1);
		}
	}


	private static void NHxxACAF_Thermal_Event_Release_Release_67()
	{
		if (!Global.features.AMD_TotalPower_Limit && !Global.features.AMD_Thermal_Limit)
		{
			Global.features.RestoreAMD_PowerConsumption();
			DebugLog.WriteDbgLog2Outlog("Release ThermalLimit");
		}
	}


	private static void NHxxACAF_Thermal_Limit_68()
	{
		Global.features.SetAMD_PowerConsumption(Global.features.clevo_AMD_Mode.Thermal_1);
		Global.features.AMD_Thermal_Limit = true;
		DebugLog.WriteDbgLog2Outlog("Get ThermalLimit");
	}

}
