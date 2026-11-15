using System;
using System.Linq;
using CCenter.Platform;

namespace FnKey;

static class Program
{
	// FnKey                      long-running hot-key agent (socket $XDG_RUNTIME_DIR/ccenter-fnkey.sock)
	// FnKey --event 0x59         send a firmware notify value to the running agent (else handle it one-shot)
	// FnKey --osd osd_caps_on    show an OSD directly (also: volume:40 brightness:70 kbwhite6:3 ...)
	// FnKey --sleep | --resume | --quit | --msg "clevocc30^101^2"
	// FnKey --no-init            agent without the start-up DCHU initialisation (OnFirstRun/OnStart)
	// FnKey --first-run          allow the destructive first-run init (zero AppSettings, fan defaults); also FNKEY_FIRST_RUN=1
	// FnKey --screenshot out.png --page <OsdImageName>
	[STAThread]
	public static int Main(string[] args)
	{
		if (AppHost.IsScreenshot || args.Contains("--screenshot")) return AppHost.Run<App>(args);
		var cmds = App.CommandsFromArgs(args);
		if (cmds.Count > 0 && Sock.Send(cmds.ToArray())) return 0;   // handed to the running instance
		return AppHost.Run<App>(args);
	}
}
