using System;
using Avalonia.Controls;
using Avalonia.Threading;
using RippleCC.Platform;

namespace CC;

/// <summary>Not in the original: --page NAME puts the main window in a given state for screenshots.</summary>
public static class ScreenshotPages
{
	public static void Apply(MainWindow w, string page)
	{
		if (string.IsNullOrEmpty(page) || page == "main" || page == "loading") return;
		if (page == "extra")
		{
			w.page_extra_setting.init();
			w.page_flexikey.Close_UI();
			w.page_extra_setting.Open_UI();
			return;
		}
		if (page == "extra-full")
		{
			var b = Global.support_bit; b.Switch_MSHybrid_DGPU = true; b._3DHP = true; b.ESS = true; b.OC_GPU = true;
			Reg.SetValue("HKEY_CURRENT_USER\\Software\\ControlCenter2.0\\UI\\", "Support_Page_SystemMonitor", "1");
			w.page_extra_setting.init();
			w.page_flexikey.Close_UI();
			w.page_extra_setting.Open_UI();
			return;
		}
		if (page == "dialog")
		{
			w.page_extra_setting.UC_DialogBox.Show(1, 2);
			w.page_flexikey.Close_UI();
			w.page_extra_setting.Open_UI();
			return;
		}
		w.page_flexikey.ShotState(page);
	}
}
