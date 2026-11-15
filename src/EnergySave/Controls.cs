using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
namespace DesktopBridge_EnergySave;

/// <summary>ProgressCircle.xaml: background ellipse, pie (Expression Arc, thickness 1000 = full pie) and a ring-border path.</summary>
public class ProgressCircle : Control
{
    public static readonly StyledProperty<IBrush> IndicatorBrushProperty = AvaloniaProperty.Register<ProgressCircle, IBrush>(nameof(IndicatorBrush));
    public static readonly StyledProperty<IBrush> BackgroundBrushProperty = AvaloniaProperty.Register<ProgressCircle, IBrush>(nameof(BackgroundBrush));
    public static readonly StyledProperty<IBrush> ProgressBorderBrushProperty = AvaloniaProperty.Register<ProgressCircle, IBrush>(nameof(ProgressBorderBrush));
    public static readonly StyledProperty<int> ValueProperty = AvaloniaProperty.Register<ProgressCircle, int>(nameof(Value));
    public IBrush IndicatorBrush { get => GetValue(IndicatorBrushProperty); set => SetValue(IndicatorBrushProperty, value); }
    public IBrush BackgroundBrush { get => GetValue(BackgroundBrushProperty); set => SetValue(BackgroundBrushProperty, value); }
    public IBrush ProgressBorderBrush { get => GetValue(ProgressBorderBrushProperty); set => SetValue(ProgressBorderBrushProperty, value); }
    public int Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    static ProgressCircle() { AffectsRender<ProgressCircle>(IndicatorBrushProperty, BackgroundBrushProperty, ProgressBorderBrushProperty, ValueProperty); }

    const string PathData = "M146,50.499996 C198.74319,50.499996 241.5,88.108078 241.5,134.5 C241.5,180.89192 198.74319,218.5 146,218.5 C93.256805,218.5 50.5,180.89192 50.500002,134.5 C50.5,88.108078 93.256805,50.499996 146,50.499996 M146,15.499996 C137.40395,15.499996 128.77259,16.280296 120.31724,17.829239 C111.92523,19.366579 103.63723,21.674147 95.665886,24.715017 C87.807064,27.712961 80.189381,31.449827 73.025829,35.858042 C65.99881,40.182241 59.34676,45.191857 53.277561,50.781361 C47.338933,56.250612 41.903382,62.327559 37.162314,68.862255 C32.519867,75.261019 28.498268,82.163364 25.255569,89.373177 C22.066711,96.463284 19.598915,103.92562 17.957661,111.52462 C16.331619,119.05317 15.5,126.79785 15.5,134.5 C15.5,141.74932 16.235626,149.0342 17.677654,156.13865 C19.131689,163.30227 21.317545,170.35241 24.151815,177.09026 C27.029182,183.93057 30.601252,190.51439 34.741632,196.67282 C38.962857,202.95149 43.812288,208.84582 49.134733,214.22282 C54.568073,219.71183 60.540512,224.70993 66.881186,229.11972 C73.344131,233.61456 80.241964,237.53569 87.39772,240.8172 C94.663673,244.14926 102.25174,246.84832 109.98421,248.87452 C117.79298,250.9207 125.80725,252.29631 133.85041,252.98271 C141.92081,253.67143 150.07919,253.67143 158.14959,252.98271 C166.19275,252.29631 174.20702,250.9207 182.01579,248.87452 C189.74826,246.84832 197.33632,244.14926 204.60228,240.8172 C211.75804,237.53569 218.65587,233.61456 225.11882,229.11972 C231.45949,224.70993 237.43193,219.71183 242.86526,214.22282 C248.18771,208.84582 253.03714,202.95149 257.25836,196.67282 C261.39874,190.51439 264.97081,183.93057 267.84819,177.09026 C270.68245,170.35241 272.86832,163.30227 274.32234,156.13865 C275.76437,149.0342 276.5,141.74932 276.5,134.5 C276.5,126.79785 275.66838,119.05317 274.04234,111.52462 C272.40109,103.92562 269.93329,96.463284 266.74443,89.373177 C263.50174,82.163364 259.48013,75.261019 254.83769,68.862255 C250.09662,62.327559 244.66107,56.250612 238.72244,50.781361 C232.65324,45.191857 226.00119,40.182241 218.97417,35.858042 C211.81062,31.449827 204.19293,27.712961 196.33411,24.715017 C188.36276,21.674147 180.07477,19.366579 171.68276,17.829239 C163.22741,16.280296 154.59605,15.499996 146,15.499996 M146,0.5 C226.35744,0.49999619 291.5,60.493835 291.5,134.5 C291.5,208.50616 226.35742,268.5 146,268.5 C65.642555,268.5 0.5,208.50616 0.5,134.5 C0.5,60.493835 65.64257,0.49999619 146,0.5";
    static readonly Geometry RingGeo = Geometry.Parse(PathData);

    public override void Render(DrawingContext ctx)
    {
        double w = Bounds.Width, h = Bounds.Height;
        if (w <= 0 || h <= 0) return;
        var full = new Rect(0, 0, w, h);
        if (BackgroundBrush != null) ctx.DrawEllipse(BackgroundBrush, null, new Point(w / 2, h / 2), w / 2, h / 2);
        double ang = Value * 0.01 * 360.0;
        if (IndicatorBrush != null && ang > 0)
        {
            Geometry g;
            if (ang >= 360) g = new EllipseGeometry(full);
            else
            {
                var sg = new StreamGeometry();
                using (var c = sg.Open())
                {
                    double rx = w / 2, ry = h / 2, a = ang * Math.PI / 180;
                    c.BeginFigure(new Point(rx, ry), true);
                    c.LineTo(new Point(rx, 0));
                    c.ArcTo(new Point(rx + rx * Math.Sin(a), ry - ry * Math.Cos(a)), new Size(rx, ry), 0, ang > 180, SweepDirection.Clockwise);
                    c.EndFigure(true);
                }
                g = sg;
            }
            using (ctx.PushGeometryClip(g)) ctx.DrawRectangle(IndicatorBrush, null, full);
        }
        if (ProgressBorderBrush is ISolidColorBrush sb && sb.Color.A > 0)
        {
            var m = Matrix.CreateTranslation(-0.5, -0.5) * Matrix.CreateScale(w / 291.0, h / 268.0);
            using (ctx.PushTransform(Matrix.CreateTranslation(1.41, 1.41)))
            using (ctx.PushTransform(m)) ctx.DrawGeometry(new SolidColorBrush(Colors.Black, 0.6), null, RingGeo);
            using (ctx.PushTransform(m)) ctx.DrawGeometry(ProgressBorderBrush, null, RingGeo);
        }
    }
}

/// <summary>Replacement for the WPF Slider (0..24, snap to integer) that was templated into a rotating knob.</summary>
public abstract class KnobBase : Border
{
    public static readonly StyledProperty<double> ValueProperty = AvaloniaProperty.Register<KnobBase, double>(nameof(Value), 0.0, coerce: (o, v) =>
    {
        var k = (KnobBase)o;
        v = Math.Max(k.Minimum, Math.Min(k.Maximum, v));
        return Math.Round(v, MidpointRounding.AwayFromZero);
    });
    public double Minimum { get; set; } = 0;
    public double Maximum { get; set; } = 24;
    public int Set { get; set; }
    public double Value { get => GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public Canvas Inner { get; } = new Canvas { Width = 300, Height = 300, Margin = new Thickness(5) };
    public Canvas ThumbCanvas { get; } = new Canvas { Width = 300, Height = 300 };
    public Ellipse ThumbEllipse { get; }
    public TextBlock ThumbText { get; }
    readonly RotateTransform _rot = new RotateTransform();

    protected KnobBase(string letter)
    {
        Width = 310; Height = 310;
        Child = Inner;
        ThumbEllipse = new Ellipse { Fill = new SolidColorBrush(Color.Parse("#00FFDE")), Width = 40, Height = 40, Cursor = new Cursor(StandardCursorType.Hand), Tag = "thumb" };
        Canvas.SetLeft(ThumbEllipse, 133); Canvas.SetTop(ThumbEllipse, 15);
        ThumbText = new TextBlock { Text = letter, Foreground = Brushes.Black, FontSize = 24, Cursor = new Cursor(StandardCursorType.Hand), Tag = "thumb" };
        Canvas.SetLeft(ThumbText, 148); Canvas.SetTop(ThumbText, 20);
        ThumbCanvas.Children.Add(ThumbEllipse); ThumbCanvas.Children.Add(ThumbText);
        ThumbCanvas.RenderTransform = _rot;
        ThumbCanvas.RenderTransformOrigin = new RelativePoint(150, 150, RelativeUnit.Absolute);
        ThumbCanvas.IsHitTestVisible = true;
        UpdateThumb();
    }
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == ValueProperty) UpdateThumb();
    }
    void UpdateThumb()
    {
        double a = Value / (Maximum - Minimum) * 360.0;   // MyHelper.GetAngle
        if (a == 360.0) a = 359.999;
        _rot.Angle = a;
        ToolTip.SetTip(ThumbEllipse, ((int)Value).ToString());
    }
    /// <summary>Hit test used by the main window: is the pointer over this knob's thumb?</summary>
    public static bool IsThumb(object src) => src is Control c && Equals(c.Tag, "thumb");
}

public class KnobStart : KnobBase
{
    public ProgressCircle Circle { get; }
    public ProgressCircle Circle2 { get; }
    public KnobStart() : base("S")
    {
        Circle = new ProgressCircle { Width = 300, Height = 300, Value = 100, BackgroundBrush = Brushes.Transparent, IndicatorBrush = new SolidColorBrush(Color.Parse("#00FFDE")), ProgressBorderBrush = Brushes.Transparent };
        Circle2 = new ProgressCircle { Width = 300, Height = 300, Value = 100, BackgroundBrush = Brushes.Transparent, ProgressBorderBrush = new SolidColorBrush(Color.Parse("#1C1C1C")) };
        Inner.Children.Add(Circle); Inner.Children.Add(Circle2); Inner.Children.Add(ThumbCanvas);
    }
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (Circle2.IndicatorBrush == null)
            Circle2.IndicatorBrush = new ImageBrush(Img.Load(Set == 2 ? "Assets/circle_bg2.png" : "Assets/circle_bg.png")) { Stretch = Stretch.Fill };
    }
}

public class KnobEnd : KnobBase
{
    public KnobEnd() : base("E")
    {
        Inner.Children.Add(new Ellipse { Width = 300, Height = 300 });
        Inner.Children.Add(ThumbCanvas);
    }
}

public class ResizeGrip : Control
{
    public ResizeGrip() { Cursor = new Cursor(StandardCursorType.BottomRightCorner); }
    public override void Render(DrawingContext ctx)
    {
        var br = new SolidColorBrush(Color.Parse("#FFA0A0A0"));
        for (int r = 0; r < 3; r++)
            for (int c = 0; c <= r; c++)
                ctx.DrawRectangle(br, null, new Rect(Bounds.Width - 3 - (r - c) * 4, Bounds.Height - 3 - c * 4, 2, 2));
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed && TopLevel.GetTopLevel(this) is Window w)
            w.BeginResizeDrag(WindowEdge.SouthEast, e);
    }
}

/// <summary>Hand-drawn replacement for the WinForms Chart (two SplineArea series, dotted cyan grid, #1C1C1C background).</summary>
public class PowerChart : Control
{
    public double YMaximum { get; set; } = 100;
    public readonly List<double> AC = new();
    public readonly List<double?> DC = new();
    static readonly Color Back = Color.FromRgb(28, 28, 28);
    static readonly Color Grid = Color.FromArgb(240, 0, 255, 222);

    static double NiceStep(double range)
    {
        foreach (var s in new double[] { 1, 2, 5, 10, 20, 25, 50, 100, 200, 250, 500 })
            if (range / s <= 6.01) return s;
        return range / 5;
    }

    public override void Render(DrawingContext ctx)
    {
        double W = Bounds.Width, H = Bounds.Height;
        ctx.FillRectangle(new SolidColorBrush(Back), new Rect(0, 0, W, H));
        double scale = 1.0;
        try { var top = TopLevel.GetTopLevel(this); if (top != null) { var m = this.TransformToVisual(top); if (m.HasValue) scale = Math.Abs(m.Value.M11); } } catch { }
        if (scale <= 0.05) scale = 1.0;
        // WinForms host: text keeps its device size (8pt = 10.67px) while the host rectangle is scaled by the Viewbox.
        double fs = 10.667 / scale;
        var face = new Typeface(new FontFamily("Microsoft Sans Serif"));
        double ystep = NiceStep(YMaximum);
        string widest = ((int)YMaximum).ToString();
        var probe = new FormattedText(widest, System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, face, fs, Brushes.White);
        double left = 12 / scale + probe.Width + 6 / scale, right = W - 12 / scale;
        double ptop = 23, bottom = 353;   // aligned with the consumption arrow marker (Top = 485 - W*330/limit)
        var plot = new Rect(left, ptop, right - left, bottom - ptop);
        var pen = new Pen(new SolidColorBrush(Grid), 1, new DashStyle(new double[] { 1, 2 }, 0));
        for (double v = 0; v <= YMaximum + 1e-6; v += ystep)
        {
            double y = bottom - v / YMaximum * (bottom - ptop);
            ctx.DrawLine(pen, new Point(left, y), new Point(right, y));
            var ft = new FormattedText(((int)v).ToString(), System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, face, fs, Brushes.White);
            ctx.DrawText(ft, new Point(left - 4 / scale - ft.Width, y - ft.Height / 2));
        }
        const double xmax = 60;
        for (int v = 0; v <= xmax; v += 10)
        {
            double x = left + v / xmax * plot.Width;
            ctx.DrawLine(pen, new Point(x, ptop), new Point(x, bottom));
            var ft = new FormattedText(v.ToString(), System.Globalization.CultureInfo.InvariantCulture, FlowDirection.LeftToRight, face, fs, Brushes.White);
            ctx.DrawText(ft, new Point(x - ft.Width / 2, bottom + 4 / scale));
        }
        using (ctx.PushClip(plot))
        {
            DrawSeries(ctx, plot, AC.ConvertAll(a => (double?)a), Color.FromArgb(240, 0, 255, 222), xmax);
            DrawSeries(ctx, plot, DC, Color.FromArgb(240, 255, 180, 0), xmax);
        }
    }

    void DrawSeries(DrawingContext ctx, Rect plot, List<double?> vals, Color col, double xmax)
    {
        var pts = new List<Point>();
        for (int i = 0; i < vals.Count; i++)
        {
            if (vals[i] == null) continue;
            double v = Math.Max(0, Math.Min(YMaximum, vals[i].Value));
            pts.Add(new Point(plot.X + (i + 1) / xmax * plot.Width, plot.Bottom - v / YMaximum * plot.Height));
        }
        if (pts.Count < 2) return;
        var sg = new StreamGeometry();
        using (var c = sg.Open())
        {
            c.BeginFigure(new Point(pts[0].X, plot.Bottom), true);
            c.LineTo(pts[0]);
            for (int i = 0; i < pts.Count - 1; i++)
            {
                var p0 = pts[Math.Max(i - 1, 0)]; var p1 = pts[i]; var p2 = pts[i + 1]; var p3 = pts[Math.Min(i + 2, pts.Count - 1)];
                var c1 = new Point(p1.X + (p2.X - p0.X) / 6, p1.Y + (p2.Y - p0.Y) / 6);
                var c2 = new Point(p2.X - (p3.X - p1.X) / 6, p2.Y - (p3.Y - p1.Y) / 6);
                c.CubicBezierTo(c1, c2, p2);
            }
            c.LineTo(new Point(pts[^1].X, plot.Bottom));
            c.EndFigure(true);
        }
        ctx.DrawGeometry(new SolidColorBrush(col), null, sg);
    }
}

/// <summary>Windows-style MessageBox.Show(text) replacement. Modal via nested dispatcher frame (non-blocking in screenshot mode).</summary>
public static class MessageBox
{
    public static Window Owner;
    public static Window Last;
    public static void Show(string text)
    {
        var ok = new Button { Content = "OK", Width = 75, Height = 23, Margin = new Thickness(0, 0, 12, 0), HorizontalAlignment = HorizontalAlignment.Right, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Background = new SolidColorBrush(Color.Parse("#FFE1E1E1")), BorderBrush = new SolidColorBrush(Color.Parse("#FF0078D7")), BorderThickness = new Thickness(1), CornerRadius = new CornerRadius(0), Padding = new Thickness(0), FontSize = 12, Foreground = Brushes.Black };
        var dlg = new Window
        {
            Title = "", SizeToContent = SizeToContent.WidthAndHeight, MinWidth = 160, MaxWidth = 700, CanResize = false, ShowInTaskbar = false,
            WindowStartupLocation = WindowStartupLocation.CenterOwner, Background = Brushes.White, FontSize = 12,
            Content = new Border
            {
                BorderBrush = new SolidColorBrush(Color.Parse("#FF9A9A9A")), BorderThickness = new Thickness(1),
                Child = new StackPanel
                {
                    Children =
                    {
                        new Border { Background = Brushes.White, Padding = new Thickness(18, 24, 18, 22), MinHeight = 62, Child = new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, Foreground = Brushes.Black } },
                        new Border { Background = new SolidColorBrush(Color.Parse("#FFF0F0F0")), Height = 42, Child = ok },
                    }
                }
            }
        };
        ok.Click += (_, _) => dlg.Close();
        dlg.KeyDown += (_, e) => { if (e.Key == Key.Escape || e.Key == Key.Enter) dlg.Close(); };
        Last = dlg;
        if (CCenter.Platform.AppHost.IsScreenshot) { dlg.Show(Owner); return; }
        var frame = new Avalonia.Threading.DispatcherFrame();
        dlg.Closed += (_, _) => frame.Continue = false;
        if (Owner != null) dlg.ShowDialog(Owner); else dlg.Show();
        Avalonia.Threading.Dispatcher.UIThread.PushFrame(frame);
    }
}

public static class PowerChartExt
{
	/// <summary>Chart.DataBind(): copy the table columns into the chart series and redraw.</summary>
	public static void DataBind(this PowerChart c, PowerTable t)
	{
		c.AC.Clear(); c.DC.Clear();
		foreach (var r in t.Rows) { c.AC.Add(r.AC ?? 0); c.DC.Add(r.DC); }
		c.InvalidateVisual();
	}
}
