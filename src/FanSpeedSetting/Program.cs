using System;
using CCenter.Platform;
namespace FanSpeedSetting;
static class Program
{
    [STAThread]
    public static int Main(string[] args) => AppHost.Run<App>(args);
}
