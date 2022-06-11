using System;
using System.Windows.Forms;
using SeeingSharp.Core;

namespace SeeingSharp.WinFormsSamples
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
#if NETCOREAPP3_0_OR_GREATER
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif

            GraphicsCore.Loader
                .SupportWinForms()
                .Load();

            Application.Run(new MainWindow());
        }
    }
}