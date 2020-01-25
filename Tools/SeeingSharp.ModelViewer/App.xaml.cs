using System;
using System.Text;
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
            this.DispatcherUnhandledException += this.OnDispatcherUnhandledException;

            GraphicsCore.Loader
                .SupportWpf()
                .RegisterModelImporter(new AssimpImporter())
                .Load();

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            const string msgBoxCaption = "SeeingSharp ModelViewer";

            // Extract basic information from the exception
            var messageBuilder = new StringBuilder(128);
            messageBuilder.AppendLine("Unhandled exception:");
            messageBuilder.AppendLine();
            messageBuilder.AppendLine($"Message: {e.Exception.Message}");
            messageBuilder.AppendLine($"Type: {e.Exception.GetType().FullName}");

            // Try to get the most interesting line from the stacktrace
            var stackTrace = e.Exception.StackTrace;
            if (!string.IsNullOrEmpty(stackTrace))
            {
                foreach (var actLine in stackTrace.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (!actLine.Contains($"{nameof(SeeingSharp)}.{nameof(ModelViewer)}")){ continue; }

                    messageBuilder.AppendLine();
                    messageBuilder.AppendLine($"Location: {actLine}");
                    break;
                }
            }

            // Show the message box
            var mainWindow = this.MainWindow;
            if (mainWindow != null)
            {
                MessageBox.Show(
                    mainWindow,
                    messageBuilder.ToString(), msgBoxCaption,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                MessageBox.Show(
                    messageBuilder.ToString(), msgBoxCaption,
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
        }
    }
}
