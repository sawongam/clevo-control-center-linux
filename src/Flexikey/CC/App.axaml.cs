using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CCenter.Platform;
namespace CC;

public partial class App : Application
{
	public static string StartUpPage;
	public override void Initialize() => AvaloniaXamlLoader.Load(this);
	public override void OnFrameworkInitializationCompleted()
	{
		// WPF App.OnStartup: first command line argument -> Properties["StartUpPage"]
		var a = System.Environment.GetCommandLineArgs();
		if (a.Length > 1 && !a[1].StartsWith("--")) StartUpPage = a[1];
		if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
			d.MainWindow = new MainWindow();
		base.OnFrameworkInitializationCompleted();
	}
}
