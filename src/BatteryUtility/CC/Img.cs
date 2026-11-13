using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
namespace CC;
public static class Img
{
    static readonly Dictionary<string, Bitmap> Cache = new();
    public static Bitmap Load(string rel)
    {
        lock (Cache)
        {
            if (!Cache.TryGetValue(rel, out var b))
                Cache[rel] = b = new Bitmap(AssetLoader.Open(new Uri("avares://BatteryUtility/" + rel.TrimStart('/'))));
            return b;
        }
    }
}
