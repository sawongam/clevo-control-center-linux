using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace CPUOC30.View;

public partial class index
{
	private void SL_ratio1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio1.Value < SL_ratio2.Value) SL_ratio2.Value = SL_ratio1.Value;
		comboBox_CPURatioMinc1.SelectedIndex = (int)SL_ratio1.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio1.Value = comboBox_CPURatioMinc1.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio2.Value > SL_ratio1.Value) SL_ratio1.Value = SL_ratio2.Value;
		if (SL_ratio2.Value < SL_ratio3.Value) SL_ratio3.Value = SL_ratio2.Value;
		comboBox_CPURatioMinc2.SelectedIndex = (int)SL_ratio2.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc2_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio2.Value = comboBox_CPURatioMinc2.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio3.Value > SL_ratio2.Value) SL_ratio2.Value = SL_ratio3.Value;
		if (SL_ratio3.Value < SL_ratio4.Value) SL_ratio4.Value = SL_ratio3.Value;
		comboBox_CPURatioMinc3.SelectedIndex = (int)SL_ratio3.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc3_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio3.Value = comboBox_CPURatioMinc3.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio4_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio4.Value > SL_ratio3.Value) SL_ratio3.Value = SL_ratio4.Value;
		if (SL_ratio4.Value < SL_ratio5.Value) SL_ratio5.Value = SL_ratio4.Value;
		comboBox_CPURatioMinc4.SelectedIndex = (int)SL_ratio4.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc4_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio4.Value = comboBox_CPURatioMinc4.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio5_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio5.Value > SL_ratio4.Value) SL_ratio4.Value = SL_ratio5.Value;
		if (SL_ratio5.Value < SL_ratio6.Value) SL_ratio6.Value = SL_ratio5.Value;
		comboBox_CPURatioMinc5.SelectedIndex = (int)SL_ratio5.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc5_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio5.Value = comboBox_CPURatioMinc5.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio6_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio6.Value > SL_ratio5.Value) SL_ratio5.Value = SL_ratio6.Value;
		if (SL_ratio6.Value < SL_ratio7.Value) SL_ratio7.Value = SL_ratio6.Value;
		comboBox_CPURatioMinc6.SelectedIndex = (int)SL_ratio6.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc6_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio6.Value = comboBox_CPURatioMinc6.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio7_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio7.Value > SL_ratio6.Value) SL_ratio6.Value = SL_ratio7.Value;
		if (SL_ratio7.Value < SL_ratio8.Value) SL_ratio8.Value = SL_ratio7.Value;
		comboBox_CPURatioMinc7.SelectedIndex = (int)SL_ratio7.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc7_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio7.Value = comboBox_CPURatioMinc7.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio8_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio8.Value > SL_ratio7.Value) SL_ratio7.Value = SL_ratio8.Value;
		if (SL_ratio8.Value < SL_ratio9.Value) SL_ratio9.Value = SL_ratio8.Value;
		comboBox_CPURatioMinc8.SelectedIndex = (int)SL_ratio8.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc8_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio8.Value = comboBox_CPURatioMinc8.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio9_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio9.Value > SL_ratio8.Value) SL_ratio8.Value = SL_ratio9.Value;
		if (SL_ratio9.Value < SL_ratio10.Value) SL_ratio10.Value = SL_ratio9.Value;
		comboBox_CPURatioMinc9.SelectedIndex = (int)SL_ratio9.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc9_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio9.Value = comboBox_CPURatioMinc9.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratio10_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratio10.Value > SL_ratio9.Value) SL_ratio9.Value = SL_ratio10.Value;
		comboBox_CPURatioMinc10.SelectedIndex = (int)SL_ratio10.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMinc10_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratio10.Value = comboBox_CPURatioMinc10.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE1_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE1.Value < SL_ratioE2.Value) SL_ratioE2.Value = SL_ratioE1.Value;
		comboBox_CPURatioMincE1.SelectedIndex = (int)SL_ratioE1.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE1.Value = comboBox_CPURatioMincE1.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE2_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE2.Value > SL_ratioE1.Value) SL_ratioE1.Value = SL_ratioE2.Value;
		if (SL_ratioE2.Value < SL_ratioE3.Value) SL_ratioE3.Value = SL_ratioE2.Value;
		comboBox_CPURatioMincE2.SelectedIndex = (int)SL_ratioE2.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE2_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE2.Value = comboBox_CPURatioMincE2.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE3_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE3.Value > SL_ratioE2.Value) SL_ratioE2.Value = SL_ratioE3.Value;
		if (SL_ratioE3.Value < SL_ratioE4.Value) SL_ratioE4.Value = SL_ratioE3.Value;
		comboBox_CPURatioMincE3.SelectedIndex = (int)SL_ratioE3.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE3_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE3.Value = comboBox_CPURatioMincE3.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE4_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE4.Value > SL_ratioE3.Value) SL_ratioE3.Value = SL_ratioE4.Value;
		if (SL_ratioE4.Value < SL_ratioE5.Value) SL_ratioE5.Value = SL_ratioE4.Value;
		comboBox_CPURatioMincE4.SelectedIndex = (int)SL_ratioE4.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE4_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE4.Value = comboBox_CPURatioMincE4.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE5_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE5.Value > SL_ratioE4.Value) SL_ratioE4.Value = SL_ratioE5.Value;
		if (SL_ratioE5.Value < SL_ratioE6.Value) SL_ratioE6.Value = SL_ratioE5.Value;
		comboBox_CPURatioMincE5.SelectedIndex = (int)SL_ratioE5.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE5_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE5.Value = comboBox_CPURatioMincE5.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE6_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE6.Value > SL_ratioE5.Value) SL_ratioE5.Value = SL_ratioE6.Value;
		if (SL_ratioE6.Value < SL_ratioE7.Value) SL_ratioE7.Value = SL_ratioE6.Value;
		comboBox_CPURatioMincE6.SelectedIndex = (int)SL_ratioE6.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE6_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE6.Value = comboBox_CPURatioMincE6.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE7_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE7.Value > SL_ratioE6.Value) SL_ratioE6.Value = SL_ratioE7.Value;
		if (SL_ratioE7.Value < SL_ratioE8.Value) SL_ratioE8.Value = SL_ratioE7.Value;
		comboBox_CPURatioMincE7.SelectedIndex = (int)SL_ratioE7.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE7_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE7.Value = comboBox_CPURatioMincE7.SelectedIndex;
		CheckSaveButtonStatus();
	}
	private void SL_ratioE8_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
	{
		if (SL_ratioE8.Value > SL_ratioE7.Value) SL_ratioE7.Value = SL_ratioE8.Value;
		comboBox_CPURatioMincE8.SelectedIndex = (int)SL_ratioE8.Value;
		CheckSaveButtonStatus();
	}
	private void comboBox_CPURatioMincE8_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		SL_ratioE8.Value = comboBox_CPURatioMincE8.SelectedIndex;
		CheckSaveButtonStatus();
	}
}
