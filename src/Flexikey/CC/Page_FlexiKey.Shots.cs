using System;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Test1;

namespace CC;

public partial class Page_FlexiKey
{
	/// <summary>Not in the original: drives the page into a state for --page screenshots.</summary>
	public void ShotState(string state)
	{
		void KeyDown(int code, int status, int delay) => MyKB_Hook_KeyPress(null, new KeyboardEventArgs(code, status, delay));
		void SelectKey(CheckBox cb) { cb.IsChecked = true; CB_MacroKB_roup_Click(cb, null); }
		void Action(CheckBox cb) { cb.IsChecked = true; CB_Action_group_Click(cb, null); }
		switch (state)
		{
			case "enabled":
				SL_FlexiKey_KB_status.Value = 1;
				break;
			case "mouse":
				RBTN_MacroMouse.IsChecked = true;
				break;
			case "mouse-enabled":
				SL_FlexiKey_Mouse_status.Value = 1; RBTN_MacroMouse.IsChecked = true;
				break;
			case "mouse-key":
				SL_FlexiKey_Mouse_status.Value = 1; RBTN_MacroMouse.IsChecked = true;
				CB_MouseMouse_Left.IsChecked = true; CB_MacroMouse_roup_Click(CB_MouseMouse_Left, null);
				break;
			case "stats":
				RBTN_statistics.IsChecked = true;
				break;
			case "stats-key":
				RBTN_statistics.IsChecked = true;
				break;
			case "key":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30);
				break;
			case "express":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30); Action(CB_Express);
				break;
			case "express-rec":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30); Action(CB_Express);
				CB_ExpressKey_Start.IsChecked = true; CB_ExpressKey_Start_Click(CB_ExpressKey_Start, null);
				KeyDown(30, 0, 0); KeyDown(30, 1, 80); KeyDown(48, 0, 120); KeyDown(48, 1, 60); KeyDown(46, 0, 90); KeyDown(46, 1, 70);
				break;
			case "express-done":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30); Action(CB_Express);
				CB_ExpressKey_Start.IsChecked = true; CB_ExpressKey_Start_Click(CB_ExpressKey_Start, null);
				KeyDown(30, 0, 0); KeyDown(30, 1, 80); KeyDown(48, 0, 120); KeyDown(48, 1, 60);
				CB_ExpressKey_Start.IsChecked = false; CB_ExpressKey_Start_Click(CB_ExpressKey_Start, null);
				break;
			case "text":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30); Action(CB_ExpressText);
				break;
			case "text-done":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30); Action(CB_ExpressText);
				CB_ExpressText_Start.IsChecked = true; CB_ExpressText_Start_Click(CB_ExpressText_Start, null);
				KeyDown(29, 0, 0); KeyDown(48, 0, 0); KeyDown(48, 1, 0); KeyDown(29, 1, 0);
				TB_ExpressText_content.Text = "Hello Flexikey";
				break;
			case "launch":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30); Action(CB_Launch);
				break;
			case "disable":
				SL_FlexiKey_KB_status.Value = 1; SelectKey(CB_keyCode_30); Action(CB_DisableKey);
				break;
			case "profile":
				grid_page_key_mouse.IsEnabled = false; FlexiKey_PageBTN.IsEnabled = false;
				TB_new_profile_name.Text = _profileInfo[0].ProfileName; LoadImage(_profileInfo[0].ProfileIconPath);
				grid_profile_custom.IsVisible = true;
				break;
		}
		Console.Error.WriteLine($"shot {state}: kb={SL_FlexiKey_KB_status.Value} mouse={SL_FlexiKey_Mouse_status.Value} rbMouseEnable={RBTN_MouseStatus_Enable.IsChecked}");
	}
}
