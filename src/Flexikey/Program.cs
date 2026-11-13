using System;
using RippleCC.Platform;
namespace Flexikey;
static class Program
{
    [STAThread]
    public static int Main(string[] args) => AppHost.Run<CC.App>(args);
}
