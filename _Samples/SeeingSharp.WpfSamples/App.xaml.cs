using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            GraphicsCore.Initialize(
                new DeviceLoadSettings()
                {
                    DebugEnabled = true
                },
                initializer => initializer.SupportWpf());

            base.OnStartup(e);
        }
    }
}
