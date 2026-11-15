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
using CCenter.Platform;
using CC.Compat;
using System;
using System.ComponentModel;
using System.Diagnostics;

namespace CC.UControl;

public partial class UserControl_FAN : UserControl
{
	public DispatcherTimer timer;

	public int vaule;

	private double Angle_new_GPUFan = 360.0;

	public UserControl_FAN()
	{
		InitializeComponent();
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		timer = new DispatcherTimer();
		timer.Interval = TimeSpan.FromMilliseconds(100.0);
		timer.Tick += Timer_Tick;
		timer.Start();
	}

	private void Timer_Tick(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post((() =>
		{
			if (vaule == 0)
			{
				if (R_FanLayer2.IsVisible)
				{
					R_FanLayer2.IsVisible = false;
				}
				timer.Stop();
			}
			else
			{
				if (!R_FanLayer2.IsVisible)
				{
					R_FanLayer2.IsVisible = true;
				}
				UpdateUI_GPUFan(vaule);
			}
		}));
	}

	private void UpdateUI_GPUFan(int angle_offset)
	{
		Angle_new_GPUFan -= angle_offset;
		if (Angle_new_GPUFan < 0.0)
		{
			Angle_new_GPUFan = 360 - angle_offset;
		}
		RotateTransform renderTransform = new RotateTransform(Angle_new_GPUFan);
		R_FanLayer2.RenderTransform = (Transform)(object)renderTransform;
		R_FanLayer3.RenderTransform = (Transform)(object)renderTransform;
	}
}
