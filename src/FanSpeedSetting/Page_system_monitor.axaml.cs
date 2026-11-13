using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using FanSpeedSetting.UControl;

namespace FanSpeedSetting;

public partial class Page_system_monitor : UserControl
{
	private DispatcherTimer SystemMonitor_timer = new DispatcherTimer();

	private int SystemMonitor_Readcount = 0;

	private bool SL_Fan_Init = false;

	private double TurboFan_MaxDuty = 255.0;

	private bool Change_CPUSkinColor = false;




	public Page_system_monitor()
	{
		InitializeComponent();
		WireEvents();
	}


	void WireEvents()
	{
		RBTN_GPU1.IsCheckedChanged += (o, e) => { if (RBTN_GPU1.IsChecked == true) RBTN_GPU1_Checked(o, e); };
		RBTN_GPU2.IsCheckedChanged += (o, e) => { if (RBTN_GPU2.IsChecked == true) RBTN_GPU2_Checked(o, e); };
		foreach (var cb in new[] { CB_Resume, CB_Week, CB_0, CB_1, CB_2, CB_3, CB_4, CB_5, CB_6 }) cb.Click += Scheduler_Click;
		CB_AM.SelectionChanged += Scheduler_SelectionChanged;
		CB_H.SelectionChanged += Scheduler_SelectionChanged;
		BTN_AntiDustNow.Click += BTN_AntiDustNow_Click;
		BTN_AntiDustSetting.Click += BTN_AntiDustSetting_Click;
		BTN_Reset_Fan.Click += BTN_Reset_Fan_Click;
		BTN_SAVE_Fan.Click += BTN_SAVE_Fan_Click;
		slider_FAN_AutoOffset.ValueChanged += slider_FAN_AutoOffset_ValueChanged;
		comboBox_FanAutoOffset.SelectionChanged += comboBox_FanAutoOffset_SelectionChanged;
		RB_customFanCPU.IsCheckedChanged += (o, e) => { if (RB_customFanCPU.IsChecked == true) RB_customFanCPU_Checked(o, e); };
		RB_customFanGPU1.IsCheckedChanged += (o, e) => { if (RB_customFanGPU1.IsChecked == true) RB_customFanGPU1_Checked(o, e); };
		RB_customFanGPU2.IsCheckedChanged += (o, e) => { if (RB_customFanGPU2.IsChecked == true) RB_customFanGPU2_Checked(o, e); };
		// WPF: Checked (state) then Click (hardware write). Avalonia raises Click on release, Checked via IsCheckedChanged.
		Hook(RB_FAN_auto, RB_FAN_auto_Checked, RB_FAN_auto_Click);
		Hook(RB_FAN_max, RB_FAN_max_Checked, RB_FAN_max_Click);
		Hook(RB_FAN_Maxq, RB_FAN_Maxq_Checked, RB_FAN_Maxq_Click);
		Hook(RB_FAN_Silent, RB_FAN_Silent_Checked, RB_FAN_Silent_Click);
		Hook(RB_FAN_custom, RB_FAN_custom_Checked, RB_FAN_custom_Click);
	}

	static void Hook(RadioButton rb, EventHandler<RoutedEventArgs> chk, EventHandler<RoutedEventArgs> click)
	{
		rb.IsCheckedChanged += (o, e) => { if (rb.IsChecked == true) chk(o, e); };
		rb.Click += click;
	}

	// ---- hidden test hooks (not in the original) ----
	internal void TestShowAntiDust() { Show_AntDustSetting_UI(); }

	internal void RunTestActions(string acts)
	{
		foreach (var a in acts.Split(';', StringSplitOptions.RemoveEmptyEntries))
		{
			var t = a.Split(':');
			switch (t[0])
			{
			case "click":
				{
					var c = this.FindControl<Button>(t[1]);
					if (c is RadioButton rb) rb.IsChecked = true;
					if (c != null) c.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
					break;
				}
			case "save-curve": BTN_SAVE_Fan.IsEnabled = true; BTN_SAVE_Fan.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); break;
			case "reset-curve": BTN_Reset_Fan.RaiseEvent(new RoutedEventArgs(Button.ClickEvent)); break;
			case "drag":
				{
					var uc = t[1] == "CPU" ? UC_FanTable_CPU : t[1] == "GPU1" ? UC_FanTable_GPU1 : UC_FanTable_GPU2;
					uc.TestDrag(int.Parse(t[2]), double.Parse(t[3], System.Globalization.CultureInfo.InvariantCulture), double.Parse(t[4], System.Globalization.CultureInfo.InvariantCulture));
					UC_FanTable_MouseMove(null, null);
					break;
				}
			}
		}
	}

	public void Init_SystemMonitor_UI()
	{
		SystemMonitor_timer.Interval = TimeSpan.FromMilliseconds(1000.0);
		SystemMonitor_timer.Tick += SystemMonitor_timer_Tick;
		UC_DialogBox.CancelEvent += UC_DialogBox_CancelEvent;
		UC_DialogBox.OKEvent += UC_DialogBox_OKEvent;
		GetCustomerID();
		string eCVersion = Global.RW_REG.ECVersion;
		string eCVersion2 = Global.insyde.GetECVersion();
		if (eCVersion != eCVersion2)
		{
			Global.RW_REG.ECVersion = eCVersion2;
			Global.support_bit.ECUpdate = true;
		}
		Init_Fan_Set_UI();
		SL_Fan_Init = true;
		LoadMultiLang();
		LoadGPUSkinColor();
		LoadCPUSkinColor();
		ReadSituationalMode();
		Open_UI();
	}

	public void GetCustomerID()
	{
		byte[] array = new byte[256];
		array = Global.insyde.GetWMIPackage(17);
		Global.support_bit.CustomerID = array[2];
	}

	private void LoadGPUSkinColor()
	{
		try
		{
			byte b = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "GPUSkinColorR"));
			byte b2 = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "GPUSkinColorG"));
			byte b3 = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "GPUSkinColorB"));
			Color val = Color.FromArgb(byte.MaxValue, b, b2, b3);
			Brush foreground = new SolidColorBrush(val);
			info_GPU.Foreground = foreground;
		}
		catch
		{
		}
	}

	private void LoadCPUSkinColor()
	{
		try
		{
			byte b = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "CPUSkinColorR"));
			byte b2 = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "CPUSkinColorG"));
			byte b3 = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "CPUSkinColorB"));
			Color val = Color.FromArgb(byte.MaxValue, b, b2, b3);
			Brush val2 = new SolidColorBrush(val);
		}
		catch
		{
			Change_CPUSkinColor = true;
		}
	}

	public void LoadMultiLang()
	{
		Global.MultiLang.Load_language();
		T_temp.Text = Global.MultiLang.Translation("_Temperature");
		F_speed.Text = Global.MultiLang.Translation("_FanSpeed");
		T_temp1.Text = Global.MultiLang.Translation("_Temperature");
		F_speed2.Text = Global.MultiLang.Translation("_FanSpeed");
		T_FANSET.Text = Global.MultiLang.Translation("_FanSpeedSetting");
		RB_FAN_auto.Content = Global.MultiLang.Translation("_AUTOMATIC");
		RB_FAN_max.Content = Global.MultiLang.Translation("_MAXIMUM");
		RB_FAN_custom.Content = Global.MultiLang.Translation("_Custom");
		BTN_Reset_Fan.Content = Global.MultiLang.Translation("_Default");
		BTN_SAVE_Fan.Content = Global.MultiLang.Translation("_Save");
		RB_customFanCPU.Content = Global.MultiLang.Translation("_CPUFan");
		RB_customFanGPU1.Content = Global.MultiLang.Translation("_GPUFan");
		RB_customFanGPU2.Content = Global.MultiLang.Translation("_GPUFan2");
		T_FanAutoOffset.Text = Global.MultiLang.Translation("_Offset");
		RB_FAN_Silent.Content = Global.MultiLang.Translation("_Silent");
		BTN_AntiDustSetting.Content = Global.MultiLang.Translation("_AntiDustSetting");
		BTN_AntiDustNow.Content = Global.MultiLang.Translation("_AntiDustNow");
		T_AntiDustScheduler.Text = Global.MultiLang.Translation("_AntiDustScheduler");
		CB_Resume.Content = Global.MultiLang.Translation("_AntiDustResume");
		CB_Week.Content = Global.MultiLang.Translation("_WeekSpecified");
		CB_0.Content = Global.MultiLang.Translation("_Sun");
		CB_1.Content = Global.MultiLang.Translation("_Mon");
		CB_2.Content = Global.MultiLang.Translation("_Tue");
		CB_3.Content = Global.MultiLang.Translation("_Wed");
		CB_4.Content = Global.MultiLang.Translation("_Thu");
		CB_5.Content = Global.MultiLang.Translation("_Fri");
		CB_6.Content = Global.MultiLang.Translation("_Sat");
		T_time.Text = Global.MultiLang.Translation("_Time");
		int selectedIndex = CB_AM.SelectedIndex;
		CB_AM.Items.Clear();
		CB_AM.Items.Add((object)Global.MultiLang.Translation("_AM"));
		CB_AM.Items.Add((object)Global.MultiLang.Translation("_PM"));
		CB_AM.SelectedIndex = selectedIndex;
		if (Global.fan.AntiDust_S_Hour >= 12)
		{
			CB_H.SelectedIndex = Global.fan.AntiDust_S_Hour - 12;
			CB_AM.SelectedIndex = 1;
		}
		else
		{
			CB_H.SelectedIndex = Global.fan.AntiDust_S_Hour;
			CB_AM.SelectedIndex = 0;
		}
	}

	public void Open_UI()
	{
		SystemMonitor_timer.Start();
		page_system_monitor.IsVisible = true;
		Global.RW_REG.barkup_VB_CPUFan_H = VB_CPUFan.Height;
		Global.RW_REG.barkup_VB_CPUFan_W = VB_CPUFan.Width;
		Global.RW_REG.barkup_VB_CPU_Temp_W = VB_temp.Width;
		Global.RW_REG.barkup_VB_CPU_Temp_H = VB_temp.Height;
		if (Global.support_bit.AntiDust_Fan)
		{
			if (Global.fan.AntiDust_RunStatus == 1)
			{
				Show_AntDustSetting_UI();
			}
			else
			{
				grid_fan_AntiDust.IsVisible = false;
			}
		}
		else
		{
			grid_fan_AntiDust.IsVisible = false;
		}
	}

	public void Close_UI()
	{
		SystemMonitor_timer.Stop();
		page_system_monitor.IsVisible = false;
		UC_FAN_GPU.Value = 0;
		UC_FAN_CPU.Value = 0;
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	public void UpdateUI_ModeV2()
	{
	}

	private void RBTN_GPU_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void RBTN_FAN_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void RBTN_MEM_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void RBTN_CPU_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void SystemMonitor_timer_Tick(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			if (SystemMonitor_Readcount != 0 && SystemMonitor_Readcount == 1)
			{
				Thread thread = new Thread(Thread_Read_FanSpeed);
				thread.Start();
				Read_FAN_Data();
			}
		}));
		if (SystemMonitor_Readcount == 1)
		{
			SystemMonitor_Readcount = 0;
		}
		else
		{
			SystemMonitor_Readcount++;
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	private void XTU_timer_Tick(object sender, EventArgs e)
	{
	}

	private void Read_FAN_Data()
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			UpdateUI_GPUFan();
			UpdateUI_CPUFan();
			UpdateUI_CPUTemp(Global.fan.Fan_CPU.remote);
			if (RBTN_GPU1.IsChecked == true)
			{
				UpdateUI_GPUTemp(Global.fan.Fan_GPU1.remote);
			}
			else if (RBTN_GPU2.IsChecked == true)
			{
				UpdateUI_GPUTemp(Global.fan.Fan_GPU2.remote);
			}
			if (Global.support_bit.AntiDust_Fan && grid_fan_AntiDust.IsVisible)
			{
				if (Global.fan.AntiDust_RunStatus == 1)
				{
					UpdatUI_AntiDust_Runing();
				}
				else
				{
					int num = Convert.ToInt16(T_CPUFan_percent_value.Text);
					int num2 = Convert.ToInt16(T_GPUFan_percent_value.Text);
					if (num > 50 || num2 > 50)
					{
						UpdatUI_AntiDust_Lock();
					}
					else
					{
						UpdatUI_AntiDust_Done();
					}
				}
			}
		}));
	}

	private void Thread_Read_FanSpeed()
	{
		Global.fan.Read_FanSpeed();
	}

	private void UpdateUI_GPUFan()
	{
		double num = 0.0;
		if (RBTN_GPU1.IsChecked == true)
		{
			num = Global.fan.Fan_GPU1.rpm;
		}
		else if (RBTN_GPU2.IsChecked == true)
		{
			num = Global.fan.Fan_GPU2.rpm;
		}
		if (num != 0.0)
		{
			num = 60.0 / (5.565217391304348E-05 * num);
			num *= 2.0;
			num = Math.Round(num, 0);
			num = Math.Round(num, 0);
		}
		T_GPUFan_RPM_value.Text = num.ToString();
		double num2 = 255.0;
		if (RB_FAN_custom.IsChecked == false)
		{
			num2 = TurboFan_MaxDuty;
		}
		int num3 = 0;
		if (RBTN_GPU1.IsChecked == true)
		{
			num3 = (int)Math.Round((double)(int)Global.fan.Fan_GPU1.duty / num2 * 100.0, 0);
		}
		else if (RBTN_GPU2.IsChecked == true)
		{
			num3 = (int)Math.Round((double)(int)Global.fan.Fan_GPU2.duty / num2 * 100.0, 0);
		}
		if (num3 > 100 && Global.fan.TurboFanStatus == 1)
		{
			T_GPUFan_percent_value.Text = "Turbo";
			F_unit.IsVisible = false;
			Color val = Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0);
			Brush foreground = new SolidColorBrush(val);
			T_GPUFan_percent_value.Foreground = foreground;
		}
		else
		{
			Color val2 = Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			Brush foreground2 = new SolidColorBrush(val2);
			T_GPUFan_percent_value.Foreground = foreground2;
			F_unit.IsVisible = true;
			if (num3 > 100)
			{
				T_GPUFan_percent_value.Text = "100";
			}
			else
			{
				T_GPUFan_percent_value.Text = num3.ToString();
			}
		}
		UC_FAN_GPU.Value = num3 / 2;
	}

	private void UpdateUI_CPUFan()
	{
		double num = Global.fan.Fan_CPU.rpm;
		if (num != 0.0)
		{
			num = 60.0 / (5.565217391304348E-05 * num);
			num *= 2.0;
			num = Math.Round(num, 0);
		}
		T_CPUFan_RPM_value.Text = num.ToString();
		int num2 = 0;
		num2 = ((RB_FAN_custom.IsChecked != true) ? ((int)Math.Round((double)(int)Global.fan.Fan_CPU.duty / TurboFan_MaxDuty * 100.0, 0)) : ((int)Math.Round((double)(int)Global.fan.Fan_CPU.duty / 255.0 * 100.0, 0)));
		if (num2 > 100 && Global.fan.TurboFanStatus == 1)
		{
			T_CPUFan_percent_value.Text = "Turbo";
			Color val = Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, (byte)0);
			Brush foreground = new SolidColorBrush(val);
			T_CPUFan_percent_value.Foreground = foreground;
			F_unit2.IsVisible = false;
		}
		else
		{
			Color val2 = Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			Brush foreground2 = new SolidColorBrush(val2);
			T_CPUFan_percent_value.Foreground = foreground2;
			F_unit2.IsVisible = true;
			if (num2 > 100)
			{
				T_CPUFan_percent_value.Text = "100";
			}
			else
			{
				T_CPUFan_percent_value.Text = num2.ToString();
			}
		}
		UC_FAN_CPU.Value = num2 / 2;
		UpdateUI_CPUFan(num2);
	}

	private void UpdateUI_CPUTemp(int CPU_Temp_new)
	{
		double num = (double)CPU_Temp_new * 1.8 + 225.0;
		RotateTransform renderTransform = new RotateTransform(num);
		CPUTemp_needle_layer1.RenderTransform = renderTransform;
		CPUTemp_needle_layer2.RenderTransform = renderTransform;
		T_cpu_temp_value.Text = CPU_Temp_new.ToString();
	}

	private void UpdateUI_GPUTemp(int GPU_Temp_new)
	{
		if (!(GPUTemp_value.Text == GPU_Temp_new.ToString()))
		{
			if (GPU_Temp_new == 20)
			{
				GPU_Temp_new = 0;
				GPUTemp_value.Text = "OFF";
			}
			else
			{
				GPUTemp_value.Text = GPU_Temp_new.ToString();
			}
			double num = (double)GPU_Temp_new * 1.8 + 225.0;
			RotateTransform renderTransform = new RotateTransform(num);
			GPUTemp_needle_layer1.RenderTransform = renderTransform;
			GPUTemp_needle_layer2.RenderTransform = renderTransform;
		}
	}

	private void UpdateUI_CPUFan(int percent)
	{
		if (!(GPUTemp_value.Text == percent.ToString()))
		{
			bool flag = percent == 20;
			if (percent > 100 && Global.fan.TurboFanStatus == 1)
			{
				percent = 110;
			}
			else if (percent > 100)
			{
				percent = 100;
			}
			double num = (double)percent * 1.8 + 225.0;
			RotateTransform renderTransform = new RotateTransform(num);
			CPUFAN_needle_layer3.RenderTransform = renderTransform;
			CPUFAN_needle_layer4.RenderTransform = renderTransform;
			if (percent > 100)
			{
				CPUFan_value1.Text = "Turbo";
				T_unit2.IsVisible = false;
			}
			else
			{
				CPUFan_value1.Text = percent.ToString();
				T_unit2.IsVisible = true;
			}
		}
	}

	public void Init_Fan_Set_UI()
	{
		Global.fan.Read_FanInfo();
		if (Global.support_bit.ACPIBridge_Load_Default || Global.RW_REG.First_run == 0 || Global.support_bit.ECUpdate)
		{
			DebugLog.WriteDbgLog2Outlog("First_run");
			Global.fan.WMI13_LoadDefault();
			Global.fan.Read_WMI13();
			if (Global.fan.SupportCustomFan)
			{
				Global.fan.Write_WMI14();
			}
			Global.fan.SetFanMode(0);
			Global.fan.SetFanOffset(0);
			Global.RW_REG.First_run = 1;
		}
		if (Global.fan.FanMode == 1)
		{
			RB_FAN_max.IsChecked = true;
		}
		else if (Global.fan.FanMode == 5)
		{
			RB_FAN_Maxq.IsChecked = true;
		}
		else if (Global.fan.FanMode == 6)
		{
			RB_FAN_custom.IsChecked = true;
		}
		else
		{
			RB_FAN_auto.IsChecked = true;
		}
		if (!Global.support_bit.FanOffset)
		{
			grid_fan_auto.IsVisible = false;
			if (Global.fan.FanOffset != 0)
			{
				Global.fan.FanOffset = 0;
				Global.fan.SetFanOffset(0);
			}
		}
		int num = 100;
		List<decimal> list = new List<decimal>();
		for (int i = 0; i <= num; i++)
		{
			list.Add(i);
		}
		slider_FAN_AutoOffset.Maximum = list.Count - 1;
		T_FanAutoMin.Text = list[0].ToString();
		T_FanAutoMax.Text = list[list.Count - 1].ToString();
		comboBox_FanAutoOffset.ItemsSource = list;
		comboBox_FanAutoOffset.SelectedIndex = Global.fan.FanOffset;
		if (!Global.support_bit.MaxQ)
		{
			SP_FANBTN.Children.Remove(SP_MaxQ);
		}
		if (!Global.support_bit.FanLess)
		{
			SP_FANBTN.Children.Remove(SP_Silent);
		}
		if (Global.support_bit.TurboFan)
		{
			GetTurboFanOffMaxDuty();
		}
		RBTN_GPU1.IsChecked = true;
		RBTN_GPU2.IsChecked = false;
		if (Global.fan.FanCount == 2)
		{
			RBTN_GPU1.IsVisible = false;
			RBTN_GPU2.IsVisible = false;
			if (Global.fan.SupportCustomFan)
			{
				UC_FanTable_CPU.Init(Global.fan.Fan_CPU);
				UC_FanTable_GPU1.Init(Global.fan.Fan_GPU1);
				UC_FanTable_GPU2.IsVisible = false;
				RB_customFanGPU2.IsVisible = false;
				UC_FanTable_CPU.FocusFanTableEvent += UC_FanTable_CPU_FocusFanTableEvent;
				UC_FanTable_GPU1.FocusFanTableEvent += UC_FanTable_GPU1_FocusFanTableEvent;
			}
			else
			{
				SP_FANBTN.Children.Remove(SP_Custom);
			}
		}
		else if (Global.fan.FanCount == 3)
		{
			RBTN_GPU1.IsVisible = true;
			RBTN_GPU2.IsVisible = true;
			if (Global.fan.SupportCustomFan)
			{
				UC_FanTable_CPU.Init(Global.fan.Fan_CPU);
				UC_FanTable_GPU1.Init(Global.fan.Fan_GPU1);
				UC_FanTable_GPU2.Init(Global.fan.Fan_GPU2);
				UC_FanTable_GPU1.FocusFanTableEvent += UC_FanTable_GPU1_FocusFanTableEvent;
				UC_FanTable_GPU2.FocusFanTableEvent += UC_FanTable_GPU2_FocusFanTableEvent;
			}
			else
			{
				SP_FANBTN.Children.Remove(SP_Custom);
			}
		}
		else
		{
			RBTN_GPU1.IsVisible = false;
			RBTN_GPU2.IsVisible = false;
			SP_FANBTN.Children.Remove(SP_Custom);
			info_FAN_CPU.IsVisible = true;
			grid_GPUfan.IsVisible = false;
			grid_CPUfan.IsVisible = false;
			info_temp_fan_gpu.IsVisible = false;
		}
		if (Global.support_bit.AntiDust_Fan)
		{
			if (Global.fan.AntiDust_S_Resume == 1)
			{
				CB_Resume.IsChecked = true;
			}
			else
			{
				CB_Resume.IsChecked = false;
			}
			if (Global.fan.AntiDust_S_Day == 0)
			{
				CB_Week.IsChecked = false;
			}
			else
			{
				if ((Global.fan.AntiDust_S_Day & 1) == 1)
				{
					CB_0.IsChecked = true;
				}
				else
				{
					CB_0.IsChecked = false;
				}
				if ((Global.fan.AntiDust_S_Day & 2) == 2)
				{
					CB_1.IsChecked = true;
				}
				else
				{
					CB_1.IsChecked = false;
				}
				if ((Global.fan.AntiDust_S_Day & 4) == 4)
				{
					CB_2.IsChecked = true;
				}
				else
				{
					CB_2.IsChecked = false;
				}
				if ((Global.fan.AntiDust_S_Day & 8) == 8)
				{
					CB_3.IsChecked = true;
				}
				else
				{
					CB_3.IsChecked = false;
				}
				if ((Global.fan.AntiDust_S_Day & 0x10) == 16)
				{
					CB_4.IsChecked = true;
				}
				else
				{
					CB_4.IsChecked = false;
				}
				if ((Global.fan.AntiDust_S_Day & 0x20) == 32)
				{
					CB_5.IsChecked = true;
				}
				else
				{
					CB_5.IsChecked = false;
				}
				if ((Global.fan.AntiDust_S_Day & 0x40) == 64)
				{
					CB_6.IsChecked = true;
				}
				else
				{
					CB_6.IsChecked = false;
				}
			}
			if (Global.fan.AntiDust_S_Hour >= 12)
			{
				CB_H.SelectedIndex = Global.fan.AntiDust_S_Hour - 12;
				CB_AM.SelectedIndex = 1;
			}
			else
			{
				CB_H.SelectedIndex = Global.fan.AntiDust_S_Hour;
				CB_AM.SelectedIndex = 0;
			}
		}
		else
		{
			grid_btn_AntiDustSetting.IsVisible = false;
		}
	}

	public void ReadSituationalMode()
	{
		byte[] aPPData = Global.insyde.GetAPPData(1, 1, 1);
		Global.fan.CurrentSituationalMode = aPPData[0];
		RB_FAN_auto.IsEnabled = true;
		RB_FAN_max.IsEnabled = true;
		RB_FAN_custom.IsEnabled = true;
		UI_TurboFan_GPU1.IsVisible = false;
		UI_TurboFan_GPU2.IsVisible = false;
		UI_TurboFan_CPU1.IsVisible = false;
		UI_TurboFan_CPU2.IsVisible = false;
		system_temperature_layer6_turbofan.IsVisible = false;
		Image_turbofan.IsVisible = false;
		Global.fan.TurboFanStatus = 0;
		if (aPPData[0] == 0)
		{
			RB_FAN_auto.Content = Global.MultiLang.Translation("_Silent");
			byte[] aPPData2 = Global.insyde.GetAPPData(4, 5, 1);
			Global.fan.FanMode = aPPData2[0];
			if (Global.fan.FanMode == 1)
			{
				RB_FAN_max.IsChecked = true;
			}
			else if (Global.fan.FanMode == 5)
			{
				RB_FAN_Maxq.IsChecked = true;
			}
			else if (Global.fan.FanMode == 6)
			{
				RB_FAN_custom.IsChecked = true;
			}
			else
			{
				RB_FAN_auto.IsChecked = true;
			}
		}
		else if (aPPData[0] == 2)
		{
			RB_FAN_auto.Content = Global.MultiLang.Translation("_AUTOMATIC");
			if (Global.support_bit.TurboFan)
			{
				byte[] aPPData3 = Global.insyde.GetAPPData(4, 8, 1);
				if (aPPData3[0] == 1)
				{
					system_temperature_layer6_turbofan.IsVisible = true;
					Image_turbofan.IsVisible = true;
					UI_TurboFan_GPU1.IsVisible = true;
					UI_TurboFan_GPU2.IsVisible = true;
					UI_TurboFan_CPU1.IsVisible = true;
					UI_TurboFan_CPU2.IsVisible = true;
					RB_FAN_custom.IsEnabled = false;
					Global.fan.TurboFanStatus = 1;
				}
				else
				{
					system_temperature_layer6_turbofan.IsVisible = false;
					Image_turbofan.IsVisible = false;
					UI_TurboFan_GPU1.IsVisible = false;
					UI_TurboFan_GPU2.IsVisible = false;
					UI_TurboFan_CPU1.IsVisible = false;
					UI_TurboFan_CPU2.IsVisible = false;
				}
			}
			byte[] aPPData4 = Global.insyde.GetAPPData(4, 5, 1);
			Global.fan.FanMode = aPPData4[0];
			if (Global.fan.FanMode == 1)
			{
				RB_FAN_max.IsChecked = true;
			}
			else if (Global.fan.FanMode == 5)
			{
				RB_FAN_Maxq.IsChecked = true;
			}
			else if (Global.fan.FanMode == 6)
			{
				RB_FAN_custom.IsChecked = true;
			}
			else
			{
				RB_FAN_auto.IsChecked = true;
			}
		}
		else
		{
			RB_FAN_auto.Content = Global.MultiLang.Translation("_AUTOMATIC");
			byte[] aPPData5 = Global.insyde.GetAPPData(4, 5, 1);
			Global.fan.FanMode = aPPData5[0];
			if (Global.fan.FanMode == 1)
			{
				RB_FAN_max.IsChecked = true;
			}
			else if (Global.fan.FanMode == 5)
			{
				RB_FAN_Maxq.IsChecked = true;
			}
			else if (Global.fan.FanMode == 6)
			{
				RB_FAN_custom.IsChecked = true;
			}
			else
			{
				RB_FAN_auto.IsChecked = true;
			}
		}
	}

	public void UpdateUI_TurboFanON()
	{
		system_temperature_layer6_turbofan.IsVisible = true;
		Image_turbofan.IsVisible = true;
		UI_TurboFan_GPU1.IsVisible = true;
		UI_TurboFan_GPU2.IsVisible = true;
		UI_TurboFan_CPU1.IsVisible = true;
		UI_TurboFan_CPU2.IsVisible = true;
		RB_FAN_custom.IsEnabled = false;
		RB_FAN_auto.IsChecked = true;
		Global.fan.TurboFanStatus = 1;
	}

	public void UpdateUI_TurboFanOFF()
	{
		system_temperature_layer6_turbofan.IsVisible = false;
		Image_turbofan.IsVisible = false;
		UI_TurboFan_GPU1.IsVisible = false;
		UI_TurboFan_GPU2.IsVisible = false;
		UI_TurboFan_CPU1.IsVisible = false;
		UI_TurboFan_CPU2.IsVisible = false;
		RB_FAN_custom.IsEnabled = true;
		RB_FAN_auto.IsChecked = true;
		Global.fan.TurboFanStatus = 0;
	}

	private void comboBox_FanAutoOffset_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		try
		{
			slider_FAN_AutoOffset.Value = comboBox_FanAutoOffset.SelectedIndex;
		}
		catch
		{
		}
	}

	private void slider_FAN_AutoOffset_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		comboBox_FanAutoOffset.SelectedIndex = (int)slider_FAN_AutoOffset.Value;
		if (SL_Fan_Init)
		{
			byte fanOffset = Convert.ToByte(comboBox_FanAutoOffset.SelectedValue);
			Global.fan.SetFanOffset(fanOffset);
		}
	}

	private void RB_FAN_auto_Checked(object sender, RoutedEventArgs e)
	{
		grid_fan_custom.IsVisible = false;
		if (SL_Fan_Init && Global.support_bit.FanOffset)
		{
			grid_fan_auto.IsVisible = true;
		}
		Grid_save_reset_fan.IsVisible = false;
	}

	private void RB_FAN_auto_Click(object sender, RoutedEventArgs e)
	{
		if (RB_FAN_auto.IsChecked == true)
		{
			Global.fan.SetFanMode(0);
		}
	}

	private void RB_FAN_max_Checked(object sender, RoutedEventArgs e)
	{
		grid_fan_custom.IsVisible = false;
		grid_fan_auto.IsVisible = false;
		Grid_save_reset_fan.IsVisible = false;
	}

	private void RB_FAN_max_Click(object sender, RoutedEventArgs e)
	{
		if (RB_FAN_max.IsChecked == true)
		{
			Global.fan.SetFanMode(1);
		}
	}

	private void RB_FAN_Maxq_Checked(object sender, RoutedEventArgs e)
	{
		grid_fan_custom.IsVisible = false;
		grid_fan_auto.IsVisible = false;
		Grid_save_reset_fan.IsVisible = false;
	}

	private void RB_FAN_Maxq_Click(object sender, RoutedEventArgs e)
	{
		if (RB_FAN_Maxq.IsChecked == true)
		{
			Global.fan.SetFanMode(5);
		}
	}

	private void RB_FAN_Silent_Checked(object sender, RoutedEventArgs e)
	{
		grid_fan_custom.IsVisible = false;
		grid_fan_auto.IsVisible = false;
		Grid_save_reset_fan.IsVisible = false;
	}

	private void RB_FAN_Silent_Click(object sender, RoutedEventArgs e)
	{
		if (RB_FAN_Silent.IsChecked == true)
		{
			Global.fan.SetFanMode(3);
		}
	}

	private void RB_FAN_custom_Checked(object sender, RoutedEventArgs e)
	{
		grid_fan_custom.IsVisible = true;
		grid_fan_auto.IsVisible = false;
		Grid_save_reset_fan.IsVisible = true;
	}

	private void RB_FAN_custom_Click(object sender, RoutedEventArgs e)
	{
		if (RB_FAN_custom.IsChecked == true)
		{
			Global.fan.SetFanMode(6);
		}
	}

	private void BTN_SAVE_Fan_Click(object sender, RoutedEventArgs e)
	{
		SP_customFan1.IsEnabled = false;
		UC_FanTable_CPU.Save_FanTable(1);
		UC_FanTable_GPU1.Save_FanTable(2);
		UC_FanTable_GPU2.Save_FanTable(3);
		Global.fan.Write_WMI14();
		BTN_SAVE_Fan.IsEnabled = false;
		SP_customFan1.IsEnabled = true;
	}

	private void BTN_Reset_Fan_Click(object sender, RoutedEventArgs e)
	{
		switch (Global.fan.FanCount)
		{
		case 1:
			UC_FanTable_CPU.Set_defult_FanTable(1);
			UC_FanTable_CPU.change_select_T(0);
			break;
		case 2:
			UC_FanTable_CPU.Set_defult_FanTable(1);
			UC_FanTable_GPU1.Set_defult_FanTable(2);
			UC_FanTable_CPU.change_select_T(0);
			UC_FanTable_GPU1.change_select_T(0);
			break;
		default:
			UC_FanTable_CPU.Set_defult_FanTable(1);
			UC_FanTable_GPU1.Set_defult_FanTable(2);
			UC_FanTable_GPU2.Set_defult_FanTable(3);
			UC_FanTable_CPU.change_select_T(0);
			UC_FanTable_GPU1.change_select_T(0);
			UC_FanTable_GPU2.change_select_T(0);
			break;
		}
		Global.fan.Write_WMI14();
		BTN_SAVE_Fan.IsEnabled = false;
	}

	private void UC_FanTable_MouseMove(object sender, PointerEventArgs e)
	{
		object tag = UC_FanTable_CPU.Tag;
		object tag2 = UC_FanTable_GPU1.Tag;
		object tag3 = UC_FanTable_GPU2.Tag;
		if (tag.ToString() == "1" || tag2.ToString() == "1" || tag3.ToString() == "1")
		{
			BTN_SAVE_Fan.IsEnabled = true;
		}
	}

	private void RB_customFanCPU_Checked(object sender, RoutedEventArgs e)
	{
		SP_customFan1.Children.RemoveAt(0);
		SP_customFan2.Children.RemoveAt(0);
		SP_customFan3.Children.RemoveAt(0);
		SP_customFan1.Children.Add(SP_FanTable_CPU);
		SP_customFan2.Children.Add(SP_FanTable_GPU1);
		SP_customFan3.Children.Add(SP_FanTable_GPU2);
		UC_FanTable_GPU1.change_select_T(0);
	}

	private void RB_customFanGPU1_Checked(object sender, RoutedEventArgs e)
	{
		Console.Error.WriteLine("TRACE gpu1 checked enter");
		SP_customFan1.Children.RemoveAt(0);
		SP_customFan2.Children.RemoveAt(0);
		SP_customFan3.Children.RemoveAt(0);
		SP_customFan1.Children.Add(SP_FanTable_GPU1);
		SP_customFan2.Children.Add(SP_FanTable_CPU);
		SP_customFan3.Children.Add(SP_FanTable_GPU2);
		UC_FanTable_CPU.change_select_T(0);
		UC_FanTable_GPU2.change_select_T(0);
		Console.Error.WriteLine("TRACE gpu1 checked exit");
	}

	private void RB_customFanGPU2_Checked(object sender, RoutedEventArgs e)
	{
		SP_customFan1.Children.RemoveAt(0);
		SP_customFan2.Children.RemoveAt(0);
		SP_customFan3.Children.RemoveAt(0);
		SP_customFan1.Children.Add(SP_FanTable_GPU2);
		SP_customFan2.Children.Add(SP_FanTable_GPU1);
		SP_customFan3.Children.Add(SP_FanTable_CPU);
		UC_FanTable_CPU.change_select_T(0);
		UC_FanTable_GPU1.change_select_T(0);
	}

	private void UC_FanTable_CPU_FocusFanTableEvent()
	{
		if (RB_customFanCPU.IsChecked != true)
		{
			SP_customFan1.Children.RemoveAt(0);
			SP_customFan2.Children.RemoveAt(0);
			SP_customFan3.Children.RemoveAt(0);
			SP_customFan1.Children.Add(SP_FanTable_CPU);
			SP_customFan2.Children.Add(SP_FanTable_GPU1);
			SP_customFan3.Children.Add(SP_FanTable_GPU2);
			UC_FanTable_GPU1.change_select_T(0);
			RB_customFanCPU.IsChecked = true;
		}
	}

	private void UC_FanTable_GPU1_FocusFanTableEvent()
	{
		if (RB_customFanGPU1.IsChecked != true)
		{
			SP_customFan1.Children.RemoveAt(0);
			SP_customFan2.Children.RemoveAt(0);
			SP_customFan3.Children.RemoveAt(0);
			SP_customFan1.Children.Add(SP_FanTable_GPU1);
			SP_customFan2.Children.Add(SP_FanTable_CPU);
			SP_customFan3.Children.Add(SP_FanTable_GPU2);
			UC_FanTable_CPU.change_select_T(0);
			UC_FanTable_GPU2.change_select_T(0);
			RB_customFanGPU1.IsChecked = true;
		}
	}

	private void UC_FanTable_GPU2_FocusFanTableEvent()
	{
		SP_customFan1.Children.RemoveAt(0);
		SP_customFan2.Children.RemoveAt(0);
		SP_customFan3.Children.RemoveAt(0);
		SP_customFan1.Children.Add(SP_FanTable_GPU2);
		SP_customFan2.Children.Add(SP_FanTable_GPU1);
		SP_customFan3.Children.Add(SP_FanTable_CPU);
		UC_FanTable_CPU.change_select_T(0);
		UC_FanTable_GPU1.change_select_T(0);
		RB_customFanGPU2.IsChecked = true;
	}

	private void UC_DialogBox_OKEvent()
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
		}));
	}

	private void UC_DialogBox_CancelEvent()
	{
	}

	private void RBTN_GPU1_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void RBTN_GPU2_Checked(object sender, RoutedEventArgs e)
	{
	}

	private void BTN_AntiDustSetting_Click(object sender, RoutedEventArgs e)
	{
		if (grid_fan_AntiDust.IsVisible)
		{
			grid_fan_AntiDust.IsVisible = false;
			Setting_FAN.IsVisible = true;
			SP_FANBTN.IsVisible = true;
			Color val = Color.FromArgb(byte.MaxValue, (byte)39, (byte)39, (byte)39);
			Brush background = new SolidColorBrush(val);
			BTN_AntiDustSetting.Background = background;
			VB_CPUFan.Height = Global.RW_REG.barkup_VB_CPUFan_H;
			VB_CPUFan.Width = Global.RW_REG.barkup_VB_CPUFan_W;
			VB_GPUFan.Height = Global.RW_REG.barkup_VB_CPUFan_H;
			VB_GPUFan.Width = Global.RW_REG.barkup_VB_CPUFan_W;
			VB_temp.Width = Global.RW_REG.barkup_VB_CPU_Temp_W;
			VB_temp.Height = Global.RW_REG.barkup_VB_CPU_Temp_H;
		}
		else
		{
			Show_AntDustSetting_UI();
		}
	}

	private void Show_AntDustSetting_UI()
	{
		grid_fan_AntiDust.IsVisible = true;
		Setting_FAN.IsVisible = false;
		SP_FANBTN.IsVisible = false;
		BTN_AntiDustSetting.Background = r_layer2.Fill;
		VB_CPUFan.Height = Global.RW_REG.barkup_VB_CPUFan_H * 1.45;
		VB_CPUFan.Width = Global.RW_REG.barkup_VB_CPUFan_W * 1.45;
		VB_GPUFan.Height = Global.RW_REG.barkup_VB_CPUFan_H * 1.45;
		VB_GPUFan.Width = Global.RW_REG.barkup_VB_CPUFan_W * 1.45;
		VB_temp.Width = Global.RW_REG.barkup_VB_CPU_Temp_W * 0.8;
		VB_temp.Height = Global.RW_REG.barkup_VB_CPU_Temp_H * 0.8;
		Global.fan.Read_AntiDust_Status();
		UpdateUI_AntiDust_Status();
	}

	private void BTN_AntiDustNow_Click(object sender, RoutedEventArgs e)
	{
		if (Global.fan.AntiDust_RunStatus == 0)
		{
			UpdatUI_AntiDust_Runing();
			Global.fan.RunAntiDustMode();
		}
		else
		{
			UpdatUI_AntiDust_Done();
			Global.fan.AntiDust_Done();
		}
	}

	public void UpdatUI_AntiDust_Runing()
	{
		if (!UC_FAN_CPU.AntiDustMode)
		{
			UC_FAN_CPU.AntiDustMode = true;
			UC_FAN_GPU.AntiDustMode = true;
		}
		BTN_AntiDustNow.Content = Global.MultiLang.Translation("_AntiDustRuning");
		BTN_AntiDustNow.IsEnabled = false;
		BTN_AntiDustSetting.IsVisible = false;
	}

	public void UpdateUI_AntiDust_Status()
	{
		if (Global.fan.AntiDust_RunStatus == 1)
		{
			UpdatUI_AntiDust_Runing();
			if (!grid_fan_AntiDust.IsVisible)
			{
				Show_AntDustSetting_UI();
			}
			return;
		}
		int num = Convert.ToInt16(T_CPUFan_percent_value.Text);
		int num2 = Convert.ToInt16(T_GPUFan_percent_value.Text);
		if (num > 50 || num2 > 50 || Global.fan.FanMode == 1)
		{
			UpdatUI_AntiDust_Lock();
		}
		else
		{
			UpdatUI_AntiDust_Done();
		}
	}

	public void UpdatUI_AntiDust_Done()
	{
		if (UC_FAN_CPU.AntiDustMode)
		{
			UC_FAN_CPU.AntiDustMode = false;
			UC_FAN_GPU.AntiDustMode = false;
		}
		BTN_AntiDustNow.IsEnabled = true;
		BTN_AntiDustNow.Content = Global.MultiLang.Translation("_AntiDustNow");
		BTN_AntiDustSetting.IsVisible = true;
	}

	public void UpdatUI_AntiDust_Lock()
	{
		BTN_AntiDustNow.Content = Global.MultiLang.Translation("_AntiDustNote");
		BTN_AntiDustNow.IsEnabled = false;
		BTN_AntiDustSetting.IsVisible = true;
	}

	private void Set_AntiDustFan_Scheduler()
	{
		int num = 0;
		int hour = 0;
		if (CB_Week.IsChecked == true)
		{
			hour = ((CB_AM.SelectedIndex != 1) ? CB_H.SelectedIndex : (CB_H.SelectedIndex + 12));
			if (CB_0.IsChecked == true)
			{
				num |= 1;
			}
			if (CB_1.IsChecked == true)
			{
				num |= 2;
			}
			if (CB_2.IsChecked == true)
			{
				num |= 4;
			}
			if (CB_3.IsChecked == true)
			{
				num |= 8;
			}
			if (CB_4.IsChecked == true)
			{
				num |= 0x10;
			}
			if (CB_5.IsChecked == true)
			{
				num |= 0x20;
			}
			if (CB_6.IsChecked == true)
			{
				num |= 0x40;
			}
			if (num == 0)
			{
				CB_Week.IsChecked = false;
				CB_0.IsChecked = true;
				CB_1.IsChecked = true;
				CB_2.IsChecked = true;
				CB_3.IsChecked = true;
				CB_4.IsChecked = true;
				CB_5.IsChecked = true;
				CB_6.IsChecked = true;
			}
		}
		if (CB_Resume.IsChecked == true)
		{
			Global.fan.SetAntiDust_Scheduler(1, num, hour);
		}
		else
		{
			Global.fan.SetAntiDust_Scheduler(0, num, hour);
		}
	}

	private void Scheduler_Click(object sender, RoutedEventArgs e)
	{
		Set_AntiDustFan_Scheduler();
	}

	private void Scheduler_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (SL_Fan_Init)
		{
			Set_AntiDustFan_Scheduler();
		}
	}

	public void GetTurboFanOffMaxDuty()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		byte[] array2 = new byte[24];
		array[0] = 1;
		array[1] = 11;
		array[2] = 0;
		array[3] = 0;
		array[4] = 0;
		array[5] = 0;
		array[6] = 192;
		int num = Global.insyde.SetWMIPackageEx(4, array, ref o_buffer);
		DebugLog.WriteDbgLog2Outlog("turbo fan max=" + o_buffer[2]);
		TurboFan_MaxDuty = (int)o_buffer[2];
	}
}
