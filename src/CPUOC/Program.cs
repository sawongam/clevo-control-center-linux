using System;
using CCenter.Platform;
namespace CPUOC30;
static class Program
{
    [STAThread]
    public static int Main(string[] args) => AppHost.Run<App>(args);
}
