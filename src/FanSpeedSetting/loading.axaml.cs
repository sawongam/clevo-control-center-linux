using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace FanSpeedSetting;

public partial class loading : Window
{
	private double _dbRawWidth = 0.0;

	private double _dbRawHeight = 0.0;

	// original: Storyboard_load (8 circles, opacity ramps, 2.4 s, forever); reproduced as a phase-stepped timer
	readonly DispatcherTimer _anim = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(300) };
	int _phase;
	static readonly double[] Ramp = { 1, 0, 0.143, 0.286, 0.429, 0.571, 0.714, 0.857 };

	public loading()
	{
		InitializeComponent();
		_dbRawWidth = Width;
		_dbRawHeight = Height;
		FitWindowSize();
		var circles = new[] { circle_1, circle_2, circle_3, circle_4, circle_5, circle_6, circle_7, circle_8 };
		_anim.Tick += (_, _) =>
		{
			_phase = (_phase + 1) % 8;
			for (int i = 0; i < 8; i++) circles[i].Opacity = Ramp[(i - _phase + 8) % 8];
		};
		Opened += (_, _) => _anim.Start();
		Closed += (_, _) => _anim.Stop();
	}

	private void FitWindowSize()
	{
		var scr = Screens.Primary;
		double primaryScreenWidth = scr == null ? 1920 : scr.Bounds.Width / scr.Scaling;
		double primaryScreenHeight = scr == null ? 1080 : scr.Bounds.Height / scr.Scaling;
		double num = 1920.0 / primaryScreenWidth * 1.2;
		double num2 = 1080.0 / primaryScreenHeight * 1.2;
		Width = _dbRawWidth / num;
		Height = _dbRawHeight / num2;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
	}
}
