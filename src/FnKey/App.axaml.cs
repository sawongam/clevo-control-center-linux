using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using RippleCC.Platform;

namespace FnKey;

public class App : Application
{
	OsdForm osd;
	Form1 ctl;
	SocketEventSource src;
	IClassicDesktopStyleApplicationLifetime life;

	public override void Initialize() => AvaloniaXamlLoader.Load(this);

	static string Opt(string[] a, string n) { int i = Array.IndexOf(a, n); return i >= 0 && i + 1 < a.Length ? a[i + 1] : null; }

	/// <summary>Command lines implied by the CLI flags (same protocol as the socket).</summary>
	public static List<string> CommandsFromArgs(string[] a)
	{
		var l = new List<string>();
		for (int i = 0; i < a.Length; i++)
		{
			if (i + 1 < a.Length && a[i] == "--event") l.Add("event " + a[i + 1]);
			else if (i + 1 < a.Length && a[i] == "--osd") l.Add("osd " + a[i + 1]);
			else if (i + 1 < a.Length && a[i] == "--msg") l.Add("msg " + a[i + 1]);
			else if (a[i] == "--sleep") l.Add("sleep");
			else if (a[i] == "--resume") l.Add("resume");
			else if (a[i] == "--quit") l.Add("quit");
		}
		return l;
	}

	public override void OnFrameworkInitializationCompleted()
	{
		life = ApplicationLifetime as IClassicDesktopStyleApplicationLifetime;
		if (life != null) life.ShutdownMode = ShutdownMode.OnExplicitShutdown;
		var args = AppHost.Args;
		osd = new OsdForm();
		ctl = new Form1(osd);
		var cmds = CommandsFromArgs(args);
		bool shot = AppHost.IsScreenshot;
		bool oneShot = cmds.Count > 0 && !shot;
		if (shot)
		{
			// screenshot mode: Ready state without hardware init, show the requested OSD
			ctl.Start(skipInit: true);
			string page = AppHost.PageArg ?? "osd_volume";
			Dispatcher.UIThread.Post(() => { try { osd.ShowByName(page); } catch (Exception ex) { Console.Error.WriteLine(ex); } });
		}
		else
		{
			ctl.Start(skipInit: oneShot || args.Contains("--no-init") || Environment.GetEnvironmentVariable("FNKEY_NO_INIT") == "1");
			if (!oneShot)
			{
				src = new SocketEventSource();
				src.Line += l => Dispatcher.UIThread.Post(() => Execute(l));
				try { src.Start(); } catch (Exception ex) { Console.Error.WriteLine("fnkey: socket: " + ex.Message); }
			}
			else
			{
				foreach (var c in cmds) Execute(c);
				// one-shot: exit after the OSD has faded (1500 ms + ~200 ms) with some slack
				DispatcherTimer.RunOnce(() => life?.Shutdown(), TimeSpan.FromMilliseconds(2200));
			}
		}
		base.OnFrameworkInitializationCompleted();
	}

	static int ParseCode(string s)
	{
		s = s.Trim();
		if (s.StartsWith("0x", StringComparison.OrdinalIgnoreCase)) return Convert.ToInt32(s.Substring(2), 16);
		return Convert.ToInt32(s);
	}

	/// <summary>One protocol line. See PORT_NOTES.md.</summary>
	void Execute(string line)
	{
		try
		{
			int sp = line.IndexOf(' ');
			string verb = sp < 0 ? line : line.Substring(0, sp);
			string rest = sp < 0 ? "" : line.Substring(sp + 1).Trim();
			switch (verb)
			{
			case "event": ctl.HandleNotify(ParseCode(rest)); break;   // value = ACPI notify value: low byte event, rest data
			case "msg": ctl.Handle(rest); break;                       // raw original log message e.g. "Arg001^89", "clevocc30^101^2"
			case "osd": if (!osd.ShowByName(rest)) Console.Error.WriteLine("fnkey: unknown OSD '" + rest + "'"); break;
			case "sleep": ctl.Sleep(); break;
			case "resume": ctl.Resume(); break;
			case "toggle-osd": ctl.ToggleOsdSetting(); break;
			case "quit": ctl.Close(); break;
			default: Console.Error.WriteLine("fnkey: unknown command '" + line + "'"); break;
			}
		}
		catch (Exception ex) { Console.Error.WriteLine("fnkey: " + line + ": " + ex.Message); }
	}
}
