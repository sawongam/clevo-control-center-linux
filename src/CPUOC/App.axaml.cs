using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using CPUOC30.Model;
using CPUOC30.View;
using CCenter.Platform;

namespace CPUOC30;

public class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime d)
        {
            SeedMock.Apply();
            d.MainWindow = CreateMain();
        }
        base.OnFrameworkInitializationCompleted();
    }

    /// <summary>The original index() constructor ran these checks and closed itself / showed Window1 on failure.</summary>
    static Window CreateMain()
    {
        if (AppHost.PageArg == "error") return new Window1("Unable to start CPU Overclocking Application because the following reason:\r\n\r\n-Intel XTU NOT RUNNING.");
        if (AppHost.PageArg == "error_model") return new Window1("This UWP app is not suitable for this model.");
        if (AppHost.PageArg == "error_bios") return new Window1("Unable to start CPU Overclocking Application because CPU OverClocking feature is disabled in BIOS Setup Menu.");
        if (!XtuStub.PlatformCompatible(out var issues))
        {
            var text = "Unable to start CPU Overclocking Application because an incompatible OS feature is enable:\r\n\r\n";
            foreach (var s in issues) text += "- " + s + "\r\n";
            return new Window1(text);
        }
        var vm = new CPUOC30.ViewModel.IndexViewModel();
        switch (vm.CheckSupportCPUOC())
        {
            case -1: return new Window1("This UWP app is not suitable for this model.");
            case -2: return new Window1("Unable to start CPU Overclocking Application because CPU OverClocking feature is disabled in BIOS Setup Menu.");
        }
        switch (vm.CheckXTUServiceStatus())
        {
            case -1: return new Window1("Unable to start CPU Overclocking Application because the following reason:\r\n\r\n-Intel XTU SDK NOT FOUND.");
            case -2: return new Window1("Unable to start CPU Overclocking Application because the following reason:\r\n\r\n-Intel XTU NOT RUNNING.");
        }
        return new index(vm);
    }
}
