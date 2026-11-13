using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
namespace CC;

/// <summary>Small modal window that looks like the Windows MessageBox (original: MessageBox.Show).</summary>
public static class MessageBoxWin
{
    public static void Show(string text, string caption = "LED Keyboard Setting")
    {
        var w = new Window { Title = caption, Width = 320, Height = 140, CanResize = false, WindowStartupLocation = WindowStartupLocation.CenterScreen, Background = Brushes.White, FontSize = 12 };
        var ok = new Button { Content = "OK", Width = 75, Height = 23, HorizontalContentAlignment = HorizontalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 0, 12, 10), VerticalAlignment = VerticalAlignment.Bottom };
        ok.Click += (_, _) => w.Close();
        var g = new Grid();
        g.Children.Add(new TextBlock { Text = text, Margin = new Thickness(20, 20, 20, 50), TextWrapping = TextWrapping.Wrap });
        g.Children.Add(new Border { Background = new SolidColorBrush(Color.FromRgb(240, 240, 240)), Height = 43, VerticalAlignment = VerticalAlignment.Bottom, Child = null });
        g.Children.Add(ok);
        w.Content = g;
        w.Show();
    }
}
