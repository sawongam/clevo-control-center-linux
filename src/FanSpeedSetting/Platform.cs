using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using RippleCC.Platform;

namespace FanSpeedSetting;

/// <summary>Image loader: replaces new BitmapImage(new Uri("Image/x.png", UriKind.Relative)).</summary>
public static class Img
{
    static readonly Dictionary<string, Bitmap> Cache = new();
    public static Bitmap Load(string path)
    {
        path = path.Replace('\\', '/').TrimStart('/').ToLowerInvariant();
        lock (Cache)
        {
            if (Cache.TryGetValue(path, out var b)) return b;
            b = new Bitmap(AssetLoader.Open(new Uri("avares://FanSpeedSetting/" + path)));
            Cache[path] = b;
            return b;
        }
    }
}

public enum RegistryValueKind { String, DWord }

/// <summary>Microsoft.Win32.Registry replacement backed by RippleCC.Platform.Reg.</summary>
public static class Registry
{
    public static object GetValue(string key, string name, object def) => Reg.GetValue(key, name, def);
    public static void SetValue(string key, string name, object value) => Reg.SetValue(key, name, value);
    public static void SetValue(string key, string name, object value, RegistryValueKind kind) => Reg.SetValue(key, name, value);
}

/// <summary>WPF Thickness is mutable (margin.Left = x); Avalonia's is not. Ported code aliases Thickness to this.</summary>
public struct MutableThickness
{
    public double Left, Top, Right, Bottom;
    public MutableThickness(double l, double t, double r, double b) { Left = l; Top = t; Right = r; Bottom = b; }
    public static implicit operator Avalonia.Thickness(MutableThickness m) => new Avalonia.Thickness(m.Left, m.Top, m.Right, m.Bottom);
    public static implicit operator MutableThickness(Avalonia.Thickness t) => new MutableThickness(t.Left, t.Top, t.Right, t.Bottom);
}
