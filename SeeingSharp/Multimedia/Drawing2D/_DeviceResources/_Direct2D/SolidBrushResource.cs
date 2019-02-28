#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion
#region using

// Some namespace mappings
using D2D = SharpDX.Direct2D1;

#endregion

namespace SeeingSharp.Multimedia.Drawing2D
{
    #region using

    using System;
    using Checking;
    using Core;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class SolidBrushResource : BrushResource
    {
        #region Resources
        private D2D.SolidColorBrush[] m_loadedBrushes;
        #endregion

        #region Configuration

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

            Opacity = opacity;
            Color = singleColor;
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
            if (base.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var result = m_loadedBrushes[engineDevice.DeviceIndex];

            if (result == null)
            {
                // Load the brush
                result = new D2D.SolidColorBrush(
                    engineDevice.FakeRenderTarget2D,
                    Color,
                    new D2D.BrushProperties()
                    {
                        Opacity = Opacity,
                        Transform = Matrix3x2.Identity
                    });
                m_loadedBrushes[engineDevice.DeviceIndex] = result;
            }

            return result;
        }

        public Color4 Color { get; }

        public float Opacity { get; }
    }
}