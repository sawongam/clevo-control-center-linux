using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;

namespace FnKey;

// Port of FnKey.OsdForm (WinForms). Geometry, timing and behaviour are copied 1:1 from OsdForm.cs:
//  - form 429x90 (re-sized to the image size on every show), BackColor Black, borderless, TopMost, shown without activation
//  - location (50,60) in screen pixels
//  - pic (0,0,img size) at the back, pic_bar (107,0,<=240 x imgH) above it, panel1 (352,29,67x33) in front holding
//    the label T_value (Arial 15.75pt, white, MiddleCenter)
//  - opacity 0.8 (ShowOsd) / 0.9 (bar OSDs); timer1 1500 ms, then timer2 every 50 ms: opacity -= 0.2 until <= 0.3 -> hidden
public class OsdForm
{
	public enum OsdImage
	{
		osd_3g_off, osd_3g_on, osd_airplane_off, osd_airplane_on, osd_backlight_high, osd_backlight_low, osd_backlight_off,
		osd_backlight_on, osd_bluetooth_off, osd_bluetooth_on, osd_camera_off, osd_camera_on, osd_caps_off, osd_caps_on,
		osd_docking_off, osd_docking_on, osd_gps_off, osd_gps_on, osd_num_off, osd_num_on, osd_scr_off, osd_scr_on,
		osd_silent_off, osd_silent_on, osd_touchpad_off, osd_touchpad_on, osd_wireless_off, osd_wireless_on,
		osd_fan_speed_auto, osd_fan_speed_max, osd_mute, osd_battery_off, osd_battery_on, osd_combo_on, osd_combo_off,
		osd_volume, osd_volume_mute, osd_volume_max, osd_volume_min, osd_brightness, osd_brightness_max, osd_brightness_min,
		osd_backlight_0, osd_backlight_1, osd_backlight_2, osd_backlight_3, osd_backlight_4, osd_backlight_5,
		osd_tp_led_off, osd_tp_led_on, osd_flexiaccess_on, osd_flexiaccess_off, osd_gc_on, osd_gc_off,
		osd_fan_speed_MaxQ, osd_fan_speed_Custom, osd_fan_speed_silent, osd_fan_speed_AntiDustRuning,
		osd_entetainment2, osd_peformance2, osd_quiet2, osd_powerSaving2, osd_Fn_lock_disable, osd_Fn_lock_enable,
		osd_fan_speed_Turbo, osd_balanced, osd_Gaming
	}

	private bool bShowOsd = true;

	private bool bSupportLockOsd = true;

	private bool bInit;

	private double opacity;          // Form.Opacity
	private bool visible;            // Form.Visible

	private readonly Timer timer1 = new Timer();   // 1500 ms
	private readonly Timer timer2 = new Timer();   // 50 ms

	// view
	private readonly Window win;
	private readonly Border root;
	private readonly Canvas canvas;
	private readonly Image pic;
	private readonly Image pic_bar;
	private readonly Border panel1;
	private readonly TextBlock T_value;
	private int formW = 429, formH = 90;
	private int picW, picH;
	private bool picVisible, barVisible, valueVisible, panelVisible;
	private int barWidth;

	public Window Window => win;

	public bool IsSupportOSD { set { bShowOsd = value; } }

	public bool IsSupportLockOsd { set { bSupportLockOsd = value; } }

	public OsdForm()
	{
		bInit = true;
		pic = new Image { Stretch = Stretch.None, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
		pic_bar = new Image { Stretch = Stretch.None, HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top };
		T_value = new TextBlock
		{
			FontFamily = new FontFamily("Arial"),
			FontSize = 21,                       // 15.75 pt at 96 dpi
			Foreground = Brushes.White,
			Text = "100",
			TextAlignment = TextAlignment.Center,
			VerticalAlignment = VerticalAlignment.Center,
			HorizontalAlignment = HorizontalAlignment.Stretch,
			TextTrimming = TextTrimming.CharacterEllipsis,
		};
		panel1 = new Border { Background = Brushes.Transparent, Width = 67, Height = 33, Child = T_value, ClipToBounds = true };
		canvas = new Canvas { Width = 429, Height = 90, Background = Brushes.Black, ClipToBounds = true, RenderTransformOrigin = RelativePoint.TopLeft };
		var barHost = new Border { ClipToBounds = true, Child = pic_bar, Width = 240, Height = 90 };
		pic_bar.Tag = barHost;
		canvas.Children.Add(pic);                // z-order: pic at the back,
		canvas.Children.Add(barHost);            // pic_bar above it,
		canvas.Children.Add(panel1);             // panel1 in front (Controls.Add order panel1,pic_bar,pic = front..back)
		root = new Border { Child = canvas, Opacity = 0 };
		win = new Window
		{
			WindowDecorations = WindowDecorations.None,
			Topmost = true,
			ShowActivated = false,
			ShowInTaskbar = false,
			CanResize = false,
			SizeToContent = SizeToContent.Manual,
			TransparencyLevelHint = new[] { WindowTransparencyLevel.Transparent },
			Background = AppHostIsShot() ? new SolidColorBrush(Color.FromRgb(0x80, 0x80, 0x80)) : Brushes.Transparent,
			Title = "OsdForm",
			Content = root,
			Focusable = false,
			WindowStartupLocation = WindowStartupLocation.Manual,
		};
		canvas.IsHitTestVisible = false;
		timer1.Interval = 1500;
		timer1.Tick += timer1_Tick;
		timer2.Interval = 50;
		timer2.Tick += timer2_Tick;
		opacity = 0.0;
		InitOsd();
	}

	static bool AppHostIsShot() => CCenter.Platform.AppHost.IsScreenshot;

	private double Scale => win.Screens?.Primary?.Scaling ?? 1.0;

	private void ApplyLayout()
	{
		double s = Scale;
		canvas.Width = formW; canvas.Height = formH;
		canvas.RenderTransform = new ScaleTransform(1 / s, 1 / s);
		win.Width = formW / s; win.Height = formH / s;
		pic.Width = picW; pic.Height = picH;
		pic.IsVisible = picVisible;
		var barHost = (Border)pic_bar.Tag;
		barHost.IsVisible = barVisible;
		barHost.Width = barWidth; barHost.Height = picH;
		pic_bar.Width = 240; pic_bar.Height = picH;
		Canvas.SetLeft(barHost, 107); Canvas.SetTop(barHost, 0);
		// panel1: Anchor Top|Bottom|Right relative to the original 429x90 design size
		panel1.IsVisible = panelVisible;
		panel1.Width = 67; panel1.Height = 33 + (formH - 90);
		Canvas.SetLeft(panel1, 352 + (formW - 429)); Canvas.SetTop(panel1, 29);
		T_value.IsVisible = valueVisible;
		root.Opacity = opacity;
	}

	private void SetOpacity(double v) { opacity = v; root.Opacity = v; }

	private void ShowWindow(int cmd)
	{
		// CallingVariations.ShowWindow: 4 = SW_SHOWNOACTIVATE, 0 = SW_HIDE
		if (cmd == 4)
		{
			if (!win.IsVisible) { win.Position = new PixelPoint(50, 60); win.Show(); }
		}
		else if (win.IsVisible) win.Hide();
	}

	private void InitTimer_Tick() { InitOsd(); }

	private void timer1_Tick(object sender, EventArgs e)
	{
		if (bInit)
		{
			win.Topmost = true;
		}
		timer2.Start();
	}

	private void timer2_Tick(object sender, EventArgs e)
	{
		SetOpacity(Math.Max(0.0, opacity - 0.2));
		if (opacity <= 0.3 && opacity != 0.0)
		{
			picVisible = false; barVisible = false; valueVisible = false; panelVisible = false;
			SetOpacity(0.0);
			ApplyLayout();
			ShowWindow(4);
		}
		else if (opacity == 0.0)
		{
			timer1.Stop();
			timer2.Stop();
			bInit = false;
			visible = false;
			ShowWindow(0);
		}
	}

	private void InitOsd()
	{
		picVisible = false; valueVisible = false; panelVisible = false; barVisible = false;
		timer1.Stop();
		timer2.Stop();
		SetOpacity(0.0);
		win.Position = new PixelPoint(50, 60);
		win.Topmost = true;
		formW = 429; formH = 90;
		ApplyLayout();
		timer2.Start();
	}

	public void ShowOsd(OsdImage osdimage)
	{
		if (bShowOsd && Global.fnKeyStatus == Global.FnKeyStatus.Ready && (bSupportLockOsd || (osdimage != OsdImage.osd_caps_off && osdimage != OsdImage.osd_caps_on && osdimage != OsdImage.osd_scr_off && osdimage != OsdImage.osd_scr_on && osdimage != OsdImage.osd_num_off && osdimage != OsdImage.osd_num_on && osdimage != OsdImage.osd_Fn_lock_disable && osdimage != OsdImage.osd_Fn_lock_enable)))
		{
			picVisible = true;
			valueVisible = false; panelVisible = false; barVisible = false;
			timer1.Stop();
			timer2.Stop();
			Bitmap bmp = Img.Load(osdimage.ToString());
			pic.Source = bmp;
			picW = bmp.PixelSize.Width; picH = bmp.PixelSize.Height;
			formW = picW; formH = picH;
			SetOpacity(1.0);
			SetOpacity(0.8);
			visible = true;
			ApplyLayout();
			ShowWindow(4);
			timer1.Enabled = true;
		}
	}

	public void ShowVolume(int value)
	{
		switch (value)
		{
		case 100:
			ShowBarOSD(OsdImage.osd_volume_max, 100, value, showBar: false);
			break;
		case 0:
			ShowBarOSD(OsdImage.osd_volume_min, 100, value, showBar: false);
			break;
		default:
			ShowBarOSD(OsdImage.osd_volume, 100, value, showBar: true);
			break;
		}
	}

	public void ShowVolumeMute()
	{
		ShowBarOSD(OsdImage.osd_volume_mute, 100, 0, showBar: false);
	}

	private void ShowBarOSD(OsdImage osdimage, int value_max, int value, bool showBar)
	{
		if (!bShowOsd)
		{
			return;
		}
		timer1.Stop();
		timer2.Stop();
		Bitmap bmp = Img.Load(osdimage.ToString());
		pic.Source = bmp;
		picW = bmp.PixelSize.Width; picH = bmp.PixelSize.Height;
		formW = picW; formH = picH;
		picVisible = true;
		if (showBar)
		{
			double num = 240.0 / (double)value_max * (double)value;
			if (num < 10.0)
			{
				num = 8.0;
			}
			else if (num > 230.0)
			{
				num = 240.0;
			}
			barWidth = (int)Math.Round(num, 0);
			if (pic_bar.Source == null) pic_bar.Source = Img.Load("osd_bar_full");
			barVisible = true;
			T_value.Text = value.ToString();
			valueVisible = true;
			panelVisible = true;
		}
		else
		{
			valueVisible = false; panelVisible = false; barVisible = false;
		}
		SetOpacity(0.9);
		visible = true;
		ApplyLayout();
		ShowWindow(4);
		timer1.Enabled = true;
	}

	public void ShowWhiteKbfor6(int value)
	{
		ShowBarOSD(value switch
		{
			0 => OsdImage.osd_backlight_0,
			1 => OsdImage.osd_backlight_1,
			2 => OsdImage.osd_backlight_2,
			3 => OsdImage.osd_backlight_3,
			4 => OsdImage.osd_backlight_4,
			_ => OsdImage.osd_backlight_5,
		}, 5, value, showBar: true);
	}

	public void ShowWhiteKbfor5(int value)
	{
		ShowBarOSD(value switch
		{
			0 => OsdImage.osd_backlight_0,
			1 => OsdImage.osd_backlight_1,
			2 => OsdImage.osd_backlight_2,
			3 => OsdImage.osd_backlight_3,
			_ => OsdImage.osd_backlight_4,
		}, 4, value, showBar: true);
	}

	public void ShowBrightness(int value)
	{
		switch (value)
		{
		case 100:
			ShowBarOSD(OsdImage.osd_brightness_max, 100, value, showBar: false);
			break;
		case 0:
			ShowBarOSD(OsdImage.osd_brightness_min, 100, value, showBar: false);
			break;
		default:
			ShowBarOSD(OsdImage.osd_brightness, 100, value, showBar: true);
			break;
		}
	}

	/// <summary>Parses "osd_caps_on", "caps_on", "volume:37", "brightness:50", "kbwhite6:3", "kbwhite5:2", "volume_mute".</summary>
	public bool ShowByName(string spec)
	{
		string name = spec, arg = null;
		int i = spec.IndexOf(':');
		if (i >= 0) { name = spec.Substring(0, i); arg = spec.Substring(i + 1); }
		int.TryParse(arg, out var v);
		switch (name.ToLowerInvariant())
		{
		case "volume": ShowVolume(v); return true;
		case "volumemute": ShowVolumeMute(); return true;
		case "brightness": ShowBrightness(v); return true;
		case "kbwhite6": ShowWhiteKbfor6(v); return true;
		case "kbwhite5": ShowWhiteKbfor5(v); return true;
		}
		if (Enum.TryParse<OsdImage>(name, true, out var img) || Enum.TryParse<OsdImage>("osd_" + name, true, out img))
		{
			// osd_volume* / osd_brightness* / osd_backlight_N via the plain ShowOsd show the bare image like the original ShowOsd would
			ShowOsd(img);
			return true;
		}
		return false;
	}
}
