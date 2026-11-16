using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using GPUOverclocking.UControl;
using CCenter.Platform;
using static GPUOverclocking.NVGPU;

namespace GPUOverclocking;

public partial class Page_system_monitor : UserControl
{
	private DispatcherTimer SystemMonitor_timer = new DispatcherTimer();
	private int SystemMonitor_Readcount = 0;
	private bool Change_CPUSkinColor = false;
	private string Lang_OFF = "OFF";
	private int Current_GPU_clock = 0;
	private int OLD_GPU_clock = 0;
	private int Current_GPU_clock_offset = 0;
	private int Dialogbox_number = 0;
	private int SituationalMode = 3;

	public Page_system_monitor()
	{
		InitializeComponent();
		slider_CoreInc.ValueChanged += slider_CoreInc_ValueChanged;
		comboBox_GPUMinc.SelectionChanged += comboBox_GPUMinc_SelectionChanged;
		slider_VRAMInc.ValueChanged += slider_VRAMInc_ValueChanged;
		comboBox_VRAMinc.SelectionChanged += comboBox_VRAMinc_SelectionChanged;
		slider_CoreInc1.ValueChanged += slider_CoreInc1_ValueChanged;
		comboBox_GPUMinc1.SelectionChanged += comboBox_GPUMinc1_SelectionChanged;
		slider_VRAMInc1.ValueChanged += slider_VRAMInc1_ValueChanged;
		comboBox_VRAMinc1.SelectionChanged += comboBox_VRAMinc1_SelectionChanged;
		BTN_Reset_GPUOC.Click += BTN_Reset_GPUOC_Click;
		BTN_SAVE_GPUOC.Click += BTN_SAVE_GPUOC_Click;
		// WPF ToggleButton.Checked
		RBTN_GPU1.IsCheckedChanged += (s, e) => { if (RBTN_GPU1.IsChecked == true) RBTN_GPU1_Checked(s, e); };
		RBTN_GPU2.IsCheckedChanged += (s, e) => { if (RBTN_GPU2.IsChecked == true) RBTN_GPU2_Checked(s, e); };
		RBTN_FAN.IsCheckedChanged += (s, e) => { if (RBTN_FAN.IsChecked == true) RBTN_FAN_Checked(s, e); };
		RBTN_CPU.IsCheckedChanged += (s, e) => { if (RBTN_CPU.IsChecked == true) RBTN_CPU_Checked(s, e); };
		RBTN_MEM.IsCheckedChanged += (s, e) => { if (RBTN_MEM.IsChecked == true) RBTN_MEM_Checked(s, e); };
		RBTN_GPU.IsCheckedChanged += (s, e) => { if (RBTN_GPU.IsChecked == true) RBTN_GPU_Checked(s, e); };
	}

	// WPF ComboBox.Text = "x" on a non-editable box selects the item whose text matches
	private static void SelectByText(ComboBox cb, string text)
	{
		if (cb.ItemsSource is System.Collections.IEnumerable items)
		{
			int i = 0;
			foreach (object item in items)
			{
				if (item.ToString() == text) { cb.SelectedIndex = i; return; }
				i++;
			}
		}
	}

	public void Init_SystemMonitor_UI()
	{
		RBTN_FAN.IsChecked = true;
		SystemMonitor_timer.Interval = TimeSpan.FromMilliseconds(1000.0);
		SystemMonitor_timer.Tick += SystemMonitor_timer_Tick;
		UC_DialogBox.CancelEvent += UC_DialogBox_CancelEvent;
		UC_DialogBox.OKEvent += UC_DialogBox_OKEvent;
		Init_NVIDIA();
		LoadMultiLang();
		LoadGPUSkinColor();
		ReadSituationalMode();
		// SystemEvents.PowerModeChanged (resume) has no Linux equivalent here; see PORT_NOTES.md
	}

	private void LoadGPUSkinColor()
	{
		try
		{
			byte b = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "GPUSkinColorR"));
			byte b2 = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "GPUSkinColorG"));
			byte b3 = Convert.ToByte(Global.RW_ini.Read_OEM("CustomSkinColor", "GPUSkinColorB"));
			Color val = Color.FromArgb(byte.MaxValue, b, b2, b3);
			Brush foreground = (Brush)new SolidColorBrush(val);
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
			Brush foreground = (Brush)new SolidColorBrush(val);
			info_CPU.Foreground = foreground;
		}
		catch
		{
			Change_CPUSkinColor = true;
			info_CPU.Foreground = bg_system_setting_tab.Fill;
		}
	}

	public void LoadMultiLang()
	{
		Global.MultiLang.Load_language();
		T_temp.Text = Global.MultiLang.Translation("_Temperature");
		T_temp1.Text = Global.MultiLang.Translation("_Temperature");
		T_Utilization.Text = Global.MultiLang.Translation("_Utilization");
		T_CPU_util.Text = Global.MultiLang.Translation("_Utilization");
		T_GPU_BOOST.Text = Global.MultiLang.Translation("_CoreBoost");
		T_GPU_BASE.Text = Global.MultiLang.Translation("_CoreBase");
		T_VRAM.Text = Global.MultiLang.Translation("_VRamClock");
		T_RAM.Text = Global.MultiLang.Translation("_MemoryClock");
		T_CPU_TurboMax.Text = Global.MultiLang.Translation("_TurboMax");
		T_CPU_TurboMin.Text = Global.MultiLang.Translation("_CoreBase");
		T_RAM_Size_Copy.Text = Global.MultiLang.Translation("_MemoryUsage");
		T_RAM_Size.Text = Global.MultiLang.Translation("_TotalMemorySize");
		T_CPU_voltage.Text = Global.MultiLang.Translation("_CPUVoltage");
		T_GPUSET.Text = Global.MultiLang.Translation("_GPUSetting");
		T_GPU_CoreINC.Text = Global.MultiLang.Translation("_CoreIncrement");
		T_GPU_MemINC.Text = Global.MultiLang.Translation("_MemoryIncrement");
		T_GPU_CoreINC1.Text = Global.MultiLang.Translation("_CoreIncrement");
		T_GPU_MemINC1.Text = Global.MultiLang.Translation("_MemoryIncrement");
		BTN_Reset_GPUOC.Content = Global.MultiLang.Translation("_Default");
		BTN_SAVE_GPUOC.Content = Global.MultiLang.Translation("_Save");
		Lang_OFF = Global.MultiLang.Translation("_OFF");
		if (SituationalMode == 0)
		{
			T_GPUSET_mode.Text = Global.MultiLang.Translation("_Quiet");
			T_GPUSET_mode1.Text = Global.MultiLang.Translation("_FN3");
		}
		else if (SituationalMode == 1)
		{
			T_GPUSET_mode.Text = Global.MultiLang.Translation("_PowerSaving");
			T_GPUSET_mode1.Text = Global.MultiLang.Translation("_FN3");
		}
		else if (SituationalMode == 3)
		{
			T_GPUSET_mode.Text = Global.MultiLang.Translation("_Entertainment");
			T_GPUSET_mode1.Text = Global.MultiLang.Translation("_FN3");
		}
		else if (PowerLine.IsOffline)
		{
			T_GPUSET_mode.Text = Global.MultiLang.Translation("_PlugInPower");
		}
	}

	public void Open_UI()
	{
		if (Change_CPUSkinColor)
		{
			info_CPU.Foreground = bg_system_setting_tab.Fill;
		}
		SystemMonitor_timer.Start();
		page_system_monitor.IsVisible = true;
	}

	public void Close_UI()
	{
		SystemMonitor_timer.Stop();
		page_system_monitor.IsVisible = false;
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
		Dispatcher.UIThread.Post(() =>
		{
			Read_NVIDIA_Data();
		});
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

	private void Init_NVIDIA()
	{
		if (InitGPU_API() == 0)
		{
			string text = Marshal.PtrToStringUni(Get_GPU_name());
			GPU_NAME_Value.Text = "NVIDIA " + text;
			int gPU_TotalNumber = Get_GPU_TotalNumber();
			if (gPU_TotalNumber <= 2)
			{
				if (gPU_TotalNumber == 1)
				{
					RBTN_GPU1.IsVisible = false;
					RBTN_GPU2.IsVisible = false;
				}
				if (Set_GPU_Number(0) == 0)
				{
					GPU_BASE_Value.Text = Get_GPU_Base_Clock().ToString();
					GPU_BOOST_Value.Text = Get_GPU_Boost_Clock().ToString();
					Read_NVIDIA_Data();
				}
			}
			Init_NVIDIA_OC();
		}
		else
		{
			UC_DialogBox.Show(0, 11);
		}
	}

	private void Read_NVIDIA_Data()
	{
		if (Check_NVStatus())
		{
			Current_GPU_clock = Get_GPU_Graphics_Clock();
			if (OLD_GPU_clock != Current_GPU_clock)
			{
				Current_GPU_clock_offset = (Current_GPU_clock - OLD_GPU_clock) / 19;
				GPU_Core_Value.Text = Current_GPU_clock.ToString();
				GPU_clock_Unit.IsVisible = true;
				if (Current_GPU_clock <= 139)
				{
					UC_GPUCoreClock.core_clock(1);
				}
				else
				{
					int num = Convert.ToInt32(GPU_BASE_Value.Text);
					int num2 = Convert.ToInt32(GPU_BOOST_Value.Text);
					if (num != 0)
					{
						if (num2 < Current_GPU_clock && Current_GPU_clock > num)
						{
							int num3 = (num2 - num) / 2;
							if (Current_GPU_clock - num < num3)
							{
								UC_GPUCoreClock.core_clock(11);
							}
							else
							{
								UC_GPUCoreClock.core_clock(12);
							}
						}
						else if (Current_GPU_clock > num2)
						{
							UC_GPUCoreClock.core_clock(12);
						}
						else
						{
							int level = Current_GPU_clock / (num / 10);
							UC_GPUCoreClock.core_clock(level);
						}
					}
				}
			}
			UpdateUI_VRAM_Clock(Get_GPU_Memory_Clock());
			UpdateUI_GPUUtiliztion(Get_GPU_Util());
			UpdateUI_GPUTemp(Get_GPU_Temp());
		}
		else
		{
			GPU_Core_Value.Text = Lang_OFF;
			GPU_clock_Unit.IsVisible = false;
			UC_GPUCoreClock.core_clock(0);
			UpdateUI_VRAM_Clock(0);
			UpdateUI_GPUTemp(0);
			GPUTemp_value.Text = Lang_OFF;
			UpdateUI_GPUUtiliztion(0);
		}
		if (R_mem1.IsVisible)
		{
			R_mem1.InvalidateVisual();
		}
		else if (R_mem2.IsVisible)
		{
			R_mem2.InvalidateVisual();
		}
		else if (R_mem3.IsVisible)
		{
			R_mem3.InvalidateVisual();
		}
		else if (R_mem4.IsVisible)
		{
			R_mem4.InvalidateVisual();
		}
		else if (R_mem5.IsVisible)
		{
			R_mem5.InvalidateVisual();
		}
	}

	private void Init_NVIDIA_OC()
	{
		bool flag = true;
		List<decimal> list = new List<decimal>();
		int num = 0;
		int selectedIndex = 0;
		for (int i = Get_GPU_Overclock_rangeMin(); i <= Get_GPU_Overclock_rangeMax(); i += 5)
		{
			list.Add(i);
			if (i == Get_GPU_Overclock_range())
			{
				selectedIndex = num;
			}
			num++;
		}
		slider_CoreInc.Maximum = list.Count - 1;   // last index (original: Count, one past the end)
		T_CoreIncMin.Text = list[0].ToString();
		T_CoreIncMax.Text = list[list.Count - 1].ToString();
		comboBox_GPUMinc.ItemsSource = list;
		comboBox_GPUMinc.SelectedIndex = selectedIndex;
		bool flag2 = true;
		List<decimal> list2 = new List<decimal>();
		int num2 = 0;
		int selectedIndex2 = 0;
		for (int j = Get_Memory_range_min(); j <= Get_Memory_range_max(); j += 5)
		{
			list2.Add(j);
			if (j == Get_Memory_range())
			{
				selectedIndex2 = num2;
			}
			num2++;
		}
		slider_VRAMInc.Maximum = list2.Count - 1;   // last index (original: Count, one past the end)
		T_MEMIncMin.Text = list2[0].ToString();
		T_MEMIncMax.Text = list2[list2.Count - 1].ToString();
		comboBox_VRAMinc.ItemsSource = list2;
		SelectByText(comboBox_VRAMinc, Get_Memory_range().ToString());
		comboBox_VRAMinc.SelectedIndex = selectedIndex2;
		if (Get_GPU_TotalNumber() > 1)
		{
			Set_GPU_Number(1);
			bool flag3 = true;
			List<decimal> list3 = new List<decimal>();
			int num3 = 0;
			int selectedIndex3 = 0;
			for (int k = Get_GPU_Overclock_rangeMin(); k <= Get_GPU_Overclock_rangeMax(); k += 5)
			{
				list3.Add(k);
				if (k == Get_GPU_Overclock_range())
				{
					selectedIndex3 = num3;
				}
				num3++;
			}
			slider_CoreInc1.Maximum = list3.Count - 1;   // last index (original: Count, one past the end)
			T_CoreIncMin1.Text = list3[0].ToString();
			T_CoreIncMax1.Text = list3[list3.Count - 1].ToString();
			comboBox_GPUMinc1.ItemsSource = list3;
			comboBox_GPUMinc1.SelectedIndex = selectedIndex3;
			bool flag4 = true;
			List<decimal> list4 = new List<decimal>();
			int num4 = 0;
			int selectedIndex4 = 0;
			for (int l = Get_Memory_range_min(); l <= Get_Memory_range_max(); l += 5)
			{
				list4.Add(l);
				if (l == Get_Memory_range())
				{
					selectedIndex4 = num4;
				}
				num4++;
			}
			slider_VRAMInc1.Maximum = list4.Count - 1;   // last index (original: Count, one past the end)
			T_MEMIncMin1.Text = list4[0].ToString();
			T_MEMIncMax1.Text = list4[list4.Count - 1].ToString();
			comboBox_VRAMinc1.ItemsSource = list4;
			comboBox_VRAMinc1.SelectedIndex = selectedIndex4;
			SP_GPUOC.Children.Remove(SP_G0);
		}
		else
		{
			SP_GPUOC.Children.Remove(SP_G2);
		}
		BTN_SAVE_GPUOC.IsEnabled = false;
	}

	private void UpdateUI_GPUCoreClock()
	{
		GPU_clock_Unit.IsVisible = true;
		int num = Convert.ToInt16(GPU_Core_Value.Text) + Current_GPU_clock_offset;
		if ((num < Current_GPU_clock && Current_GPU_clock_offset < 0) || (num > Current_GPU_clock && Current_GPU_clock_offset > 0))
		{
			GPU_Core_Value.Text = Current_GPU_clock.ToString();
		}
		else
		{
			GPU_Core_Value.Text = num.ToString();
		}
		if (SystemMonitor_Readcount == 19)
		{
			GPU_Core_Value.Text = Current_GPU_clock.ToString();
			OLD_GPU_clock = Current_GPU_clock;
		}
	}

	private void UpdateUI_GPUUtiliztion(int util)
	{
		if (!(GPU_util_value.Text == util.ToString()))
		{
			gpu_unilization_layer3.Width = (double)util * 8.3;
			GPU_util_value.Text = util.ToString();
		}
	}

	private void UpdateUI_VRAM_Clock(int clock)
	{
		R_mem1.IsVisible = false;
		R_mem2.IsVisible = false;
		R_mem3.IsVisible = false;
		R_mem4.IsVisible = false;
		R_mem5.IsVisible = false;
		VRAMClock_Value.IsVisible = true;
		if (clock == 0)
		{
			VRAMClock_Value.IsVisible = false;
		}
		else if (clock <= 405)
		{
			R_mem1.IsVisible = true;
		}
		else if (clock <= 810)
		{
			R_mem2.IsVisible = true;
		}
		else if (clock < 5000)
		{
			R_mem3.IsVisible = true;
		}
		else if (clock < Get_Memory_OC_max() - 4)
		{
			R_mem4.IsVisible = true;
		}
		else
		{
			R_mem5.IsVisible = true;
		}
		VRAMClock_Value.Text = clock.ToString();
	}

	// slider <-> dropdown mirror each other; the guard stops each change bouncing back
	private bool _syncing;

	private void comboBox_GPUMinc_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_syncing || comboBox_GPUMinc.SelectedIndex < 0) return;
		_syncing = true;
		try { slider_CoreInc.Value = comboBox_GPUMinc.SelectedIndex; } finally { _syncing = false; }
	}

	private void slider_CoreInc_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		BTN_SAVE_GPUOC.IsEnabled = true;
		int i = (int)Math.Round(slider_CoreInc.Value);
		if (_syncing || comboBox_GPUMinc.SelectedIndex == i) return;
		_syncing = true;
		try { comboBox_GPUMinc.SelectedIndex = i; } finally { _syncing = false; }
	}

	private void comboBox_VRAMinc_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_syncing || comboBox_VRAMinc.SelectedIndex < 0) return;
		_syncing = true;
		try { slider_VRAMInc.Value = comboBox_VRAMinc.SelectedIndex; } finally { _syncing = false; }
	}

	private void slider_VRAMInc_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		BTN_SAVE_GPUOC.IsEnabled = true;
		int i = (int)Math.Round(slider_VRAMInc.Value);
		if (_syncing || comboBox_VRAMinc.SelectedIndex == i) return;
		_syncing = true;
		try { comboBox_VRAMinc.SelectedIndex = i; } finally { _syncing = false; }
	}

	private void comboBox_GPUMinc1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_syncing || comboBox_GPUMinc1.SelectedIndex < 0) return;
		_syncing = true;
		try { slider_CoreInc1.Value = comboBox_GPUMinc1.SelectedIndex; } finally { _syncing = false; }
	}

	private void slider_CoreInc1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		BTN_SAVE_GPUOC.IsEnabled = true;
		int i = (int)Math.Round(slider_CoreInc1.Value);
		if (_syncing || comboBox_GPUMinc1.SelectedIndex == i) return;
		_syncing = true;
		try { comboBox_GPUMinc1.SelectedIndex = i; } finally { _syncing = false; }
	}

	private void comboBox_VRAMinc1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_syncing || comboBox_VRAMinc1.SelectedIndex < 0) return;
		_syncing = true;
		try { slider_VRAMInc1.Value = comboBox_VRAMinc1.SelectedIndex; } finally { _syncing = false; }
	}

	private void slider_VRAMInc1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		BTN_SAVE_GPUOC.IsEnabled = true;
		int i = (int)Math.Round(slider_VRAMInc1.Value);
		if (_syncing || comboBox_VRAMinc1.SelectedIndex == i) return;
		_syncing = true;
		try { comboBox_VRAMinc1.SelectedIndex = i; } finally { _syncing = false; }
	}

	private void BTN_SAVE_GPUOC_Click(object sender, RoutedEventArgs e)
	{
		Show_dialog_box_GPU();
	}

	private void BTN_Reset_GPUOC_Click(object sender, RoutedEventArgs e)
	{
		byte[] array = new byte[4];
		array = Global.insyde.GetAPPData(5, 2, 4);
		int num = 0;
		int num2 = 0;
		num = (((array[1] & 0x80) != 128) ? (array[0] + (array[1] << 8)) : (array[0] + (array[1] << 8) - 65535));
		num2 = (((array[3] & 0x80) != 128) ? (array[2] + (array[3] << 8)) : (array[2] + (array[3] << 8) - 65535));
		string text = AppDomain.CurrentDomain.BaseDirectory + "\\ControlGPU.exe";
		DebugLog.WriteDbgLog2Inlog("w_arg255^" + text + "^set_increment:0:" + num + ":" + num2);
		if (Get_GPU_TotalNumber() > 1)
		{
			DebugLog.WriteDbgLog2Inlog("w_arg255^" + text + "^set_increment:1:" + num + ":" + num2);
		}
		byte[] data = new byte[8]
		{
			array[0],
			array[1],
			array[2],
			array[3],
			array[0],
			array[1],
			array[2],
			array[3]
		};
		HardwareGate.GuardOcWrite(() => Global.insyde.SetAPPData(5, 16, 8, data));
		UpdateUI_GPUPOC();
	}

	public void UpdateUI_GPUPOC()
	{
		Set_GPU_Number(0);
		SelectByText(comboBox_GPUMinc, Get_GPU_Overclock_range().ToString());
		SelectByText(comboBox_VRAMinc, Get_Memory_range().ToString());
		if (Get_GPU_TotalNumber() > 1)
		{
			Set_GPU_Number(1);
			SelectByText(comboBox_GPUMinc1, Get_GPU_Overclock_range().ToString());
			SelectByText(comboBox_VRAMinc1, Get_Memory_range().ToString());
		}
		BTN_SAVE_GPUOC.IsEnabled = false;
	}

	private void UpdateUI_GPUTemp(int GPU_Temp_new)
	{
		if (!(GPUTemp_value.Text == GPU_Temp_new.ToString()))
		{
			double num = (double)GPU_Temp_new * 1.8 + 135.0;
			RotateTransform renderTransform = new RotateTransform(0.0 - num);
			GPUTemp_needle_layer1.RenderTransform = (Transform)(object)renderTransform;
			GPUTemp_needle_layer2.RenderTransform = (Transform)(object)renderTransform;
			GPUTemp_value.Text = GPU_Temp_new.ToString();
		}
	}

	private void UC_DialogBox_OKEvent()
	{
		Dispatcher.UIThread.Post(() =>
		{
			Console.WriteLine("get ok event");
			if (Dialogbox_number == 2)
			{
				Save_OC_GPU();
			}
			UC_DialogBox.IsVisible = false;
			GC.Collect();
			GC.WaitForPendingFinalizers();
			SystemMonitor_timer.Start();
		});
	}

	private void UC_DialogBox_CancelEvent()
	{
		UC_DialogBox.IsVisible = false;
		GC.Collect();
		GC.WaitForPendingFinalizers();
		SystemMonitor_timer.Start();
	}

	private void Show_dialog_box_GPU()
	{
		Dialogbox_number = 2;
		UC_DialogBox.Show(0, 1);
	}

	private void RBTN_GPU1_Checked(object sender, RoutedEventArgs e)
	{
		if (Set_GPU_Number(0) == 0)
		{
			GPU_BASE_Value.Text = Get_GPU_Base_Clock().ToString();
			GPU_BOOST_Value.Text = Get_GPU_Boost_Clock().ToString();
			Read_NVIDIA_Data();
		}
	}

	private void RBTN_GPU2_Checked(object sender, RoutedEventArgs e)
	{
		if (Set_GPU_Number(1) == 0)
		{
			GPU_BASE_Value.Text = Get_GPU_Base_Clock().ToString();
			GPU_BOOST_Value.Text = Get_GPU_Boost_Clock().ToString();
			Read_NVIDIA_Data();
		}
	}

	private void Save_OC_GPU()
	{
		BTN_SAVE_GPUOC.IsEnabled = false;
		int num = Convert.ToInt16(comboBox_GPUMinc.SelectedValue);
		byte[] array = new byte[2];
		if (num < 0)
		{
			array[0] = (byte)((num - 1) & 0xFF);
			array[1] = (byte)(num - 1 >> 8);
		}
		else
		{
			array[0] = (byte)(num & 0xFF);
			array[1] = (byte)(num >> 8);
		}
		HardwareGate.GuardOcWrite(() => Global.insyde.SetAPPData(5, 16, 2, (byte[])array.Clone()));
		int num2 = Convert.ToInt16(comboBox_VRAMinc.SelectedValue);
		if (num2 < 0)
		{
			array[0] = (byte)((num2 - 1) & 0xFF);
			array[1] = (byte)(num2 - 1 >> 8);
		}
		else
		{
			array[0] = (byte)(num2 & 0xFF);
			array[1] = (byte)(num2 >> 8);
		}
		HardwareGate.GuardOcWrite(() => Global.insyde.SetAPPData(5, 18, 2, array));
		string text = AppDomain.CurrentDomain.BaseDirectory + "\\ControlGPU.exe";
		DebugLog.WriteDbgLog2Inlog("w_arg255^" + text + "^set_increment:0:" + num + ":" + num2);
		if (Get_GPU_TotalNumber() > 1)
		{
			num = Convert.ToInt16(comboBox_GPUMinc1.SelectedValue);
			if (num < 0)
			{
				array[0] = (byte)((num - 1) & 0xFF);
				array[1] = (byte)(num - 1 >> 8);
			}
			else
			{
				array[0] = (byte)(num & 0xFF);
				array[1] = (byte)(num >> 8);
			}
			HardwareGate.GuardOcWrite(() => Global.insyde.SetAPPData(5, 20, 2, array));
			Set_CoreOC(1, num);
			num2 = Convert.ToInt16(comboBox_VRAMinc1.SelectedValue);
			if (num2 < 0)
			{
				array[0] = (byte)((num2 - 1) & 0xFF);
				array[1] = (byte)(num2 - 1 >> 8);
			}
			else
			{
				array[0] = (byte)(num2 & 0xFF);
				array[1] = (byte)(num2 >> 8);
			}
			HardwareGate.GuardOcWrite(() => Global.insyde.SetAPPData(5, 22, 2, array));
			DebugLog.WriteDbgLog2Inlog("w_arg255^" + text + "^set_increment:1:" + num + ":" + num2);
		}
	}

	public void ReadSituationalMode()
	{
		byte[] aPPData = Global.insyde.GetAPPData(1, 1, 1);
		SituationalMode = aPPData[0];
		if (aPPData[0] == 2)
		{
			if (PowerLine.IsOffline)
			{
				T_GPUSET_mode.IsVisible = true;
				T_GPUSET_mode.Text = Global.MultiLang.Translation("_PlugInPower");
				Setting_GPU.IsEnabled = false;
				SP_GPUOC.Opacity = 0.1;
				Grid_save_reset_GPU.Opacity = 0.1;
			}
			else
			{
				T_GPUSET_mode.IsVisible = false;
				Setting_GPU.IsEnabled = true;
				SP_GPUOC.Opacity = 1.0;
				Grid_save_reset_GPU.Opacity = 1.0;
			}
			T_GPUSET_mode1.IsVisible = false;
			SP_GPUOC.IsVisible = true;
			Grid_save_reset_GPU.IsVisible = true;
			UpdateUI_GPUPOC();
		}
		else
		{
			SP_GPUOC.IsVisible = false;
			Grid_save_reset_GPU.IsVisible = false;
			T_GPUSET_mode.IsVisible = true;
			T_GPUSET_mode1.IsVisible = true;
			if (aPPData[0] == 0)
			{
				T_GPUSET_mode.Text = Global.MultiLang.Translation("_Quiet");
			}
			else if (aPPData[0] == 1)
			{
				T_GPUSET_mode.Text = Global.MultiLang.Translation("_PowerSaving");
			}
			else
			{
				T_GPUSET_mode.Text = Global.MultiLang.Translation("_Entertainment");
			}
		}
	}


	// --page NAME (dev aid for screenshots; not in the original)
	public void DevPage(string name)
	{
		if (name == "dialog") Show_dialog_box_GPU();
		else if (name == "dialog10") UC_DialogBox.Show(0, 10);
		else if (name == "dialog11") UC_DialogBox.Show(0, 11);
		else if (name == "battery") { PowerLine.Force = true; SeededMock.Mode = 2; ReadSituationalMode(); }
		else if (name.StartsWith("mode=")) { SeededMock.Mode = int.Parse(name.Substring(5)); ReadSituationalMode(); }
		else if (name.StartsWith("core=")) slider_CoreInc.Value = double.Parse(name.Substring(5));
		else if (name.StartsWith("mem=")) slider_VRAMInc.Value = double.Parse(name.Substring(4));
	}
}
