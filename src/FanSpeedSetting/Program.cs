using System;
using RippleCC.Platform;
namespace FanSpeedSetting;
static class Program
{
    [STAThread]
    public static int Main(string[] args) => AppHost.Run<App>(args);
}
