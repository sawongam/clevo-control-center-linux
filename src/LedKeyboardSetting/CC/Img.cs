using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
namespace CC;

/// <summary>WPF "Image/Page_LEDdevice/x.png" relative-URI loader. Resolves case-insensitively through the
/// original resource LogicalName map (names with spaces/parentheses were sanitised on disk).</summary>
public static class Img
{
    static readonly Dictionary<string, Bitmap> Cache = new();
    static Dictionary<string, string> _map;
    static string Resolve(string path)
    {
        if (_map == null)
        {
            using var s = AssetLoader.Open(new Uri("avares://LedKeyboardSetting/tools/imagemap.json"));
            _map = JsonSerializer.Deserialize<Dictionary<string, string>>(s);
        }
        var p = path.Replace('\\', '/').TrimStart('/');
        if (_map.TryGetValue(p.ToLowerInvariant(), out var r)) return r;
        return p;
    }
    public static Bitmap Load(string path)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(path, out var b)) return b;
            var rel = Resolve(path);
            b = new Bitmap(AssetLoader.Open(new Uri("avares://LedKeyboardSetting/" + rel)));
            Cache[path] = b;
            return b;
        }
    }
}
