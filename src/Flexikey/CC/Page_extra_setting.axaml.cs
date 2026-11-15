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
using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using CC.UControl;

namespace CC;

public partial class Page_extra_setting : UserControl
{
	private bool Init = true;

	private string Lang_Disconnected = "Disconnected";

	private string Lang_Connected = "Connected";

	/// <summary>hp.dll Set_effect (headphone effect, Windows audio driver): no Linux equivalent, no-op.</summary>
	public static int Set_effect(int effect) => 0;

	public Page_extra_setting()
	{
		InitializeComponent();
	}

	public void init()
	{
		string userDomainName = Environment.UserDomainName;
		T_NB_name.Text = userDomainName;
		if (!Global.support_bit.Switch_MSHybrid_DGPU)
		{
			grid_switch.IsVisible = false;
		}
		else
		{
			SL_GPUSwitch.Value = Global.RW_REG.Get_GPUSwitchStatus();
			if (SL_GPUSwitch.Value == 0.0)
			{
				RBTN_GPUSwitch_MSHybrid.IsChecked = true;
			}
			else
			{
				RBTN_GPUSwitch_Discrete.IsChecked = true;
			}
			UC_DialogBox.CancelEvent += UC_DialogBox_CancelEvent;
			UC_DialogBox.OKEvent += UC_DialogBox_OKEvent;
		}
		if (!Global.support_bit._3DHP)
		{
			SP_Hpadphone.Children.Remove((Control)SP_HP);
		}
		else
		{
			Update_HP_Status(Global.RW_REG.HeadPhone);
		}
		if (!Global.support_bit.ESS)
		{
			SP_Hpadphone.Children.Remove((Control)SP_ESS);
		}
		if (!Global.support_bit._3DHP && !Global.support_bit.ESS)
		{
			grid_Headphone.IsVisible = false;
		}
		if (Global.RW_REG.Support_Page_SystemMonitor() == 0)
		{
			grid_FA.IsVisible = false;
		}
		else if (Global.RW_REG.Support_Page_SystemMonitor() == 1)
		{
			grid_FA.IsVisible = true;
		}
		Init = false;
		LoadMultiLang();
	}

	private void LoadMultiLang()
	{
		T_SkinColor.Text = Global.MultiLang.Translation("_ChangeSkinColor");
		T_HPI.Text = Global.MultiLang.Translation("_HeadphoneImpedance");
		T_HP.Text = Global.MultiLang.Translation("_Headphone");
		CB_HP_music.Content = Global.MultiLang.Translation("_MUSIC");
		CB_HP_game.Content = Global.MultiLang.Translation("_GAME");
		CB_HP_theater.Content = Global.MultiLang.Translation("_THEATER");
		CB_HP_direct.Content = Global.MultiLang.Translation("_DIRECT");
		t_GPUSwitch.Text = Global.MultiLang.Translation("_GPU Switch");
		RBTN_GPUSwitch_MSHybrid.Content = Global.MultiLang.Translation("_MSHybrid");
		RBTN_GPUSwitch_Discrete.Content = Global.MultiLang.Translation("_Discrete");
		T_IPAddress.Text = Global.MultiLang.Translation("_IPAddress");
		tb_port_txt.Text = Global.MultiLang.Translation("_Port");
		RB_FA_ON.Content = Global.MultiLang.Translation("_ON");
		RB_FA_OFF.Content = Global.MultiLang.Translation("_OFF");
		BTN_QRCode.Content = Global.MultiLang.Translation("_QRCode");
		Lang_Disconnected = Global.MultiLang.Translation("_Disconnected");
		Lang_Connected = Global.MultiLang.Translation("_Connected");
	}

	public void Open_UI()
	{
		main.IsVisible = true;
		if (!Global.support_bit.OC_GPU)
		{
			grid_FA.IsVisible = false;
		}
		else if (Global.RW_REG.FlexiAccessStatus == 1)
		{
			Update_FA_IP();
			RB_FA_ON.IsChecked = true;
			Image_Page_ExtraSetting_ic_phone_png.Opacity = 0.2;
		}
		else if (Global.RW_REG.FlexiAccessStatus == 2)
		{
			Update_FA_IP();
			RB_FA_ON.IsChecked = true;
			Image_Page_ExtraSetting_ic_phone_png.Opacity = 1.0;
		}
		else if (Global.RW_REG.FlexiAccessStatus == 3)
		{
			Update_FA_IP();
			RB_FA_ON.IsChecked = true;
			Image_Page_ExtraSetting_ic_phone_png.Opacity = 0.2;
		}
		else
		{
			RB_FA_OFF.IsChecked = true;
			Image_Page_ExtraSetting_ic_phone_png.Opacity = 0.2;
		}
	}

	public void Close_UI()
	{
		main.IsVisible = false;
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}

	public void Update_Skin_color()
	{
		switch (12848284)
		{
		case 16711680:
			RB_Skin_FF0000.IsChecked = true;
			break;
		case 16740864:
			RB_Skin_FF7200.IsChecked = true;
			break;
		case 16771584:
			RB_Skin_FFEA00.IsChecked = true;
			break;
		case 10682112:
			RB_Skin_A2FF00.IsChecked = true;
			break;
		case 3997440:
			RB_Skin_3CFF00.IsChecked = true;
			break;
		case 65508:
			RB_Skin_00FFE4.IsChecked = true;
			break;
		case 26367:
			RB_Skin_0066FF.IsChecked = true;
			break;
		case 10617087:
			RB_Skin_A200FF.IsChecked = true;
			break;
		case 16711890:
			RB_Skin_FF00D2.IsChecked = true;
			break;
		case 16711788:
			RB_Skin_FF006C.IsChecked = true;
			break;
		}
	}

	private void RB_Skin_Color_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		int num = 0;
		byte b;
		byte b2;
		byte b3;
		if (RB_Skin_FF0000.IsChecked == true)
		{
			b = byte.MaxValue;
			b2 = 0;
			b3 = 0;
		}
		else if (RB_Skin_FF7200.IsChecked == true)
		{
			b = byte.MaxValue;
			b2 = 114;
			b3 = 0;
		}
		else if (RB_Skin_FFEA00.IsChecked == true)
		{
			b = byte.MaxValue;
			b2 = 234;
			b3 = 0;
		}
		else if (RB_Skin_A2FF00.IsChecked == true)
		{
			b = 162;
			b2 = byte.MaxValue;
			b3 = 0;
		}
		else if (RB_Skin_3CFF00.IsChecked == true)
		{
			b = 60;
			b2 = byte.MaxValue;
			b3 = 0;
		}
		else if (RB_Skin_00FFE4.IsChecked == true)
		{
			b = 0;
			b2 = byte.MaxValue;
			b3 = 228;
		}
		else if (RB_Skin_0066FF.IsChecked == true)
		{
			b = 0;
			b2 = 102;
			b3 = byte.MaxValue;
		}
		else if (RB_Skin_A200FF.IsChecked == true)
		{
			b = 162;
			b2 = 0;
			b3 = byte.MaxValue;
		}
		else if (RB_Skin_FF00D2.IsChecked == true)
		{
			b = byte.MaxValue;
			b2 = 0;
			b3 = 210;
		}
		else if (RB_Skin_FF006C.IsChecked == true)
		{
			b = byte.MaxValue;
			b2 = 0;
			b3 = 108;
		}
		else
		{
			b = byte.MaxValue;
			b2 = 0;
			b3 = 0;
		}
		IBrush borderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, b, b2, b3));
		userControl_Extra.BorderBrush = borderBrush;
		num = (b << 16) + (b2 << 8) + b3;
		Global.RW_REG.window_Color = num;
	}

	public void FlexikeySetColor()
	{
		IBrush borderBrush = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)196, (byte)12, (byte)156));
		userControl_Extra.BorderBrush = borderBrush;
	}

	private void BTN_FA_QR_close_Click(object sender, RoutedEventArgs e)
	{
		grid_FA_QR.IsVisible = false;
		BTN_QRCode.IsEnabled = true;
	}

	private void QR_I_MouseDown(object sender, PointerPressedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo("https://itunes.apple.com/app/flexiaccess/id1170989382?mt=8") { UseShellExecute = true });
		}
		catch
		{
		}
	}

	private void QR_A_MouseDown(object sender, PointerPressedEventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo("https://play.google.com/store/apps/details?id=com.fa.flexiaccess") { UseShellExecute = true });
		}
		catch
		{
		}
	}

	private void BTN_QRCode_Click(object sender, RoutedEventArgs e)
	{
		BTN_QRCode.IsEnabled = false;
		grid_FA_QR.IsVisible = true;
	}

	public void Update_FA_IP()
	{
		string text = "";
		text = Global.RW_REG.FA_ClientName;
		if (text.Length == 0)
		{
			T_connect_staus.Text = "Disconnected";
			T_phome_name.Text = "";
			Image_Page_ExtraSetting_ic_phone_png.Opacity = 0.2;
		}
		else
		{
			T_connect_staus.Text = "Connected";
			T_phome_name.Text = text;
			Image_Page_ExtraSetting_ic_phone_png.Opacity = 1.0;
		}
		try
		{
			string fA_Port = Global.RW_REG.FA_Port;
			tb_port_value.Text = fA_Port;
			if (fA_Port.Equals(string.Empty))
			{
				T_IP_value.Text = string.Empty;
				return;
			}
			IPHostEntry hostEntry = Dns.GetHostEntry(Dns.GetHostName());
			string text2 = "";
			IPAddress[] addressList = hostEntry.AddressList;
			foreach (IPAddress iPAddress in addressList)
			{
				if (iPAddress.AddressFamily == AddressFamily.InterNetwork)
				{
					Console.WriteLine("Local IP: " + iPAddress.ToString());
					text2 = iPAddress.ToString();
				}
			}
			T_IP_value.Text = text2;
			if (Global.RW_REG.FA_IP != text2)
			{
				Open_FA20_exe();
			}
		}
		catch
		{
			tb_port_value.Text = string.Empty;
		}
	}

	// FASer21.exe (FlexiAccess phone-remote server, Windows only): not started/killed on Linux, only the status value is kept.
	private void Open_FA20_exe()
	{
		Console.WriteLine("Open_FA20_exe: FASer21.exe not available on Linux");
		Global.RW_REG.FlexiAccessStatus = 1;
	}

	private void Close_FA20_exe()
	{
		Console.WriteLine("Close_FA20_exe: FASer21.exe not available on Linux");
		Global.RW_REG.FlexiAccessStatus = 0;
	}

	private void RB_FA_OFF_Click(object sender, RoutedEventArgs e)
	{
		RB_FA_ON.IsEnabled = false;
		RB_FA_OFF.IsEnabled = false;
		if (RB_FA_OFF.IsChecked == true)
		{
			Close_FA20_exe();
			T_IP_value.Text = "";
			T_connect_staus.Text = "";
			tb_port_value.Text = "";
			Global.clevo_msg.SendToHKTray(576);
		}
	}

	private void RB_FA_ON_Click(object sender, RoutedEventArgs e)
	{
		RB_FA_ON.IsEnabled = false;
		RB_FA_OFF.IsEnabled = false;
		if (RB_FA_ON.IsChecked == true)
		{
			Open_FA20_exe();
			Update_FA_IP();
		}
	}

	private void CB_HP_music_Click(object sender, RoutedEventArgs e)
	{
		Global.clevo_msg.SendToHKTray(270, 3);
	}

	private void CB_HP_game_Click(object sender, RoutedEventArgs e)
	{
		Global.clevo_msg.SendToHKTray(270, 4);
	}

	private void CB_HP_theater_Click(object sender, RoutedEventArgs e)
	{
		Global.clevo_msg.SendToHKTray(270, 5);
	}

	private void CB_HP_direct_Click(object sender, RoutedEventArgs e)
	{
		Global.clevo_msg.SendToHKTray(270, 2);
	}

	private void Update_HP_Status(int data)
	{
		switch (data)
		{
		case 2:
			CB_HP_direct.IsChecked = true;
			break;
		case 3:
			CB_HP_music.IsChecked = true;
			break;
		case 4:
			CB_HP_game.IsChecked = true;
			break;
		case 5:
			CB_HP_theater.IsChecked = true;
			break;
		}
	}

	private void GetESS(int data)
	{
		switch (data)
		{
		case 0:
			T_ESS_vaule.Text = "< 48";
			break;
		case 1:
			T_ESS_vaule.Text = "49-96";
			break;
		case 2:
			T_ESS_vaule.Text = "97-240";
			break;
		case 3:
			T_ESS_vaule.Text = "241-630";
			break;
		case 4:
			T_ESS_vaule.Text = "> 631";
			break;
		case 255:
			T_ESS_vaule.Text = "Unknown";
			break;
		}
	}

	private void SL_GPUSwitch_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (!Init)
		{
			if (SL_GPUSwitch.Value == 0.0)
			{
				RBTN_GPUSwitch_MSHybrid.IsChecked = true;
			}
			else
			{
				RBTN_GPUSwitch_Discrete.IsChecked = true;
			}
			UC_DialogBox.Show(1, 2);
		}
	}

	private void RBTN_GPUSwitch_MSHybrid_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		SL_GPUSwitch.Value = 0.0;
	}

	private void RBTN_GPUSwitch_Discrete_Checked(object sender, RoutedEventArgs e)
	{
		if (((ToggleButton)sender).IsChecked != true) return; // WPF Checked event
		SL_GPUSwitch.Value = 1.0;
	}

	private void UC_DialogBox_OKEvent()
	{
		Dispatcher.UIThread.Post((() =>
		{
			Console.WriteLine("get ok event");
			if (SL_GPUSwitch.Value == 1.0)
			{
				Global.clevo_msg.SendToHKTray(259, 1);
			}
			else
			{
				Global.clevo_msg.SendToHKTray(259, 0);
			}
			Thread.Sleep(1000);
			Console.WriteLine("shutdown.exe -f -r -t 0 (not executed on Linux)");
		}));
	}

	private void UC_DialogBox_CancelEvent()
	{
		SL_GPUSwitch.Value = Global.RW_REG.Get_GPUSwitchStatus();
		if (SL_GPUSwitch.Value == 0.0)
		{
			RBTN_GPUSwitch_MSHybrid.IsChecked = true;
		}
		else
		{
			RBTN_GPUSwitch_Discrete.IsChecked = true;
		}
		UC_DialogBox.IsVisible = false;
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
}
