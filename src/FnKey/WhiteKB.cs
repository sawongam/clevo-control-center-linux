using System;
using System.Text;
using FnKey.other_Class;

namespace FnKey;

internal class WhiteKB : BaseKB
{
	private readonly int DefaultBrightness = 5;

	private readonly int Default3LevelBrightness = 2;

	private int _brightness;

	private bool _initRead;

	private int _iStatus;

	public int Brightness
	{
		get
		{
			return _brightness;
		}
		set
		{
			if (!_brightness.Equals(value))
			{
				InsydeAcpiProt.WriteAcpiBrightness(value);
				WriteLog(_brightness, value, "Brightness");
				_brightness = value;
			}
		}
	}

	public int Status
	{
		get
		{
			return _iStatus;
		}
		set
		{
			if (_initRead)
			{
				_iStatus = value;
				return;
			}
			InsydeAcpiProt.WriteAcpiStatus(value);
			WriteLog(_iStatus, value, "Status");
			_iStatus = value;
		}
	}

	public void FirstRun()
	{
		if (Global.features.Support6WhKb)
		{
			InsydeAcpiProt.WriteAcpiBrightness(DefaultBrightness);
		}
		else
		{
			InsydeAcpiProt.WriteAcpiBrightness(Default3LevelBrightness);
		}
	}

	public void Init()
	{
		_initRead = true;
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		UpdateStatus();
		SetBrightnessLevel();
		_initRead = false;
	}

	public void SetBrightnessLevel()
	{
		byte value = (byte)Brightness;
		if (Status == 0)
		{
			value = 0;
		}
		byte[] bytes = BitConverter.GetBytes((short)value);
		InsydeAcpiProt.SetDCHU_Data(39, bytes, bytes.Length);
	}

	public void SetBrightnessLevel(int level)
	{
		Brightness = level;
		byte[] bytes = BitConverter.GetBytes((short)(byte)level);
		InsydeAcpiProt.SetDCHU_Data(39, bytes, bytes.Length);
		if (level == 0)
		{
			Status = 0;
		}
		else
		{
			Status = 1;
		}
	}

	public void UpdateBrightness()
	{
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
	}

	public void SetBrightnessDown()
	{
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		if (Brightness != 1)
		{
			Brightness--;
		}
		Status = 1;
		SetBrightnessLevel();
	}

	public void SetBrightnessUp()
	{
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		if (Brightness != 5)
		{
			Brightness++;
		}
		Status = 1;
		SetBrightnessLevel();
	}

	public int SetStatus()
	{
		if (Global.KB.Status == 1)
		{
			SetBrightnessLevel();
		}
		else
		{
			SetBrightnessOFF();
		}
		DebugLog.WriteDbgLog2Outlog("set Status=" + Status);
		return Status;
	}

	public void SetBrightnessOFF()
	{
		byte[] bytes = BitConverter.GetBytes((short)0);
		InsydeAcpiProt.SetDCHU_Data(39, bytes, bytes.Length);
	}

	public void UpdateStatus()
	{
		Status = InsydeAcpiProt.ReadAcpiStatus();
		DebugLog.WriteDbgLog2Outlog("Status=" + Status);
	}

	public void GetBrightnessNow()
	{
		int data = 90;
		InsydeAcpiProt.GetDCHU_Data_Integer(61, ref data);
		Brightness = data;
		if (Brightness == 0)
		{
			Status = 0;
		}
		else
		{
			Status = 1;
		}
	}

	public void SetOverrideBootEffect(int value)
	{
		byte[] bytes = BitConverter.GetBytes(503316480 + value);
		InsydeAcpiProt.SetDCHU_Data(121, bytes, 4);
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
