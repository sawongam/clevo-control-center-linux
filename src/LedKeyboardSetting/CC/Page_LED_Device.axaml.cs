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
using RippleCC.Platform;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using CC.Device;
using CC.other_Class;

namespace CC;

public partial class Page_LED_Device : UserControl
{
	private struct KeyBoardLocation
	{
		public string Name { get; set; }

		public double LeftPoint { get; set; }

		public double TopPoint { get; set; }

		public double RightPoint { get; set; }

		public double BottomPoint { get; set; }
	}

	private int KBType;

	private int PerKeyType;

	private int PerKeyLang;

	private bool txt_color_isKeyboardFocus;

	private bool sleep_timer_init;

	private const int MIN_KB_SLEEP_TIME = 5;

	private const int DEFAULT_SLEEP_TIME = 30;

	private LightBar_X170 X170LB = new LightBar_X170();

	private Point start_point;

	private Point end_point;

	private Rectangle myRect = new Rectangle();

	private List<KeyBoardLocation> kbl = new List<KeyBoardLocation>();

	private List<KeyBoardLocation> kbl_last_time = new List<KeyBoardLocation>();

	private List<KeyBoardLocation> kbl_keep = new List<KeyBoardLocation>();

	private bool is_mouse_trigger;

	private string System_ManufacturerName = "Notebook";

	private bool WhiteLEDStatusEvent;

	private bool x170LightBar_InitStatus;







































































































































































































































































































































































































































	public static IntPtr InitPerkeyIo() => PerkeyApi.InitPerkeyIo();

	public static void SetLEDStatus(byte cmd, byte data0, byte data1, byte data2, byte data3) => PerkeyApi.SetLEDStatus(cmd, data0, data1, data2, data3);

	public static int GetLedData(byte cmd, byte type, byte[] arr, byte size) => PerkeyApi.GetLedData(cmd, type, arr, size);

	public Page_LED_Device()
	{
		InitializeComponent();
		WireEvents();
	}

	/// <summary>WPF raised main.Loaded after the Window.Loaded handler had created Global.KB; called from MainWindow.</summary>
	public void StartUp() => main_Loaded(this, new RoutedEventArgs());

	public void Open_UI()
	{
		main.IsVisible = true;
		Color val;
		if (KBType != 0)
		{
			UpdateBrightnessUI();
			UpdateSleepTimerUI();
			sleep_timer_init = true;
			if (KBType == 2)
			{
				InitRgbStackPanel();
			}
			if (KBType == 3)
			{
				UpdateSpeedUI();
				UpdateRGBText();
			}
			if (KBType == 6)
			{
				val = Global.KB.GetRGBLeftColor();
				string r = ((int)val.R).ToString();
				val = Global.KB.GetRGBLeftColor();
				string g = ((int)val.G).ToString();
				val = Global.KB.GetRGBLeftColor();
				SetRGBText(r, g, ((int)val.B).ToString());
				UpdateAllKBUI();
			}
			UpdateGridColor(Global.KB.GetBrightness());
		}
		if (Global.KB.SupportLogo())
		{
			Path showLogoColor = ShowLogoColor;
			val = Global.KB.GetRGBLogoColor();
			byte r2 = val.R;
			val = Global.KB.GetRGBLogoColor();
			byte g2 = val.G;
			val = Global.KB.GetRGBLogoColor();
			showLogoColor.Fill = new SolidColorBrush(Color.FromRgb(r2, g2, val.B));
		}
		if (Global.KB.SupportLightbar())
		{
			Rectangle showLightBar = ShowLightBar;
			val = Global.KB.GetRGBLightBarColor();
			byte r3 = val.R;
			val = Global.KB.GetRGBLightBarColor();
			byte g3 = val.G;
			val = Global.KB.GetRGBLightBarColor();
			showLightBar.Fill = new SolidColorBrush(Color.FromRgb(r3, g3, val.B));
		}
		if (Global.KB.SupportOverrideBootEffect())
		{
			if (Global.KB.OverrideBootEffect != 0)
			{
				ChkKBSelectBootEffect.IsChecked = true;
			}
			else
			{
				ChkKBSelectBootEffect.IsChecked = false;
			}
		}
	}

	public void Close_UI()
	{
		Global.KB.Closing();
		main.IsVisible = false;
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	public void ClosingUIEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			Global.KB.Closing();
			Environment.Exit(Environment.ExitCode);
		}));
	}

	public void UpdateToggleRGBEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			Global.KB.SetRGBAllColor(InsydeAcpiProt.ReadAcpiAllColor());
			UpdateAllKBUI();
		}));
	}

	public void Arg001_GetEventHandler(object sender, int Event, int E_Data)
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			_ = Event;
			_ = 93;
		}));
	}

	private void UpdateKBLayout(int value)
	{
		switch (value)
		{
		case 0:
			checkBox_key_20.Tag = string.Empty;
			checkBox_key_2b.Tag = string.Empty;
			checkBox_key_2d.Tag = string.Empty;
			checkBox_key_2e2f.IsVisible = true;
			checkBox_key_4c.Tag = string.Empty;
			checkBox_key_4d.Tag = string.Empty;
			checkBox_key_4e4f.IsVisible = true;
			checkBox_key_5373.IsVisible = true;
			checkBox_key_6061.Tag = "Caps Lock";
			checkBox_key_6b.Tag = string.Empty;
			checkBox_key_6c.Tag = "US";
			checkBox_key_6d6f.IsVisible = true;
			checkBox_key_8082.IsVisible = true;
			checkBox_key_8a.IsVisible = true;
			checkBox_key_8b.IsVisible = true;
			checkBox_key_8c.IsVisible = true;
			checkBox_key_8d8e.IsVisible = true;
			checkBox_key_8d8e.Tag = "US";
			checkBox_key_a5a9.IsVisible = true;
			checkBox_key_aa.Tag = "Alt";
			checkBox_key_ab.IsVisible = true;
			checkBox_key_ab.Tag = null;
			checkBox_key_53.IsVisible = false;
			checkBox_key_73.IsVisible = false;
			checkBox_key_6e.IsVisible = false;
			checkBox_key_6f.IsVisible = false;
			checkBox_key_82.IsVisible = false;
			checkBox_key_ab1.IsVisible = false;
			checkBox_key_a5.IsVisible = false;
			checkBox_key_80.IsVisible = false;
			checkBox_key_8a_BR.IsVisible = false;
			if (Global.KB.IsCustom3ColKB())
			{
				checkBox_key_10_JP3C.IsVisible = false;
				checkBox_key_11_JP3C.IsVisible = false;
				checkBox_key_12_JP3C.IsVisible = false;
				checkBox_key_30_JP3C.IsVisible = false;
				checkBox_key_31_JP3C.IsVisible = false;
				checkBox_key_32_JP3C.IsVisible = false;
				checkBox_key_b1_JP3C.IsVisible = false;
				checkBox_key_b2_JP3C.IsVisible = false;
				checkBox_key_b3_JP3C.IsVisible = false;
				checkBox_key_8f_JP3C.IsVisible = false;
				checkBox_key_ae_JP3C.IsVisible = false;
				checkBox_key_af_JP3C.IsVisible = false;
				checkBox_key_b0_JP3C.IsVisible = false;
				checkBox_key_8d8e_JP3C.IsVisible = false;
				checkBox_key_8f.IsVisible = true;
				checkBox_key_ae.IsVisible = true;
				checkBox_key_af.IsVisible = true;
				checkBox_key_b0.IsVisible = true;
				checkBox_key_b1.IsVisible = true;
				checkBox_key_b2.IsVisible = true;
				checkBox_key_10.IsVisible = true;
				checkBox_key_11.IsVisible = true;
				checkBox_key_12.IsVisible = true;
				checkBox_key_13.IsVisible = true;
				checkBox_key_30.IsVisible = true;
				checkBox_key_31.IsVisible = true;
				checkBox_key_32.IsVisible = true;
				checkBox_key_33.IsVisible = true;
				checkBox_key_93b3.IsVisible = true;
				grid_KB_ALL.Width = 1380.0;
				RB_KB_Partial_right.Width = 460.0;
			}
			else if (InsydeAcpiProt.Read_PanelSize() < 15.0)
			{
				checkBox_key_8f.IsVisible = false;
				checkBox_key_ab.IsVisible = false;
				checkBox_key_ae.IsVisible = false;
				checkBox_key_af.IsVisible = false;
				checkBox_key_b0.IsVisible = false;
				checkBox_key_acad.IsVisible = false;
				checkBox_key_10.IsVisible = false;
				checkBox_key_11.IsVisible = false;
				checkBox_key_12.IsVisible = false;
				checkBox_key_13.IsVisible = false;
				checkBox_key_30.IsVisible = false;
				checkBox_key_31.IsVisible = false;
				checkBox_key_32.IsVisible = false;
				checkBox_key_33.IsVisible = false;
				checkBox_key_50.IsVisible = false;
				checkBox_key_51.IsVisible = false;
				checkBox_key_52.IsVisible = false;
				checkBox_key_5373.IsVisible = false;
				checkBox_key_70.IsVisible = false;
				checkBox_key_71.IsVisible = false;
				checkBox_key_72.IsVisible = false;
				checkBox_key_90.IsVisible = false;
				checkBox_key_91.IsVisible = false;
				checkBox_key_92.IsVisible = false;
				checkBox_key_93b3.IsVisible = false;
				checkBox_key_b1.IsVisible = false;
				checkBox_key_b2.IsVisible = false;
				checkBox_key_8d8e.IsVisible = false;
				checkBox_key_d.IsVisible = false;
				checkBox_key_e.IsVisible = false;
				checkBox_key_f.IsVisible = false;
				checkBox_key_10_JP3C.IsVisible = false;
				checkBox_key_11_JP3C.IsVisible = false;
				checkBox_key_12_JP3C.IsVisible = false;
				checkBox_key_30_JP3C.IsVisible = false;
				checkBox_key_31_JP3C.IsVisible = false;
				checkBox_key_32_JP3C.IsVisible = false;
				checkBox_key_b1_JP3C.IsVisible = false;
				checkBox_key_b2_JP3C.IsVisible = false;
				checkBox_key_b3_JP3C.IsVisible = false;
				checkBox_key_8f_JP3C.IsVisible = false;
				checkBox_key_ae_JP3C.IsVisible = false;
				checkBox_key_af_JP3C.IsVisible = false;
				checkBox_key_b0_JP3C.IsVisible = false;
				checkBox_key_8d8e_JP3C.IsVisible = false;
				checkBox_key_8f_14.IsVisible = true;
				checkBox_key_ae_14.IsVisible = true;
				checkBox_key_af_14.IsVisible = true;
				checkBox_key_b0_14.IsVisible = true;
				checkBox_key_acad_14.IsVisible = true;
				checkBox_key_12_14.IsVisible = true;
				checkBox_key_13_14.IsVisible = true;
				checkBox_key_8d8e_14.IsVisible = true;
				checkBox_key_d_14.IsVisible = true;
				checkBox_key_e_14.IsVisible = true;
				checkBox_key_f_14.IsVisible = true;
				checkBox_key_12_1_14.IsVisible = true;
				grid_KB_ALL.Width = 1087.0;
			}
			else
			{
				checkBox_key_32_JP3C.IsVisible = false;
				checkBox_key_32.IsVisible = true;
			}
			break;
		case 1:
			checkBox_key_20.Tag = string.Empty;
			checkBox_key_2b.Tag = string.Empty;
			checkBox_key_2d.Tag = string.Empty;
			checkBox_key_2e2f.IsVisible = true;
			checkBox_key_4c.Tag = string.Empty;
			checkBox_key_4d.Tag = string.Empty;
			checkBox_key_5373.IsVisible = true;
			checkBox_key_6061.Tag = "Caps Lock";
			checkBox_key_6b.Tag = string.Empty;
			checkBox_key_6c.Tag = "UK";
			checkBox_key_6e.IsVisible = true;
			checkBox_key_6e.Tag = "UK";
			checkBox_key_6f.IsVisible = true;
			checkBox_key_80.IsVisible = true;
			checkBox_key_82.IsVisible = true;
			checkBox_key_8a.IsVisible = true;
			checkBox_key_8b.IsVisible = true;
			checkBox_key_8c.IsVisible = true;
			checkBox_key_8d8e.IsVisible = true;
			checkBox_key_8d8e.Tag = "UK";
			checkBox_key_a5a9.IsVisible = true;
			checkBox_key_aa.Tag = "Alt Gr";
			checkBox_key_ab.Tag = null;
			checkBox_key_4e4f.IsVisible = false;
			checkBox_key_53.IsVisible = false;
			checkBox_key_73.IsVisible = false;
			checkBox_key_6d6f.IsVisible = false;
			checkBox_key_a5.IsVisible = false;
			checkBox_key_8082.IsVisible = false;
			checkBox_key_8a_BR.IsVisible = false;
			if (Global.KB.IsCustom3ColKB())
			{
				checkBox_key_10_JP3C.IsVisible = false;
				checkBox_key_11_JP3C.IsVisible = false;
				checkBox_key_12_JP3C.IsVisible = false;
				checkBox_key_30_JP3C.IsVisible = false;
				checkBox_key_31_JP3C.IsVisible = false;
				checkBox_key_32_JP3C.IsVisible = false;
				checkBox_key_b1_JP3C.IsVisible = false;
				checkBox_key_b2_JP3C.IsVisible = false;
				checkBox_key_b3_JP3C.IsVisible = false;
				checkBox_key_8f_JP3C.IsVisible = false;
				checkBox_key_ae_JP3C.IsVisible = false;
				checkBox_key_af_JP3C.IsVisible = false;
				checkBox_key_b0_JP3C.IsVisible = false;
				checkBox_key_8d8e_JP3C.IsVisible = false;
				checkBox_key_8f.IsVisible = true;
				checkBox_key_ae.IsVisible = true;
				checkBox_key_af.IsVisible = true;
				checkBox_key_b0.IsVisible = true;
				checkBox_key_b1.IsVisible = true;
				checkBox_key_b2.IsVisible = true;
				checkBox_key_10.IsVisible = true;
				checkBox_key_11.IsVisible = true;
				checkBox_key_12.IsVisible = true;
				checkBox_key_13.IsVisible = true;
				checkBox_key_30.IsVisible = true;
				checkBox_key_31.IsVisible = true;
				checkBox_key_32.IsVisible = true;
				checkBox_key_33.IsVisible = true;
				checkBox_key_93b3.IsVisible = true;
				grid_KB_ALL.Width = 1380.0;
				RB_KB_Partial_right.Width = 460.0;
			}
			if (InsydeAcpiProt.Read_PanelSize() < 15.0)
			{
				UpdateKBLayout(0);
				break;
			}
			checkBox_key_32_JP3C.IsVisible = false;
			checkBox_key_32.IsVisible = true;
			break;
		case 2:
			checkBox_key_20.Tag = "JB";
			checkBox_key_2b.Tag = "JB";
			checkBox_key_2d.Tag = "JB";
			checkBox_key_2e.Tag = "JB";
			checkBox_key_4c.Tag = "JB";
			checkBox_key_4d.Tag = "JB";
			checkBox_key_5373.IsVisible = true;
			checkBox_key_6061.Tag = "Caps Lock";
			checkBox_key_6b.Tag = "JB";
			checkBox_key_6c.Tag = "JB";
			checkBox_key_6e.IsVisible = true;
			checkBox_key_6e.Tag = "JB";
			checkBox_key_6e.Content = "6e";
			checkBox_key_6f.IsVisible = true;
			checkBox_key_8082.IsVisible = true;
			checkBox_key_8a.IsVisible = true;
			checkBox_key_8b.IsVisible = true;
			checkBox_key_8c.IsVisible = true;
			checkBox_key_8d_JP.Tag = "JB";
			checkBox_key_a5.IsVisible = true;
			checkBox_key_aa.Tag = "JB";
			checkBox_key_ab.Tag = "Alt";
			checkBox_key_ac.Tag = "JB";
			checkBox_key_2e2f.IsVisible = false;
			checkBox_key_4e4f.IsVisible = false;
			checkBox_key_53.IsVisible = false;
			checkBox_key_73.IsVisible = false;
			checkBox_key_6d6f.IsVisible = false;
			checkBox_key_80.IsVisible = false;
			checkBox_key_82.IsVisible = false;
			checkBox_key_8d8e.IsVisible = false;
			checkBox_key_a5a9.IsVisible = false;
			checkBox_key_8a_BR.IsVisible = false;
			if (Global.KB.IsCustom3ColKB())
			{
				checkBox_key_8e.IsVisible = false;
				checkBox_key_8f.IsVisible = false;
				checkBox_key_ae.IsVisible = false;
				checkBox_key_af.IsVisible = false;
				checkBox_key_b0.IsVisible = false;
				checkBox_key_b1.IsVisible = false;
				checkBox_key_b2.IsVisible = false;
				checkBox_key_10.IsVisible = false;
				checkBox_key_11.IsVisible = false;
				checkBox_key_12.IsVisible = false;
				checkBox_key_13.IsVisible = false;
				checkBox_key_30.IsVisible = false;
				checkBox_key_31.IsVisible = false;
				checkBox_key_32.IsVisible = false;
				checkBox_key_33.IsVisible = false;
				checkBox_key_5373.IsVisible = false;
				checkBox_key_93b3.IsVisible = false;
				checkBox_key_10_JP3C.IsVisible = true;
				checkBox_key_11_JP3C.IsVisible = true;
				checkBox_key_12_JP3C.IsVisible = true;
				checkBox_key_30_JP3C.IsVisible = true;
				checkBox_key_31_JP3C.IsVisible = true;
				checkBox_key_32_JP3C.IsVisible = true;
				checkBox_key_b1_JP3C.IsVisible = true;
				checkBox_key_b2_JP3C.IsVisible = true;
				checkBox_key_b3_JP3C.IsVisible = true;
				checkBox_key_8f_JP3C.IsVisible = true;
				checkBox_key_ae_JP3C.IsVisible = true;
				checkBox_key_af_JP3C.IsVisible = true;
				checkBox_key_b0_JP3C.IsVisible = true;
				checkBox_key_8d8e_JP3C.IsVisible = true;
				grid_KB_ALL.Width = 1320.0;
				RB_KB_Partial_right.Width = 400.0;
			}
			if (InsydeAcpiProt.Read_PanelSize() < 15.0)
			{
				UpdateKBLayout(0);
				break;
			}
			checkBox_key_32_JP3C.IsVisible = false;
			checkBox_key_32.IsVisible = true;
			break;
		case 3:
			checkBox_key_20.Tag = string.Empty;
			checkBox_key_2b.Tag = string.Empty;
			checkBox_key_2d.Tag = string.Empty;
			checkBox_key_2e2f.IsVisible = true;
			checkBox_key_4c.Tag = "BR";
			checkBox_key_4d.Tag = "BR";
			checkBox_key_6061.Tag = "BR";
			checkBox_key_6b.Tag = "BR";
			checkBox_key_6c.Tag = "BR";
			checkBox_key_6e.IsVisible = true;
			checkBox_key_6e.Tag = "BR";
			checkBox_key_6f.IsVisible = true;
			checkBox_key_80.IsVisible = true;
			checkBox_key_82.IsVisible = true;
			checkBox_key_8a_BR.IsVisible = true;
			checkBox_key_8d_BR.Tag = "BR";
			checkBox_key_a5a9.IsVisible = true;
			checkBox_key_aa.Tag = "Alt Gr";
			checkBox_key_ab.Tag = null;
			checkBox_key_53.IsVisible = true;
			checkBox_key_73.IsVisible = true;
			checkBox_key_4e4f.IsVisible = false;
			checkBox_key_6d6f.IsVisible = false;
			checkBox_key_8082.IsVisible = false;
			checkBox_key_8a.IsVisible = false;
			checkBox_key_8b.IsVisible = false;
			checkBox_key_8c.IsVisible = false;
			checkBox_key_8d8e.IsVisible = false;
			checkBox_key_8d8e.Tag = "UK";
			checkBox_key_a5.IsVisible = false;
			checkBox_key_5373.IsVisible = false;
			if (Global.KB.IsCustom3ColKB())
			{
				checkBox_key_10_JP3C.IsVisible = false;
				checkBox_key_11_JP3C.IsVisible = false;
				checkBox_key_12_JP3C.IsVisible = false;
				checkBox_key_30_JP3C.IsVisible = false;
				checkBox_key_31_JP3C.IsVisible = false;
				checkBox_key_32_JP3C.IsVisible = false;
				checkBox_key_b1_JP3C.IsVisible = false;
				checkBox_key_b2_JP3C.IsVisible = false;
				checkBox_key_b3_JP3C.IsVisible = false;
				checkBox_key_8f_JP3C.IsVisible = false;
				checkBox_key_ae_JP3C.IsVisible = false;
				checkBox_key_af_JP3C.IsVisible = false;
				checkBox_key_b0_JP3C.IsVisible = false;
				checkBox_key_8d8e_JP3C.IsVisible = false;
				checkBox_key_8f.IsVisible = true;
				checkBox_key_ae.IsVisible = true;
				checkBox_key_af.IsVisible = true;
				checkBox_key_b0.IsVisible = true;
				checkBox_key_b1.IsVisible = true;
				checkBox_key_b2.IsVisible = true;
				checkBox_key_10.IsVisible = true;
				checkBox_key_11.IsVisible = true;
				checkBox_key_12.IsVisible = true;
				checkBox_key_13.IsVisible = true;
				checkBox_key_30.IsVisible = true;
				checkBox_key_31.IsVisible = true;
				checkBox_key_32.IsVisible = true;
				checkBox_key_33.IsVisible = true;
				checkBox_key_93b3.IsVisible = true;
				grid_KB_ALL.Width = 1380.0;
				RB_KB_Partial_right.Width = 460.0;
			}
			if (InsydeAcpiProt.Read_PanelSize() < 15.0)
			{
				UpdateKBLayout(0);
				break;
			}
			checkBox_key_32_JP3C.IsVisible = false;
			checkBox_key_32.IsVisible = true;
			break;
		}
	}

	public void Init_UI()
	{
		KBType = Global.KB.GetKBType();
		int flexikey_LED_Language = Global.Flexikey_LED_Language;
		switch (KBType)
		{
		case 0:
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_PerKey);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_RGB);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_White);
			RB_LEDMode_Perkey_Effect.IsChecked = false;
			break;
		case 1:
		{
			Show_UI_KB_ALL(show: true);
			Show_UI_Perkey(0);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_PerKey);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_RGB);
			grid_LED_Speed.IsVisible = false;
			Canvas_Perkey_Effect.IsVisible = false;
			grid_color.IsVisible = false;
			RB_LEDMode_Perkey_Effect.IsChecked = false;
			RB_LEDMode_White.IsChecked = true;
			if (!Global.KB.SupportWhiteKB6Level())
			{
				InitBrightnessUI(2);
			}
			if (Global.RW_REG.IsFirstRun.Equals(1) || Global.KB.GetCurrentAppVersion().Equals("0.0.0.0"))
			{
				Global.KB.FirstRun();
				Global.RW_REG.IsFirstRun = 0;
			}
			Global.KB.Init();
			UpdateKBLayout(flexikey_LED_Language);
			Color val = Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
			grid_KB_ALL.Background = new SolidColorBrush(val);
			break;
		}
		case 2:
			Show_UI_Perkey(0);
			Show_UI_KB_ALL(show: true);
			RB_LEDMode_KB_ALL.IsChecked = true;
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_PerKey);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_White);
			InitBrightnessUI(4);
			UpdateKBLayout(flexikey_LED_Language);
			if (Global.RW_REG.IsFirstRun.Equals(1) || Global.KB.GetCurrentAppVersion().Equals("0.0.0.0"))
			{
				Global.KB.FirstRun();
				Global.RW_REG.IsFirstRun = 0;
			}
			Global.KB.Init();
			break;
		case 3:
			Show_UI_KB_ALL(show: false);
			Show_UI_KB_Partial(show: false);
			Show_UI_KB_Effect(show: false);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_RGB);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_White);
			InitBrightnessUI(4);
			InitPerkeyIo();
			GetPerkeyLayoutInfo(ref PerKeyType, ref PerKeyLang);
			UpdateKBLayout(PerKeyLang);
			if (Global.RW_REG.IsFirstRun.Equals(1) || Global.KB.GetCurrentAppVersion().Equals("0.0.0.0"))
			{
				Global.KB.FirstRun();
				Global.RW_REG.IsFirstRun = 0;
				if (Global.KB.SupportPerkeyCustomStatic())
				{
					PerkeyFirstRunStaticColor(Global.KB.PerkeyCustomDefaultStaticColor());
				}
			}
			Global.KB.Init();
			if (Global.KB.SupportPerkeyNewEffect())
			{
				canvas_perkey_Wave.IsVisible = false;
				canvas_perkey_Scan.IsVisible = false;
				canvas_perkey_Random.IsVisible = false;
				canvas_perkey_Snake.IsVisible = false;
			}
			else
			{
				canvas_perkey_Wave_V1p19.IsVisible = false;
				canvas_perkey_Scan_V1p19.IsVisible = false;
				canvas_perkey_Random_V1p19.IsVisible = false;
				canvas_perkey_Snake_V1p19.IsVisible = false;
			}
			if (Global.KB.StaticStatus.Equals(1))
			{
				Show_UI_Perkey(1);
			}
			else
			{
				Show_UI_Perkey(2);
			}
			break;
		case 6:
			Show_UI_KB_ALL(show: true);
			Show_UI_Perkey(0);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_PerKey);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_RGB);
			grid_LED_Speed.IsVisible = false;
			Canvas_Perkey_Effect.IsVisible = false;
			BTN_color_picker.IsEnabled = false;
			RB_LEDMode_White.IsChecked = true;
			InitBrightnessUI(4);
			grid_color_picker.IsVisible = false;
			grud_10_color_picker.IsVisible = false;
			grid_color_picker_Whole.IsVisible = true;
			canvas_color__picker_text.IsVisible = false;
			ModeRectTop.IsVisible = false;
			ModeRect.IsVisible = false;
			ModeRectBottom.IsVisible = false;
			UpdateKBLayout(flexikey_LED_Language);
			RB_LEDMode_Perkey_Effect.IsChecked = false;
			RB_LEDMode_White.IsChecked = true;
			if (Global.RW_REG.IsFirstRun.Equals(1) || Global.KB.GetCurrentAppVersion().Equals("0.0.0.0"))
			{
				Global.KB.FirstRun();
				Global.RW_REG.IsFirstRun = 0;
			}
			Global.KB.Init();
			Init_UI_RGB15Color();
			break;
		case 22:
			Show_UI_KB_ALL(show: true);
			Show_UI_Perkey(0);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_PerKey);
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Keyboard_RGB);
			grid_LED_Speed.IsVisible = false;
			Canvas_Perkey_Effect.IsVisible = false;
			grid_color.IsVisible = false;
			RB_LEDMode_White.IsChecked = true;
			InitBrightnessUI(4);
			UpdateKBLayout(flexikey_LED_Language);
			if (Global.RW_REG.IsFirstRun.Equals(1) || Global.KB.GetCurrentAppVersion().Equals("0.0.0.0"))
			{
				Global.KB.FirstRun();
				Global.RW_REG.IsFirstRun = 0;
			}
			Global.KB.Init();
			break;
		}
		Color val2 = Global.KB.CustomMainSkin();
		if (val2.R != 0 || val2.G != 0 || val2.B != 0)
		{
			userControl.BorderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, val2.R, val2.G, val2.B));
		}
		Color[] color = new Color[10];
		Global.KB.GetPalette(ref color);
		for (int i = 0; i < 10; i++)
		{
			if (color[i].R != 0 || color[i].G != 0 || color[i].B != 0)
			{
				Brush borderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, color[i].R, color[i].G, color[i].B));
				if (i.Equals(0))
				{
					BTN_color_plate_pink.BorderBrush = borderBrush;
				}
				else if (i.Equals(1))
				{
					BTN_color_plate_purple.BorderBrush = borderBrush;
				}
				else if (i.Equals(2))
				{
					BTN_color_plate_bule.BorderBrush = borderBrush;
				}
				else if (i.Equals(3))
				{
					BTN_color_plate_sky_blue.BorderBrush = borderBrush;
				}
				else if (i.Equals(4))
				{
					BTN_color_plate_green.BorderBrush = borderBrush;
				}
				else if (i.Equals(5))
				{
					BTN_color_plate_yellow.BorderBrush = borderBrush;
				}
				else if (i.Equals(6))
				{
					BTN_color_plate_orange.BorderBrush = borderBrush;
				}
				else if (i.Equals(7))
				{
					BTN_color_plate_red.BorderBrush = borderBrush;
				}
				else if (i.Equals(8))
				{
					BTN_color_plate_white.BorderBrush = borderBrush;
				}
				else if (i.Equals(9))
				{
					BTN_color_plate_black.BorderBrush = borderBrush;
				}
			}
		}
		if (Global.KB.SupportLogo())
		{
			SP_RB_Around_Device.IsVisible = true;
		}
		else
		{
			SP_RB_Around_Device.Children.Remove((Control)(object)RB_LEDMode_LOGO);
		}
		if (Global.KB.SupportLightbar())
		{
			SP_RB_Around_Device.IsVisible = true;
		}
		else
		{
			SP_RB_Around_Device.Children.Remove((Control)(object)RB_LEDMode_LightBar);
		}
		if (!Global.KB.SupportLogo() && !Global.KB.SupportLightbar())
		{
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_Around_Device);
		}
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsVisible = true;
		}
		else
		{
			ChkKBSelectBootEffect.IsVisible = false;
		}
		List<int> list = new List<int>();
		for (int j = 0; j < 256; j++)
		{
			list.Add(j);
		}
		((ItemsControl)txt_color_red).ItemsSource = list;
		((ItemsControl)txt_color_green).ItemsSource = list;
		((ItemsControl)txt_color_blue).ItemsSource = list;
		if (KBType == 3)
		{
			kbl = GetAllKeyLocation();
		}
		List<int> list2 = new List<int>();
		for (int k = 0; k < 60; k++)
		{
			list2.Add(k);
		}
		((ItemsControl)SleepTimerSecCustom).ItemsSource = list2;
		((ItemsControl)SleepTimerMinCustom).ItemsSource = list2;
		list2 = new List<int>();
		for (int l = 0; l < 2; l++)
		{
			list2.Add(l);
		}
		((ItemsControl)SleepTimerHourCustom).ItemsSource = list2;
		if (KBType != 0)
		{
			LoadKBSleepTimer();
		}
		LoadMultiLang();
		KBModule kB = Global.KB;
		kB.UpdateBrightness = (EventHandler)Delegate.Combine(kB.UpdateBrightness, new EventHandler(UpdateBrightnessEvent));
		KBModule kB2 = Global.KB;
		kB2.UpdateUILanguage = (EventHandler)Delegate.Combine(kB2.UpdateUILanguage, new EventHandler(UpdateUILanguageEvent));
		KBModule kB3 = Global.KB;
		kB3.CloseUI = (EventHandler)Delegate.Combine(kB3.CloseUI, new EventHandler(ClosingUIEvent));
		KBModule kB4 = Global.KB;
		kB4.UpdateToggleRGB = (EventHandler)Delegate.Combine(kB4.UpdateToggleRGB, new EventHandler(UpdateToggleRGBEvent));
		Global.KB.Arg001_GetEvent += Arg001_GetEventHandler;
		KBModule kB5 = Global.KB;
		kB5.UpdateWhiteLEDStatus = (EventHandler)Delegate.Combine(kB5.UpdateWhiteLEDStatus, new EventHandler(UpdateWhiteLEDStatusEvent));
		if (Global.KB.SupportLightbar_x170())
		{
			if (X170LB.Init())
			{
				x170LightBar_InitStatus = true;
			}
			else
			{
				SP_LED_Mode.Children.Remove((Control)(object)SP_RB_X170_Device);
			}
		}
		else
		{
			SP_LED_Mode.Children.Remove((Control)(object)SP_RB_X170_Device);
		}
	}

	private void Init_UI_RGB15Color()
	{
		string val = null;
		int num = 0;
		if (Global.KB.CustomID() == 0)
		{
			if (System_ManufacturerName.Contains("MONSTER"))
			{
				num = 6;
			}
		}
		else
		{
			num = Global.KB.CustomID();
		}
		switch (num)
		{
		case 1:
			val = "Image/Page_LEDdevice/Color_Picker/15color_plate_custom1.png";
			break;
		case 5:
			val = "Image/Page_LEDdevice/Color_Picker/15color_plate_custom5.png";
			break;
		case 6:
			val = "Image/Page_LEDdevice/Color_Picker/16color_plate_layer4.png";
			break;
		default:
			val = "Image/Page_LEDdevice/Color_Picker/15color_plate_layer4.png";
			break;
		}
		image_color_picker_whole.Source = Img.Load(val);
	}

	private void InitBrightnessUI(int level)
	{
		if (level.Equals(2))
		{
			((RangeBase)KbBrightnessSlider).Maximum = level;
			brightness_5_tb.IsVisible = false;
			brightness_4_tb.IsVisible = false;
			brightness_3_tb.IsVisible = false;
			brightness_2_tb.Margin = new Thickness(0.0, 29.75, 10.0, 0.0);
			brightness_1_tb.Margin = new Thickness(0.0, 187.0, 10.0, 0.0);
		}
		else if (level.Equals(4))
		{
			((RangeBase)KbBrightnessSlider).Maximum = level;
			brightness_5_tb.IsVisible = false;
			brightness_4_tb.Margin = new Thickness(0.0, 29.75, 10.0, 0.0);
			brightness_3_tb.Margin = new Thickness(0.0, 108.5, 10.0, 0.0);
			brightness_2_tb.Margin = new Thickness(0.0, 187.0, 10.0, 0.0);
			brightness_1_tb.Margin = new Thickness(0.0, 265.0, 10.0, 0.0);
		}
	}

	private void GetPerkeyLayoutInfo(ref int _perkey_type, ref int _perkey_language)
	{
		byte[] array = new byte[1];
		GetLedData(17, 1, array, Convert.ToByte(array.Length));
		_perkey_language = array[0] & 0xF;
		_perkey_type = (array[0] >> 4) & 0xF;
	}

	private List<KeyBoardLocation> GetAllKeyLocation()
	{
		List<KeyBoardLocation> list = new List<KeyBoardLocation>();
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val = child;
			if (val.IsVisible)
			{
				Thickness margin = canvas_KB_layout.Margin;
				double left = margin.Left;
				margin = canvas_KB_layout.Margin;
				double top = margin.Top;
				object value = (object)Canvas.GetLeft(val);
				object value2 = (object)Canvas.GetTop(val);
				KeyBoardLocation item = new KeyBoardLocation
				{
					Name = val.Name,
					LeftPoint = (double)value + left,
					TopPoint = (double)value2 + top,
					RightPoint = (double)value + left + val.Width,
					BottomPoint = (double)value2 + top + val.Height
				};
				list.Add(item);
			}
		}
		return list;
	}

	private void LoadMultiLang()
	{
		T_LedMode.Text = Global.MultiLang.Translation("_LEDMode");
		RB_LEDMode_Perkey_static.Content = Global.MultiLang.Translation("_Keyboard");
		RB_LEDMode_Perkey_Effect.Content = Global.MultiLang.Translation("_KeyboardEffect");
		RB_LEDMode_KB_ALL.Content = Global.MultiLang.Translation("_KeyboardAll");
		RB_LEDMode_KB_Partial.Content = Global.MultiLang.Translation("_KeyboardPartial");
		RB_LEDMode_KB_Effect.Content = RB_LEDMode_Perkey_Effect.Content;
		RB_LEDMode_White.Content = RB_LEDMode_Perkey_static.Content;
		RB_LEDMode_LOGO.Content = Global.MultiLang.Translation("_LOGOLed");
		RB_LEDMode_LightBar.Content = Global.MultiLang.Translation("_LightBar");
		RB_LEDMode_LightBarX170.Content = Global.MultiLang.Translation("_LightBar");
		label_kb_LedSpeed.Content = Global.MultiLang.Translation("_LEDSpeed");
		label_kb_brightness.Content = Global.MultiLang.Translation("_Brightness");
		SleepTimerHourCustom.Tag = Global.MultiLang.Translation("_H");
		SleepTimerMinCustom.Tag = Global.MultiLang.Translation("_M");
		SleepTimerSecCustom.Tag = Global.MultiLang.Translation("_S");
		label_kb_sleep_timer.Content = Global.MultiLang.Translation("_KeyboardSleepTimer");
		RB_KBSleepTimer_OFF.Content = Global.MultiLang.Translation("_DISABLE");
		RB_KBSleepTimer_ON.Content = Global.MultiLang.Translation("_Enable");
		T_Blue.Text = Global.MultiLang.Translation("_Blue") + ":";
		T_Green.Text = Global.MultiLang.Translation("_Green") + ":";
		T_Read.Text = Global.MultiLang.Translation("_Red") + ":";
		BTN_Restore_PerKey_static.Content = Global.MultiLang.Translation("_Restore");
		ToolTip.SetTip(RB_PerKey_Effect_wave, Global.MultiLang.Translation("_WaveMode"));
		ToolTip.SetTip(RB_PerKey_Effect_Breath, Global.MultiLang.Translation("_BreatheMode"));
		ToolTip.SetTip(RB_PerKey_Effect_Scan, Global.MultiLang.Translation("_ScanMode"));
		ToolTip.SetTip(RB_PerKey_Effect_Blink, Global.MultiLang.Translation("_BlinkMode"));
		ToolTip.SetTip(RB_PerKey_Effect_Random, Global.MultiLang.Translation("_RandomMode"));
		ToolTip.SetTip(RB_PerKey_Effect_ripple, Global.MultiLang.Translation("_RippleMode"));
		ToolTip.SetTip(RB_PerKey_Effect_Snake, Global.MultiLang.Translation("_SnakeMode"));
		textBlock_WSS.Text = ToolTip.GetTip(RB_PerKey_Effect_wave).ToString();
		textBlock_WSS1.Text = ToolTip.GetTip(RB_PerKey_Effect_Scan).ToString();
		textBlock_WSS3.Text = ToolTip.GetTip(RB_PerKey_Effect_Random).ToString();
		textBlock_WSS2.Text = ToolTip.GetTip(RB_PerKey_Effect_Snake).ToString();
		RB_Perkey_Breath_random.Content = Global.MultiLang.Translation("_Random");
		RB_Perkey_Breath_custom.Content = Global.MultiLang.Translation("_Custom");
		RB_Perkey_Scan_Random.Content = RB_Perkey_Breath_random.Content;
		RB_Perkey_Scan_Custom.Content = RB_Perkey_Breath_custom.Content;
		RB_Perkey_Scan_Color1.Content = "Color1";
		RB_Perkey_Scan_Color2.Content = "Color2";
		RB_Perkey_Blink_random.Content = RB_Perkey_Breath_random.Content;
		RB_Perkey_Blink_custom.Content = RB_Perkey_Breath_custom.Content;
		RB_Perkey_Random_random.Content = RB_Perkey_Breath_random.Content;
		RB_Perkey_Random_custom.Content = RB_Perkey_Breath_custom.Content;
		RB_Perkey_Ripple_random.Content = RB_Perkey_Breath_random.Content;
		RB_Perkey_Ripple_custom.Content = RB_Perkey_Breath_custom.Content;
		RB_Perkey_Ripple_Ripple.Content = Global.MultiLang.Translation("_Ripple");
		RB_Perkey_Ripple_Reactive.Content = Global.MultiLang.Translation("_Reactive");
		RB_Wave_Default.Content = Global.MultiLang.Translation("_Default");
		RB_Wave_Custom.Content = RB_Perkey_Breath_custom.Content;
		RB_Snake_Random.Content = RB_Perkey_Breath_random.Content;
		RB_Snake_Custom.Content = RB_Perkey_Breath_custom.Content;
		ToolTip.SetTip(RB_KB_Effect_wave, Global.MultiLang.Translation("_WaveMode"));
		ToolTip.SetTip(RB_KB_Effect_Breath, Global.MultiLang.Translation("_BreatheMode"));
		ToolTip.SetTip(RB_KB_Effect_cycle, Global.MultiLang.Translation("_CycleMode"));
		ToolTip.SetTip(RB_KB_Effect_random, Global.MultiLang.Translation("_RandomMode"));
		ToolTip.SetTip(RB_KB_Effect_Flash, Global.MultiLang.Translation("_FlashMode"));
		ToolTip.SetTip(RB_KB_Effect_Dance, Global.MultiLang.Translation("_DanceMode"));
		ToolTip.SetTip(RB_KB_Effect_Tempo, Global.MultiLang.Translation("_TempoMode"));
		RB_KB_Partial_left.Content = Global.MultiLang.Translation("_Left");
		RB_KB_Partial_Mid.Content = Global.MultiLang.Translation("_Mid");
		RB_KB_Partial_right.Content = Global.MultiLang.Translation("_Right");
		ChkKBSelectBootEffect.Content = Global.MultiLang.Translation("_SelectBootEffect");
		ToolTip.SetTip(RB_x170_Effect_Static, Global.MultiLang.Translation("_Static"));
		ToolTip.SetTip(RB_x170_Effect_Breath, Global.MultiLang.Translation("_BreatheMode"));
		ToolTip.SetTip(RB_x170_Effect_Blink, Global.MultiLang.Translation("_BlinkMode"));
		ToolTip.SetTip(RB_x170_Effect_ColorCycle, Global.MultiLang.Translation("_CycleMode"));
		ToolTip.SetTip(RB_x170_Effect_Meteor, Global.MultiLang.Translation("_Meteor"));
		ToolTip.SetTip(RB_x170_Effect_wave, Global.MultiLang.Translation("_WaveMode"));
		ToolTip.SetTip(RB_x170_Effect_ripple, Global.MultiLang.Translation("_RippleMode"));
		ToolTip.SetTip(RB_x170_Effect_Random, Global.MultiLang.Translation("_RandomMode"));
		T_Effect_name_Static.Text = ToolTip.GetTip(RB_x170_Effect_Static).ToString();
		T_Effect_name_ColorCycle.Text = ToolTip.GetTip(RB_x170_Effect_ColorCycle).ToString();
		T_Effect_name_Wave.Text = ToolTip.GetTip(RB_x170_Effect_wave).ToString();
		T_Effect_name_Ripple.Text = ToolTip.GetTip(RB_x170_Effect_ripple).ToString();
		T_Effect_name_Random.Text = ToolTip.GetTip(RB_x170_Effect_Random).ToString();
		RB_X170_Breath_random.Content = Global.MultiLang.Translation("_Random");
		RB_X170_Breath_custom.Content = Global.MultiLang.Translation("_Custom");
		RB_X170_Blink_random.Content = Global.MultiLang.Translation("_Random");
		RB_X170_Blink_custom.Content = Global.MultiLang.Translation("_Custom");
		RB_X170_Meteor_random.Content = Global.MultiLang.Translation("_Random");
		RB_X170_Meteor_custom.Content = Global.MultiLang.Translation("_Custom");
	}

	private void RB_LEDMode_Perkey_static_Click(object sender, RoutedEventArgs e)
	{
		Show_UI_Perkey(1);
		UpdateRGBText();
		Global.KB.StaticStatus = 1;
		EnableLightBar();
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsEnabled = true;
		}
	}

	private void RB_LEDMode_Perkey_Effect_Click(object sender, RoutedEventArgs e)
	{
		foreach (CheckBox child in canvas_kb.Children)
		{
			child.IsChecked = false;
		}
		Show_UI_Perkey(2);
		UpdateRGBText();
		DisableLightBar();
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsEnabled = true;
		}
	}

	private void RB_LEDMode_KB_ALL_Click(object sender, RoutedEventArgs e)
	{
		Show_UI_KB_ALL(show: true);
		UpdateRGBText();
		Set_color();
		EnableLightBar();
	}

	private void RB_LEDMode_KB_Partial_Click(object sender, RoutedEventArgs e)
	{
		Show_UI_KB_Partial(show: true);
		UpdateRGBText();
		Set_color();
		EnableLightBar();
	}

	private void RB_LEDMode_KB_Effect_Click(object sender, RoutedEventArgs e)
	{
		Show_UI_KB_Effect(show: true);
		InitKeyboardEffect();
		DisableLightBar();
	}

	private void UpdatePartialKBUI()
	{
		RB_KB_Partial_left.Background = new SolidColorBrush(Global.KB.GetRGBLeftColor());
		RB_KB_Partial_Mid.Background = new SolidColorBrush(Global.KB.GetRGBMidColor());
		RB_KB_Partial_right.Background = new SolidColorBrush(Global.KB.GetRGBRightColor());
	}

	private void UpdateAllKBUI()
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			string[] array = new string[6] { "UpdateAllKBUI R=", null, null, null, null, null };
			Color rGBAllColor = Global.KB.GetRGBAllColor();
			array[1] = rGBAllColor.R.ToString();
			array[2] = " G=";
			rGBAllColor = Global.KB.GetRGBAllColor();
			array[3] = rGBAllColor.G.ToString();
			array[4] = " B=";
			rGBAllColor = Global.KB.GetRGBAllColor();
			array[5] = rGBAllColor.B.ToString();
			ClevoLog.Write(string.Concat(array));
			grid_KB_ALL.Background = new SolidColorBrush(Global.KB.GetRGBAllColor());
		}));
	}

	private void InitRgbStackPanel()
	{
		if (Global.KB.Mode == 8)
		{
			Show_UI_KB_Partial(show: false);
			Show_UI_KB_ALL(show: true);
			RB_LEDMode_KB_Partial.IsChecked = false;
			RB_LEDMode_KB_Effect.IsChecked = false;
			RB_LEDMode_KB_ALL.IsChecked = true;
			UpdateRGBText();
			UpdateAllKBUI();
		}
		else if (Global.KB.Mode == 1)
		{
			Show_UI_KB_Partial(show: true);
			RB_LEDMode_KB_ALL.IsChecked = false;
			RB_LEDMode_KB_Effect.IsChecked = false;
			RB_LEDMode_KB_Partial.IsChecked = true;
			UpdateRGBText();
			UpdatePartialKBUI();
		}
		else
		{
			Show_UI_KB_Effect(show: true);
			RB_LEDMode_KB_ALL.IsChecked = false;
			RB_LEDMode_KB_Partial.IsChecked = false;
			RB_LEDMode_KB_Effect.IsChecked = true;
			InitKeyboardEffect();
			DisableLightBar();
		}
	}

	private void Show_UI_KB_ALL(bool show)
	{
		if (show)
		{
			grid_KB_ALL.IsVisible = true;
			if (Global.KB.Status.Equals(0))
			{
				grid_color.IsEnabled = false;
			}
			else
			{
				grid_color.IsEnabled = true;
			}
			Show_UI_KB_Partial(show: false);
			Show_UI_KB_Effect(show: false);
		}
		else
		{
			grid_KB_ALL.IsVisible = false;
		}
	}

	private void Show_UI_KB_Partial(bool show)
	{
		if (show)
		{
			grid_KB_Partial.IsVisible = true;
			canvas_KB_layout.IsEnabled = false;
			canvas_KB_layout.IsHitTestVisible = false;
			if (Global.KB.Status.Equals(0))
			{
				grid_color.IsEnabled = false;
			}
			else
			{
				grid_color.IsEnabled = true;
			}
			Show_UI_KB_ALL(show: false);
			Show_UI_KB_Effect(show: false);
		}
		else
		{
			grid_KB_Partial.IsVisible = false;
		}
	}

	private void Show_UI_KB_Effect(bool show)
	{
		if (show)
		{
			canvas_KB_layout.IsEnabled = false;
			canvas_KB_layout.IsHitTestVisible = false;
			grid_color.IsEnabled = false;
			Canvas_KB_Effect.IsVisible = true;
			Show_UI_KB_ALL(show: false);
			Show_UI_KB_Partial(show: false);
		}
		else
		{
			Canvas_KB_Effect.IsVisible = false;
		}
	}

	private void DisableAllKBEffectChecked()
	{
		RB_KB_Effect_random.IsChecked = false;
		RB_KB_Effect_Breath.IsChecked = false;
		RB_KB_Effect_cycle.IsChecked = false;
		RB_KB_Effect_wave.IsChecked = false;
		RB_KB_Effect_Dance.IsChecked = false;
		RB_KB_Effect_Tempo.IsChecked = false;
		RB_KB_Effect_Flash.IsChecked = false;
	}

	private void InitKeyboardEffect()
	{
		DisableAllKBEffectChecked();
		switch (Global.KB.Mode)
		{
		case 0:
			RB_KB_Effect_random.IsChecked = true;
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_random).ToString();
			return;
		case 2:
			RB_KB_Effect_Breath.IsChecked = true;
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Breath).ToString();
			return;
		case 3:
			RB_KB_Effect_cycle.IsChecked = true;
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_cycle).ToString();
			return;
		case 4:
			RB_KB_Effect_wave.IsChecked = true;
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_wave).ToString();
			return;
		case 5:
			RB_KB_Effect_Dance.IsChecked = true;
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Dance).ToString();
			return;
		case 6:
			RB_KB_Effect_Tempo.IsChecked = true;
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Tempo).ToString();
			return;
		case 7:
			RB_KB_Effect_Flash.IsChecked = true;
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Flash).ToString();
			return;
		}
		Global.KB.Mode = 4;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		RB_KB_Effect_wave.IsChecked = true;
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_wave).ToString();
	}

	private void UpdateRGBText()
	{
		Color val;
		if (RB_LEDMode_KB_ALL.IsChecked.HasValue && RB_LEDMode_KB_ALL.IsChecked.Value)
		{
			val = Global.KB.GetRGBAllColor();
			string r = ((int)val.R).ToString();
			val = Global.KB.GetRGBAllColor();
			string g = ((int)val.G).ToString();
			val = Global.KB.GetRGBAllColor();
			SetRGBText(r, g, ((int)val.B).ToString());
			return;
		}
		if (RB_LEDMode_KB_Partial.IsChecked.HasValue && RB_LEDMode_KB_Partial.IsChecked.Value)
		{
			if (RB_KB_Partial_left.IsChecked.HasValue && RB_KB_Partial_left.IsChecked.Value)
			{
				val = Global.KB.GetRGBLeftColor();
				string r2 = ((int)val.R).ToString();
				val = Global.KB.GetRGBLeftColor();
				string g2 = ((int)val.G).ToString();
				val = Global.KB.GetRGBLeftColor();
				SetRGBText(r2, g2, ((int)val.B).ToString());
			}
			else if (RB_KB_Partial_Mid.IsChecked.HasValue && RB_KB_Partial_Mid.IsChecked.Value)
			{
				val = Global.KB.GetRGBMidColor();
				string r3 = ((int)val.R).ToString();
				val = Global.KB.GetRGBMidColor();
				string g3 = ((int)val.G).ToString();
				val = Global.KB.GetRGBMidColor();
				SetRGBText(r3, g3, ((int)val.B).ToString());
			}
			else if (RB_KB_Partial_right.IsChecked.HasValue && RB_KB_Partial_right.IsChecked.Value)
			{
				val = Global.KB.GetRGBRightColor();
				string r4 = ((int)val.R).ToString();
				val = Global.KB.GetRGBRightColor();
				string g4 = ((int)val.G).ToString();
				val = Global.KB.GetRGBRightColor();
				SetRGBText(r4, g4, ((int)val.B).ToString());
			}
			return;
		}
		if (RB_LEDMode_Perkey_Effect.IsChecked.HasValue && RB_LEDMode_Perkey_Effect.IsChecked.Value)
		{
			if (RB_PerKey_Effect_Breath.IsChecked.HasValue && RB_PerKey_Effect_Breath.IsChecked.Value && RB_Perkey_Breath_custom.IsChecked.HasValue && RB_Perkey_Breath_custom.IsChecked.Value)
			{
				val = Global.KB.GetPerkeyBreathColor();
				string r5 = val.R.ToString();
				val = Global.KB.GetPerkeyBreathColor();
				string g5 = val.G.ToString();
				val = Global.KB.GetPerkeyBreathColor();
				SetRGBText(r5, g5, val.B.ToString());
			}
			if (RB_PerKey_Effect_Scan.IsChecked.HasValue && RB_PerKey_Effect_Scan.IsChecked.Value && RB_Perkey_Scan_Custom.IsChecked.HasValue && RB_Perkey_Scan_Custom.IsChecked.Value)
			{
				if (RB_Perkey_Scan_Color2.IsChecked.Equals(true))
				{
					val = Global.KB.GetPerkeyScanColor2();
					string r6 = val.R.ToString();
					val = Global.KB.GetPerkeyScanColor2();
					string g6 = val.G.ToString();
					val = Global.KB.GetPerkeyScanColor2();
					SetRGBText(r6, g6, val.B.ToString());
				}
				else
				{
					val = Global.KB.GetPerkeyScanColor1();
					string r7 = val.R.ToString();
					val = Global.KB.GetPerkeyScanColor1();
					string g7 = val.G.ToString();
					val = Global.KB.GetPerkeyScanColor1();
					SetRGBText(r7, g7, val.B.ToString());
				}
			}
			else if (RB_PerKey_Effect_Blink.IsChecked.HasValue && RB_PerKey_Effect_Blink.IsChecked.Value && RB_Perkey_Blink_custom.IsChecked.HasValue && RB_Perkey_Blink_custom.IsChecked.Value)
			{
				val = Global.KB.GetPerkeyBlinkColor();
				string r8 = val.R.ToString();
				val = Global.KB.GetPerkeyBlinkColor();
				string g8 = val.G.ToString();
				val = Global.KB.GetPerkeyBlinkColor();
				SetRGBText(r8, g8, val.B.ToString());
			}
			else if (RB_PerKey_Effect_Random.IsChecked.HasValue && RB_PerKey_Effect_Random.IsChecked.Value && RB_Perkey_Random_custom.IsChecked.HasValue && RB_Perkey_Random_custom.IsChecked.Value)
			{
				val = Global.KB.GetPerkeyRandomColor();
				string r9 = val.R.ToString();
				val = Global.KB.GetPerkeyRandomColor();
				string g9 = val.G.ToString();
				val = Global.KB.GetPerkeyRandomColor();
				SetRGBText(r9, g9, val.B.ToString());
			}
			else if (RB_PerKey_Effect_ripple.IsChecked.HasValue && RB_PerKey_Effect_ripple.IsChecked.Value && RB_Perkey_Ripple_custom.IsChecked.HasValue && RB_Perkey_Ripple_custom.IsChecked.Value)
			{
				val = Global.KB.GetPerkeyRippleColor();
				string r10 = val.R.ToString();
				val = Global.KB.GetPerkeyRippleColor();
				string g10 = val.G.ToString();
				val = Global.KB.GetPerkeyRippleColor();
				SetRGBText(r10, g10, val.B.ToString());
			}
			else if (RB_PerKey_Effect_wave.IsChecked.HasValue && RB_PerKey_Effect_wave.IsChecked.Value && RB_Wave_Custom.IsChecked.HasValue && RB_Wave_Custom.IsChecked.Value)
			{
				val = Global.KB.GetPerkeyWaveColor();
				string r11 = val.R.ToString();
				val = Global.KB.GetPerkeyWaveColor();
				string g11 = val.G.ToString();
				val = Global.KB.GetPerkeyWaveColor();
				SetRGBText(r11, g11, val.B.ToString());
			}
			else if (RB_PerKey_Effect_Snake.IsChecked.HasValue && RB_PerKey_Effect_Snake.IsChecked.Value && RB_Snake_Custom.IsChecked.HasValue && RB_Snake_Custom.IsChecked.Value)
			{
				val = Global.KB.GetPerkeySnakeColor();
				string r12 = val.R.ToString();
				val = Global.KB.GetPerkeySnakeColor();
				string g12 = val.G.ToString();
				val = Global.KB.GetPerkeySnakeColor();
				SetRGBText(r12, g12, val.B.ToString());
			}
			else if (RB_PerKey_Effect_Fn.IsChecked.HasValue && RB_PerKey_Effect_Fn.IsChecked.Value)
			{
				val = Global.KB.GetPerkeyFnColor();
				string r13 = val.R.ToString();
				val = Global.KB.GetPerkeyFnColor();
				string g13 = val.G.ToString();
				val = Global.KB.GetPerkeyFnColor();
				SetRGBText(r13, g13, val.B.ToString());
			}
			return;
		}
		if (RB_LEDMode_LOGO.IsChecked.HasValue && RB_LEDMode_LOGO.IsChecked.Value)
		{
			val = Global.KB.GetRGBLogoColor();
			string r14 = val.R.ToString();
			val = Global.KB.GetRGBLogoColor();
			string g14 = val.G.ToString();
			val = Global.KB.GetRGBLogoColor();
			SetRGBText(r14, g14, val.B.ToString());
			return;
		}
		if (RB_LEDMode_LightBar.IsChecked.HasValue && RB_LEDMode_LightBar.IsChecked.Value)
		{
			val = Global.KB.GetRGBLightBarColor();
			string r15 = val.R.ToString();
			val = Global.KB.GetRGBLightBarColor();
			string g15 = val.G.ToString();
			val = Global.KB.GetRGBLightBarColor();
			SetRGBText(r15, g15, val.B.ToString());
			return;
		}
		if (RB_LEDMode_Perkey_static.IsChecked == true)
		{
			SetCbText(txt_color_red, "");
			SetCbText(txt_color_green, "");
			SetCbText(txt_color_blue, "");
			{
				foreach (CheckBox child in canvas_kb.Children)
				{
					CheckBox val2 = child;
					if (val2.IsChecked == true && val2.IsVisible)
					{
						ComboBox val3 = txt_color_red;
						val = BrushColor(val2.BorderBrush);
						SetCbText(val3, val.R.ToString());
						ComboBox val4 = txt_color_green;
						val = BrushColor(val2.BorderBrush);
						SetCbText(val4, val.G.ToString());
						ComboBox val5 = txt_color_blue;
						val = BrushColor(val2.BorderBrush);
						SetCbText(val5, val.B.ToString());
					}
				}
				return;
			}
		}
		if (RB_LEDMode_LightBarX170.IsChecked == true)
		{
			if (RB_x170_Effect_Static.IsChecked == true)
			{
				SetRGBText(X170LB.Static_R.ToString(), X170LB.Static_G.ToString(), X170LB.Static_B.ToString());
			}
			else if (RB_x170_Effect_Breath.IsChecked == true)
			{
				SetRGBText(X170LB.Breath_R.ToString(), X170LB.Breath_G.ToString(), X170LB.Breath_B.ToString());
			}
			else if (RB_x170_Effect_Blink.IsChecked == true)
			{
				SetRGBText(X170LB.Blink_R.ToString(), X170LB.Blink_G.ToString(), X170LB.Blink_B.ToString());
			}
			else if (RB_x170_Effect_Meteor.IsChecked == true)
			{
				SetRGBText(X170LB.Meteor_R.ToString(), X170LB.Meteor_G.ToString(), X170LB.Meteor_B.ToString());
			}
		}
	}

	private void SetRGBText(string R, string G, string B)
	{
		SetCbText(txt_color_red, R);
		SetCbText(txt_color_green, G);
		SetCbText(txt_color_blue, B);
	}

	private void Show_Color_KBLayout_UI(bool ColorUI, bool KBUI)
	{
		if (ColorUI)
		{
			if (Global.KB.Status == 0)
			{
				grid_color.IsEnabled = false;
			}
			else
			{
				grid_color.IsEnabled = true;
			}
		}
		else
		{
			grid_color.IsEnabled = false;
		}
		if (KBUI)
		{
			canvas_KB_layout.IsEnabled = true;
		}
		else
		{
			canvas_KB_layout.IsEnabled = false;
		}
	}

	/// <summary>WPF: ComboBox.Text setter on a non-editable ComboBox selects the item whose text matches (or clears the selection).</summary>
	private static void SetCbText(ComboBox cb, string text)
	{
		object found = null;
		if (cb.ItemsSource != null)
			foreach (var it in cb.ItemsSource)
				if (it != null && it.ToString() == text) { found = it; break; }
		cb.SelectedItem = found;
	}

	private static Color BrushColor(IBrush b) => b is ISolidColorBrush s ? s.Color : default;

	/// <summary>WPF: CroppedBitmap(1x1).CopyPixels -> BGRA of the bitmap pixel under the pointer.</summary>
	private static byte[] PixelAt(Image img, Point position)
	{
		var array = new byte[4];
		if (img.Source is Avalonia.Media.Imaging.Bitmap bmp && img.Bounds.Width > 0 && img.Bounds.Height > 0)
		{
			int x = (int)(position.X / img.Bounds.Width * bmp.PixelSize.Width);
			int y = (int)(position.Y / img.Bounds.Height * bmp.PixelSize.Height);
			x = Math.Clamp(x, 0, bmp.PixelSize.Width - 1);
			y = Math.Clamp(y, 0, bmp.PixelSize.Height - 1);
			var h = System.Runtime.InteropServices.GCHandle.Alloc(array, System.Runtime.InteropServices.GCHandleType.Pinned);
			try { bmp.CopyPixels(new PixelRect(x, y, 1, 1), h.AddrOfPinnedObject(), 4, 4); }
			finally { h.Free(); }
		}
		return array;
	}

	private void BTN_color_picker_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		byte[] array = PixelAt(image_color_picker, e.GetPosition(image_color_picker));
		SetCbText(txt_color_red, Convert.ToString(array[2]));
		SetCbText(txt_color_green, Convert.ToString(array[1]));
		SetCbText(txt_color_blue, Convert.ToString(array[0]));
	}

	private void BTN_color_picker_whole_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		byte[] array = PixelAt(image_color_picker_whole, e.GetPosition(image_color_picker_whole));
		SetCbText(txt_color_red, Convert.ToString(array[2]));
		SetCbText(txt_color_green, Convert.ToString(array[1]));
		SetCbText(txt_color_blue, Convert.ToString(array[0]));
	}

	private void BTN_color_picker_Click(object sender, RoutedEventArgs e)
	{
		Set_color();
	}

	private void BTN_color_plate_red_Click(object sender, RoutedEventArgs e)
	{
		Set_color();
	}

	private void BTN_color_plate_red_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_red.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_red.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_red.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_pink_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_pink.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_pink.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_pink.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_purple_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_purple.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_purple.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_purple.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_bule_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_bule.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_bule.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_bule.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_sky_blue_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_sky_blue.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_sky_blue.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_sky_blue.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_green_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_green.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_green.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_green.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_yellow_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_yellow.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_yellow.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_yellow.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_orange_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_orange.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_orange.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_orange.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_white_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_white.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_white.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_white.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_black_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_black.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_black.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_black.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		Button val = (Button)sender;
		ComboBox val2 = txt_color_red;
		Color val3 = BrushColor(val.BorderBrush);
		val2.Text = val3.R.ToString();
		ComboBox val4 = txt_color_green;
		val3 = BrushColor(val.BorderBrush);
		SetCbText(val4, val3.G.ToString());
		ComboBox val5 = txt_color_blue;
		val3 = BrushColor(val.BorderBrush);
		SetCbText(val5, val3.B.ToString());
	}

	private void txt_color_selectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (txt_color_isKeyboardFocus)
		{
			Set_color();
		}
	}

	private void txt_color_EnterKeyDown(object sender, KeyEventArgs e)
	{
		if (((int)e.Key > 34 && (int)e.Key < 43) || ((int)e.Key > 74 && (int)e.Key < 83) || (int)e.Key == 6)
		{
			Set_color();
		}
	}

	private void txt_color_GotKeyboardFocus(object sender, RoutedEventArgs e)
	{
		txt_color_isKeyboardFocus = true;
	}

	private void Set_color()
	{
		byte b = Convert.ToByte(((SelectingItemsControl)txt_color_red).SelectedValue);
		byte b2 = Convert.ToByte(((SelectingItemsControl)txt_color_green).SelectedValue);
		byte b3 = Convert.ToByte(((SelectingItemsControl)txt_color_blue).SelectedValue);
		if (RB_LEDMode_Perkey_static.IsChecked == true)
		{
			string text = "[Color] Red: " + b + ", Green: " + b2 + ", Blue: " + b3 + "\n";
			foreach (CheckBox child in canvas_kb.Children)
			{
				CheckBox val = child;
				if (val.IsChecked == true && val.IsVisible)
				{
					short num = Convert.ToInt16(val.Content.ToString(), 16);
					byte b4 = (byte)(num >> 8);
					byte key = (byte)(num & 0xFF);
					Global.KB.SetPerkeyStaticColorWithReg(key, Color.FromRgb(b, b2, b3));
					text = text + "Key " + key + ", ";
					if (b4 > 0)
					{
						Global.KB.SetPerkeyStaticColorWithReg(b4, Color.FromRgb(b, b2, b3));
						text = text + "Key " + b4 + ", ";
					}
					if (PerKeyType == 1 && val.Content.Equals("a5a9"))
					{
						Global.KB.SetPerkeyStaticColorWithReg(166, Color.FromRgb(b, b2, b3));
						Global.KB.SetPerkeyStaticColorWithReg(168, Color.FromRgb(b, b2, b3));
						text = text + "Key " + 166 + ", ";
						text = text + "Key " + 168 + ", ";
					}
					if (val.Content.Equals("6f"))
					{
						Global.KB.SetPerkeyStaticColorWithReg(78, Color.FromRgb(b, b2, b3));
						Global.KB.SetPerkeyStaticColorWithReg(79, Color.FromRgb(b, b2, b3));
						text = text + "Key " + 78 + ", ";
						text = text + "Key " + 79 + ", ";
					}
					if (b == 0 && b2 == 0 && b3 == 0)
					{
						val.BorderBrush = new SolidColorBrush(Color.FromArgb((byte)0, b, b2, b3));
					}
					else
					{
						val.BorderBrush = new SolidColorBrush(Color.FromArgb((byte)200, b, b2, b3));
					}
				}
			}
			Global.KB.WriteLog(text, "Perkey_Message");
			SetSleepTimer();
		}
		else if (RB_LEDMode_Perkey_Effect.IsChecked == true)
		{
			if (RB_PerKey_Effect_Breath.IsChecked == true)
			{
				Perkey_SetBreathWithColor(b, b2, b3);
			}
			if (RB_PerKey_Effect_Scan.IsChecked == true)
			{
				if (RB_Perkey_Scan_Color1.IsChecked.Equals(true))
				{
					Global.KB.SetPerkeyScanColorMode(Color.FromRgb(b, b2, b3), Global.KB.GetPerkeyScanColor2());
				}
				else if (RB_Perkey_Scan_Color2.IsChecked.Equals(true))
				{
					Global.KB.SetPerkeyScanColorMode(Global.KB.GetPerkeyScanColor1(), Color.FromRgb(b, b2, b3));
				}
				SetSleepTimer();
			}
			if (RB_PerKey_Effect_Blink.IsChecked == true)
			{
				Perkey_SetBlinkWithColor(b, b2, b3);
			}
			if (RB_PerKey_Effect_Random.IsChecked == true)
			{
				Global.KB.SetPerkeyRandomColorMode(Color.FromRgb(b, b2, b3));
				SetSleepTimer();
			}
			if (RB_PerKey_Effect_ripple.IsChecked == true && RB_Perkey_Ripple_Ripple.IsChecked == true)
			{
				Perkey_SetRipponColor(b, b2, b3);
			}
			if (RB_PerKey_Effect_ripple.IsChecked == true && RB_Perkey_Ripple_Reactive.IsChecked == true)
			{
				Perkey_SetReactiveColor(b, b2, b3);
			}
			if (RB_PerKey_Effect_wave.IsChecked == true && RB_Wave_Custom.IsChecked == true)
			{
				int wave_mode = GetWaveVector() + 8;
				Global.KB.SetPerkeyWaveColorMode(wave_mode, Color.FromRgb(b, b2, b3));
				SetSleepTimer();
			}
			if (RB_PerKey_Effect_Snake.IsChecked.Equals(true) && RB_Snake_Custom.IsChecked.Equals(true))
			{
				int snakeMode = GetSnakeVector() + 4;
				Global.KB.SetPerkeySnakeColorMode(snakeMode, Color.FromRgb(b, b2, b3));
				SetSleepTimer();
			}
			if (RB_PerKey_Effect_Fn.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyFnColorMode(Color.FromRgb(b, b2, b3));
				SetSleepTimer();
			}
		}
		else if (RB_LEDMode_KB_ALL.IsChecked == true)
		{
			Global.KB.Mode = 8;
			if (!(Global.KB.GetRGBAllColor() == Color.FromRgb(b, b2, b3)))
			{
				Global.KB.SetRGBAllColor(Color.FromRgb(b, b2, b3));
				Global.KB.SetRGB3ParStatusOn();
				Global.KB.SetMode();
				UpdateAllKBUI();
			}
		}
		else if (RB_LEDMode_KB_Partial.IsChecked == true)
		{
			if (RB_KB_Partial_left.IsChecked == true)
			{
				Global.KB.Mode = 1;
				Global.KB.SetRGBLeftColor(Color.FromRgb(b, b2, b3));
			}
			else if (RB_KB_Partial_Mid.IsChecked == true)
			{
				Global.KB.Mode = 1;
				Global.KB.SetRGBMidColor(Color.FromRgb(b, b2, b3));
			}
			else
			{
				Global.KB.Mode = 1;
				Global.KB.SetRGBRightColor(Color.FromRgb(b, b2, b3));
			}
			Global.KB.SetRGB3ParStatusOn();
			Global.KB.SetMode();
			UpdatePartialKBUI();
		}
		else if (RB_LEDMode_LOGO.IsChecked == true)
		{
			Global.KB.SetRGBLogoColor(Color.FromRgb(b, b2, b3));
			Global.KB.SetMode();
			ShowLogoColor.Fill = new SolidColorBrush(Color.FromRgb(b, b2, b3));
		}
		else if (RB_LEDMode_LightBar.IsChecked == true)
		{
			Global.KB.SetRGBLightBarColor(Color.FromRgb(b, b2, b3));
			Global.KB.SetMode();
			ShowLightBar.Fill = new SolidColorBrush(Color.FromRgb(b, b2, b3));
		}
		else if (RB_LEDMode_White.IsChecked == true && KBType.Equals(6))
		{
			Global.KB.Mode = 8;
			if (!(Global.KB.GetRGBAllColor() == Color.FromRgb(b, b2, b3)))
			{
				Global.KB.SetRGBAllColor(Color.FromRgb(b, b2, b3));
				Global.KB.SetRGB3ParStatusOn();
				Global.KB.SetMode();
				UpdateAllKBUI();
			}
		}
		else if (RB_LEDMode_LightBarX170.IsChecked == true)
		{
			if (RB_x170_Effect_Static.IsChecked == true)
			{
				Update_LightBar_UI(176, b, b2, b3);
			}
			else if (RB_x170_Effect_Breath.IsChecked == true)
			{
				Update_LightBar_UI(178, b, b2, b3);
			}
			else if (RB_x170_Effect_Blink.IsChecked == true)
			{
				Update_LightBar_UI(179, b, b2, b3);
			}
			else if (RB_x170_Effect_Meteor.IsChecked == true)
			{
				Update_LightBar_UI(181, b, b2, b3);
			}
			Show_LightBar_x170.Fill = new SolidColorBrush(Color.FromRgb(b, b2, b3));
		}
	}

	private void Show_UI_Perkey(int page)
	{
		switch (page)
		{
		case 1:
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: true);
			grid_LED_Speed.IsVisible = false;
			Canvas_Perkey_Effect.IsVisible = false;
			Global.KB.StaticStatus = 1;
			RB_LEDMode_Perkey_static.IsChecked = true;
			grid_perkey_static.IsVisible = true;
			Global.KB.PerkeyClearColor();
			PerkeyLoadStatic();
			break;
		case 2:
			grid_LED_Speed.IsVisible = true;
			Canvas_Perkey_Effect.IsVisible = true;
			grid_perkey_static.IsVisible = false;
			RB_LEDMode_Perkey_Effect.IsChecked = true;
			Load_PerKey_Effet(Global.KB.Mode);
			break;
		default:
			Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
			grid_LED_Speed.IsVisible = false;
			Canvas_Perkey_Effect.IsVisible = false;
			grid_perkey_static.IsVisible = false;
			break;
		}
	}

	private void PerkeyLoadStatic()
	{
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val = child;
			try
			{
				if (val.IsVisible)
				{
					short num = Convert.ToInt16(val.Content.ToString(), 16);
					byte b = (byte)(num >> 8);
					byte b2 = (byte)(num & 0xFF);
					int row = (b2 >> 5) & 7;
					int col = b2 & 0x1F;
					Color perkeyStaticColor = Global.KB.GetPerkeyStaticColor(row, col);
					if (val.Content.Equals("a5a9") && PerKeyType == 1)
					{
						Global.KB.SetPerkeyStaticColor(166, perkeyStaticColor);
						Global.KB.SetPerkeyStaticColor(168, perkeyStaticColor);
					}
					if (val.Content.Equals("6f"))
					{
						Global.KB.SetPerkeyStaticColor(78, perkeyStaticColor);
						Global.KB.SetPerkeyStaticColor(79, perkeyStaticColor);
					}
					Global.KB.SetPerkeyStaticColor(b2, perkeyStaticColor);
					if (b > 0)
					{
						Global.KB.SetPerkeyStaticColor(b, perkeyStaticColor);
					}
					if (perkeyStaticColor.Equals(Color.FromRgb((byte)0, (byte)0, (byte)0)))
					{
						val.BorderBrush = new SolidColorBrush(Color.FromArgb((byte)0, (byte)0, (byte)0, (byte)0));
					}
					else
					{
						val.BorderBrush = new SolidColorBrush(Color.FromArgb((byte)200, perkeyStaticColor.R, perkeyStaticColor.G, perkeyStaticColor.B));
					}
				}
			}
			catch
			{
			}
		}
		Global.KB.SetStatus();
		if (Global.KB.SleepTimerTrigger.Equals(1))
		{
			Global.KB.SetSleepTimerTrigger(Global.KB.SleepTimer);
		}
		else
		{
			Global.KB.SetSleepTimerTriggerOff();
		}
	}

	private void PerkeyFirstRunStaticColor(Color color)
	{
		InsydeAcpiProt.WriteAcpiStaticStatus(1);
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val = child;
			try
			{
				if (val.IsVisible)
				{
					short num = Convert.ToInt16(val.Content.ToString(), 16);
					byte b = (byte)(num >> 8);
					byte key = (byte)(num & 0xFF);
					if (val.Content.Equals("a5a9") && PerKeyType == 1)
					{
						Global.KB.SetPerkeyStaticColorOnlyReg(166, color);
						Global.KB.SetPerkeyStaticColorOnlyReg(168, color);
					}
					if (val.Content.Equals("6f"))
					{
						Global.KB.SetPerkeyStaticColorOnlyReg(78, color);
						Global.KB.SetPerkeyStaticColorOnlyReg(79, color);
					}
					Global.KB.SetPerkeyStaticColorOnlyReg(key, color);
					if (b > 0)
					{
						Global.KB.SetPerkeyStaticColorOnlyReg(b, color);
					}
				}
			}
			catch
			{
			}
		}
	}

	private void BTN_Restore_PerKey_static_Click(object sender, RoutedEventArgs e)
	{
		foreach (CheckBox child in canvas_kb.Children)
		{
			child.IsChecked = false;
			SetCbText(txt_color_red, "");
			SetCbText(txt_color_green, "");
			SetCbText(txt_color_blue, "");
			child.BorderBrush = new SolidColorBrush(Color.FromArgb((byte)0, (byte)0, (byte)0, (byte)0));
		}
		Global.KB.PerkeyStaticDefault();
		Global.KB.WriteLog("Static Restore", "Perkey_Message");
	}

	private void Load_PerKey_Effet(int mode)
	{
		int num = 0;
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		Global.KB.Mode = mode;
		Global.KB.StaticStatus = 0;
		switch (mode)
		{
		case 0:
			RB_PerKey_Effect_wave.IsChecked = true;
			break;
		case 1:
			RB_PerKey_Effect_Breath.IsChecked = true;
			if (Global.KB.GetPerkeyBreathStatus() == 0)
			{
				RB_Perkey_Breath_random.IsChecked = true;
				break;
			}
			RB_Perkey_Breath_custom.IsChecked = true;
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			break;
		case 2:
			RB_PerKey_Effect_Blink.IsChecked = true;
			if (Global.KB.GetPerkeyBlinkStatus() == 0)
			{
				RB_Perkey_Blink_random.IsChecked = true;
				break;
			}
			RB_Perkey_Blink_custom.IsChecked = true;
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			break;
		case 3:
			RB_PerKey_Effect_Random.IsChecked = true;
			break;
		case 4:
			RB_PerKey_Effect_Scan.IsChecked = true;
			break;
		case 5:
			RB_PerKey_Effect_ripple.IsChecked = true;
			switch (Global.KB.GetPerkeyRippleStatus())
			{
			case 0:
				RB_Perkey_Ripple_Ripple.IsChecked = true;
				RB_Perkey_Ripple_random.IsChecked = true;
				break;
			case 1:
				RB_Perkey_Ripple_Ripple.IsChecked = true;
				RB_Perkey_Ripple_custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 2:
				RB_Perkey_Ripple_Reactive.IsChecked = true;
				RB_Perkey_Ripple_random.IsChecked = true;
				break;
			default:
				RB_Perkey_Ripple_Reactive.IsChecked = true;
				RB_Perkey_Ripple_custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			}
			break;
		case 6:
			RB_PerKey_Effect_Snake.IsChecked = true;
			break;
		case 7:
			RB_PerKey_Effect_wave.IsChecked = true;
			switch (Global.KB.GetPerkeyWaveStatus())
			{
			case 0:
				RB_Perkey_Wave_Arrow1.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 1:
				RB_Perkey_Wave_Arrow2.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 2:
				RB_Perkey_Wave_Arrow3.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 3:
				RB_Perkey_Wave_Arrow4.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 4:
				RB_Perkey_Wave_Arrow5.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 5:
				RB_Perkey_Wave_Arrow6.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 6:
				RB_Perkey_Wave_Arrow7.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 7:
				RB_Perkey_Wave_Arrow8.IsChecked = true;
				RB_Wave_Default.IsChecked = true;
				break;
			case 8:
				RB_Perkey_Wave_Arrow1.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 9:
				RB_Perkey_Wave_Arrow2.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 10:
				RB_Perkey_Wave_Arrow3.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 11:
				RB_Perkey_Wave_Arrow4.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 12:
				RB_Perkey_Wave_Arrow5.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 13:
				RB_Perkey_Wave_Arrow6.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 14:
				RB_Perkey_Wave_Arrow7.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 15:
				RB_Perkey_Wave_Arrow8.IsChecked = true;
				RB_Wave_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			}
			break;
		case 8:
			RB_PerKey_Effect_Scan.IsChecked = true;
			if (Global.KB.GetPerkeyScanStatus() == 0)
			{
				RB_Perkey_Scan_Random.IsChecked = true;
				break;
			}
			RB_Perkey_Scan_Custom.IsChecked = true;
			RB_Perkey_Scan_Color1.IsChecked = true;
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			break;
		case 9:
			RB_PerKey_Effect_Random.IsChecked = true;
			if (Global.KB.GetPerkeyRandomStatus() == 0)
			{
				RB_Perkey_Random_random.IsChecked = true;
				break;
			}
			RB_Perkey_Random_custom.IsChecked = true;
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			break;
		case 10:
			RB_PerKey_Effect_Snake.IsChecked = true;
			switch (Global.KB.GetPerkeySnakeStatus())
			{
			case 0:
				RB_Perkey_Snake_Arrow1.IsChecked = true;
				RB_Snake_Random.IsChecked = true;
				break;
			case 1:
				RB_Perkey_Snake_Arrow2.IsChecked = true;
				RB_Snake_Random.IsChecked = true;
				break;
			case 2:
				RB_Perkey_Snake_Arrow3.IsChecked = true;
				RB_Snake_Random.IsChecked = true;
				break;
			case 3:
				RB_Perkey_Snake_Arrow4.IsChecked = true;
				RB_Snake_Random.IsChecked = true;
				break;
			case 4:
				RB_Perkey_Snake_Arrow1.IsChecked = true;
				RB_Snake_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 5:
				RB_Perkey_Snake_Arrow2.IsChecked = true;
				RB_Snake_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 6:
				RB_Perkey_Snake_Arrow3.IsChecked = true;
				RB_Snake_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			case 7:
				RB_Perkey_Snake_Arrow4.IsChecked = true;
				RB_Snake_Custom.IsChecked = true;
				Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
				break;
			}
			break;
		case 11:
			RB_PerKey_Effect_Fn.IsChecked = true;
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			break;
		}
		Global.KB.SetMode();
		if (Global.KB.SleepTimerTrigger.Equals(1))
		{
			Global.KB.SetSleepTimerTrigger(Global.KB.SleepTimer);
		}
		else
		{
			Global.KB.SetSleepTimerTriggerOff();
		}
	}

	private void RB_PerKey_Effect_wave_Click(object sender, RoutedEventArgs e)
	{
		if (Global.KB.SupportPerkeyNewEffect())
		{
			Load_PerKey_Effet(7);
			UpdateRGBText();
		}
		else
		{
			Load_PerKey_Effet(0);
		}
	}

	private void RB_Wave_Default_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		int waveVector = GetWaveVector();
		Global.KB.SetPerkeyWaveDefaultMode(waveVector);
		SetSleepTimer();
	}

	private void RB_Wave_Custom_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
		int perkeyWaveColorMode = GetWaveVector() + 8;
		Global.KB.SetPerkeyWaveColorMode(perkeyWaveColorMode);
		SetSleepTimer();
		UpdateRGBText();
	}

	private int GetWaveVector()
	{
		int result = 0;
		if (RB_Perkey_Wave_Arrow1.IsChecked.Equals(true))
		{
			result = 0;
		}
		else if (RB_Perkey_Wave_Arrow2.IsChecked.Equals(true))
		{
			result = 1;
		}
		else if (RB_Perkey_Wave_Arrow3.IsChecked.Equals(true))
		{
			result = 2;
		}
		else if (RB_Perkey_Wave_Arrow4.IsChecked.Equals(true))
		{
			result = 3;
		}
		else if (RB_Perkey_Wave_Arrow5.IsChecked.Equals(true))
		{
			result = 4;
		}
		else if (RB_Perkey_Wave_Arrow6.IsChecked.Equals(true))
		{
			result = 5;
		}
		else if (RB_Perkey_Wave_Arrow7.IsChecked.Equals(true))
		{
			result = 6;
		}
		else if (RB_Perkey_Wave_Arrow8.IsChecked.Equals(true))
		{
			result = 7;
		}
		return result;
	}

	private void RB_Perkey_Wave_Arrow_Click(object sender, RoutedEventArgs e)
	{
		RadioButton val = (RadioButton)sender;
		if (val.Name.Contains("1"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(8);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(0);
			}
		}
		else if (val.Name.Contains("2"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(9);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(1);
			}
		}
		else if (val.Name.Contains("3"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(10);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(2);
			}
		}
		else if (val.Name.Contains("4"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(11);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(3);
			}
		}
		else if (val.Name.Contains("5"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(12);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(4);
			}
		}
		else if (val.Name.Contains("6"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(13);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(5);
			}
		}
		else if (val.Name.Contains("7"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(14);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(6);
			}
		}
		else if (val.Name.Contains("8"))
		{
			if (RB_Wave_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeyWaveColorMode(15);
			}
			else
			{
				Global.KB.SetPerkeyWaveDefaultMode(7);
			}
		}
		SetSleepTimer();
	}

	private void RB_PerKey_Effect_breath_Click(object sender, RoutedEventArgs e)
	{
		Load_PerKey_Effet(1);
		UpdateRGBText();
	}

	private void RB_PerKey_Effect_Blink_Click(object sender, RoutedEventArgs e)
	{
		Load_PerKey_Effet(2);
		UpdateRGBText();
	}

	private void RB_PerKey_Effect_Random_Click(object sender, RoutedEventArgs e)
	{
		if (Global.KB.SupportPerkeyNewEffect())
		{
			Load_PerKey_Effet(9);
			UpdateRGBText();
		}
		else
		{
			Load_PerKey_Effet(3);
		}
	}

	private void RB_Perkey_Random_random_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		Global.KB.SetPerkeyRandomRandomMode();
		SetSleepTimer();
	}

	private void RB_Perkey_Random_custom_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
		Global.KB.SetPerkeyRandomColorMode();
		SetSleepTimer();
		UpdateRGBText();
	}

	private void RB_PerKey_Effect_Scan_Click(object sender, RoutedEventArgs e)
	{
		if (Global.KB.SupportPerkeyNewEffect())
		{
			Load_PerKey_Effet(8);
			UpdateRGBText();
		}
		else
		{
			Load_PerKey_Effet(4);
		}
	}

	private void RB_PerKey_Effect_ripple_Click(object sender, RoutedEventArgs e)
	{
		Load_PerKey_Effet(5);
		UpdateRGBText();
	}

	private void RB_PerKey_Effect_Snake_Click(object sender, RoutedEventArgs e)
	{
		if (Global.KB.SupportPerkeyNewEffect())
		{
			Load_PerKey_Effet(10);
			UpdateRGBText();
		}
		else
		{
			Load_PerKey_Effet(6);
		}
	}

	private void RB_Snake_Random_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		int snakeVector = GetSnakeVector();
		Global.KB.SetPerkeySnakeRandomMode(snakeVector);
		SetSleepTimer();
	}

	private void RB_Snake_Custom_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
		int perkeySnakeColorMode = GetSnakeVector() + 4;
		Global.KB.SetPerkeySnakeColorMode(perkeySnakeColorMode);
		SetSleepTimer();
		UpdateRGBText();
	}

	private int GetSnakeVector()
	{
		int result = 0;
		if (RB_Perkey_Snake_Arrow1.IsChecked.Equals(true))
		{
			result = 0;
		}
		else if (RB_Perkey_Snake_Arrow2.IsChecked.Equals(true))
		{
			result = 1;
		}
		else if (RB_Perkey_Snake_Arrow3.IsChecked.Equals(true))
		{
			result = 2;
		}
		else if (RB_Perkey_Snake_Arrow4.IsChecked.Equals(true))
		{
			result = 3;
		}
		return result;
	}

	private void RB_Perkey_Snake_Arrow_Click(object sender, RoutedEventArgs e)
	{
		RadioButton val = (RadioButton)sender;
		if (val.Name.Contains("1"))
		{
			if (RB_Snake_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeySnakeColorMode(4);
			}
			else
			{
				Global.KB.SetPerkeySnakeRandomMode(0);
			}
		}
		else if (val.Name.Contains("2"))
		{
			if (RB_Snake_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeySnakeColorMode(5);
			}
			else
			{
				Global.KB.SetPerkeySnakeRandomMode(1);
			}
		}
		else if (val.Name.Contains("3"))
		{
			if (RB_Snake_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeySnakeColorMode(6);
			}
			else
			{
				Global.KB.SetPerkeySnakeRandomMode(2);
			}
		}
		else if (val.Name.Contains("4"))
		{
			if (RB_Snake_Custom.IsChecked.Equals(true))
			{
				Global.KB.SetPerkeySnakeColorMode(7);
			}
			else
			{
				Global.KB.SetPerkeySnakeRandomMode(3);
			}
		}
		SetSleepTimer();
	}

	private void RB_Perkey_Breath_random_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		Perkey_SetBreathRandomColor();
	}

	private void RB_Perkey_Breath_custom_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
		Global.KB.SetPerkeyBreathColorMode();
		SetSleepTimer();
		UpdateRGBText();
	}

	private void RB_Perkey_Scan_Random_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		Global.KB.SetPerkeyScanRandomMode();
	}

	private void RB_Perkey_Scan_Custom_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
		Global.KB.SetPerkeyScanColorMode();
		SetSleepTimer();
		UpdateRGBText();
		if (!RB_Perkey_Scan_Color1.IsChecked.Equals(true) && !RB_Perkey_Scan_Color2.IsChecked.Equals(true))
		{
			RB_Perkey_Scan_Color1.IsChecked = true;
		}
	}

	private void RB_Perkey_Scan_Color1_Click(object sender, RoutedEventArgs e)
	{
		Color perkeyScanColor = Global.KB.GetPerkeyScanColor1();
		SetRGBText(perkeyScanColor.R.ToString(), perkeyScanColor.G.ToString(), perkeyScanColor.B.ToString());
	}

	private void RB_Perkey_Scan_Color2_Click(object sender, RoutedEventArgs e)
	{
		Color perkeyScanColor = Global.KB.GetPerkeyScanColor2();
		SetRGBText(perkeyScanColor.R.ToString(), perkeyScanColor.G.ToString(), perkeyScanColor.B.ToString());
	}

	private void RB_Perkey_Blink_random_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		Perkey_SetBlinkRandomColor();
	}

	private void RB_Perkey_Blink_custom_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
		Global.KB.SetPerkeyBlinkColorMode();
		SetSleepTimer();
		UpdateRGBText();
	}

	private void RB_Perkey_Ripple_random_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
		if (RB_Perkey_Ripple_Ripple.IsChecked == true)
		{
			Perkey_SetRippon();
		}
		else
		{
			Perkey_SetReactive();
		}
	}

	private void RB_Perkey_Ripple_custom_Click(object sender, RoutedEventArgs e)
	{
		Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
		Color perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
		byte r = perkeyRippleColor.R;
		perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
		byte g = perkeyRippleColor.G;
		perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
		byte b = perkeyRippleColor.B;
		if (RB_Perkey_Ripple_Ripple.IsChecked == true)
		{
			Perkey_SetRipponColor(r, g, b);
		}
		else
		{
			Perkey_SetReactiveColor(r, g, b);
		}
		UpdateRGBText();
	}

	private void RB_Perkey_Ripple_Ripple_Click(object sender, RoutedEventArgs e)
	{
		if (RB_Perkey_Ripple_custom.IsChecked == true)
		{
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			Color perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
			byte r = perkeyRippleColor.R;
			perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
			byte g = perkeyRippleColor.G;
			perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
			byte b = perkeyRippleColor.B;
			Perkey_SetRipponColor(r, g, b);
			UpdateRGBText();
		}
		else
		{
			Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
			Perkey_SetRippon();
		}
	}

	private void RB_Perkey_Ripple_Reactive_Click(object sender, RoutedEventArgs e)
	{
		if (RB_Perkey_Ripple_custom.IsChecked == true)
		{
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			Color perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
			byte r = perkeyRippleColor.R;
			perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
			byte g = perkeyRippleColor.G;
			perkeyRippleColor = Global.KB.GetPerkeyRippleColor();
			byte b = perkeyRippleColor.B;
			Perkey_SetReactiveColor(r, g, b);
			UpdateRGBText();
		}
		else
		{
			Show_Color_KBLayout_UI(ColorUI: false, KBUI: false);
			Perkey_SetReactive();
		}
	}

	private void RB_PerKey_Effect_Fn_Click(object sender, RoutedEventArgs e)
	{
		Load_PerKey_Effet(11);
		UpdateRGBText();
	}

	private void KbSpeedSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (RB_LEDMode_LightBarX170.IsChecked == true)
		{
			X170LB.SetSpeed((byte)((RangeBase)(Slider)sender).Value);
			return;
		}
		Global.KB.Speed = (int)((RangeBase)(Slider)sender).Value;
		Global.KB.SetStatus();
		if (Global.KB.SleepTimerTrigger.Equals(1))
		{
			Global.KB.SetSleepTimerTrigger(Global.KB.SleepTimer);
		}
		else
		{
			Global.KB.SetSleepTimerTriggerOff();
		}
	}

	private void Perkey_SetClearAll()
	{
		Global.KB.PerkeyClearColor();
	}

	private void Perkey_SetBreathRandomColor()
	{
		Global.KB.SetPerkeyBreathRandomMode();
		SetSleepTimer();
	}

	private void Perkey_SetBreathWithColor(byte red, byte green, byte blue)
	{
		Global.KB.SetPerkeyBreathColorMode(Color.FromRgb(red, green, blue));
		SetSleepTimer();
	}

	private void Perkey_SetWave()
	{
		Global.KB.SetPerkeyWaveMode();
		SetSleepTimer();
	}

	private void Perkey_SetBlinkRandomColor()
	{
		Global.KB.SetPerkeyBlinkRandomMode();
		SetSleepTimer();
	}

	private void Perkey_SetBlinkWithColor(byte red, byte green, byte blue)
	{
		Global.KB.SetPerkeyBlinkColorMode(Color.FromRgb(red, green, blue));
		SetSleepTimer();
	}

	private void Perkey_SetStaticKeyRGB(byte key, byte red, byte green, byte blue)
	{
		byte data = (byte)Math.Round(200.0 / 255.0 * (double)(int)blue, 0);
		SetLEDStatus(1, key, red, green, data);
	}

	private void Perkey_SetRippon()
	{
		Global.KB.SetPerkeyRippleRandomMode();
		SetSleepTimer();
	}

	private void Perkey_SetRipponColor(byte red, byte green, byte blue)
	{
		if (red == 0 && green == 0 && blue == 0)
		{
			red = byte.MaxValue;
		}
		Global.KB.SetPerkeyRippleColorMode(Color.FromRgb(red, green, blue));
		SetSleepTimer();
	}

	private void Perkey_SetReactive()
	{
		Global.KB.SetPerkeyFIAORandomMode();
		SetSleepTimer();
	}

	private void Perkey_SetReactiveColor(byte red, byte green, byte blue)
	{
		if (red == 0 && green == 0 && blue == 0)
		{
			red = byte.MaxValue;
		}
		Global.KB.SetPerkeyFIAOColorMode(Color.FromRgb(red, green, blue));
		SetSleepTimer();
	}

	private void Perkey_SetScan()
	{
		Global.KB.SetPerkeyScanMode();
		SetSleepTimer();
	}

	private void Perkey_SetSnake()
	{
		Global.KB.SetPerkeySnakeMode();
		SetSleepTimer();
	}

	private void Perkey_SetRandom()
	{
		Global.KB.SetPerkeyRandomMode();
		SetSleepTimer();
	}

	private void RB_KB_Effect_wave_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.Mode = 4;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_wave).ToString();
	}

	private void RB_KB_Effect_Breath_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.Mode = 2;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Breath).ToString();
	}

	private void RB_KB_Effect_cycle_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.Mode = 3;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_cycle).ToString();
	}

	private void RB_KB_Effect_random_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.Mode = 0;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_random).ToString();
	}

	private void RB_KB_Effect_Flash_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.Mode = 7;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Flash).ToString();
	}

	private void RB_KB_Effect_Dance_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.Mode = 5;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Dance).ToString();
	}

	private void RB_KB_Effect_Tempo_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.Mode = 6;
		Global.KB.SetRGB3ParStatusOn();
		Global.KB.SetMode();
		T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Tempo).ToString();
	}

	private void KbBrightnessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		int num = (int)((RangeBase)(Slider)sender).Value;
		if (KBType == 1)
		{
			Global.KB.SetBrightnessLevel(num);
			if (Global.KB.Status == 0)
			{
				num = 0;
			}
		}
		else if (KBType == 2 || KBType == 6 || KBType == 22)
		{
			if (num == 0)
			{
				Global.KB.SetRGB3ParStatusOn();
				Global.KB.Status = 0;
				Global.KB.SetBrightnessLevel(num);
			}
			else
			{
				Global.KB.SetRGB3ParStatusOn();
				Global.KB.Status = 1;
				Global.KB.SetBrightnessLevel(num);
			}
		}
		else if (KBType == 3)
		{
			Global.KB.SetBrightnessLevel(num);
		}
		grid_KB_ALL.Opacity = 1.0 / ((RangeBase)KbBrightnessSlider).Maximum * (double)num;
		Global.KB.WriteLog("grid_KB_ALL.Opacity", grid_KB_ALL.Opacity.ToString());
		UpdateGridColor(num);
	}

	private void UpdateGridColor(int br)
	{
		if (KBType == 1)
		{
			return;
		}
		if (KBType == 2 || KBType == 6)
		{
			if (!RB_LEDMode_KB_Effect.IsChecked.HasValue || !RB_LEDMode_KB_Effect.IsChecked.Value)
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
					grid_KB_ALL.Opacity = 0.0;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
		}
		else
		{
			if (KBType != 3)
			{
				return;
			}
			if (RB_LEDMode_Perkey_static.IsChecked.HasValue && RB_LEDMode_Perkey_static.IsChecked.Value)
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
				return;
			}
			if (RB_PerKey_Effect_wave.IsChecked.HasValue && RB_PerKey_Effect_wave.IsChecked.Value && RB_Wave_Custom.IsChecked.HasValue && RB_Wave_Custom.IsChecked.Value && Global.KB.SupportPerkeyNewEffect())
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			if (RB_PerKey_Effect_Breath.IsChecked.HasValue && RB_PerKey_Effect_Breath.IsChecked.Value && RB_Perkey_Breath_custom.IsChecked.HasValue && RB_Perkey_Breath_custom.IsChecked.Value)
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			if (RB_PerKey_Effect_Scan.IsChecked.HasValue && RB_PerKey_Effect_Scan.IsChecked.Value && RB_Perkey_Scan_Custom.IsChecked.HasValue && RB_Perkey_Scan_Custom.IsChecked.Value && Global.KB.SupportPerkeyNewEffect())
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			if (RB_PerKey_Effect_Blink.IsChecked.HasValue && RB_PerKey_Effect_Blink.IsChecked.Value && RB_Perkey_Blink_custom.IsChecked.HasValue && RB_Perkey_Blink_custom.IsChecked.Value)
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			if (RB_PerKey_Effect_Random.IsChecked.HasValue && RB_PerKey_Effect_Random.IsChecked.Value && RB_Perkey_Random_custom.IsChecked.HasValue && RB_Perkey_Random_custom.IsChecked.Value && Global.KB.SupportPerkeyNewEffect())
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			if (RB_PerKey_Effect_ripple.IsChecked.HasValue && RB_PerKey_Effect_ripple.IsChecked.Value && RB_Perkey_Ripple_custom.IsChecked.HasValue && RB_Perkey_Ripple_custom.IsChecked.Value)
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			if (RB_PerKey_Effect_Snake.IsChecked.HasValue && RB_PerKey_Effect_Snake.IsChecked.Value && RB_Snake_Custom.IsChecked.HasValue && RB_Snake_Custom.IsChecked.Value && Global.KB.SupportPerkeyNewEffect())
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			if (RB_PerKey_Effect_Fn.IsChecked.HasValue && RB_PerKey_Effect_Fn.IsChecked.Value)
			{
				if (br.Equals(0) || Global.KB.Status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
		}
	}

	public void UpdateKbStatus(int status)
	{
		if (status == 0)
		{
			if (KBType == 2 || KBType == 6 || KBType == 22)
			{
				Global.KB.Status = 0;
			}
			((RangeBase)KbBrightnessSlider).Value = 0.0;
			grid_color.IsEnabled = false;
			return;
		}
		Global.KB.Status = 1;
		status = Global.KB.UpdateCurrentBrightness();
		((RangeBase)KbBrightnessSlider).Value = status;
		if (status == 0 || RB_LEDMode_Perkey_Effect.IsChecked == true || RB_LEDMode_KB_Effect.IsChecked == true)
		{
			grid_color.IsEnabled = false;
		}
		if (RB_LEDMode_Perkey_Effect.IsChecked == true)
		{
			Show_UI_Perkey(2);
		}
		else
		{
			grid_color.IsEnabled = true;
		}
	}

	public void UpdateSpeedUI()
	{
		if (RB_LEDMode_LightBarX170.IsChecked == true)
		{
			((RangeBase)KbSpeedSlider).Value = (int)X170LB.Speed;
		}
		else
		{
			((RangeBase)KbSpeedSlider).Value = Global.KB.Speed;
		}
	}

	public void UpdateBrightnessEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			Global.KB.UpdateCurrentBrightness();
			Global.KB.UpdateCurrentStatus();
			UpdateBrightnessUI();
		}));
	}

	public void UpdateWhiteLEDStatusEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			WhiteLEDStatusEvent = true;
			Global.KB.UpdateCurrentStatus();
			UpdateWhiteLEDStatusUI();
		}));
	}

	private void UpdateWhiteLEDStatusUI()
	{
		int status = Global.KB.Status;
		int brightness = Global.KB.GetBrightness();
		if (status.Equals(0))
		{
			((RangeBase)KbBrightnessSlider).Value = 0.0;
		}
		else
		{
			((RangeBase)KbBrightnessSlider).Value = brightness;
		}
		WhiteLEDStatusEvent = false;
	}

	public void UpdateUILanguageEvent(object sender, EventArgs e)
	{
		Dispatcher.UIThread.Post((Action)(() =>
		{
			Global.MultiLang.Update_lang();
			LoadMultiLang();
			UpdateUIModeText();
			int flexikey_LED_Language = Global.Flexikey_LED_Language;
			switch (KBType)
			{
			case 1:
				UpdateKBLayout(flexikey_LED_Language);
				break;
			case 2:
				UpdateKBLayout(flexikey_LED_Language);
				break;
			case 6:
				UpdateKBLayout(flexikey_LED_Language);
				break;
			case 22:
				UpdateKBLayout(flexikey_LED_Language);
				break;
			}
			grid_KB_ALL.InvalidateVisual();
		}));
	}

	private void UpdateUIModeText()
	{
		switch (Global.KB.Mode)
		{
		case 0:
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_random).ToString();
			break;
		case 2:
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Breath).ToString();
			break;
		case 3:
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_cycle).ToString();
			break;
		case 4:
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_wave).ToString();
			break;
		case 5:
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Dance).ToString();
			break;
		case 6:
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Tempo).ToString();
			break;
		case 7:
			T_KB_Effect_mode.Text = ToolTip.GetTip(RB_KB_Effect_Flash).ToString();
			break;
		case 1:
			break;
		}
	}

	public void UpdateBrightnessUI()
	{
		int status = Global.KB.Status;
		int brightness = Global.KB.GetBrightness();
		if (status.Equals(0))
		{
			((RangeBase)KbBrightnessSlider).Value = 0.0;
		}
		else
		{
			((RangeBase)KbBrightnessSlider).Value = brightness;
		}
		if (RB_LEDMode_Perkey_Effect.IsChecked == true)
		{
			if ((RB_PerKey_Effect_Blink.IsChecked == true && RB_Perkey_Blink_custom.IsChecked == true) || (RB_PerKey_Effect_Breath.IsChecked == true && RB_Perkey_Breath_custom.IsChecked == true) || (RB_PerKey_Effect_ripple.IsChecked == true && RB_Perkey_Ripple_custom.IsChecked == true))
			{
				if (status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			else if (RB_PerKey_Effect_wave.IsChecked.Equals(true) && RB_Wave_Custom.IsChecked.Equals(true))
			{
				if (Global.KB.SupportPerkeyNewEffect())
				{
					if (status.Equals(0))
					{
						grid_color.IsEnabled = false;
					}
					else
					{
						grid_color.IsEnabled = true;
					}
				}
			}
			else if (RB_PerKey_Effect_Scan.IsChecked.Equals(true) && RB_Perkey_Scan_Custom.IsChecked.Equals(true))
			{
				if (Global.KB.SupportPerkeyNewEffect())
				{
					if (status.Equals(0))
					{
						grid_color.IsEnabled = false;
					}
					else
					{
						grid_color.IsEnabled = true;
					}
				}
			}
			else if (RB_PerKey_Effect_Random.IsChecked.Equals(true) && RB_Perkey_Random_custom.IsChecked.Equals(true))
			{
				if (Global.KB.SupportPerkeyNewEffect())
				{
					if (status.Equals(0))
					{
						grid_color.IsEnabled = false;
					}
					else
					{
						grid_color.IsEnabled = true;
					}
				}
			}
			else if (RB_PerKey_Effect_Snake.IsChecked.Equals(true) && RB_Snake_Custom.IsChecked.Equals(true))
			{
				if (Global.KB.SupportPerkeyNewEffect())
				{
					if (status.Equals(0))
					{
						grid_color.IsEnabled = false;
					}
					else
					{
						grid_color.IsEnabled = true;
					}
				}
			}
			else if (RB_PerKey_Effect_Fn.IsChecked.HasValue && RB_PerKey_Effect_Fn.IsChecked.Value)
			{
				if (status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			else
			{
				grid_color.IsEnabled = false;
			}
		}
		else if (RB_LEDMode_Perkey_static.IsChecked == true)
		{
			if (status.Equals(0))
			{
				grid_color.IsEnabled = false;
			}
			else
			{
				grid_color.IsEnabled = true;
			}
		}
		else if (status.Equals(0) || RB_LEDMode_KB_Effect.IsChecked == true)
		{
			grid_color.IsEnabled = false;
		}
		else
		{
			grid_color.IsEnabled = true;
		}
	}

	public void UpdateBrightnessStatus(int status)
	{
		if (KBType == 3)
		{
			if (Global.KB.Status == 0)
			{
				status = 0;
			}
			((RangeBase)KbBrightnessSlider).Value = status;
		}
		else if (KBType == 2 || KBType == 6 || KBType == 22)
		{
			if (Global.KB.Status == 0)
			{
				status = 0;
			}
			switch (status)
			{
			case 0:
				Global.KB.Status = 0;
				((RangeBase)KbBrightnessSlider).Value = 0.0;
				break;
			case 1:
				Global.KB.Status = 1;
				Global.KB.SetBrightnessLevel(1);
				((RangeBase)KbBrightnessSlider).Value = 1.0;
				break;
			case 2:
				Global.KB.Status = 1;
				Global.KB.SetBrightnessLevel(2);
				((RangeBase)KbBrightnessSlider).Value = 2.0;
				break;
			case 3:
				Global.KB.Status = 1;
				Global.KB.SetBrightnessLevel(3);
				((RangeBase)KbBrightnessSlider).Value = 3.0;
				break;
			case 4:
				Global.KB.Status = 1;
				Global.KB.SetBrightnessLevel(4);
				((RangeBase)KbBrightnessSlider).Value = 4.0;
				break;
			}
		}
		else
		{
			((RangeBase)KbBrightnessSlider).Value = status;
		}
		if (RB_LEDMode_Perkey_Effect.IsChecked == true)
		{
			if ((RB_PerKey_Effect_Blink.IsChecked == true && RB_Perkey_Blink_custom.IsChecked == true) || (RB_PerKey_Effect_Breath.IsChecked == true && RB_Perkey_Breath_custom.IsChecked == true) || (RB_PerKey_Effect_ripple.IsChecked == true && RB_Perkey_Ripple_custom.IsChecked == true))
			{
				if (status.Equals(0))
				{
					grid_color.IsEnabled = false;
				}
				else
				{
					grid_color.IsEnabled = true;
				}
			}
			else
			{
				grid_color.IsEnabled = false;
			}
		}
		else if (RB_LEDMode_Perkey_static.IsChecked == true)
		{
			if (status.Equals(0))
			{
				grid_color.IsEnabled = false;
			}
			else
			{
				grid_color.IsEnabled = true;
			}
		}
		else if (status.Equals(0) || RB_LEDMode_KB_Effect.IsChecked == true)
		{
			grid_color.IsEnabled = false;
		}
		else
		{
			grid_color.IsEnabled = true;
		}
	}

	public void LoadKBSleepTimer()
	{
		UpdateSleepTimerUI();
	}

	public void UpdateSleepTimerUI()
	{
		if (Global.KB.SleepTimerTrigger == 0)
		{
			UpdateSleepStatus(0);
		}
		else
		{
			UpdateSleepStatus(Global.KB.SleepTimer);
		}
	}

	private void UpdateSleepStatus(int kb_sleep_timer)
	{
		if (kb_sleep_timer.Equals(0))
		{
			RB_KBSleepTimer_OFF.IsChecked = true;
			Canvas_sleep_timer_btn.IsVisible = false;
		}
		else if (kb_sleep_timer < 7200)
		{
			RB_KBSleepTimer_ON.IsChecked = true;
			Canvas_sleep_timer_btn.IsVisible = true;
			SetUISleepTimer(kb_sleep_timer / 60 / 60, kb_sleep_timer / 60 % 60, kb_sleep_timer % 60);
			UpdateSleepSecCbo();
		}
		else
		{
			RB_KBSleepTimer_ON.IsChecked = true;
			Canvas_sleep_timer_btn.IsVisible = true;
			SetUISleepTimer(0, 0, 30);
			UpdateSleepSecCbo();
		}
	}

	private void SetUISleepTimer(int hh, int mm, int ss)
	{
		sleep_timer_init = false;
		if (hh.Equals(0) && mm.Equals(0) && ss < 5)
		{
			Global.KB.SleepTimer = 5;
			SetUISleepTimer(0, 0, 5);
		}
		else
		{
			((SelectingItemsControl)SleepTimerHourCustom).SelectedItem = hh;
			((SelectingItemsControl)SleepTimerMinCustom).SelectedItem = mm;
			((SelectingItemsControl)SleepTimerSecCustom).SelectedItem = ss;
		}
		sleep_timer_init = true;
	}

	private void RB_KBSleepTimer_OFF_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.SleepTimerTrigger = 0;
		Canvas_sleep_timer_btn.IsVisible = false;
		Global.KB.SetSleepTimerTriggerOff();
	}

	private void RB_KBSleepTimer_ON_Click(object sender, RoutedEventArgs e)
	{
		Global.KB.SleepTimerTrigger = 1;
		Canvas_sleep_timer_btn.IsVisible = true;
		SetUISleepTimer(Global.KB.SleepTimer / 60 / 60, Global.KB.SleepTimer / 60 % 60, Global.KB.SleepTimer % 60);
		Global.KB.SetSleepTimerTrigger(Global.KB.SleepTimer);
	}

	private void SleepTimerCustom_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (!sleep_timer_init)
		{
			return;
		}
		_ = (Func<int, byte>)((int x) => (byte)(x / 256));
		_ = (Func<int, byte>)((int x) => (byte)(x % 256));
		if (!((SelectingItemsControl)SleepTimerSecCustom).SelectedIndex.Equals(-1) && !((SelectingItemsControl)SleepTimerMinCustom).SelectedIndex.Equals(-1) && !((SelectingItemsControl)SleepTimerHourCustom).SelectedIndex.Equals(-1))
		{
			int num = (int)((SelectingItemsControl)SleepTimerHourCustom).SelectedItem;
			int num2 = (int)((SelectingItemsControl)SleepTimerMinCustom).SelectedItem;
			int num3 = (int)((SelectingItemsControl)SleepTimerSecCustom).SelectedItem;
			if (num.Equals(0) && num2.Equals(0) && num3 < 5)
			{
				((SelectingItemsControl)SleepTimerSecCustom).SelectedItem = 5;
			}
			else
			{
				int sleepTimer = Convert.ToInt32(num3 + num2 * 60 + num * 3600);
				Global.KB.SleepTimer = sleepTimer;
				Global.KB.SetSleepTimerTrigger(Global.KB.SleepTimer);
			}
			UpdateSleepSecCbo();
		}
	}

	private void SetSleepTimer()
	{
		_ = (Func<int, byte>)((int x) => (byte)(x / 256));
		_ = (Func<int, byte>)((int x) => (byte)(x % 256));
		if (!((SelectingItemsControl)SleepTimerSecCustom).SelectedIndex.Equals(-1) && !((SelectingItemsControl)SleepTimerMinCustom).SelectedIndex.Equals(-1) && !((SelectingItemsControl)SleepTimerHourCustom).SelectedIndex.Equals(-1) && RB_KBSleepTimer_OFF.IsChecked != true)
		{
			int num = (int)((SelectingItemsControl)SleepTimerHourCustom).SelectedItem;
			int num2 = (int)((SelectingItemsControl)SleepTimerMinCustom).SelectedItem;
			int num3 = (int)((SelectingItemsControl)SleepTimerSecCustom).SelectedItem;
			if (num.Equals(0) && num2.Equals(0) && num3 < 5)
			{
				((SelectingItemsControl)SleepTimerSecCustom).SelectedItem = 5;
				return;
			}
			int sleepTimer = Convert.ToInt32(num3 + num2 * 60 + num * 3600);
			Global.KB.SleepTimer = sleepTimer;
			Global.KB.SetSleepTimerTrigger(Global.KB.SleepTimer);
		}
	}

	private void UpdateSleepSecCbo()
	{
		if (KBType == 0)
		{
			return;
		}
		sleep_timer_init = false;
		if (((int)((SelectingItemsControl)SleepTimerHourCustom).SelectedValue).Equals(0) && ((int)((SelectingItemsControl)SleepTimerMinCustom).SelectedValue).Equals(0) && ((ItemsControl)SleepTimerSecCustom).Items.Count > 55)
		{
			object selectedItem = ((SelectingItemsControl)SleepTimerSecCustom).SelectedItem;
			((ItemsControl)SleepTimerSecCustom).ItemsSource = null;
			List<int> list = new List<int>();
			for (int i = 5; i < 60; i++)
			{
				list.Add(i);
			}
			((ItemsControl)SleepTimerSecCustom).ItemsSource = list;
			((SelectingItemsControl)SleepTimerSecCustom).SelectedItem = selectedItem;
		}
		else if (((int)((SelectingItemsControl)SleepTimerHourCustom).SelectedValue != 0 || (int)((SelectingItemsControl)SleepTimerMinCustom).SelectedValue != 0) && ((ItemsControl)SleepTimerSecCustom).Items.Count < 60)
		{
			object selectedItem2 = ((SelectingItemsControl)SleepTimerSecCustom).SelectedItem;
			((ItemsControl)SleepTimerSecCustom).ItemsSource = null;
			List<int> list2 = new List<int>();
			for (int j = 0; j < 60; j++)
			{
				list2.Add(j);
			}
			((ItemsControl)SleepTimerSecCustom).ItemsSource = list2;
			((SelectingItemsControl)SleepTimerSecCustom).SelectedItem = selectedItem2;
		}
		sleep_timer_init = true;
	}

	private void RB_KB_Partial_left_Click(object sender, RoutedEventArgs e)
	{
		UpdateRGBText();
	}

	private void RB_KB_Partial_Mid_Click(object sender, RoutedEventArgs e)
	{
		UpdateRGBText();
	}

	private void RB_KB_Partial_right_Click(object sender, RoutedEventArgs e)
	{
		UpdateRGBText();
	}

	private void BTN_color_picker_MouseLeave(object sender, PointerEventArgs e)
	{
		UpdateRGBText();
	}

	private void RB_KB_Partial_MouseLeave(object sender, PointerEventArgs e)
	{
		grid_KB_ALL.InvalidateVisual();
	}

	private void RB_KB_Partial_MouseEnter(object sender, PointerEventArgs e)
	{
		grid_KB_ALL.InvalidateVisual();
	}

	private void main_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		if (KBType != 3 || RB_LEDMode_Perkey_static.IsChecked == false)
		{
			return;
		}
		start_point = ((PointerEventArgs)e).GetPosition(this);
		myRect.Stroke = (Brush)(object)Brushes.White;
		Rectangle val = myRect;
		var val2 = new Avalonia.Collections.AvaloniaList<double> { 2.0 };
		val.StrokeDashArray = val2;
		myRect.Fill = (Brush)(object)Brushes.Transparent;
		myRect.HorizontalAlignment = (HorizontalAlignment)0;
		myRect.VerticalAlignment = (VerticalAlignment)0;
		myRect.Margin = new Thickness(start_point.X, start_point.Y, 0.0, 0.0);
		myRect.Height = 0.0;
		myRect.Width = 0.0;
		if (!main.Children.Contains((Control)(object)myRect))
		{
			main.Children.Add((Control)(object)myRect);
		}
		if (!e.KeyModifiers.HasFlag(KeyModifiers.Control))
		{
			IEnumerable<KeyBoardLocation> source = kbl.Where((KeyBoardLocation tmp) =>
			{
				KeyBoardLocation keyBoardLocation = tmp;
				if (keyBoardLocation.LeftPoint < start_point.X)
				{
					keyBoardLocation = tmp;
					if (keyBoardLocation.RightPoint > start_point.X)
					{
						keyBoardLocation = tmp;
						if (keyBoardLocation.TopPoint < start_point.Y)
						{
							keyBoardLocation = tmp;
							return keyBoardLocation.BottomPoint > start_point.Y;
						}
						return false;
					}
				}
				return false;
			});
			if (source.Count() != 0)
			{
				foreach (CheckBox child in canvas_kb.Children)
				{
					CheckBox val3 = child;
					if (!source.First().Name.Equals(val3.Name))
					{
						val3.IsChecked = false;
					}
				}
			}
			else
			{
				foreach (CheckBox child2 in canvas_kb.Children)
				{
					child2.IsChecked = false;
				}
			}
		}
		else
		{
			foreach (CheckBox child3 in canvas_kb.Children)
			{
				CheckBox cb = child3;
				if (cb.IsChecked == true)
				{
					int num = kbl.FindIndex((KeyBoardLocation x) => x.Name.Equals(cb.Name));
					if (num != -1)
					{
						kbl_keep.Add(kbl[num]);
					}
				}
			}
		}
		is_mouse_trigger = true;
		e.Pointer.Capture(main);
	}

	private void main_MouseMove(object sender, PointerEventArgs e)
	{
		if (KBType != 3 || RB_LEDMode_Perkey_static.IsChecked == false || !e.GetCurrentPoint(this).Properties.IsLeftButtonPressed || !is_mouse_trigger)
		{
			return;
		}
		end_point = e.GetPosition(this);
		double num = end_point.X - start_point.X;
		double num2 = end_point.Y - start_point.Y;
		if (num >= 0.0 && num2 >= 0.0)
		{
			myRect.Margin = new Thickness(start_point.X, start_point.Y, 0.0, 0.0);
			myRect.Width = num;
			myRect.Height = num2;
		}
		else if (num >= 0.0 && num2 < 0.0)
		{
			myRect.Margin = new Thickness(start_point.X, end_point.Y, 0.0, 0.0);
			myRect.Width = num;
			myRect.Height = 0.0 - num2;
		}
		else if (num < 0.0 && num2 >= 0.0)
		{
			myRect.Margin = new Thickness(end_point.X, start_point.Y, 0.0, 0.0);
			myRect.Width = 0.0 - num;
			myRect.Height = num2;
		}
		else if (num < 0.0 && num2 < 0.0)
		{
			myRect.Margin = new Thickness(end_point.X, end_point.Y, 0.0, 0.0);
			myRect.Width = 0.0 - num;
			myRect.Height = 0.0 - num2;
		}
		Point tmp_left_top;
		Point tmp_right_bottom;
		if (num >= 0.0 && num2 >= 0.0)
		{
			tmp_left_top = start_point;
			tmp_right_bottom = end_point;
		}
		else if (num >= 0.0 && num2 < 0.0)
		{
			tmp_left_top = new Point(start_point.X, end_point.Y);
			tmp_right_bottom = new Point(end_point.X, start_point.Y);
		}
		else if (num < 0.0 && num2 >= 0.0)
		{
			tmp_left_top = new Point(end_point.X, start_point.Y);
			tmp_right_bottom = new Point(start_point.X, end_point.Y);
		}
		else
		{
			tmp_left_top = end_point;
			tmp_right_bottom = start_point;
		}
		IEnumerable<KeyBoardLocation> enumerable = kbl.Where((KeyBoardLocation tmp) =>
		{
			KeyBoardLocation keyBoardLocation = tmp;
			int num3;
			if (keyBoardLocation.LeftPoint < tmp_left_top.X)
			{
				keyBoardLocation = tmp;
				num3 = ((keyBoardLocation.RightPoint < tmp_left_top.X) ? 1 : 0);
			}
			else
			{
				num3 = 0;
			}
			keyBoardLocation = tmp;
			int num4;
			if (keyBoardLocation.LeftPoint > tmp_right_bottom.X)
			{
				keyBoardLocation = tmp;
				num4 = ((keyBoardLocation.RightPoint > tmp_right_bottom.X) ? 1 : 0);
			}
			else
			{
				num4 = 0;
			}
			int num5 = num3 | num4;
			keyBoardLocation = tmp;
			int num6;
			if (keyBoardLocation.TopPoint < tmp_left_top.Y)
			{
				keyBoardLocation = tmp;
				num6 = ((keyBoardLocation.BottomPoint < tmp_left_top.Y) ? 1 : 0);
			}
			else
			{
				num6 = 0;
			}
			int num7 = num5 | num6;
			keyBoardLocation = tmp;
			int num8;
			if (keyBoardLocation.TopPoint > tmp_right_bottom.Y)
			{
				keyBoardLocation = tmp;
				num8 = ((keyBoardLocation.BottomPoint > tmp_right_bottom.Y) ? 1 : 0);
			}
			else
			{
				num8 = 0;
			}
			return (num7 | num8) == 0;
		});
		if (kbl_last_time.Count.Equals(0))
		{
			foreach (KeyBoardLocation item in enumerable)
			{
				foreach (CheckBox child in canvas_kb.Children)
				{
					CheckBox val = child;
					if (item.Name.Equals(val.Name))
					{
						val.IsChecked = true;
						break;
					}
				}
			}
		}
		else
		{
			foreach (KeyBoardLocation item2 in enumerable)
			{
				if (kbl_last_time.Contains(item2))
				{
					kbl_last_time.Remove(item2);
					continue;
				}
				foreach (CheckBox child2 in canvas_kb.Children)
				{
					CheckBox val2 = child2;
					if (item2.Name.Equals(val2.Name))
					{
						val2.IsChecked = true;
						break;
					}
				}
			}
			if (kbl_last_time.Count != 0)
			{
				foreach (KeyBoardLocation item3 in kbl_last_time)
				{
					bool flag = false;
					foreach (KeyBoardLocation item4 in kbl_keep)
					{
						if (item4.Name.Equals(item3.Name))
						{
							flag = true;
						}
					}
					if (flag)
					{
						continue;
					}
					foreach (CheckBox child3 in canvas_kb.Children)
					{
						CheckBox val3 = child3;
						if (item3.Name.Equals(val3.Name))
						{
							val3.IsChecked = false;
							break;
						}
					}
				}
			}
		}
		kbl_last_time.Clear();
		foreach (KeyBoardLocation item5 in enumerable)
		{
			kbl_last_time.Add(item5);
		}
	}

	private void main_PreviewMouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		if (KBType == 3 && RB_LEDMode_Perkey_static.IsChecked != false)
		{
			if (main.Children.Contains((Control)(object)myRect))
			{
				main.Children.Remove((Control)(object)myRect);
			}
			grid_KB_ALL.InvalidateVisual();
			is_mouse_trigger = false;
			kbl_last_time.Clear();
			kbl_keep.Clear();
			(e.Pointer.Captured == main ? e.Pointer : null)?.Capture(null);
		}
	}

	private void templateRoot_PreviewMouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		main_MouseLeftButtonDown(sender, e);
	}

	private void RB_LEDMode_LOGO_Click(object sender, RoutedEventArgs e)
	{
		switch (KBType)
		{
		case 2:
			grid_KB_ALL.IsVisible = false;
			grid_KB_Partial.IsVisible = false;
			Canvas_KB_Effect.IsVisible = false;
			break;
		case 3:
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			grid_LED_Speed.IsVisible = false;
			Canvas_Perkey_Effect.IsVisible = false;
			break;
		}
		grid_KB_Brightness.IsVisible = false;
		grid_KB_Sleep_Timer.IsVisible = false;
		grid_color.IsEnabled = true;
		BTN_color_picker.IsEnabled = false;
		canvas_KB_layout.IsVisible = false;
		grid_perkey_static.IsVisible = false;
		GridLogo.IsVisible = true;
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsVisible = false;
		}
		UpdateRGBText();
	}

	private void RB_LEDMode_LOGO_Unchecked(object sender, RoutedEventArgs e)
	{
		grid_KB_Brightness.IsVisible = true;
		grid_KB_Sleep_Timer.IsVisible = true;
		BTN_color_picker.IsEnabled = true;
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsVisible = true;
		}
		GridLogo.IsVisible = false;
		canvas_KB_layout.IsVisible = true;
	}

	private void RB_LEDMode_LightBar_Click(object sender, RoutedEventArgs e)
	{
		switch (KBType)
		{
		case 2:
			grid_KB_ALL.IsVisible = false;
			grid_KB_Partial.IsVisible = false;
			Canvas_KB_Effect.IsVisible = false;
			break;
		case 3:
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			grid_LED_Speed.IsVisible = false;
			Canvas_Perkey_Effect.IsVisible = false;
			break;
		}
		grid_KB_Brightness.IsVisible = false;
		grid_KB_Sleep_Timer.IsVisible = false;
		grid_color.IsEnabled = true;
		canvas_KB_layout.IsVisible = false;
		grid_perkey_static.IsVisible = false;
		GridLightBar.IsVisible = true;
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsVisible = false;
		}
		UpdateRGBText();
	}

	private void RB_LEDMode_LightBar_Unchecked(object sender, RoutedEventArgs e)
	{
		grid_KB_Brightness.IsVisible = true;
		grid_KB_Sleep_Timer.IsVisible = true;
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsVisible = true;
		}
		GridLightBar.IsVisible = false;
		canvas_KB_layout.IsVisible = true;
	}

	private void DisableLightBar()
	{
		if (Global.KB.SupportLightbar())
		{
			RB_LEDMode_LightBar.Opacity = 0.3;
			RB_LEDMode_LightBar.IsEnabled = false;
		}
	}

	private void EnableLightBar()
	{
		if (Global.KB.SupportLightbar())
		{
			RB_LEDMode_LightBar.Opacity = 1.0;
			RB_LEDMode_LightBar.IsEnabled = true;
		}
	}

	private void ChkKBSelectBootEffect_Click(object sender, RoutedEventArgs e)
	{
		if (ChkKBSelectBootEffect.IsChecked == true)
		{
			Global.KB.SetOverrideBootEffect(1);
		}
		else
		{
			Global.KB.SetOverrideBootEffect(0);
		}
	}

	private void main_Loaded(object sender, RoutedEventArgs e)
	{
		userControl.BorderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, (byte)0, (byte)210));
		System_ManufacturerName = Smbios.ManufacturerName();
		Init_UI();
		Open_UI();
	}

	private void RB_LEDMode_Perkey_static_Checked(object sender, RoutedEventArgs e)
	{
		try
		{
			BTN_color_plate_black.BorderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)0, (byte)0, (byte)0));
		}
		catch
		{
		}
	}

	private void RB_LEDMode_Perkey_static_Unchecked(object sender, RoutedEventArgs e)
	{
		try
		{
			Color[] color = new Color[10];
			Global.KB.GetPalette(ref color);
			if (color[9].R != 0 && color[9].G != 0 && color[9].B != 0)
			{
				BTN_color_plate_black.BorderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, color[9].R, color[9].G, color[9].B));
			}
			else
			{
				BTN_color_plate_black.BorderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)192, (byte)192, (byte)192));
			}
		}
		catch
		{
		}
	}

	private void RB_LEDMode_LightBarX170_Click(object sender, RoutedEventArgs e)
	{
		switch (KBType)
		{
		case 2:
			grid_KB_ALL.IsVisible = false;
			grid_KB_Partial.IsVisible = false;
			Canvas_KB_Effect.IsVisible = false;
			break;
		case 3:
			Show_Color_KBLayout_UI(ColorUI: true, KBUI: false);
			Canvas_Perkey_Effect.IsVisible = false;
			break;
		}
		grid_LED_Speed.IsVisible = true;
		SP_BR.Children.Clear();
		SP_BR.Children.Add((Control)(object)SP_BR_X170LB);
		grid_KB_Sleep_Timer.IsVisible = false;
		canvas_KB_layout.IsVisible = false;
		grid_perkey_static.IsVisible = false;
		GridLightBar_X170.IsVisible = true;
		ChkKBSelectBootEffect.IsVisible = false;
		Canvas_X170LightBar_Effect.IsVisible = true;
		grid_color.IsEnabled = false;
		switch (X170LB.Mode)
		{
		case 176:
			RB_x170_Effect_Static.IsChecked = true;
			Update_LightBar_UI(176, X170LB.Static_R, X170LB.Static_G, X170LB.Static_B);
			break;
		case 178:
			RB_x170_Effect_Breath.IsChecked = true;
			if (X170LB.BreathMode == 0)
			{
				RB_X170_Breath_random.IsChecked = true;
				Update_LightBar_UI_OFF(178);
			}
			else
			{
				RB_X170_Breath_custom.IsChecked = true;
				Update_LightBar_UI(178, X170LB.Breath_R, X170LB.Breath_G, X170LB.Breath_B);
			}
			break;
		case 179:
			RB_x170_Effect_Blink.IsChecked = true;
			if (X170LB.BlinkMode == 0)
			{
				RB_X170_Blink_random.IsChecked = true;
				Update_LightBar_UI_OFF(179);
			}
			else
			{
				RB_X170_Blink_custom.IsChecked = true;
				Update_LightBar_UI(179, X170LB.Blink_R, X170LB.Blink_G, X170LB.Blink_B);
			}
			break;
		case 180:
			RB_x170_Effect_ColorCycle.IsChecked = true;
			break;
		case 181:
			RB_x170_Effect_Meteor.IsChecked = true;
			if (X170LB.MeteorMode == 0)
			{
				RB_X170_Meteor_random.IsChecked = true;
				Update_LightBar_UI_OFF(181);
			}
			else
			{
				RB_X170_Meteor_custom.IsChecked = true;
				Update_LightBar_UI(181, X170LB.Meteor_R, X170LB.Meteor_G, X170LB.Meteor_B);
			}
			break;
		case 182:
			RB_x170_Effect_wave.IsChecked = true;
			break;
		case 183:
			RB_x170_Effect_ripple.IsChecked = true;
			break;
		case 184:
			RB_x170_Effect_Random.IsChecked = true;
			break;
		default:
			RB_x170_Effect_Meteor.IsChecked = true;
			break;
		}
		UpdateSpeedUI();
		if (X170LB.LED_ON == 0)
		{
			((RangeBase)X170LB_BrightnessSlider).Value = 0.0;
		}
		else
		{
			((RangeBase)X170LB_BrightnessSlider).Value = (int)X170LB.Brightness;
		}
		R_color_plate_layer.IsVisible = false;
		BTN_color_plate_black.IsVisible = false;
		BTN_color_plate_white.IsVisible = false;
		grud_9_color_picker.IsVisible = true;
		grud_10_color_picker.IsVisible = false;
		grid_color_picker.IsVisible = false;
		grid_color_text.IsVisible = false;
	}

	private void RB_LEDMode_LightBarX170_Unchecked(object sender, RoutedEventArgs e)
	{
		SP_BR.Children.Clear();
		SP_BR.Children.Add((Control)(object)SP_BR_KB);
		grid_KB_Sleep_Timer.IsVisible = true;
		if (Global.KB.SupportOverrideBootEffect())
		{
			ChkKBSelectBootEffect.IsVisible = true;
		}
		GridLightBar_X170.IsVisible = false;
		canvas_KB_layout.IsVisible = true;
		Canvas_X170LightBar_Effect.IsVisible = false;
		R_color_plate_layer.IsVisible = true;
		BTN_color_plate_black.IsVisible = true;
		BTN_color_plate_white.IsVisible = true;
		grud_9_color_picker.IsVisible = false;
		grud_10_color_picker.IsVisible = true;
		grid_color_picker.IsVisible = true;
		grid_color_text.IsVisible = true;
		UpdateSpeedUI();
	}

	private void RB_x170_Effect_Static_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			Update_LightBar_UI(176, X170LB.Static_R, X170LB.Static_G, X170LB.Static_B);
		}
	}

	private void RB_x170_Effect_Breath_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			if (X170LB.BreathMode == 0)
			{
				RB_X170_Breath_random.IsChecked = true;
				Update_LightBar_UI_OFF(178);
			}
			else
			{
				RB_X170_Breath_custom.IsChecked = true;
				Update_LightBar_UI(178, X170LB.Breath_R, X170LB.Breath_G, X170LB.Breath_B);
			}
		}
	}

	private void RB_x170_Effect_Blink_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			if (X170LB.BlinkMode == 0)
			{
				RB_X170_Blink_random.IsChecked = true;
				Update_LightBar_UI_OFF(179);
			}
			else
			{
				RB_X170_Blink_custom.IsChecked = true;
				Update_LightBar_UI(179, X170LB.Blink_R, X170LB.Blink_G, X170LB.Blink_B);
			}
		}
	}

	private void RB_x170_Effect_ColorCycle_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			Update_LightBar_UI_OFF(180);
		}
	}

	private void RB_x170_Effect_Meteor_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			if (X170LB.MeteorMode == 0)
			{
				RB_X170_Meteor_random.IsChecked = true;
				Update_LightBar_UI_OFF(181);
			}
			else
			{
				RB_X170_Meteor_custom.IsChecked = true;
				Update_LightBar_UI(181, X170LB.Meteor_R, X170LB.Meteor_G, X170LB.Meteor_B);
			}
		}
	}

	private void RB_x170_Effect_wave_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			Update_LightBar_UI_OFF(182);
		}
	}

	private void RB_x170_Effect_ripple_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			Update_LightBar_UI_OFF(183);
		}
	}

	private void RB_x170_Effect_Random_Checked(object sender, RoutedEventArgs e)
	{
		if (x170LightBar_InitStatus)
		{
			Update_LightBar_UI_OFF(184);
		}
	}

	private void RB_X170_Breath_custom_Checked(object sender, RoutedEventArgs e)
	{
		Update_LightBar_UI(178, X170LB.Breath_R, X170LB.Breath_G, X170LB.Breath_B);
	}

	private void RB_X170_Breath_random_Checked(object sender, RoutedEventArgs e)
	{
		Update_LightBar_UI_OFF(178);
	}

	private void RB_X170_Blink_custom_Checked(object sender, RoutedEventArgs e)
	{
		Update_LightBar_UI(179, X170LB.Blink_R, X170LB.Blink_G, X170LB.Blink_B);
	}

	private void RB_X170_Blink_random_Checked(object sender, RoutedEventArgs e)
	{
		Update_LightBar_UI_OFF(179);
	}

	private void RB_X170_Meteor_custom_Checked(object sender, RoutedEventArgs e)
	{
		Update_LightBar_UI(181, X170LB.Meteor_R, X170LB.Meteor_G, X170LB.Meteor_B);
	}

	private void RB_X170_Meteor_random_Checked(object sender, RoutedEventArgs e)
	{
		Update_LightBar_UI_OFF(181);
	}

	private void X170LB_BrightnessSlider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (!x170LightBar_InitStatus)
		{
			return;
		}
		byte b = (byte)((RangeBase)(Slider)sender).Value;
		if (b == 0)
		{
			X170LB.SetLED_OFF();
			Canvas_X170LightBar_Effect.IsEnabled = false;
			Canvas_X170LightBar_Effect.Opacity = 0.3;
			grid_color.IsEnabled = false;
			return;
		}
		X170LB.SetBrightness(b);
		Canvas_X170LightBar_Effect.IsEnabled = true;
		Canvas_X170LightBar_Effect.Opacity = 1.0;
		if (RB_x170_Effect_Static.IsChecked == true || (RB_x170_Effect_Meteor.IsChecked == true && RB_X170_Meteor_custom.IsChecked == true) || (RB_x170_Effect_Breath.IsChecked == true && RB_X170_Breath_custom.IsChecked == true) || (RB_x170_Effect_Blink.IsChecked == true && RB_X170_Blink_custom.IsChecked == true))
		{
			grid_color.IsEnabled = true;
		}
	}

	private void Update_LightBar_UI(byte cmd, byte R, byte G, byte B)
	{
		grid_color.IsEnabled = true;
		UpdateRGBText();
		X170LB.Set_CustomRGB(cmd, R, G, B);
		Show_LightBar_x170.Fill = new SolidColorBrush(Color.FromRgb(R, G, B));
	}

	private void Update_LightBar_UI_OFF(byte cmd)
	{
		grid_color.IsEnabled = false;
		X170LB.Set_Mode(cmd);
		Show_LightBar_x170.Fill = new SolidColorBrush(Color.FromRgb((byte)0, (byte)0, (byte)0));
	}

	private void BTN_color_plate_sky_blue1_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_sky_blue1.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_sky_blue1.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_sky_blue1.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_bule1_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_bule1.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_bule1.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_bule1.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_purple1_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_purple1.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_purple1.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_purple1.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_pink1_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_pink1.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_pink1.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_pink1.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void BTN_color_plate_9_MouseMove(object sender, PointerEventArgs e)
	{
		txt_color_isKeyboardFocus = false;
		ComboBox val = txt_color_red;
		Color val2 = BrushColor(BTN_color_plate_9.BorderBrush);
		val.Text = val2.R.ToString();
		ComboBox val3 = txt_color_green;
		val2 = BrushColor(BTN_color_plate_9.BorderBrush);
		SetCbText(val3, val2.G.ToString());
		ComboBox val4 = txt_color_blue;
		val2 = BrushColor(BTN_color_plate_9.BorderBrush);
		SetCbText(val4, val2.B.ToString());
	}

	private void ListView_SelectionChanged()
	{
	}

	private void Get_PanelSize()
	{
	}
}
