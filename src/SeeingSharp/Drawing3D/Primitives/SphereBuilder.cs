using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;

namespace SeeingSharp.Drawing3D.Primitives
{
    public static class SphereBuilder
    {
        /// <summary>
        /// Builds a sphere geometry.
        /// </summary>
        public static BuiltVerticesRange BuildShpere(this GeometrySurface target, int tDiv, int pDiv, double radius)
        {
            tDiv = Math.Max(tDiv, 3);
            pDiv = Math.Max(pDiv, 2);
            radius = Math.Max(Math.Abs(radius), EngineMath.TOLERANCE_FLOAT_POSITIVE);

            Vector3 SphereGetPosition(double theta, double phi)
            {
                var x = radius * Math.Sin(theta) * Math.Sin(phi);
                var y = radius * Math.Cos(phi);
                var z = radius * Math.Cos(theta) * Math.Sin(phi);

                return new Vector3((float)x, (float)y, (float)z);
            }
            Vector2 SphereGetTextureCoordinate(double theta, double phi)
            {
                return new Vector2(
                    (float)(theta / (2 * Math.PI)),
                    (float)(phi / Math.PI));
            }

            var startVertex = target.Owner.CountVertices;
            var dt = Math.PI * 2 / tDiv;
            var dp = Math.PI / pDiv;

            for (var pi = 0; pi <= pDiv; pi++)
            {
                var phi = pi * dp;

                for (var ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    var theta = ti * dt;

                    var position = SphereGetPosition(theta, phi);
                    var vertex = new VertexBasic(
                        position,
                        SphereGetTextureCoordinate(theta, phi),
                        Vector3.Normalize(position));
                    target.Owner.Vertices.Add(vertex);
                }
            }

            for (var pi = 0; pi < pDiv; pi++)
            {
                for (var ti = 0; ti < tDiv; ti++)
                {
                    var x0 = ti;
                    var x1 = ti + 1;
                    var y0 = pi * (tDiv + 1);
                    var y1 = (pi + 1) * (tDiv + 1);

                    target.Triangles.Add(
                        x0 + y0,
                        x0 + y1,
                        x1 + y0);

                    target.Triangles.Add(
                        x1 + y0,
                        x0 + y1,
                        x1 + y1);
                }
            }

            return new BuiltVerticesRange(target.Owner, startVertex, target.Owner.CountVertices - startVertex);
        }
    }
}
