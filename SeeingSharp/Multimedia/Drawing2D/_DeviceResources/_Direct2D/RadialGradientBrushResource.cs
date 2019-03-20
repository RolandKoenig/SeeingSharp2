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

using System;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;
using SharpDX;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class RadialGradientBrushResource : BrushResource
    {
        // Resources
        private LoadedBrushResources[] m_loadedBrushes;

        // Configuration
        private D2D.GradientStop[] m_gradientStops;
        private float m_opacity;

        /// <summary>
        /// Initializes a new instance of the <see cref="RadialGradientBrushResource" /> class.
        /// </summary>
        public RadialGradientBrushResource(
            Vector2 center, Vector2 gradientOriginOffset, float radiusX, float radiusY,
            D2D.GradientStop[] gradientStops,
            D2D.ExtendMode extendMode = D2D.ExtendMode.Clamp,
            D2D.Gamma gamma = D2D.Gamma.StandardRgb,
            float opacity = 1f)
        {;
            gradientStops.EnsureNotNullOrEmpty(nameof(gradientStops));
            opacity.EnsureInRange(0f, 1f, nameof(opacity));
            radiusX.EnsurePositive(nameof(radiusX));
            radiusY.EnsurePositive(nameof(radiusY));

            m_gradientStops = gradientStops;
            this.Center = center;
            this.GradientOriginOffset = gradientOriginOffset;
            this.RadiusX = radiusX;
            this.RadiusY = radiusY;
            this.ExtendMode = extendMode;
            this.Gamma = gamma;
            m_opacity = opacity;

            m_loadedBrushes = new LoadedBrushResources[GraphicsCore.Current.DeviceCount];
        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            var loadedBrush = m_loadedBrushes[engineDevice.DeviceIndex];

            if (loadedBrush.Brush != null)
            {
                loadedBrush.Brush = SeeingSharpUtil.DisposeObject(loadedBrush.Brush);
                loadedBrush.GradientStops = SeeingSharpUtil.DisposeObject(loadedBrush.GradientStops);

                m_loadedBrushes[engineDevice.DeviceIndex] = loadedBrush;
            }
        }

        /// <summary>
        /// Gets the brush for the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to get the brush.</param>
        internal override D2D.Brush GetBrush(EngineDevice engineDevice)
        {
            // Check for disposed state
            if (this.IsDisposed)
            {
                throw new ObjectDisposedException(this.GetType().Name);
            }

            var result = m_loadedBrushes[engineDevice.DeviceIndex];

            if (result.Brush == null)
            {
                // Convert gradient stops to structure from SharpDX
                var d2dGradientStops = new D2D.GradientStop[m_gradientStops.Length];

                for(var loop =0; loop<d2dGradientStops.Length; loop++)
                {
                    d2dGradientStops[loop] = new D2D.GradientStop
                    {
                        Color = m_gradientStops[loop].Color,
                        Position = m_gradientStops[loop].Position
                    };
                }

                // Create the brush
                result = new LoadedBrushResources
                {
                    GradientStops = new D2D.GradientStopCollection(
                        engineDevice.FakeRenderTarget2D,
                        d2dGradientStops,
                        this.Gamma,
                        this.ExtendMode)
                };

                result.Brush = new D2D.RadialGradientBrush(
                    engineDevice.FakeRenderTarget2D,
                    new D2D.RadialGradientBrushProperties
                    {
                        Center = this.Center,
                        GradientOriginOffset = this.GradientOriginOffset,
                        RadiusX = this.RadiusX,
                        RadiusY = this.RadiusY
                    },
                    new D2D.BrushProperties
                    {
                        Opacity = m_opacity,
                        Transform = Matrix3x2.Identity
                    },
                    result.GradientStops);

                m_loadedBrushes[engineDevice.DeviceIndex] = result;
            }

            return result.Brush;
        }

        public D2D.Gamma Gamma { get; }

        public D2D.ExtendMode ExtendMode { get; }

        public Vector2 Center { get; }

        public Vector2 GradientOriginOffset { get; }

        public float RadiusX { get; }

        public float RadiusY { get; }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// A simple helper storing both resources..
        ///  - the GradientStopCollection
        ///  - and the RadialGradientBrush itself
        /// </summary>
        private struct LoadedBrushResources
        {
            public D2D.GradientStopCollection GradientStops;
            public D2D.RadialGradientBrush Brush;
        }
    }
}