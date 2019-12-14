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
using System.Numerics;
using SeeingSharp.Util;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Objects
{
    public class ExtrudeGeometryFactory : GeometryFactory
    {
        private List<D2D.Triangle[]> m_generatedTriangles;

        /// <summary>
        /// Create a new <see cref="ExtruderTessellationSink"/>
        /// </summary>
        /// <param name="geometry">The new geometry to be set.</param>
        /// <param name="flatteningTolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution.</param>
        /// <param name="extrudeOptions">Additional options for extruding.</param>
        public ExtrudeGeometryFactory(
            D2D.Geometry geometry, float flatteningTolerance, 
            ExtrudeGeometryOptions extrudeOptions = ExtrudeGeometryOptions.None)
        {
            this.UpdateGeometry(geometry, flatteningTolerance, extrudeOptions);
        }

        /// <summary>
        /// Changes current geometry of this object.
        /// </summary>
        /// <param name="geometry">The new geometry to be set.</param>
        /// <param name="flatteningTolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution.</param>
        /// <param name="extrudeOptions">Additional options for extruding.</param>
        public void UpdateGeometry(
            D2D.Geometry geometry, float flatteningTolerance,
            ExtrudeGeometryOptions extrudeOptions = ExtrudeGeometryOptions.None)
        {
            // Get triangles out of given geometry
            List<D2D.Triangle[]> generatedTriangles = null;
            using (var tessellationSink = new ExtruderTessellationSink())
            {
                geometry.Tessellate(flatteningTolerance, tessellationSink);
                generatedTriangles = tessellationSink.Triangles;
            }

            // Ensure that triangle list is created
            if(m_generatedTriangles == null) { m_generatedTriangles = new List<D2D.Triangle[]>(); }

            // Define methods for calculating bounds
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            var minPoint = Vector2.Zero;
            void UpdateMinWidthHeight(SharpDX.Mathematics.Interop.RawVector2 actCorner)
            {
                if (actCorner.X < minX) { minX = actCorner.X; }
                if (actCorner.X > maxX) { maxX = actCorner.X; }
                if (actCorner.Y < minY) { minY = actCorner.Y; }
                if (actCorner.Y > maxY) { maxY = actCorner.Y; }
            }

            //  Do some postprocessing
            var triangleCount = 0;
            var bounds = new Size2F();
            foreach (var actTriangleArray in generatedTriangles)
            {
                foreach (var actTriangle in actTriangleArray)
                {
                    UpdateMinWidthHeight(actTriangle.Point1);
                    UpdateMinWidthHeight(actTriangle.Point2);
                    UpdateMinWidthHeight(actTriangle.Point3);

                    triangleCount++;
                }
            }
            if (triangleCount > 0)
            {
                bounds = new Size2F(maxX - minX, maxY - minY);
                minPoint = new Vector2(minX, minY);
            }

            // Change with / height of the geometry depending on ExtrudeGeometryOptions
            if (extrudeOptions.HasFlag(ExtrudeGeometryOptions.RescaleToUnitSize))
            {
                var scaleFactorX = !EngineMath.EqualsWithTolerance(bounds.Width, 0f) ? 1f / bounds.Width : 1f;
                var scaleFactorY = !EngineMath.EqualsWithTolerance(bounds.Height, 0f) ? 1f / bounds.Height : 1f;
                if (scaleFactorX < scaleFactorY)
                {
                    scaleFactorY = scaleFactorX;
                }
                else
                {
                    scaleFactorX = scaleFactorY;
                }

                foreach (var actTriangleArray in generatedTriangles)
                {
                    for (var loop = 0; loop < actTriangleArray.Length; loop++)
                    {
                        var actTriangle = actTriangleArray[loop];
                        actTriangle.Point1 = new SharpDX.Mathematics.Interop.RawVector2(actTriangle.Point1.X * scaleFactorX, actTriangle.Point1.Y * scaleFactorY);
                        actTriangle.Point2 = new SharpDX.Mathematics.Interop.RawVector2(actTriangle.Point2.X * scaleFactorX, actTriangle.Point2.Y * scaleFactorY);
                        actTriangle.Point3 = new SharpDX.Mathematics.Interop.RawVector2(actTriangle.Point3.X * scaleFactorX, actTriangle.Point3.Y * scaleFactorY);

                        actTriangleArray[loop] = actTriangle;
                    }
                }
                bounds = new Size2F(
                    bounds.Width * scaleFactorX, 
                    bounds.Height * scaleFactorY);
                minPoint = new Vector2(minPoint.X * scaleFactorX, minPoint.Y * scaleFactorY);
            }

            // Change the origin depending on ExtrudeGeometryOptions
            if (extrudeOptions.HasFlag(ExtrudeGeometryOptions.ChangeOriginToCenter))
            {
                var newOrigin = new Vector2(
                    minPoint.X + bounds.Width / 2f,
                    minPoint.Y + bounds.Height / 2f);
                foreach (var actTriangleArray in generatedTriangles)
                {
                    for (var loop = 0; loop < actTriangleArray.Length; loop++)
                    {
                        var actTriangle = actTriangleArray[loop];
                        actTriangle.Point1 = SdxMathHelper.RawFromVector2(SdxMathHelper.Vector2FromRaw(actTriangle.Point1) - newOrigin);
                        actTriangle.Point2 = SdxMathHelper.RawFromVector2(SdxMathHelper.Vector2FromRaw(actTriangle.Point2) - newOrigin);
                        actTriangle.Point3 = SdxMathHelper.RawFromVector2(SdxMathHelper.Vector2FromRaw(actTriangle.Point3) - newOrigin);

                        actTriangleArray[loop] = actTriangle;
                    }
                }
                minPoint = new Vector2(0f, 0f);
            }

            // Apply values
            this.TriangleCount = triangleCount;
            this.Bounds = bounds;
            m_generatedTriangles = generatedTriangles;
        }

        /// <inheritdoc />
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry(this.TriangleCount * 3);
            var surface = result.CreateSurface(this.TriangleCount);

            var generatedTriangles = m_generatedTriangles;
            foreach (var actTriangleArray in generatedTriangles)
            {
                foreach (var actTriangle in actTriangleArray)
                {
                    surface.BuildTriangleV(
                        new Vector3(actTriangle.Point1.X, 0f, actTriangle.Point1.Y),
                        new Vector3(actTriangle.Point2.X, 0f, actTriangle.Point2.Y),
                        new Vector3(actTriangle.Point3.X, 0f, actTriangle.Point3.Y), this.Color);
                }
            }

            return result;
        }

        /// <summary>
        /// Total count of generated triangles
        /// </summary>
        public int TriangleCount { get; private set; }

        /// <summary>
        /// Bounds of the given geometry.
        /// </summary>
        public Size2F Bounds { get; private set; }

        /// <summary>
        /// The <see cref="Color4"/> to be assigned to each generated <see cref="Vertex"/>
        /// </summary>
        public Color4 Color { get; set; } = Color4.Transparent;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ExtruderTessellationSink : DummyComObject, D2D.TessellationSink
        {
            public void AddTriangles(D2D.Triangle[] triangles)
            {
                if (triangles == null ||
                    triangles.Length == 0)
                {
                    return;
                }

                this.Triangles.Add(triangles);
            }

            public void Close()
            {
                
            }

            public List<D2D.Triangle[]> Triangles { get; } = new List<D2D.Triangle[]>();
        }
    }
}
