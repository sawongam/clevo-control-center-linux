using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.VisualTree;

namespace DesktopBridge_EnergySave;

public static class MyHelper
{
	public static T FindParent<T>(Visual current) where T : Visual
	{
		while (current != null)
		{
			current = current.GetVisualParent();
			if (current is T result) return result;
		}
		return default;
	}

	public static double GetAngle(double value, double maximum, double minimum)
	{
		double num = value / (maximum - minimum) * 360.0;
		if (num == 360.0) num = 359.999;
		return num;
	}

	public static double GetAngleR(Point pos, double radius)
	{
		Point val = new Point(radius, radius);
		double num = val.X - pos.X;
		double num2 = val.Y - pos.Y;
		double num3 = Math.Sqrt(num * num + num2 * num2);
		double num4 = Math.Acos((val.Y - pos.Y) / num3);
		if (pos.X < radius) num4 = Math.PI * 2.0 - num4;
		if (double.IsNaN(num4)) return 0.0;
		return num4;
	}
}
