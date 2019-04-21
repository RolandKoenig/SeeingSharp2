using Microsoft.Win32;
using SeeingSharp.Util;
using System.Windows;

namespace SeeingSharpModelViewer
{
    public class CommonDialogsViewService : ICommonDialogsViewService
    {
        private FrameworkElement m_host;

        public void SetViewServiceHost(FrameworkElement frameworkElement)
        {
            m_host = frameworkElement;
        }

        public ResourceLink PickFileByDialog(string fileFilter)
        {
            // Prepare the dialog
            OpenFileDialog dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = fileFilter;
            dlgOpenFile.CheckFileExists = true;
            dlgOpenFile.CheckPathExists = true;
            dlgOpenFile.Multiselect = false;
            dlgOpenFile.RestoreDirectory = true;
            dlgOpenFile.ValidateNames = true;

            // Show the dialog and return the result
            if (true == dlgOpenFile.ShowDialog(Window.GetWindow(m_host)))
            {
                return dlgOpenFile.FileName;
            }
            else
            {
                return null;
            }
        }
    }
}