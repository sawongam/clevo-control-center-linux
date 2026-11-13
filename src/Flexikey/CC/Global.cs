using Avalonia.Media;

namespace CC;

public class Global
{
	public class KB
	{
		public Color left;

		public Color mid;

		public Color right;

		public Color TP;

		public Color LOGO;

		public Color LightBar;

		public int _iKbBrightness;

		public int _iKbStatus;

		public int _iKbMode;

		public int _iKbLeft;

		public int _iKbMid;

		public int _iKbRight;

		public int _iKbTp;

		public int _iVolume;

		public int _iKbLogo;

		public int _iKbAutoLightBar;

		public int _iKbSleepLEDTimer;
	}

	public static RWReg RW_REG;

	public static Support_BIT support_bit;

	public static Clevo_MSG clevo_msg;

	public static byte[] kled_kb_array = new byte[255];

	public static KB kb_color = new KB();

	public static language MultiLang;

	public static RWIni RW_ini;

	public static int Flexikey_LED_Language;

	public static bool FlexikeyInitialFlag = false;
}
