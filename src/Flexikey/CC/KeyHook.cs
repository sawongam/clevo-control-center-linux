using System;
using System.Collections.Generic;
using Avalonia.Input;

namespace Test1;

public class KeyboardEventArgs : EventArgs
{
	public int KeyValue { get; private set; }
	public int Status { get; private set; }
	public int DelayTime { get; private set; }
	public KeyboardEventArgs(int KeyValue, int Status, int DelayTime) { this.KeyValue = KeyValue; this.Status = Status; this.DelayTime = DelayTime; }
}

/// <summary>
/// Replacement for gamefeetlib.Test1.Class1 + Windowshook.KeyboardHook (WH_KEYBOARD_LL global hook).
/// The Linux port records only keys pressed inside the app window (fed by MainWindow tunnel key handlers).
/// Emits the same values: KeyValue = set-1 scan code (+0xE000 for extended keys), Status 0=make 1=break.
/// </summary>
public class Class1
{
	DateTime start; bool FirstKey = true; bool _Enable;
	readonly HashSet<int> saved = new();
	public static Class1 Active;
	public bool Enable
	{
		get => _Enable;
		set { _Enable = value; if (value) { FirstKey = true; Active = this; } else if (Active == this) Active = null; saved.Clear(); }
	}
	public bool Monopolize { get; set; }
	public event EventHandler KeyPress;

	/// <summary>Returns true when the event was consumed.</summary>
	public bool Feed(KeyEventArgs e, bool down)
	{
		if (!_Enable) return false;
		if (!ScanMap.TryGet(e.PhysicalKey, out int scan, out bool ext, out int vk)) return false;
		int keyValue;
		if (vk == 255 || ext) keyValue = (vk != 161 && vk != 144) ? 57344 + scan : scan;
		else keyValue = vk != 19 ? scan : (down ? 57413 + scan : 57629);
		if (down)
		{
			if (!saved.Add(keyValue)) return true;
			int delay;
			if (FirstKey) { FirstKey = false; start = DateTime.Now; delay = 0; }
			else { delay = (int)Math.Round((DateTime.Now - start).TotalMilliseconds); start = DateTime.Now; }
			KeyPress?.Invoke(this, new KeyboardEventArgs(keyValue, 0, delay));
		}
		else
		{
			saved.Remove(keyValue);
			int delay = (int)Math.Round((DateTime.Now - start).TotalMilliseconds);
			start = DateTime.Now;
			KeyPress?.Invoke(this, new KeyboardEventArgs(keyValue, 1, delay));
		}
		return true;
	}
}

/// <summary>Avalonia PhysicalKey -> (set-1 scan code, extended flag, Windows VK for the few special cases).</summary>
public static class ScanMap
{
	static readonly Dictionary<PhysicalKey, (int scan, bool ext, int vk)> M = Build();
	public static bool TryGet(PhysicalKey k, out int scan, out bool ext, out int vk)
	{
		if (M.TryGetValue(k, out var v)) { scan = v.scan; ext = v.ext; vk = v.vk; return true; }
		scan = 0; ext = false; vk = 0; return false;
	}
	static Dictionary<PhysicalKey, (int, bool, int)> Build()
	{
		var d = new Dictionary<PhysicalKey, (int, bool, int)>();
		void A(PhysicalKey k, int s, bool e = false, int vk = 0) => d[k] = (s, e, vk);
		A(PhysicalKey.Escape, 0x01);
		var dig = new[] { PhysicalKey.Digit1, PhysicalKey.Digit2, PhysicalKey.Digit3, PhysicalKey.Digit4, PhysicalKey.Digit5, PhysicalKey.Digit6, PhysicalKey.Digit7, PhysicalKey.Digit8, PhysicalKey.Digit9, PhysicalKey.Digit0 };
		for (int i = 0; i < 10; i++) A(dig[i], 0x02 + i);
		A(PhysicalKey.Minus, 0x0C); A(PhysicalKey.Equal, 0x0D); A(PhysicalKey.Backspace, 0x0E); A(PhysicalKey.Tab, 0x0F);
		var q = new[] { PhysicalKey.Q, PhysicalKey.W, PhysicalKey.E, PhysicalKey.R, PhysicalKey.T, PhysicalKey.Y, PhysicalKey.U, PhysicalKey.I, PhysicalKey.O, PhysicalKey.P };
		for (int i = 0; i < 10; i++) A(q[i], 0x10 + i);
		A(PhysicalKey.BracketLeft, 0x1A); A(PhysicalKey.BracketRight, 0x1B); A(PhysicalKey.Enter, 0x1C); A(PhysicalKey.ControlLeft, 0x1D);
		var a = new[] { PhysicalKey.A, PhysicalKey.S, PhysicalKey.D, PhysicalKey.F, PhysicalKey.G, PhysicalKey.H, PhysicalKey.J, PhysicalKey.K, PhysicalKey.L };
		for (int i = 0; i < 9; i++) A(a[i], 0x1E + i);
		A(PhysicalKey.Semicolon, 0x27); A(PhysicalKey.Quote, 0x28); A(PhysicalKey.Backquote, 0x29); A(PhysicalKey.ShiftLeft, 0x2A); A(PhysicalKey.Backslash, 0x2B);
		var z = new[] { PhysicalKey.Z, PhysicalKey.X, PhysicalKey.C, PhysicalKey.V, PhysicalKey.B, PhysicalKey.N, PhysicalKey.M };
		for (int i = 0; i < 7; i++) A(z[i], 0x2C + i);
		A(PhysicalKey.Comma, 0x33); A(PhysicalKey.Period, 0x34); A(PhysicalKey.Slash, 0x35); A(PhysicalKey.ShiftRight, 0x36, false, 161);
		A(PhysicalKey.NumPadMultiply, 0x37); A(PhysicalKey.AltLeft, 0x38); A(PhysicalKey.Space, 0x39); A(PhysicalKey.CapsLock, 0x3A);
		var f = new[] { PhysicalKey.F1, PhysicalKey.F2, PhysicalKey.F3, PhysicalKey.F4, PhysicalKey.F5, PhysicalKey.F6, PhysicalKey.F7, PhysicalKey.F8, PhysicalKey.F9, PhysicalKey.F10 };
		for (int i = 0; i < 10; i++) A(f[i], 0x3B + i);
		A(PhysicalKey.NumLock, 0x45, true, 144); A(PhysicalKey.ScrollLock, 0x46);
		A(PhysicalKey.NumPad7, 0x47); A(PhysicalKey.NumPad8, 0x48); A(PhysicalKey.NumPad9, 0x49); A(PhysicalKey.NumPadSubtract, 0x4A);
		A(PhysicalKey.NumPad4, 0x4B); A(PhysicalKey.NumPad5, 0x4C); A(PhysicalKey.NumPad6, 0x4D); A(PhysicalKey.NumPadAdd, 0x4E);
		A(PhysicalKey.NumPad1, 0x4F); A(PhysicalKey.NumPad2, 0x50); A(PhysicalKey.NumPad3, 0x51); A(PhysicalKey.NumPad0, 0x52); A(PhysicalKey.NumPadDecimal, 0x53);
		A(PhysicalKey.IntlBackslash, 0x56); A(PhysicalKey.F11, 0x57); A(PhysicalKey.F12, 0x58);
		A(PhysicalKey.IntlYen, 0x7D); A(PhysicalKey.IntlRo, 0x73); A(PhysicalKey.Convert, 0x79); A(PhysicalKey.NonConvert, 0x7B); A(PhysicalKey.KanaMode, 0x70);
		// extended (E0) keys
		A(PhysicalKey.NumPadEnter, 0x1C, true); A(PhysicalKey.ControlRight, 0x1D, true); A(PhysicalKey.NumPadDivide, 0x35, true);
		A(PhysicalKey.PrintScreen, 0x37, true); A(PhysicalKey.AltRight, 0x38, true); A(PhysicalKey.Home, 0x47, true); A(PhysicalKey.ArrowUp, 0x48, true);
		A(PhysicalKey.PageUp, 0x49, true); A(PhysicalKey.ArrowLeft, 0x4B, true); A(PhysicalKey.ArrowRight, 0x4D, true); A(PhysicalKey.End, 0x4F, true);
		A(PhysicalKey.ArrowDown, 0x50, true); A(PhysicalKey.PageDown, 0x51, true); A(PhysicalKey.Insert, 0x52, true); A(PhysicalKey.Delete, 0x53, true);
		A(PhysicalKey.MetaLeft, 0x5B, true); A(PhysicalKey.MetaRight, 0x5C, true); A(PhysicalKey.ContextMenu, 0x5D, true);
		A(PhysicalKey.Pause, 0x45, false, 19);
		return d;
	}
}
