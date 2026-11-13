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
using RippleCC.Platform;
using CC.Compat;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;

namespace CC.UControl;

public partial class UserControl_DialogBox : UserControl
{
	public delegate void CancelHandler();

	public delegate void OKHandler();

	public event CancelHandler CancelEvent;

	public event OKHandler OKEvent;

	public UserControl_DialogBox()
	{
		InitializeComponent();
	}

	private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
	{
		CancelEvent();
	}

	private void BackGroundOkEvetn()
	{
		Thread.Sleep(50);
		OKEvent();
	}

	private void BTN_OK_Click(object sender, RoutedEventArgs e)
	{
		UC_DialogBox.IsVisible = false;
		new Thread(BackGroundOkEvetn).Start();
	}

	public void Show(int icon, int msg)
	{
		BTN_OK.Content = Global.MultiLang.Translation("_OK");
		BTN_Cancel.Content = Global.MultiLang.Translation("_Cancel");
		switch (icon)
		{
		case 0:
			Image_dialog_restart.IsVisible = false;
			Image_dialog_warning.IsVisible = true;
			break;
		case 1:
			Image_dialog_restart.IsVisible = true;
			Image_dialog_warning.IsVisible = false;
			break;
		default:
			Image_dialog_restart.IsVisible = false;
			Image_dialog_warning.IsVisible = true;
			break;
		}
		switch (msg)
		{
		case 0:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgCPUOC");
			break;
		case 1:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgGPUOC");
			break;
		case 2:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgRestart");
			break;
		case 3:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgS3");
			break;
		case 4:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgS4");
			break;
		case 5:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgSignOut");
			break;
		}
		UC_DialogBox.IsVisible = true;
	}

	private void UC_DialogBox_IsVisibleChanged(object sender, AvaloniaPropertyChangedEventArgs e)
	{
	}
}
