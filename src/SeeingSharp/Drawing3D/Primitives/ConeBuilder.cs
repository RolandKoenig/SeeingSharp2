using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Drawing3D.Primitives
{
    public static class ConeBuilder
    {
        /// <summary>
        /// Builds a cone into the geometry with correct texture coordinates and normals.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="radius">The radius of the cone.</param>
        /// <param name="height">The height of the cone.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        public static BuiltVerticesRange BuildCone(this GeometrySurface target, float radius, float height, int countOfSegments)
        {
            var startVertex = target.Owner.CountVertices;
            radius = Math.Max(EngineMath.TOLERANCE_FLOAT_POSITIVE, radius);
            height = Math.Max(EngineMath.TOLERANCE_FLOAT_POSITIVE, height);
            countOfSegments = Math.Max(3, countOfSegments);

            var diameter = radius * 2f;

            // Get texture offsets
            var texX = 1f;
            var texY = 1f;
            if (target.IsTextureTileModeEnabled(out var tileSize))
            {
                texX = diameter / tileSize.X;
                texY = diameter / tileSize.Y;
            }

            // Specify bottom and top middle coordinates
            var bottomCoordinate = new Vector3(0f, -(height / 2f), 0f);
            var topCoordinate = new Vector3(bottomCoordinate.X, bottomCoordinate.Y + height, bottomCoordinate.Z);

            // Create bottom and top vertices
            var bottomVertex = new VertexBasic(bottomCoordinate, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));

            // AddObject bottom and top vertices to the geometry
            var bottomVertexIndex = target.Owner.AddVertex(bottomVertex);

            // Generate all segments
            var countOfSegmentsF = (float)countOfSegments;
            for (var loop = 0; loop < countOfSegments; loop++)
            {
                // Calculate rotation values for each segment border
                var startRadian = EngineMath.RAD_360DEG * (loop / countOfSegmentsF);
                var targetRadian = EngineMath.RAD_360DEG * ((loop + 1) / countOfSegmentsF);
                var normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                // Generate all normals
                var sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                var sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                var sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //Generate all points
                var sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                var sideRightBottomCoord = bottomCoordinate + sideRightNormal * radius;
                var sideMiddleBottomCoord = bottomCoordinate + sideNormal * radius;
                var sideLeftTexCoord = new Vector2(
                    texX / (diameter / (sideLeftBottomCoord.X + radius)),
                    texY / (diameter / (sideLeftBottomCoord.Z + radius)));
                var sideRightTexCoord = new Vector2(
                    texX / (diameter / (sideRightBottomCoord.X + radius)),
                    texY / (diameter / (sideRightBottomCoord.Z + radius)));

                //AddObject segment bottom triangle
                var segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord, sideLeftTexCoord);
                var segmentBottomRight = bottomVertex.Copy(sideRightBottomCoord, sideRightTexCoord);
                target.AddTriangle(
                    bottomVertexIndex, target.Owner.AddVertex(segmentBottomLeft), target.Owner.AddVertex(segmentBottomRight));

                //Generate side normal
                var vectorToTop = topCoordinate - sideMiddleBottomCoord;
                var vectorToTopRotation = Vector3Ex.ToHVRotation(vectorToTop);
                vectorToTopRotation.Y = vectorToTopRotation.Y + EngineMath.RAD_90DEG;
                var topSideNormal = Vector3Ex.NormalFromHVRotation(vectorToTopRotation);

                //AddObject segment top triangle
                var topVertex = new VertexBasic(topCoordinate, new Vector2(texX / 2f, texY / 2f), topSideNormal);
                var segmentTopLeft = topVertex.Copy(sideLeftBottomCoord, sideLeftTexCoord);
                var segmentTopRight = topVertex.Copy(sideRightBottomCoord, sideRightTexCoord);

                target.AddTriangle(target.Owner.AddVertex(topVertex), target.Owner.AddVertex(segmentTopRight), target.Owner.AddVertex(segmentTopLeft));
            }

            return new BuiltVerticesRange(target.Owner, startVertex, target.Owner.CountVertices - startVertex);
        }
    }
}
