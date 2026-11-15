using System;
using System.IO;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Threading;
using CC.UControl;
using CC.other_Class;
using CCenter.Platform;
namespace CC;

public partial class Page_Battery_Refresh : UserControl
{
	public enum RefreshStatus { Unexecuted, Feasible, Infeasible, Ongoing, Finish }
	public enum OngoingStatus { Stop, Discharge1, Charge1, Discharge2, Charge2, AdjustingDischarge }
	public enum RefreshResult { Healthy, Deteriorated, NeedReplacement, PermanentFailure }

	public struct BatteryUtilityData
	{
		public ushort version;
		public ushort ManufacturerDate;
		public ushort CycleCount;
		public ushort FullChargeCapacity;
		public ushort DesignCapacity;
		public ushort BatteryStatus;
		public uint PFStatus;
		public uint OperationStatus;
		public ushort StopChargingThreshold;
		public int Current_Capacity;
		public int Remaining;
	}
	public struct Battery_ini
	{
		public string Warning_condition;
		public string Replace_condition;
		public int RCycle, R1Cycle, WCycle, W1Cycle;
		public int RCapacity, R1Capacity, WCapacity, W1Capacity;
		public int RDate, WDate;
	}

	private battery BAT_PowerStatus = new battery();
	private DispatcherTimer refrash_timer = new DispatcherTimer();
	private DispatcherTimer idle_watch = new DispatcherTimer();
	private RefreshStatus Current_RefreshStatus;
	private OngoingStatus Current_OngoingStatus;
	public BatteryUtilityData myBatteryUtilityData;
	public Battery_ini myBattery_ini;
	private bool SetAC_ON_Finish = false;
	private int ss = 0, mm = 0, hh = 0;
	private int DialogBoxNumber = 0;
	private string _lastSig = "";

	public Page_Battery_Refresh()
	{
		InitializeComponent();
		myBattery_ini.Replace_condition = ""; myBattery_ini.Warning_condition = "";
	}

	public void init()
	{
		// Registry.CurrentUser.CreateSubKey(...BatteryUtility): Reg creates keys on first write.
		UC_DialogBox.CancelEvent += UC_DialogBox_CancelEvent;
		UC_DialogBox.OKEvent += UC_DialogBox_OKEvent;
		LoadMultiLang();
		Read_battery_ini();
		refrash_timer.Interval = TimeSpan.FromMilliseconds(1000.0);
		refrash_timer.Tick += Refrash_timer_Tick;
		// SystemEvents.PowerModeChanged(StatusChange) / firmware events 85/90/91 have no Linux source:
		// polled here instead (see PORT_NOTES).
		idle_watch.Interval = TimeSpan.FromMilliseconds(2000.0);
		idle_watch.Tick += (s, e) => PollEvents();
		idle_watch.Start();
		Current_OngoingStatus = (OngoingStatus)Global.RW_REG.OngoingStatus;
	}

	void PollEvents()
	{
		BAT_PowerStatus.UpdatePowerInfo();
		string sig = BAT_PowerStatus.PowerLineStatus + "/" + BAT_PowerStatus.BatteryLifePercent;
		bool changed = sig != _lastSig;
		_lastSig = sig;
		if (Current_RefreshStatus == RefreshStatus.Ongoing)
			Update_Battery_Full_Status();   // stands in for firmware events 85 (full) / 91 (fully discharged)
		else if (changed)
			UpdatePowerStatus();            // stands in for PowerModeChanged.StatusChange
	}

	public void Open_UI()
	{
		IsVisible = true;
		UpdatePowerStatus();
		if (Current_RefreshStatus != RefreshStatus.Infeasible)
		{
			if (Current_OngoingStatus == OngoingStatus.Stop)
			{
				Update_InstantResult();
				return;
			}
			UpdateUI_RefreshStatus(RefreshStatus.Ongoing);
			UpdateUI_OngoingStatus((OngoingStatus)Global.RW_REG.OngoingStatus);
			BAT_PowerStatus.Import_BatteryUtility_PowerPlan();
			BTN_stop.IsEnabled = true;
			ss = Global.RW_REG.Refresh_ElapsedTime_ss;
			mm = Global.RW_REG.Refresh_ElapsedTime_mm;
			hh = Global.RW_REG.Refresh_ElapsedTime_hh;
			T_Time_v.Text = hh.ToString("00") + ":" + mm.ToString("00") + ":" + ss.ToString("00");
			refrash_timer.Start();
		}
	}

	public void Close_UI()
	{
		IsVisible = false;
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	public void LoadMultiLang()
	{
		Global.MultiLang.Load_language();
		T_BatteryRefresh.Text = Global.MultiLang.Translation("_BatteryRefresh");
		T_details.Text = Global.MultiLang.Translation("_MsgBATRefreshDetails");
		T_note.Text = Global.MultiLang.Translation("_note");
		T_note1.Text = Global.MultiLang.Translation("_MsgNote1");
		T_note2.Text = Global.MultiLang.Translation("_MsgNote2");
		T_note3.Text = Global.MultiLang.Translation("_MsgNote3");
		T_note4.Text = Global.MultiLang.Translation("_MsgNote4");
		T_note5.Text = Global.MultiLang.Translation("_MsgNote5");
		T_note6.Text = Global.MultiLang.Translation("_MsgNote6");
		BTN_start.Content = Global.MultiLang.Translation("_RefreshStart");
		BTN_stop.Content = Global.MultiLang.Translation("_Exit");
		T_Time.Text = Global.MultiLang.Translation("_ElapsedTime");
		T_FailNote.Text = Global.MultiLang.Translation("_MsgFailNote1");
		T_FailNote1.Text = Global.MultiLang.Translation("_MsgFailNote2");
		T_FailNote2.Text = Global.MultiLang.Translation("_MsgFailNote3");
		T_FailNote3.Text = Global.MultiLang.Translation("_MsgFailNote4");
		T_lastTime.Text = Global.MultiLang.Translation("_LastTime");
	}

	private void UpdateUI_RefreshStatus(RefreshStatus status)
	{
		Current_RefreshStatus = status;
		switch (status)
		{
		case RefreshStatus.Unexecuted:
			T_BatteryRefreshStatus.Text = Global.MultiLang.Translation("_Unexecuted");
			SP_ElapsedTime.IsVisible = false;
			break;
		case RefreshStatus.Feasible:
			T_BatteryRefreshStatus.Text = "";
			SP_ElapsedTime.IsVisible = false;
			break;
		case RefreshStatus.Infeasible:
			T_BatteryRefreshStatus.Text = Global.MultiLang.Translation("_Infeasible");
			SP_ElapsedTime.IsVisible = false;
			break;
		case RefreshStatus.Ongoing:
			T_BatteryRefreshStatus.Text = Global.MultiLang.Translation("_Ongoing");
			grid_result.IsVisible = false;
			grid_RunRefresh.IsVisible = true;
			SP_LastTime.IsVisible = false;
			SP_ElapsedTime.IsVisible = true;
			SP_Fail_note.IsVisible = false;
			SP_Fail_nothingBAT.IsVisible = false;
			R_Hidden_CloseIcon.IsVisible = true;
			BTN_start.IsEnabled = false;
			break;
		case RefreshStatus.Finish:
			Current_OngoingStatus = OngoingStatus.Stop;
			T_BatteryRefreshStatus.Text = Global.MultiLang.Translation("_Finish");
			SP_LastTime.IsVisible = false;
			T_lastTime_v.Text = DateTime.Now.ToShortDateString() + DateTime.Now.ToString(" HH:MM");
			Global.RW_REG.last_RefreshDate = T_lastTime_v.Text;
			R_.Width = 900.0;
			Update_RefreshResult();
			break;
		}
	}

	private void Update_InstantResult()
	{
		T_Result.Text = Global.MultiLang.Translation("_InstantDiagnosticsResult");
		CheckBattery_Result();
		if (Current_RefreshStatus != RefreshStatus.Infeasible)
		{
			if (Global.RW_REG.last_RefreshDate == "null" || Global.RW_REG.last_RefreshDate == "")
			{
				SP_LastTime.IsVisible = false;
				UpdateUI_RefreshStatus(RefreshStatus.Unexecuted);
			}
			else
			{
				SP_LastTime.IsVisible = true;
				T_lastTime_v.Text = Global.RW_REG.last_RefreshDate;
				UpdateUI_RefreshStatus(RefreshStatus.Feasible);
			}
		}
		Save_BatteryLog();
	}

	private void Update_RefreshResult()
	{
		T_Result.Text = Global.MultiLang.Translation("_BatteryRefreshDiagnosticsResult");
		CheckBattery_Result();
		Save_BatteryLog();
	}

	private void CheckBattery_Result()
	{
		Read_package_WMI7();
		if (CheckPF()) UpdateUI_Result(RefreshResult.PermanentFailure);
		else if (Replace_condition()) UpdateUI_Result(RefreshResult.NeedReplacement);
		else if (Deteriorated_condition()) UpdateUI_Result(RefreshResult.Deteriorated);
		else UpdateUI_Result(RefreshResult.Healthy);
	}

	public bool CheckPF() => ((myBatteryUtilityData.OperationStatus >> 12) & 1) == 1;

	/// <summary>Replaces the MSScriptControl VBScript Eval: expression of 0/1 digits, '+', '*', parentheses.</summary>
	static int Eval(string s)
	{
		int p = 0;
		int Sum()
		{
			int v = Prod();
			while (p < s.Length && s[p] == '+') { p++; v += Prod(); }
			return v;
		}
		int Prod()
		{
			int v = Atom();
			while (p < s.Length && s[p] == '*') { p++; v *= Atom(); }
			return v;
		}
		int Atom()
		{
			while (p < s.Length && s[p] == ' ') p++;
			if (s[p] == '(') { p++; int v = Sum(); if (s[p] != ')') throw new FormatException(); p++; return v; }
			int st = p;
			while (p < s.Length && char.IsDigit(s[p])) p++;
			if (st == p) throw new FormatException();
			return int.Parse(s.Substring(st, p - st));
		}
		int r = Sum();
		if (p != s.Length) throw new FormatException();
		return r;
	}

	static string Sub(string cond, string letter, bool one)
		=> cond.Replace(letter, one ? "1" : "0");

	public bool Deteriorated_condition()
	{
		string c = myBattery_ini.Warning_condition.ToLower();
		var d = myBatteryUtilityData; var ini = myBattery_ini;
		if (c.Contains("d")) c = Sub(c, "d", !(ini.R1Capacity >= d.Current_Capacity || d.Current_Capacity >= ini.WCapacity));
		if (c.Contains("i")) c = Sub(c, "i", !(ini.WCapacity >= d.Current_Capacity || d.Current_Capacity >= ini.W1Capacity));
		if (c.Contains("e")) c = Sub(c, "e", !(ini.WCycle >= d.CycleCount || d.CycleCount >= ini.R1Cycle));
		if (c.Contains("j")) c = Sub(c, "j", !(ini.W1Cycle >= d.CycleCount || d.CycleCount >= ini.WCycle));
		if (c.Contains("f"))
		{
			int num = (DateTime.Now.Year - 1980) * 512 + DateTime.Now.Month * 32 + DateTime.Now.Day;
			num = (num - d.ManufacturerDate) / 512;
			c = Sub(c, "f", !(ini.WDate <= num || num <= ini.RDate));
		}
		try { return Eval(c) > 0; } catch { return false; }
	}

	public bool Replace_condition()
	{
		string c = myBattery_ini.Replace_condition.ToLower();
		var d = myBatteryUtilityData; var ini = myBattery_ini;
		if (c.Contains("a")) c = Sub(c, "a", !(d.Current_Capacity >= ini.RCapacity));
		if (c.Contains("g")) c = Sub(c, "g", !(ini.RCapacity >= d.Current_Capacity || d.Current_Capacity >= ini.R1Capacity));
		if (c.Contains("b")) c = Sub(c, "b", !(ini.RCycle >= d.CycleCount));
		if (c.Contains("h")) c = Sub(c, "h", !(ini.R1Cycle >= d.Current_Capacity || d.Current_Capacity >= ini.R1Cycle));
		if (c.Contains("c"))
		{
			int num = (DateTime.Now.Year - 1980) * 512 + DateTime.Now.Month * 32 + DateTime.Now.Day;
			num = (num - d.ManufacturerDate) / 512;
			c = Sub(c, "c", !(num <= ini.RDate));
		}
		try { return Eval(c) > 0; } catch { return false; }
	}

	private void Read_battery_ini()
	{
		byte[] array = BatteryPackage.Read();
		myBattery_ini.RCapacity = array[32];
		myBattery_ini.R1Capacity = array[36];
		myBattery_ini.RCycle = (ushort)((array[34] << 8) + array[33]);
		myBattery_ini.R1Cycle = (ushort)((array[38] << 8) + array[37]);
		myBattery_ini.RDate = array[35];
		myBattery_ini.WCapacity = array[39];
		myBattery_ini.W1Capacity = array[43];
		myBattery_ini.WCycle = (ushort)((array[41] << 8) + array[40]);
		myBattery_ini.W1Cycle = (ushort)((array[45] << 8) + array[44]);
		myBattery_ini.WDate = array[42];
		byte[] array2 = new byte[32];
		try
		{
			if (array[48] != 0)
			{
				for (int i = 0; i < 32 && array2[i] != 36; i++) array2[i] = array[48 + i];
			}
			else array2 = Encoding.ASCII.GetBytes("(A+B+C)+(G*H)$");
			string text = Encoding.ASCII.GetString(array2);
			int length = text.IndexOf('$');
			myBattery_ini.Replace_condition = text.Substring(0, length);
			if (array[80] != 0)
			{
				for (int j = 0; j < 32 && array2[j] != 36; j++) array2[j] = array[80 + j];
			}
			else array2 = Encoding.ASCII.GetBytes("(D+E+F)+(I*J)$");
			text = Encoding.ASCII.GetString(array2);
			length = text.IndexOf('$');
			myBattery_ini.Warning_condition = text.Substring(0, length);
		}
		catch
		{
			// PORT: the original would throw here on a malformed package; fall back to the default criteria.
			myBattery_ini.Replace_condition = "(A+B+C)+(G*H)";
			myBattery_ini.Warning_condition = "(D+E+F)+(I*J)";
		}
	}

	private void Read_package_WMI7()
	{
		try
		{
			byte[] array = BatteryPackage.Read();
			myBatteryUtilityData.version = (ushort)((array[1] << 8) + array[0]);
			myBatteryUtilityData.ManufacturerDate = (ushort)((array[3] << 8) + array[2]);
			myBatteryUtilityData.CycleCount = (ushort)((array[5] << 8) + array[4]);
			myBatteryUtilityData.FullChargeCapacity = (ushort)((array[7] << 8) + array[6]);
			myBatteryUtilityData.DesignCapacity = (ushort)((array[9] << 8) + array[8]);
			myBatteryUtilityData.Current_Capacity = myBatteryUtilityData.FullChargeCapacity * 100 / myBatteryUtilityData.DesignCapacity;
			myBatteryUtilityData.BatteryStatus = (ushort)((array[11] << 8) + array[10]);
			DebugLog.WriteDbgLog2Outlog("BAT Status=" + myBatteryUtilityData.BatteryStatus);
			myBatteryUtilityData.PFStatus = (ushort)((array[15] << 24) + (array[14] << 16) + (array[13] << 8) + array[12]);
			myBatteryUtilityData.OperationStatus = (ushort)((array[19] << 24) + (array[18] << 16) + (array[17] << 8) + array[16]);
			myBatteryUtilityData.StopChargingThreshold = array[20];
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\wmi", "WMI_7", Convert.ToHexString(array));
		}
		catch { }
	}

	private void Save_BatteryLog()
	{
		try
		{
			int bAT_log_count = Global.RW_REG.BAT_log_count;
			string dir = CcPaths.Map("C:\\LOG\\");
			if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
			string path = CcPaths.Map("C:\\LOG\\" + DateTime.Now.ToString("LOGyyyMMdd99") + ".log");
			if (File.Exists(path))
			{
				path = CcPaths.Map("C:\\LOG\\" + DateTime.Now.ToString("LOGyyyMMdd" + bAT_log_count.ToString("00")) + ".log");
				bAT_log_count++;
				Global.RW_REG.BAT_log_count = bAT_log_count;
			}
			else
			{
				path = CcPaths.Map("C:\\LOG\\" + DateTime.Now.ToString("LOGyyyMMdd00") + ".log");
				int num = 0;
				while (File.Exists(path))
				{
					num++;
					path = CcPaths.Map("C:\\LOG\\" + DateTime.Now.ToString("LOGyyyMMdd" + num.ToString("00")) + ".log");
				}
			}
			using var w = new StreamWriter(path, append: false);
			w.WriteLine(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss"));
			w.WriteLine("\tRefresh Status          =" + T_BatteryRefreshStatus.Text);
			w.WriteLine("\tLast Refresh date       =" + T_lastTime_v.Text);
			w.WriteLine("\tManufacturer Date       =" + myBatteryUtilityData.ManufacturerDate);
			w.WriteLine("\tCycle Count             =" + myBatteryUtilityData.CycleCount);
			w.WriteLine("\tFull Charge Capacity    =" + myBatteryUtilityData.FullChargeCapacity);
			w.WriteLine("\tDesign Capacity         =" + myBatteryUtilityData.DesignCapacity);
			w.WriteLine("\tBattery Status          =" + myBatteryUtilityData.BatteryStatus);
			w.WriteLine("\tPF Status               =" + myBatteryUtilityData.PFStatus);
			w.WriteLine("\tOperation Status        =" + myBatteryUtilityData.OperationStatus);
			w.WriteLine("\tStop Charging Threshold =" + myBatteryUtilityData.StopChargingThreshold);
			w.WriteLine("\tJudgement Result        =" + T_Result_Velue.Text);
		}
		catch { }
	}

	private void UpdateUI_Result(RefreshResult result)
	{
		Color val = Color.FromArgb(255, 0, 255, 0);
		grid_note.IsVisible = true;
		SP_Fail_note.IsVisible = false;
		SP_Fail_nothingBAT.IsVisible = false;
		switch (result)
		{
		case RefreshResult.Healthy:
			T_Result_Velue.Text = Global.MultiLang.Translation("_Healthy");
			break;
		case RefreshResult.Deteriorated:
			T_Result_Velue.Text = Global.MultiLang.Translation("_Deteriorated");
			val = Color.FromArgb(255, 255, 255, 0);
			break;
		case RefreshResult.NeedReplacement:
			T_Result_Velue.Text = Global.MultiLang.Translation("_NeedReplacement");
			val = Color.FromArgb(255, 255, 0, 0);
			break;
		case RefreshResult.PermanentFailure:
			T_Result_Velue.Text = Global.MultiLang.Translation("_PermanentFailure");
			val = Color.FromArgb(255, 255, 0, 0);
			UpdateUI_RefreshStatus(RefreshStatus.Infeasible);
			SP_Fail_note.IsVisible = true;
			grid_note.IsVisible = false;
			SP_Fail_nothingBAT.IsVisible = false;
			break;
		}
		R_BATCapacity.Fill = new SolidColorBrush(val);
		grid_result.IsVisible = true;
	}

	private void BTN_start_Click(object sender, RoutedEventArgs e)
	{
		SetACOFF(1);
		EnableEnergySave(bEnable: false);
		if (BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online)
		{
			UpdateUI_RefreshStatus(RefreshStatus.Ongoing);
			UpdateUI_OngoingStatus(OngoingStatus.Discharge1);
			BAT_PowerStatus.Import_BatteryUtility_PowerPlan();
			BTN_stop.IsEnabled = true;
			DebugLog.WriteLog2Outlog("clevobattery^207^1");
			return;
		}
		Thread.Sleep(2000);
		BAT_PowerStatus.UpdatePowerInfo();
		if (BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online)
		{
			UpdateUI_RefreshStatus(RefreshStatus.Ongoing);
			UpdateUI_OngoingStatus(OngoingStatus.Discharge1);
			BAT_PowerStatus.Import_BatteryUtility_PowerPlan();
			BTN_stop.IsEnabled = true;
			DebugLog.WriteLog2Outlog("clevobattery^207^1");
		}
		else
		{
			DialogBoxNumber = 6;
			UC_DialogBox.Show(0, 6);
		}
	}

	private void EnableEnergySave(bool bEnable)
	{
		int num = !bEnable ? 83886080 : 83886081;
		Global.insyde.SetWMI(118, (uint)num);
	}

	private void Refrash_timer_Tick(object sender, EventArgs e)
	{
		ss++;
		if (ss > 59) { ss = 0; mm++; }
		if (mm > 59) { mm = 0; hh++; }
		T_Time_v.Text = hh.ToString("00") + ":" + mm.ToString("00") + ":" + ss.ToString("00");
		UpdatePowerStatus();
	}

	public void Update_Battery_Full_Status()
	{
		Read_package_WMI7();
		if ((myBatteryUtilityData.BatteryStatus & 0x10) == 16)
		{
			if (BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online) return;
			if (Current_OngoingStatus == OngoingStatus.Discharge1) UpdateUI_OngoingStatus(OngoingStatus.Charge1);
			else if (Current_OngoingStatus == OngoingStatus.Discharge2) UpdateUI_OngoingStatus(OngoingStatus.Charge2);
		}
		if ((myBatteryUtilityData.BatteryStatus & 0x20) == 32)
		{
			if (Current_OngoingStatus == OngoingStatus.Charge1) UpdateUI_OngoingStatus(OngoingStatus.Discharge2);
			else if (Current_OngoingStatus == OngoingStatus.Charge2) UpdateUI_OngoingStatus(OngoingStatus.AdjustingDischarge);
		}
	}

	public void Update_Battery_FullCharge_Status() { }

	public void Update_Battery_FullDischarge_Status()
	{
		Read_package_WMI7();
		if (Current_OngoingStatus == OngoingStatus.Discharge1) UpdateUI_OngoingStatus(OngoingStatus.Charge1);
		else if (Current_OngoingStatus == OngoingStatus.Discharge2) UpdateUI_OngoingStatus(OngoingStatus.Charge2);
	}

	public void ACOutEvent()
	{
		refrash_timer.Stop();
		DialogBoxNumber = 8;
		UC_DialogBox.Show(0, 8);
		if (Current_OngoingStatus == OngoingStatus.Discharge1 || Current_OngoingStatus == OngoingStatus.Discharge2)
			BatteryRefreshCommand(1, 1, 0);
	}

	public void BatteryRefreshCommand(int ACOFF, int Refresh, int FlexiCharge)
	{
		SetACOFF(ACOFF);
		SetBatteryRefresh(Refresh);
		SetBatteryFlexiDischange(FlexiCharge);
	}

	// DCHU 0x79 sub 0x1C (28): AC-off (1 = AC connected to system, 0 = AC cut so battery discharges)
	public void SetACOFF(int value)
	{
		Global.insyde.SetWMI(121, 28, (uint)value);
		DebugLog.WriteDbgLog2Outlog("SetACOFF= " + value);
	}
	// DCHU 0x76 sub 0x07: FlexiCharger discharge
	public void SetBatteryFlexiDischange(int value)
	{
		Global.insyde.SetWMI(118, 7, (uint)value);
		DebugLog.WriteDbgLog2Outlog("SetBatteryFlexiDischange= " + value);
	}
	// DCHU 0x79 sub 0x1D (29): battery refresh mode
	public void SetBatteryRefresh(int value) => Global.insyde.SetWMI(121, 29, (uint)value);

	private void UpdateUI_OngoingStatus(OngoingStatus stattus)
	{
		Global.RW_REG.OngoingStatus = (int)stattus;
		Current_OngoingStatus = stattus;
		Global.RW_REG.last_RefreshDate = T_lastTime_v.Text;
		byte b = 1, refresh = 0, flexiCharge = 1;
		switch (stattus)
		{
		case OngoingStatus.Stop:
			b = 1; refresh = 0; flexiCharge = 1;
			R_.Width = 0.0;
			refrash_timer.Stop();
			BAT_PowerStatus.Delete_BatteryUtility_PowerPlan();
			break;
		case OngoingStatus.Discharge1: b = 0; refresh = 1; flexiCharge = 0; R_.Width = 180.0; refrash_timer.Start(); break;
		case OngoingStatus.Charge1: b = 1; refresh = 1; flexiCharge = 0; R_.Width = 360.0; break;
		case OngoingStatus.Discharge2: b = 0; refresh = 1; flexiCharge = 0; R_.Width = 540.0; break;
		case OngoingStatus.Charge2: b = 1; refresh = 1; flexiCharge = 0; R_.Width = 720.0; break;
		case OngoingStatus.AdjustingDischarge: b = 1; refresh = 1; flexiCharge = 0; R_.Width = 900.0; break;
		}
		BatteryRefreshCommand(b, refresh, flexiCharge);
		if (b == 1) SetAC_ON_Finish = false;
	}

	private void BTN_stop_Click(object sender, RoutedEventArgs e)
	{
		BTN_stop.IsEnabled = false;
		if (Current_RefreshStatus == RefreshStatus.Finish) { DialogBoxNumber = 9; UC_DialogBox.Show(0, 9); }
		else { DialogBoxNumber = 7; UC_DialogBox.Show(0, 7); }
	}

	private void UC_DialogBox_CancelEvent()
	{
		BTN_start.IsEnabled = false;
		BTN_stop.IsEnabled = true;
		UC_DialogBox.IsVisible = false;
	}

	private void UC_DialogBox_OKEvent()
	{
		if (DialogBoxNumber != 8)
		{
			ss = 0; mm = 0; hh = 0;
			Dispatcher.UIThread.Post(() =>
			{
				Update_InstantResult();
				R_Hidden_CloseIcon.IsVisible = false;
				UpdateUI_OngoingStatus(OngoingStatus.Stop);
				BTN_start.IsEnabled = true;
				DebugLog.WriteLog2Outlog("clevobattery^207^0");
			});
		}
	}

	private void UpdatePowerStatus()
	{
		BAT_PowerStatus.UpdatePowerInfo();
		float num = BAT_PowerStatus.BatteryLifePercent;
		if (num > 100f) num = 100f;
		if (BAT_PowerStatus.BatteryChargeStatus == BatteryChargeStatus.NoSystemBattery)
		{
			UpdateUI_Result(RefreshResult.PermanentFailure);
			SP_Fail_nothingBAT.IsVisible = true;
			SP_Fail_note.IsVisible = false;
		}
		if (BAT_PowerStatus.PowerLineStatus == PowerLineStatus.Online)
		{
			I_AC.IsVisible = true;
			if (Current_RefreshStatus == RefreshStatus.Ongoing)
			{
				if (!refrash_timer.IsEnabled)
				{
					refrash_timer.Start();
					if (Current_OngoingStatus == OngoingStatus.Discharge1 || Current_OngoingStatus == OngoingStatus.Discharge2)
						BatteryRefreshCommand(0, 1, 0);
				}
				if (Current_OngoingStatus == OngoingStatus.Charge1 || Current_OngoingStatus == OngoingStatus.Charge2)
					SetAC_ON_Finish = true;
			}
		}
		else
		{
			I_AC.IsVisible = false;
			if ((Current_OngoingStatus == OngoingStatus.Charge1 || Current_OngoingStatus == OngoingStatus.Charge2) && SetAC_ON_Finish)
			{
				refrash_timer.Stop();
				DialogBoxNumber = 8;
				UC_DialogBox.Show(0, 8);
			}
		}
		if (Current_RefreshStatus == RefreshStatus.Ongoing)
		{
			if (T_BatteryValue.Text != num + "%")
			{
				Global.RW_REG.Refresh_ElapsedTime_ss = ss;
				Global.RW_REG.Refresh_ElapsedTime_mm = mm;
				Global.RW_REG.Refresh_ElapsedTime_hh = hh;
			}
			if (Current_OngoingStatus == OngoingStatus.AdjustingDischarge)
			{
				if ((float)(int)myBatteryUtilityData.StopChargingThreshold < BAT_PowerStatus.BatteryLifePercent)
					BatteryRefreshCommand(0, 1, 0);
				else
				{
					UpdateUI_OngoingStatus(OngoingStatus.Stop);
					UpdateUI_RefreshStatus(RefreshStatus.Finish);
				}
			}
		}
		T_BatteryValue.Text = num + "%";
		R_BATCapacity.Height = 2.75 * (double)num;
	}

	/// <summary>PORT: screenshot helper (--page). Forces UI states without touching hardware.</summary>
	public void DebugState(string s)
	{
		switch (s)
		{
		case "healthy": UpdateUI_Result(RefreshResult.Healthy); break;
		case "deteriorated": UpdateUI_Result(RefreshResult.Deteriorated); break;
		case "replace": UpdateUI_Result(RefreshResult.NeedReplacement); break;
		case "failure": UpdateUI_Result(RefreshResult.PermanentFailure); break;
		case "discharge1": UpdateUI_RefreshStatus(RefreshStatus.Ongoing); UpdateUI_OngoingStatus(OngoingStatus.Discharge1); BTN_stop.IsEnabled = true; break;
		case "charge1": UpdateUI_RefreshStatus(RefreshStatus.Ongoing); UpdateUI_OngoingStatus(OngoingStatus.Charge1); BTN_stop.IsEnabled = true; break;
		case "discharge2": UpdateUI_RefreshStatus(RefreshStatus.Ongoing); UpdateUI_OngoingStatus(OngoingStatus.Discharge2); BTN_stop.IsEnabled = true; break;
		case "charge2": UpdateUI_RefreshStatus(RefreshStatus.Ongoing); UpdateUI_OngoingStatus(OngoingStatus.Charge2); BTN_stop.IsEnabled = true; break;
		case "adjusting": UpdateUI_RefreshStatus(RefreshStatus.Ongoing); UpdateUI_OngoingStatus(OngoingStatus.AdjustingDischarge); BTN_stop.IsEnabled = true; break;
		case "finish": UpdateUI_RefreshStatus(RefreshStatus.Finish); BTN_stop.IsEnabled = true; break;
		case "dialog6": DialogBoxNumber = 6; UC_DialogBox.Show(0, 6); break;
		case "dialog7": DialogBoxNumber = 7; UC_DialogBox.Show(0, 7); break;
		case "dialog8": DialogBoxNumber = 8; UC_DialogBox.Show(0, 8); break;
		case "dialog9": DialogBoxNumber = 9; UC_DialogBox.Show(0, 9); break;
		}
	}
}
