using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer.Util
{
    public class RenderingOptions
    {
        private readonly RenderLoop m_renderLoop;

        public RenderingOptions(RenderLoop renderLoop)
        {
            m_renderLoop = renderLoop;
        }

        [Category("Rendering")]
        public bool Antialiasing
        {
            get => m_renderLoop.Configuration.AntialiasingEnabled;
            set => m_renderLoop.Configuration.AntialiasingEnabled = value;
        }

        [Category("Rendering")]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => m_renderLoop.Configuration.AntialiasingQuality;
            set
            {
                if (value != m_renderLoop.Configuration.AntialiasingQuality)
                {
                    m_renderLoop.Configuration.AntialiasingQuality = value;
                }
            }
        }
    }
}
