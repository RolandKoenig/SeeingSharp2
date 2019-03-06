#region License information
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

// Namespace mappings
using D2D = SharpDX.Direct2D1;
using SDXM = SharpDX.Mathematics.Interop;

#endregion

namespace SeeingSharp.Multimedia.Drawing2D
{
    #region using

    using Checking;
    using Core;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class EllipseGeometryResource : Geometry2DResourceBase
    {
        #region Resources
        private D2D.EllipseGeometry m_geometry;
        #endregion

        #region Configuration

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="EllipseGeometryResource"/> class.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="radiusX">The radius in x direction.</param>
        /// <param name="radiusY">The radius in y direction.</param>
        public EllipseGeometryResource(Vector2 center, float radiusX, float radiusY)
        {
            SetContent(center, radiusX, radiusY);
        }

        /// <summary>
        /// Sets the content to all lines in the given polygon.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="radiusX">The radius in x direction.</param>
        /// <param name="radiusY">The radius in y direction.</param>
        public unsafe void SetContent(Vector2 center, float radiusX, float radiusY)
        {
            radiusX.EnsurePositive(nameof(radiusX));
            radiusY.EnsurePositive(nameof(radiusY));

            Center = center;
            RadiusX = radiusX;
            RadiusY = radiusY;

            SeeingSharpTools.SafeDispose(ref m_geometry);
            m_geometry = new D2D.EllipseGeometry(
                GraphicsCore.Current.FactoryD2D,
                new D2D.Ellipse(
                    *(SDXM.RawVector2*)&center,
                    radiusX, radiusY));
        }

        /// <summary>
        /// Gets the geometry object.
        /// </summary>
        internal override D2D.Geometry GetGeometry()
        {
            this.EnsureNotNullOrDisposed(nameof(m_geometry));

            return m_geometry;
        }

        public override void Dispose()
        {
            SeeingSharpTools.SafeDispose(ref m_geometry);
        }

        public override bool IsDisposed => m_geometry == null;

        public Vector2 Center { get; private set; }

        public float RadiusX { get; private set; }

        public float RadiusY { get; private set; }
    }
}