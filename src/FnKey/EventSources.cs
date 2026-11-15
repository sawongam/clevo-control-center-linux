using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace FnKey;

/// <summary>Where firmware/hot-key events come from. Windows: Arg001Event (ACPI WAIT_EVENT) -> "Arg001^value" log lines.</summary>
public interface IEventSource : IDisposable
{
	/// <summary>Raised on a worker thread with one command line (see PORT_NOTES.md "Command protocol").</summary>
	event Action<string> Line;

	void Start();
}

public static class Sock
{
	public static string Path
	{
		get
		{
			var d = Environment.GetEnvironmentVariable("XDG_RUNTIME_DIR");
			if (string.IsNullOrEmpty(d) || !Directory.Exists(d)) d = "/tmp";
			return System.IO.Path.Combine(d, "ccenter-fnkey.sock");
		}
	}

	/// <summary>Send lines to a running instance. Returns false when nobody listens.</summary>
	public static bool Send(params string[] lines)
	{
		try
		{
			using var s = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
			s.Connect(new UnixDomainSocketEndPoint(Path));
			s.Send(Encoding.UTF8.GetBytes(string.Join("\n", lines) + "\n"));
			s.Shutdown(SocketShutdown.Send);
			return true;
		}
		catch { return false; }
	}
}

/// <summary>Unix-socket source: one text command per line. A later phase feeds it from the kernel module / daemon.</summary>
public sealed class SocketEventSource : IEventSource
{
	public event Action<string> Line;
	Socket listener;
	volatile bool stop;

	public void Start()
	{
		string p = Sock.Path;
		try { if (File.Exists(p)) File.Delete(p); } catch { }
		listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
		listener.Bind(new UnixDomainSocketEndPoint(p));
		try { File.SetUnixFileMode(p, UnixFileMode.UserRead | UnixFileMode.UserWrite); } catch { }
		listener.Listen(8);
		new Thread(AcceptLoop) { IsBackground = true, Name = "fnkey-accept" }.Start();
	}

	void AcceptLoop()
	{
		while (!stop)
		{
			try
			{
				var c = listener.Accept();
				new Thread(() => Serve(c)) { IsBackground = true }.Start();
			}
			catch { if (stop) return; }
		}
	}

	void Serve(Socket c)
	{
		using (c)
		using (var r = new StreamReader(new NetworkStream(c), Encoding.UTF8))
		{
			string l;
			try { while ((l = r.ReadLine()) != null) { l = l.Trim(); if (l.Length > 0) Line?.Invoke(l); } } catch { }
		}
	}

	public void Dispose()
	{
		stop = true;
		try { listener?.Dispose(); File.Delete(Sock.Path); } catch { }
	}
}
