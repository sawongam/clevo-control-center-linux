using System;
using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ControlCenter30;

/// <summary>
/// WPF new BitmapImage(new Uri("Image/x.png", UriKind.Relative)) replacement.
/// WPF resource lookup is case-insensitive; Linux is not, so every resource lives under a lower-case
/// path (image/epson/pgup3.png) and this helper lower-cases the requested path.
/// </summary>
internal static class Img
{
	static readonly Dictionary<string, Bitmap> Cache = new();

	public static string Norm(string path)
	{
		path = path.Replace('\\', '/').TrimStart('/').ToLowerInvariant().Replace(' ', '-');
		return path;
	}

	public static Bitmap Load(string path)
	{
		var key = Norm(path);
		lock (Cache)
		{
			if (Cache.TryGetValue(key, out var b)) return b;
			try { b = new Bitmap(AssetLoader.Open(new Uri("avares://ControlCenter30/" + key))); }
			catch { b = null; }
			Cache[key] = b;
			return b;
		}
	}
}
