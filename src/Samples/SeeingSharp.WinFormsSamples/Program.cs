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
            GraphicsCore.Loader
                .SupportWinForms()
                .Load();

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}