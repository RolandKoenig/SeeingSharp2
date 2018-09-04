using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SeeingSharp.WinFormsSamples
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            GraphicsCore.Initialize(
                new DeviceLoadSettings()
                {
                    DebugEnabled = true
                },
                initializer => initializer.SupportWinForms());

            Application.Run(new MainWindow());
        }
    }
}
