using System;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Styling;

namespace GPUOverclocking;

public partial class loading : Window
{
	private double _dbRawWidth = 0.0;
	private double _dbRawHeight = 0.0;

	public loading()
	{
		InitializeComponent();
		_dbRawWidth = Width;
		_dbRawHeight = Height;
		Opened += (s, e) => { FitWindowSize(); StartAnimation(); };
	}

	private void FitWindowSize()
	{
		double primaryScreenWidth = 1920.0, primaryScreenHeight = 1080.0;
		var scr = Screens?.Primary;
		if (scr != null) { primaryScreenWidth = scr.Bounds.Width / scr.Scaling; primaryScreenHeight = scr.Bounds.Height / scr.Scaling; }
		double num = 1920.0 / primaryScreenWidth * 1.2;
		double num2 = 1080.0 / primaryScreenHeight * 1.2;
		Width = _dbRawWidth / num;
		Height = _dbRawHeight / num2;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
	}

	// Storyboard_load (RepeatBehavior=Forever, 1.6 s, linear key frames on Opacity)
	private void StartAnimation()
	{
		var frames = new (string name, (double t, double v)[] k)[]
		{
			("circle_1", new (double, double)[] { (0, 1), (0.2, 0), (0.4, 0.14), (0.6, 0.29), (0.8, 0.43), (1, 0.57), (1.2, 0.71), (1.4, 0.86), (1.6, 1) }),
			("circle_8", new (double, double)[] { (0.2, 1), (0.4, 0), (0.6, 0.14), (0.8, 0.29), (1, 0.43), (1.2, 0.57), (1.4, 0.71), (1.6, 0.86) }),
			("circle_7", new (double, double)[] { (0.2, 0.86), (0.4, 1), (0.6, 0), (0.8, 0.14), (1, 0.29), (1.2, 0.43), (1.4, 0.57), (1.6, 0.71) }),
			("circle_6", new (double, double)[] { (0.2, 0.71), (0.4, 0.86), (0.6, 1), (0.8, 0), (1, 0.14), (1.2, 0.29), (1.4, 0.43), (1.6, 0.57) }),
			("circle_5", new (double, double)[] { (0.2, 0.57), (0.4, 0.71), (0.6, 0.86), (0.8, 1), (1, 0), (1.2, 0.14), (1.4, 0.29), (1.6, 0.43) }),
			("circle_4", new (double, double)[] { (0.2, 0.43), (0.4, 0.57), (0.6, 0.71), (0.8, 0.86), (1, 1), (1.2, 0), (1.4, 0.14), (1.6, 0.29) }),
			("circle_3", new (double, double)[] { (0.2, 0.29), (0.4, 0.43), (0.6, 0.57), (0.8, 0.71), (1, 0.86), (1.2, 1), (1.4, 0), (1.6, 0.14) }),
			("circle_2", new (double, double)[] { (0.2, 0.14), (0.4, 0.29), (0.6, 0.43), (0.8, 0.57), (1, 0.71), (1.2, 0.86), (1.4, 1), (1.6, 0) }),
		};
		foreach (var (name, k) in frames)
		{
			var ell = this.FindControl<Control>(name);
			if (ell == null) continue;
			var anim = new Animation { Duration = TimeSpan.FromSeconds(1.6), IterationCount = IterationCount.Infinite, FillMode = FillMode.Both };
			foreach (var (t, v) in k)
			{
				var kf = new KeyFrame { Cue = new Cue(t / 1.6) };
				kf.Setters.Add(new Setter(OpacityProperty, v));
				anim.Children.Add(kf);
			}
			anim.RunAsync(ell);
		}
	}
}
