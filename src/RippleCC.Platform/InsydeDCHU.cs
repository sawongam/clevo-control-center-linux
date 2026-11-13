// SPDX-License-Identifier: GPL-2.0-only
using System;
using System.Runtime.InteropServices;

namespace RippleCC.Platform;

/// <summary>
/// Drop-in replacement for the P/Invoke surface of InsydeDCHU.dll / InsydeDCHU_x86.dll.
/// Same names and signatures as the Windows DllImports, so ported code only has to delete
/// its [DllImport] declarations and call these instead.
/// Routed to the ripple-ccd daemon (unix socket), or to an in-process simulator when the
/// daemon is not running or RIPPLE_CC_MOCK=1.
/// Return values: the Windows DLL returns the firmware's integer result (original code does
/// `x = GetDCHU_Data_Integer(122, ref x)` and uses SetDCHU_Data's return as the answer of cmd 65),
/// so Get_Integer and SetDCHU_Data return that value; 0 on failure. Buffer/app-settings calls: 1/0.
/// </summary>
public static class InsydeDCHU
{
    public const int BufLen = 256;

    public static int GetDCHU_Data_Integer(int command, ref int data)
    {
        var r = Backend.Current.Call(command, 0, null);
        if (r == null) return 0;
        if (r.Buf != null && r.Buf.Length >= 4) data = BitConverter.ToInt32(r.Buf, 0);
        else data = unchecked((int)r.Int);
        return data;
    }

    public static int GetDCHU_Data_Buffer(int command, ref byte buffer)
    {
        var r = Backend.Current.Call(command, 0, null);
        if (r == null) return 0;
        var dst = MemoryMarshal.CreateSpan(ref buffer, BufLen);
        if (r.Buf != null) r.Buf.AsSpan(0, Math.Min(BufLen, r.Buf.Length)).CopyTo(dst);
        else BitConverter.GetBytes((uint)r.Int).CopyTo(dst);
        return 1;
    }

    /// <summary>length &lt;= 4: integer argument (little-endian). Otherwise a 256-byte package buffer.</summary>
    public static int SetDCHU_Data(int command, byte[] buffer, int length)
    {
        var r = SetInternal(command, buffer, length);
        if (r == null) return 0;
        return r.Buf != null && r.Buf.Length >= 4 ? BitConverter.ToInt32(r.Buf, 0) : unchecked((int)r.Int);
    }

    public static int SetDCHU_DataEx(int command, byte[] buffer, int length, ref byte OutputBuffer)
    {
        var r = SetInternal(command, buffer, length);
        if (r == null) return 0;
        var dst = MemoryMarshal.CreateSpan(ref OutputBuffer, BufLen);
        if (r.Buf != null) r.Buf.AsSpan(0, Math.Min(BufLen, r.Buf.Length)).CopyTo(dst);
        else BitConverter.GetBytes((uint)r.Int).CopyTo(dst);
        return 1;
    }

    static DchuResult SetInternal(int command, byte[] buffer, int length)
    {
        if (buffer == null) return null;
        if (length <= 4)
        {
            var b = new byte[4];
            Array.Copy(buffer, b, Math.Min(4, buffer.Length));
            return Backend.Current.Call(command, BitConverter.ToUInt32(b, 0), null);
        }
        var pkg = new byte[BufLen];
        Array.Copy(buffer, pkg, Math.Min(BufLen, buffer.Length));
        return Backend.Current.Call(command, 0, pkg);
    }

    public static int ReadAppSettings(int page, int offset, int length, ref byte buffer)
    {
        var data = Backend.Current.AppGet(page, offset, length);
        if (data == null) return 0;
        data.AsSpan(0, Math.Min(length, data.Length)).CopyTo(MemoryMarshal.CreateSpan(ref buffer, length));
        return 1;
    }

    public static int WriteAppSettings(int page, int offset, int length, ref byte buffer)
    {
        var data = MemoryMarshal.CreateSpan(ref buffer, length).ToArray();
        return Backend.Current.AppSet(page, offset, data) ? 1 : 0;
    }
}

public sealed class DchuResult
{
    public ulong Int;
    public byte[] Buf;
}
