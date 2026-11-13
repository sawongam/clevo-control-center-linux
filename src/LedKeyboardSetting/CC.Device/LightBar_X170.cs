using RippleCC.Platform;
using System;
using System.Runtime.InteropServices;

namespace CC.Device;

public class LightBar_X170
{
	public byte Speed = 1;

	public byte Brightness = 4;

	public byte LED_ON = 1;

	public byte Mode;

	public byte BreathMode;

	public byte BlinkMode;

	public byte MeteorMode;

	public byte Static_R = byte.MaxValue;

	public byte Static_G;

	public byte Static_B;

	public byte Breath_R = byte.MaxValue;

	public byte Breath_G;

	public byte Breath_B;

	public byte Blink_R = byte.MaxValue;

	public byte Blink_G;

	public byte Blink_B;

	public byte Meteor_R = byte.MaxValue;

	public byte Meteor_G;

	public byte Meteor_B;

	public static int ReadAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.ReadAppSettings(page, offset, length, ref buffer);

	public static int WriteAppSettings(int page, int offset, int length, ref byte buffer) => InsydeDCHU.WriteAppSettings(page, offset, length, ref buffer);

	public static bool InitLineLEDIo() => LineLEDAPI.InitLineLEDIo();

	public static int SetLightBarData62(byte cmd, byte[] data) => LineLEDAPI.SetLightBarData62(cmd, data);

	public void FirstRun()
	{
	}

	public bool Init()
	{
		if (InitLineLEDIo())
		{
			ReadX170LightBarData();
			return true;
		}
		return false;
	}

	public void ReadX170LightBarData()
	{
		byte[] array = new byte[19];
		ReadAppSettings(2, 108, 19, ref array[0]);
		Mode = array[0];
		if (Mode == 0)
		{
			Mode = 181;
			array[0] = Mode;
			array[1] = Speed;
			array[2] = Brightness;
			array[3] = LED_ON;
			array[4] = Static_R;
			array[5] = Static_G;
			array[6] = Static_B;
			array[7] = BreathMode;
			array[8] = Breath_R;
			array[9] = Breath_G;
			array[10] = Breath_B;
			array[11] = BlinkMode;
			array[12] = Blink_R;
			array[13] = Blink_G;
			array[14] = Blink_B;
			array[15] = MeteorMode;
			array[16] = Meteor_R;
			array[17] = Meteor_G;
			array[18] = Meteor_B;
			WriteAppSettings(2, 108, 19, ref array[0]);
		}
		else
		{
			Speed = array[1];
			Brightness = array[2];
			LED_ON = array[3];
			Static_R = array[4];
			Static_G = array[5];
			Static_B = array[6];
			BreathMode = array[7];
			Breath_R = array[8];
			Breath_G = array[9];
			Breath_B = array[10];
			BlinkMode = array[11];
			Blink_R = array[12];
			Blink_G = array[13];
			Blink_B = array[14];
			MeteorMode = array[15];
			Meteor_R = array[16];
			Meteor_G = array[17];
			Meteor_B = array[18];
		}
	}

	public void Set_Mode(byte cmd)
	{
		Mode = cmd;
		byte[] array = new byte[62];
		array[0] = 0;
		array[1] = Speed;
		SetLightBarData62(cmd, array);
		WriteAppSettings(2, 108, 3, ref (new byte[3] { cmd, Speed, Brightness })[0]);
		byte[] array2 = new byte[1] { 0 };
		switch (cmd)
		{
		case 178:
			BreathMode = 0;
			WriteAppSettings(2, 115, 1, ref array2[0]);
			break;
		case 179:
			BlinkMode = 0;
			WriteAppSettings(2, 119, 1, ref array2[0]);
			break;
		case 181:
			MeteorMode = 0;
			WriteAppSettings(2, 123, 1, ref array2[0]);
			break;
		}
	}

	public void Set_CustomRGB(byte cmd, byte R, byte G, byte B)
	{
		Mode = cmd;
		byte[] array = new byte[62];
		array[0] = 1;
		array[1] = Speed;
		array[2] = R;
		array[3] = G;
		array[4] = B;
		SetLightBarData62(cmd, array);
		WriteAppSettings(2, 108, 3, ref (new byte[3] { cmd, Speed, Brightness })[0]);
		byte[] array2 = new byte[4] { 1, R, G, B };
		int offset = 114;
		switch (cmd)
		{
		case 176:
			offset = 111;
			array2[0] = LED_ON;
			Static_R = R;
			Static_G = G;
			Static_B = B;
			break;
		case 178:
			offset = 115;
			BreathMode = 1;
			Breath_R = R;
			Breath_G = G;
			Breath_B = B;
			break;
		case 179:
			offset = 119;
			BlinkMode = 1;
			Blink_R = R;
			Blink_G = G;
			Blink_B = B;
			break;
		case 181:
			offset = 123;
			MeteorMode = 1;
			Meteor_R = R;
			Meteor_G = G;
			Meteor_B = B;
			break;
		}
		WriteAppSettings(2, offset, 4, ref array2[0]);
	}

	public void SetLED_OFF()
	{
		LED_ON = 0;
		byte[] data = new byte[62];
		SetLightBarData62(191, data);
		WriteAppSettings(2, 111, 1, ref (new byte[1] { LED_ON })[0]);
	}

	public void SetBrightness(byte level)
	{
		Brightness = level;
		byte[] array = new byte[62];
		array[0] = level;
		SetLightBarData62(191, array);
		byte[] array2 = new byte[1] { Brightness };
		WriteAppSettings(2, 110, 1, ref array2[0]);
		LED_ON = 1;
		array2[0] = LED_ON;
		WriteAppSettings(2, 111, 1, ref array2[0]);
	}

	public void SetSpeed(byte NewSpeed)
	{
		Speed = NewSpeed;
		byte[] array = new byte[62];
		if (Mode == 176)
		{
			array[0] = 0;
			array[1] = Speed;
			array[2] = Static_R;
			array[3] = Static_G;
			array[4] = Static_B;
		}
		else if (Mode == 178)
		{
			array[0] = BreathMode;
			array[1] = Speed;
			array[2] = Breath_R;
			array[3] = Breath_G;
			array[4] = Breath_B;
		}
		else if (Mode == 179)
		{
			array[0] = BlinkMode;
			array[1] = Speed;
			array[2] = Blink_R;
			array[3] = Blink_G;
			array[4] = Blink_B;
		}
		else if (Mode == 181)
		{
			array[0] = MeteorMode;
			array[1] = Speed;
			array[2] = Meteor_R;
			array[3] = Meteor_G;
			array[4] = Meteor_B;
		}
		else
		{
			array[0] = 0;
			array[1] = Speed;
			array[2] = 0;
			array[3] = 0;
			array[4] = 0;
		}
		SetLightBarData62(Mode, array);
		WriteAppSettings(2, 109, 1, ref (new byte[1] { Speed })[0]);
	}
}
