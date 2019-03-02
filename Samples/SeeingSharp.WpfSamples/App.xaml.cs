using System.Windows;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.WpfSamples
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