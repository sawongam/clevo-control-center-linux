using System;
using RippleCC.Platform;
namespace DesktopBridge_EnergySave;
static class Program
{
    [STAThread]
    public static int Main(string[] args) => AppHost.Run<App>(args);
}
