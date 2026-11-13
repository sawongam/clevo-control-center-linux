using Avalonia.Controls;

namespace GPUOverclocking.UControl;

public partial class UserControl_CoreClock : UserControl
{
	public UserControl_CoreClock()
	{
		InitializeComponent();
	}

	public void core_clock(int level)
	{
		p_layer1.IsVisible = level == 1;
		p_layer2.IsVisible = level == 2;
		p_layer3.IsVisible = level == 3;
		p_layer4.IsVisible = level == 4;
		p_layer5.IsVisible = level == 5;
		p_layer6.IsVisible = level == 6;
		p_layer7.IsVisible = level == 7;
		p_layer8.IsVisible = level == 8;
		p_layer9.IsVisible = level == 9;
		p_layer10.IsVisible = level == 10;
		p_layer11.IsVisible = level == 11;
		p_layer12.IsVisible = level == 12;
	}
}
