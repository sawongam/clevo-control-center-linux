using System;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;

namespace ControlCenter30;

public partial class UserControl_DialogBox : UserControl
{
	public delegate void CancelHandler();

	public delegate void OKHandler();

	private language lang = language.Instance;

	public event CancelHandler CancelEvent;

	public event OKHandler OKEvent;

	public UserControl_DialogBox()
	{
		InitializeComponent();
		BTN_Cancel.Click += BTN_Cancel_Click;
		BTN_OK.Click += BTN_OK_Click;
		// XAML: Visibility="Hidden" on the instance in Window1
	}

	private void BTN_Cancel_Click(object sender, RoutedEventArgs e)
	{
		CancelEvent?.Invoke();
	}

	private void BackGroundOkEvetn()
	{
		Thread.Sleep(50);
		OKEvent?.Invoke();
	}

	private void BTN_OK_Click(object sender, RoutedEventArgs e)
	{
		IsVisible = false;
		Thread thread = new Thread(BackGroundOkEvetn);
		thread.Start();
	}

	public void Show(int icon, int msg)
	{
		BTN_OK.Content = lang.TranslationLanguage("_OK");
		BTN_Cancel.Content = lang.TranslationLanguage("_Cancel");
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
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgCPUOC");
			break;
		case 1:
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgGPUOC");
			break;
		case 2:
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgRestart");
			break;
		case 3:
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgS3");
			break;
		case 4:
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgS4");
			break;
		case 5:
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgSignOut");
			break;
		case 6:
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgACExit");
			BTN_Cancel.IsVisible = false;
			BTN_OK.Content = lang.TranslationLanguage("_Yes");
			break;
		case 7:
			T_Dialog_info.Text = lang.TranslationLanguage("_BCancel");
			BTN_OK.Content = lang.TranslationLanguage("_Yes");
			BTN_Cancel.Content = lang.TranslationLanguage("_No");
			break;
		case 8:
			T_Dialog_info.Text = lang.TranslationLanguage("_MsgACKeep");
			BTN_Cancel.IsVisible = false;
			BTN_OK.Content = lang.TranslationLanguage("_Yes");
			break;
		case 9:
			T_Dialog_info.Text = lang.TranslationLanguage("_BFinishExit");
			BTN_OK.Content = lang.TranslationLanguage("_Yes");
			BTN_Cancel.Content = lang.TranslationLanguage("_No");
			break;
		case 10:
			T_Dialog_info.Text = "This machine does not support.";
			BTN_Cancel.IsVisible = false;
			BTN_OK.IsVisible = false;
			break;
		case 11:
			T_Dialog_info.Text = "NVIDIA Device not ready.";
			BTN_Cancel.IsVisible = false;
			BTN_OK.IsVisible = false;
			break;
		case 12:
			T_Dialog_info_Copy.Text = lang.TranslationLanguage("_Are");
			T_Dialog_info.Text = lang.TranslationLanguage("_AdjustingCPUvoltage");
			BTN_Cancel.IsVisible = true;
			BTN_OK.IsVisible = true;
			break;
		case 13:
			T_Dialog_info_Copy.Text = "";
			T_Dialog_info.Text = lang.TranslationLanguage("_LowRefreshRate_INFO") + "\n" + lang.TranslationLanguage("_LowRefreshRate_ON_INFO");
			BTN_Cancel.IsVisible = true;
			BTN_OK.IsVisible = true;
			break;
		}
		this.IsVisible = true;
	}
}
