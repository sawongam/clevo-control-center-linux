// SPDX-License-Identifier: GPL-2.0-only
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Headless;
using Avalonia.Input;
using System.Threading.Tasks;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace RippleCC.Platform;

/// <summary>
/// Common entry point for every ported app.
///   Normal run:            AppHost.Run&lt;App&gt;(args)
///   Screenshot (no display): app --screenshot out.png [--after-ms 800] [--page NAME]
/// In screenshot mode the app runs on Avalonia's headless platform with real Skia rendering,
/// the main window is shown, and a PNG of it is written. Use this to compare against the XAML.
/// </summary>
public static class AppHost
{
    public static bool IsScreenshot { get; private set; }
    /// <summary>Value of --page, for apps that want to open on a specific page/state when screenshotting.</summary>
    public static string PageArg { get; private set; }
    public static string[] Args { get; private set; } = Array.Empty<string>();

    static string Opt(string[] a, string name)
    {
        int i = Array.IndexOf(a, name);
        return i >= 0 && i + 1 < a.Length ? a[i + 1] : null;
    }

    public static int Run<TApp>(string[] args) where TApp : Application, new()
    {
        Args = args;
        PageArg = Opt(args, "--page");
        var shot = Opt(args, "--screenshot");
        if (shot == null)
            return AppBuilder.Configure<TApp>().UsePlatformDetect().With(Fonts()).LogToTrace()
                .StartWithClassicDesktopLifetime(args);

        IsScreenshot = true;
        Environment.SetEnvironmentVariable("RIPPLE_CC_MOCK", "1");
        int afterMs = int.TryParse(Opt(args, "--after-ms"), out var ms) ? ms : 800;
        int rc = 2;
        ClassicDesktopStyleApplicationLifetime life = null;
        var builder = AppBuilder.Configure<TApp>()
            .UseSkia().UseHeadless(new AvaloniaHeadlessPlatformOptions { UseHeadlessDrawing = false }).With(Fonts())
            .AfterSetup(_ => DispatcherTimer.RunOnce(async () =>
            {
                try
                {
                    life = (ClassicDesktopStyleApplicationLifetime)Application.Current.ApplicationLifetime;
                    Window Top() => life.Windows.LastOrDefault(x => x.IsVisible) ?? life.MainWindow;
                    // The app may have swapped windows (splash -> main); capture the newest visible one.
                    var w = Top();
                    if (w == null) { Console.Error.WriteLine("screenshot: app created no window"); rc = 2; }
                    else
                    {
                        if (!w.IsVisible) w.Show();
                        Pump();
                        rc = Save(Top(), shot) ? 0 : 3;
                        // Click script: --clicks "x,y[,wait_ms][;x,y...]" (window coords of the topmost window),
                        // --drag "x1,y1,x2,y2[,steps]" (left button), --rclicks same as --clicks but right button.
                        // Each step saves out-N.png next to the base screenshot (shot-1.png, shot-2.png ...).
                        var steps = new System.Collections.Generic.List<Func<Task>>();
                        var inv = System.Globalization.CultureInfo.InvariantCulture;
                        Func<Task> MakeDrag(double[] d)
                        {
                            int n = d.Length > 4 ? (int)d[4] : 8;
                            return async () =>
                            {
                                var t = Top(); var a = new Point(d[0], d[1]);
                                t.MouseMove(a); Pump(); t.MouseDown(a, MouseButton.Left); Pump(); t.MouseMove(a, RawInputModifiers.LeftMouseButton); Pump();
                                for (int i = 1; i <= n; i++)
                                {
                                    var q = new Point(d[0] + (d[2] - d[0]) * i / n, d[1] + (d[3] - d[1]) * i / n);
                                    t.MouseMove(q, RawInputModifiers.LeftMouseButton); Pump(); await Task.Delay(20);
                                }
                                t.MouseUp(new Point(d[2], d[3]), MouseButton.Left); Pump();
                                await Task.Delay(150); Pump();
                            };
                        }
                        // --clicks items (';' separated): "x,y[,wait_ms]" click | "r:x,y[,wait]" right click | "d:x1,y1,x2,y2[,steps]" drag |
                        // "w:ms" wait | "k:KeyName" key press (Avalonia Key enum) | "s:x,y,delta" wheel at point.
                        string clicks = Opt(args, "--clicks"), drag = Opt(args, "--drag");
                        if (drag != null) clicks = (clicks == null ? "" : clicks + ";") + "d:" + drag;
                        if (clicks != null)
                            foreach (var c0 in clicks.Split(';', StringSplitOptions.RemoveEmptyEntries))
                            {
                                var c = c0.Trim();
                                if (c.StartsWith("w:")) { int ww = int.Parse(c.Substring(2)); steps.Add(async () => { await Task.Delay(ww); Pump(); }); continue; }
                                if (c.StartsWith("d:")) { steps.Add(MakeDrag(c.Substring(2).Split(',').Select(v => double.Parse(v, inv)).ToArray())); continue; }
                                if (c.StartsWith("k:"))
                                {
                                    var key = Enum.Parse<Key>(c.Substring(2));
                                    steps.Add(async () => { var t = Top(); t.KeyPress(key, RawInputModifiers.None, PhysicalKey.None, null); t.KeyRelease(key, RawInputModifiers.None, PhysicalKey.None, null); await Task.Delay(150); Pump(); });
                                    continue;
                                }
                                if (c.StartsWith("s:"))
                                {
                                    var q = c.Substring(2).Split(',').Select(v => double.Parse(v, inv)).ToArray();
                                    steps.Add(async () => { var t = Top(); var pt = new Point(q[0], q[1]); t.MouseMove(pt); Pump(); t.MouseWheel(pt, new Vector(0, q[2])); await Task.Delay(150); Pump(); });
                                    continue;
                                }
                                var btn = MouseButton.Left;
                                if (c.StartsWith("r:")) { btn = MouseButton.Right; c = c.Substring(2); }
                                var p = c.Split(',');
                                double x = double.Parse(p[0], inv), y = double.Parse(p[1], inv);
                                int wait = p.Length > 2 ? int.Parse(p[2]) : 150;
                                steps.Add(async () =>
                                {
                                    var t = Top(); var pt = new Point(x, y);
                                    t.MouseMove(pt); Pump();
                                    t.MouseDown(pt, btn); Pump();
                                    t.MouseUp(pt, btn); Pump();
                                    await Task.Delay(wait); Pump();
                                });
                            }
                        string baseName = Path.Combine(Path.GetDirectoryName(Path.GetFullPath(shot)), Path.GetFileNameWithoutExtension(shot));
                        int k = 0;
                        foreach (var st in steps)
                        {
                            await st(); k++;
                            var t = Top();
                            Console.WriteLine($"step {k}: top window = {t?.GetType().Name} \"{t?.Title}\" {t?.Bounds.Width}x{t?.Bounds.Height}");
                            if (t != null) Save(t, $"{baseName}-{k}.png");
                        }
                    }
                }
                catch (Exception ex) { Console.Error.WriteLine("screenshot failed: " + ex); rc = 4; }
                Environment.Exit(rc);
            }, TimeSpan.FromMilliseconds(afterMs)));
        builder.StartWithClassicDesktopLifetime(args);
        return rc;
    }

    static void Pump() { for (int i = 0; i < 4; i++) { Dispatcher.UIThread.RunJobs(); AvaloniaHeadlessPlatform.ForceRenderTimerTick(); } }

    static bool Save(Window w, string path)
    {
        var frame = w.CaptureRenderedFrame();
        if (frame == null) { Console.Error.WriteLine("screenshot: nothing rendered"); return false; }
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)));
#pragma warning disable CS0618 // string overload is sufficient for PNG output
        frame.Save(path);
#pragma warning restore CS0618
        Console.WriteLine($"screenshot: {path} {frame.PixelSize.Width}x{frame.PixelSize.Height}");
        return true;
    }

    /// <summary>
    /// The Windows fonts the original XAML names do not exist on Linux. Map them to the closest
    /// metric-compatible free fonts so text lands where the original layout expects it.
    /// Keep the original FontFamily names in the XAML; never rename them there.
    /// </summary>
    static FontManagerOptions Fonts()
    {
        var sans = new FontFamily("Liberation Sans");
        return new FontManagerOptions
        {
            DefaultFamilyName = "Noto Sans",   // WPF default is Segoe UI
            FontFamilyMappings = new System.Collections.Generic.Dictionary<string, FontFamily>(StringComparer.OrdinalIgnoreCase)
            {
                ["Microsoft Sans Serif"] = sans,
                ["Arial"] = sans,
                ["Segoe UI"] = new FontFamily("Noto Sans"),
                ["Microsoft YaHei UI"] = new FontFamily("Noto Sans CJK SC, Noto Sans"),
                ["Microsoft JhengHei UI"] = new FontFamily("Noto Sans CJK TC, Noto Sans"),
                ["Yu Gothic UI"] = new FontFamily("Noto Sans CJK JP, Noto Sans"),
            },
        };
    }

    /// <summary>Start a sibling ported app (replaces UWP Launcher.LaunchUriAsync / Process.Start of the other CC apps).</summary>
    public static bool LaunchSibling(string appName, string arguments = "")
    {
        try
        {
            var baseDir = AppContext.BaseDirectory;
            string[] candidates =
            {
                Path.Combine(baseDir, appName),
                Path.Combine(baseDir, "..", appName, appName),
                // dev layout: ui/<App>/bin/<cfg>/net10.0/<App>
                Path.Combine(baseDir, "..", "..", "..", "..", appName, "bin", new DirectoryInfo(baseDir).Parent?.Name ?? "Debug", "net10.0", appName),
                Path.Combine("/opt/ripple-cc", appName, appName),
            };
            var exe = candidates.FirstOrDefault(File.Exists);
            if (IsScreenshot) { Console.WriteLine($"LaunchSibling: {appName} args=\"{arguments}\" -> {(exe == null ? "NOT FOUND" : Path.GetFullPath(exe))} (not started in screenshot mode)"); return exe != null; }
            if (exe == null) return false;
            Process.Start(new ProcessStartInfo(exe, arguments) { UseShellExecute = false });
            return true;
        }
        catch { return false; }
    }
}
