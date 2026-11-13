using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
namespace CC;
public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);
    public override void OnFrameworkInitializationCompleted()
    {
        // Original App.OnStartup: Properties["StartUpPage"] = e.Args[0] or 0 (never read by this app).
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d) d.MainWindow = new MainWindow();
        base.OnFrameworkInitializationCompleted();
    }
}
