using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Util;
using SharpDX;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Objects._GeometryFactories
{
    public class ExtrudeGeometryFactory : GeometryFactory
    {
        private List<D2D.Triangle[]> m_generatedTriangles;

        /// <summary>
        /// Create a new <see cref="ExtruderTessellationSink"/>
        /// </summary>
        /// <param name="geometry">The new geometry to be set.</param>
        /// <param name="flatteningTolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution.</param>
        public ExtrudeGeometryFactory(D2D.Geometry geometry, float flatteningTolerance)
        {
            this.UpdateGeometry(geometry, flatteningTolerance);
        }

        /// <summary>
        /// Changes current geometry of this object.
        /// </summary>
        /// <param name="geometry">The new geometry to be set.</param>
        /// <param name="flatteningTolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution.</param>
        public void UpdateGeometry(D2D.Geometry geometry, float flatteningTolerance)
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
            void UpdateMinWidthHeight(Vector2 actCorner)
            {
                if (actCorner.X < minX) { minX = actCorner.X; }
                if (actCorner.X > maxX) { maxX = actCorner.X; }
                if (actCorner.Y < minY) { minY = actCorner.Y; }
                if (actCorner.Y > maxY) { maxY = actCorner.Y; }
            }

            //  Do some postprocessing
            var triangleCount = 0;
            var bounds = Size2F.Empty;
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
            }

            // Apply values
            TriangleCount = triangleCount;
            Bounds = bounds;
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
                        new Vector3(actTriangle.Point3.X, 0f, actTriangle.Point3.Y),
                        this.Color);
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
        public Color4 Color { get; set; } = Color4Ex.Transparent;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ExtruderTessellationSink : DummyComObject, D2D.TessellationSink
        {
            public void AddTriangles(D2D.Triangle[] triangles)
            {
                if ((triangles == null) ||
                    (triangles.Length == 0))
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
