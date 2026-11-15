using System;
using CC.Device;
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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CC.other_Class;

namespace CC;

internal class PerkeyKB : BaseKB
{
	private bool _initRead;

	private bool _initPerkeyIO;

	public bool SupportCustomStaticColor;

	public Color CustomDefaultStaticColor;

	public bool SupportMcuNewEffect;

	public string Minor;

	public string Revision;

	private int _iStatus;

	private int _brightness;

	private int _speed;

	private int _iMode;

	private Color _wave = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private int _iwave;

	private int _iwavefreeze;

	private Color _breath = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private int _ibreath;

	private Color _scan1 = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private Color _scan2 = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private int _iscan;

	private Color _blink = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private int _iblink;

	private Color _random = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private int _irandom;

	private Color _ripple = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private int _iripple;

	private Color _snake = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	private int _isnake;

	private Color _fn = Color.FromRgb((byte)0, (byte)0, byte.MaxValue);

	public int _istatic;

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

	public int Speed
	{
		get
		{
			return _speed;
		}
		set
		{
			if (!_speed.Equals(value))
			{
				if (_initRead)
				{
					_speed = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiSpeed(value);
				WriteLog(_speed, value, "Speed");
				_speed = value;
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

	public Color Wave
	{
		get
		{
			return _wave;
		}
		set
		{
			if (!_wave.Equals(value))
			{
				if (_initRead)
				{
					_wave = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiWaveColor(value);
				WriteLog(_wave, value, "WaveColor");
				_wave = value;
			}
		}
	}

	public int iWave
	{
		get
		{
			return _iwave;
		}
		set
		{
			if (!_iwave.Equals(value))
			{
				if (_initRead)
				{
					_iwave = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiWaveStatus(value);
				WriteLog(_iwave, value, "WaveStatus");
				_iwave = value;
			}
		}
	}

	public int iWaveFreeze
	{
		get
		{
			return _iwavefreeze;
		}
		set
		{
			if (!_iwavefreeze.Equals(value))
			{
				if (_initRead)
				{
					_iwavefreeze = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiWaveFreeze(value);
				WriteLog(_iwavefreeze, value, "WaveFreeze");
				_iwavefreeze = value;
			}
		}
	}

	public Color Breath
	{
		get
		{
			return _breath;
		}
		set
		{
			if (!_breath.Equals(value))
			{
				if (_initRead)
				{
					_breath = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiBreathColor(value);
				WriteLog(_breath, value, "BreathColor");
				_breath = value;
			}
		}
	}

	public int iBreath
	{
		get
		{
			return _ibreath;
		}
		set
		{
			if (!_ibreath.Equals(value))
			{
				if (_initRead)
				{
					_ibreath = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiBreathStatus(value);
				WriteLog(_ibreath, value, "BreathStatus");
				_ibreath = value;
			}
		}
	}

	public Color Scan1
	{
		get
		{
			return _scan1;
		}
		set
		{
			if (!_scan1.Equals(value))
			{
				if (_initRead)
				{
					_scan1 = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiScanColor1(value);
				WriteLog(_scan1, value, "ScanColor1");
				_scan1 = value;
			}
		}
	}

	public Color Scan2
	{
		get
		{
			return _scan2;
		}
		set
		{
			if (!_scan2.Equals(value))
			{
				if (_initRead)
				{
					_scan2 = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiScanColor2(value);
				WriteLog(_scan2, value, "ScanColor2");
				_scan2 = value;
			}
		}
	}

	public int iScan
	{
		get
		{
			return _iscan;
		}
		set
		{
			if (!_iscan.Equals(value))
			{
				if (_initRead)
				{
					_iscan = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiScanStatus(value);
				WriteLog(_iscan, value, "ScanStatus");
				_iscan = value;
			}
		}
	}

	public Color Blink
	{
		get
		{
			return _blink;
		}
		set
		{
			if (!_blink.Equals(value))
			{
				if (_initRead)
				{
					_blink = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiBlinkColor(value);
				WriteLog(_blink, value, "BlinkColor");
				_blink = value;
			}
		}
	}

	public int iBlink
	{
		get
		{
			return _iblink;
		}
		set
		{
			if (!_iblink.Equals(value))
			{
				if (_initRead)
				{
					_iblink = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiBlinkStatus(value);
				WriteLog(_iblink, value, "BlinkStatus");
				_iblink = value;
			}
		}
	}

	public Color Random
	{
		get
		{
			return _random;
		}
		set
		{
			if (!_random.Equals(value))
			{
				if (_initRead)
				{
					_random = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiRandomColor(value);
				WriteLog(_random, value, "RandomColor");
				_random = value;
			}
		}
	}

	public int iRandom
	{
		get
		{
			return _irandom;
		}
		set
		{
			if (!_irandom.Equals(value))
			{
				if (_initRead)
				{
					_irandom = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiRandomStatus(value);
				WriteLog(_irandom, value, "RandomStatus");
				_irandom = value;
			}
		}
	}

	public Color Ripple
	{
		get
		{
			return _ripple;
		}
		set
		{
			if (!_ripple.Equals(value))
			{
				if (_initRead)
				{
					_ripple = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiRippleColor(value);
				WriteLog(_ripple, value, "RippleColor");
				_ripple = value;
			}
		}
	}

	public int iRipple
	{
		get
		{
			return _iripple;
		}
		set
		{
			if (!_iripple.Equals(value))
			{
				if (_initRead)
				{
					_iripple = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiRippleStatus(value);
				WriteLog(_iripple, value, "RippleStatus");
				_iripple = value;
			}
		}
	}

	public Color Snake
	{
		get
		{
			return _snake;
		}
		set
		{
			if (!_snake.Equals(value))
			{
				if (_initRead)
				{
					_snake = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiSnakeColor(value);
				WriteLog(_snake, value, "SnakeColor");
				_snake = value;
			}
		}
	}

	public int iSnake
	{
		get
		{
			return _isnake;
		}
		set
		{
			if (!_isnake.Equals(value))
			{
				if (_initRead)
				{
					_isnake = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiSnakeStatus(value);
				WriteLog(_isnake, value, "SnakeStatus");
				_isnake = value;
			}
		}
	}

	public Color Fn
	{
		get
		{
			return _fn;
		}
		set
		{
			if (!_fn.Equals(value))
			{
				if (_initRead)
				{
					_fn = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiFnColor(value);
				WriteLog(_fn, value, "FnColor");
				_fn = value;
			}
		}
	}

	public int Static
	{
		get
		{
			return _istatic;
		}
		set
		{
			if (_istatic.Equals(value))
			{
				return;
			}
			if (_initRead)
			{
				_istatic = value;
				return;
			}
			InsydeAcpiProt.WriteAcpiStaticStatus(value);
			WriteLog(_istatic, value, "StaticStatus");
			_istatic = value;
			if (value.Equals(1))
			{
				WriteLogMode(12);
			}
		}
	}

	public static IntPtr InitPerkeyIo() => PerkeyApi.InitPerkeyIo();

	public static void SetLEDStatus(byte cmd, byte data0, byte data1, byte data2, byte data3) => PerkeyApi.SetLEDStatus(cmd, data0, data1, data2, data3);

	public static int GetLedData(byte cmd, byte type, byte[] arr, byte size) => PerkeyApi.GetLedData(cmd, type, arr, size);

	public void FirstRun()
	{
		if (!_initPerkeyIO)
		{
			InitPerkeyIo();
			GetMcuVersion();
			_initPerkeyIO = true;
		}
		InsydeAcpiProt.WriteAcpiStatus(1);
		InsydeAcpiProt.WriteAcpiBrightness(4);
		InsydeAcpiProt.WriteAcpiSpeed(2);
		InsydeAcpiProt.WriteAcpiStaticStatus(0);
		InsydeAcpiProt.PerkeyClearReg();
		if (SupportMcuNewEffect)
		{
			InsydeAcpiProt.WriteAcpiMode(7);
			InsydeAcpiProt.WriteAcpiWaveStatus(3);
			WriteLog("Effect", "Support New Effect");
		}
		else
		{
			InsydeAcpiProt.WriteAcpiMode(0);
			InsydeAcpiProt.WriteAcpiWaveStatus(0);
		}
		InsydeAcpiProt.WriteAcpiWaveColor(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiWaveFreeze(0);
		InsydeAcpiProt.WriteAcpiBreathStatus(0);
		InsydeAcpiProt.WriteAcpiBreathColor(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiScanColor1(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiScanColor2(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiScanStatus(0);
		InsydeAcpiProt.WriteAcpiBlinkStatus(0);
		InsydeAcpiProt.WriteAcpiBlinkColor(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiRandomStatus(0);
		InsydeAcpiProt.WriteAcpiRandomColor(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiRippleStatus(0);
		InsydeAcpiProt.WriteAcpiRippleColor(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiSnakeStatus(0);
		InsydeAcpiProt.WriteAcpiSnakeColor(Color.FromRgb((byte)0, (byte)0, byte.MaxValue));
		InsydeAcpiProt.WriteAcpiFnColor(Color.FromRgb(byte.MaxValue, (byte)0, (byte)0));
		if (InsydeAcpiProt.SupportPerkeyDefaultStaticColor())
		{
			ResetWmi13ToDefault();
			SupportCustomStaticColor = true;
			CustomDefaultStaticColor = GetDefaultLeftColor();
		}
		else
		{
			SupportCustomStaticColor = false;
		}
	}

	private Color GetDefaultLeftColor()
	{
		byte[] _copy = new byte[256];
		CopyWmi13(ref _copy);
		return Color.FromRgb(_copy[2], _copy[3], _copy[4]);
	}

	public void Init()
	{
		_initRead = true;
		Status = InsydeAcpiProt.ReadAcpiStatus();
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		Speed = InsydeAcpiProt.ReadAcpiSpeed();
		Static = InsydeAcpiProt.ReadAcpiStaticStatus();
		Mode = InsydeAcpiProt.ReadAcpiMode();
		Wave = InsydeAcpiProt.ReadAcpiWaveColor();
		iWave = InsydeAcpiProt.ReadAcpiWaveStatus();
		iWaveFreeze = InsydeAcpiProt.ReadAcpiWaveFreeze();
		Breath = InsydeAcpiProt.ReadAcpiBreathColor();
		iBreath = InsydeAcpiProt.ReadAcpiBreathStatus();
		Scan1 = InsydeAcpiProt.ReadAcpiScanColor1();
		Scan2 = InsydeAcpiProt.ReadAcpiScanColor2();
		iScan = InsydeAcpiProt.ReadAcpiScanStatus();
		Blink = InsydeAcpiProt.ReadAcpiBlinkColor();
		iBlink = InsydeAcpiProt.ReadAcpiBlinkStatus();
		Random = InsydeAcpiProt.ReadAcpiRandomColor();
		iRandom = InsydeAcpiProt.ReadAcpiRandomStatus();
		Ripple = InsydeAcpiProt.ReadAcpiRippleColor();
		iRipple = InsydeAcpiProt.ReadAcpiRippleStatus();
		Snake = InsydeAcpiProt.ReadAcpiSnakeColor();
		iSnake = InsydeAcpiProt.ReadAcpiSnakeStatus();
		Static = InsydeAcpiProt.ReadAcpiStaticStatus();
		Fn = InsydeAcpiProt.ReadAcpiFnColor();
		_initRead = false;
		if (!_initPerkeyIO)
		{
			InitPerkeyIo();
			GetMcuVersion();
			_initPerkeyIO = true;
		}
	}

	private void GetMcuVersion()
	{
		byte[] array = new byte[2];
		GetLedData(17, 4, array, Convert.ToByte(array.Length));
		byte[] array2 = new byte[9];
		byte[] array3 = new byte[3];
		GetLedData(17, 5, array3, Convert.ToByte(array3.Length));
		Array.Copy(array3, 0, array2, 0, 3);
		GetLedData(17, 6, array3, Convert.ToByte(array3.Length));
		Array.Copy(array3, 0, array2, 3, 3);
		GetLedData(17, 7, array3, Convert.ToByte(array3.Length));
		Array.Copy(array3, 0, array2, 6, 3);
		int num = Array.IndexOf(array2, Convert.ToByte(36));
		if (num > 0)
		{
			for (int i = num; i < array2.Length; i++)
			{
				array2[i] = 0;
			}
		}
		ClearColor();
		GetLedData(17, 16, array3, Convert.ToByte(array3.Length));
		if ((array3[0] & 1) == 1)
		{
			SupportMcuNewEffect = true;
		}
		if (array[0] >= 48 && array[0] <= 57 && array[1] >= 48 && array[1] <= 57)
		{
			Minor = Encoding.ASCII.GetString(array);
			Revision = Encoding.ASCII.GetString(array2);
			WriteLog("Version", Encoding.ASCII.GetString(array) + "." + Encoding.ASCII.GetString(array2));
		}
		else
		{
			WriteLog("Version", "NotSupportGetVersion");
		}
	}

	public void LoadStaticColor()
	{
		byte[] array = new byte[384];
		InsydeAcpiProt.ReadPerkeyAllStaticColor(ref array[0]);
		byte b = 0;
		for (int i = 0; i < 6; i++)
		{
			for (int j = 0; j < 20; j++)
			{
				byte b2 = array[64 * i + j * 3 + j / 5];
				byte b3 = array[64 * i + j * 3 + j / 5 + 1];
				byte b4 = array[64 * i + j * 3 + j / 5 + 2];
				if (!b2.Equals(0) || !b3.Equals(0) || !b4.Equals(0))
				{
					Color color = Color.FromRgb(b2, b3, b4);
					b = Convert.ToByte((i << 5) | j);
					SetStaticColor(b, color);
				}
			}
		}
		SetStatus();
	}

	public void UpdateStatus()
	{
		Status = InsydeAcpiProt.ReadAcpiStatus();
	}

	public void SetBrightnessLevel()
	{
		int num = 0;
		SetBrightnessValue(Brightness switch
		{
			1 => 2, 
			2 => 4, 
			3 => 6, 
			4 => 10, 
			_ => 0, 
		});
	}

	private void SetBrightnessValue(int value)
	{
		if (Status.Equals(0))
		{
			SetLEDStatus(9, 0, 0, 0, 0);
		}
		else
		{
			SetLEDStatus(9, (byte)value, (byte)Speed, 0, 0);
		}
	}

	public void UpdateBrightness()
	{
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
	}

	public void SetBrightnessDown()
	{
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		Status = 1;
		if (Brightness != 1)
		{
			Brightness--;
		}
		SetBrightnessLevel();
	}

	public void SetBrightnessUp()
	{
		Brightness = InsydeAcpiProt.ReadAcpiBrightness();
		Status = 1;
		if (Brightness != 4)
		{
			Brightness++;
		}
		SetBrightnessLevel();
	}

	public void SetOverrideBootEffect(int value)
	{
		byte[] bytes = BitConverter.GetBytes(503316480 + value);
		InsydeAcpiProt.SetDCHU_Data(121, bytes, 4);
		SetLEDStatus(32, (byte)value, 0, 0, 0);
	}

	public void SetMode()
	{
		switch (Mode)
		{
		case 0:
			SetWaveMode();
			break;
		case 1:
			if (iBreath.Equals(0))
			{
				SetBreathRandomMode();
			}
			else
			{
				SetBreathColorMode(Breath);
			}
			break;
		case 2:
			if (iBlink.Equals(0))
			{
				SetBlinkRandomMode();
			}
			else
			{
				SetBlinkColorMode(Blink);
			}
			break;
		case 3:
			SetRandomMode();
			break;
		case 4:
			SetScanMode();
			break;
		case 5:
			switch (iRipple)
			{
			case 0:
				SetRippleRandomMode();
				break;
			case 1:
				SetRippleColorMode(Ripple);
				break;
			case 2:
				SetFIAORandomMode();
				break;
			case 3:
				SetFIAOColorMode(Ripple);
				break;
			}
			break;
		case 6:
			SetSnakeMode();
			break;
		case 7:
			if (iWave < 8)
			{
				SetWaveMode(iWave);
			}
			else
			{
				SetWaveMode(iWave, Wave);
			}
			break;
		case 8:
			if (iScan.Equals(0))
			{
				SetScanRandomMode();
			}
			else
			{
				SetScanColorMode(Scan1, Scan2);
			}
			break;
		case 9:
			if (iRandom.Equals(0))
			{
				SetRandomRandomMode();
			}
			else
			{
				SetRandomColorMode(Random);
			}
			break;
		case 10:
			if (iSnake < 4)
			{
				SetSnakeMode(iSnake);
			}
			else
			{
				SetSnakeMode(iSnake, Snake);
			}
			break;
		case 11:
			SetFnColorMode(Fn);
			break;
		}
		SetStatus();
		WriteLogMode(Mode);
	}

	public void ClearColor()
	{
		SetLEDStatus(0, 12, 0, 0, 0);
		Thread.Sleep(100);
	}

	public void SetStatus()
	{
		if (Status.Equals(1))
		{
			SetBrightnessLevel();
		}
		else
		{
			SetLEDStatus(9, 0, 0, 0, 0);
		}
	}

	public void SetStaticColor(byte key, Color color)
	{
		SetLEDStatus(1, key, color.R, color.G, color.B);
	}

	public void SetWaveMode()
	{
		Mode = 0;
		SetLEDStatus(0, 4, 0, 0, 0);
		SetStatus();
	}

	public void SetWaveMode(int wave_mode)
	{
		iWave = wave_mode;
		Mode = 7;
		switch (wave_mode)
		{
		case 0:
			SetLEDStatus(21, 113, 0, 0, 0);
			break;
		case 1:
			SetLEDStatus(21, 114, 0, 0, 0);
			break;
		case 2:
			SetLEDStatus(21, 115, 0, 0, 0);
			break;
		case 3:
			SetLEDStatus(21, 116, 0, 0, 0);
			break;
		case 4:
			SetLEDStatus(21, 117, 0, 0, 0);
			break;
		case 5:
			SetLEDStatus(21, 118, 0, 0, 0);
			break;
		case 6:
			SetLEDStatus(21, 119, 0, 0, 0);
			break;
		case 7:
			SetLEDStatus(21, 120, 0, 0, 0);
			break;
		}
		SetStatus();
	}

	public void SetWaveMode(int wave_mode, Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iWave = wave_mode;
		Wave = color;
		Mode = 7;
		switch (wave_mode)
		{
		case 8:
			SetLEDStatus(21, 161, color.R, color.G, color.B);
			break;
		case 9:
			SetLEDStatus(21, 162, color.R, color.G, color.B);
			break;
		case 10:
			SetLEDStatus(21, 163, color.R, color.G, color.B);
			break;
		case 11:
			SetLEDStatus(21, 164, color.R, color.G, color.B);
			break;
		case 12:
			SetLEDStatus(21, 165, color.R, color.G, color.B);
			break;
		case 13:
			SetLEDStatus(21, 166, color.R, color.G, color.B);
			break;
		case 14:
			SetLEDStatus(21, 167, color.R, color.G, color.B);
			break;
		case 15:
			SetLEDStatus(21, 168, color.R, color.G, color.B);
			break;
		}
		SetStatus();
	}

	public void SetBreathRandomMode()
	{
		iBreath = 0;
		Mode = 1;
		SetLEDStatus(10, 0, 0, 0, 0);
		SetStatus();
	}

	public void SetBreathColorMode(Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iBreath = 1;
		Breath = color;
		Mode = 1;
		SetLEDStatus(10, 170, color.R, color.G, color.B);
		SetStatus();
	}

	public void SetScanMode()
	{
		Mode = 4;
		SetLEDStatus(0, 10, 0, 0, 0);
		SetStatus();
	}

	public void SetScanRandomMode()
	{
		iScan = 0;
		Mode = 8;
		SetLEDStatus(0, 10, 0, 0, 0);
		SetStatus();
	}

	public void SetScanColorMode(Color color1, Color color2)
	{
		if (color1.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)) && color2.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color1 = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
			color2 = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iScan = 1;
		Scan1 = color1;
		Scan2 = color2;
		Mode = 8;
		SetLEDStatus(23, 161, color1.R, color1.G, color1.B);
		SetLEDStatus(23, 162, color2.R, color2.G, color2.B);
		SetStatus();
	}

	public void SetBlinkRandomMode()
	{
		iBlink = 0;
		Mode = 2;
		SetLEDStatus(11, 0, 0, 0, 0);
		SetStatus();
	}

	public void SetBlinkColorMode(Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iBlink = 1;
		Blink = color;
		Mode = 2;
		SetLEDStatus(11, 170, color.R, color.G, color.B);
		SetStatus();
	}

	public void SetRandomMode()
	{
		Mode = 3;
		SetLEDStatus(0, 9, 0, 0, 0);
		SetStatus();
	}

	public void SetRandomRandomMode()
	{
		iRandom = 0;
		Mode = 9;
		SetLEDStatus(0, 9, 0, 0, 0);
		SetStatus();
	}

	public void SetRandomColorMode(Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iRandom = 1;
		Random = color;
		Mode = 9;
		SetLEDStatus(24, 161, color.R, color.G, color.B);
		SetStatus();
	}

	public void SetRippleRandomMode()
	{
		iRipple = 0;
		Mode = 5;
		SetLEDStatus(7, 0, 0, 0, 0);
		SetStatus();
		WriteLogMode(Mode);
	}

	public void SetRippleColorMode(Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iRipple = 1;
		Ripple = color;
		Mode = 5;
		SetLEDStatus(7, 0, 0, 0, 0);
		SetStatus();
		WriteLogMode(Mode);
	}

	public void SetFIAORandomMode()
	{
		iRipple = 2;
		Mode = 5;
		SetLEDStatus(7, 0, 0, 0, 0);
		SetStatus();
		WriteLogMode(Mode);
	}

	public void SetFIAOColorMode(Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iRipple = 3;
		Ripple = color;
		Mode = 5;
		SetLEDStatus(7, 0, 0, 0, 0);
		SetStatus();
		WriteLogMode(Mode);
	}

	public void UpdateRippleColor()
	{
		Ripple = InsydeAcpiProt.ReadAcpiRippleColor();
	}

	public void UpdateRippleStatus()
	{
		iRipple = InsydeAcpiProt.ReadAcpiRippleStatus();
	}

	public void SetSnakeMode()
	{
		Mode = 6;
		SetLEDStatus(0, 11, 0, 0, 0);
		SetStatus();
	}

	public void SetSnakeMode(int snakeMode)
	{
		iSnake = snakeMode;
		Mode = 10;
		switch (snakeMode)
		{
		case 0:
			SetLEDStatus(22, 113, 0, 0, 0);
			break;
		case 1:
			SetLEDStatus(22, 114, 0, 0, 0);
			break;
		case 2:
			SetLEDStatus(22, 115, 0, 0, 0);
			break;
		case 3:
			SetLEDStatus(22, 116, 0, 0, 0);
			break;
		}
		SetStatus();
	}

	public void SetSnakeMode(int snakeMode, Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		iSnake = snakeMode;
		Snake = color;
		Mode = 10;
		switch (snakeMode)
		{
		case 4:
			SetLEDStatus(22, 161, color.R, color.G, color.B);
			break;
		case 5:
			SetLEDStatus(22, 162, color.R, color.G, color.B);
			break;
		case 6:
			SetLEDStatus(22, 163, color.R, color.G, color.B);
			break;
		case 7:
			SetLEDStatus(22, 164, color.R, color.G, color.B);
			break;
		}
		SetStatus();
	}

	public void SetFnColorMode(Color color)
	{
		if (color.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
		{
			color = Color.FromRgb(byte.MaxValue, (byte)0, (byte)0);
		}
		Fn = color;
		Mode = 11;
		SetLEDStatus(0, 12, 0, 0, 0);
		WriteLogMode(Mode);
	}

	public void UpdateFnColor()
	{
		Fn = InsydeAcpiProt.ReadAcpiFnColor();
	}

	public void TriggerFnEffect()
	{
		SetLEDStatus(1, 1, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 2, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 3, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 5, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 6, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 7, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 8, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 9, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 10, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 11, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 12, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 33, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 35, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 49, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 50, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 51, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 83, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 115, byte.MaxValue, 0, 0);
		SetLEDStatus(1, 162, byte.MaxValue, 0, 0);
	}

	private string ColorToString(Color color)
	{
		return color.R + "_" + color.G + "_" + color.B;
	}

	private void WriteLog(int before, int after, string attribute)
	{
		StringBuilder stringBuilder = new StringBuilder("Keyboard_");
		stringBuilder.Append(attribute + "_" + after);
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		stringBuilder.Append("Set " + attribute + ": " + before + " -> " + after);
		ClevoLog.Write(stringBuilder.ToString());
		Thread.Sleep(30);
	}

	private void WriteLog(Color before, Color after, string attribute)
	{
		StringBuilder stringBuilder = new StringBuilder("Keyboard_");
		stringBuilder.Append(attribute + "_" + ColorToString(after));
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		stringBuilder.Append("Set " + attribute + ": " + ColorToString(before) + " -> " + ColorToString(after));
		ClevoLog.Write(stringBuilder.ToString());
		Thread.Sleep(30);
	}

	private void WriteLog(string attribute, string message)
	{
		StringBuilder stringBuilder = new StringBuilder("Keyboard_");
		stringBuilder.Append(attribute + "_" + message);
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		ClevoLog.Write(stringBuilder.ToString());
		Thread.Sleep(30);
	}

	private void WriteLogMode(int mode)
	{
		StringBuilder stringBuilder = new StringBuilder("clevokeyboard^");
		stringBuilder.Append("202^" + mode);
		ClevoLog.Write(stringBuilder.ToString());
		Thread.Sleep(30);
	}

	public PerkeyKB()
	{
	}
}
