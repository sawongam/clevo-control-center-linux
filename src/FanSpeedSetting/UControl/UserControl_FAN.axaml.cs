using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Threading;

namespace FanSpeedSetting.UControl;

public partial class UserControl_FAN : UserControl
{
	private int _value = 0;

	private bool _AntiDustMode = false;

	// original: System.Windows.Forms.Timer, Interval 50
	private DispatcherTimer FanAngle_timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };

	private int angle = 0;

	private int now_angle_lange = 360;

	private double angle_antidust = 360.0;

	private double now_angle_lange_antidust = 0.0;

	private int p = 0;

	// Original bound RotateTransform.Angle to this control's FontSize (rotGrid) and to R_FanLayer3.StrokeDashOffset;
	// Avalonia cannot bind transform properties by element name, so the angles are applied directly.
	private double _fontSize = 360.0;

	double FontSizeAngle
	{
		get => _fontSize;
		set { _fontSize = value; rotGrid.RenderTransform = new RotateTransform(value); }
	}

	double DashOffset
	{
		get => R_FanLayer3.StrokeDashOffset;
		set
		{
			R_FanLayer3.StrokeDashOffset = value;
			R_FanLayer3.RenderTransform = new RotateTransform(value);
		}
	}

	public int Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value != value)
			{
				_value = value;
				Fanpercent(value);
			}
		}
	}

	public bool AntiDustMode
	{
		get
		{
			return _AntiDustMode;
		}
		set
		{
			_AntiDustMode = value;
			if (_AntiDustMode)
			{
				FontSizeAngle = 360.0;
				angle = 0;
				angle_antidust = 360.0;
				R_FanLayer2.IsVisible = false;
				R_FanLayer2_AntiDustMode2.IsVisible = true;
			}
			else
			{
				R_FanLayer2.IsVisible = true;
				R_FanLayer2_AntiDustMode2.IsVisible = false;
				DashOffset = 360.0;
			}
		}
	}

	private void UserControl_Loaded(object sender, Avalonia.Interactivity.RoutedEventArgs e)
	{
		AntiDustMode = false;
	}

	public UserControl_FAN()
	{
		InitializeComponent();
		FanAngle_timer.Tick += FanAngle_timer_Tick;
		FontSizeAngle = 360.0;
		Loaded += UserControl_Loaded;
	}

	private void FanAngle_timer_Tick(object sender, EventArgs e)
	{
		if (!AntiDustMode)
		{
			angle += now_angle_lange;
			FontSizeAngle = angle;
			if (angle >= 360)
			{
				angle = now_angle_lange;
			}
		}
		else
		{
			angle_antidust -= now_angle_lange;
			DashOffset = angle_antidust;
			if (angle_antidust <= 0.0)
			{
				angle_antidust = 360.0 - angle_antidust;
			}
		}
	}

	public void Fanpercent(int percent)
	{
		p = percent / 2;
		if (percent == 0)
		{
			FanAngle_timer.Stop();
			return;
		}
		FanAngle_timer.Start();
		if (percent > 1 && (double)percent <= 12.5)
		{
			now_angle_lange = 2;
		}
		else if ((double)percent > 12.5 && percent <= 25)
		{
			now_angle_lange = 10;
		}
		else if (percent > 25 && (double)percent <= 37.5)
		{
			now_angle_lange = 18;
		}
		else if ((double)percent > 37.5 && percent <= 50)
		{
			now_angle_lange = 36;
		}
	}
}
