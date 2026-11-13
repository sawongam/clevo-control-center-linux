namespace GPUOverclocking;

// The original Global also carried keyboard-LED state (KB, kled_kb_array, kb_color) that this app only
// filled from the removed WM_COPYDATA IPC and never read; those members were dropped.
public class Global
{
	public static RWReg RW_REG;
	public static Support_BIT support_bit;
	public static InsydeDriver insyde;
	public static FAN fan;
	public static language MultiLang;
	public static RWIni RW_ini;
	public static int Flexikey_LED_Language;
}
