using System;
using System.Diagnostics;

namespace SeeingSharp.SampleContainer
{
    public static class PlatformDependentMethods
    {
        private static Action<string>? s_showUrlInBrowser;

        public static void SetOpenUrlInBrowser(Action<string> showUrlInBrowser)
        {
            s_showUrlInBrowser = showUrlInBrowser;
        }

        public static void OpenUrlInBrowser(string url)
        {
            if (s_showUrlInBrowser != null) { s_showUrlInBrowser(url); }
            else
            {
                // Default implementation
                var psi = new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                };
                Process.Start(psi);
            }
        }
    }
}