// SPDX-License-Identifier: GPL-2.0-only
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;

namespace CCenter.Platform;

/// <summary>Where the port keeps per-user state (replaces registry / ProgramData / AppData).</summary>
public static class CcPaths
{
    public static string ConfigDir
    {
        get
        {
            var d = Environment.GetEnvironmentVariable("CCENTER_CONFIG")
                    ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".config", "ccenter");
            Directory.CreateDirectory(d);
            return d;
        }
    }

    /// <summary>
    /// Convert a Windows-style path used by the original code into a Linux path.
    /// Backslashes become slashes. Drive-rooted paths (C:\ProgramData\x\y.ini) land in ConfigDir/x/y.ini.
    /// Relative paths and paths under the app directory are left where they are.
    /// </summary>
    public static string Map(string winPath)
    {
        if (string.IsNullOrEmpty(winPath)) return winPath;
        var p = winPath.Replace('\\', '/');
        if (p.Length >= 2 && p[1] == ':')
        {
            p = p.Substring(2).TrimStart('/');
            p = Path.Combine(ConfigDir, p);
            Directory.CreateDirectory(Path.GetDirectoryName(p));
        }
        while (p.Contains("//")) p = p.Replace("//", "/");
        return p;
    }
}

/// <summary>
/// kernel32 Get/WritePrivateProfileString replacements (same signatures as the usual DllImports).
/// Handles UTF-16/UTF-8/ANSI files, case-insensitive sections and keys. File names go through CcPaths.Map.
/// </summary>
public static class Ini
{
    sealed class Doc { public DateTime Stamp; public Dictionary<string, Dictionary<string, string>> S; public Encoding Enc; }
    static readonly Dictionary<string, Doc> Cache = new();

    static Doc Load(string file)
    {
        file = ResolveCase(CcPaths.Map(file));
        lock (Cache)
        {
            var stamp = File.Exists(file) ? File.GetLastWriteTimeUtc(file) : DateTime.MinValue;
            if (Cache.TryGetValue(file, out var d) && d.Stamp == stamp) return d;
            d = new Doc { Stamp = stamp, S = new(StringComparer.OrdinalIgnoreCase), Enc = new UTF8Encoding(false) };
            if (File.Exists(file))
            {
                var raw = File.ReadAllBytes(file);
                if (raw.Length >= 2 && raw[0] == 0xFF && raw[1] == 0xFE) d.Enc = Encoding.Unicode;
                else if (raw.Length >= 3 && raw[0] == 0xEF && raw[1] == 0xBB) d.Enc = new UTF8Encoding(true);
                var text = d.Enc.GetString(raw).TrimStart('\uFEFF');
                Dictionary<string, string> cur = null;
                foreach (var l0 in text.Split('\n'))
                {
                    var l = l0.Trim('\r', ' ', '\t');
                    if (l.Length == 0 || l[0] == ';') continue;
                    if (l[0] == '[' && l.IndexOf(']') > 0)
                    {
                        var name = l.Substring(1, l.IndexOf(']') - 1);
                        if (!d.S.TryGetValue(name, out cur)) d.S[name] = cur = new(StringComparer.OrdinalIgnoreCase);
                        continue;
                    }
                    int eq = l.IndexOf('=');
                    if (eq <= 0 || cur == null) continue;
                    var k = l.Substring(0, eq).Trim();
                    if (!cur.ContainsKey(k)) cur[k] = l.Substring(eq + 1).Trim();
                }
            }
            Cache[file] = d;
            return d;
        }
    }

    /// <summary>Linux is case-sensitive, Windows is not: find the real file ignoring case.</summary>
    public static string ResolveCase(string path)
    {
        if (File.Exists(path) || Directory.Exists(path)) return path;
        var dir = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dir)) dir = ".";
        dir = Directory.Exists(dir) ? dir : ResolveCase(dir);
        if (!Directory.Exists(dir)) return path;
        var name = Path.GetFileName(path);
        foreach (var e in Directory.EnumerateFileSystemEntries(dir))
            if (string.Equals(Path.GetFileName(e), name, StringComparison.OrdinalIgnoreCase)) return e;
        return path;
    }

    public static string Read(string section, string key, string def, string file)
    {
        var d = Load(file);
        return d.S.TryGetValue(section, out var s) && s.TryGetValue(key, out var v) ? v : def;
    }

    public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath)
    {
        var v = Read(section, key, def ?? "", filePath) ?? "";
        if (v.Length >= size) v = v.Substring(0, Math.Max(0, size - 1));
        retVal.Clear(); retVal.Append(v);
        return v.Length;
    }

    public static int GetPrivateProfileInt(string section, string key, int def, string filePath)
        => int.TryParse(Read(section, key, null, filePath), out var v) ? v : def;

    public static bool WritePrivateProfileString(string section, string key, string val, string filePath)
    {
        try
        {
            var d = Load(filePath);
            var file = ResolveCase(CcPaths.Map(filePath));
            lock (Cache)
            {
                if (!d.S.TryGetValue(section, out var s)) d.S[section] = s = new(StringComparer.OrdinalIgnoreCase);
                if (val == null) s.Remove(key); else s[key] = val;
                var sb = new StringBuilder();
                foreach (var sec in d.S)
                {
                    sb.Append('[').Append(sec.Key).Append("]\r\n");
                    foreach (var kv in sec.Value) sb.Append(kv.Key).Append('=').Append(kv.Value).Append("\r\n");
                }
                Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(file)));
                var pre = d.Enc.GetPreamble();
                var body = d.Enc.GetBytes(sb.ToString());
                var all = new byte[pre.Length + body.Length];
                pre.CopyTo(all, 0); body.CopyTo(all, pre.Length);
                File.WriteAllBytes(file, all);
                d.Stamp = File.GetLastWriteTimeUtc(file);
            }
            return true;
        }
        catch { return false; }
    }
}

/// <summary>
/// Registry replacement: a JSON key/value file in ConfigDir. Use for everything the Windows code
/// kept under HKLM/HKCU. Key path = original registry path (any slashes), value name as on Windows.
/// </summary>
public static class Reg
{
    static Dictionary<string, Dictionary<string, JsonElement>> _d;
    static string FilePath => Path.Combine(CcPaths.ConfigDir, "registry.json");
    static string Norm(string key) => (key ?? "").Replace('/', '\\').Trim('\\').ToUpperInvariant();

    static void Load()
    {
        if (_d != null) return;
        try { _d = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, JsonElement>>>(File.ReadAllText(FilePath)); } catch { }
        _d ??= new();
    }

    public static object GetValue(string key, string name, object def = null)
    {
        lock (typeof(Reg))
        {
            Load();
            if (!_d.TryGetValue(Norm(key), out var k) || !k.TryGetValue(name ?? "", out var v)) return def;
            return v.ValueKind switch
            {
                JsonValueKind.Number => v.TryGetInt32(out var i) ? i : v.GetInt64(),
                JsonValueKind.String => v.GetString(),
                JsonValueKind.True => 1,
                JsonValueKind.False => 0,
                _ => def
            };
        }
    }

    public static int GetInt(string key, string name, int def = 0)
    {
        var v = GetValue(key, name);
        if (v == null) return def;
        try { return Convert.ToInt32(v); } catch { return def; }
    }

    public static string GetString(string key, string name, string def = "") => GetValue(key, name)?.ToString() ?? def;

    public static void SetValue(string key, string name, object value)
    {
        lock (typeof(Reg))
        {
            Load();
            if (!_d.TryGetValue(Norm(key), out var k)) _d[Norm(key)] = k = new();
            k[name ?? ""] = JsonSerializer.SerializeToElement(value is string or null ? value?.ToString() ?? "" : (object)Convert.ToInt64(value));
            try { File.WriteAllText(FilePath, JsonSerializer.Serialize(_d, new JsonSerializerOptions { WriteIndented = true })); } catch { }
        }
    }
}

/// <summary>Overclocking writes stay switched off until explicitly enabled by creating the unlock file (see UnlockFile).</summary>
public static class HardwareGate
{
    public const string UnlockFile = "/etc/ccenter/oc-unlocked";
    public static bool OcWritesEnabled => File.Exists(UnlockFile) || Backend.IsMock;
    /// <summary>Run a hardware write that changes voltages / ratios / power limits / GPU clocks only if unlocked.</summary>
    public static bool GuardOcWrite(Action write)
    {
        if (!OcWritesEnabled) return false;
        write();
        return true;
    }
}
