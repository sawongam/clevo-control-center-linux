using System;
using System.Text;
using FnKey.other_Class;

namespace FnKey;

public abstract class BaseKB
{
	private const int PAGE_SIZE = 256;

	private byte[] _wmi13;

	private int _overridebooteffect;

	private int _sleepTimer;

	private int _sleepTimerTirgger;

	public int OverrideBootEffect
	{
		get
		{
			return _overridebooteffect;
		}
		set
		{
			if (!_overridebooteffect.Equals(value))
			{
				InsydeAcpiProt.WriteAcpiOverrideBootEffect(value);
				WriteLog(_overridebooteffect, value, "OverrideBootEffect");
				_overridebooteffect = value;
			}
		}
	}

	public int SleepTimer
	{
		get
		{
			return _sleepTimer;
		}
		set
		{
			if (!_sleepTimer.Equals(value))
			{
				InsydeAcpiProt.WriteAcpiSleepTimer(value);
				WriteLog(_sleepTimer, value, "SleepTimer");
				_sleepTimer = value;
			}
		}
	}

	public int SleepTimerTrigger
	{
		get
		{
			return _sleepTimerTirgger;
		}
		set
		{
			if (!_sleepTimerTirgger.Equals(value))
			{
				InsydeAcpiProt.WriteAcpiSleepTimerTrigger(value);
				WriteLog(_sleepTimerTirgger, value, "SleepTimerTrigger");
				_sleepTimerTirgger = value;
			}
		}
	}

	private void GetWmi13()
	{
		_wmi13 = new byte[256];
		InsydeAcpiProt.GetDCHU_Data_Buffer(13, ref _wmi13[0]);
	}

	public void CopyWmi13(ref byte[] _copy)
	{
		if (_wmi13 == null)
		{
			GetWmi13();
		}
		Array.Copy(_wmi13, _copy, 256);
	}

	public int GetKBType()
	{
		if (_wmi13 == null)
		{
			GetWmi13();
		}
		if (_wmi13[15] == 3 || _wmi13[15] == 19 || _wmi13[15] == 35 || _wmi13[15] == 51 || _wmi13[15] == 243)
		{
			return 3;
		}
		return _wmi13[15];
	}

	public void WriteKBTypeToReg()
	{
		if (_wmi13 != null)
		{
			int offset = 4;
			int length = 1;
			InsydeAcpiProt.WriteAppSettings(2, offset, length, ref (new byte[1] { (byte)GetKBType() })[0]);
		}
	}

	public int ReadKBTypeFromReg()
	{
		int offset = 4;
		int num = 1;
		byte[] array = new byte[num];
		InsydeAcpiProt.ReadAppSettings(2, offset, num, ref array[0]);
		return array[0];
	}

	public bool SupportOverrideBootEffect()
	{
		if (_wmi13 == null)
		{
			GetWmi13();
		}
		return (_wmi13[43] & 1) == 1;
	}

	public void UpdateCurrentOverrideBootEffect()
	{
		OverrideBootEffect = InsydeAcpiProt.ReadAcpiOverrideBootEffect();
	}

	public void SetSleepTimerTrigger(int value)
	{
		byte[] bytes = BitConverter.GetBytes((402653184 + (value << 8)) | 0xFF);
		InsydeAcpiProt.SetDCHU_Data(121, bytes, bytes.Length);
	}

	public void SetSleepTimerTriggerOff()
	{
		byte[] bytes = BitConverter.GetBytes(402653184);
		InsydeAcpiProt.SetDCHU_Data(121, bytes, bytes.Length);
	}

	public void ResetWmi13ToDefault()
	{
		byte[] bytes = BitConverter.GetBytes(570425345);
		InsydeAcpiProt.SetDCHU_Data(121, bytes, bytes.Length);
		_wmi13 = null;
	}

	private void WriteLog(int before, int after, string attribute)
	{
		StringBuilder stringBuilder = new StringBuilder("Clevo^");
		stringBuilder.Append(attribute + "^" + after);
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		stringBuilder.Append("Set " + attribute + ": " + before + " -> " + after);
		DebugLog.WriteDbgLog2Inlog(stringBuilder.ToString());
	}
}
