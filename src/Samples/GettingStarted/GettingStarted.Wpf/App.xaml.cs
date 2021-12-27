using System;
using System.Collections.Generic;
using System.Windows;
using SeeingSharp;
using SeeingSharp.Core;

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
