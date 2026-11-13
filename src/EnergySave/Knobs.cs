using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
namespace DesktopBridge_EnergySave;

public partial class MainWindow
{

	private void KnobMove1(PointerEventArgs e)
	{
		ProgressCircle progressCircle = knob.Circle;
		ProgressCircle progressCircle2 = knob.Circle2;
		if (!SaveModeList[SelectItem].isSchedule1_Enable)
		{
			return;
		}
		int num = 0;
		if (_isPressed)
		{
			Point position = e.GetPosition(knob.Inner);
			double angleR = MyHelper.GetAngleR(position, 150.0);
			double num3 = (int)((knob.Maximum - knob.Minimum) * angleR / (Math.PI * 2.0));
			num = 4 * (int)knob.Value + (int)knob.Value / 6;
			if (num3 < EndValue)
			{
				knob.Value = num3;
				StartValue = knob.Value;
				progressCircle2.Value = 4 * (int)knob.Value + (int)knob.Value / 6;
			}
		}
		if (_isPressed_2)
		{
			Point position2 = e.GetPosition(knob.Inner);
			double angleR2 = MyHelper.GetAngleR(position2, 150.0);
			double num4 = (int)((knob2.Maximum - knob2.Minimum) * angleR2 / (Math.PI * 2.0));
			if (num4 > StartValue)
			{
				knob2.Value = num4;
				EndValue = knob2.Value;
				progressCircle.Value = 4 * (int)knob2.Value + (int)knob2.Value / 6;
			}
			else if (num4 == 0.0 && progressCircle.Value == 95)
			{
				EndValue = knob2.Value + 0.1;
				knob2.Value = 0.0;
				progressCircle.Value = 100;
			}
		}
		SaveModeList[SelectItem].Schedule1_Start = (int)StartValue;
		SaveModeList[SelectItem].Schedule1_End = (int)EndValue;
		string text = "";
		text = text + StartValue + " - ";
		text = ((!(EndValue > 23.0)) ? (text + EndValue) : (text + "0"));
		Label_EnergySave_schedule1_time.Content = text;
				UpdateEnergySaveUI();
		if (CheckScheduleOverlapOrNot())
		{
			Image_EnergySave_save.IsEnabled = false;
			Bitmap source3 = Img.Load("Assets/btn_save_pressed.png");
			Image_EnergySave_save.Source = source3;
			Label_EnergySave_save.IsEnabled = false;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)159, (byte)159, (byte)159));
			Label_EnergySave_save.Foreground = foreground;
			Label_EnergySave_Warning.IsVisible = true;
		}
		else
		{
			Image_EnergySave_save.IsEnabled = true;
			Bitmap source4 = Img.Load("Assets/btn_save_normal.png");
			Image_EnergySave_save.Source = source4;
			Label_EnergySave_save.IsEnabled = true;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_save.Foreground = foreground2;
			Label_EnergySave_Warning.IsVisible = false;
		}
	}

	private void KnobMove2(PointerEventArgs e)
	{
		ProgressCircle progressCircle = knob3.Circle;
		ProgressCircle progressCircle2 = knob3.Circle2;
		if (!SaveModeList[SelectItem].isSchedule2_Enable)
		{
			return;
		}
		int num = 0;
		if (_isPressed_3)
		{
			Point position = e.GetPosition(knob3.Inner);
			double angleR = MyHelper.GetAngleR(position, 150.0);
			double num3 = (int)((knob3.Maximum - knob3.Minimum) * angleR / (Math.PI * 2.0));
			num = 4 * (int)knob3.Value + (int)knob3.Value / 6;
			if (num3 < EndValue_2)
			{
				knob3.Value = num3;
				StartValue_2 = knob3.Value;
				progressCircle2.Value = 4 * (int)knob3.Value + (int)knob3.Value / 6;
			}
		}
		if (_isPressed_4)
		{
			Point position2 = e.GetPosition(knob3.Inner);
			double angleR2 = MyHelper.GetAngleR(position2, 150.0);
			double num4 = (int)((knob4.Maximum - knob4.Minimum) * angleR2 / (Math.PI * 2.0));
			if (num4 > StartValue_2)
			{
				knob4.Value = num4;
				EndValue_2 = knob4.Value;
				progressCircle.Value = 4 * (int)knob4.Value + (int)knob4.Value / 6;
			}
			else if (num4 == 0.0 && progressCircle.Value == 95)
			{
				EndValue_2 = knob4.Value + 0.1;
				knob4.Value = 0.0;
				progressCircle.Value = 100;
			}
		}
		SaveModeList[SelectItem].Schedule2_Start = (int)StartValue_2;
		SaveModeList[SelectItem].Schedule2_End = (int)EndValue_2;
		string text = "";
		text = text + StartValue_2 + " - ";
		text = ((!(EndValue_2 > 23.0)) ? (text + EndValue_2) : (text + "0"));
		Label_EnergySave_schedule2_time.Content = text;
				UpdateEnergySaveUI();
		if (CheckScheduleOverlapOrNot())
		{
			Image_EnergySave_save.IsEnabled = false;
			Bitmap source3 = Img.Load("Assets/btn_save_pressed.png");
			Image_EnergySave_save.Source = source3;
			Label_EnergySave_save.IsEnabled = false;
			SolidColorBrush foreground = new SolidColorBrush(Color.FromArgb(byte.MaxValue, (byte)159, (byte)159, (byte)159));
			Label_EnergySave_save.Foreground = foreground;
			Label_EnergySave_Warning.IsVisible = true;
		}
		else
		{
			Image_EnergySave_save.IsEnabled = true;
			Bitmap source4 = Img.Load("Assets/btn_save_normal.png");
			Image_EnergySave_save.Source = source4;
			Label_EnergySave_save.IsEnabled = true;
			SolidColorBrush foreground2 = new SolidColorBrush(Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue));
			Label_EnergySave_save.Foreground = foreground2;
			Label_EnergySave_Warning.IsVisible = false;
		}
	}
}
