using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CCenter.Platform;
using CC.Compat;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using CC.FlexiKey;
using CC.UControl;
using Clevo;
using Test1;

namespace CC;

public partial class Page_FlexiKey : UserControl
{
	[Flags]
	private enum ClevoMouse
	{
		CB_MouseMouse_Left = 1,
		CB_MouseMouse_Right = 4,
		CB_MouseMouse_Mid = 0x10,
		CB_MouseMouse_Backward = 0x40,
		CB_MouseMouse_Foward = 0x100
	}

	private Set2Driver setkb;

	private Class1 myKB_Hook;

	private List<ProfileInfo> _profileInfo;

	private List<KeyInfo> myKeyInfo = new List<KeyInfo>();

	private List<UserControl_ExpressKey_groups> list_KeyGroups = new List<UserControl_ExpressKey_groups>();

	private byte CustomId;

	private bool IsNumPad3Col;

	private bool Is14Inch;

	private bool bIsStatisticsSaved;

	private bool InitOK;

	private int iNowIndex;

	private int KeyInfo_CurrentIndex = -1;

	private int BackUp_group = -1;

	private string xmlFileName = "profile.xml";

	private string _strProfile = "Profile";

	private string _strProfileLimitNotice = "The maximum number of profile is 12.";

	private string _strComboKeys = "Flexikey®";

	private string lang_LastOneProfile = "Last one profile";

	public bool InitialFlag;

	private bool isCustomOKClick;

	private bool isStatisticsEnabled;

	public int ClevoID;

	public int NV40ME_MB = 16408;

	private EventLogStub outlog = new EventLogStub();

	private int KeyboardStatus = -1;

	private int MouseStatus = -1;

	private string profile_ImagePath = "";

	private const int MAX_KEY_NUM = 16;

	private int key_sum;

	private int key_make_num;

	private DispatcherTimer statistics_timer = new DispatcherTimer
	{
		Interval = TimeSpan.FromSeconds(1.0)
	};

	private List<KbStaClass> sta = new List<KbStaClass>();

	private int sta_sec;

	private int sta_time_m;

	private int sta_time_h;

	public static int GetPrivateProfileString(string section, string key, string def, StringBuilder retStr, int bufferSize, string filePath) => Ini.GetPrivateProfileString(section, key, def, retStr, bufferSize, filePath);
	private static long WritePrivateProfileString(string Section, string Key, string Value, string FilePath) => Ini.WritePrivateProfileString(Section, Key, Value, FilePath) ? 1 : 0;
	/// <summary>GetProductdll.dll (PCI product id) - no Linux equivalent, returns 0 as the DLL does when no product is found.</summary>
	public static int GetProductID_PCI() => 0;

	public Page_FlexiKey()
	{
		InitializeComponent();
		// WPF raised Checked while loading the XAML for the three radio buttons that start checked; Avalonia does not.
		RBTN_KBStatus_disable_Checked(RBTN_KBStatus_disable, null);
		RBTN_MouseStatus_disable_Checked(RBTN_MouseStatus_disable, null);
		RBTN_MacroKB_Checked(RBTN_MacroKB, null);
	}

	public void Open_UI()
	{
		main.IsVisible = true;
		UpdateFlexikeyStatus();
		updateKeyboard_Mouse();
		UpdateUI_KB_Mouse_Status();
		Global.FlexikeyInitialFlag = false;
	}

	public void Close_UI()
	{
		main.IsVisible = false;
		GC.Collect();
		GC.WaitForPendingFinalizers();
		if (InitOK)
		{
			Stop_HookFunction();
			Start_Statistics_Function(bStart: false);
		}
		BTN_Restore_statistics_Click(null, null);
	}

	private int GetProductIDFromInsydeBuffer()
	{
		int page = 0;
		int offset = 0;
		int length = 2;
		int result = 0;
		try
		{
			byte[] array = DCHU.ReadAppSettings(page, offset, length);
			byte b = array[0];
			array[0] = array[1];
			array[1] = b;
			result = BitConverter.ToInt16(array, 0);
		}
		catch
		{
			MessageBox.Show("Error");
		}
		return result;
	}

	public void Init()
	{
		string subkey = "Software\\ControlCenter2.0\\FlexiKey";
		myKeyInfo = Init_KeyInfo(myKeyInfo, KeyGlobal.DeviceType.none);
		setkb = new Set2Driver();
		myKB_Hook = new Class1();
		KeyGlobal.hookKeysTable = new HookKeysTable();
		myKB_Hook.KeyPress += MyKB_Hook_KeyPress;
		statistics_timer.Tick += Statistics_timer_Tick;
		string text = CcPaths.Map("C:\\ComboKey");
		if (!Directory.Exists(text))
		{
			Directory.CreateDirectory(text);
		}
		xmlFileName = System.IO.Path.Combine(text, xmlFileName);
		_profileInfo = new List<ProfileInfo>();
		if (!LoadXml(xmlFileName))
		{
			AddProfile();
			setkb.SetKbCleanFilter();
			setkb.SetKbDisableFilter();
			setkb.SetMouseCleanFilter();
			setkb.SetMouseDisableFilter();
		}
		else
		{
			iNowIndex = _profileInfo.FindIndex((ProfileInfo _data) => _data.IsSelected.Equals(obj: true));
			Update_Profile_List();
		}
		InitOK = true;
		ClevoID = GetProductIDFromInsydeBuffer();
		if (ClevoID == 0)
		{
			ClevoID = GetProductID_PCI();
		}
		GetCustomIdAndIsNumPad3Col();
		GetPanelSize();
		LoadMultiLang();
		LoadKeyboardMultiLang();
		InitialEventLog();
	}

	private void GetPanelSize()
	{
		byte[] array = new byte[256];
		byte[] o_buffer = new byte[256];
		int num = 0;
		int num2 = 0;
		double num3 = 15.5;
		array[0] = 12;
		DCHU.SetWMIPackageEx(4, array, ref o_buffer);
		num = Convert.ToInt32(o_buffer[21]);
		num2 = Convert.ToInt32(o_buffer[22]);
		if (num != 0 && num != 0)
		{
			num3 = Math.Sqrt(Math.Pow(num, 2.0) + Math.Pow(num2, 2.0)) / 2.54;
		}
		if (num3 < 15.0)
		{
			Is14Inch = true;
		}
	}

	private void GetCustomIdAndIsNumPad3Col()
	{
		byte[] array = new byte[256];
		array = DCHU.GetWMIPackage(17);
		CustomId = array[2];
		if (array[14] == 0)
		{
			IsNumPad3Col = false;
		}
		else
		{
			IsNumPad3Col = true;
		}
	}

	private void InitialEventLog()
	{
		// Windows: EventLog "OutLog"/"PowerBIOSServer_Out" watcher; entries containing "clevofnkey^205" (Flexikey
		// status changed by the Fn hotkey service) re-read app settings page 1 offsets 8/9, "clevofnkey^102" (language
		// changed) reloads the strings. No DCHUService on Linux: nothing delivers these events.
		outlog.WriteEntry("Flexikey, start to receive event log.");
	}

	/// <summary>Event-log entry handler, kept for the later phase that receives service notifications.</summary>
	public void outlog_EntryWritten_Linux(string message)
	{
		if (message.Contains("clevofnkey^205")) { UpdateFlexikeyStatus(); Dispatcher.UIThread.Post(() => updateKeyboard_Mouse()); }
		else if (message.Contains("clevofnkey^102")) { Dispatcher.UIThread.Post(() => { LoadMultiLang(); LoadKeyboardMultiLang(); }); }
	}

	private void UpdateFlexikeyStatus()
	{
		byte[] array = new byte[4];
		DCHU.ReadAppSettings(1, 8, 1, ref array[0]);
		KeyboardStatus = BitConverter.ToInt32(array, 0);
		DCHU.ReadAppSettings(1, 9, 1, ref array[0]);
		MouseStatus = BitConverter.ToInt32(array, 0);
	}

	private void updateKeyboard_Mouse()
	{
		if (KeyboardStatus == 1)
		{
			SL_FlexiKey_KB_status.Value = 1.0;
		}
		else
		{
			SL_FlexiKey_KB_status.Value = 0.0;
		}
		if (MouseStatus == 1)
		{
			SL_FlexiKey_Mouse_status.Value = 1.0;
		}
		else
		{
			SL_FlexiKey_Mouse_status.Value = 0.0;
		}
	}

	private void SetKeyboardStatus(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		DCHU.WriteAppSettings(1, 8, 1, ref bytes[0]);
	}

	private void SetMouseStatus(int value)
	{
		byte[] bytes = BitConverter.GetBytes(value);
		DCHU.WriteAppSettings(1, 9, 1, ref bytes[0]);
	}

	private void LoadMultiLang()
	{
		T_FnBackspace.Text = TranslationLanguage("_FnBackspace");
		RBTN_MacroKB.Content = TranslationLanguage("_MacroKeyboard");
		RBTN_statistics.Content = TranslationLanguage("_Statistics");
		RBTN_MacroMouse.Content = TranslationLanguage("_MacroMouse");
		T_Name.Text = TranslationLanguage("_Name");
		T_Tooltip.Text = TranslationLanguage("_ToolTips");
		CB_Express.Content = TranslationLanguage("_ExpressKey");
		CB_Launch.Content = TranslationLanguage("_LaunchAPP");
		CB_ExpressText.Content = TranslationLanguage("_ExpressText");
		CB_DisableKey.Content = TranslationLanguage("_DISABLE");
		CB_Express1.Content = CB_Express.Content;
		CB_ExpressText1.Content = CB_ExpressText.Content;
		CB_Statistics_DisableKey.Content = CB_DisableKey.Content;
		RBTN_MouseStatus_disable.Content = CB_DisableKey.Content;
		RBTN_MouseStatus_Enable.Content = TranslationLanguage("_Enable");
		RBTN_KBStatus_disable.Content = RBTN_MouseStatus_disable.Content;
		RBTN_KBStatus_Enable.Content = RBTN_MouseStatus_Enable.Content;
		CB_ExpressKey_Start.Content = TranslationLanguage("_Record");
		T_KeyName1.Text = TranslationLanguage("_Name");
		T_KeyAction.Text = TranslationLanguage("_Action");
		T_KeyDelayTime.Text = TranslationLanguage("_DelayTime") + " (mS)";
		CB_ExpressKey_EnableTime.Content = TranslationLanguage("_EnableDelayTime");
		BTN_Restore_expresss.Content = TranslationLanguage("_Restore");
		BTN_launch_browse.Content = TranslationLanguage("_Browse");
		BTN_Restore_launch.Content = BTN_Restore_expresss.Content;
		CB_ExpressText_Start.Content = CB_ExpressKey_Start.Content;
		T_startKey.Text = TranslationLanguage("_StartKey");
		T_TextContent.Text = TranslationLanguage("_TextContent");
		BTN_SAVE_ExpressText.Content = TranslationLanguage("_Save");
		T_SendKey.Text = TranslationLanguage("_SendKey");
		CB_ExpressText_Send.Content = CB_ExpressKey_Start.Content;
		BTN_Restore_express_text.Content = BTN_Restore_expresss.Content;
		BTN_Restore_Disable.Content = BTN_Restore_expresss.Content;
		T_QuickDisable.Text = TranslationLanguage("_QuickDisable");
		T_PressesTimes.Text = TranslationLanguage("_PressesTimes");
		BTN_SAVE_Statistics.Content = BTN_SAVE_ExpressText.Content;
		BTN_Restore_statistics.Content = BTN_Restore_expresss.Content;
		T_EditProfileInfo.Text = TranslationLanguage("_EditProfileInformation");
		BTN_ProfileCustom_OK.Content = TranslationLanguage("_OK");
		BTN_ProfileCustom_Cancel.Content = TranslationLanguage("_Cancel");
		lang_LastOneProfile = TranslationLanguage("_LastOneProfile");
		_strProfileLimitNotice = TranslationLanguage("_MsgMaxProfiles");
	}

	private int GetLanguageFromSystem()
	{
		string text = CultureInfo.CurrentCulture.Name.ToLower();
		if (text.Contains("en"))
		{
			if (text == "en-us")
			{
				return 1;
			}
			return 2;
		}
		if (text.Contains("es"))
		{
			return 2;
		}
		if (text.Contains("de"))
		{
			return 2;
		}
		if (text.Contains("it"))
		{
			return 2;
		}
		if (text.Contains("ja-ep"))
		{
			text = "ja-ep";
			return 18;
		}
		if (text.Contains("ja"))
		{
			text = "ja";
			return 6;
		}
		if (text.Contains("ko"))
		{
			text = "ko";
			return 1;
		}
		if (text.Contains("zh-cn"))
		{
			text = "zh-cn";
			return 1;
		}
		if (text.Contains("zh-tw"))
		{
			text = "zh-tw";
			return 1;
		}
		if (text.Contains("fr-fr") || text.Contains("zh-fr"))
		{
			text = "fr-fr";
			return 2;
		}
		if (text.Contains("fr-ca") || text.Contains("zh-ca"))
		{
			text = "fr-ca";
			return 2;
		}
		if (text.Contains("pt-br") || text.Contains("zh-br"))
		{
			text = "pt-br";
			return 12;
		}
		if (text.Contains("pt-pt") || text.Contains("zh-pt"))
		{
			text = "pt-pt";
			return 2;
		}
		if (text.Contains("tr-tr") || text.Contains("zh-tr"))
		{
			text = "tr-tr";
			return 2;
		}
		if (text.Contains("th-th") || text.Contains("zh-th"))
		{
			text = "th-th";
			return 1;
		}
		if (text.Contains("vi-vn") || text.Contains("zh-vn"))
		{
			text = "vi-vn";
			return 1;
		}
		if (text.Contains("ru"))
		{
			text = "ru";
			return 2;
		}
		text = "en-us";
		return 1;
	}

	private void LoadKeyboardMultiLang()
	{
		HideAllKey();
		int num = -1;
		switch (DCHU.ReadAcpiUILanguage() switch
		{
			0 => GetLanguageFromSystem(), 
			1 => 1, 
			2 => 2, 
			3 => 2, 
			4 => 2, 
			5 => 2, 
			6 => 6, 
			7 => 1, 
			8 => 1, 
			9 => 1, 
			10 => 2, 
			11 => 2, 
			12 => 12, 
			13 => 2, 
			14 => 2, 
			15 => 1, 
			16 => 1, 
			17 => 2, 
			18 => 18, 
			_ => 1, 
		})
		{
		case 1:
			if (IsNumPad3Col)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_40.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_41.IsVisible = true;
				CB_keyCode_42.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_28.IsVisible = true;
				CB_keyCode_43.IsVisible = true;
				CB_keyCode_3Col_54.IsVisible = true;
				CB_keyCode_3Col_57416.IsVisible = true;
				CB_keyCode_3Col_57419.IsVisible = true;
				CB_keyCode_3Col_57424.IsVisible = true;
				CB_keyCode_3Col_57421.IsVisible = true;
				CB_keyCode_3Col_69.IsVisible = true;
				CB_keyCode_3Col_57397.IsVisible = true;
				CB_keyCode_3Col_55.IsVisible = true;
				CB_keyCode_3Col_74.IsVisible = true;
				CB_keyCode_3Col_78.IsVisible = true;
				CB_keyCode_3Col_71.IsVisible = true;
				CB_keyCode_3Col_72.IsVisible = true;
				CB_keyCode_3Col_73.IsVisible = true;
				CB_keyCode_3Col_75.IsVisible = true;
				CB_keyCode_3Col_76.IsVisible = true;
				CB_keyCode_3Col_77.IsVisible = true;
				CB_keyCode_3Col_79.IsVisible = true;
				CB_keyCode_3Col_80.IsVisible = true;
				CB_keyCode_3Col_81.IsVisible = true;
				CB_keyCode_3Col_57372.IsVisible = true;
				CB_keyCode_3Col_57417.IsVisible = true;
			}
			else if (Is14Inch)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_40.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_41.IsVisible = true;
				CB_keyCode_42.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_14Inch_54.IsVisible = true;
				CB_keyCode_14Inch_14.IsVisible = true;
				CB_keyCode_14Inch_28.IsVisible = true;
				CB_keyCode_14Inch_57427.IsVisible = true;
				CB_keyCode_14Inch_57629.IsVisible = true;
				CB_keyCode_14Inch_57426.IsVisible = true;
				CB_keyCode_14Inch_43.IsVisible = true;
				CB_keyCode_14Inch_57417.IsVisible = true;
				CB_keyCode_14Inch_57425.IsVisible = true;
				CB_keyCode_14Inch_57416.IsVisible = true;
				CB_keyCode_14Inch_57419.IsVisible = true;
				CB_keyCode_14Inch_57424.IsVisible = true;
				CB_keyCode_14Inch_57421.IsVisible = true;
				CB_keyCode_14Inch_57400.IsVisible = true;
			}
			else
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_69.IsVisible = true;
				CB_keyCode_57397.IsVisible = true;
				CB_keyCode_55.IsVisible = true;
				CB_keyCode_74.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_71.IsVisible = true;
				CB_keyCode_72.IsVisible = true;
				CB_keyCode_73.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_40.IsVisible = true;
				CB_keyCode_75.IsVisible = true;
				CB_keyCode_76.IsVisible = true;
				CB_keyCode_77.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_57416.IsVisible = true;
				CB_keyCode_79.IsVisible = true;
				CB_keyCode_80.IsVisible = true;
				CB_keyCode_81.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_57419.IsVisible = true;
				CB_keyCode_57424.IsVisible = true;
				CB_keyCode_57421.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_41.IsVisible = true;
				CB_keyCode_42.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_78.IsVisible = true;
				CB_keyCode_57372.IsVisible = true;
				CB_keyCode_54.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_57415.IsVisible = true;
				CB_keyCode_57423.IsVisible = true;
				CB_keyCode_57417.IsVisible = true;
				CB_keyCode_57425.IsVisible = true;
				CB_keyCode_28.IsVisible = true;
				CB_keyCode_43.IsVisible = true;
			}
			break;
		case 2:
			if (IsNumPad3Col)
			{
				CB_keyCode_57400.Tag = "Alt Gr";
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_86.IsVisible = true;
				CB_keyCode_3Col_54.IsVisible = true;
				CB_keyCode_3Col_57416.IsVisible = true;
				CB_keyCode_3Col_57419.IsVisible = true;
				CB_keyCode_3Col_57424.IsVisible = true;
				CB_keyCode_3Col_57421.IsVisible = true;
				CB_keyCode_3Col_69.IsVisible = true;
				CB_keyCode_3Col_57397.IsVisible = true;
				CB_keyCode_3Col_55.IsVisible = true;
				CB_keyCode_3Col_74.IsVisible = true;
				CB_keyCode_3Col_78.IsVisible = true;
				CB_keyCode_3Col_71.IsVisible = true;
				CB_keyCode_3Col_72.IsVisible = true;
				CB_keyCode_3Col_73.IsVisible = true;
				CB_keyCode_3Col_75.IsVisible = true;
				CB_keyCode_3Col_76.IsVisible = true;
				CB_keyCode_3Col_77.IsVisible = true;
				CB_keyCode_3Col_79.IsVisible = true;
				CB_keyCode_3Col_80.IsVisible = true;
				CB_keyCode_3Col_81.IsVisible = true;
				CB_keyCode_3Col_57372.IsVisible = true;
				CB_keyCode_3Col_57417.IsVisible = true;
				CB_keyCode_UK_42.IsVisible = true;
				CB_keyCode_UK_40.IsVisible = true;
				CB_keyCode_UK_43.IsVisible = true;
				CB_keyCode_UK_28.IsVisible = true;
				CB_keyCode_UK_41.IsVisible = true;
			}
			else if (Is14Inch)
			{
				CB_keyCode_57400.Tag = "Alt Gr";
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_86.IsVisible = true;
				CB_keyCode_UK_14Inch_54.IsVisible = true;
				CB_keyCode_UK_14Inch_57373.IsVisible = true;
				CB_keyCode_UK_14Inch_57417.IsVisible = true;
				CB_keyCode_UK_14Inch_57425.IsVisible = true;
				CB_keyCode_UK_14Inch_57416.IsVisible = true;
				CB_keyCode_UK_14Inch_57419.IsVisible = true;
				CB_keyCode_UK_14Inch_57424.IsVisible = true;
				CB_keyCode_UK_14Inch_57421.IsVisible = true;
				CB_keyCode_UK_42.IsVisible = true;
				CB_keyCode_UK_40.IsVisible = true;
				CB_keyCode_UK_43.IsVisible = true;
				CB_keyCode_UK_28.IsVisible = true;
				CB_keyCode_UK_41.IsVisible = true;
			}
			else
			{
				CB_keyCode_57400.Tag = "Alt Gr";
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_69.IsVisible = true;
				CB_keyCode_57397.IsVisible = true;
				CB_keyCode_55.IsVisible = true;
				CB_keyCode_74.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_71.IsVisible = true;
				CB_keyCode_72.IsVisible = true;
				CB_keyCode_73.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_40.IsVisible = true;
				CB_keyCode_75.IsVisible = true;
				CB_keyCode_76.IsVisible = true;
				CB_keyCode_77.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_57416.IsVisible = true;
				CB_keyCode_79.IsVisible = true;
				CB_keyCode_80.IsVisible = true;
				CB_keyCode_81.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_57419.IsVisible = true;
				CB_keyCode_57424.IsVisible = true;
				CB_keyCode_57421.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_78.IsVisible = true;
				CB_keyCode_57372.IsVisible = true;
				CB_keyCode_54.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_57415.IsVisible = true;
				CB_keyCode_57423.IsVisible = true;
				CB_keyCode_57417.IsVisible = true;
				CB_keyCode_57425.IsVisible = true;
				CB_keyCode_86.IsVisible = true;
				CB_keyCode_UK_42.IsVisible = true;
				CB_keyCode_UK_40.IsVisible = true;
				CB_keyCode_UK_43.IsVisible = true;
				CB_keyCode_UK_28.IsVisible = true;
				CB_keyCode_UK_41.IsVisible = true;
			}
			break;
		case 6:
			if (IsNumPad3Col)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_JP_3Col_69.IsVisible = true;
				CB_keyCode_JP_3Col_57397.IsVisible = true;
				CB_keyCode_JP_3Col_55.IsVisible = true;
				CB_keyCode_JP_3Col_74.IsVisible = true;
				CB_keyCode_JP_3Col_78.IsVisible = true;
				CB_keyCode_JP_3Col_71.IsVisible = true;
				CB_keyCode_JP_3Col_72.IsVisible = true;
				CB_keyCode_JP_3Col_73.IsVisible = true;
				CB_keyCode_JP_3Col_75.IsVisible = true;
				CB_keyCode_JP_3Col_76.IsVisible = true;
				CB_keyCode_JP_3Col_77.IsVisible = true;
				CB_keyCode_JP_3Col_79.IsVisible = true;
				CB_keyCode_JP_3Col_80.IsVisible = true;
				CB_keyCode_JP_3Col_81.IsVisible = true;
				CB_keyCode_JP_3Col_57416.IsVisible = true;
				CB_keyCode_JP_3Col_57419.IsVisible = true;
				CB_keyCode_JP_3Col_57424.IsVisible = true;
				CB_keyCode_JP_3Col_57421.IsVisible = true;
				CB_keyCode_JP_3Col_57372.IsVisible = true;
				CB_keyCode_JP_3Col_57417.IsVisible = true;
				CB_keyCode_JP_3Col_14.IsVisible = true;
				CB_keyCode_JP_3Col_54.IsVisible = true;
				CB_keyCode_JP_3Col_115.IsVisible = true;
				CB_keyCode_JP_3Col_28.IsVisible = true;
				CB_keyCode_JP_3Col_51.IsVisible = true;
				CB_keyCode_JP_3Col_52.IsVisible = true;
				CB_keyCode_JP_3Col_53.IsVisible = true;
				CB_keyCode_JP_3Col_57427.IsVisible = true;
				CB_keyCode_JP_41.IsVisible = true;
				CB_keyCode_JP_125.IsVisible = true;
				CB_keyCode_JP_12.IsVisible = true;
				CB_keyCode_JP_13.IsVisible = true;
				CB_keyCode_JP_123.IsVisible = true;
				CB_keyCode_JP_121.IsVisible = true;
				CB_keyCode_JP_112.IsVisible = true;
				CB_keyCode_JP_57373.IsVisible = true;
				CB_keyCode_JP_57437.IsVisible = true;
				CB_keyCode_JP_57.IsVisible = true;
				CB_keyCode_JP_57400.IsVisible = true;
				CB_keyCode_JP_43.IsVisible = true;
				CB_keyCode_JP_26.IsVisible = true;
				CB_keyCode_JP_27.IsVisible = true;
				CB_keyCode_JP_39.IsVisible = true;
				CB_keyCode_JP_40.IsVisible = true;
				CB_keyCode_JP_42.IsVisible = true;
			}
			else if (Is14Inch)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_JP_14Inch_57373.IsVisible = true;
				CB_keyCode_JP_14Inch_57417.IsVisible = true;
				CB_keyCode_JP_14Inch_57425.IsVisible = true;
				CB_keyCode_JP_14Inch_57416.IsVisible = true;
				CB_keyCode_JP_14Inch_57419.IsVisible = true;
				CB_keyCode_JP_14Inch_57424.IsVisible = true;
				CB_keyCode_JP_14Inch_57421.IsVisible = true;
				CB_keyCode_JP_14Inch_14.IsVisible = true;
				CB_keyCode_JP_14Inch_54.IsVisible = true;
				CB_keyCode_JP_14Inch_115.IsVisible = true;
				CB_keyCode_JP_14Inch_28.IsVisible = true;
				CB_keyCode_JP_14Inch_51.IsVisible = true;
				CB_keyCode_JP_14Inch_52.IsVisible = true;
				CB_keyCode_JP_14Inch_53.IsVisible = true;
				CB_keyCode_JP_14Inch_57427.IsVisible = true;
				CB_keyCode_JP_41.IsVisible = true;
				CB_keyCode_JP_125.IsVisible = true;
				CB_keyCode_JP_12.IsVisible = true;
				CB_keyCode_JP_13.IsVisible = true;
				CB_keyCode_JP_123.IsVisible = true;
				CB_keyCode_JP_121.IsVisible = true;
				CB_keyCode_JP_112.IsVisible = true;
				CB_keyCode_JP_57.IsVisible = true;
				CB_keyCode_JP_57400.IsVisible = true;
				CB_keyCode_JP_43.IsVisible = true;
				CB_keyCode_JP_26.IsVisible = true;
				CB_keyCode_JP_27.IsVisible = true;
				CB_keyCode_JP_39.IsVisible = true;
				CB_keyCode_JP_40.IsVisible = true;
				CB_keyCode_JP_42.IsVisible = true;
			}
			else
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_69.IsVisible = true;
				CB_keyCode_57397.IsVisible = true;
				CB_keyCode_55.IsVisible = true;
				CB_keyCode_74.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_71.IsVisible = true;
				CB_keyCode_72.IsVisible = true;
				CB_keyCode_73.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_75.IsVisible = true;
				CB_keyCode_76.IsVisible = true;
				CB_keyCode_77.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_57416.IsVisible = true;
				CB_keyCode_79.IsVisible = true;
				CB_keyCode_80.IsVisible = true;
				CB_keyCode_81.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57419.IsVisible = true;
				CB_keyCode_57424.IsVisible = true;
				CB_keyCode_57421.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_78.IsVisible = true;
				CB_keyCode_57372.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_57415.IsVisible = true;
				CB_keyCode_57423.IsVisible = true;
				CB_keyCode_57417.IsVisible = true;
				CB_keyCode_57425.IsVisible = true;
				CB_keyCode_JP_41.IsVisible = true;
				CB_keyCode_JP_14.IsVisible = true;
				CB_keyCode_JP_125.IsVisible = true;
				CB_keyCode_JP_12.IsVisible = true;
				CB_keyCode_JP_13.IsVisible = true;
				CB_keyCode_JP_54.IsVisible = true;
				CB_keyCode_JP_115.IsVisible = true;
				CB_keyCode_JP_123.IsVisible = true;
				CB_keyCode_JP_121.IsVisible = true;
				CB_keyCode_JP_112.IsVisible = true;
				CB_keyCode_JP_57373.IsVisible = true;
				CB_keyCode_JP_57437.IsVisible = true;
				CB_keyCode_JP_57.IsVisible = true;
				CB_keyCode_JP_57400.IsVisible = true;
				CB_keyCode_JP_43.IsVisible = true;
				CB_keyCode_JP_26.IsVisible = true;
				CB_keyCode_JP_27.IsVisible = true;
				CB_keyCode_JP_39.IsVisible = true;
				CB_keyCode_JP_40.IsVisible = true;
				CB_keyCode_JP_28.IsVisible = true;
				CB_keyCode_JP_42.IsVisible = true;
				CB_keyCode_JP_51.IsVisible = true;
				CB_keyCode_JP_52.IsVisible = true;
				CB_keyCode_JP_53.IsVisible = true;
			}
			break;
		case 18:
			if (IsNumPad3Col)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_JA_EP_3Col_54.IsVisible = true;
				CB_keyCode_JA_EP_3Col_69.IsVisible = true;
				CB_keyCode_JA_EP_3Col_57397.IsVisible = true;
				CB_keyCode_JA_EP_3Col_55.IsVisible = true;
				CB_keyCode_JA_EP_3Col_74.IsVisible = true;
				CB_keyCode_JA_EP_3Col_78.IsVisible = true;
				CB_keyCode_JA_EP_3Col_71.IsVisible = true;
				CB_keyCode_JA_EP_3Col_72.IsVisible = true;
				CB_keyCode_JA_EP_3Col_73.IsVisible = true;
				CB_keyCode_JA_EP_3Col_75.IsVisible = true;
				CB_keyCode_JA_EP_3Col_76.IsVisible = true;
				CB_keyCode_JA_EP_3Col_77.IsVisible = true;
				CB_keyCode_JA_EP_3Col_57416.IsVisible = true;
				CB_keyCode_JA_EP_3Col_79.IsVisible = true;
				CB_keyCode_JA_EP_3Col_80.IsVisible = true;
				CB_keyCode_JA_EP_3Col_81.IsVisible = true;
				CB_keyCode_JA_EP_3Col_57419.IsVisible = true;
				CB_keyCode_JA_EP_3Col_57424.IsVisible = true;
				CB_keyCode_JA_EP_3Col_57421.IsVisible = true;
				CB_keyCode_JA_EP_3Col_57372.IsVisible = true;
				CB_keyCode_JA_EP_3Col_57417.IsVisible = true;
				CB_keyCode_JP_41.IsVisible = true;
				CB_keyCode_JP_14.IsVisible = true;
				CB_keyCode_JP_125.IsVisible = true;
				CB_keyCode_JP_12.IsVisible = true;
				CB_keyCode_JP_13.IsVisible = true;
				CB_keyCode_JP_115.IsVisible = true;
				CB_keyCode_JP_123.IsVisible = true;
				CB_keyCode_JP_121.IsVisible = true;
				CB_keyCode_JP_112.IsVisible = true;
				CB_keyCode_JP_57373.IsVisible = true;
				CB_keyCode_JP_57437.IsVisible = true;
				CB_keyCode_JP_57.IsVisible = true;
				CB_keyCode_JP_57400.IsVisible = true;
				CB_keyCode_JP_43.IsVisible = true;
				CB_keyCode_JP_26.IsVisible = true;
				CB_keyCode_JP_27.IsVisible = true;
				CB_keyCode_JP_39.IsVisible = true;
				CB_keyCode_JP_40.IsVisible = true;
				CB_keyCode_JP_28.IsVisible = true;
				CB_keyCode_JP_42.IsVisible = true;
				CB_keyCode_JP_51.IsVisible = true;
				CB_keyCode_JP_52.IsVisible = true;
				CB_keyCode_JP_53.IsVisible = true;
			}
			else if (Is14Inch)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_57373.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_57417.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_57425.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_57416.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_57419.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_57424.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_57421.IsVisible = true;
				CB_keyCode_JA_EP_14Inch_54.IsVisible = true;
				CB_keyCode_JP_41.IsVisible = true;
				CB_keyCode_JP_14.IsVisible = true;
				CB_keyCode_JP_125.IsVisible = true;
				CB_keyCode_JP_12.IsVisible = true;
				CB_keyCode_JP_13.IsVisible = true;
				CB_keyCode_JP_115.IsVisible = true;
				CB_keyCode_JP_123.IsVisible = true;
				CB_keyCode_JP_121.IsVisible = true;
				CB_keyCode_JP_112.IsVisible = true;
				CB_keyCode_JP_57.IsVisible = true;
				CB_keyCode_JP_57400.IsVisible = true;
				CB_keyCode_JP_43.IsVisible = true;
				CB_keyCode_JP_26.IsVisible = true;
				CB_keyCode_JP_27.IsVisible = true;
				CB_keyCode_JP_39.IsVisible = true;
				CB_keyCode_JP_40.IsVisible = true;
				CB_keyCode_JP_28.IsVisible = true;
				CB_keyCode_JP_42.IsVisible = true;
				CB_keyCode_JP_51.IsVisible = true;
				CB_keyCode_JP_52.IsVisible = true;
				CB_keyCode_JP_53.IsVisible = true;
			}
			else
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_69.IsVisible = true;
				CB_keyCode_57397.IsVisible = true;
				CB_keyCode_55.IsVisible = true;
				CB_keyCode_74.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_71.IsVisible = true;
				CB_keyCode_72.IsVisible = true;
				CB_keyCode_73.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_75.IsVisible = true;
				CB_keyCode_76.IsVisible = true;
				CB_keyCode_77.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_57416.IsVisible = true;
				CB_keyCode_79.IsVisible = true;
				CB_keyCode_80.IsVisible = true;
				CB_keyCode_81.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57419.IsVisible = true;
				CB_keyCode_57424.IsVisible = true;
				CB_keyCode_57421.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_78.IsVisible = true;
				CB_keyCode_57372.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_57415.IsVisible = true;
				CB_keyCode_57423.IsVisible = true;
				CB_keyCode_57417.IsVisible = true;
				CB_keyCode_57425.IsVisible = true;
				CB_keyCode_JP_41.IsVisible = true;
				CB_keyCode_JP_14.IsVisible = true;
				CB_keyCode_JP_125.IsVisible = true;
				CB_keyCode_JP_12.IsVisible = true;
				CB_keyCode_JP_13.IsVisible = true;
				CB_keyCode_JP_54.IsVisible = true;
				CB_keyCode_JP_115.IsVisible = true;
				CB_keyCode_JP_123.IsVisible = true;
				CB_keyCode_JP_121.IsVisible = true;
				CB_keyCode_JP_112.IsVisible = true;
				CB_keyCode_JP_57373.IsVisible = true;
				CB_keyCode_JP_57437.IsVisible = true;
				CB_keyCode_JP_57.IsVisible = true;
				CB_keyCode_JP_57400.IsVisible = true;
				CB_keyCode_JP_43.IsVisible = true;
				CB_keyCode_JP_26.IsVisible = true;
				CB_keyCode_JP_27.IsVisible = true;
				CB_keyCode_JP_39.IsVisible = true;
				CB_keyCode_JP_40.IsVisible = true;
				CB_keyCode_JP_28.IsVisible = true;
				CB_keyCode_JP_42.IsVisible = true;
				CB_keyCode_JP_51.IsVisible = true;
				CB_keyCode_JP_52.IsVisible = true;
				CB_keyCode_JP_53.IsVisible = true;
			}
			break;
		case 12:
			if (IsNumPad3Col)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_86.IsVisible = true;
				CB_keyCode_BR_3Col_54.IsVisible = true;
				CB_keyCode_BR_3Col_57416.IsVisible = true;
				CB_keyCode_BR_3Col_57419.IsVisible = true;
				CB_keyCode_BR_3Col_57424.IsVisible = true;
				CB_keyCode_BR_3Col_57421.IsVisible = true;
				CB_keyCode_BR_3Col_69.IsVisible = true;
				CB_keyCode_BR_3Col_57397.IsVisible = true;
				CB_keyCode_BR_3Col_55.IsVisible = true;
				CB_keyCode_BR_3Col_74.IsVisible = true;
				CB_keyCode_BR_3Col_78.IsVisible = true;
				CB_keyCode_BR_3Col_71.IsVisible = true;
				CB_keyCode_BR_3Col_72.IsVisible = true;
				CB_keyCode_BR_3Col_73.IsVisible = true;
				CB_keyCode_BR_3Col_75.IsVisible = true;
				CB_keyCode_BR_3Col_76.IsVisible = true;
				CB_keyCode_BR_3Col_77.IsVisible = true;
				CB_keyCode_BR_3Col_79.IsVisible = true;
				CB_keyCode_BR_3Col_80.IsVisible = true;
				CB_keyCode_BR_3Col_81.IsVisible = true;
				CB_keyCode_BR_3Col_57372.IsVisible = true;
				CB_keyCode_BR_3Col_57417.IsVisible = true;
				CB_keyCode_BR_28.IsVisible = true;
				CB_keyCode_BR_39.IsVisible = true;
				CB_keyCode_BR_40.IsVisible = true;
				CB_keyCode_BR_43.IsVisible = true;
				CB_keyCode_BR_27.IsVisible = true;
				CB_keyCode_BR_26.IsVisible = true;
				CB_keyCode_BR_51.IsVisible = true;
				CB_keyCode_BR_52.IsVisible = true;
				CB_keyCode_BR_115.IsVisible = true;
				CB_keyCode_BR_53.IsVisible = true;
				CB_keyCode_BR_42.IsVisible = true;
				CB_keyCode_BR_41.IsVisible = true;
			}
			else if (Is14Inch)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_86.IsVisible = true;
				CB_keyCode_BR_14Inch_54.IsVisible = true;
				CB_keyCode_BR_14Inch_57373.IsVisible = true;
				CB_keyCode_BR_14Inch_57417.IsVisible = true;
				CB_keyCode_BR_14Inch_57425.IsVisible = true;
				CB_keyCode_BR_14Inch_57416.IsVisible = true;
				CB_keyCode_BR_14Inch_57419.IsVisible = true;
				CB_keyCode_BR_14Inch_57424.IsVisible = true;
				CB_keyCode_BR_14Inch_57421.IsVisible = true;
				CB_keyCode_BR_28.IsVisible = true;
				CB_keyCode_BR_39.IsVisible = true;
				CB_keyCode_BR_40.IsVisible = true;
				CB_keyCode_BR_43.IsVisible = true;
				CB_keyCode_BR_27.IsVisible = true;
				CB_keyCode_BR_26.IsVisible = true;
				CB_keyCode_BR_51.IsVisible = true;
				CB_keyCode_BR_52.IsVisible = true;
				CB_keyCode_BR_115.IsVisible = true;
				CB_keyCode_BR_53.IsVisible = true;
				CB_keyCode_BR_42.IsVisible = true;
				CB_keyCode_BR_41.IsVisible = true;
			}
			else
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_69.IsVisible = true;
				CB_keyCode_57397.IsVisible = true;
				CB_keyCode_55.IsVisible = true;
				CB_keyCode_74.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_71.IsVisible = true;
				CB_keyCode_72.IsVisible = true;
				CB_keyCode_73.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_75.IsVisible = true;
				CB_keyCode_76.IsVisible = true;
				CB_keyCode_77.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_57416.IsVisible = true;
				CB_keyCode_79.IsVisible = true;
				CB_keyCode_80.IsVisible = true;
				CB_keyCode_81.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_57419.IsVisible = true;
				CB_keyCode_57424.IsVisible = true;
				CB_keyCode_57421.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_78.IsVisible = true;
				CB_keyCode_57372.IsVisible = true;
				CB_keyCode_54.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_57415.IsVisible = true;
				CB_keyCode_57423.IsVisible = true;
				CB_keyCode_57417.IsVisible = true;
				CB_keyCode_57425.IsVisible = true;
				CB_keyCode_86.IsVisible = true;
				CB_keyCode_BR_28.IsVisible = true;
				CB_keyCode_BR_39.IsVisible = true;
				CB_keyCode_BR_40.IsVisible = true;
				CB_keyCode_BR_43.IsVisible = true;
				CB_keyCode_BR_27.IsVisible = true;
				CB_keyCode_BR_26.IsVisible = true;
				CB_keyCode_BR_51.IsVisible = true;
				CB_keyCode_BR_52.IsVisible = true;
				CB_keyCode_BR_115.IsVisible = true;
				CB_keyCode_BR_53.IsVisible = true;
				CB_keyCode_BR_42.IsVisible = true;
				CB_keyCode_BR_41.IsVisible = true;
			}
			break;
		default:
			if (IsNumPad3Col)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_40.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_41.IsVisible = true;
				CB_keyCode_42.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_28.IsVisible = true;
				CB_keyCode_43.IsVisible = true;
				CB_keyCode_3Col_54.IsVisible = true;
				CB_keyCode_3Col_57416.IsVisible = true;
				CB_keyCode_3Col_57419.IsVisible = true;
				CB_keyCode_3Col_57424.IsVisible = true;
				CB_keyCode_3Col_57421.IsVisible = true;
				CB_keyCode_3Col_69.IsVisible = true;
				CB_keyCode_3Col_57397.IsVisible = true;
				CB_keyCode_3Col_55.IsVisible = true;
				CB_keyCode_3Col_74.IsVisible = true;
				CB_keyCode_3Col_78.IsVisible = true;
				CB_keyCode_3Col_71.IsVisible = true;
				CB_keyCode_3Col_72.IsVisible = true;
				CB_keyCode_3Col_73.IsVisible = true;
				CB_keyCode_3Col_75.IsVisible = true;
				CB_keyCode_3Col_76.IsVisible = true;
				CB_keyCode_3Col_77.IsVisible = true;
				CB_keyCode_3Col_79.IsVisible = true;
				CB_keyCode_3Col_80.IsVisible = true;
				CB_keyCode_3Col_81.IsVisible = true;
				CB_keyCode_3Col_57372.IsVisible = true;
				CB_keyCode_3Col_57417.IsVisible = true;
			}
			else if (Is14Inch)
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_40.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_41.IsVisible = true;
				CB_keyCode_42.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_14Inch_54.IsVisible = true;
				CB_keyCode_14Inch_14.IsVisible = true;
				CB_keyCode_14Inch_28.IsVisible = true;
				CB_keyCode_14Inch_57427.IsVisible = true;
				CB_keyCode_14Inch_57629.IsVisible = true;
				CB_keyCode_14Inch_57426.IsVisible = true;
				CB_keyCode_14Inch_43.IsVisible = true;
				CB_keyCode_14Inch_57417.IsVisible = true;
				CB_keyCode_14Inch_57425.IsVisible = true;
				CB_keyCode_14Inch_57416.IsVisible = true;
				CB_keyCode_14Inch_57419.IsVisible = true;
				CB_keyCode_14Inch_57424.IsVisible = true;
				CB_keyCode_14Inch_57421.IsVisible = true;
			}
			else
			{
				CB_keyCode_2.IsVisible = true;
				CB_keyCode_3.IsVisible = true;
				CB_keyCode_4.IsVisible = true;
				CB_keyCode_5.IsVisible = true;
				CB_keyCode_6.IsVisible = true;
				CB_keyCode_7.IsVisible = true;
				CB_keyCode_8.IsVisible = true;
				CB_keyCode_9.IsVisible = true;
				CB_keyCode_10.IsVisible = true;
				CB_keyCode_11.IsVisible = true;
				CB_keyCode_12.IsVisible = true;
				CB_keyCode_13.IsVisible = true;
				CB_keyCode_69.IsVisible = true;
				CB_keyCode_57397.IsVisible = true;
				CB_keyCode_55.IsVisible = true;
				CB_keyCode_74.IsVisible = true;
				CB_keyCode_16.IsVisible = true;
				CB_keyCode_17.IsVisible = true;
				CB_keyCode_18.IsVisible = true;
				CB_keyCode_19.IsVisible = true;
				CB_keyCode_20.IsVisible = true;
				CB_keyCode_21.IsVisible = true;
				CB_keyCode_22.IsVisible = true;
				CB_keyCode_23.IsVisible = true;
				CB_keyCode_24.IsVisible = true;
				CB_keyCode_25.IsVisible = true;
				CB_keyCode_26.IsVisible = true;
				CB_keyCode_27.IsVisible = true;
				CB_keyCode_71.IsVisible = true;
				CB_keyCode_72.IsVisible = true;
				CB_keyCode_73.IsVisible = true;
				CB_keyCode_30.IsVisible = true;
				CB_keyCode_31.IsVisible = true;
				CB_keyCode_32.IsVisible = true;
				CB_keyCode_33.IsVisible = true;
				CB_keyCode_34.IsVisible = true;
				CB_keyCode_35.IsVisible = true;
				CB_keyCode_36.IsVisible = true;
				CB_keyCode_37.IsVisible = true;
				CB_keyCode_38.IsVisible = true;
				CB_keyCode_39.IsVisible = true;
				CB_keyCode_40.IsVisible = true;
				CB_keyCode_75.IsVisible = true;
				CB_keyCode_76.IsVisible = true;
				CB_keyCode_77.IsVisible = true;
				CB_keyCode_44.IsVisible = true;
				CB_keyCode_45.IsVisible = true;
				CB_keyCode_46.IsVisible = true;
				CB_keyCode_47.IsVisible = true;
				CB_keyCode_48.IsVisible = true;
				CB_keyCode_49.IsVisible = true;
				CB_keyCode_50.IsVisible = true;
				CB_keyCode_51.IsVisible = true;
				CB_keyCode_52.IsVisible = true;
				CB_keyCode_53.IsVisible = true;
				CB_keyCode_57416.IsVisible = true;
				CB_keyCode_79.IsVisible = true;
				CB_keyCode_80.IsVisible = true;
				CB_keyCode_81.IsVisible = true;
				CB_keyCode_162.IsVisible = true;
				CB_keyCode_163.IsVisible = true;
				CB_keyCode_56.IsVisible = true;
				CB_keyCode_57400.IsVisible = true;
				CB_keyCode_57437.IsVisible = true;
				CB_keyCode_57419.IsVisible = true;
				CB_keyCode_57424.IsVisible = true;
				CB_keyCode_57421.IsVisible = true;
				CB_keyCode_82.IsVisible = true;
				CB_keyCode_83.IsVisible = true;
				CB_keyCode_58.IsVisible = true;
				CB_keyCode_15.IsVisible = true;
				CB_keyCode_41.IsVisible = true;
				CB_keyCode_42.IsVisible = true;
				CB_keyCode_29.IsVisible = true;
				CB_keyCode_57.IsVisible = true;
				CB_keyCode_57373.IsVisible = true;
				CB_keyCode_14.IsVisible = true;
				CB_keyCode_78.IsVisible = true;
				CB_keyCode_57372.IsVisible = true;
				CB_keyCode_54.IsVisible = true;
				CB_keyCode_1.IsVisible = true;
				CB_keyCode_59.IsVisible = true;
				CB_keyCode_60.IsVisible = true;
				CB_keyCode_61.IsVisible = true;
				CB_keyCode_62.IsVisible = true;
				CB_keyCode_63.IsVisible = true;
				CB_keyCode_64.IsVisible = true;
				CB_keyCode_65.IsVisible = true;
				CB_keyCode_66.IsVisible = true;
				CB_keyCode_67.IsVisible = true;
				CB_keyCode_68.IsVisible = true;
				CB_keyCode_87.IsVisible = true;
				CB_keyCode_88.IsVisible = true;
				CB_keyCode_57399.IsVisible = true;
				CB_keyCode_57426.IsVisible = true;
				CB_keyCode_57427.IsVisible = true;
				CB_keyCode_57415.IsVisible = true;
				CB_keyCode_57423.IsVisible = true;
				CB_keyCode_57417.IsVisible = true;
				CB_keyCode_57425.IsVisible = true;
				CB_keyCode_28.IsVisible = true;
				CB_keyCode_43.IsVisible = true;
			}
			break;
		}
	}

	private void Set_Filiter_Driver()
	{
		int index = listBox_profile.SelectedIndex;
		setkb.SetKbCleanFilter();
		setkb.SetMouseCleanFilter();
		if (listBox_profile.SelectedIndex == -1)
		{
			iNowIndex = 0;
			index = 0;
		}
		bool isKbEnable = _profileInfo[index].IsKbEnable;
		bool isMouseEnable = _profileInfo[index].IsMouseEnable;
		if (!isKbEnable && !isMouseEnable)
		{
			setkb.SetKbDisableFilter();
			setkb.SetMouseDisableFilter();
			return;
		}
		if (isKbEnable)
		{
			setkb.SetKbEnableFilter();
		}
		else
		{
			setkb.SetKbDisableFilter();
		}
		if (isMouseEnable)
		{
			setkb.SetMouseEnableFilter();
		}
		else
		{
			setkb.SetMouseDisableFilter();
		}
		Thread.Sleep(50);
		foreach (KeyInfo profileDatum in _profileInfo[index].profileData)
		{
			switch (profileDatum.ActionType)
			{
			case KeyGlobal.KeyActionType.keypress:
				setkb.SetMacroKey(profileDatum);
				break;
			case KeyGlobal.KeyActionType.launch:
				setkb.SetLaunchApp(profileDatum);
				break;
			case KeyGlobal.KeyActionType.text:
				setkb.SetTextMacro(profileDatum);
				break;
			case KeyGlobal.KeyActionType.disable:
				setkb.SetKeyDisable(profileDatum);
				break;
			}
		}
	}

	private bool LoadXml(string fileName)
	{
		if (!File.Exists(fileName))
		{
			return false;
		}
		_profileInfo.Clear();
		if (!XmlRW.LoadXml(fileName, out _profileInfo))
		{
			return false;
		}
		if (_profileInfo.Count == 0)
		{
			return false;
		}
		return true;
	}

	private void LoadProfile(int index)
	{
		try
		{
			if (index < _profileInfo.Count)
			{
				Load_KB_Mouse_KeyInfo(_profileInfo[index].profileData);
				Cancel_KB_Mouse_UI();
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	private void SaveCurrentProfile(int Index, string filename)
	{
		if (listBox_profile.Items.Count != 0)
		{
			if (listBox_profile.SelectedIndex == _profileInfo.Count)
			{
				iNowIndex = listBox_profile.SelectedIndex - 1;
			}
			else
			{
				_profileInfo[Index].profileData = Save_ProfileData();
			}
		}
		int num = 256;
		for (int i = 0; i < _profileInfo.Count; i++)
		{
			for (int j = 0; j < _profileInfo[i].profileData.Count; j++)
			{
				if (_profileInfo[i].profileData[j].ActionType == KeyGlobal.KeyActionType.launch)
				{
					_profileInfo[i].profileData[j].AppEventId = num;
					num++;
				}
			}
		}
		XmlRW.Save2Xml(_profileInfo, filename);
		Set_Filiter_Driver();
	}

	public List<KeyInfo> Save_ProfileData()
	{
		List<KeyInfo> list = new List<KeyInfo>();
		foreach (KeyInfo item in myKeyInfo)
		{
			if (item.ActionType != KeyGlobal.KeyActionType.none)
			{
				list.Add(item);
			}
		}
		return list;
	}

	private void Load_KB_Mouse_KeyInfo(List<KeyInfo> infoList)
	{
		myKeyInfo.Clear();
		myKeyInfo = Init_KeyInfo(myKeyInfo, KeyGlobal.DeviceType.none);
		foreach (KeyInfo loadInfo in infoList)
		{
			if (loadInfo.Device == KeyGlobal.DeviceType.keyboard)
			{
				int num = myKeyInfo.FindIndex((KeyInfo key) => key.CheckBoxName.Equals(loadInfo.CheckBoxName));
				if (num != -1)
				{
					myKeyInfo[num].KeyName = loadInfo.KeyName;
					myKeyInfo[num].ActionType = loadInfo.ActionType;
					myKeyInfo[num].AppPath = loadInfo.AppPath;
					myKeyInfo[num].AppEventId = loadInfo.AppEventId;
					myKeyInfo[num].MacroKey = loadInfo.MacroKey;
					myKeyInfo[num].IsKeypressDelay = loadInfo.IsKeypressDelay;
					myKeyInfo[num].MacroKeyGruopFlag = loadInfo.MacroKeyGruopFlag;
					myKeyInfo[num].SendKey = loadInfo.SendKey;
					myKeyInfo[num].StartKey = loadInfo.StartKey;
					if (loadInfo.TextContent == null)
					{
						myKeyInfo[num].TextContent = "";
					}
					else
					{
						myKeyInfo[num].TextContent = loadInfo.TextContent;
					}
					foreach (CheckBox child in canvas_kb.Children)
					{
						CheckBox val = child;
						if (val.Name == myKeyInfo[num].CheckBoxName)
						{
							val.BorderBrush = ActionType_color(myKeyInfo[num].ActionType);
						}
					}
				}
				else
				{
					myKeyInfo = Init_KeyInfo(myKeyInfo, KeyGlobal.DeviceType.keyboard);
				}
			}
			else
			{
				if (loadInfo.Device != KeyGlobal.DeviceType.mouse)
				{
					continue;
				}
				int num = myKeyInfo.FindIndex((KeyInfo key) => key.CheckBoxName.Equals(loadInfo.CheckBoxName));
				if (num != -1)
				{
					myKeyInfo[num].CheckBoxName = loadInfo.CheckBoxName;
					myKeyInfo[num].ActionType = loadInfo.ActionType;
					myKeyInfo[num].AppPath = loadInfo.AppPath;
					myKeyInfo[num].AppEventId = loadInfo.AppEventId;
					myKeyInfo[num].MacroKey = loadInfo.MacroKey;
					myKeyInfo[num].IsKeypressDelay = loadInfo.IsKeypressDelay;
					myKeyInfo[num].MacroKeyGruopFlag = loadInfo.MacroKeyGruopFlag;
					myKeyInfo[num].SendKey = loadInfo.SendKey;
					myKeyInfo[num].StartKey = loadInfo.StartKey;
					if (loadInfo.TextContent == null)
					{
						myKeyInfo[num].TextContent = "";
					}
					else
					{
						myKeyInfo[num].TextContent = loadInfo.TextContent;
					}
					myKeyInfo[num].Tip = loadInfo.Tip;
					foreach (CheckBox child2 in grid_mouse_BTN.Children)
					{
						CheckBox val2 = child2;
						if (val2.Name == myKeyInfo[num].CheckBoxName)
						{
							val2.Tag = ((int)myKeyInfo[num].ActionType).ToString();
						}
					}
				}
				else
				{
					myKeyInfo = Init_KeyInfo(myKeyInfo, KeyGlobal.DeviceType.mouse);
				}
			}
		}
		SaveCurrentProfile(iNowIndex, xmlFileName);
	}

	private void Update_Keyinfo()
	{
		if (CB_DisableKey.IsChecked == true)
		{
			myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.disable;
		}
		else if (CB_Express.IsChecked == true)
		{
			if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count > 0)
			{
				myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.keypress;
			}
			else
			{
				myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.none;
			}
		}
		else if (CB_ExpressText.IsChecked == true)
		{
			if (myKeyInfo[KeyInfo_CurrentIndex].StartKey.Count > 0 && myKeyInfo[KeyInfo_CurrentIndex].SendKey.Count > 0 && myKeyInfo[KeyInfo_CurrentIndex].TextContent != "")
			{
				myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.text;
			}
			else
			{
				myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.none;
			}
		}
		else if (CB_Launch.IsChecked == true)
		{
			if (T_AppPath.Text == "")
			{
				myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.none;
			}
			else
			{
				myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.launch;
				myKeyInfo[KeyInfo_CurrentIndex].AppPath = T_AppPath.Text;
			}
		}
		else
		{
			myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.none;
		}
		myKeyInfo[KeyInfo_CurrentIndex].Tip = TB_Tooltip.Text;
		Console.WriteLine("save Update_Keyinfo = " + iNowIndex);
		SaveCurrentProfile(iNowIndex, xmlFileName);
		Update_Key_Action_icon();
	}

	public List<KeyInfo> Init_KeyInfo(List<KeyInfo> mylist, KeyGlobal.DeviceType Type)
	{
		if (mylist == null)
		{
			mylist = new List<KeyInfo>();
		}
		if (Type == KeyGlobal.DeviceType.keyboard || Type == KeyGlobal.DeviceType.none)
		{
			foreach (CheckBox child in canvas_kb.Children)
			{
				string name = child.Name;
				if (name.Contains("CB_keyCode_"))
				{
					KeyInfo keyInfo = new KeyInfo
					{
						CheckBoxName = name
					};
					Console.WriteLine(name);
					if (name.Contains("CB_keyCode_14Inch_"))
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(18));
					}
					else if (name.Contains("CB_keyCode_UK_14Inch_") || name.Contains("CB_keyCode_JP_14Inch_") || name.Contains("CB_keyCode_BR_14Inch_"))
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(21));
					}
					else if (name.Contains("CB_keyCode_JA_EP_14Inch_"))
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(24));
					}
					else if (name.Contains("CB_keyCode_3Col_"))
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(16));
					}
					else if (name.Contains("CB_keyCode_UK_3Col_") || name.Contains("CB_keyCode_JP_3Col_") || name.Contains("CB_keyCode_BR_3Col_"))
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(19));
					}
					else if (name.Contains("CB_keyCode_JA_EP_3Col_"))
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(22));
					}
					else if (name.Contains("CB_keyCode_UK_") || name.Contains("CB_keyCode_JP_") || name.Contains("CB_keyCode_BR_"))
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(14));
					}
					else
					{
						keyInfo.KeyCode = Convert.ToInt32(name.Substring(11));
					}
					keyInfo.ActionType = KeyGlobal.KeyActionType.none;
					keyInfo.Device = KeyGlobal.DeviceType.keyboard;
					keyInfo.IsSelected = false;
					keyInfo.AppPath = "";
					keyInfo.TextContent = "";
					mylist.Add(keyInfo);
				}
				child.BorderBrush = ActionType_color(KeyGlobal.KeyActionType.none);
			}
		}
		if (Type == KeyGlobal.DeviceType.mouse || Type == KeyGlobal.DeviceType.none)
		{
			foreach (CheckBox child2 in grid_mouse_BTN.Children)
			{
				CheckBox val2 = child2;
				KeyInfo keyInfo2 = new KeyInfo();
				keyInfo2.CheckBoxName = val2.Name;
				keyInfo2.KeyCode = (int)Enum.Parse(typeof(ClevoMouse), val2.Name);
				keyInfo2.ActionType = KeyGlobal.KeyActionType.none;
				keyInfo2.Device = KeyGlobal.DeviceType.mouse;
				keyInfo2.IsSelected = false;
				keyInfo2.AppPath = "";
				keyInfo2.TextContent = "";
				mylist.Add(keyInfo2);
				val2.Tag = 0.ToString();
			}
		}
		return mylist;
	}

	private void listBox_profile_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		try
		{
			if (isCustomOKClick)
			{
				return;
			}
			if (listBox_profile.Items.Count == 0)
			{
				SaveCurrentProfile(iNowIndex, xmlFileName);
				return;
			}
			Console.WriteLine("iNowIndex=" + iNowIndex + "  listBox_profile.SelectedIndex=" + listBox_profile.SelectedIndex);
			if (iNowIndex == listBox_profile.SelectedIndex)
			{
				return;
			}
			SaveCurrentProfile(iNowIndex, xmlFileName);
			Console.WriteLine("更新目前選擇的Profile");
			for (int i = 0; i < listBox_profile.Items.Count; i++)
			{
				_ = (UserControl_profile)listBox_profile.Items[i];
				if (i == listBox_profile.SelectedIndex)
				{
					_profileInfo[i].IsSelected = true;
				}
				else
				{
					_profileInfo[i].IsSelected = false;
				}
			}
			iNowIndex = listBox_profile.SelectedIndex;
			LoadProfile(iNowIndex);
			Console.WriteLine("Load new Profile = " + iNowIndex);
		}
		catch
		{
		}
	}

	private void BTN_profile_add_Click(object sender, RoutedEventArgs e)
	{
		listBox_profile.Focus();
		AddProfile();
	}

	private void BTN_profile_delete_Click(object sender, RoutedEventArgs e)
	{
		DeleteProfile();
	}

	private void BTN_profile_export_Click(object sender, RoutedEventArgs e)
	{
		ExportProfile();
	}

	private void BTN_profile_import_Click(object sender, RoutedEventArgs e)
	{
		ImportProfile();
	}

	private void AddProfile()
	{
		if (listBox_profile.Items.Count > 11)
		{
			MessageBox.Show(_strProfileLimitNotice, _strComboKeys, (MessageBoxButton)0, (MessageBoxImage)48);
			return;
		}
		if (listBox_profile.Items.Count != 0)
		{
			_profileInfo[iNowIndex].IsSelected = false;
			Console.WriteLine("save add profile = " + iNowIndex);
			SaveCurrentProfile(iNowIndex, xmlFileName);
		}
		List<string> list = new List<string>();
		int num = -1;
		foreach (UserControl_profile item in (IEnumerable)listBox_profile.Items)
		{
			list.Add(item.T_porfileName.Text.ToString());
		}
		for (int i = 0; i < 12; i++)
		{
			bool flag = false;
			foreach (string item2 in list)
			{
				if (item2.Equals(_strProfile + i))
				{
					flag = true;
				}
			}
			if (!flag)
			{
				num = i;
				break;
			}
		}
		UserControl_profile userControl_profile2 = new UserControl_profile();
		userControl_profile2.T_porfileName.Text = _strProfile + listBox_profile.Items.Count;
		if (num != -1)
		{
			userControl_profile2.T_porfileName.Text = _strProfile + num;
		}
		listBox_profile.Items.Add((object)userControl_profile2);
		if (listBox_profile.Items.Count >= 2)
		{
			if (listBox_profile.SelectedIndex == -1)
			{
				listBox_profile.SelectedIndex = 0;
			}
			_ = (UserControl_profile)listBox_profile.Items[listBox_profile.SelectedIndex];
		}
		ProfileInfo profileInfo = new ProfileInfo();
		profileInfo.ProfileName = userControl_profile2.T_porfileName.Text;
		profileInfo.ProfileIconPath = "";
		profileInfo.IsSelected = true;
		profileInfo.ProfileIndex = listBox_profile.Items.Count - 1;
		if (SL_FlexiKey_KB_status.Value == 1.0)
		{
			profileInfo.IsKbEnable = true;
		}
		else
		{
			profileInfo.IsKbEnable = false;
		}
		if (SL_FlexiKey_Mouse_status.Value == 1.0)
		{
			profileInfo.IsMouseEnable = true;
		}
		else
		{
			profileInfo.IsMouseEnable = false;
		}
		_profileInfo.Add(profileInfo);
		listBox_profile.SelectedIndex = listBox_profile.Items.Count - 1;
		listBox_profile.ScrollIntoView(listBox_profile.SelectedItem);
	}

	private void DeleteProfile()
	{
		listBox_profile.Focus();
		if (listBox_profile.Items.Count == 1)
		{
			MessageBox.Show(lang_LastOneProfile, _strComboKeys, (MessageBoxButton)0, (MessageBoxImage)48);
			return;
		}
		_profileInfo.RemoveAt(listBox_profile.SelectedIndex);
		_profileInfo[0].IsSelected = true;
		Update_Profile_List();
		LoadProfile(iNowIndex);
	}

	private void ImportProfile()
	{
		OpenFileDialog val = new OpenFileDialog();
		val.Title = "Import profile";
		val.Filter = "XML Files (*.xml)|*.xml";
		if (val.ShowDialog() == true)
		{
			if (LoadXml(val.FileName))
			{
				Update_Profile_List();
				XmlRW.Save2Xml(_profileInfo, xmlFileName);
			}
			else
			{
				MessageBox.Show("The format is not correct in this profile!", "Load fail", (MessageBoxButton)0, (MessageBoxImage)64);
			}
		}
	}

	private void ExportProfile()
	{
		SaveFileDialog val = new SaveFileDialog();
		val.Title = "Export profile";
		val.Filter = "XML Files (*.xml)|*.xml";
		if (val.ShowDialog() == true && val.FileName != "")
		{
			SaveCurrentProfile(iNowIndex, val.FileName);
		}
	}

	private void Update_Profile_List()
	{
		listBox_profile.Items.Clear();
		foreach (ProfileInfo item in _profileInfo)
		{
			UserControl_profile userControl_profile = new UserControl_profile();
			userControl_profile.T_porfileName.Text = item.ProfileName;
			userControl_profile.ImageFileName = item.ProfileIconPath;
			listBox_profile.Items.Add((object)userControl_profile);
			if (item.IsSelected)
			{
				listBox_profile.SelectedItem = userControl_profile;
				iNowIndex = listBox_profile.Items.Count - 1;
			}
		}
		listBox_profile.ScrollIntoView(listBox_profile.SelectedItem);
		LoadProfile(listBox_profile.SelectedIndex);
	}

	private void listBox_profile_MouseDoubleClick(object sender, TappedEventArgs e)
	{
		if (e.Source is Image)
		{
			grid_page_key_mouse.IsEnabled = false;
			FlexiKey_PageBTN.IsEnabled = false;
			int selectedIndex = listBox_profile.SelectedIndex;
			TB_new_profile_name.Text = _profileInfo[selectedIndex].ProfileName;
			LoadImage(_profileInfo[selectedIndex].ProfileIconPath);
			grid_profile_custom.IsVisible = true;
		}
	}

	private void BTN_ProfileCustom_Cancel_Click(object sender, RoutedEventArgs e)
	{
		grid_page_key_mouse.IsEnabled = true;
		FlexiKey_PageBTN.IsEnabled = true;
		grid_profile_custom.IsVisible = false;
	}

	private void BTN_ProfileCustom_OK_Click(object sender, RoutedEventArgs e)
	{
		isCustomOKClick = true;
		_profileInfo[iNowIndex].ProfileName = TB_new_profile_name.Text;
		_profileInfo[iNowIndex].ProfileIconPath = profile_ImagePath;
		SaveCurrentProfile(iNowIndex, xmlFileName);
		int selectedIndex = listBox_profile.SelectedIndex;
		listBox_profile.Items.Clear();
		for (int i = 0; i < _profileInfo.Count; i++)
		{
			UserControl_profile userControl_profile = new UserControl_profile();
			userControl_profile.T_porfileName.Text = _profileInfo[i].ProfileName;
			listBox_profile.Items.Add((object)userControl_profile);
		}
		listBox_profile.Focus();
		listBox_profile.SelectedIndex = selectedIndex;
		grid_page_key_mouse.IsEnabled = true;
		FlexiKey_PageBTN.IsEnabled = true;
		grid_profile_custom.IsVisible = false;
		Update_Profile_List();
		isCustomOKClick = false;
	}

	private void Image_new_profile_image_MouseDown(object sender, PointerPressedEventArgs e)
	{
		OpenFileDialog val = new OpenFileDialog();
		val.Title = "Import image";
		val.Filter = "PNG Files (*.png)|*.png";
		if (val.ShowDialog() == true)
		{
			LoadImage(val.FileName);
		}
	}

	private void LoadImage(string filename)
	{
		profile_ImagePath = filename;
		if (!File.Exists(profile_ImagePath) || filename == "")
		{
			Image_new_profile_image.Source = Img.Load("Image/Page_FlexiKey/profile/btn_profile_normal.png");
		}
		else if (filename.Contains(".png") || filename.Contains(".jpg"))
		{
			using (var fs = new FileStream(filename, FileMode.Open, FileAccess.Read))
				Image_new_profile_image.Source = new Bitmap(fs);
		}
		else
		{
			// Icon.ExtractAssociatedIcon: no Windows shell icons; fall back to the default image
			Image_new_profile_image.Source = Img.Load("Image/Page_FlexiKey/profile/btn_profile_normal.png");
		}
	}

	private void RBTN_MacroKB_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		RBTN_MacroMouse.IsChecked = false; // Avalonia raises Checked before the radio group is updated (WPF: after)
		RBTN_statistics.IsChecked = false; // Avalonia raises Checked before the radio group is updated (WPF: after)
		if (isStatisticsEnabled)
		{
			if ((int)MessageBox.Show("All statistics data will vanish if you leave statistics page. Are you sure to leave?", "Warning", (MessageBoxButton)4, (MessageBoxImage)48) == 6)
			{
				grid_page_key_mouse.IsVisible = true;
				grid_Keyboard.IsVisible = true;
				grid_statistics.IsVisible = false;
				grid_profile_BTN.IsVisible = true;
				grid_description.Margin = new Thickness(0, 73.0, 507.0, 0);
				UpdateUI_KB_Mouse_Status();
				isStatisticsEnabled = false;
			}
			else
			{
				RBTN_statistics.IsChecked = true;
			}
		}
		else
		{
			grid_page_key_mouse.IsVisible = true;
			grid_Keyboard.IsVisible = true;
			grid_statistics.IsVisible = false;
			grid_profile_BTN.IsVisible = true;
			grid_description.Margin = new Thickness(0, 73.0, 507.0, 0);
			UpdateUI_KB_Mouse_Status();
		}
	}

	private void RBTN_MacroMouse_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		RBTN_MacroKB.IsChecked = false; // Avalonia raises Checked before the radio group is updated (WPF: after)
		RBTN_statistics.IsChecked = false; // Avalonia raises Checked before the radio group is updated (WPF: after)
		if (isStatisticsEnabled)
		{
			if ((int)MessageBox.Show("All statistics data will vanish if you leave statistics page. Are you sure to leave?", "Question", (MessageBoxButton)4, (MessageBoxImage)48) == 6)
			{
				grid_page_key_mouse.IsVisible = true;
				grid_Keyboard.IsVisible = false;
				grid_statistics.IsVisible = false;
				grid_profile_BTN.IsVisible = true;
				grid_description.Margin = new Thickness(0, 73.0, 507.0, 0);
				UpdateUI_KB_Mouse_Status();
				isStatisticsEnabled = false;
			}
			else
			{
				RBTN_statistics.IsChecked = true;
			}
		}
		else
		{
			grid_page_key_mouse.IsVisible = true;
			grid_Keyboard.IsVisible = false;
			grid_statistics.IsVisible = false;
			grid_profile_BTN.IsVisible = true;
			grid_description.Margin = new Thickness(0, 73.0, 507.0, 0);
			UpdateUI_KB_Mouse_Status();
		}
	}

	private void RBTN_statistics_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		RBTN_MacroKB.IsChecked = false; // Avalonia raises Checked before the radio group is updated (WPF: after)
		RBTN_MacroMouse.IsChecked = false; // Avalonia raises Checked before the radio group is updated (WPF: after)
		if (!isStatisticsEnabled)
		{
			grid_page_key_mouse.IsVisible = false;
			grid_Keyboard.IsVisible = true;
			grid_statistics.IsVisible = true;
			grid_profile_BTN.IsVisible = false;
			grid_description.Margin = new Thickness(0, 73.0, 0.0, 0);
			UpdateUI_KB_Mouse_Status();
		}
	}

	private void BTN_Reset_Click(object sender, RoutedEventArgs e)
	{
	}

	private void BTN_SAVE_Click(object sender, RoutedEventArgs e)
	{
	}

	private void SL_FlexiKey_Mouse_status_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (InitOK)
		{
			try
			{
				Clipboard.Clear();
			}
			catch (Exception ex)
			{
				string message = ex.Message;
				message += ", Clipboard.Clear() error:";
				message += getOpenClipboardWindowText();
				outlog.WriteEntry(message);
			}
			if (SL_FlexiKey_Mouse_status.Value == 1.0)
			{
				_profileInfo[iNowIndex].IsMouseEnable = true;
				RBTN_MouseStatus_Enable.IsChecked = true;
			}
			else
			{
				_profileInfo[iNowIndex].IsMouseEnable = false;
				RBTN_MouseStatus_disable.IsChecked = true;
			}
			SetMouseStatus((int)SL_FlexiKey_Mouse_status.Value);
			Global.RW_REG.FlexiKey_Mouse_Enable = (int)SL_FlexiKey_Mouse_status.Value;
			SaveCurrentProfile(iNowIndex, xmlFileName);
			UpdateUI_KB_Mouse_Status();
		}
	}

	private void SL_FlexiKey_KB_status_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (InitOK)
		{
			try
			{
				Clipboard.Clear();
			}
			catch (Exception ex)
			{
				string message = ex.Message;
				message += ", Clipboard.Clear() error:";
				message += getOpenClipboardWindowText();
				outlog.WriteEntry(message);
			}
			if (SL_FlexiKey_KB_status.Value == 1.0)
			{
				_profileInfo[iNowIndex].IsKbEnable = true;
				RBTN_KBStatus_Enable.IsChecked = true;
			}
			else
			{
				_profileInfo[iNowIndex].IsKbEnable = false;
				RBTN_KBStatus_disable.IsChecked = true;
			}
			Global.RW_REG.FlexiKey_KB_Enable = (int)SL_FlexiKey_KB_status.Value;
			SaveCurrentProfile(iNowIndex, xmlFileName);
			if (RBTN_statistics.IsChecked == false)
			{
				UpdateUI_KB_Mouse_Status();
			}
		}
	}

	private void RBTN_KBStatus_disable_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		SL_FlexiKey_KB_status.Value = 0.0;
		if (!Global.FlexikeyInitialFlag)
		{
			SetKeyboardStatus((int)SL_FlexiKey_KB_status.Value);
		}
	}

	private void RBTN_KBStatus_Enable_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		SL_FlexiKey_KB_status.Value = 1.0;
		if (!Global.FlexikeyInitialFlag)
		{
			SetKeyboardStatus((int)SL_FlexiKey_KB_status.Value);
		}
	}

	private void RBTN_MouseStatus_disable_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		SL_FlexiKey_Mouse_status.Value = 0.0;
		if (!Global.FlexikeyInitialFlag)
		{
			SetMouseStatus((int)SL_FlexiKey_Mouse_status.Value);
		}
	}

	private void RBTN_MouseStatus_Enable_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		SL_FlexiKey_Mouse_status.Value = 1.0;
		if (!Global.FlexikeyInitialFlag)
		{
			SetMouseStatus((int)SL_FlexiKey_Mouse_status.Value);
		}
	}

	private void UpdateUI_KB_Mouse_Status()
	{
		if (!InitOK)
		{
			return;
		}
		Stop_HookFunction();
		Start_Statistics_Function(bStart: false);
		if (RBTN_MacroKB.IsChecked == true)
		{
			SL_FlexiKey_KB_status.IsVisible = true;
			SL_FlexiKey_Mouse_status.IsVisible = false;
			grid_Keyboard.IsEnabled = true;
			if (SL_FlexiKey_KB_status.Value == 1.0)
			{
				grid_Disable_background.IsVisible = false;
			}
			else
			{
				grid_Disable_background.IsVisible = true;
			}
		}
		else if (RBTN_MacroMouse.IsChecked == true)
		{
			SL_FlexiKey_KB_status.IsVisible = false;
			SL_FlexiKey_Mouse_status.IsVisible = true;
			if (SL_FlexiKey_Mouse_status.Value == 1.0)
			{
				grid_Disable_background.IsVisible = false;
			}
			else
			{
				grid_Disable_background.IsVisible = true;
			}
		}
		else
		{
			SL_FlexiKey_KB_status.IsVisible = false;
			SL_FlexiKey_Mouse_status.IsVisible = false;
			grid_Disable_background.IsVisible = false;
			grid_Keyboard.IsEnabled = false;
		}
		Reset_Statistics_Color_UI();
		Cancel_KB_Mouse_UI();
	}

	public void Flexikey_Disable_Enable()
	{
		Thread.Sleep(300);
		if (Global.RW_REG.FlexiKey_KB_Enable == 1)
		{
			SL_FlexiKey_KB_status.Value = 1.0;
		}
		else
		{
			SL_FlexiKey_KB_status.Value = 0.0;
		}
		if (Global.RW_REG.FlexiKey_Mouse_Enable == 1)
		{
			SL_FlexiKey_Mouse_status.Value = 1.0;
		}
		else
		{
			SL_FlexiKey_Mouse_status.Value = 0.0;
		}
	}

	private void CB_MacroMouse_roup_Click(object sender, RoutedEventArgs e)
	{
		CheckBox val = (CheckBox)sender;
		if (val.Name != CB_MouseMouse_Left.Name)
		{
			CB_MouseMouse_Left.IsChecked = false;
		}
		if (val.Name != CB_MouseMouse_Right.Name)
		{
			CB_MouseMouse_Right.IsChecked = false;
		}
		if (val.Name != CB_MouseMouse_Mid.Name)
		{
			CB_MouseMouse_Mid.IsChecked = false;
		}
		if (val.Name != CB_MouseMouse_Backward.Name)
		{
			CB_MouseMouse_Backward.IsChecked = false;
		}
		if (val.Name != CB_MouseMouse_Foward.Name)
		{
			CB_MouseMouse_Foward.IsChecked = false;
		}
		Hidden_Action_UI();
		if (val.IsChecked == true)
		{
			Load_Action_UI(val.Name);
			Show_Action_UI(val.Content.ToString());
		}
	}

	private void CB_MacroKB_roup_Click(object sender, RoutedEventArgs e)
	{
		CheckBox val = (CheckBox)sender;
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val2 = child;
			if (val.Name != val2.Name && val2.IsChecked == true)
			{
				val2.IsChecked = false;
			}
		}
		if (RBTN_statistics.IsChecked == true)
		{
			BTN_SAVE_Statistics.IsEnabled = true;
			BTN_Restore_statistics.IsVisible = true;
			if (val.IsChecked == true)
			{
				val.Background = Statistics_color(6);
			}
			else
			{
				if (ToolTip.GetTip(val) == null)
				{
					return;
				}
				string text = ToolTip.GetTip(val).ToString();
				int num = Convert.ToInt16(text.Remove(text.Count() - 4, 4));
				switch (num)
				{
				case 0:
					num = 0;
					break;
				case 1:
				case 2:
				case 3:
				case 4:
					num = 1;
					break;
				default:
					if (num > 4 && num <= 8)
					{
						num = 2;
					}
					else if (num > 8 && num <= 12)
					{
						num = 3;
					}
					else if (num > 12 && num <= 16)
					{
						num = 4;
					}
					else if (num > 16)
					{
						num = 5;
					}
					break;
				}
				val.Background = Statistics_color(num);
			}
			return;
		}
		Hidden_Action_UI();
		if (val.IsChecked == true)
		{
			Load_Action_UI(val.Name);
			if (val.Tag == null)
			{
				Show_Action_UI("");
			}
			else
			{
				Show_Action_UI(val.Tag.ToString());
			}
		}
	}

	private void Cancel_KB_Mouse_UI()
	{
		CB_MouseMouse_Left.IsChecked = false;
		CB_MouseMouse_Right.IsChecked = false;
		CB_MouseMouse_Mid.IsChecked = false;
		CB_MouseMouse_Backward.IsChecked = false;
		CB_MouseMouse_Foward.IsChecked = false;
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val = child;
			if (val.IsChecked == true)
			{
				val.IsChecked = false;
			}
		}
		Hidden_Action_UI();
		KeyInfo_CurrentIndex = -1;
	}

	private void CB_Action_group_Click(object sender, RoutedEventArgs e)
	{
		CheckBox val = (CheckBox)sender;
		if (val.Name != CB_Express.Name)
		{
			CB_Express.IsChecked = false;
		}
		if (val.Name != CB_ExpressText.Name)
		{
			CB_ExpressText.IsChecked = false;
		}
		if (val.Name != CB_Launch.Name)
		{
			CB_Launch.IsChecked = false;
		}
		if (val.Name != CB_DisableKey.Name)
		{
			CB_DisableKey.IsChecked = false;
		}
		Update_Keyinfo();
	}

	private void Show_Action_UI(string KeyName)
	{
		if (KeyInfo_CurrentIndex != -1)
		{
			SP_ActionBTN.IsEnabled = true;
			SP_name_tips.IsVisible = true;
			T_KeyName.Text = KeyName;
			TB_Tooltip.Text = myKeyInfo[KeyInfo_CurrentIndex].Tip;
		}
	}

	private void Hidden_Action_UI()
	{
		SP_ActionBTN.IsEnabled = false;
		SP_name_tips.IsVisible = false;
		CB_Express.IsChecked = false;
		CB_ExpressText.IsChecked = false;
		CB_Launch.IsChecked = false;
		CB_DisableKey.IsChecked = false;
		TB_Tooltip.Text = "";
		Update_LaunchAPP_Icon(null);
		list_KeyGroups.Clear();
		listBox_ExpressKey.Items.Clear();
		BTN_Restore_expresss.IsVisible = false;
		T_ExpressText_send.Text = "";
		T_ExpressText_start.Text = "";
		TB_ExpressText_content.Text = "";
	}

	private void Load_Action_UI(string CheckBoxName)
	{
		int num = myKeyInfo.FindIndex((KeyInfo key) => key.CheckBoxName.Equals(CheckBoxName));
		if (num != -1)
		{
			KeyInfo_CurrentIndex = num;
			if (myKeyInfo[KeyInfo_CurrentIndex].ActionType == KeyGlobal.KeyActionType.keypress)
			{
				CB_Express.IsChecked = true;
				Update_ExpressKey_UI();
				return;
			}
			if (myKeyInfo[KeyInfo_CurrentIndex].ActionType == KeyGlobal.KeyActionType.launch)
			{
				CB_Launch.IsChecked = true;
				Update_LaunchAPP_Icon(myKeyInfo[KeyInfo_CurrentIndex].AppPath);
				return;
			}
			if (myKeyInfo[KeyInfo_CurrentIndex].ActionType == KeyGlobal.KeyActionType.text)
			{
				CB_ExpressText.IsChecked = true;
				Update_ExpressText_UI();
				return;
			}
			if (myKeyInfo[KeyInfo_CurrentIndex].ActionType == KeyGlobal.KeyActionType.disable)
			{
				CB_DisableKey.IsChecked = true;
				return;
			}
			CB_Launch.IsChecked = false;
			CB_Express.IsChecked = false;
			CB_ExpressText.IsChecked = false;
			CB_DisableKey.IsChecked = false;
			T_Tooltip.IsVisible = false;
			grid_TB_tooltip.IsVisible = false;
		}
	}

	private void TB_Tooltip_KeyUp(object sender, KeyEventArgs e)
	{
		Update_Keyinfo();
	}

	private void Update_Key_Action_icon()
	{
		if (RBTN_MacroMouse.IsChecked == true)
		{
			foreach (CheckBox child in grid_mouse_BTN.Children)
			{
				CheckBox val = child;
				if (val.Name == myKeyInfo[KeyInfo_CurrentIndex].CheckBoxName)
				{
					val.Tag = ((int)myKeyInfo[KeyInfo_CurrentIndex].ActionType).ToString();
				}
			}
			return;
		}
		if (RBTN_MacroKB.IsChecked != true)
		{
			return;
		}
		foreach (CheckBox child2 in canvas_kb.Children)
		{
			CheckBox val2 = child2;
			if (val2.Name == myKeyInfo[KeyInfo_CurrentIndex].CheckBoxName)
			{
				val2.BorderBrush = ActionType_color(myKeyInfo[KeyInfo_CurrentIndex].ActionType);
			}
		}
	}

	private IBrush ActionType_color(KeyGlobal.KeyActionType ActionType)
	{
		byte b = 0;
		byte b2 = 0;
		byte b3 = 0;
		switch (ActionType)
		{
		case KeyGlobal.KeyActionType.keypress:
			b = 88;
			b2 = 200;
			b3 = 2;
			break;
		case KeyGlobal.KeyActionType.launch:
			b = byte.MaxValue;
			b2 = 59;
			b3 = 60;
			break;
		case KeyGlobal.KeyActionType.text:
			b = 30;
			b2 = 140;
			b3 = byte.MaxValue;
			break;
		case KeyGlobal.KeyActionType.disable:
			b = 73;
			b2 = 71;
			b3 = 92;
			break;
		default:
			return new SolidColorBrush(Color.FromArgb((byte)0, b, b2, b3));
		}
		return new SolidColorBrush(Color.FromArgb(byte.MaxValue, b, b2, b3));
	}

	private void BTN_launch_browse_Click(object sender, RoutedEventArgs e)
	{
		OpenFileDialog val = new OpenFileDialog();
		val.Title = "app";
		val.Filter = "EXE Files (*.exe)|*.exe";
		if (val.ShowDialog() == true)
		{
			Update_LaunchAPP_Icon(val.FileName);
			Update_Keyinfo();
		}
	}

	private bool Update_LaunchAPP_Icon(string filename)
	{
		if (filename == null)
		{
			CB_Launch.IsChecked = false;
			image_AppIcon.Source = null;
			T_AppPath.Text = "";
			return true;
		}
		if (!File.Exists(filename))
		{
			return false;
		}
		// Icon.ExtractAssociatedIcon (shell icon): no Linux equivalent, icon left empty
		image_AppIcon.Source = null;
		T_AppPath.Text = filename;
		return true;
	}

	private void BTN_Restore_launch_Click(object sender, RoutedEventArgs e)
	{
		Update_LaunchAPP_Icon(null);
		Update_Keyinfo();
	}

	private void BTN_Restore_Disable_Click(object sender, RoutedEventArgs e)
	{
		CB_DisableKey.IsChecked = false;
		Update_Keyinfo();
	}

	private void CB_ExpressKey_Start_Click(object sender, RoutedEventArgs e)
	{
		key_sum = 0;
		key_make_num = 0;
		if (CB_ExpressKey_Start.IsChecked == true)
		{
			Update_UI_ExpressKey_RecordStart();
			return;
		}
		Update_Keyinfo();
		Update_UI_ExpressKey_RecordStop();
		Update_ExpressKey_UI();
	}

	private void Update_UI_ExpressKey_RecordStart()
	{
		BackUp_group = -1;
		listBox_ExpressKey.IsEnabled = false;
		list_KeyGroups.Clear();
		listBox_ExpressKey.Items.Clear();
		myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Clear();
		SP_ActionBTN.IsEnabled = false;
		grid_TB_tooltip.IsEnabled = false;
		grid_mouse.IsEnabled = false;
		SL_FlexiKey_Mouse_status.IsEnabled = false;
		grid_Keyboard.IsEnabled = false;
		SL_FlexiKey_KB_status.IsEnabled = false;
		grid_profile_BTN.IsHitTestVisible = false;
		BTN_Restore_expresss.IsVisible = false;
		CB_ExpressKey_EnableTime.IsChecked = true;
		myKB_Hook.Enable = true;
	}

	private void Update_UI_ExpressKey_RecordStop()
	{
		listBox_ExpressKey.IsEnabled = true;
		SP_ActionBTN.IsEnabled = true;
		grid_TB_tooltip.IsEnabled = true;
		grid_mouse.IsEnabled = true;
		SL_FlexiKey_Mouse_status.IsEnabled = true;
		grid_Keyboard.IsEnabled = true;
		SL_FlexiKey_KB_status.IsEnabled = true;
		grid_profile_BTN.IsHitTestVisible = true;
		myKB_Hook.Enable = false;
	}

	private void BTN_Restore_expresss_Click(object sender, RoutedEventArgs e)
	{
		CB_Express.IsChecked = false;
		BackUp_group = -1;
		listBox_ExpressKey.IsEnabled = false;
		list_KeyGroups.Clear();
		listBox_ExpressKey.Items.Clear();
		myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Clear();
		Update_Keyinfo();
		BTN_Restore_expresss.IsVisible = false;
	}

	private void Update_ExpressKey_UI()
	{
		list_KeyGroups.Clear();
		listBox_ExpressKey.Items.Clear();
		if (KeyInfo_CurrentIndex == -1)
		{
			return;
		}
		int count = myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count;
		int num = -1;
		bool isKeypressDelay = myKeyInfo[KeyInfo_CurrentIndex].IsKeypressDelay;
		CB_ExpressKey_EnableTime.IsChecked = isKeypressDelay;
		if (count > 0)
		{
			BTN_Restore_expresss.IsVisible = true;
		}
		else
		{
			BTN_Restore_expresss.IsVisible = false;
		}
		for (int i = 0; i < count; i++)
		{
			bool make = true;
			if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyMakeBreak == KeyGlobal.MakeBreak.KeyBreak)
			{
				make = false;
			}
			if (num != myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeysGroup)
			{
				UserControl_ExpressKey_groups userControl_ExpressKey_groups = new UserControl_ExpressKey_groups();
				userControl_ExpressKey_groups.AddKey(myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyDisplay, make, myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyDelay);
				userControl_ExpressKey_groups.BorderBrush = path_sw.Stroke;
				userControl_ExpressKey_groups.Enable_DelayTime(isKeypressDelay);
				num = myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeysGroup;
				list_KeyGroups.Add(userControl_ExpressKey_groups);
				listBox_ExpressKey.Items.Add((object)list_KeyGroups[num]);
				userControl_ExpressKey_groups.BTN_Delete_KeyGroup.Click += BTN_Delete_KeyGroup_Click;
				userControl_ExpressKey_groups.KeyDelayTimeChangeEvent += Key_groups_KeyDelayTimeChangeEvent;
			}
			else
			{
				list_KeyGroups[num].AddKey(myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyDisplay, make, myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyDelay);
			}
		}
	}

	private void Key_groups_KeyDelayTimeChangeEvent(string KeyName, bool action, int delay_time)
	{
		int selectedIndex = listBox_ExpressKey.SelectedIndex;
		int count = myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count;
		for (int i = 0; i < count; i++)
		{
			if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeysGroup != selectedIndex || !(myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyDisplay == KeyName))
			{
				continue;
			}
			if (action)
			{
				if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyMakeBreak == KeyGlobal.MakeBreak.KeyMake)
				{
					myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyDelay = delay_time;
				}
			}
			else if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyMakeBreak == KeyGlobal.MakeBreak.KeyBreak)
			{
				myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyDelay = delay_time;
			}
		}
		Update_Keyinfo();
	}

	private void CB_ExpressKey_EnableTime_Click(object sender, RoutedEventArgs e)
	{
		if (CB_ExpressKey_EnableTime.IsChecked == true)
		{
			myKeyInfo[KeyInfo_CurrentIndex].IsKeypressDelay = true;
		}
		else
		{
			myKeyInfo[KeyInfo_CurrentIndex].IsKeypressDelay = false;
		}
		Update_Keyinfo();
		Update_ExpressKey_UI();
	}

	private void BTN_Delete_KeyGroup_Click(object sender, RoutedEventArgs e)
	{
		Console.WriteLine("Delete ");
		int num = listBox_ExpressKey.SelectedIndex;
		for (int i = 0; i < myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count; i++)
		{
			if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeysGroup == num)
			{
				myKeyInfo[KeyInfo_CurrentIndex].MacroKey.RemoveAt(i);
				i--;
			}
		}
		int count = myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count;
		for (int j = 0; j < count; j++)
		{
			int num2 = myKeyInfo[KeyInfo_CurrentIndex].MacroKey[j].KeysGroup - num;
			if (num2 == 1)
			{
				myKeyInfo[KeyInfo_CurrentIndex].MacroKey[j].KeysGroup = num;
			}
			else if (num2 > 1)
			{
				num++;
				myKeyInfo[KeyInfo_CurrentIndex].MacroKey[j].KeysGroup = num;
			}
		}
		Update_Keyinfo();
		Update_ExpressKey_UI();
	}

	private void listBox_ExpressKey_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (listBox_ExpressKey.SelectedIndex != -1)
		{
			int count = list_KeyGroups.Count;
			for (int i = 0; i < count; i++)
			{
				list_KeyGroups[i].IsEnabled = false;
			}
			list_KeyGroups[listBox_ExpressKey.SelectedIndex].IsEnabled = true;
		}
	}

	private void CB_ExpressText_Start_Click(object sender, RoutedEventArgs e)
	{
		if (CB_ExpressText_Start.IsChecked == true)
		{
			grid_ExpressText_content.IsEnabled = false;
			grid_ExpressText_Send.IsEnabled = false;
			Update_UI_ExpressText_RecordStart();
			T_ExpressText_start.Text = "";
			myKeyInfo[KeyInfo_CurrentIndex].StartKey.Clear();
			myKB_Hook.Enable = true;
		}
	}

	private void TB_ExpressText_content_KeyUp(object sender, KeyEventArgs e)
	{
		grid_ExpressText_Start.IsEnabled = false;
		grid_ExpressText_Send.IsEnabled = false;
		Update_UI_ExpressText_RecordStart();
		if (TB_ExpressText_content.Text != "")
		{
			BTN_SAVE_ExpressText.IsEnabled = true;
		}
		else
		{
			BTN_SAVE_ExpressText.IsEnabled = false;
		}
	}

	private void BTN_SAVE_ExpressText_Click(object sender, RoutedEventArgs e)
	{
		myKeyInfo[KeyInfo_CurrentIndex].TextContent = TB_ExpressText_content.Text;
		BTN_SAVE_ExpressText.IsEnabled = false;
		grid_ExpressText_Start.IsEnabled = true;
		grid_ExpressText_Send.IsEnabled = true;
		Check_ExpressText_status();
	}

	private void CB_ExpressText_Send_Click(object sender, RoutedEventArgs e)
	{
		if (CB_ExpressText_Send.IsChecked == true)
		{
			grid_ExpressText_Start.IsEnabled = false;
			grid_ExpressText_content.IsEnabled = false;
			Update_UI_ExpressText_RecordStart();
			T_ExpressText_send.Text = "";
			myKeyInfo[KeyInfo_CurrentIndex].SendKey.Clear();
			myKB_Hook.Enable = true;
		}
	}

	private void Update_UI_ExpressText_RecordStart()
	{
		SP_ActionBTN.IsEnabled = false;
		grid_TB_tooltip.IsEnabled = false;
		grid_mouse.IsEnabled = false;
		SL_FlexiKey_Mouse_status.IsEnabled = false;
		grid_Keyboard.IsEnabled = false;
		SL_FlexiKey_KB_status.IsEnabled = false;
		grid_profile_BTN.IsHitTestVisible = false;
	}

	private void Update_UI_ExpressText_RecordStop()
	{
		grid_ExpressText_Start.IsEnabled = true;
		grid_ExpressText_Send.IsEnabled = true;
		grid_ExpressText_content.IsEnabled = true;
		BTN_SAVE_ExpressText.IsEnabled = false;
		SP_ActionBTN.IsEnabled = true;
		grid_TB_tooltip.IsEnabled = true;
		grid_mouse.IsEnabled = true;
		SL_FlexiKey_Mouse_status.IsEnabled = true;
		grid_Keyboard.IsEnabled = true;
		SL_FlexiKey_KB_status.IsEnabled = true;
		grid_profile_BTN.IsHitTestVisible = true;
		myKB_Hook.Enable = false;
	}

	private void Check_ExpressText_status()
	{
		if (T_ExpressText_start.Text != "" && T_ExpressText_send.Text != "" && TB_ExpressText_content.Text != "")
		{
			Update_UI_ExpressText_RecordStop();
			Update_Keyinfo();
		}
		else
		{
			Update_UI_ExpressText_RecordStop();
			Update_UI_ExpressText_RecordStart();
		}
	}

	private void BTN_Restore_express_text_Click(object sender, RoutedEventArgs e)
	{
		T_ExpressText_send.Text = "";
		T_ExpressText_start.Text = "";
		TB_ExpressText_content.Text = "";
		myKeyInfo[KeyInfo_CurrentIndex].StartKey.Clear();
		myKeyInfo[KeyInfo_CurrentIndex].SendKey.Clear();
		myKeyInfo[KeyInfo_CurrentIndex].TextContent = "";
		Update_UI_ExpressText_RecordStop();
		Update_Keyinfo();
	}

	private void Update_ExpressText_UI()
	{
		T_ExpressText_send.Text = "";
		T_ExpressText_start.Text = "";
		TB_ExpressText_content.Text = "";
		if (KeyInfo_CurrentIndex != -1)
		{
			T_ExpressText_send.Text = CombineMacroKey(myKeyInfo[KeyInfo_CurrentIndex].SendKey);
			T_ExpressText_start.Text = CombineMacroKey(myKeyInfo[KeyInfo_CurrentIndex].StartKey);
			TB_ExpressText_content.Text = myKeyInfo[KeyInfo_CurrentIndex].TextContent;
		}
	}

	private string CombineMacroKey(List<MacrokeyContent> mc)
	{
		if (mc.Count == 0 || mc == null)
		{
			return "";
		}
		string text = "";
		foreach (MacrokeyContent item in mc)
		{
			if (item.KeyMakeBreak == KeyGlobal.MakeBreak.KeyMake)
			{
				text = ((!(text == "")) ? (text + item.KeyDisplay) : item.KeyDisplay);
			}
		}
		return text;
	}

	private void MyKB_Hook_KeyPress(object sender, EventArgs e)
	{
		KeyboardEventArgs e2 = e as KeyboardEventArgs;
		if (!Add_KeyList_To_KeyInfo(e2.KeyValue, e2.Status, e2.DelayTime))
		{
			return;
		}
		_ = CB_ExpressText.IsChecked == true;
		if (CB_Express.IsChecked == true)
		{
			int index = myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count - 1;
			bool make = true;
			if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey[index].KeyMakeBreak == KeyGlobal.MakeBreak.KeyBreak)
			{
				make = false;
			}
			if (BackUp_group != myKeyInfo[KeyInfo_CurrentIndex].MacroKey[index].KeysGroup)
			{
				UserControl_ExpressKey_groups userControl_ExpressKey_groups = new UserControl_ExpressKey_groups();
				userControl_ExpressKey_groups.AddKey(myKeyInfo[KeyInfo_CurrentIndex].MacroKey[index].KeyDisplay, make, myKeyInfo[KeyInfo_CurrentIndex].MacroKey[index].KeyDelay);
				userControl_ExpressKey_groups.BorderBrush = path_sw.Stroke;
				BackUp_group = myKeyInfo[KeyInfo_CurrentIndex].MacroKey[index].KeysGroup;
				list_KeyGroups.Add(userControl_ExpressKey_groups);
				listBox_ExpressKey.Items.Add((object)list_KeyGroups[BackUp_group]);
				userControl_ExpressKey_groups.BTN_Delete_KeyGroup.Click += BTN_Delete_KeyGroup_Click;
				userControl_ExpressKey_groups.KeyDelayTimeChangeEvent += Key_groups_KeyDelayTimeChangeEvent;
			}
			else
			{
				list_KeyGroups[BackUp_group].AddKey(myKeyInfo[KeyInfo_CurrentIndex].MacroKey[index].KeyDisplay, make, myKeyInfo[KeyInfo_CurrentIndex].MacroKey[index].KeyDelay);
			}
		}
	}

	private bool Add_KeyList_To_KeyInfo(int KeyCode, int KeyStatus, int KeyDelay)
	{
		int num = 0;
		string text = "";
		text = KeyGlobal.hookKeysTable.GetScanCodeKeysDisplay(KeyCode.ToString());
		if (CB_ExpressText.IsChecked == true)
		{
			if (CB_ExpressText_Start.IsChecked == true)
			{
				myKeyInfo[KeyInfo_CurrentIndex].StartKey = CheckTextMacroKey(myKeyInfo[KeyInfo_CurrentIndex].StartKey, KeyCode, KeyStatus, KeyDelay, text);
			}
			if (CB_ExpressText_Send.IsChecked == true)
			{
				myKeyInfo[KeyInfo_CurrentIndex].SendKey = CheckTextMacroKey(myKeyInfo[KeyInfo_CurrentIndex].SendKey, KeyCode, KeyStatus, KeyDelay, text);
			}
		}
		if (CB_Express.IsChecked == true)
		{
			if (!CheckExpressLimit(KeyStatus))
			{
				return false;
			}
			MacrokeyContent macrokeyContent = new MacrokeyContent();
			macrokeyContent.KeyCode = KeyCode;
			macrokeyContent.KeyDisplay = text;
			switch (KeyStatus)
			{
			case 0:
				macrokeyContent.KeyMakeBreak = KeyGlobal.MakeBreak.KeyMake;
				break;
			case 1:
				macrokeyContent.KeyMakeBreak = KeyGlobal.MakeBreak.KeyBreak;
				break;
			}
			macrokeyContent.KeyDelay = KeyDelay;
			if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count == 0)
			{
				myKeyInfo[KeyInfo_CurrentIndex].MacroKeyGruopFlag = 0;
				macrokeyContent.KeysGroup = 0;
				myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Add(macrokeyContent);
				return true;
			}
			int currtGroup = myKeyInfo[KeyInfo_CurrentIndex].MacroKeyGruopFlag;
			num = myKeyInfo[KeyInfo_CurrentIndex].MacroKey.FindIndex((MacrokeyContent _i) => _i.KeysGroup.Equals(currtGroup));
			int i;
			for (i = num; i < myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Count; i++)
			{
				if (myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyMakeBreak == KeyGlobal.MakeBreak.KeyMake)
				{
					int num2 = myKeyInfo[KeyInfo_CurrentIndex].MacroKey.FindLastIndex((MacrokeyContent _i) => _i.KeyCode.Equals(myKeyInfo[KeyInfo_CurrentIndex].MacroKey[i].KeyCode) && _i.KeyMakeBreak.Equals(KeyGlobal.MakeBreak.KeyBreak));
					if (num2 == -1 || num2 <= num)
					{
						macrokeyContent.KeysGroup = currtGroup;
						myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Add(macrokeyContent);
						myKeyInfo[KeyInfo_CurrentIndex].ActionType = KeyGlobal.KeyActionType.keypress;
						return true;
					}
				}
			}
			myKeyInfo[KeyInfo_CurrentIndex].MacroKeyGruopFlag = myKeyInfo[KeyInfo_CurrentIndex].MacroKeyGruopFlag + 1;
			macrokeyContent.KeysGroup = myKeyInfo[KeyInfo_CurrentIndex].MacroKeyGruopFlag;
			myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Add(macrokeyContent);
		}
		return true;
	}

	private bool CheckExpressLimit(int KeyStatus)
	{
		if (key_sum >= 32)
		{
			return false;
		}
		if (key_make_num >= 16 && KeyStatus.Equals(0))
		{
			return false;
		}
		if (KeyStatus.Equals(0))
		{
			key_make_num++;
		}
		key_sum++;
		return true;
	}

	private List<MacrokeyContent> CheckTextMacroKey(List<MacrokeyContent> textMacro, int KeyCode, int KeyStatus, int KeyDelay, string KeyDisplay)
	{
		MacrokeyContent macrokeyContent = new MacrokeyContent();
		macrokeyContent.KeyCode = KeyCode;
		macrokeyContent.KeyDelay = KeyDelay;
		macrokeyContent.KeyDisplay = KeyDisplay;
		if (KeyStatus == 0)
		{
			macrokeyContent.KeyMakeBreak = KeyGlobal.MakeBreak.KeyMake;
		}
		else
		{
			macrokeyContent.KeyMakeBreak = KeyGlobal.MakeBreak.KeyBreak;
		}
		if (textMacro.Count == 0)
		{
			textMacro.Add(macrokeyContent);
		}
		else if (textMacro[0].KeyCode == macrokeyContent.KeyCode)
		{
			textMacro.Add(macrokeyContent);
		}
		else if ((textMacro[0].KeyCode == 42 || textMacro[0].KeyCode == 54 || textMacro[0].KeyCode == 56 || textMacro[0].KeyCode == 57400 || textMacro[0].KeyCode == 29 || textMacro[0].KeyCode == 57373) && textMacro.Count == 1)
		{
			textMacro.Add(macrokeyContent);
		}
		else if (textMacro.Count == 2 && textMacro[1].KeyCode == macrokeyContent.KeyCode)
		{
			textMacro.Add(macrokeyContent);
		}
		else if (textMacro.Count == 3 && textMacro[1].KeyCode == macrokeyContent.KeyCode && textMacro[1].KeyCode != textMacro[2].KeyCode)
		{
			textMacro.Add(macrokeyContent);
		}
		bool flag = false;
		if (macrokeyContent.KeyMakeBreak == KeyGlobal.MakeBreak.KeyBreak)
		{
			int i;
			for (i = 0; i < textMacro.Count; i++)
			{
				if (textMacro[i].KeyMakeBreak == KeyGlobal.MakeBreak.KeyMake && textMacro.FindLastIndex((MacrokeyContent _i) => _i.KeyCode.Equals(textMacro[i].KeyCode) && _i.KeyMakeBreak.Equals(KeyGlobal.MakeBreak.KeyBreak)) == -1)
				{
					flag = true;
					break;
				}
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			string text = "";
			foreach (MacrokeyContent item in textMacro)
			{
				if (item.KeyMakeBreak == KeyGlobal.MakeBreak.KeyMake)
				{
					text = ((!(text == "")) ? (text + " " + item.KeyDisplay) : item.KeyDisplay);
				}
			}
			Console.WriteLine("str=" + text);
			if (CB_ExpressText.IsChecked == true)
			{
				if (CB_ExpressText_Send.IsChecked == true)
				{
					T_ExpressText_send.Text = text;
				}
				if (CB_ExpressText_Start.IsChecked == true)
				{
					T_ExpressText_start.Text = text;
				}
				CB_ExpressText_Start.IsChecked = false;
				CB_ExpressText_Send.IsChecked = false;
				myKB_Hook.Enable = false;
				Check_ExpressText_status();
			}
		}
		return textMacro;
	}

	private void Stop_HookFunction()
	{
		if (CB_ExpressKey_Start.IsChecked == true)
		{
			CB_ExpressKey_Start.IsChecked = false;
			CB_Express.IsChecked = false;
			BackUp_group = -1;
			listBox_ExpressKey.IsHitTestVisible = false;
			list_KeyGroups.Clear();
			listBox_ExpressKey.Items.Clear();
			if (KeyInfo_CurrentIndex != -1)
			{
				myKeyInfo[KeyInfo_CurrentIndex].MacroKey.Clear();
				Update_Keyinfo();
			}
			Update_UI_ExpressKey_RecordStop();
		}
		if (CB_ExpressText.IsChecked != true)
		{
			return;
		}
		if (CB_ExpressText_Start.IsChecked == true || CB_ExpressText_Send.IsChecked == true)
		{
			CB_ExpressText_Start.IsChecked = false;
			CB_ExpressText_Send.IsChecked = false;
			T_ExpressText_send.Text = "";
			T_ExpressText_start.Text = "";
			TB_ExpressText_content.Text = "";
			if (KeyInfo_CurrentIndex != -1)
			{
				myKeyInfo[KeyInfo_CurrentIndex].StartKey.Clear();
				myKeyInfo[KeyInfo_CurrentIndex].SendKey.Clear();
				myKeyInfo[KeyInfo_CurrentIndex].TextContent = "";
				Update_Keyinfo();
			}
		}
		Update_UI_ExpressText_RecordStop();
	}

	private void Statistics_timer_Tick(object sender, EventArgs e)
	{
		sta_sec++;
		if (sta_sec >= 60)
		{
			sta_time_m++;
			sta_sec = 0;
		}
		if (sta_time_m >= 60)
		{
			sta_time_h++;
			sta_time_m = 0;
		}
		T_statistics_time.Text = sta_time_h + ":" + sta_time_m.ToString().PadLeft(2, '0') + ":" + sta_sec.ToString().PadLeft(2, '0');
		isStatisticsEnabled = true;
	}

	private void CB_Statistics_Start_Click(object sender, RoutedEventArgs e)
	{
		CB_Statistics_Start.Focusable = false;
		if (CB_Statistics_Start.IsChecked == true)
		{
			Start_Statistics_Function(bStart: true);
		}
		else
		{
			Start_Statistics_Function(bStart: false);
		}
	}

	private void Start_Statistics_Function(bool bStart)
	{
		if (bStart)
		{
			grid_statistics_time.IsVisible = true;
			Reset_Statistics_Color_UI();
			Set2Driver.StartKbSta();
			statistics_timer.Start();
			grid_Keyboard.IsEnabled = false;
			return;
		}
		CB_Statistics_Start.IsChecked = false;
		grid_Keyboard.IsEnabled = true;
		statistics_timer.Stop();
		T_statistics_time.Text = "0:00:00";
		sta_sec = 0;
		sta_time_m = 0;
		sta_time_h = 0;
		grid_statistics_time.IsVisible = false;
		sta.Clear();
		if (Set2Driver.StopKbSta())
		{
			sta = Set2Driver.GetKbStaLog();
			if (sta.Count == 0)
			{
				grid_Keyboard.IsEnabled = false;
				Reset_Statistics_Color_UI();
			}
			else
			{
				Update_Statistics_Color_UI(sta);
			}
		}
	}

	private void Reset_Statistics_Color_UI()
	{
		grid_statistics_time.IsVisible = true;
		T_statistics_time.Text = "0:00:00";
		grid_qick_disable.IsVisible = false;
		foreach (CheckBox child in canvas_kb.Children)
		{
			child.IsChecked = false;
			child.Background = Statistics_color(255);
			ToolTip.SetTip(child, null);
		}
	}

	private void Update_Statistics_Color_UI(List<KbStaClass> KBsta)
	{
		grid_qick_disable.IsVisible = true;
		CB_Quick_DIS0.IsEnabled = false;
		CB_Quick_DIS4.IsEnabled = false;
		CB_Quick_DIS8.IsEnabled = false;
		CB_Quick_DIS12.IsEnabled = false;
		CB_Quick_DIS16.IsEnabled = false;
		CB_Quick_DIS20.IsEnabled = false;
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val = child;
			if (val.Name.Contains("CB_keyCode_"))
			{
				val.Background = Statistics_color(0);
				ToolTip.SetTip(val, "0 time");
			}
		}
		for (int i = 0; i < KBsta.Count; i++)
		{
			int keyCount = KBsta[i].KeyCount;
			int staLevel = KBsta[i].StaLevel;
			string text = "CB_keyCode_" + KBsta[i].KeyCode;
			foreach (CheckBox child2 in canvas_kb.Children)
			{
				CheckBox val2 = child2;
				_ = val2.Name;
				if (val2.Name == text)
				{
					val2.Background = Statistics_color(staLevel);
					ToolTip.SetTip(val2, keyCount + " time");
					switch (staLevel)
					{
					case 1:
						CB_Quick_DIS4.IsEnabled = true;
						break;
					case 2:
						CB_Quick_DIS8.IsEnabled = true;
						break;
					case 3:
						CB_Quick_DIS12.IsEnabled = true;
						break;
					case 4:
						CB_Quick_DIS16.IsEnabled = true;
						break;
					case 5:
						CB_Quick_DIS20.IsEnabled = true;
						break;
					}
				}
			}
		}
		foreach (CheckBox child3 in canvas_kb.Children)
		{
			CheckBox val3 = child3;
			if (ToolTip.GetTip(val3) == null)
			{
				break;
			}
			if (ToolTip.GetTip(val3).ToString() == "0 time")
			{
				CB_Quick_DIS0.IsEnabled = true;
				break;
			}
		}
	}

	private IBrush Statistics_color(int level)
	{
		byte b = 0;
		byte b2 = 0;
		byte b3 = 0;
		switch (level)
		{
		case 0:
			b = 0;
			b2 = 99;
			b3 = 105;
			break;
		case 1:
			b = 0;
			b2 = 197;
			b3 = 216;
			break;
		case 2:
			b = 0;
			b2 = 99;
			b3 = 165;
			break;
		case 3:
			b = 2;
			b2 = 31;
			b3 = 133;
			break;
		case 4:
			b = 28;
			b2 = 9;
			b3 = 90;
			break;
		case 5:
			b = 73;
			b2 = 1;
			b3 = 129;
			break;
		case 6:
			b = 73;
			b2 = 71;
			b3 = 92;
			break;
		default:
			return new SolidColorBrush(Color.FromArgb((byte)0, b, b2, b3));
		}
		return new SolidColorBrush(Color.FromArgb(byte.MaxValue, b, b2, b3));
	}

	private void CB_Quick_DIS0_Click(object sender, RoutedEventArgs e)
	{
		if (CB_Quick_DIS0.IsChecked == true)
		{
			Quick_Disable_Function(bDisable: true, 0);
		}
		else
		{
			Quick_Disable_Function(bDisable: false, 0);
		}
	}

	private void CB_Quick_DIS4_Click(object sender, RoutedEventArgs e)
	{
		if (CB_Quick_DIS4.IsChecked == true)
		{
			Quick_Disable_Function(bDisable: true, 1);
		}
		else
		{
			Quick_Disable_Function(bDisable: false, 1);
		}
	}

	private void CB_Quick_DIS8_Click(object sender, RoutedEventArgs e)
	{
		if (CB_Quick_DIS8.IsChecked == true)
		{
			Quick_Disable_Function(bDisable: true, 2);
		}
		else
		{
			Quick_Disable_Function(bDisable: false, 2);
		}
	}

	private void CB_Quick_DIS12_Click(object sender, RoutedEventArgs e)
	{
		if (CB_Quick_DIS12.IsChecked == true)
		{
			Quick_Disable_Function(bDisable: true, 3);
		}
		else
		{
			Quick_Disable_Function(bDisable: false, 3);
		}
	}

	private void CB_Quick_DIS16_Click(object sender, RoutedEventArgs e)
	{
		if (CB_Quick_DIS16.IsChecked == true)
		{
			Quick_Disable_Function(bDisable: true, 4);
		}
		else
		{
			Quick_Disable_Function(bDisable: false, 4);
		}
	}

	private void CB_Quick_DIS20_Click(object sender, RoutedEventArgs e)
	{
		if (CB_Quick_DIS20.IsChecked == true)
		{
			Quick_Disable_Function(bDisable: true, 5);
		}
		else
		{
			Quick_Disable_Function(bDisable: false, 5);
		}
	}

	private void Quick_Disable_Function(bool bDisable, int Disable_level)
	{
		BTN_SAVE_Statistics.IsEnabled = true;
		BTN_Restore_statistics.IsVisible = true;
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val = child;
			val.IsChecked = false;
			if (ToolTip.GetTip(val) == null)
			{
				continue;
			}
			string text = ToolTip.GetTip(val).ToString();
			int num = Convert.ToInt16(text.Remove(text.Count() - 4, 4));
			switch (num)
			{
			case 0:
				num = 0;
				break;
			case 1:
			case 2:
			case 3:
			case 4:
				num = 1;
				break;
			default:
				if (num > 4 && num <= 8)
				{
					num = 2;
				}
				else if (num > 8 && num <= 12)
				{
					num = 3;
				}
				else if (num > 12 && num <= 16)
				{
					num = 4;
				}
				else if (num > 16)
				{
					num = 5;
				}
				break;
			}
			if (num == Disable_level)
			{
				if (bDisable)
				{
					val.Background = Statistics_color(6);
				}
				else
				{
					val.Background = Statistics_color(Disable_level);
				}
			}
		}
	}

	private void BTN_SAVE_Statistics_Click(object sender, RoutedEventArgs e)
	{
		BTN_SAVE_Statistics.IsEnabled = false;
		BTN_Restore_statistics.IsVisible = false;
		foreach (CheckBox child in canvas_kb.Children)
		{
			CheckBox val = child;
			Color color = ((SolidColorBrush)val.Background).Color;
			if (color.R == 73 && color.G == 71 && color.B == 92)
			{
				Save_Statistics_KeyInfo(val.Name);
				val.BorderBrush = ActionType_color(KeyGlobal.KeyActionType.disable);
			}
			else if (bIsStatisticsSaved && color.R != 73 && color.G != 71 && color.B != 92)
			{
				Save_Statistics_KeyInfo_None(val.Name);
				val.BorderBrush = ActionType_color(KeyGlobal.KeyActionType.none);
			}
		}
		if (SL_FlexiKey_KB_status.Value == 0.0)
		{
			SL_FlexiKey_KB_status.Value = 1.0;
		}
		else
		{
			SaveCurrentProfile(iNowIndex, xmlFileName);
		}
		Update_Statistics_Color_UI(sta);
		bIsStatisticsSaved = true;
	}

	private void Save_Statistics_KeyInfo(string CheckBoxName)
	{
		int num = myKeyInfo.FindIndex((KeyInfo key) => key.CheckBoxName.Equals(CheckBoxName));
		if (num != -1)
		{
			myKeyInfo[num].ActionType = KeyGlobal.KeyActionType.disable;
		}
	}

	private void Save_Statistics_KeyInfo_None(string CheckBoxName)
	{
		int num = myKeyInfo.FindIndex((KeyInfo key) => key.CheckBoxName.Equals(CheckBoxName));
		if (num != -1)
		{
			myKeyInfo[num].ActionType = KeyGlobal.KeyActionType.none;
		}
	}

	private void BTN_Restore_statistics_Click(object sender, RoutedEventArgs e)
	{
		BTN_SAVE_Statistics.IsEnabled = false;
		BTN_Restore_statistics.IsVisible = false;
		CB_Quick_DIS0.IsChecked = false;
		CB_Quick_DIS4.IsChecked = false;
		CB_Quick_DIS8.IsChecked = false;
		CB_Quick_DIS12.IsChecked = false;
		CB_Quick_DIS16.IsChecked = false;
		CB_Quick_DIS20.IsChecked = false;
		Update_Statistics_Color_UI(sta);
	}

	public string TranslationLanguage(string Text)
	{
		StringBuilder stringBuilder = new StringBuilder(255);
		string def = "null";
		int bufferSize = 255;
		string filePath = System.IO.Path.Combine(AppContext.BaseDirectory, "mtlanguage.ini");
		GetPrivateProfileString((CleDef.UI_Language)GetUILanguage() switch
		{
			CleDef.UI_Language.system => CultureInfo.CurrentCulture.Name.ToLower(), 
			CleDef.UI_Language.en_us => "en-us", 
			CleDef.UI_Language.en_gb => "en-us", 
			CleDef.UI_Language.es => "es", 
			CleDef.UI_Language.de => "de", 
			CleDef.UI_Language.it => "it", 
			CleDef.UI_Language.ja => "ja", 
			CleDef.UI_Language.ko => "ko", 
			CleDef.UI_Language.zh_cn => "zh-cn", 
			CleDef.UI_Language.zh_tw => "zh-tw", 
			CleDef.UI_Language.fr_fr => "fr-fr", 
			CleDef.UI_Language.fr_ca => "fr-ca", 
			CleDef.UI_Language.pt_br => "pt-br", 
			CleDef.UI_Language.pt_pt => "pt-pt", 
			CleDef.UI_Language.tr_tr => "tr-tr", 
			CleDef.UI_Language.th_th => "th-th", 
			CleDef.UI_Language.vi_vn => "vi-vn", 
			CleDef.UI_Language.ru => "ru", 
			CleDef.UI_Language.ja_ep => "ja-ep", 
			_ => "en-us", 
		}, Text, def, stringBuilder, bufferSize, filePath);
		if (stringBuilder.ToString().Equals("null"))
		{
			GetPrivateProfileString("en-us", Text, def, stringBuilder, bufferSize, filePath);
		}
		return stringBuilder.ToString();
	}

	public byte GetUILanguage()
	{
		int page = 1;
		int offset = 2;
		int length = 1;
		try
		{
			byte[] array = DCHU.ReadAppSettings(page, offset, length);
			if (array[0] == 6 && CustomId == 5)
			{
				return 18;
			}
			return array[0];
		}
		catch
		{
			MessageBox.Show("Error");
		}
		return 0;
	}

	private string getOpenClipboardWindowText()
	{
		return "";
	}

	private void showAllKey()
	{
		foreach (CheckBox child in canvas_kb.Children)
		{
			child.IsVisible = true;
		}
	}

	private void HideAllKey()
	{
		foreach (CheckBox child in canvas_kb.Children)
		{
			child.IsVisible = false;
		}
	}
}
