using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
namespace CC;
public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
        {
            // dev/screenshot only: --page dialogN shows the (otherwise unused) UserControl_DialogBox
            var pg = RippleCC.Platform.AppHost.PageArg;
            if (pg != null && pg.StartsWith("dialog"))
            {
                CC.Global.RW_REG = new RWReg();
                CC.Global.MultiLang = new language();
                var dlg = new CC.UControl.UserControl_DialogBox();
                int msg = pg.Length > 6 && int.TryParse(pg.Substring(6), out var m) ? m : 0;
                dlg.Show(msg == 2 ? 1 : 0, msg);
                d.MainWindow = new Avalonia.Controls.Window { Width = 1920, Height = 1080, WindowDecorations = Avalonia.Controls.WindowDecorations.None, Background = Avalonia.Media.Brushes.Black, Content = dlg };
            }
            else if (pg == "loading") d.MainWindow = new loading();
            else d.MainWindow = new MainWindow();
        }
        base.OnFrameworkInitializationCompleted();
    }
}
