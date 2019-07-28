using SeeingSharp.Multimedia.Core;
using SeeingSharp;
using System.Reflection;
using System.Windows;

namespace SeeingSharpModelViewer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected async override void OnStartup(StartupEventArgs e)
        {
            GraphicsCore.Loader
                .SupportWpf()
                .Load();

            // Start wpf application
            base.OnStartup(e);

            // Execute bootstrapper
            await ModelViewerBootstrapper.ExecuteAsync();

            // Load the main window
            MainWindow mainWindow = new MainWindow();
            ViewServiceHandler viewServiceHandler = new ViewServiceHandler();
            viewServiceHandler.Initialize(mainWindow);
            mainWindow.Show();
        }
    }
}