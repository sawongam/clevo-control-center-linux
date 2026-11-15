using System;
using CCenter.Platform;
namespace ControlCenter30;
static class Program
{
    [STAThread]
    public static int Main(string[] args) => AppHost.Run<App>(args);
}
