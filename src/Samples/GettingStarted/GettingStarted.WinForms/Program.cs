using System;
using System.Collections.Generic;
using System.Windows.Forms;
using SeeingSharp;
using SeeingSharp.Core;

namespace GettingStarted.WinForms
{
    static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            GraphicsCore.Loader
                .SupportWinForms()
                .Load();

            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }
    }
}
