using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
namespace DesktopBridge_EnergySave;

/// <summary>new BitmapImage(new Uri("pack://application:,,,/Assets/x.png")) replacement (cached).</summary>
public static class Img
{
    static readonly Dictionary<string, Bitmap> Cache = new();
    public static Bitmap Load(string rel)
    {
        lock (Cache)
        {
            if (Cache.TryGetValue(rel, out var b)) return b;
            b = new Bitmap(AssetLoader.Open(new Uri("avares://EnergySave/" + rel.TrimStart('/'))));
            Cache[rel] = b;
            return b;
        }
    }
}
