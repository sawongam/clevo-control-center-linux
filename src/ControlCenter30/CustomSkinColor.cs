using System;
using System.IO;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace ControlCenter30;

internal class CustomSkinColor
{
	private static byte custom_Red;

	private static byte custom_Green;

	private static byte custom_Blue;

	public static void SetCustomSkinColor(byte r, byte g, byte b)
	{
		custom_Red = r;
		custom_Green = g;
		custom_Blue = b;
	}

	public static WriteableBitmap SetImageName(string image_name)
	{
		using Stream stream = AssetLoader.Open(new Uri("avares://ControlCenter30/" + Img.Norm("Image/" + image_name)));
		var src = WriteableBitmap.Decode(stream);
		RemoveBlue(src);
		return src;
	}

	// Replaces the RGB of every pixel with the custom skin colour, keeping alpha (BGRA byte order).
	private unsafe static void RemoveBlue(WriteableBitmap writeableBitmap)
	{
		using var fb = writeableBitmap.Lock();
		int num2 = fb.RowBytes * fb.Size.Height;
		byte* ptr = (byte*)(void*)fb.Address;
		for (int i = 0; i + 4 < num2; i += 4)
		{
			ptr[i] = custom_Blue;
			ptr[i + 1] = custom_Green;
			ptr[i + 2] = custom_Red;
		}
	}
}
