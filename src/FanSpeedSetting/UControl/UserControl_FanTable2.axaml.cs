using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Thickness = FanSpeedSetting.MutableThickness;

namespace FanSpeedSetting.UControl;

public partial class UserControl_FanTable2 : UserControl
{
	public delegate void FocusHandler();

	public struct FanArgs
	{
		public byte T1;

		public byte T2;

		public byte T3;

		public byte T4;

		public byte T2_Default;

		public byte T3_Default;

		public byte D1;

		public byte D2;

		public byte D3;

		public byte D4;

		public byte D2_Default;

		public byte D3_Default;

		public int R12;

		public int R23;

		public int R34;

		public int rpm;

		public int local;

		public int remote;

		public byte duty;
	}

	private bool T2_move = false;

	private bool T3_move = false;

	private int select_T = 0;

	private FanArgs Currentfan = default;


	public event FocusHandler FocusFanTableEvent;

	public UserControl_FanTable2()
	{
		InitializeComponent();
		R_layer.PointerMoved += R_layer_MouseMove;
		R_layer.PointerReleased += R_layer_MouseLeftButtonUp;
		// Port: Avalonia captures the pointer to the pressed point, so R_layer (WPF's hit layer) never sees the drag; route the point's own moves to the same handler.
		T2.PointerMoved += R_layer_MouseMove;
		T3.PointerMoved += R_layer_MouseMove;
		T2.PointerPressed += T2_MouseLeftButtonDown;
		T2.PointerEntered += T2_MouseEnter;
		T2.PointerReleased += T2_MouseLeftButtonUp;
		T3.PointerPressed += T3_MouseLeftButtonDown;
		T3.PointerEntered += T3_MouseEnter;
		T3.PointerReleased += T3_MouseLeftButtonUp;
		userControl.PropertyChanged += (o, e) => { if (e.Property == IsEnabledProperty) userControl_IsEnabledChanged(o, null); };
		Loaded += UserControl_Loaded;
	}

	/// <summary>Test hook: same code path as dragging point T (2 or 3) to (x,y) in R_layer coordinates.</summary>
	internal void TestDrag(int T, double x, double y)
	{
		if (T == 2) { T2_MouseLeftButtonDown(null, null); } else { T3_MouseLeftButtonDown(null, null); }
		MoveTo(x, y);
		R_layer_MouseLeftButtonUp(null, null);
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		LoadMultiLang();
	}

	public void LoadMultiLang()
	{
		Global.MultiLang.Load_language();
		T_fan_Copy.Text = Global.MultiLang.Translation("_Fan");
		textBlock_Copy18.Text = Global.MultiLang.Translation("_Fan");
		T_temp_Copy1.Text = Global.MultiLang.Translation("_Temperature");
		textBlock_Copy19.Text = Global.MultiLang.Translation("_Temperature") + "(°C)";
		ToolTip.SetTip(T1, T_fan_Copy.Text + " = " + Currentfan.D1 + "%\n" + T_temp_Copy1.Text + " = " + Currentfan.T1 + " °C");
		ToolTip.SetTip(T2, T_fan_Copy.Text + " = " + Currentfan.D2 + "%\n" + T_temp_Copy1.Text + " = " + Currentfan.T2 + " °C");
		ToolTip.SetTip(T3, T_fan_Copy.Text + " = " + Currentfan.D3 + "%\n" + T_temp_Copy1.Text + " = " + Currentfan.T3 + " °C");
		ToolTip.SetTip(T4, T_fan_Copy.Text + " = " + Currentfan.D4 + "%\n" + T_temp_Copy1.Text + " = " + Currentfan.T4 + " °C");
	}

	public void Init(Global.FanArgs FanTable)
	{
		string text = string.Empty;
		int num = FanTable.D1;
		int num2 = FanTable.T1;
		int num3 = FanTable.D2;
		int num4 = FanTable.T2;
		int num5 = FanTable.D3;
		int num6 = FanTable.T3;
		int num7 = FanTable.D4;
		int num8 = FanTable.T4;
		if (0 >= num || num >= num3 || num3 >= num5 || num5 >= num7 || num7 > 100)
		{
			text += $"The Fan{FanTable} duty value ERROR!\n";
		}
		if (0 >= num2 || num2 >= num4 || num4 >= num6 || num6 >= num8 || num8 > 100)
		{
			text += $"The Fan{FanTable} temp value ERROR!";
		}
		if (text != string.Empty)
		{
			text += "\n";
			text += "Control Center will set the legal value!";
			num = 50;
			num3 = 60;
			num5 = 70;
			num7 = 100;
			num2 = 40;
			num4 = 60;
			num6 = 80;
			num8 = 100;
		}
		Update_T1_UI(num, num2);
		Update_T2_UI(num3, num4);
		Update_T3_UI(num5, num6);
		Update_T4_UI(num7, num8);
	}

	private void R_layer_MouseMove(object sender, PointerEventArgs e)
	{
		if (!e.GetCurrentPoint(R_layer).Properties.IsLeftButtonPressed)
		{
			T2_move = false;
			T3_move = false;
			return;
		}
		Point position = e.GetPosition(R_layer);
		MoveTo(position.X, position.Y);
	}

	private void MoveTo(double px, double py)
	{
		double num = Math.Floor(px);
		double num2 = Math.Floor(py);
		Thickness val = new Thickness(num, num2, 0, 0);
		Thickness margin;
		if (T2_move)
		{
			margin = (T3_point).Margin;
			if (num >= margin.Left - 7.0)
			{
				margin = (T3_point).Margin;
				val.Left = margin.Left - 7.0;
			}
			else
			{
				margin = (T1_point).Margin;
				if (num <= margin.Left)
				{
					margin = (T1_point).Margin;
					val.Left = margin.Left + 7.0;
				}
				else
				{
					val.Left = num;
				}
			}
			margin = (T3_point).Margin;
			if (num2 >= margin.Top - 3.0)
			{
				margin = (T3_point).Margin;
				val.Top = margin.Top - 3.0;
			}
			else
			{
				margin = (T1_point).Margin;
				if (num2 <= margin.Top)
				{
					margin = (T1_point).Margin;
					val.Top = margin.Top + 3.0;
				}
				else
				{
					val.Top = num2;
				}
			}
			T_temp.Text = (Math.Ceiling(val.Left / 7.0) + 35.0).ToString();
			T_fan.Text = Math.Ceiling(val.Top / 3.0).ToString();
			Update_T2_UI(Convert.ToInt16(T_fan.Text), Convert.ToInt16(T_temp.Text));
		}
		if (!T3_move)
		{
			return;
		}
		margin = (T4_point).Margin;
		if (num >= margin.Left - 7.0)
		{
			margin = (T4_point).Margin;
			val.Left = margin.Left - 7.0;
		}
		else
		{
			margin = (T2_point).Margin;
			if (num <= margin.Left)
			{
				margin = (T2_point).Margin;
				val.Left = margin.Left + 7.0;
			}
			else
			{
				val.Left = num;
			}
		}
		margin = (T4_point).Margin;
		if (num2 >= margin.Top - 3.0)
		{
			margin = (T4_point).Margin;
			val.Top = margin.Top - 6.0;
		}
		else
		{
			margin = (T2_point).Margin;
			if (num2 <= margin.Top)
			{
				margin = (T2_point).Margin;
				val.Top = margin.Top + 3.0;
			}
			else
			{
				val.Top = num2;
			}
		}
		T_temp.Text = (Math.Ceiling(val.Left / 7.0) + 35.0).ToString();
		T_fan.Text = Math.Ceiling(val.Top / 3.0).ToString();
		Update_T3_UI(Convert.ToInt16(T_fan.Text), Convert.ToInt16(T_temp.Text));
	}

	private void Update_T1_UI(int Fan, int temp)
	{
		Thickness margin = default;
		Currentfan.D1 = (byte)Fan;
		Currentfan.T1 = (byte)temp;
		ToolTip.SetTip(T1, T_fan_Copy.Text + " = " + Fan + "%\n" + T_temp_Copy1.Text + " = " + temp + " °C");
		margin.Left = (temp - 35) * 7;
		margin.Top = Fan * 3;
		margin.Bottom = -20.0;
		margin.Right = -20.0;
		(T1_point).Margin = margin;
		Thickness margin2 = (T2_point).Margin;
		double left = margin2.Left;
		margin2 = (T1_point).Margin;
		double width = left - margin2.Left;
		margin2 = (T2_point).Margin;
		double top = margin2.Top;
		margin2 = (T1_point).Margin;
		double height = top - margin2.Top;
		(R12).Width = width;
		(R12).Height = height;
	}

	private void Update_T2_UI(int Fan, int temp)
	{
		Thickness margin = default;
		Currentfan.D2 = (byte)Fan;
		Currentfan.T2 = (byte)temp;
		ToolTip.SetTip(T2, T_fan_Copy.Text + " = " + Fan + "%\n" + T_temp_Copy1.Text + " = " + temp + " °C");
		margin.Left = (temp - 35) * 7;
		margin.Top = Fan * 3;
		margin.Bottom = -20.0;
		margin.Right = -20.0;
		(T2_point).Margin = margin;
		Thickness margin2 = (T2_point).Margin;
		double left = margin2.Left;
		margin2 = (T1_point).Margin;
		double width = left - margin2.Left;
		margin2 = (T2_point).Margin;
		double top = margin2.Top;
		margin2 = (T1_point).Margin;
		double height = top - margin2.Top;
		(R12).Width = width;
		(R12).Height = height;
		margin2 = (T3_point).Margin;
		double left2 = margin2.Left;
		margin2 = (T2_point).Margin;
		width = left2 - margin2.Left;
		margin2 = (T3_point).Margin;
		double top2 = margin2.Top;
		margin2 = (T2_point).Margin;
		height = top2 - margin2.Top;
		(R23).Width = width;
		(R23).Height = height;
	}

	private void Update_T3_UI(int Fan, int temp)
	{
		Thickness margin = default;
		Currentfan.D3 = (byte)Fan;
		Currentfan.T3 = (byte)temp;
		ToolTip.SetTip(T3, T_fan_Copy.Text + " = " + Fan + "%\n" + T_temp_Copy1.Text + " = " + temp + " °C");
		margin.Left = (temp - 35) * 7;
		margin.Top = Fan * 3;
		margin.Bottom = -20.0;
		margin.Right = -20.0;
		(T3_point).Margin = margin;
		Thickness margin2 = (T3_point).Margin;
		double left = margin2.Left;
		margin2 = (T2_point).Margin;
		double width = left - margin2.Left;
		margin2 = (T3_point).Margin;
		double top = margin2.Top;
		margin2 = (T2_point).Margin;
		double height = top - margin2.Top;
		(R23).Width = width;
		(R23).Height = height;
		margin2 = (T4_point).Margin;
		double left2 = margin2.Left;
		margin2 = (T3_point).Margin;
		width = left2 - margin2.Left;
		margin2 = (T4_point).Margin;
		double top2 = margin2.Top;
		margin2 = (T3_point).Margin;
		height = top2 - margin2.Top;
		(R34).Width = width;
		(R34).Height = height;
	}

	private void Update_T4_UI(int Fan, int temp)
	{
		Thickness margin = default;
		Currentfan.D4 = (byte)Fan;
		Currentfan.T4 = (byte)temp;
		ToolTip.SetTip(T4, T_fan_Copy.Text + " = " + Fan + "%\n" + T_temp_Copy1.Text + " = " + temp + " °C");
		margin.Left = (temp - 35) * 7;
		margin.Top = Fan * 3;
		margin.Bottom = -20.0;
		margin.Right = -20.0;
		(T4_point).Margin = margin;
		Thickness margin2 = (T4_point).Margin;
		double left = margin2.Left;
		margin2 = (T3_point).Margin;
		double width = left - margin2.Left;
		margin2 = (T4_point).Margin;
		double top = margin2.Top;
		margin2 = (T3_point).Margin;
		double height = top - margin2.Top;
		(R34).Width = width;
		(R34).Height = height;
	}

	private void Update_Default_UI(int Duty2, int Duty3, int Temp2, int Temp3)
	{
		Thickness margin = default;
		Currentfan.D2 = (byte)Duty2;
		Currentfan.T2 = (byte)Temp2;
		ToolTip.SetTip(T2, T_fan_Copy.Text + " = " + Duty2 + "%\n" + T_temp_Copy1.Text + " = " + Temp2 + " °C");
		margin.Left = (Temp2 - 35) * 7;
		margin.Top = Duty2 * 3;
		margin.Bottom = -20.0;
		margin.Right = -20.0;
		(T2_point).Margin = margin;
		Currentfan.D3 = (byte)Duty3;
		Currentfan.T3 = (byte)Temp3;
		ToolTip.SetTip(T3, T_fan_Copy.Text + " = " + Duty3 + "%\n" + T_temp_Copy1.Text + " = " + Temp3 + " °C");
		margin.Left = (Temp3 - 35) * 7;
		margin.Top = Duty3 * 3;
		margin.Bottom = -20.0;
		margin.Right = -20.0;
		(T3_point).Margin = margin;
		Thickness margin2 = (T2_point).Margin;
		double left = margin2.Left;
		margin2 = (T1_point).Margin;
		double width = left - margin2.Left;
		margin2 = (T2_point).Margin;
		double top = margin2.Top;
		margin2 = (T1_point).Margin;
		double height = top - margin2.Top;
		(R12).Width = width;
		(R12).Height = height;
		margin2 = (T3_point).Margin;
		double left2 = margin2.Left;
		margin2 = (T2_point).Margin;
		width = left2 - margin2.Left;
		margin2 = (T3_point).Margin;
		double top2 = margin2.Top;
		margin2 = (T2_point).Margin;
		height = top2 - margin2.Top;
		(R23).Width = width;
		(R23).Height = height;
		margin2 = (T4_point).Margin;
		double left3 = margin2.Left;
		margin2 = (T3_point).Margin;
		width = left3 - margin2.Left;
		margin2 = (T4_point).Margin;
		double top3 = margin2.Top;
		margin2 = (T3_point).Margin;
		height = top3 - margin2.Top;
		(R34).Width = width;
		(R34).Height = height;
	}

	private void T2_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		FocusFanTableEvent();
		T2_move = true;
		T3_move = false;
		change_select_T(2);
		(userControl).Tag = "1";
	}

	private void T3_MouseLeftButtonDown(object sender, PointerPressedEventArgs e)
	{
		FocusFanTableEvent();
		T2_move = false;
		T3_move = true;
		change_select_T(3);
		(userControl).Tag = "1";
	}

	public void change_select_T(int T)
	{
		select_T = T;
		Color val = Color.FromArgb(byte.MaxValue, (byte)160, (byte)160, (byte)160);
		Brush stroke = (Brush)new SolidColorBrush(val);
		(T2).Stroke = stroke;
		(T3).Stroke = stroke;
		(T2).StrokeThickness = 2.0;
		(T3).StrokeThickness = 2.0;
		T_temp.Text = "";
		T_fan.Text = "";
		val = Color.FromArgb(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
		stroke = (Brush)new SolidColorBrush(val);
		Thickness margin;
		if (T == 2)
		{
			(T2).Stroke = stroke;
			(T2).StrokeThickness = 4.0;
			margin = (T2_point).Margin;
			double num = margin.Left / 7.0 + 35.0;
			T_temp.Text = num.ToString();
			margin = (T2_point).Margin;
			num = margin.Top / 3.0;
			T_fan.Text = num.ToString();
		}
		if (T == 3)
		{
			(T3).Stroke = stroke;
			(T3).StrokeThickness = 4.0;
			margin = (T3_point).Margin;
			double num2 = margin.Left / 7.0 + 35.0;
			T_temp.Text = num2.ToString();
			margin = (T3_point).Margin;
			num2 = margin.Top / 3.0;
			T_fan.Text = num2.ToString();
		}
	}

	private void userControl_IsEnabledChanged(object sender, EventArgs e)
	{
		if ((userControl).IsEnabled)
		{
			(R12).StrokeThickness = 6.0;
			(R23).StrokeThickness = 6.0;
			(R34).StrokeThickness = 6.0;
		}
		else
		{
			(R12).StrokeThickness = 3.0;
			(R23).StrokeThickness = 3.0;
			(R34).StrokeThickness = 3.0;
		}
	}

	public void Save_FanTable(int FanTableIndex)
	{
		(userControl).Tag = "0";
		Thickness margin = (T2_point).Margin;
		double num = margin.Top / 3.0;
		margin = (T3_point).Margin;
		double num2 = margin.Top / 3.0;
		margin = (T2_point).Margin;
		double num3 = margin.Left / 7.0 + 35.0;
		margin = (T3_point).Margin;
		double num4 = margin.Left / 7.0 + 35.0;
		switch (FanTableIndex)
		{
		case 1:
			Global.fan.Fan_CPU.D2 = (byte)num;
			Global.fan.Fan_CPU.D3 = (byte)num2;
			Global.fan.Fan_CPU.T2 = (byte)num3;
			Global.fan.Fan_CPU.T3 = (byte)num4;
			break;
		case 2:
			Global.fan.Fan_GPU1.D2 = (byte)num;
			Global.fan.Fan_GPU1.D3 = (byte)num2;
			Global.fan.Fan_GPU1.T2 = (byte)num3;
			Global.fan.Fan_GPU1.T3 = (byte)num4;
			break;
		case 3:
			Global.fan.Fan_GPU2.D2 = (byte)num;
			Global.fan.Fan_GPU2.D3 = (byte)num2;
			Global.fan.Fan_GPU2.T2 = (byte)num3;
			Global.fan.Fan_GPU2.T3 = (byte)num4;
			break;
		}
	}

	public void Set_defult_FanTable(int FanTableIndex)
	{
		(userControl).Tag = "0";
		switch (FanTableIndex)
		{
		case 1:
			Global.fan.Fan_CPU.D2 = Global.fan.Fan_CPU.D2_Default;
			Global.fan.Fan_CPU.D3 = Global.fan.Fan_CPU.D3_Default;
			Global.fan.Fan_CPU.T2 = Global.fan.Fan_CPU.T2_Default;
			Global.fan.Fan_CPU.T3 = Global.fan.Fan_CPU.T3_Default;
			Update_Default_UI(Global.fan.Fan_CPU.D2, Global.fan.Fan_CPU.D3, Global.fan.Fan_CPU.T2, Global.fan.Fan_CPU.T3);
			break;
		case 2:
			Global.fan.Fan_GPU1.D2 = Global.fan.Fan_GPU1.D2_Default;
			Global.fan.Fan_GPU1.D3 = Global.fan.Fan_GPU1.D3_Default;
			Global.fan.Fan_GPU1.T2 = Global.fan.Fan_GPU1.T2_Default;
			Global.fan.Fan_GPU1.T3 = Global.fan.Fan_GPU1.T3_Default;
			Update_Default_UI(Global.fan.Fan_GPU1.D2, Global.fan.Fan_GPU1.D3, Global.fan.Fan_GPU1.T2, Global.fan.Fan_GPU1.T3);
			break;
		case 3:
			Global.fan.Fan_GPU2.D2 = Global.fan.Fan_GPU2.D2_Default;
			Global.fan.Fan_GPU2.D3 = Global.fan.Fan_GPU2.D3_Default;
			Global.fan.Fan_GPU2.T2 = Global.fan.Fan_GPU2.T2_Default;
			Global.fan.Fan_GPU2.T3 = Global.fan.Fan_GPU2.T3_Default;
			Update_Default_UI(Global.fan.Fan_GPU2.D2, Global.fan.Fan_GPU2.D3, Global.fan.Fan_GPU2.T2, Global.fan.Fan_GPU2.T3);
			break;
		}
	}

	private void T2_MouseEnter(object sender, PointerEventArgs e)
	{
		R_layer.IsVisible = true;
	}

	private void T2_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		R_layer.IsVisible = false;
	}

	private void R_layer_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		R_layer.IsVisible = false;
	}

	private void T3_MouseEnter(object sender, PointerEventArgs e)
	{
		R_layer.IsVisible = true;
	}

	private void T3_MouseLeftButtonUp(object sender, PointerReleasedEventArgs e)
	{
		R_layer.IsVisible = false;
	}
}
