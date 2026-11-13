using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace CC;

public partial class loading : Window
{
	private double _dbRawWidth;
	private double _dbRawHeight;
	static readonly double[] Seq = { 1, 0, 0.14, 0.29, 0.43, 0.57, 0.71, 0.86 };
	readonly DispatcherTimer _t = new() { Interval = TimeSpan.FromMilliseconds(30) };
	readonly DateTime _t0 = DateTime.Now;

	public loading()
	{
		InitializeComponent();
		_dbRawWidth = Width;
		_dbRawHeight = Height;
		FitWindowSize();
		var circles = new[] { circle_1, circle_2, circle_3, circle_4, circle_5, circle_6, circle_7, circle_8 };
		// Storyboard_load: every 0.2 s each circle moves one step along Seq (linear key frames), forever
		_t.Tick += (_, _) =>
		{
			double pos = ((DateTime.Now - _t0).TotalSeconds / 0.2) % 8.0;
			for (int k = 0; k < 8; k++)
			{
				double p = ((pos - k) % 8 + 8) % 8;
				int i = (int)p; double f = p - i;
				circles[k].Opacity = Seq[i] + (Seq[(i + 1) % 8] - Seq[i]) * f;
			}
		};
		_t.Start();
		Closed += (_, _) => _t.Stop();
	}

	private void FitWindowSize()
	{
		var scr = Screens?.Primary;
		double w = scr != null ? scr.Bounds.Width / scr.Scaling : 1920, h = scr != null ? scr.Bounds.Height / scr.Scaling : 1080;
		double num = 1920.0 / w * 1.2;
		double num2 = 1080.0 / h * 1.2;
		Width = _dbRawWidth / num;
		Height = _dbRawHeight / num2;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
	}
}
