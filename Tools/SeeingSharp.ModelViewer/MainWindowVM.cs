using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer
{
    public class MainWindowVM : PropertyChangedBase
    {
        private RenderLoop m_renderLoop;
        private string m_loadedFile;

        public MainWindowVM(RenderLoop renderLoop)
        {
            m_renderLoop = renderLoop;
            m_loadedFile = string.Empty;

            this.UpdateTitle();
        }

        private void UpdateTitle()
        { 
            var titleBuilder = new StringBuilder(128);
            titleBuilder.Append("SeeingSharp 2 ModelViewer");
            if (!string.IsNullOrEmpty(m_loadedFile))
            {
                titleBuilder.Append($" - {m_loadedFile}");
            }

            this.AppTitle = titleBuilder.ToString();
            this.RaisePropertyChanged(nameof(this.AppTitle));
        }

        public string AppTitle
        {
            get;
            set;
        }

    }
}
