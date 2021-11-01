using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    // Code based on method AddTorus from
    // https://github.com/helix-toolkit/helix-toolkit/blob/develop/Source/HelixToolkit.Shared/Geometry/MeshBuilder.cs

    public static class TorusBuilder
    {
        /// <summary>
        /// Builds a torus.
        /// </summary>
        /// <param name="target">Target <see cref="GeometrySurface"/>.</param>
        /// <param name="torusDiameter">The diameter of the torus.</param>
        /// <param name="tubeDiameter">The diameter of the torus "tube".</param>
        /// <param name="tDiv">The number of subdivisions around the torus.</param>
        /// <param name="pDiv">The number of subdivisions of the torus' "tube.</param>
        public static BuiltVerticesRange BuildTorus(
            this GeometrySurface target,
            int tDiv, int pDiv, float torusDiameter, float tubeDiameter)
        {
            var startVertex = target.Owner.CountVertices;

            if (torusDiameter == 0.0)
            {
                // No Torus Diameter means we treat the Visual3D like a Sphere
                target.BuildShpere(tDiv, pDiv, tubeDiameter);
            }
            else if (tubeDiameter == 0.0)
            {
                // If the second Diameter is zero, we can't build out torus
                throw new SeeingSharpException("Torus must have a Diameter bigger than 0");
            }

            // Points of the Cross-Section of the torus "tube"
            IList<Vector2> crossSectionPoints;

            // Self-intersecting Torus, if the "Tube" Diameter is bigger than the Torus Diameter
            var selfIntersecting = tubeDiameter > torusDiameter;
            if (selfIntersecting)
            {
                // Angle-Calculations for Circle Segment https://de.wikipedia.org/wiki/Gleichschenkliges_Dreieck
                var angleIcoTriangle =
                    (float)Math.Acos(1 - ((torusDiameter * torusDiameter) / (2 * (tubeDiameter * tubeDiameter * .25))));
                var circleAngle = (float)Math.PI + angleIcoTriangle;
                var offset = -circleAngle / 2;

                // The Cross-Section is defined by only a Segment of a Circle
                crossSectionPoints = GetCircleSegment(pDiv, circleAngle, offset);
            }
            else
            {
                // "normal" Torus (with a Circle as Cross-Section of the Torus
                crossSectionPoints = GetCircle(pDiv, true);
            }

            // Transform crosssection to real Size
            crossSectionPoints = crossSectionPoints
                .Select(p => new Vector2(p.X * tubeDiameter * .5f, p.Y * tubeDiameter * .5f)).ToList();

            // Transform the Cross-Section Points to 3D Space
            var crossSection3DPoints = crossSectionPoints.Select(p => new Vector3(p.X, 0, p.Y)).ToList();

            // Add the needed Vertex-Positions of the Torus
            for (var i = 0; i < tDiv; i++)
            {
                // Angle of the current Cross-Section in the XY-Plane
                var angle = EngineMath.PI * 2 * ((float)i / tDiv);

                // Rotate the Cross-Section around the Origin by using the angle and the defined torusDiameter
                var rotatedPoints = crossSection3DPoints.Select(p3D =>
                    new Vector3((float)Math.Cos(angle) * (p3D.X + torusDiameter * .5f),
                        (float)Math.Sin(angle) * (p3D.X + torusDiameter * .5f), p3D.Z)).ToList();
                for (var j = 0; j < pDiv; j++)
                {
                    // If selfintersecting Torus, skip the first and last Point of the Cross-Sections, when not the first Cross Section.
                    // We only need the first and last Point of the first Cross-Section once!
                    if (selfIntersecting && i > 0 && (j == 0 || j == (pDiv - 1))) { continue; }

                    // Get position
                    var position = rotatedPoints[j];

                    // Calculate normal
                    var normal = Vector3.Zero;
                    switch (selfIntersecting)
                    {
                        case true when i == 0 && j == 0:
                            normal = new Vector3(0, 0, -1);
                            break;

                        case true when i == 0 && j == (pDiv - 1):
                            normal = new Vector3(0, 0, 1);
                            break;

                        default:
                            var rotatedOrigin =
                                new Vector3((float)Math.Cos(angle) * torusDiameter * .5f,
                                    (float)Math.Sin(angle) * torusDiameter * .5f, 0);
                            normal = Vector3.Normalize(position - rotatedOrigin);
                            break;
                    }

                    // Calculate texture coordinates
                    var texU = (float)i / tDiv;
                    float textV = 0;
                    if (i > 0 && selfIntersecting){ textV = (float)(j + 1) / pDiv; }
                    else { textV = (float)j / pDiv; }
                    var texCoord = new Vector2(texU, textV);

                    // Create and add vertex
                    var newVertex = new VertexBasic();
                    newVertex.Position = position;
                    newVertex.Normal = normal;
                    newVertex.TexCoord1 = texCoord;
                    target.Owner.AddVertex(newVertex);
                }
            }

            // Add Triangle-Indices
            for (var i = 0; i < tDiv; i++)
            {
                if (!selfIntersecting)
                {
                    // Normal non-selfintersecting Torus
                    // Just add Triangle-Strips between all neighboring Cross-Sections

                    var firstPointIdx = i * pDiv;
                    var firstPointIdxNextCircle = ((i + 1) % tDiv) * pDiv;
                    for (var j = 0; j < pDiv; j++)
                    {
                        var jNext = (j + 1) % pDiv;

                        target.AddTriangle(
                            firstPointIdx + j + startVertex,
                            firstPointIdx + jNext + startVertex,
                            firstPointIdxNextCircle + j + startVertex);
                        target.AddTriangle(
                            firstPointIdxNextCircle + j + startVertex,
                            firstPointIdx + jNext + startVertex,
                            firstPointIdxNextCircle + jNext + startVertex);
                    }
                }
                else
                {
                    // Selfintersecting Torus

                    // Add intermediate Triangles like for the non-selfintersecting Torus
                    // Skip the first and last Triangles, the "Caps" will be added later
                    // Determine the Index of the first Point of the first Cross-Section
                    var firstPointIdx = i * (pDiv - 2) + 1;
                    firstPointIdx += i > 0 ? 1 : 0;

                    // Determine the Index of the first Point of the next Cross-Section
                    var firstPointIdxNextCircle = pDiv + firstPointIdx - 1;
                    firstPointIdxNextCircle -= i > 0 ? 1 : 0;
                    if (firstPointIdxNextCircle >= target.Owner.CountVertices)
                    {
                        firstPointIdxNextCircle %= target.Owner.CountVertices;
                        firstPointIdxNextCircle++;
                    }

                    // Add Triangles between the "middle" Parts of the neighboring Cross-Sections
                    for (var j = 1; j < pDiv - 2; j++)
                    {
                        target.AddTriangle(
                            firstPointIdx + j - 1 + startVertex,
                            firstPointIdxNextCircle + j - 1 + startVertex,
                            firstPointIdx + j + startVertex);
                        target.AddTriangle(
                            firstPointIdxNextCircle + j - 1 + startVertex,
                            firstPointIdxNextCircle + j + startVertex,
                            firstPointIdx + j + startVertex);
                    }
                }
            }

            // For selfintersecting Tori
            if (selfIntersecting)
            {
                // Add bottom Cap by creating a List of Vertex-Indices
                // and using them to create a Triangle-Fan
                var vertexIndices = new List<int>(tDiv + 1);
                vertexIndices.Add(0);
                for (var i = 0; i < tDiv; i++)
                {
                    if (i == 0)
                    {
                        vertexIndices.Add(1 + startVertex);
                    }
                    else
                    {
                        vertexIndices.Add(pDiv + (i - 1) * (pDiv - 2) + startVertex);
                    }
                }
                vertexIndices.Add(1 + startVertex);
                vertexIndices.Reverse();
                target.AddTriangleFan(vertexIndices);

                // Add top Cap by creating a List of Vertex-Indices
                // and using them to create a Triangle-Fan
                vertexIndices = new List<int>(tDiv + 1);
                vertexIndices.Add(pDiv - 1 + startVertex);
                for (var i = 0; i < tDiv; i++)
                {
                    if (i == 0)
                    {
                        vertexIndices.Add(pDiv - 2 + startVertex);
                    }
                    else
                    {
                        vertexIndices.Add(pDiv + i * (pDiv - 2) - 1 + startVertex);
                    }
                }
                vertexIndices.Add(pDiv - 2 + startVertex);
                target.AddTriangleFan(vertexIndices);
            }

            return new BuiltVerticesRange(target.Owner, startVertex, target.Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Gets a circle section (cached).
        /// </summary>
        private static IList<Vector2> GetCircle(int thetaDiv, bool closed = false)
        {
            // Determine the angle steps
            var circle = new List<Vector2>(thetaDiv);
            var num = closed ? thetaDiv : thetaDiv - 1;
            for (var loop = 0; loop < thetaDiv; loop++)
            {
                var theta = EngineMath.PI * 2f * ((float)loop / num);
                circle.Add(new Vector2((float)Math.Cos(theta), -(float)Math.Sin(theta)));
            }

            return circle;
        }

        private static IList<Vector2> GetCircleSegment(int thetaDiv, float totalAngle = 2f * EngineMath.PI, float angleOffset = 0f)
        {
            var circleSegment = new List<Vector2>(thetaDiv);
            for (var loop = 0; loop < thetaDiv; loop++)
            {
                var theta = totalAngle * ((float)loop / (thetaDiv - 1)) + angleOffset;
                circleSegment.Add(new Vector2((float)Math.Cos(theta), (float)Math.Sin(theta)));
            }

            return circleSegment;
        }
    }
}
