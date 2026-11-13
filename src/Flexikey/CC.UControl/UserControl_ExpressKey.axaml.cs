using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using RippleCC.Platform;
using CC.Compat;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace CC.UControl;

public partial class UserControl_ExpressKey : UserControl
{
	public delegate void DelayTimeHandler(string KeyName, bool action, int delay_time);

	public event DelayTimeHandler DelayTimeChangeEvent;

	public UserControl_ExpressKey()
	{
		InitializeComponent();
	}

	public void UpdateKeyUI(string KeyName, bool action, int delay_time)
	{
		T_KeyName.Text = KeyName;
		if (action)
		{
			path_down.IsVisible = true;
			path_up.IsVisible = false;
		}
		else
		{
			path_down.IsVisible = false;
			path_up.IsVisible = true;
		}
		TB_Delay_Time.Text = delay_time.ToString();
	}

	private void TB_Delay_Time_KeyUp(object sender, KeyEventArgs e)
	{
		int num = 0;
		bool flag = true;
		try
		{
			num = Convert.ToInt32(TB_Delay_Time.Text);
			if (num > 60000)
			{
				num = 60000;
			}
		}
		catch
		{
			num = 0;
		}
		TB_Delay_Time.Text = num.ToString();
		flag = path_down.IsVisible;
		DelayTimeChangeEvent(T_KeyName.Text, flag, num);
	}

	private void main_IsHitTestVisibleChanged(object sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (main.IsHitTestVisible)
		{
			T_Delay_Time0.IsVisible = false;
			TB_Delay_Time.IsVisible = true;
		}
		else
		{
			T_Delay_Time0.IsVisible = true;
			TB_Delay_Time.IsVisible = false;
		}
	}
}
