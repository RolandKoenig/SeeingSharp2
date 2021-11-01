using System;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public partial class GeometrySurface
    {
        // Members for build-time transform
        private Vector2 _tileSize = Vector2.Zero;

        /// <summary>
        /// Enables texture tile mode.
        /// </summary>
        public void EnableTextureTileMode(Vector2 tileSize)
        {
            _tileSize = tileSize;
        }

        public bool IsTextureTileModeEnabled(out Vector2 tileSize)
        {
            tileSize = _tileSize;
            return tileSize != Vector2.Zero;
        }

        /// <summary>
        /// Disables texture tile mode.
        /// </summary>
        public void DisableTextureTileMode()
        {
            _tileSize = Vector2.Zero;
        }

        /// <summary>
        /// Performs a simple picking test against all triangles of this object.
        /// </summary>
        /// <param name="pickingRay">The picking ray.</param>
        /// <param name="distance">Additional picking options.</param>
        /// <param name="pickingOptions">The distance if picking succeeds.</param>
        public bool Intersects(Ray pickingRay, PickingOptions pickingOptions, out float distance)
        {
            distance = float.MaxValue;
            var result = false;

            for (var loop = 0; loop < _corners.Count; loop += 3)
            {
                ref var vertex1 = ref this.Owner.GetVertexBasicRef(_corners[loop].Index);
                ref var vertex2 = ref this.Owner.GetVertexBasicRef(_corners[loop + 1].Index);
                ref var vertex3 = ref this.Owner.GetVertexBasicRef(_corners[loop + 2].Index);

                if (pickingRay.Intersects(ref vertex1.Position, ref vertex2.Position, ref vertex3.Position, out float currentDistance))
                {
                    result = true;
                    if (currentDistance < distance)
                    {
                        distance = currentDistance;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a plain polygon using the given coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates to build the polygon from.</param>
        public BuiltVerticesRange BuildPlainPolygon(Vector3[] coordinates)
        {
            // Build the polygon
            var polygon = new Polygon(coordinates);

            // Try to triangulate it
            var indices = polygon.TriangulateUsingCuttingEars();
            if (indices == null)
            {
                throw new SeeingSharpGraphicsException("Unable to triangulate given polygon!");
            }

            // Append all vertices
            var baseIndex = this.Owner.CountVertices;

            for (var loopCoordinates = 0; loopCoordinates < coordinates.Length; loopCoordinates++)
            {
                this.Owner.AddVertex(new VertexBasic(coordinates[loopCoordinates]));
            }

            // Append all indices
            using (var indexEnumerator = indices.GetEnumerator())
            {
                while (indexEnumerator.MoveNext())
                {
                    var index1 = indexEnumerator.Current;
                    var index2 = 0;
                    var index3 = 0;

                    if (indexEnumerator.MoveNext()) { index2 = indexEnumerator.Current; } else { break; }
                    if (indexEnumerator.MoveNext()) { index3 = indexEnumerator.Current; } else { break; }

                    this.AddTriangle(index1 + baseIndex, index2 + baseIndex, index3 + baseIndex);
                }
            }

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - coordinates.Length, coordinates.Length);
        }

        /// <summary>
        /// Build a Triangle.
        /// </summary>
        public BuiltVerticesRange BuildTriangle(Vector3 pointA, Vector3 pointB, Vector3 pointC)
        {
            var texX = 1f;
            var texY = 1f;

            if (_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / _tileSize.X;
                texY = (pointC - pointB).Length() / _tileSize.Y;
            }

            var vertex = new VertexBasic(pointA, new Vector2(0f, texY));

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));

            this.AddTriangleAndCalculateNormalsFlat(a, c, b);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, TextureCoordinateCalculationAlignment uCoordAlignment, TextureCoordinateCalculationAlignment vCoordAlignment, float coordRepeatUnit)
        {
            var startVertex = this.Owner.CountVertices;

            //Define texture coordinate calculation functions
            float CalculateU(Vector3 actPosition)
            {
                switch (uCoordAlignment)
                {
                    case TextureCoordinateCalculationAlignment.XAxis:
                        return actPosition.X / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.YAxis:
                        return actPosition.Y / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.ZAxis:
                        return actPosition.Z / coordRepeatUnit;
                }
                return 0f;
            }

            float CalculateV(Vector3 actPosition)
            {
                switch (vCoordAlignment)
                {
                    case TextureCoordinateCalculationAlignment.XAxis:
                        return actPosition.X / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.YAxis:
                        return actPosition.Y / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.ZAxis:
                        return actPosition.Z / coordRepeatUnit;
                }
                return 0f;
            }

            var textureCoordinate = new Vector2(CalculateU(pointA), CalculateV(pointA));
            var vertex = new VertexBasic(pointA, textureCoordinate, normal);

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(CalculateU(pointB), CalculateV(pointB))));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(CalculateU(pointC), CalculateV(pointC))));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(CalculateU(pointD), CalculateV(pointD))));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, startVertex, this.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing)
        /// </summary>
        public BuiltVerticesRange BuildRect(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
        {
            var texX = 1f;
            var texY = 1f;

            if (_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / _tileSize.X;
                texY = (pointC - pointB).Length() / _tileSize.Y;
            }

            var vertex = new VertexBasic(pointA, new Vector2(0f, texY));

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal)
        {
            var texX = 1f;
            var texY = 1f;

            if (_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / _tileSize.X;
                texY = (pointC - pointB).Length() / _tileSize.Y;
            }

            var vertex = new VertexBasic(pointA, new Vector2(0f, texY), normal);

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a single rectangle into the geometry (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, Vector2 minTexCoord, Vector2 maxTexCoord)
        {
            var vertex = new VertexBasic(pointA, new Vector2(minTexCoord.X, maxTexCoord.Y), normal);

            var a = this.Owner.AddVertex(vertex);
            var b = this.Owner.AddVertex(vertex.Copy(pointB, new Vector2(maxTexCoord.X, maxTexCoord.Y)));
            var c = this.Owner.AddVertex(vertex.Copy(pointC, new Vector2(maxTexCoord.X, minTexCoord.Y)));
            var d = this.Owner.AddVertex(vertex.Copy(pointD, new Vector2(minTexCoord.X, minTexCoord.Y)));

            this.AddTriangle(a, c, b);
            this.AddTriangle(a, d, c);

            return new BuiltVerticesRange(this.Owner, this.Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Changes the index order of each triangle.
        /// </summary>
        public void ToggleTriangleIndexOrder()
        {
            for (var loop = 2; loop < _corners.Count; loop += 3)
            {
                var edge1 = _corners[loop - 2];
                var edge2 = _corners[loop - 1];
                var edge3 = _corners[loop];
                _corners[loop] = edge1;
                _corners[loop - 1] = edge2;
                _corners[loop - 2] = edge3;
            }
        }
    }
}