using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
namespace CC.UControl;
public partial class UserControl_DialogBox : UserControl
{
	public delegate void CancelHandler();
	public delegate void OKHandler();
	public event CancelHandler CancelEvent;
	public event OKHandler OKEvent;
	public UserControl_DialogBox() { InitializeComponent(); }
	private void BTN_Cancel_Click(object sender, RoutedEventArgs e) { CancelEvent?.Invoke(); }
	private void BackGroundOkEvetn() { Thread.Sleep(50); OKEvent?.Invoke(); }
	private void BTN_OK_Click(object sender, RoutedEventArgs e)
	{
		IsVisible = false;
		var thread = new Thread(BackGroundOkEvetn);
		thread.Start();
	}
	public void Show(int icon, int msg)
	{
		BTN_OK.Content = Global.MultiLang.Translation("_OK");
		BTN_Cancel.Content = Global.MultiLang.Translation("_Cancel");
		switch (icon)
		{
		case 1: Image_dialog_restart.IsVisible = true; Image_dialog_warning.IsVisible = false; break;
		default: Image_dialog_restart.IsVisible = false; Image_dialog_warning.IsVisible = true; break;
		}
		switch (msg)
		{
		case 0: T_Dialog_info.Text = Global.MultiLang.Translation("_MsgCPUOC"); break;
		case 1: T_Dialog_info.Text = Global.MultiLang.Translation("_MsgGPUOC"); break;
		case 2: T_Dialog_info.Text = Global.MultiLang.Translation("_MsgRestart"); break;
		case 3: T_Dialog_info.Text = Global.MultiLang.Translation("_MsgS3"); break;
		case 4: T_Dialog_info.Text = Global.MultiLang.Translation("_MsgS4"); break;
		case 5: T_Dialog_info.Text = Global.MultiLang.Translation("_MsgSignOut"); break;
		case 6:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgACExit");
			BTN_Cancel.IsVisible = false;
			BTN_OK.Content = Global.MultiLang.Translation("_Yes");
			break;
		case 7:
			T_Dialog_info.Text = Global.MultiLang.Translation("_BCancel");
			BTN_OK.Content = Global.MultiLang.Translation("_Yes");
			BTN_Cancel.Content = Global.MultiLang.Translation("_No");
			break;
		case 8:
			T_Dialog_info.Text = Global.MultiLang.Translation("_MsgACKeep");
			BTN_Cancel.IsVisible = false;
			BTN_OK.Content = Global.MultiLang.Translation("_Yes");
			break;
		case 9:
			T_Dialog_info.Text = Global.MultiLang.Translation("_BFinishExit");
			BTN_OK.Content = Global.MultiLang.Translation("_Yes");
			BTN_Cancel.Content = Global.MultiLang.Translation("_No");
			break;
		case 10:
			T_Dialog_info.Text = "This machine does not support.";
			BTN_Cancel.IsVisible = false;
			BTN_OK.IsVisible = false;
			break;
		}
		IsVisible = true;
	}
}
