using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SeeingSharp;
using SeeingSharp.Multimedia.Core;

namespace GettingStarted.Wpf
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            GraphicsCore.Loader
                .SupportWpf()
                .Load();

            base.OnStartup(e);
        }
    }
}
