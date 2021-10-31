/*
    SeeingSharp and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using System;
using System.Text;
using System.Windows;
using System.Windows.Threading;
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
                .RegisterAssimpImporter()
                .Load();

            base.OnStartup(e);
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
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
