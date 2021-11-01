using System;

namespace SeeingSharp.ModelViewer
{
    public class OpenFileDialogEventArgs : EventArgs
    {
        public string FilterString { get; }

        public string SelectedFile { get; set; }

        public OpenFileDialogEventArgs(string filterString)
        {
            this.FilterString = filterString;
            this.SelectedFile = string.Empty;
        }
    }

    public enum CameraMode
    {
        Perspective,

        Orthographic
    }
}
