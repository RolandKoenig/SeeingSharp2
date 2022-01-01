using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Drawing3D.Primitives
{
    public static class CubeBuilder
    {
        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        public static BuiltVerticesRange BuildCube(this GeometrySurface target, float width, float height, float depth)
        {
            return BuildCube(
                target, 
                new Vector3(-(width / 2f), -(height / 2f), -(depth / 2f)),
                new Vector3(width, height, depth));
        }

        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        public static BuiltVerticesRange BuildCube(this GeometrySurface target, Vector3 size)
        {
            return BuildCube(
                target, 
                new Vector3(-(size.X / 2f), -(size.Y / 2f), -(size.Z / 2f)),
                new Vector3(size.X, size.Y, size.Z));
        }

        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        public static BuiltVerticesRange BuildCube(this GeometrySurface target, Vector3 start, Vector3 size)
        {
            var result = new BuiltVerticesRange(target.Owner);

            result.Merge(target.BuildCubeSides(start, size));
            result.Merge(target.BuildCubeTop(start, size));
            result.Merge(target.BuildCubeBottom(start, size));

            return result;
        }

        /// <summary>
        /// Builds a cube of 4 vertices and a defined height.
        /// </summary>
        public static BuiltVerticesRange BuildCube(this GeometrySurface target, Vector3 topA, Vector3 topB, Vector3 topC, Vector3 topD, float height)
        {
            var result = new BuiltVerticesRange(target.Owner)
            {
                StartVertex = target.Owner.CountVertices
            };

            var startTriangleIndex = target.CountTriangles;

            // Calculate texture coordinates
            var size = new Vector3(
                (topB - topA).Length(),
                Math.Abs(height),
                (topC - topB).Length());
            var texX = 1f;
            var texY = 1f;
            var texZ = 1f;

            if (target.IsTextureTileModeEnabled(out var tileSize))
            {
                texX = size.X / tileSize.X;
                texY = size.Y / tileSize.Y;
                texZ = size.Z / tileSize.X;
            }

            // Calculate bottom vectors
            var bottomA = new Vector3(topA.X, topA.Y - height, topA.Z);
            var bottomB = new Vector3(topB.X, topB.Y - height, topB.Z);
            var bottomC = new Vector3(topC.X, topC.Y - height, topC.Z);
            var bottomD = new Vector3(topD.X, topD.Y - height, topD.Z);

            // Build Top side
            var vertex = new VertexBasic(topA, new Vector2(texX, 0f), new Vector3(0f, 1f, 0f));
            var a = target.Owner.AddVertex(vertex);
            var b = target.Owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            var c = target.Owner.AddVertex(vertex.Copy(topC, new Vector2(0f, texY)));
            var d = target.Owner.AddVertex(vertex.Copy(topD, new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            // Build Bottom side
            vertex = new VertexBasic(topA, new Vector2(0f, 0f), new Vector3(0f, -1f, 0f));
            a = target.Owner.AddVertex(vertex);
            b = target.Owner.AddVertex(vertex.Copy(topD, new Vector2(texX, 0f)));
            c = target.Owner.AddVertex(vertex.Copy(topC, new Vector2(texX, texY)));
            d = target.Owner.AddVertex(vertex.Copy(topB, new Vector2(0f, texY)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            // Build Front side
            vertex = new VertexBasic(topA, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            a = target.Owner.AddVertex(vertex);
            b = target.Owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            c = target.Owner.AddVertex(vertex.Copy(bottomB, new Vector2(texX, 0f)));
            d = target.Owner.AddVertex(vertex.Copy(bottomA, new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            // Build Right side
            a = target.Owner.AddVertex(vertex.Copy(topB, new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = target.Owner.AddVertex(vertex.Copy(topC, new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = target.Owner.AddVertex(vertex.Copy(bottomC, new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = target.Owner.AddVertex(vertex.Copy(bottomB, new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            // Build Back side
            a = target.Owner.AddVertex(vertex.Copy(topC, new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = target.Owner.AddVertex(vertex.Copy(topD, new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = target.Owner.AddVertex(vertex.Copy(bottomD, new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = target.Owner.AddVertex(vertex.Copy(bottomC, new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            // Build Left side
            a = target.Owner.AddVertex(vertex.Copy(topD, new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = target.Owner.AddVertex(vertex.Copy(topA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = target.Owner.AddVertex(vertex.Copy(bottomA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = target.Owner.AddVertex(vertex.Copy(bottomD, new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            // Calculate normals finally
            target.CalculateNormalsFlat(startTriangleIndex, target.CountTriangles - startTriangleIndex);

            result.VertexCount = target.Owner.CountVertices - result.StartVertex;
            return result;
        }

        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="box">Box defining bounds of generated cube.</param>
        public static BuiltVerticesRange BuildCube(this GeometrySurface target, BoundingBox box)
        {
            return target.BuildCube(box.Minimum, box.GetSize());
        }

        /// <summary>
        /// Builds a cube on the given point with the given color.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="centerLocation">The location to draw the cube at.</param>
        /// <param name="sideLength">The side length of the cube.</param>
        public static BuiltVerticesRange BuildCube(this GeometrySurface target, Vector3 centerLocation, float sideLength)
        {
            return target.BuildCube(
                centerLocation - new Vector3(sideLength / 2f, sideLength / 2f, sideLength / 2f),
                new Vector3(sideLength, sideLength, sideLength));
        }

        /// <summary>
        /// Builds a cube into this Geometry (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="bottomCenter">Bottom center point of the cube.</param>
        /// <param name="width">Width (and depth) of the cube.</param>
        /// <param name="height">Height of the cube.</param>
        public static BuiltVerticesRange BuildCube(this GeometrySurface target, Vector3 bottomCenter, float width, float height)
        {
            var start = new Vector3(
                bottomCenter.X - width / 2f,
                bottomCenter.Y,
                bottomCenter.Z - width / 2f);
            var size = new Vector3(width, height, width);
            return target.BuildCube(start, size);
        }

        /// <summary>
        /// Builds the top side of a cube into this Geometry (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public static BuiltVerticesRange BuildCubeTop(this GeometrySurface target, Vector3 start, Vector3 size)
        {
            var dest = start + size;

            return target.BuildRect(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(0f, 1f, 0f));
        }

        /// <summary>
        /// Builds the bottom side of a cube into this Geometry (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public static BuiltVerticesRange BuildCubeBottom(this GeometrySurface target, Vector3 start, Vector3 size)
        {
            var dest = start + size;

            return target.BuildRect(
                new Vector3(start.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, start.Z),
                new Vector3(start.X, start.Y, start.Z),
                new Vector3(0f, -1f, 0f));
        }

        /// <summary>
        /// Builds cube sides into this Geometry (these sides are built up of  16 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        public static BuiltVerticesRange BuildCubeSides(this GeometrySurface target, Vector3 start, Vector3 size)
        {
            var result = new BuiltVerticesRange(target.Owner)
            {
                StartVertex = target.Owner.CountVertices
            };

            var dest = start + size;

            var texX = 1f;
            var texY = 1f;
            var texZ = 1f;
            if (target.IsTextureTileModeEnabled(out var tileSize))
            {
                texX = size.X / tileSize.X;
                texY = size.Y / tileSize.Y;
                texZ = size.Z / tileSize.X;
            }

            //Front side
            var vertex = new VertexBasic(start, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            var a = target.Owner.AddVertex(vertex);
            var b = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector2(texX, texY)));
            var c = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector2(texX, 0f)));
            var d = target.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            //Right side
            a = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            //Back side
            a = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = target.Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = target.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = target.Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            //Left side
            a = target.Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = target.Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = target.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = target.Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            target.AddTriangle(a, c, b);
            target.AddTriangle(a, d, c);

            result.VertexCount = target.Owner.CountVertices - result.StartVertex;
            return result;
        }
    }
}
