using System.Windows;
using SeeingSharp;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharpModelViewer
{
    #region using
    #endregion

    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
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