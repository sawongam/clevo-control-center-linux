using System;
using RippleCC.Platform;
namespace GPUOverclocking;
static class Program
{
    [STAThread]
    public static int Main(string[] args) => AppHost.Run<App>(args);
}
