using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace FnKey;

// Resources.ResourceManager.GetObject(name) replacement: images extracted from FnKey.Properties.Resources.resx
// (tools/extract_resx.py) live in Assets/Resources/<name>.png.
public static class Img
{
	static readonly Dictionary<string, Bitmap> Cache = new();

	public static Bitmap Load(string name)
	{
		lock (Cache)
		{
			if (Cache.TryGetValue(name, out var b)) return b;
			b = new Bitmap(AssetLoader.Open(new Uri("avares://FnKey/Assets/Resources/" + name + ".png")));
			Cache[name] = b;
			return b;
		}
	}
}
