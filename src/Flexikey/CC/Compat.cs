using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using Avalonia.Threading;

namespace CC.Compat;

/// <summary>Image loader for embedded resources (all resource paths are lower-case).</summary>
public static class Img
{
	static readonly Dictionary<string, Bitmap> Cache = new();
	public static Bitmap Load(string path)
	{
		path = path.Replace('\\', '/').TrimStart('/');
		int i = path.IndexOf(";component/");
		if (i >= 0) path = path.Substring(i + 11);
		path = path.ToLowerInvariant();
		if (Cache.TryGetValue(path, out var b)) return b;
		b = new Bitmap(AssetLoader.Open(new Uri("avares://Flexikey/" + path)));
		Cache[path] = b;
		return b;
	}
}

public enum MessageBoxButton { OK = 0, OKCancel = 1, YesNoCancel = 3, YesNo = 4 }
public enum MessageBoxImage { None = 0, Error = 16, Question = 32, Warning = 48, Information = 64 }
public enum MessageBoxResult { None = 0, OK = 1, Cancel = 2, Yes = 6, No = 7 }

public static class Sync
{
	public static T Wait<T>(Task<T> t)
	{
		var frame = new DispatcherFrame();
		T res = default;
		t.ContinueWith(x => { if (!x.IsFaulted && !x.IsCanceled) res = x.Result; frame.Continue = false; }, TaskScheduler.FromCurrentSynchronizationContext());
		Dispatcher.UIThread.PushFrame(frame);
		return res;
	}
	public static Window MainWin => (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.Windows.LastOrDefault(w => w.IsVisible && !(w is MsgWindow)) ?? (Application.Current?.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow;
}

/// <summary>Windows-style message box.</summary>
public class MsgWindow : Window
{
	public MessageBoxResult Result = MessageBoxResult.None;
	public MsgWindow(string text, string caption, MessageBoxButton btn, MessageBoxImage img)
	{
		Title = caption; SizeToContent = SizeToContent.WidthAndHeight; CanResize = false; MinWidth = 160; MaxWidth = 520;
		WindowStartupLocation = WindowStartupLocation.CenterOwner; ShowInTaskbar = false; FontSize = 12; FontFamily = new FontFamily("Segoe UI");
		var icon = new TextBlock { Width = 32, Height = 32, FontSize = 22, FontWeight = FontWeight.Bold, TextAlignment = Avalonia.Media.TextAlignment.Center, VerticalAlignment = VerticalAlignment.Center, Foreground = Brushes.White };
		Control iconHost = null;
		if (img != MessageBoxImage.None)
		{
			string g = img == MessageBoxImage.Error ? "X" : img == MessageBoxImage.Warning ? "!" : img == MessageBoxImage.Question ? "?" : "i";
			var col = img == MessageBoxImage.Error ? "#C42B1C" : img == MessageBoxImage.Warning ? "#F7B500" : "#0078D7";
			icon.Text = g; icon.VerticalAlignment = VerticalAlignment.Center;
			iconHost = new Border { Width = 32, Height = 32, CornerRadius = new CornerRadius(16), Background = Brush.Parse(col), Margin = new Thickness(0, 0, 12, 0), Child = icon, VerticalAlignment = VerticalAlignment.Center };
		}
		var top = new StackPanel { Orientation = Orientation.Horizontal, Margin = new Thickness(14, 18, 14, 18) };
		if (iconHost != null) top.Children.Add(iconHost);
		top.Children.Add(new TextBlock { Text = text, TextWrapping = TextWrapping.Wrap, MaxWidth = 380, VerticalAlignment = VerticalAlignment.Center });
		var bar = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(10), Spacing = 8 };
		void Add(string t, MessageBoxResult r)
		{
			var b = new Button { Content = t, MinWidth = 75, Height = 23, Padding = new Thickness(8, 0), HorizontalContentAlignment = HorizontalAlignment.Center, VerticalContentAlignment = VerticalAlignment.Center };
			b.Click += (_, _) => { Result = r; Close(); };
			bar.Children.Add(b);
		}
		switch (btn)
		{
			case MessageBoxButton.OK: Add("OK", MessageBoxResult.OK); break;
			case MessageBoxButton.OKCancel: Add("OK", MessageBoxResult.OK); Add("Cancel", MessageBoxResult.Cancel); break;
			case MessageBoxButton.YesNo: Add("Yes", MessageBoxResult.Yes); Add("No", MessageBoxResult.No); break;
			case MessageBoxButton.YesNoCancel: Add("Yes", MessageBoxResult.Yes); Add("No", MessageBoxResult.No); Add("Cancel", MessageBoxResult.Cancel); break;
		}
		var root = new StackPanel();
		root.Children.Add(new Border { Background = Brushes.White, Child = top });
		root.Children.Add(new Border { Background = Brush.Parse("#F0F0F0"), Child = bar });
		Content = root;
	}
}

public static class MessageBox
{
	public static MessageBoxResult Show(string text) => Show(text, "", MessageBoxButton.OK, MessageBoxImage.None);
	public static MessageBoxResult Show(string text, string caption) => Show(text, caption, MessageBoxButton.OK, MessageBoxImage.None);
	public static MessageBoxResult Show(string text, string caption, MessageBoxButton b, MessageBoxImage i)
	{
		if (CCenter.Platform.AppHost.IsScreenshot) { Console.WriteLine($"MessageBox[{caption}]: {text}"); return b == MessageBoxButton.YesNo ? MessageBoxResult.Yes : MessageBoxResult.OK; }
		var w = new MsgWindow(text, caption, b, i);
		var owner = Sync.MainWin;
		var t = owner != null ? w.ShowDialog<object>(owner) : Task.FromResult<object>(null);
		if (owner != null) Sync.Wait(t); else w.Show();
		return w.Result;
	}
}

public abstract class FileDialogBase
{
	public string Title = "";
	public string Filter = "";
	public string FileName = "";
	protected List<FilePickerFileType> Types()
	{
		var l = new List<FilePickerFileType>();
		var p = Filter.Split('|');
		for (int i = 0; i + 1 < p.Length; i += 2)
			l.Add(new FilePickerFileType(p[i]) { Patterns = p[i + 1].Split(';').ToList() });
		return l;
	}
}
public class OpenFileDialog : FileDialogBase
{
	public bool? ShowDialog()
	{
		var w = Sync.MainWin; if (w == null) return false;
		var r = Sync.Wait(w.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions { Title = Title, FileTypeFilter = Types(), AllowMultiple = false }));
		if (r == null || r.Count == 0) return false;
		FileName = r[0].TryGetLocalPath() ?? ""; return true;
	}
}
public class SaveFileDialog : FileDialogBase
{
	public bool? ShowDialog()
	{
		var w = Sync.MainWin; if (w == null) return false;
		var r = Sync.Wait(w.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions { Title = Title, FileTypeChoices = Types(), DefaultExtension = "xml" }));
		if (r == null) return false;
		FileName = r.TryGetLocalPath() ?? ""; return true;
	}
}

public static class Clipboard
{
	public static void Clear() { try { Sync.MainWin?.Clipboard?.ClearAsync(); } catch { } }
}

/// <summary>Replacement for System.Diagnostics.EventLog (Windows event log used for service IPC).</summary>
public class EventLogStub
{
	public void WriteEntry(string message) { Console.WriteLine("EventLog: " + message); }
}
