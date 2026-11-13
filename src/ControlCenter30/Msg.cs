using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;

namespace ControlCenter30;

/// <summary>System.Windows.MessageBox.Show(text) replacement (classic Windows look, single OK button).</summary>
internal static class Msg
{
	public static void Show(string text, string caption = "")
	{
		void Do()
		{
			var w = new Window
			{
				Title = caption, Width = 320, SizeToContent = SizeToContent.Height, CanResize = false,
				WindowStartupLocation = WindowStartupLocation.CenterScreen, Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)),
				FontSize = 12, FontFamily = new FontFamily("Segoe UI"),
			};
			var ok = new Button { Content = "OK", Width = 75, Height = 23, HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 12, 12), HorizontalAlignment = HorizontalAlignment.Right };
			ok.Click += (_, _) => w.Close();
			var sp = new StackPanel();
			sp.Children.Add(new Border { Background = Brushes.White, Padding = new Thickness(20, 24, 20, 24), Child = new TextBlock { Text = text, Foreground = Brushes.Black, TextWrapping = TextWrapping.Wrap } });
			sp.Children.Add(new Border { Height = 12 });
			sp.Children.Add(ok);
			w.Content = sp;
			if (AppHost_IsScreenshot) { w.Show(); return; }
			var owner = (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
			var tcs = new TaskCompletionSource();
			w.Closed += (_, _) => tcs.TrySetResult();
			if (owner != null && owner.IsVisible) w.ShowDialog(owner); else w.Show();
			var frame = new DispatcherFrame();
			tcs.Task.ContinueWith(_ => frame.Continue = false);
			Dispatcher.UIThread.PushFrame(frame);
		}
		if (Dispatcher.UIThread.CheckAccess()) Do(); else Dispatcher.UIThread.Invoke(Do);
	}

	static bool AppHost_IsScreenshot => RippleCC.Platform.AppHost.IsScreenshot;
}
