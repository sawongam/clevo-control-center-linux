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
using System.ComponentModel;
using System.Diagnostics;

namespace CC.UControl;

public partial class UserControl_ExpressKey_groups : UserControl
{
	public delegate void KeyDelayTimeHandler(string KeyName, bool action, int delay_time);

	public event KeyDelayTimeHandler KeyDelayTimeChangeEvent;

	public UserControl_ExpressKey_groups()
	{
		InitializeComponent();
	}

	public void AddKey(string KeyName, bool Make, int time)
	{
		UserControl_ExpressKey userControl_ExpressKey = new UserControl_ExpressKey();
		userControl_ExpressKey.UpdateKeyUI(KeyName, Make, time);
		WPanel_Key.Children.Add((Control)userControl_ExpressKey);
		userControl_ExpressKey.DelayTimeChangeEvent += K_DelayTimeChangeEvent;
	}

	public void Enable_DelayTime(bool enable)
	{
		if (enable)
		{
			WPanel_Key.IsHitTestVisible = true;
		}
		else
		{
			WPanel_Key.IsHitTestVisible = false;
		}
	}

	private void K_DelayTimeChangeEvent(string KeyName, bool action, int delay_time)
	{
		KeyDelayTimeChangeEvent(KeyName, action, delay_time);
	}

	public void RemoveALL()
	{
		WPanel_Key.Children.RemoveRange(0, WPanel_Key.Children.Count);
	}

	private void rectangle_IsEnabledChanged(object sender, AvaloniaPropertyChangedEventArgs e)
	{
		if (userControl.IsEnabled)
		{
			R_Disable.IsVisible = false;
		}
		else
		{
			R_Disable.IsVisible = true;
		}
	}
}
