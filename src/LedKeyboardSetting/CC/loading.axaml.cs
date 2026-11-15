using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Styling;
using Avalonia.Threading;

namespace CC;

/// <summary>Screen metrics (WPF SystemParameters.PrimaryScreenWidth/Height replacement, DIPs).</summary>
public static class SystemParameters
{
    public static double PrimaryScreenWidth = 1920, PrimaryScreenHeight = 1080;
    static bool _screenshot => CCenter.Platform.AppHost.IsScreenshot;
    public static void Init(Window w)
    {
        try
        {
            var s = w.Screens?.Primary;
            if (!_screenshot && s != null && s.Bounds.Width > 0)
            {
                PrimaryScreenWidth = s.Bounds.Width / s.Scaling;
                PrimaryScreenHeight = s.Bounds.Height / s.Scaling;
            }
        }
        catch { }
    }
}

public partial class loading : Window
{
	private double _dbRawWidth;

	private double _dbRawHeight;

	// Storyboard_load (Timeline.RepeatBehavior=Forever, 1.6 s): circle opacity key frames (time s, value)
	static readonly Dictionary<string, (double t, double v)[]> Frames = new()
	{
		["circle_1"] = new (double, double)[] { (0, 1), (0.2, 0), (0.4, 0.14), (0.6, 0.29), (0.8, 0.43), (1, 0.57), (1.2, 0.71), (1.4, 0.86), (1.6, 1) },
		["circle_8"] = new (double, double)[] { (0.2, 1), (0.4, 0), (0.6, 0.14), (0.8, 0.29), (1, 0.43), (1.2, 0.57), (1.4, 0.71), (1.6, 0.86) },
		["circle_7"] = new (double, double)[] { (0.2, 0.86), (0.4, 1), (0.6, 0), (0.8, 0.14), (1, 0.29), (1.2, 0.43), (1.4, 0.57), (1.6, 0.71) },
		["circle_6"] = new (double, double)[] { (0.2, 0.71), (0.4, 0.86), (0.6, 1), (0.8, 0), (1, 0.14), (1.2, 0.29), (1.4, 0.43), (1.6, 0.57) },
		["circle_5"] = new (double, double)[] { (0.2, 0.57), (0.4, 0.71), (0.6, 0.86), (0.8, 1), (1, 0), (1.2, 0.14), (1.4, 0.29), (1.6, 0.43) },
		["circle_4"] = new (double, double)[] { (0.2, 0.43), (0.4, 0.57), (0.6, 0.71), (0.8, 0.86), (1, 1), (1.2, 0), (1.4, 0.14), (1.6, 0.29) },
		["circle_3"] = new (double, double)[] { (0.2, 0.29), (0.4, 0.43), (0.6, 0.57), (0.8, 0.71), (1, 0.86), (1.2, 1), (1.4, 0), (1.6, 0.14) },
		["circle_2"] = new (double, double)[] { (0.2, 0.14), (0.4, 0.29), (0.6, 0.43), (0.8, 0.57), (1, 0.71), (1.2, 0.86), (1.4, 1), (1.6, 0) },
	};

	public loading()
	{
		InitializeComponent();
		SystemParameters.Init(this);
		_dbRawWidth = Width;
		_dbRawHeight = Height;
		FitWindowSize();
		Opened += (_, _) => StartAnimation();
	}

	private void StartAnimation()
	{
		const double total = 1.6;
		foreach (var kv in Frames)
		{
			var el = this.FindControl<Ellipse>(kv.Key);
			if (el == null) continue;
			var anim = new Animation { Duration = TimeSpan.FromSeconds(total), IterationCount = IterationCount.Infinite };
			var frames = kv.Value.ToList();
			if (frames[0].t > 0) frames.Insert(0, (0, el.Opacity));
			foreach (var f in frames)
				anim.Children.Add(new KeyFrame { Cue = new Cue(f.t / total), Setters = { new Setter(OpacityProperty, f.v) } });
			anim.RunAsync(el);
		}
	}

	private void FitWindowSize()
	{
		double primaryScreenWidth = SystemParameters.PrimaryScreenWidth;
		double primaryScreenHeight = SystemParameters.PrimaryScreenHeight;
		double num = 1920.0 / primaryScreenWidth * 1.2;
		double num2 = 1080.0 / primaryScreenHeight * 1.2;
		Width = _dbRawWidth / num;
		Height = _dbRawHeight / num2;
		WindowStartupLocation = WindowStartupLocation.CenterScreen;
	}
}
