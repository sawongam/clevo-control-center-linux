using System;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
namespace CC;
public partial class loading : Window
{
	private double _dbRawWidth = 0.0;
	private double _dbRawHeight = 0.0;
	// Storyboard_load: 8 circles, linear key frames, repeat forever (period 1.6 s). Values from the XAML.
	static readonly double[] Seq = { 1, 0, 0.14, 0.29, 0.43, 0.57, 0.71, 0.86, 1 }; // circle_1 at t = 0,0.2..1.6
	readonly Ellipse[] _c = new Ellipse[8];
	readonly DispatcherTimer _t = new() { Interval = TimeSpan.FromMilliseconds(16) };
	readonly Stopwatch _sw = Stopwatch.StartNew();
	public loading()
	{
		InitializeComponent();
		for (int i = 0; i < 8; i++) _c[i] = this.FindControl<Ellipse>("circle_" + (i + 1));
		_dbRawWidth = Width;
		_dbRawHeight = Height;
		FitWindowSize();
		_t.Tick += (_, _) => Animate();
		_t.Start();
	}
	// circle k (0-based) is circle_1's pattern advanced by k steps of 0.2 s, going backwards (circle_8 lags by 1 step)
	double Val(int k, double t)
	{
		double u = (t % 1.6) / 0.2;           // 0..8
		double pos = (u - k) % 8; if (pos < 0) pos += 8;
		int i = (int)pos; double f = pos - i;
		return Seq[i] + (Seq[i + 1] - Seq[i]) * f;
	}
	void Animate()
	{
		double t = _sw.Elapsed.TotalSeconds;
		// circle_1 = k0, circle_8 = k1, circle_7 = k2 ... circle_2 = k7
		for (int n = 0; n < 8; n++) _c[n].Opacity = Val(n == 0 ? 0 : 8 - n, t);
	}
	private void FitWindowSize()
	{
		var scr = Screens.Primary;
		double w = scr != null ? scr.Bounds.Width / scr.Scaling : 1920;
		double h = scr != null ? scr.Bounds.Height / scr.Scaling : 1080;
		double num = 1920.0 / w * 1.2;
		double num2 = 1080.0 / h * 1.2;
		Width = _dbRawWidth / num;
		Height = _dbRawHeight / num2;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
	}
}
