/*
    Seeing# and all applications distributed together with it. 
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

using System.Collections.Generic;
using System.ComponentModel;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.SampleContainer
{
    public class SampleSettingsWith3D : SampleSettings
    {
        [Category("3D Rendering")]
        public bool EnableAntialiasing
        {
            get => base.RenderLoop?.ViewConfiguration.AntialiasingEnabled ?? false;
            set
            {
                if(value != base.RenderLoop?.ViewConfiguration.AntialiasingEnabled)
                {
                    base.RenderLoop.ViewConfiguration.AntialiasingEnabled = value;
                    RaisePropertyChanged(nameof(EnableAntialiasing));
                }
            }
        }

        [Category("3D Rendering")]
        public bool EnableWireframe
        {
            get => base.RenderLoop?.ViewConfiguration.WireframeEnabled ?? false;
            set
            {
                if (value != base.RenderLoop?.ViewConfiguration.WireframeEnabled)
                {
                    base.RenderLoop.ViewConfiguration.WireframeEnabled = value;
                    RaisePropertyChanged(nameof(EnableWireframe));
                }
            }
        }

        [Category("3D Rendering")]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => base.RenderLoop?.ViewConfiguration.AntialiasingQuality ?? AntialiasingQualityLevel.Medium;
            set
            {
                if(value != base.RenderLoop?.ViewConfiguration.AntialiasingQuality)
                {
                    base.RenderLoop.ViewConfiguration.AntialiasingQuality = value;
                    RaisePropertyChanged(nameof(AntialiasingQuality));
                }
            }
        }
    }
}
