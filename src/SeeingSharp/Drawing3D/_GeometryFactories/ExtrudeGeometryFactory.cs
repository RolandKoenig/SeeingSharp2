using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing3D
{
    public class ExtrudeGeometryFactory : GeometryFactory
    {
        private List<D2D.Triangle[]> _generatedTriangles;

        /// <summary>
        /// Total count of generated triangles
        /// </summary>
        public int TriangleCount { get; private set; }

        /// <summary>
        /// Bounds of the given geometry.
        /// </summary>
        public SizeF Bounds { get; private set; }

        public ExtrudeGeometryFactoryInternals Internals { get; }

        /// <summary>
        /// Create a new <see cref="ExtruderTessellationSink"/>
        /// </summary>
        public ExtrudeGeometryFactory()
        {
            _generatedTriangles = new List<D2D.Triangle[]>();

            this.Internals = new ExtrudeGeometryFactoryInternals(this);
        }

        /// <inheritdoc />
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            // Handle empty case
            if (_generatedTriangles.Count == 0)
            {
                return new Geometry(0);
            }

            var result = new Geometry(this.TriangleCount * 3);
            var surface = result.CreateSurface(this.TriangleCount);

            var generatedTriangles = _generatedTriangles;
            foreach (var actTriangleArray in generatedTriangles)
            {
                foreach (var actTriangle in actTriangleArray)
                {
                    surface.BuildTriangle(
                        new Vector3(actTriangle.Point1.X, 0f, actTriangle.Point1.Y),
                        new Vector3(actTriangle.Point2.X, 0f, actTriangle.Point2.Y),
                        new Vector3(actTriangle.Point3.X, 0f, actTriangle.Point3.Y));
                }
            }

            return result;
        }

        /// <summary>
        /// Sets the current geometry of this object.
        /// </summary>
        /// <param name="geometry">The new geometry to be set.</param>
        /// <param name="flatteningTolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution.</param>
        /// <param name="extrudeOptions">Additional options for extruding.</param>
        internal void SetGeometry(
            D2D.ID2D1Geometry geometry, float flatteningTolerance,
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
            if (_generatedTriangles == null) { _generatedTriangles = new List<D2D.Triangle[]>(); }

            // Define methods for calculating bounds
            var minX = float.MaxValue;
            var maxX = float.MinValue;
            var minY = float.MaxValue;
            var maxY = float.MinValue;
            var minPoint = Vector2.Zero;
            void UpdateMinWidthHeight(PointF actCorner)
            {
                if (actCorner.X < minX) { minX = actCorner.X; }
                if (actCorner.X > maxX) { maxX = actCorner.X; }
                if (actCorner.Y < minY) { minY = actCorner.Y; }
                if (actCorner.Y > maxY) { maxY = actCorner.Y; }
            }

            //  Do some postprocessing
            var triangleCount = 0;
            var bounds = new SizeF();
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
                bounds = new SizeF(maxX - minX, maxY - minY);
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
                        actTriangle.Point1 = new PointF(actTriangle.Point1.X * scaleFactorX, actTriangle.Point1.Y * scaleFactorY);
                        actTriangle.Point2 = new PointF(actTriangle.Point2.X * scaleFactorX, actTriangle.Point2.Y * scaleFactorY);
                        actTriangle.Point3 = new PointF(actTriangle.Point3.X * scaleFactorX, actTriangle.Point3.Y * scaleFactorY);

                        actTriangleArray[loop] = actTriangle;
                    }
                }
                bounds = new SizeF(
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
            _generatedTriangles = generatedTriangles;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class ExtrudeGeometryFactoryInternals
        {
            private ExtrudeGeometryFactory _owner;

            internal ExtrudeGeometryFactoryInternals(ExtrudeGeometryFactory owner)
            {
                _owner = owner;
            }

            /// <summary>
            /// Sets the current geometry of this object.
            /// </summary>
            /// <param name="geometry">The new geometry to be set.</param>
            /// <param name="flatteningTolerance">The maximum bounds on the distance between points in the polygonal approximation of the geometry. Smaller values produce more accurate results but cause slower execution.</param>
            /// <param name="extrudeOptions">Additional options for extruding.</param>
            public void SetGeometry(
                D2D.ID2D1Geometry geometry, float flatteningTolerance,
                ExtrudeGeometryOptions extrudeOptions = ExtrudeGeometryOptions.None)
            {
                _owner.SetGeometry(geometry, flatteningTolerance, extrudeOptions);
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ExtruderTessellationSink : DummyComObject, D2D.ID2D1TessellationSink
        {
            public List<D2D.Triangle[]> Triangles { get; } = new List<D2D.Triangle[]>();

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
        }
    }
}
