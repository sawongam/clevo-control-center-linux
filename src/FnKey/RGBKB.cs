using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using FnKey.other_Class;

namespace FnKey;

public class RGBKB : BaseKB
{
	private int _iStatus;

	private Color _all = Color.FromArgb(0, 0, 255);

	private Color _left = Color.FromArgb(255, 0, 0);

	private int _iLeft;

	private Color _mid = Color.FromArgb(0, 255, 0);

	private int _iMid;

	private Color _right = Color.FromArgb(0, 0, 255);

	private int _iRight;

	private Color _tp = Color.FromArgb(0, 0, 255);

	private int _itp;

	private Color _logo = Color.FromArgb(0, 255, 0);

	private int _ilogo;

	private Color _lightBar = Color.FromArgb(0, 0, 255);

	private int _ilightbar;

	private int _brightness;

	private int _iMode;

	private const int PAGE_SIZE = 256;

	private bool _initRead;

	private int _kbType = 2;

	private byte[] _wmi13;

	private List<Color> color_list;

	private byte ToggleRGBColor_index;

	private int color_type;

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

	public Color All
	{
		get
		{
			return _all;
		}
		set
		{
			if (!_all.Equals((object)value))
			{
				if (_initRead)
				{
					_all = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiAllColor(value);
				WriteLog(_all, value, "AllColor");
				_all = value;
			}
		}
	}

	public Color Left
	{
		get
		{
			return _left;
		}
		set
		{
			if (!_left.Equals((object)value))
			{
				if (_initRead)
				{
					_left = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiLeftColor(value);
				WriteLog(_left, value, "LeftColor");
				_left = value;
			}
		}
	}

	public int iLeft
	{
		get
		{
			return _iLeft;
		}
		set
		{
			if (!_iLeft.Equals(value))
			{
				if (_initRead)
				{
					_iLeft = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiLeftStatus(value);
				WriteLog(_iLeft, value, "LeftStatus");
				_iLeft = value;
			}
		}
	}

	public Color Mid
	{
		get
		{
			return _mid;
		}
		set
		{
			if (!_mid.Equals((object)value))
			{
				if (_initRead)
				{
					_mid = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiMidColor(value);
				WriteLog(_mid, value, "MidColor");
				_mid = value;
			}
		}
	}

	public int iMid
	{
		get
		{
			return _iMid;
		}
		set
		{
			if (!_iMid.Equals(value))
			{
				if (_initRead)
				{
					_iMid = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiMidStatus(value);
				WriteLog(_iMid, value, "MidStatus");
				_iMid = value;
			}
		}
	}

	public Color Right
	{
		get
		{
			return _right;
		}
		set
		{
			if (!_right.Equals((object)value))
			{
				if (_initRead)
				{
					_right = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiRightColor(value);
				WriteLog(_right, value, "RightColor");
				_right = value;
			}
		}
	}

	public int iRight
	{
		get
		{
			return _iRight;
		}
		set
		{
			if (!_iRight.Equals(value))
			{
				if (_initRead)
				{
					_iRight = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiRightStatus(value);
				WriteLog(_iRight, value, "RightStatus");
				_iRight = value;
			}
		}
	}

	public Color TP
	{
		get
		{
			return _tp;
		}
		set
		{
			if (!_tp.Equals((object)value))
			{
				if (_initRead)
				{
					_tp = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiTPColor(value);
				WriteLog(_tp, value, "TPColor");
				_tp = value;
			}
		}
	}

	public int iTp
	{
		get
		{
			return _itp;
		}
		set
		{
			if (!_itp.Equals(value))
			{
				if (_initRead)
				{
					_itp = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiTPStatus(value);
				WriteLog(_itp, value, "TPStatus");
				_itp = value;
			}
		}
	}

	public Color Logo
	{
		get
		{
			return _logo;
		}
		set
		{
			if (!_logo.Equals((object)value))
			{
				if (_initRead)
				{
					_logo = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiLogoColor(value);
				WriteLog(_logo, value, "LogoColor");
				_logo = value;
			}
		}
	}

	public int iLogo
	{
		get
		{
			return _ilogo;
		}
		set
		{
			if (!_ilogo.Equals(value))
			{
				if (_initRead)
				{
					_ilogo = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiLogoStatus(value);
				WriteLog(_ilogo, value, "LogoStatus");
				_ilogo = value;
			}
		}
	}

	public Color LightBar
	{
		get
		{
			return _lightBar;
		}
		set
		{
			if (!_lightBar.Equals((object)value))
			{
				if (_initRead)
				{
					_lightBar = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiLightBarColor(value);
				WriteLog(_lightBar, value, "LightBarColor");
				_logo = value;
			}
		}
	}

	public int iLightBar
	{
		get
		{
			return _ilightbar;
		}
		set
		{
			if (!_ilightbar.Equals(value))
			{
				if (_initRead)
				{
					_ilightbar = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiLightBarStatus(value);
				WriteLog(_ilightbar, value, "LightBarStatus");
				_ilightbar = value;
			}
		}
	}

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
				if (_initRead)
				{
					_brightness = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiBrightness(value);
				WriteLog(_brightness, value, "Brightness");
				_brightness = value;
			}
		}
	}

	public int Mode
	{
		get
		{
			return _iMode;
		}
		set
		{
			if (!_iMode.Equals(value))
			{
				if (_initRead)
				{
					_iMode = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiMode(value);
				WriteLog(_iMode, value, "Mode");
				_iMode = value;
			}
		}
	}

	public void FirstRun()
	{
		ResetWmi13ToDefault();
		Left = Color.FromArgb(255, 0, 0);
		Mid = Color.FromArgb(0, 255, 0);
		Right = Color.FromArgb(0, 0, 255);
		All = Color.FromArgb(0, 0, 255);
		TP = Color.FromArgb(0, 0, 255);
		Logo = Color.FromArgb(0, 255, 0);
		LightBar = Color.FromArgb(0, 0, 255);
		iLeft = 1;
		iMid = 1;
		iRight = 1;
		iTp = 1;
		iLogo = 1;
		iLightBar = 1;
		Status = 1;
		Brightness = 4;
		Mode = 1;
		Left = GetDefaultLeftColor();
		Mid = GetDefaultMidColor();
		Right = GetDefaultRightColor();
		if (Left.Equals((object)Mid) && Left.Equals((object)Right))
		{
			All = Left;
		}
		WriteAllReg();
	}

	private Color GetDefaultLeftColor()
	{
		if (_wmi13 == null)
		{
			_wmi13 = new byte[256];
			CopyWmi13(ref _wmi13);
		}
		return Color.FromArgb(_wmi13[2], _wmi13[3], _wmi13[4]);
	}

	private Color GetDefaultMidColor()
	{
		if (_wmi13 == null)
		{
			_wmi13 = new byte[256];
			CopyWmi13(ref _wmi13);
		}
		return Color.FromArgb(_wmi13[5], _wmi13[6], _wmi13[7]);
	}

	private Color GetDefaultRightColor()
	{
		if (_wmi13 == null)
		{
			_wmi13 = new byte[256];
			CopyWmi13(ref _wmi13);
		}
		return Color.FromArgb(_wmi13[8], _wmi13[9], _wmi13[10]);
	}

	public void Init()
	{
		_initRead = true;
		_kbType = GetKBType();
		ReadAllReg();
		_initRead = false;
		SetMode();
		SetLedOnOff();
	}

	public int SetStatus()
	{
		SetLedOnOff();
		if (Global.KB.Status == 1)
		{
			SetBrightnessLevel();
		}
		return Status;
	}

	public void UpdateStatus()
	{
		Status = InsydeAcpiProt.ReadAcpiStatus();
		iLeft = InsydeAcpiProt.ReadAcpiLeftStatus();
		iMid = InsydeAcpiProt.ReadAcpiMidStatus();
		iRight = InsydeAcpiProt.ReadAcpiRightStatus();
	}

	public void SetBrightnessLevel()
	{
		int num = 0;
		SetBrightnessValue(Brightness switch
		{
			1 => 47, 
			2 => 95, 
			3 => 143, 
			4 => 191, 
			_ => 0, 
		});
	}

	public void SetBrightnessLevel(int value)
	{
		if (value != 0)
		{
			Brightness = value;
		}
		int num = 0;
		SetBrightnessValue(value switch
		{
			1 => 47, 
			2 => 95, 
			3 => 143, 
			4 => 191, 
			_ => 47, 
		});
	}

	private void SetBrightnessValue(int value)
	{
		byte[] bytes = BitConverter.GetBytes(-201326592 + value);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
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
		else if (Global.KB.Status == 1)
		{
			return;
		}
		Status = 1;
		SetLedOnOff();
		SetBrightnessLevel();
	}

	public void SetBrightnessUp()
	{
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		if (Brightness != 4)
		{
			Brightness++;
		}
		else if (Global.KB.Status == 1)
		{
			return;
		}
		Status = 1;
		SetLedOnOff();
		SetBrightnessLevel();
	}

	public void SetOverrideBootEffect(int value)
	{
		byte[] bytes = BitConverter.GetBytes(503316480 + value);
		InsydeAcpiProt.SetDCHU_Data(121, bytes, bytes.Length);
	}

	public void SetMode()
	{
		if (Status.Equals(1))
		{
			SetBrightnessLevel(Brightness);
		}
		switch (Mode)
		{
		case 1:
			SetCustomColor();
			break;
		case 2:
			SetBreathMode();
			break;
		case 0:
			SetRandomMode();
			break;
		case 7:
			SetFlashMode();
			break;
		case 6:
			SetTempoMode();
			break;
		case 5:
			SetDanceMode();
			break;
		case 4:
			SetWaveMode();
			break;
		case 3:
			SetCycleMode();
			break;
		case 8:
			SetAllColor();
			break;
		}
	}

	public void SetCustomColor()
	{
		SetColor(1, Left);
		SetColor(2, Mid);
		SetColor(3, Right);
		if (Global.features.SupportLogoColor)
		{
			SetColor(7, Logo);
		}
		if (Global.features.SupportLightbar)
		{
			SetColor(8, LightBar);
		}
	}

	public void SetBreathMode()
	{
		SetColor(1, Left);
		SetColor(2, Mid);
		SetColor(3, Right);
		if (Global.features.SupportLogoColor)
		{
			SetColor(7, Logo);
		}
		if (Global.features.SupportLightbar)
		{
			SetColor(8, LightBar);
		}
		uint num = 2u;
		num <<= 16;
		num |= 0x10000000;
		byte[] bytes = BitConverter.GetBytes(0xA000 | num);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetRandomMode()
	{
		byte[] bytes = BitConverter.GetBytes(1879048192u);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetFlashMode()
	{
		byte[] bytes = BitConverter.GetBytes(2684354560u);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetTempoMode()
	{
		byte[] bytes = BitConverter.GetBytes(2415919104u);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetDanceMode()
	{
		byte[] bytes = BitConverter.GetBytes(2147483648u);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetWaveMode()
	{
		byte[] bytes = BitConverter.GetBytes(2952790016u);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetCycleMode()
	{
		byte[] bytes = BitConverter.GetBytes(855703552u);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetAllColor()
	{
		SetColor(1, All);
		SetColor(2, All);
		SetColor(3, All);
		if (Global.features.SupportLogoColor)
		{
			SetColor(7, Logo);
		}
		if (Global.features.SupportLightbar)
		{
			SetColor(8, LightBar);
		}
	}

	public void SetColor(int KbPart, Color _color)
	{
		uint num = 0u;
		int num2 = 0;
		switch (KbPart)
		{
		case 1:
			num2 = 240;
			break;
		case 2:
			num2 = 241;
			break;
		case 3:
			num2 = 242;
			break;
		case 8:
			num2 = 243;
			break;
		case 7:
			num2 = 246;
			break;
		}
		if (_kbType == 6 || _kbType == 22)
		{
			num = (uint)((_color.B << 16) | (_color.R << 8) | _color.G);
			if (_color.R == 0 && _color.G == byte.MaxValue && _color.B == 127)
			{
				num = (uint)(0x460000 | (_color.R << 8) | _color.G);
			}
		}
		else
		{
			num = (uint)(((byte)Math.Round(200.0 / 255.0 * (double)(int)_color.B, 0) << 16) | (_color.R << 8) | _color.G);
		}
		byte[] bytes = BitConverter.GetBytes((num2 << 24) + num);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	public void SetLedOnOff()
	{
		uint num = 0u;
		uint num2 = 7u;
		uint num3 = 0u;
		uint num4 = 0u;
		uint num5 = 0u;
		uint num6 = 0u;
		uint num7 = 0u;
		if (Status.Equals(1))
		{
			num6 = 1u;
			num2 = ((!(TP == Color.FromArgb(0, 0, 0)) && !iTp.Equals(0)) ? 1u : 7u);
			num = (uint)((!Global.features.SupportLightbar) ? ((int)((num | num4 | num5 | num2 | num3) + (num6 << 12)) + (iLogo << 13) + (iTp << 15) + (iLeft << 16) + (iMid << 17) + (iRight << 18)) : ((int)((num | num4 | num5 | num2 | num3) + (num6 << 12)) + (iLogo << 13) + (iLightBar << 15) + (iLeft << 16) + (iMid << 17) + (iRight << 18)));
		}
		else
		{
			num = (num4 | num5 | num2 | num3) + (num6 << 12) + (num7 << 13);
		}
		byte[] bytes = BitConverter.GetBytes((224 << 24) + num);
		InsydeAcpiProt.SetDCHU_Data(103, bytes, bytes.Length);
	}

	private Color StringToColor(string color)
	{
		string[] array = color.Split(new char[1] { '_' });
		return Color.FromArgb(255, Convert.ToByte(array[0]), Convert.ToByte(array[1]), Convert.ToByte(array[2]));
	}

	private string ColorToString(Color color)
	{
		return color.R + "_" + color.G + "_" + color.B;
	}

	public void WriteAllReg()
	{
		InsydeAcpiProt.WriteAcpiLeftColor(Left);
		InsydeAcpiProt.WriteAcpiMidColor(Mid);
		InsydeAcpiProt.WriteAcpiRightColor(Right);
		InsydeAcpiProt.WriteAcpiAllColor(All);
		InsydeAcpiProt.WriteAcpiTPColor(TP);
		InsydeAcpiProt.WriteAcpiLogoColor(Logo);
		InsydeAcpiProt.WriteAcpiLightBarColor(LightBar);
		InsydeAcpiProt.WriteAcpiLeftStatus(iLeft);
		InsydeAcpiProt.WriteAcpiMidStatus(iMid);
		InsydeAcpiProt.WriteAcpiRightStatus(iRight);
		InsydeAcpiProt.WriteAcpiTPStatus(iTp);
		InsydeAcpiProt.WriteAcpiLogoStatus(iLogo);
		InsydeAcpiProt.WriteAcpiLightBarStatus(iLightBar);
		InsydeAcpiProt.WriteAcpiStatus(Status);
		InsydeAcpiProt.WriteAcpiMode(Mode);
	}

	public void ReadAllReg()
	{
		Status = InsydeAcpiProt.ReadAcpiStatus();
		Mode = InsydeAcpiProt.ReadAcpiMode();
		iLeft = InsydeAcpiProt.ReadAcpiLeftStatus();
		iMid = InsydeAcpiProt.ReadAcpiMidStatus();
		iRight = InsydeAcpiProt.ReadAcpiRightStatus();
		iTp = InsydeAcpiProt.ReadAcpiTPStatus();
		iLogo = InsydeAcpiProt.ReadAcpiLogoStatus();
		iLightBar = InsydeAcpiProt.ReadAcpiLightBarStatus();
		Left = InsydeAcpiProt.ReadAcpiLeftColor();
		Mid = InsydeAcpiProt.ReadAcpiMidColor();
		Right = InsydeAcpiProt.ReadAcpiRightColor();
		All = InsydeAcpiProt.ReadAcpiAllColor();
		TP = InsydeAcpiProt.ReadAcpiTPColor();
		Logo = InsydeAcpiProt.ReadAcpiLogoColor();
		LightBar = InsydeAcpiProt.ReadAcpiLightBarColor();
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
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

	private void WriteLog(Color before, Color after, string attribute)
	{
		StringBuilder stringBuilder = new StringBuilder("Clevo^");
		stringBuilder.Append(attribute + "^" + ColorToString(after));
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		stringBuilder.Append("Set " + attribute + ": " + ColorToString(before) + " -> " + ColorToString(after));
		DebugLog.WriteDbgLog2Inlog(stringBuilder.ToString());
	}

	public void Init_RGB15Color_Table()
	{
		color_list = new List<Color>();
		color_list.Clear();
		color_list.Add(Color.FromArgb(0, 0, 255));
		color_list.Add(Color.FromArgb(0, 0, 200));
		color_list.Add(Color.FromArgb(0, 255, 0));
		color_list.Add(Color.FromArgb(255, 0, 0));
		color_list.Add(Color.FromArgb(0, 255, 200));
		color_list.Add(Color.FromArgb(0, 127, 200));
		color_list.Add(Color.FromArgb(0, 255, 127));
		color_list.Add(Color.FromArgb(255, 0, 200));
		color_list.Add(Color.FromArgb(127, 0, 200));
		color_list.Add(Color.FromArgb(255, 0, 127));
		color_list.Add(Color.FromArgb(255, 255, 0));
		color_list.Add(Color.FromArgb(127, 255, 200));
		color_list.Add(Color.FromArgb(255, 127, 0));
		color_list.Add(Color.FromArgb(63, 255, 0));
		color_list.Add(Color.FromArgb(255, 127, 63));
		color_list.Add(Color.FromArgb(63, 127, 127));
		color_list.Add(Color.FromArgb(223, 240, 124));
		int page = 2;
		int offset = 127;
		int length = 1;
		byte[] array = Global.dchu.ReadAppSettings(page, offset, length);
		ToggleRGBColor_index = array[0];
		Check_RGBColor_type();
	}

	public void Set_ToggleRGBColor()
	{
		if (color_type == 0)
		{
			if (ToggleRGBColor_index == 15)
			{
				ToggleRGBColor_index = 1;
			}
			else
			{
				ToggleRGBColor_index++;
			}
		}
		else if (ToggleRGBColor_index == 16)
		{
			ToggleRGBColor_index = 1;
		}
		else
		{
			ToggleRGBColor_index++;
		}
		All = color_list[ToggleRGBColor_index];
		DebugLog.WriteDbgLog2Outlog("RGBToggleIndex=" + ToggleRGBColor_index + " R=" + All.R + " G=" + All.G + " B=" + All.B);
		SetAllColor();
		int page = 2;
		int offset = 127;
		int length = 1;
		byte[] buffer = new byte[1] { ToggleRGBColor_index };
		Global.dchu.WriteAppSettings(page, offset, length, buffer);
	}

	private void Check_RGBColor_type()
	{
		color_type = 0;
		if (Global.features.CustomerID == 0 && Global.features.System_ManufacturerName.Contains("MONSTER"))
		{
			color_type = 16;
		}
	}
}
