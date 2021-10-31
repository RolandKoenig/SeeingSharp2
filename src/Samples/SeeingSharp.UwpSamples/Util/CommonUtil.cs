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
using System.Reflection;
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
            appView.Title = $@"{windowName} - {Assembly.GetExecutingAssembly().GetName().Version}";

            // 0975C6
            // 3689C4
            // Set titlebar colors
            var titleBar = appView.TitleBar;
            titleBar.BackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0x09, 0x75, 0xC6);
            titleBar.ForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0xFE, 0xFE, 0xFE);
            titleBar.ButtonBackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0x09, 0x75, 0xC6);
            titleBar.ButtonForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0xFE, 0xFE, 0xFE);
            titleBar.InactiveBackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0x36, 0x89, 0xC4);
            titleBar.ButtonInactiveBackgroundColor = Windows.UI.Color.FromArgb(0xFF, 0x36, 0x89, 0xC4);
            titleBar.InactiveForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0xFE, 0xFE, 0xFE);
            titleBar.ButtonInactiveForegroundColor = Windows.UI.Color.FromArgb(0xFF, 0xFE, 0xFE, 0xFE);
        }
    }
}
