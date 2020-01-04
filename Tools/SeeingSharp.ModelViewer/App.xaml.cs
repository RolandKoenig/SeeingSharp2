using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            GraphicsCore.Loader
                .SupportWpf()
                .RegisterModelImporter(new AssimpImporter())
                .Load();

            base.OnStartup(e);
        }
    }
}
