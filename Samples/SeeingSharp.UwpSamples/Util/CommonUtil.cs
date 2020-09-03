using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.UI.ViewManagement;

namespace SeeingSharp.UwpSamples.Util
{
    internal static class CommonUtil
    {
        public static void UpdateApplicationTitleBar(string windowName)
        {
            // Manipulate titlebar
            // --> see https://docs.microsoft.com/en-us/windows/uwp/design/shell/title-bar

            // First enable real titlebar 
            // Be careful, this property is persisted by the runtime. If we set it to true (also if only for a try), it will stay true after restart of the app
            var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            coreTitleBar.ExtendViewIntoTitleBar = false;

            // Set window title
            var package = Package.Current;
            var appName = package.DisplayName;
            var appView = ApplicationView.GetForCurrentView();
            appView.Title = $@"{appName} - {windowName} - {Assembly.GetExecutingAssembly().GetName().Version}";

            // Set titlebar colors
            var titleBar = appView.TitleBar;
            titleBar.BackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0x00, 0xCC, 0xFF);
            titleBar.ForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0xFE, 0xFE, 0xFE);
            titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0x00, 0xCC, 0xFF);
            titleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0xFE, 0xFE, 0xFE);
        }
    }
}
