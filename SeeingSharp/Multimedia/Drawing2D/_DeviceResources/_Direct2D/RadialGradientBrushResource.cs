﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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

    public class RadialGradientBrushResource : BrushResource
    {
        #region Resources
        private LoadedBrushResources[] m_loadedBrushes;
        #endregion

        #region Configuration
        private GradientStop[] m_gradientStops;
        private ExtendMode m_extendMode;
        private Gamma m_gamma;
        private Vector2 m_center;
        private Vector2 m_gradientOriginOffset;
        private float m_radiusX;
        private float m_radiusY;
        private float m_opacity;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="RadialGradientBrushResource" /> class.
        /// </summary>
        public RadialGradientBrushResource(
            Vector2 center, Vector2 gradientOriginOffset, float radiusX, float radiusY,
            GradientStop[] gradientStops,
            ExtendMode extendMode = ExtendMode.Clamp,
            Gamma gamma = Gamma.StandardRgb,
            float opacity = 1f)
        {;
            gradientStops.EnsureNotNullOrEmpty(nameof(gradientStops));
            opacity.EnsureInRange(0f, 1f, nameof(opacity));
            radiusX.EnsurePositive(nameof(radiusX));
            radiusY.EnsurePositive(nameof(radiusY));

            m_gradientStops = gradientStops;
            m_center = center;
            m_gradientOriginOffset = gradientOriginOffset;
            m_radiusX = radiusX;
            m_radiusY = radiusY;
            m_extendMode = extendMode;
            m_gamma = gamma;
            m_opacity = opacity;

            m_loadedBrushes = new LoadedBrushResources[GraphicsCore.Current.DeviceCount];

        }

        /// <summary>
        /// Unloads all resources loaded on the given device.
        /// </summary>
        /// <param name="engineDevice">The device for which to unload the resource.</param>
        internal override void UnloadResources(EngineDevice engineDevice)
        {
            LoadedBrushResources loadedBrush = m_loadedBrushes[engineDevice.DeviceIndex];
            if (loadedBrush.Brush != null)
            {
                loadedBrush.Brush = SeeingSharpTools.DisposeObject(loadedBrush.Brush);
                loadedBrush.GradientStops = SeeingSharpTools.DisposeObject(loadedBrush.GradientStops);

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
            if (base.IsDisposed) { throw new ObjectDisposedException(this.GetType().Name); }

            LoadedBrushResources result = m_loadedBrushes[engineDevice.DeviceIndex];
            if (result.Brush == null)
            {
                // Convert gradient stops to structure from SharpDX
                D2D.GradientStop[] d2dGradientStops = new D2D.GradientStop[m_gradientStops.Length];
                for(int loop=0; loop<d2dGradientStops.Length; loop++)
                {
                    d2dGradientStops[loop] = new D2D.GradientStop()
                    {
                        Color = m_gradientStops[loop].Color,
                        Position = m_gradientStops[loop].Position
                    };
                }

                // Create the brush
                result = new LoadedBrushResources();
                result.GradientStops = new D2D.GradientStopCollection(
                    engineDevice.FakeRenderTarget2D,
                    d2dGradientStops,
                    (D2D.Gamma)m_gamma,
                    (D2D.ExtendMode)m_extendMode);
                result.Brush = new D2D.RadialGradientBrush(
                    engineDevice.FakeRenderTarget2D,
                    new D2D.RadialGradientBrushProperties()
                    {
                        Center = m_center,
                        GradientOriginOffset = m_gradientOriginOffset,
                        RadiusX = m_radiusX,
                        RadiusY = m_radiusY
                    },
                    new D2D.BrushProperties()
                    {
                        Opacity = m_opacity,
                        Transform = Matrix3x2.Identity
                    },
                    result.GradientStops);

                m_loadedBrushes[engineDevice.DeviceIndex] = result;
            }

            return result.Brush;
        }

        public Gamma Gamma
        {
            get { return m_gamma; }
        }

        public ExtendMode ExtendMode
        {
            get { return m_extendMode; }
        }

        public Vector2 Center
        {
            get { return m_center; }
        }

        public Vector2 GradientOriginOffset
        {
            get { return m_gradientOriginOffset; }
        }

        public float RadiusX
        {
            get { return m_radiusX; }
        }

        public float RadiusY
        {
            get { return m_radiusY; }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// A simple helper storing both resurces..
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
