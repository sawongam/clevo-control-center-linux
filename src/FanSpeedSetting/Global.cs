using Avalonia.Media;

namespace FanSpeedSetting;

public class Global
{
	public class KB
	{
		public Color all = default;

		public Color left = default;

		public Color mid = default;

		public Color right = default;

		public Color TP = default;

		public Color LOGO = default;

		public Color LightBar = default;

		public int _iKbBrightness = 0;

		public int _iKbStatus = 0;

		public int _iKbMode = 0;

		public int _iKbLeft = 0;

		public int _iKbMid = 0;

		public int _iKbRight = 0;

		public int _iKbTp = 0;

		public int _iVolume = 0;

		public int _iKbLogo = 0;

		public int _iKbAutoLightBar = 0;

		public int _iKbSleepLEDTimer = 0;

		public KB()
		{
			//IL_0006: Unknown result type (might be due to invalid IL or missing references)
			//IL_0012: Unknown result type (might be due to invalid IL or missing references)
			//IL_001e: Unknown result type (might be due to invalid IL or missing references)
			//IL_002a: Unknown result type (might be due to invalid IL or missing references)
			//IL_0036: Unknown result type (might be due to invalid IL or missing references)
			//IL_0042: Unknown result type (might be due to invalid IL or missing references)
			//IL_004e: Unknown result type (might be due to invalid IL or missing references)
		}
	}

	public class FanArgs
	{
		public byte T1;

		public byte T2;

		public byte T3;

		public byte T4;

		public byte T2_Default;

		public byte T3_Default;

		public byte D1;

		public byte D2;

		public byte D3;

		public byte D4;

		public byte D2_Default;

		public byte D3_Default;

		public int R12;

		public int R23;

		public int R34;

		public int rpm;

		public int local;

		public int remote;

		public byte duty;
	}

	public static RWReg RW_REG;

	public static Support_BIT support_bit;

	public static Clevo_MSG clevo_msg;

	public static InsydeDriver insyde;

	public static FAN fan;

	public static byte[] kled_kb_array = new byte[255];

	public static KB kb_color = new KB();

	public static language MultiLang;

	public static RWIni RW_ini;

	public static int Flexikey_LED_Language;
}
