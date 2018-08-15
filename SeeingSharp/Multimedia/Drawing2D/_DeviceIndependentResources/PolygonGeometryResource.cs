#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SeeingSharp.Util;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SharpDX;

// Namespace mappings
using D2D = SharpDX.Direct2D1;
using SDXM = SharpDX.Mathematics.Interop;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public class PolygonGeometryResource : Geometry2DResourceBase
    {
        #region resources
        private D2D.PathGeometry m_d2dGeometry;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonGeometryResource"/> class.
        /// </summary>
        public PolygonGeometryResource()
        {
            m_d2dGeometry = new D2D.PathGeometry(
                GraphicsCore.Current.FactoryD2D);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonGeometryResource"/> class.
        /// </summary>
        /// <param name="polygon">The data which populates the geometry.</param>
        public PolygonGeometryResource(Polygon2D polygon)
            : this()
        {
            SetContent(polygon);
        }

        /// <summary>
        /// Sets the content to all lines in the given polygon.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        public unsafe void SetContent(Polygon2D polygon)
        {
            polygon.EnsureNotNull(nameof(polygon));
            polygon.Vertices.EnsureMoreThanZeroElements($"{nameof(polygon)}.{nameof(polygon.Vertices)}");

            using (D2D.GeometrySink geoSink = m_d2dGeometry.Open())
            {
                ReadOnlyCollection<Vector2> vertices = polygon.Vertices;

                // Start the figure
                Vector2 startPoint = vertices[0];
                geoSink.BeginFigure(
                    *(SDXM.RawVector2*)&startPoint,
                    D2D.FigureBegin.Filled);

                // Add all lines
                int vertexCount = vertices.Count;
                for (int loop = 1; loop < vertexCount; loop++)
                {
                    Vector2 actVectorOrig = vertices[loop];
                    geoSink.AddLine(*(SDXM.RawVector2*)&actVectorOrig);
                }

                // End the figure
                geoSink.EndFigure(D2D.FigureEnd.Closed);
                geoSink.Close();
            }
        }

        /// <summary>
        /// Does this geometry intersect with the given one?
        /// </summary>
        /// <param name="otherGeometry">The other geometry.</param>
        public bool Intersects(PolygonGeometryResource otherGeometry)
        {
            this.EnsureNotNullOrDisposed("this");
            otherGeometry.EnsureNotNullOrDisposed(nameof(otherGeometry));

            D2D.GeometryRelation relation = m_d2dGeometry.Compare(otherGeometry.m_d2dGeometry);

            return
                (relation != D2D.GeometryRelation.Unknown) &&
                (relation != D2D.GeometryRelation.Disjoint);
        }

        /// <summary>
        /// Gets the geometry object.
        /// </summary>
        internal override D2D.Geometry GetGeometry()
        {
            return m_d2dGeometry;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            SeeingSharpTools.SafeDispose(ref m_d2dGeometry);
        }

        public override bool IsDisposed
        {
            get
            {
                return m_d2dGeometry == null;
            }
        }
    }
}
