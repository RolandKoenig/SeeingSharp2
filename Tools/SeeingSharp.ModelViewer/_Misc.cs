using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.ModelViewer
{
    public class OpenFileDialogEventArgs : EventArgs
    {
        public OpenFileDialogEventArgs(string filterString)
        {
            this.FilterString = filterString;
            this.SelectedFile = string.Empty;
        }

        public string FilterString { get; }

        public string SelectedFile { get; set; }
    }
}
