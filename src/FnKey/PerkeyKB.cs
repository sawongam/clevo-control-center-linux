using System;
using System.Drawing;
using System.Text;
using System.Threading;
using FnKey.other_Class;

namespace FnKey;

// Test1.Class1: Windows low-level keyboard hook that triggers the ripple effect on key presses. Not available on Linux.
internal class KeyboardEventArgs : EventArgs
{
	public int Status;
	public int KeyValue;
}

internal class Class1
{
	public bool Enable;
	public event EventHandler KeyPress;
}

internal class PerkeyKB : BaseKB
{
	private bool _initRead;

	private bool _initPerkeyIO;

	private bool _initHookEvent;

	public bool SupportCustomStaticColor;

	public Color CustomDefaultStaticColor;

	public bool SupportGetMcuVersion;

	public bool SupportMcuNewEffect;

	public string Minor;

	public string Revision;

	private int _perkey_language;

	private int _perkey_type;

	public Class1 myKB_Hook;

	private int _iStatus;

	private int _brightness;

	private int _speed;

	private int _iMode;

	private Color _wave = Color.FromArgb(0, 0, 255);

	private int _iwave;

	private int _iwavefreeze;

	private Color _breath = Color.FromArgb(0, 0, 255);

	private int _ibreath;

	private Color _scan1 = Color.FromArgb(0, 0, 255);

	private Color _scan2 = Color.FromArgb(0, 0, 255);

	private int _iscan;

	private Color _blink = Color.FromArgb(0, 0, 255);

	private int _iblink;

	private Color _random = Color.FromArgb(0, 0, 255);

	private int _irandom;

	private Color _ripple = Color.FromArgb(0, 0, 255);

	private int _iripple;

	private Color _snake = Color.FromArgb(0, 0, 255);

	private int _isnake;

	private Color _fn = Color.FromArgb(0, 0, 255);

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
			if (!_wave.Equals((object)value))
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
			if (!_breath.Equals((object)value))
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
			if (!_scan1.Equals((object)value))
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
			if (!_scan2.Equals((object)value))
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
			if (!_blink.Equals((object)value))
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
			if (!_random.Equals((object)value))
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
			if (!_ripple.Equals((object)value))
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
			if (!_snake.Equals((object)value))
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
			if (!_fn.Equals((object)value))
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
			if (!_istatic.Equals(value))
			{
				if (_initRead)
				{
					_istatic = value;
					return;
				}
				InsydeAcpiProt.WriteAcpiStaticStatus(value);
				WriteLog(_istatic, value, "StaticStatus");
				_istatic = value;
			}
		}
	}

	// perkey_api.dll talks to the per-key keyboard MCU over USB-HID. Not available on Linux: STUBS that only log.
	// (A real port needs hidraw access to the MCU; see PORT_NOTES.md.) This laptop has KBTP=6 (RGB), so this class is not used.
	public static IntPtr InitPerkeyIo() { DebugLog.WriteDbgLog2Outlog("STUB perkey_api!InitPerkeyIo"); return IntPtr.Zero; }

	public static void SetLEDStatus(byte cmd, byte data0, byte data1, byte data2, byte data3)
	{
		DebugLog.WriteDbgLog2Outlog("STUB perkey_api!SetLEDStatus(" + cmd + "," + data0 + "," + data1 + "," + data2 + "," + data3 + ")");
	}

	public static int GetLedData(byte cmd, byte type, byte[] arr, byte size) { return 0; }

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
		}
		else
		{
			InsydeAcpiProt.WriteAcpiMode(0);
			InsydeAcpiProt.WriteAcpiWaveStatus(0);
		}
		InsydeAcpiProt.WriteAcpiWaveColor(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiWaveFreeze(0);
		InsydeAcpiProt.WriteAcpiBreathStatus(0);
		InsydeAcpiProt.WriteAcpiBreathColor(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiScanColor1(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiScanColor2(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiScanStatus(0);
		InsydeAcpiProt.WriteAcpiBlinkStatus(0);
		InsydeAcpiProt.WriteAcpiBlinkColor(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiRandomStatus(0);
		InsydeAcpiProt.WriteAcpiRandomColor(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiRippleStatus(0);
		InsydeAcpiProt.WriteAcpiRippleColor(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiSnakeStatus(0);
		InsydeAcpiProt.WriteAcpiSnakeColor(Color.FromArgb(0, 0, 255));
		InsydeAcpiProt.WriteAcpiFnColor(Color.FromArgb(255, 0, 0));
		if (Global.features.SupportPerkeyDefaultStaticColor)
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
		return Color.FromArgb(_copy[2], _copy[3], _copy[4]);
	}

	private void GetPerkeyLayoutInfo()
	{
		byte[] array = new byte[1];
		GetLedData(17, 1, array, Convert.ToByte(array.Length));
		_perkey_language = array[0] & 0xF;
		_perkey_type = (array[0] >> 4) & 0xF;
		WriteLog("MCU_0x01", array[0].ToString("X2"));
	}

	private int GetPerkeyLanguage()
	{
		int num = 0;
		byte[] _copy = new byte[256];
		CopyWmi13(ref _copy);
		if (_copy[15] == 243)
		{
			return _perkey_language;
		}
		return (_copy[15] >> 4) & 0xF;
	}

	public void Init()
	{
		if (_initHookEvent.Equals(obj: false))
		{
			myKB_Hook = new Class1();
			myKB_Hook.KeyPress += MyKB_Hook_KeyPress;
			_initHookEvent = true;
		}
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
			GetPerkeyLayoutInfo();
			_initPerkeyIO = true;
		}
		if (Status.Equals(0))
		{
			SetLEDStatus(9, 0, 0, 0, 0);
		}
		if (Static == 1)
		{
			ClearColor();
			LoadStaticColor();
		}
		else
		{
			ClearColor();
			SetMode();
		}
		SetStatus();
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
			SupportGetMcuVersion = true;
			Minor = Encoding.ASCII.GetString(array);
			Revision = Encoding.ASCII.GetString(array2);
		}
		else
		{
			SupportGetMcuVersion = false;
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
				byte red = array[64 * i + j * 3 + j / 5];
				byte green = array[64 * i + j * 3 + j / 5 + 1];
				byte blue = array[64 * i + j * 3 + j / 5 + 2];
				if (!red.Equals(0) || !green.Equals(0) || !blue.Equals(0))
				{
					Color color = Color.FromArgb(red, green, blue);
					b = Convert.ToByte((i << 5) | j);
					SetStaticColor(b, color);
				}
			}
		}
		SetStatus();
	}

	private void MyKB_Hook_KeyPress(object sender, EventArgs e)
	{
		KeyboardEventArgs e2 = e as KeyboardEventArgs;
		if (Mode.Equals(5))
		{
			if (e2.Status != 0)
			{
				return;
			}
			uint num = (uint)e2.KeyValue;
			if (num == 57398)
			{
				num = 54u;
			}
			if (num >> 8 == 224)
			{
				num |= 0x80;
			}
			if (num != 0)
			{
				int perkeyRippleStatus = Global.KB.GetPerkeyRippleStatus();
				Color perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
				switch (perkeyRippleStatus)
				{
				case 0:
					SetLEDStatus(7, byte.MaxValue, (byte)num, 0, 0);
					break;
				case 1:
					SetLEDStatus(7, (byte)num, perkeyRippleColor.R, perkeyRippleColor.G, perkeyRippleColor.B);
					break;
				case 2:
					SetLEDStatus(8, byte.MaxValue, (byte)num, 0, 0);
					break;
				case 3:
					SetLEDStatus(8, (byte)num, perkeyRippleColor.R, perkeyRippleColor.G, perkeyRippleColor.B);
					break;
				}
			}
		}
		else if (Mode.Equals(11) && ((uint)e2.KeyValue).Equals(111u))
		{
			if (e2.Status.Equals(0))
			{
				TriggerFnEffect();
			}
			else
			{
				SetLEDStatus(0, 12, 0, 0, 0);
			}
		}
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
		InsydeAcpiProt.SetDCHU_Data(121, bytes, bytes.Length);
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
			myKB_Hook.Enable = true;
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
			myKB_Hook.Enable = true;
			break;
		}
		SetStatus();
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
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
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
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
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
		if (color1.Equals((object)Color.FromArgb(0, 0, 0)) && color2.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color1 = Color.FromArgb(255, 0, 0);
			color2 = Color.FromArgb(255, 0, 0);
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
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
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
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
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
	}

	public void SetRippleColorMode(Color color)
	{
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
		}
		iRipple = 1;
		Ripple = color;
		Mode = 5;
		SetLEDStatus(7, 0, 0, 0, 0);
		SetStatus();
	}

	public void SetFIAORandomMode()
	{
		iRipple = 2;
		Mode = 5;
		SetLEDStatus(7, 0, 0, 0, 0);
		SetStatus();
	}

	public void SetFIAOColorMode(Color color)
	{
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
		}
		iRipple = 3;
		Ripple = color;
		Mode = 5;
		SetLEDStatus(7, 0, 0, 0, 0);
		SetStatus();
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
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
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
		if (color.Equals((object)Color.FromArgb(0, 0, 0)))
		{
			color = Color.FromArgb(255, 0, 0);
		}
		Fn = color;
		Mode = 11;
		SetLEDStatus(0, 12, 0, 0, 0);
	}

	public void UpdateFnColor()
	{
		Fn = InsydeAcpiProt.ReadAcpiFnColor();
	}

	public void TriggerFnEffect()
	{
		SetLEDStatus(1, 0, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 1, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 2, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 3, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 5, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 6, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 7, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 8, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 9, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 10, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 11, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 12, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 33, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 35, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 49, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 50, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 51, Fn.R, Fn.G, Fn.B);
		SetLEDStatus(1, 83, Fn.R, Fn.G, Fn.B);
		if (GetPerkeyLanguage() != 3)
		{
			SetLEDStatus(1, 115, Fn.R, Fn.G, Fn.B);
		}
		SetLEDStatus(1, 47, Fn.R, Fn.G, Fn.B);
		if (GetPerkeyLanguage() != 2)
		{
			SetLEDStatus(1, 46, Fn.R, Fn.G, Fn.B);
		}
		SetLEDStatus(1, 162, Fn.R, Fn.G, Fn.B);
	}

	private string ColorToString(Color color)
	{
		return color.R + "_" + color.G + "_" + color.B;
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

	private void WriteLog(string attribute, string message)
	{
		StringBuilder stringBuilder = new StringBuilder("Clevo_");
		stringBuilder.Append(attribute + "_" + message);
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		DebugLog.WriteDbgLog2Inlog(stringBuilder.ToString());
		Thread.Sleep(30);
	}
}
