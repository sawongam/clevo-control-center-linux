using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.Layout;
using CCenter.Platform;
using System.Text;
using System.Threading;
using CC.other_Class;

namespace CC;

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
		if (SupportWhiteKB6Level())
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
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		UpdateStatus();
		SetBrightnessLevel();
	}

	public void SetBrightnessLevel()
	{
		byte b = (byte)Brightness;
		if (Status == 0)
		{
			b = 0;
		}
		InsydeAcpiProt.SetDCHU_Data(39, new byte[4] { b, 0, 0, 0 }, 4);
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
		WriteLog(_iStatus, Status, "UpdateStatus");
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
		StringBuilder stringBuilder = new StringBuilder("Keyboard_");
		stringBuilder.Append(attribute + "_" + after);
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		stringBuilder.Append("Set " + attribute + ": " + before + " -> " + after);
		ClevoLog.Write(stringBuilder.ToString());
		Thread.Sleep(10);
	}
}
