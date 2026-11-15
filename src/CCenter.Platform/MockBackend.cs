// SPDX-License-Identifier: GPL-2.0-only
using System;
using System.Collections.Generic;
using System.IO;

namespace CCenter.Platform;

/// <summary>
/// Simulated firmware, seeded with values captured from the real NH5x_NH7xHP (see RE_NOTES.md).
/// Used when the daemon is not running, so the UI can be developed and screenshotted without root.
/// Set commands are remembered so the UI reads back what it wrote.
/// </summary>
public sealed class MockBackend : IBackend
{
    public bool IsMock => true;
    readonly Dictionary<int, byte[]> _pages = new();
    readonly Dictionary<int, byte[]> _pkg = new();
    /// <summary>Every call the UI made: (func, intArg, buf). Tests can inspect this.</summary>
    public readonly List<(int func, uint arg, byte[] buf)> Log = new();
    int _fanModeEc = 0x02;
    readonly string _logPath = Environment.GetEnvironmentVariable("CCENTER_MOCK_LOG");

    public MockBackend()
    {
        var d = new byte[256];           // PK0D fan table
        d[12] = 2; d[14] = 0x02; d[15] = 0x06; d[43] = 0x01;
        Curve(d, 0x10, (40, 35), (60, 55), (80, 86), (100, 100));
        Curve(d, 0x18, (48, 35), (60, 51), (80, 86), (99, 100));
        _pkg[0x0D] = d;
    }

    static void Curve(byte[] b, int at, params (int t, int pct)[] p)
    {
        for (int i = 0; i < p.Length; i++) { b[at + 2 * i] = (byte)p[i].t; b[at + 2 * i + 1] = (byte)Math.Round(p[i].pct * 2.55); }
    }

    public DchuResult Call(int func, uint arg, byte[] buf)
    {
        lock (Log)
        {
            Log.Add((func, arg, buf));
            if (_logPath != null)
                try { File.AppendAllText(_logPath, $"func=0x{func:X2} arg=0x{arg:X8} buf={(buf == null ? "-" : Convert.ToHexString(buf, 0, 48))}\n"); } catch { }
            if (func == 0x79 && buf == null)
            {
                uint sub = arg >> 24, data = arg & 0xFFFFFF;
                if (sub == 0x01) _fanModeEc = data switch { 0 => 0x02, 1 => 0x10, 2 => 0x08, 5 => 0x01, 6 => 0x04, _ => _fanModeEc };
                return new DchuResult { Int = 0x79 };
            }
            if (func == 0x0C)
            {
                var b = new byte[256];
                bool max = _fanModeEc == 0x10;
                Put(b, 2, max ? 4980 : 2200); Put(b, 4, max ? 3150 : 950);
                b[16] = b[19] = (byte)(max ? 255 : 89);
                b[18] = 50; b[21] = 47;
                return new DchuResult { Buf = b };
            }
            if (func == 0x0D) { var b = (byte[])_pkg[0x0D].Clone(); b[14] = (byte)_fanModeEc; return new DchuResult { Buf = b }; }
            if (func == 0x0E && buf != null)
            {
                var t = _pkg[0x0D];
                for (int i = 0; i < 3; i++) Array.Copy(buf, 2 + 4 * i, t, 0x10 + 8 * i + 2, 4);
                return new DchuResult { Int = 0 };
            }
            if (buf != null) { _pkg[func] = buf; return new DchuResult { Int = 0 }; }
            if (_pkg.TryGetValue(func, out var stored)) return new DchuResult { Buf = (byte[])stored.Clone() };
            return new DchuResult { Int = 0, Buf = new byte[256] };
        }
    }

    static void Put(byte[] b, int at, int rpm) { int raw = 2156250 / rpm; b[at] = (byte)(raw >> 8); b[at + 1] = (byte)raw; }

    byte[] Page(int page)
    {
        if (!_pages.TryGetValue(page, out var p)) _pages[page] = p = new byte[256];
        return p;
    }

    public byte[] AppGet(int page, int offset, int length)
    {
        lock (Log)
        {
            var o = new byte[length];
            var p = Page(page);
            if (offset >= 0 && offset < p.Length) Array.Copy(p, offset, o, 0, Math.Min(length, p.Length - offset));
            return o;
        }
    }

    public bool AppSet(int page, int offset, byte[] data)
    {
        lock (Log)
        {
            var p = Page(page);
            if (offset < 0 || offset >= p.Length) return false;
            Array.Copy(data, 0, p, offset, Math.Min(data.Length, p.Length - offset));
            return true;
        }
    }
}
