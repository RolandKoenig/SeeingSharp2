#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SeeingSharp.SampleContainer
{
    public class SampleSettings : PropertyChangedBase
    {
        private RenderLoop m_renderLoop;
        private SampleMetadata m_sampleMetadata;

        public SampleSettings()
        {

        }

        public virtual IEnumerable<SampleCommand> GetCommands()
        {
            yield return new SampleCommand(
                "Show Source",
                () => PlatformDependentMethods.OpenUrlInBrowser(m_sampleMetadata.SourceCodeUrl),
                () => !string.IsNullOrEmpty(m_sampleMetadata?.SourceCodeUrl));
        }

        public void SetEnvironment(RenderLoop renderLoop, SampleMetadata sampleMetadata)
        {
            m_renderLoop = renderLoop;
            m_sampleMetadata = sampleMetadata;

            // Trigger refresh of all properties
            RaisePropertyChanged();
        }

        [Category("Basics")]
        public bool EnableAntialiasing
        {
            get => m_renderLoop?.ViewConfiguration.AntialiasingEnabled ?? false;
            set
            {
                if(value != m_renderLoop?.ViewConfiguration.AntialiasingEnabled)
                {
                    m_renderLoop.ViewConfiguration.AntialiasingEnabled = value;
                    RaisePropertyChanged(nameof(EnableAntialiasing));
                }
            }
        }

        [Category("Basics")]
        public bool EnableWireframe
        {
            get => m_renderLoop?.ViewConfiguration.WireframeEnabled ?? false;
            set
            {
                if (value != m_renderLoop?.ViewConfiguration.WireframeEnabled)
                {
                    m_renderLoop.ViewConfiguration.WireframeEnabled = value;
                    RaisePropertyChanged(nameof(EnableWireframe));
                }
            }
        }

        [Category("Basics")]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => m_renderLoop?.ViewConfiguration.AntialiasingQuality ?? AntialiasingQualityLevel.Medium;
            set
            {
                if(value != m_renderLoop?.ViewConfiguration.AntialiasingQuality)
                {
                    m_renderLoop.ViewConfiguration.AntialiasingQuality = value;
                    RaisePropertyChanged(nameof(AntialiasingQuality));
                }
            }
        }
    }
}
