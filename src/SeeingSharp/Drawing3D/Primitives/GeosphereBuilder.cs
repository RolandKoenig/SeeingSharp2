using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Drawing3D.Primitives
{
    public static class GeosphereBuilder
    {
        /// <summary>
        /// Builds a geosphere geometry.
        /// </summary>
        public static BuiltVerticesRange BuildGeosphere(this GeometrySurface target, float radius, int countSubdivisions)
        {
            // Implemented with sample code from http://www.d3dcoder.net/d3d11.htm, Source Code Set II

            countSubdivisions = Math.Max(countSubdivisions, 0);
            radius = Math.Max(Math.Abs(radius), EngineMath.TOLERANCE_FLOAT_POSITIVE); // <-- this one prevents device by zero

            var startVertex = target.Owner.CountVertices;
            var startTriangle = target.Owner.CountTriangles;

            // Build an icosahedron
            const float X = 0.525731f;
            const float Z = 0.850651f;
            var pos = new[]
            {
                new Vector3(-X, 0f, Z),    new Vector3(X, 0f, Z), 
                new Vector3(-X, 0f, -Z), new Vector3(X, 0f, -Z),
                new Vector3(0f, Z, X),   new Vector3(0f, Z, -X),
                new Vector3(0f, -Z, X),  new Vector3(0f, -Z, -X),
                new Vector3(Z, X, 0f),   new Vector3(-Z, X, 0f),
                new Vector3(Z, -X, 0f),  new Vector3(-Z, -X, 0f)  
            };
            var k = new[]
            {
                1, 4, 0,   4, 9, 0,   4, 5, 9,   8, 5, 4,   1, 8, 4,
                1, 10, 8,  10, 3, 8,  8, 3, 5,   3, 2, 5,   3, 7, 2,
                3, 10, 7,  10, 6, 7,  6, 11, 7,  6, 0, 11,  6, 1, 0,
                10, 1, 6,  11, 0, 9,  2, 11, 9,  5, 2, 9,   11, 2, 7
            };
            foreach (var actPosition in pos)
            {
                target.Owner.AddVertex(new VertexBasic(actPosition));
            }
            for (var loop = 0; loop < k.Length; loop += 3)
            {
                target.AddTriangle(k[loop], k[loop +1], k[loop + 2]);
            }

            // Subdivide it n times
            for (var loop = 0; loop < countSubdivisions; loop++)
            {
                target.Subdivide(startTriangle);
            }

            // Project vertices onto sphere and scale
            var vertexCount = target.Owner.CountVertices;
            for (var actVertexIndex = startVertex; actVertexIndex < vertexCount; actVertexIndex++)
            {
                ref var actVertex = ref target.Owner.GetVertexBasicRef(actVertexIndex);
                actVertex.Normal = Vector3.Normalize(actVertex.Position);
                actVertex.Position = actVertex.Normal * radius;

                var theta = EngineMath.AngleFromXY(actVertex.Position.X, actVertex.Position.Z);
                var phi = (float)Math.Acos(actVertex.Position.Y / radius);
                actVertex.TexCoord1 = new Vector2(
                    theta / EngineMath.PI_2,
                    phi / EngineMath.PI);
            }

            return new BuiltVerticesRange(target.Owner, startVertex, target.Owner.CountVertices - startVertex);
        }
    }
}
