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
using SeeingSharp.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

// Some namespace mappings
using D2D = SharpDX.Direct2D1;
using System.Numerics;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class SolidBrushResource : BrushResource
    {
        #region Resources
        private D2D.SolidColorBrush[] m_loadedBrushes;
        #endregion

        #region Configuration
        private Color4 m_singleColor;
        private float m_opacity;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="SolidBrushResource" /> class.
        /// </summary>
        /// <param name="singleColor">Color of the single.</param>
        /// <param name="opacity">The opacity value of the brush.</param>
        public SolidBrushResource(
            Color4 singleColor,
            float opacity = 1f)
        {
            opacity.EnsureInRange(0f, 1f, nameof(opacity));

            m_loadedBrushes = new D2D.SolidColorBrush[GraphicsCore.Current.DeviceCount];

            m_opacity = opacity;
            m_singleColor = singleColor;
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            D2D.Brush brush = m_loadedBrushes[engineDevice.DeviceIndex];
            if(brush != null)
            {
                SeeingSharpTools.DisposeObject(brush);
                m_loadedBrushes[engineDevice.DeviceIndex] = null;
            }
        }

        /// <summary>
        /// Gets the brush for the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to get the brush.</param>
        internal override D2D.Brush GetBrush(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (base.IsDisposed) { throw new ObjectDisposedException(this.GetType().Name); }

            D2D.SolidColorBrush result = m_loadedBrushes[engineDevice.DeviceIndex];
            if (result == null)
            {
                // Load the brush
                result = new D2D.SolidColorBrush(
                    engineDevice.FakeRenderTarget2D,
                    m_singleColor,
                    new D2D.BrushProperties()
                    {
                        Opacity = m_opacity,
                        Transform = Matrix3x2.Identity
                    });
                m_loadedBrushes[engineDevice.DeviceIndex] = result;
            }

            return result;
        }

        public Color4 Color
        {
            get { return m_singleColor; }
        }

        public float Opacity
        {
            get { return m_opacity; }
        }
    }
}