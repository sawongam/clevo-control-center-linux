using Avalonia.Media;
namespace CC;
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
	}
	public static RWReg RW_REG;
	public static Support_BIT support_bit;
	public static Clevo_MSG clevo_msg;
	public static InsydeDriver insyde;
	public static byte[] kled_kb_array = new byte[255];
	public static KB kb_color = new KB();
	public static language MultiLang;
	public static RWIni RW_ini;
	public static int Flexikey_LED_Language;
}
