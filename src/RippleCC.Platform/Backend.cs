// SPDX-License-Identifier: GPL-2.0-only
using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;

namespace RippleCC.Platform;

public interface IBackend
{
    /// <summary>Evaluate DCHU command. buf == null → integer argument. Returns null on failure.</summary>
    DchuResult Call(int func, uint arg, byte[] buf);
    byte[] AppGet(int page, int offset, int length);
    bool AppSet(int page, int offset, byte[] data);
    bool IsMock { get; }
}

public static class Backend
{
    public const string SocketPath = "/run/ripple-cc/ccd.sock";
    static IBackend _cur;
    static readonly object Gate = new();

    public static IBackend Current
    {
        get
        {
            lock (Gate)
            {
                if (_cur != null) return _cur;
                bool mock = Environment.GetEnvironmentVariable("RIPPLE_CC_MOCK") == "1";
                string sock = Environment.GetEnvironmentVariable("RIPPLE_CC_SOCKET") ?? SocketPath;
                _cur = (!mock && File.Exists(sock)) ? new DaemonBackend(sock) : new MockBackend();
                return _cur;
            }
        }
        set { lock (Gate) _cur = value; }
    }

    public static bool IsMock => Current.IsMock;
}

/// <summary>JSON-lines client for ripple-ccd. One connection per call (simple, robust).</summary>
public sealed class DaemonBackend : IBackend
{
    readonly string _path;
    readonly object _lock = new();
    public DaemonBackend(string path) { _path = path; }
    public bool IsMock => false;

    JsonElement? Request(object req)
    {
        lock (_lock)
        {
            try
            {
                using var s = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                s.SendTimeout = s.ReceiveTimeout = 3000;
                s.Connect(new UnixDomainSocketEndPoint(_path));
                s.Send(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(req) + "\n"));
                var sb = new StringBuilder();
                var tmp = new byte[4096];
                while (true)
                {
                    int n = s.Receive(tmp);
                    if (n <= 0) break;
                    sb.Append(Encoding.UTF8.GetString(tmp, 0, n));
                    if (sb.Length > 0 && sb[sb.Length - 1] == '\n') break;
                }
                var doc = JsonDocument.Parse(sb.ToString());
                var root = doc.RootElement.Clone();
                if (!root.TryGetProperty("ok", out var ok) || !ok.GetBoolean()) return null;
                return root;
            }
            catch { return null; }
        }
    }

    public DchuResult Call(int func, uint arg, byte[] buf)
    {
        var r = buf == null
            ? Request(new { op = "call", func, @int = arg })
            : Request(new { op = "call", func, buf = Convert.ToHexString(buf) });
        if (r == null) return null;
        var res = new DchuResult();
        if (r.Value.TryGetProperty("int", out var i)) res.Int = i.GetUInt64();
        if (r.Value.TryGetProperty("buf", out var b) && b.ValueKind == JsonValueKind.String)
            res.Buf = Convert.FromHexString(b.GetString());
        return res;
    }

    public byte[] AppGet(int page, int offset, int length)
    {
        var r = Request(new { op = "appget", page, off = offset, len = length });
        return r == null ? null : Convert.FromHexString(r.Value.GetProperty("data").GetString());
    }

    public bool AppSet(int page, int offset, byte[] data)
        => Request(new { op = "appset", page, off = offset, data = Convert.ToHexString(data) }) != null;
}
