using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Drawing3D.Primitives
{
    public static class CylinderBuilder
    {
        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinder(this GeometrySurface target, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(
                new Vector3(0f, -(height / 2f), 0f), 
                radius, height, countOfSegments, true, true, true);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinder(this GeometrySurface target, Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(bottomMiddle, radius, height, countOfSegments, true, true, true);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinderTop(this GeometrySurface target, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(
                new Vector3(0f, -(height / 2f), 0f), 
                radius, height, countOfSegments, false, false, true);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinderTop(this GeometrySurface target, Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(bottomMiddle, radius, height, countOfSegments, false, false, true);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinderSides(this GeometrySurface target, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(
                new Vector3(0f, -(height / 2f), 0f), 
                radius, height, countOfSegments, true, false, false);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinderSides(this GeometrySurface target, Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(bottomMiddle, radius, height, countOfSegments, true, false, false);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinderBottom(this GeometrySurface target, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(
                new Vector3(0f, -(height / 2f), 0f), 
                radius, height, countOfSegments, false, true, false);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCylinderBottom(this GeometrySurface target, Vector3 bottomMiddle, float radius, float height, int countOfSegments)
        {
            return target.BuildCylinder(bottomMiddle, radius, height, countOfSegments, false, true, false);
        }

        /// <summary>
        /// Builds a cylinder into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="buildBottom">Build bottom of the cylinder.</param>
        /// <param name="buildSides">Build sides of the cylinder.</param>
        /// <param name="buildTop">Build top side of the cylinder.</param>
        public static BuiltVerticesRange BuildCylinder(
            this GeometrySurface target,
            Vector3 bottomMiddle, float radius, float height, int countOfSegments,
            bool buildSides, bool buildBottom, bool buildTop)
        {
            var startVertex = target.Owner.CountVertices;
            radius = Math.Max(EngineMath.TOLERANCE_FLOAT_POSITIVE, radius);
            height = Math.Max(EngineMath.TOLERANCE_FLOAT_POSITIVE, height);
            countOfSegments = Math.Max(3, countOfSegments);

            var diameter = radius * 2f;

            // Get texture offsets
            var texX = 1f;
            var texY = 1f;
            var texSegmentY = 1f;
            var texSegmentX = 1f;
            if (target.IsTextureTileModeEnabled(out var tileSize))
            {
                texX = diameter / tileSize.X;
                texY = diameter / tileSize.Y;
                texSegmentY = height / tileSize.Y;
                texSegmentX = EngineMath.RAD_180DEG * diameter / tileSize.X;
            }

            // Specify bottom and top middle coordinates
            var bottomCoordinate = bottomMiddle;
            var topCoordinate = new Vector3(bottomMiddle.X, bottomMiddle.Y + height, bottomMiddle.Z);

            // Create bottom and top vertices
            var bottomVertex = new VertexBasic(bottomCoordinate, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));
            var topVertex = new VertexBasic(topCoordinate, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, 1f, 0f));

            // AddObject bottom and top vertices to the geometry
            var bottomVertexIndex = target.Owner.AddVertex(bottomVertex);
            var topVertexIndex = target.Owner.AddVertex(topVertex);

            // Generate all segments
            var fullRadian = EngineMath.RAD_360DEG;
            var countOfSegmentsF = (float)countOfSegments;

            for (var loop = 0; loop < countOfSegments; loop++)
            {
                // Calculate rotation values for each segment border
                var startRadian = fullRadian * (loop / countOfSegmentsF);
                var targetRadian = fullRadian * ((loop + 1) / countOfSegmentsF);
                var normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                // Generate all normals
                var sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                var sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                var sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //
                var sideLeftTexCoord = new Vector2(0.5f + sideLeftNormal.X * radius, 0.5f + sideLeftNormal.Z * radius);
                var sideRightTexCoord = new Vector2(0.5f + sideRightNormal.X * radius, 0.5f + sideRightNormal.Z * radius);

                // Generate all points
                var sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                var sideRightBottomCoord = bottomCoordinate + sideRightNormal * radius;
                var sideLeftTopCoord = new Vector3(sideLeftBottomCoord.X, sideLeftBottomCoord.Y + height, sideLeftBottomCoord.Z);
                var sideRightTopCoord = new Vector3(sideRightBottomCoord.X, sideRightBottomCoord.Y + height, sideRightBottomCoord.Z);

                // AddObject segment bottom triangle
                if (buildBottom)
                {
                    var segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord, sideLeftTexCoord);
                    var segmentBottomRight = bottomVertex.Copy(sideRightBottomCoord, sideRightTexCoord);
                    target.AddTriangle(
                        bottomVertexIndex, target.Owner.AddVertex(segmentBottomLeft), target.Owner.AddVertex(segmentBottomRight));
                }

                // AddObject segment top triangle
                if (buildTop)
                {
                    var segmentTopLeft = topVertex.Copy(sideLeftTopCoord, sideLeftTexCoord);
                    var segmentTopRight = topVertex.Copy(sideRightTopCoord, sideRightTexCoord);
                    target.AddTriangle(
                        topVertexIndex, target.Owner.AddVertex(segmentTopRight), target.Owner.AddVertex(segmentTopLeft));
                }

                if (buildSides)
                {
                    // Calculate texture coords for side segment
                    var texCoordSegmentStart = new Vector2(texSegmentX * (loop / (float)countOfSegments), 0f);
                    var texCoordSegmentTarget = new Vector2(texSegmentX * ((loop + 1) / (float)countOfSegments), texSegmentY);

                    // AddObject segment side
                    target.BuildRect(sideLeftBottomCoord, sideRightBottomCoord, sideRightTopCoord, sideLeftTopCoord, sideNormal, texCoordSegmentStart, texCoordSegmentTarget);
                }
            }

            return new BuiltVerticesRange(target.Owner, startVertex, target.Owner.CountVertices - startVertex);
        }
    }
}
