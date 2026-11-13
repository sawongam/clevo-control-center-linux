using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace CPUOC30.View;

/// <summary>new BitmapImage(new Uri("../Assets/x.png", Relative)) replacement (cached).</summary>
static class Img
{
    static readonly Dictionary<string, Bitmap> Cache = new();
    public static Bitmap Load(string path)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(path, out var b)) return b;
            try { b = new Bitmap(AssetLoader.Open(new Uri("avares://CPUOC/" + path))); }
            catch { b = null; }
            Cache[path] = b;
            return b;
        }
    }
}
