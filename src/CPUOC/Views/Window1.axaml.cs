using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace CPUOC30.View;

public partial class Window1 : Window
{
    public Window1() { InitializeComponent(); }
    public Window1(string message)
    {
        InitializeComponent();
        T_Dialog_info.Text = message;
        // original also wrote "Close_Loading" to the in-log (closes CPUOC_Loading splash): not applicable on Linux
    }
    void BTN_OK_Click(object sender, RoutedEventArgs e)
    {
        Hide();
        Environment.Exit(Environment.ExitCode);
    }
    void Apps_DialogBox_PointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed) BeginMoveDrag(e);
    }
}
