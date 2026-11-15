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
using System.Reflection;
using System.Text;
using System.Threading;
using CC.other_Class;

namespace CC;

public sealed class KBModule : BaseKB
{
	public delegate void Arg001_Event(object sender, int Event, int E_Data);

	private WhiteKB whiteKB;

	private RGBKB rgbKB;

	private PerkeyKB perkeyKB;

	private int typeRule;

	public EventHandler UpdateBrightness;

	public EventHandler UpdateUILanguage;

	public EventHandler CloseUI;

	public EventHandler UpdateToggleRGB;

	public EventHandler UpdateWhiteLEDStatus;


	public int Status
	{
		get
		{
			int result = 0;
			switch (typeRule)
			{
			case 2:
				result = rgbKB.Status;
				break;
			case 6:
				result = rgbKB.Status;
				break;
			case 22:
				result = rgbKB.Status;
				break;
			case 3:
				result = perkeyKB.Status;
				break;
			case 1:
				whiteKB.UpdateStatus();
				result = whiteKB.Status;
				break;
			}
			return result;
		}
		set
		{
			switch (typeRule)
			{
			case 2:
				rgbKB.Status = value;
				break;
			case 6:
				rgbKB.Status = value;
				break;
			case 22:
				rgbKB.Status = value;
				break;
			case 3:
				perkeyKB.Status = value;
				break;
			case 1:
				whiteKB.Status = value;
				break;
			}
		}
	}

	public int Mode
	{
		get
		{
			int result = 0;
			switch (typeRule)
			{
			case 2:
				result = rgbKB.Mode;
				break;
			case 6:
				result = rgbKB.Mode;
				break;
			case 22:
				result = rgbKB.Mode;
				break;
			case 3:
				result = perkeyKB.Mode;
				break;
			}
			return result;
		}
		set
		{
			switch (typeRule)
			{
			case 2:
				rgbKB.Mode = value;
				break;
			case 6:
				rgbKB.Mode = value;
				break;
			case 22:
				rgbKB.Mode = value;
				break;
			case 3:
				perkeyKB.Mode = value;
				break;
			}
		}
	}

	public int Speed
	{
		get
		{
			int result = 0;
			if (typeRule == 3)
			{
				result = perkeyKB.Speed;
			}
			return result;
		}
		set
		{
			if (typeRule == 3)
			{
				perkeyKB.Speed = value;
			}
		}
	}

	public int StaticStatus
	{
		get
		{
			int result = 0;
			if (typeRule == 3)
			{
				result = perkeyKB.Static;
			}
			return result;
		}
		set
		{
			if (typeRule == 3)
			{
				perkeyKB.Static = value;
			}
		}
	}

	public event Arg001_Event Arg001_GetEvent;

	public KBModule()
	{
		whiteKB = new WhiteKB();
		rgbKB = new RGBKB();
		perkeyKB = new PerkeyKB();
		typeRule = GetKBType();
	}

	public string GetCurrentAppVersion()
	{
		return InsydeAcpiProt.ReadAcpiAppVersion();
	}

	public void FirstRun()
	{
		switch (typeRule)
		{
		case 1:
			whiteKB.FirstRun();
			break;
		case 2:
			rgbKB.FirstRun();
			break;
		case 6:
			rgbKB.FirstRun();
			break;
		case 22:
			rgbKB.FirstRun();
			break;
		case 3:
			perkeyKB.FirstRun();
			break;
		}
		SleepTimer = 30;
		SleepTimerTrigger = 1;
		SetSleepTimerTrigger(30);
		if (SupportOverrideBootEffect())
		{
			InsydeAcpiProt.WriteAcpiOverrideBootEffect(0);
		}
		WriteKBTypeToReg();
		InsydeAcpiProt.WriteAcpiAppVersion();
	}

	public void Init()
	{
		StringBuilder stringBuilder = new StringBuilder("Start LED Keyboard Setting and Initial\nVersion: " + Assembly.GetExecutingAssembly().GetName().Version);
		WriteLog(stringBuilder.ToString(), "Led_Keyboard_Setting");
		switch (typeRule)
		{
		case 1:
			whiteKB.Init();
			break;
		case 2:
			rgbKB.Init();
			break;
		case 6:
			rgbKB.Init();
			break;
		case 22:
			rgbKB.Init();
			break;
		case 3:
			perkeyKB.Init();
			break;
		}
		SleepTimer = InsydeAcpiProt.ReadAcpiSleepTimer();
		SleepTimerTrigger = InsydeAcpiProt.ReadAcpiSleepTimerTrigger();
		if (SleepTimerTrigger.Equals(1))
		{
			SetSleepTimerTrigger(SleepTimer);
		}
		else
		{
			SetSleepTimerTriggerOff();
		}
		if (SupportOverrideBootEffect())
		{
			OverrideBootEffect = InsydeAcpiProt.ReadAcpiOverrideBootEffect();
		}
		InsydeAcpiProt.SetAcpiAppLaunch(1);
		WriteLog("Opening", "Launch");
	}

	public void Closing()
	{
		InsydeAcpiProt.SetAcpiAppLaunch(0);
		WriteLog("Closing", "Launch");
	}

	public void SetOverrideBootEffect(int value)
	{
		if (SupportOverrideBootEffect())
		{
			OverrideBootEffect = value;
			switch (GetKBType())
			{
			case 1:
				whiteKB.SetOverrideBootEffect(value);
				break;
			case 2:
				rgbKB.SetOverrideBootEffect(value);
				break;
			case 6:
				rgbKB.SetOverrideBootEffect(value);
				break;
			case 22:
				rgbKB.SetOverrideBootEffect(value);
				break;
			case 3:
				perkeyKB.SetOverrideBootEffect(value);
				break;
			}
		}
	}

	public void SetStatus()
	{
		switch (typeRule)
		{
		case 2:
			rgbKB.SetStatus();
			break;
		case 6:
			rgbKB.SetStatus();
			break;
		case 22:
			rgbKB.SetStatus();
			break;
		case 3:
			perkeyKB.SetStatus();
			break;
		}
	}

	public void SetBrightnessLevel(int value)
	{
		switch (typeRule)
		{
		case 1:
			if (value == 0)
			{
				Status = 0;
				whiteKB.SetBrightnessOFF();
			}
			else
			{
				Status = 1;
				whiteKB.SetBrightnessLevel(value);
			}
			break;
		case 2:
			if (value == 0)
			{
				Status = 0;
			}
			else
			{
				Status = 1;
				rgbKB.SetBrightnessLevel(value);
			}
			rgbKB.SetLedOnOff();
			break;
		case 6:
			if (value == 0)
			{
				Status = 0;
			}
			else
			{
				Status = 1;
				rgbKB.SetBrightnessLevel(value);
			}
			rgbKB.SetLedOnOff();
			break;
		case 22:
			if (value == 0)
			{
				Status = 0;
			}
			else
			{
				Status = 1;
				rgbKB.SetBrightnessLevel(value);
			}
			rgbKB.SetLedOnOff();
			break;
		case 3:
			if (value.Equals(0))
			{
				Status = 0;
			}
			else
			{
				Status = 1;
				perkeyKB.Brightness = value;
			}
			perkeyKB.SetBrightnessLevel();
			break;
		}
	}

	public int GetBrightness()
	{
		int result = 0;
		switch (typeRule)
		{
		case 1:
			result = whiteKB.Brightness;
			break;
		case 2:
			result = rgbKB.Brightness;
			break;
		case 6:
			result = rgbKB.Brightness;
			break;
		case 22:
			result = rgbKB.Brightness;
			break;
		case 3:
			result = perkeyKB.Brightness;
			break;
		}
		return result;
	}

	public void SetBrightnessUp()
	{
		switch (typeRule)
		{
		case 2:
			rgbKB.SetBrightnessUp();
			break;
		case 6:
			rgbKB.SetBrightnessUp();
			break;
		case 22:
			rgbKB.SetBrightnessUp();
			break;
		case 3:
			perkeyKB.SetBrightnessUp();
			break;
		case 1:
			whiteKB.SetBrightnessUp();
			break;
		}
	}

	public void SetBrightnessDown()
	{
		switch (typeRule)
		{
		case 2:
			rgbKB.SetBrightnessDown();
			break;
		case 6:
			rgbKB.SetBrightnessDown();
			break;
		case 22:
			rgbKB.SetBrightnessDown();
			break;
		case 3:
			perkeyKB.SetBrightnessDown();
			break;
		case 1:
			whiteKB.SetBrightnessDown();
			break;
		}
	}

	public int UpdateCurrentBrightness()
	{
		int result = 0;
		switch (typeRule)
		{
		case 1:
			whiteKB.UpdateBrightness();
			result = whiteKB.Brightness;
			break;
		case 2:
			rgbKB.UpdateBrightness();
			result = rgbKB.Brightness;
			break;
		case 6:
			rgbKB.UpdateBrightness();
			result = rgbKB.Brightness;
			break;
		case 22:
			rgbKB.UpdateBrightness();
			result = rgbKB.Brightness;
			break;
		case 3:
			perkeyKB.UpdateBrightness();
			result = perkeyKB.Brightness;
			break;
		}
		return result;
	}

	public int UpdateCurrentStatus()
	{
		int result = 0;
		switch (typeRule)
		{
		case 2:
			rgbKB.UpdateStatus();
			result = rgbKB.Status;
			break;
		case 6:
			rgbKB.UpdateStatus();
			result = rgbKB.Status;
			break;
		case 22:
			rgbKB.UpdateStatus();
			result = rgbKB.Status;
			break;
		case 3:
			perkeyKB.UpdateStatus();
			result = perkeyKB.Status;
			break;
		case 1:
			whiteKB.UpdateStatus();
			result = whiteKB.Status;
			break;
		}
		return result;
	}

	public void SetMode()
	{
		switch (typeRule)
		{
		case 2:
			rgbKB.SetMode();
			rgbKB.SetLedOnOff();
			break;
		case 6:
			rgbKB.SetMode();
			rgbKB.SetLedOnOff();
			break;
		case 22:
			rgbKB.SetMode();
			rgbKB.SetLedOnOff();
			break;
		case 3:
			perkeyKB.SetMode();
			break;
		}
	}

	public void GetWhiteBrightnessNow()
	{
		if (GetKBType() == 1)
		{
			whiteKB.GetBrightnessNow();
		}
	}

	public Color GetRGBAllColor()
	{
		return rgbKB.All;
	}

	public Color GetRGBLeftColor()
	{
		return rgbKB.Left;
	}

	public Color GetRGBMidColor()
	{
		return rgbKB.Mid;
	}

	public Color GetRGBRightColor()
	{
		return rgbKB.Right;
	}

	public Color GetRGBLogoColor()
	{
		return rgbKB.Logo;
	}

	public Color GetRGBLightBarColor()
	{
		return rgbKB.LightBar;
	}

	public void SetRGBAllColor(Color color)
	{
		rgbKB.All = color;
	}

	public void SetRGBLeftColor(Color color)
	{
		rgbKB.Left = color;
	}

	public void SetRGBMidColor(Color color)
	{
		rgbKB.Mid = color;
	}

	public void SetRGBRightColor(Color color)
	{
		rgbKB.Right = color;
	}

	public void SetRGBLogoColor(Color color)
	{
		rgbKB.iLogo = 1;
		rgbKB.Logo = color;
	}

	public void SetRGBLightBarColor(Color color)
	{
		rgbKB.LightBar = color;
	}

	public void SetRGB3ParStatusOn()
	{
		UpdateCurrentStatus();
		rgbKB.iLeft = 1;
		rgbKB.iMid = 1;
		rgbKB.iRight = 1;
	}

	public void PerkeyClearColor()
	{
		perkeyKB.ClearColor();
	}

	public void PerkeyStaticDefault()
	{
		perkeyKB.ClearColor();
		InsydeAcpiProt.PerkeyClearReg();
	}

	public void SetPerkeyBrightnessSpeed()
	{
		perkeyKB.SetBrightnessLevel();
	}

	public void SetPerkeyStatus()
	{
		perkeyKB.SetStatus();
	}

	public int GetPerkeyWaveStatus()
	{
		return perkeyKB.iWave;
	}

	public int GetPerkeyBreathStatus()
	{
		return perkeyKB.iBreath;
	}

	public int GetPerkeyScanStatus()
	{
		return perkeyKB.iScan;
	}

	public int GetPerkeyBlinkStatus()
	{
		return perkeyKB.iBlink;
	}

	public int GetPerkeyRandomStatus()
	{
		return perkeyKB.iRandom;
	}

	public int GetPerkeyRippleStatus()
	{
		return perkeyKB.iRipple;
	}

	public int GetPerkeySnakeStatus()
	{
		return perkeyKB.iSnake;
	}

	public Color GetPerkeyWaveColor()
	{
		return perkeyKB.Wave;
	}

	public Color GetPerkeyBreathColor()
	{
		return perkeyKB.Breath;
	}

	public Color GetPerkeyScanColor1()
	{
		return perkeyKB.Scan1;
	}

	public Color GetPerkeyScanColor2()
	{
		return perkeyKB.Scan2;
	}

	public Color GetPerkeyBlinkColor()
	{
		return perkeyKB.Blink;
	}

	public Color GetPerkeyRandomColor()
	{
		return perkeyKB.Random;
	}

	public Color GetPerkeyRippleColor()
	{
		return perkeyKB.Ripple;
	}

	public Color GetPerkeySnakeColor()
	{
		return perkeyKB.Snake;
	}

	public Color GetPerkeyFnColor()
	{
		return perkeyKB.Fn;
	}

	public void UpdatePerkeyRippleStatus()
	{
		perkeyKB.UpdateRippleStatus();
	}

	public void UpdatePerkeyRippleColor()
	{
		perkeyKB.UpdateRippleColor();
	}

	public void UpdatePerkeyFnColor()
	{
		perkeyKB.UpdateFnColor();
	}

	public void SetPerkeyBreathRandomMode()
	{
		perkeyKB.SetBreathRandomMode();
	}

	public void SetPerkeyBreathColorMode()
	{
		perkeyKB.SetBreathColorMode(perkeyKB.Breath);
	}

	public void SetPerkeyBreathColorMode(Color color)
	{
		perkeyKB.SetBreathColorMode(color);
	}

	public void SetPerkeyWaveMode()
	{
		perkeyKB.SetWaveMode();
	}

	public void SetPerkeyWaveDefaultMode(int wave_mode)
	{
		perkeyKB.SetWaveMode(wave_mode);
	}

	public void SetPerkeyWaveColorMode(int wave_mode)
	{
		perkeyKB.SetWaveMode(wave_mode, perkeyKB.Wave);
	}

	public void SetPerkeyWaveColorMode(int wave_mode, Color color)
	{
		perkeyKB.SetWaveMode(wave_mode, color);
	}

	public void SetPerkeySnakeRandomMode(int snakeMode)
	{
		perkeyKB.SetSnakeMode(snakeMode);
	}

	public void SetPerkeySnakeColorMode(int snakeMode)
	{
		perkeyKB.SetSnakeMode(snakeMode, perkeyKB.Snake);
	}

	public void SetPerkeySnakeColorMode(int snakeMode, Color color)
	{
		perkeyKB.SetSnakeMode(snakeMode, color);
	}

	public void SetPerkeyBlinkRandomMode()
	{
		perkeyKB.SetBlinkRandomMode();
	}

	public void SetPerkeyBlinkColorMode()
	{
		perkeyKB.SetBlinkColorMode(perkeyKB.Blink);
	}

	public void SetPerkeyBlinkColorMode(Color color)
	{
		perkeyKB.SetBlinkColorMode(color);
	}

	public void SetPerkeyRandomMode()
	{
		perkeyKB.SetRandomMode();
	}

	public void SetPerkeyRandomRandomMode()
	{
		perkeyKB.SetRandomRandomMode();
	}

	public void SetPerkeyRandomColorMode()
	{
		perkeyKB.SetRandomColorMode(perkeyKB.Random);
	}

	public void SetPerkeyRandomColorMode(Color color)
	{
		perkeyKB.SetRandomColorMode(color);
	}

	public void SetPerkeyScanMode()
	{
		perkeyKB.SetScanMode();
	}

	public void SetPerkeyScanRandomMode()
	{
		perkeyKB.SetScanRandomMode();
	}

	public void SetPerkeyScanColorMode()
	{
		perkeyKB.SetScanColorMode(perkeyKB.Scan1, perkeyKB.Scan2);
	}

	public void SetPerkeyScanColorMode(Color color1, Color color2)
	{
		perkeyKB.SetScanColorMode(color1, color2);
	}

	public void SetPerkeySnakeMode()
	{
		perkeyKB.SetSnakeMode();
	}

	public void SetPerkeyRippleRandomMode()
	{
		perkeyKB.SetRippleRandomMode();
	}

	public void SetPerkeyRippleColorMode(Color color)
	{
		perkeyKB.SetRippleColorMode(color);
	}

	public void SetPerkeyFIAORandomMode()
	{
		perkeyKB.SetFIAORandomMode();
	}

	public void SetPerkeyFIAOColorMode(Color color)
	{
		perkeyKB.SetFIAOColorMode(color);
	}

	public void SetPerkeyFnColorMode(Color color)
	{
		perkeyKB.SetFnColorMode(color);
	}

	public Color GetPerkeyStaticColor(int row, int col)
	{
		return InsydeAcpiProt.GetStaticKeyColor(row, col);
	}

	public void SetPerkeyStaticColor(byte key, Color color)
	{
		perkeyKB.SetStaticColor(key, color);
	}

	public void SetPerkeyStaticColorWithReg(byte key, Color color)
	{
		perkeyKB.SetStaticColor(key, color);
		int row = (key >> 5) & 7;
		int col = key & 0x1F;
		InsydeAcpiProt.SetStaticKeyColor(row, col, color);
	}

	public bool SupportPerkeyCustomStatic()
	{
		return perkeyKB.SupportCustomStaticColor;
	}

	public Color PerkeyCustomDefaultStaticColor()
	{
		return perkeyKB.CustomDefaultStaticColor;
	}

	public void SetPerkeyStaticColorOnlyReg(byte key, Color color)
	{
		int row = (key >> 5) & 7;
		int col = key & 0x1F;
		InsydeAcpiProt.SetStaticKeyColor(row, col, color);
	}

	public bool SupportPerkeyNewEffect()
	{
		return perkeyKB.SupportMcuNewEffect;
	}

	public void ReadPerkeyAllStaticColor(ref byte buffer)
	{
		InsydeAcpiProt.ReadPerkeyAllStaticColor(ref buffer);
	}

	public void WritePerkeyAllStaticColor(ref byte buffer)
	{
		InsydeAcpiProt.WritePerkeyAllStaticColor(ref buffer);
	}

	/// <summary>Original: handler for the Windows EventLog "PowerBIOSServer_Out" (service messages). No service on Linux; kept for a future daemon hook.</summary>
	public void OnServiceMessage(string message)
	{
		new Thread(DoWork).Start(message);
	}

	private void DoWork(object param)
	{
		if (((string)param).Contains("clevofnkey^201"))
		{
			UpdateBrightness?.Invoke(null, null);
		}
		if (((string)param).Contains("clevofnkey^203"))
		{
			CloseUI?.Invoke(null, null);
		}
		if (((string)param).Contains("clevofnkey^102"))
		{
			UpdateUILanguage?.Invoke(null, null);
		}
		if (((string)param).Contains("clevofnkey^209"))
		{
			UpdateToggleRGB?.Invoke(null, null);
		}
		if (((string)param).Contains("clevofnkey^20A"))
		{
			UpdateWhiteLEDStatus?.Invoke(null, null);
		}
		string[] array = ((string)param).Split(new char[1] { '^' });
		if (array.Length != 1 && array[0] == "Arg001")
		{
			ClevoLog.Write("get Arg001");
			if (Arg001_GetEvent != null)
			{
				int num = Convert.ToInt32(array[1]) & 0xFF;
				int e_Data = Convert.ToInt32(array[1]) >> 8;
				Arg001_GetEvent(this, num, e_Data);
			}
		}
	}

	public void WriteLog(string str, string attribute)
	{
		StringBuilder stringBuilder = new StringBuilder("Keyboard_");
		stringBuilder.Append(attribute);
		stringBuilder.Append("\n\n");
		stringBuilder.Append("KB Type: " + GetKBType() + "\n");
		stringBuilder.Append(str);
		ClevoLog.Write(stringBuilder.ToString());
	}
}
