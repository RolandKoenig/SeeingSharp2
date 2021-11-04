using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace SeeingSharp.Drawing3D.Primitives
{
    public static class PyramidBuilder
    {
        /// <summary>
        /// Create a 4 Side Pyramid
        /// </summary>
        public static BuiltVerticesRange BuildPyramid(this GeometrySurface target, float width, float height)
        {
            width = Math.Max(EngineMath.TOLERANCE_FLOAT_POSITIVE, width);
            height = Math.Max(EngineMath.TOLERANCE_FLOAT_POSITIVE, height);

            var halfWidth = width / 2f;
            var lowerMiddle = new Vector3(0f, -(height / 2f), 0f);
            var start = new Vector3(lowerMiddle.X - halfWidth, lowerMiddle.Y, lowerMiddle.Z - halfWidth);
            var dest = start + new Vector3(width, 0f, width);
            var centerTopCoordination = new Vector3((dest.X + start.X) / 2, start.Y + height, (dest.Z + start.Z) / 2);

            target.BuildRect(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(0f, -1f, 0f));

            target.BuildTriangle(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                centerTopCoordination);
            target.BuildTriangle(
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, start.Z),
                centerTopCoordination);
            target.BuildTriangle(
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                centerTopCoordination);
            target.BuildTriangle(
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                centerTopCoordination);

            return new BuiltVerticesRange(target.Owner, target.Owner.CountVertices - 4, 4);
        }
    }
}
