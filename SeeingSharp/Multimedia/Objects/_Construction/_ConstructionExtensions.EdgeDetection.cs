#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion
namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using System.Collections.Generic;
    using SharpDX;

    #endregion

    public static partial class ConstructionExtensions
    {
        /// <summary>
        /// Generates a shadow volume structure out of the given vertex structures.
        /// </summary>
        public static VertexStructure GenerateShadowVolume(this IEnumerable<VertexStructure> structures, Vector3 lightDirection, float shadowVolumeLength)
        {
            var result = new VertexStructure();

            //Find all edges from given view direction
            List<Line> shadowVolumeEdges = GenerateEdgesSeenFromViewpoint(structures, lightDirection);

            //Build the structure based on the found edges
            var lightNormal = Vector3.Normalize(lightDirection);
            var actSurface = result.FirstSurface;

            foreach (var actEdge in shadowVolumeEdges)
            {
                var targetEdge = new Line(
                    actEdge.StartPosition + lightNormal * shadowVolumeLength,
                    actEdge.EndPosition + lightNormal * shadowVolumeLength);

                actSurface.AddTriangle(
                    new Vertex(actEdge.StartPosition, Color4.White),
                    new Vertex(actEdge.EndPosition, Color4.White),
                    new Vertex(targetEdge.EndPosition, Color4.White));
                actSurface.AddTriangle(
                    new Vertex(targetEdge.EndPosition, Color4.White),
                    new Vertex(targetEdge.StartPosition, Color4.White),
                    new Vertex(actEdge.StartPosition, Color4.White));
            }

            return result;
        }

        /// <summary>
        /// Searchs all edges seen from the given direction.
        /// </summary>
        /// <param name="structures">The structures to search edges for.</param>
        /// <param name="viewDirection">The view direction.</param>
        public static List<Line> GenerateEdgesSeenFromViewpoint(this IEnumerable<VertexStructure> structures, Vector3 viewDirection)
        {
            //Find all shadow volume edges
            List<Line> foundEndges = new List<Line>(2048);
            List<Line> edgesToRemove = new List<Line>(2048);

            foreach (var actStructure in structures)
            {
                foreach (var actSurface in actStructure.Surfaces)
                {
                    foreach (var actTriangle in actSurface.Triangles)
                    {
                        if (Vector3.Dot(viewDirection, actStructure.Vertices[actTriangle.Index1].Normal) >= 0)
                        {
                            Line[] actEdges = actTriangle.GetEdges(actStructure);

                            for (var loopEdge = 0; loopEdge < actEdges.Length; loopEdge++)
                            {
                                var actEdge = actEdges[loopEdge];

                                //Was this edge already removed?
                                var alreadyRemoved = false;

                                foreach (var edgesRemoved in edgesToRemove)
                                {
                                    if (edgesRemoved.EqualsWithTolerance(actEdge))
                                    {
                                        alreadyRemoved = true;
                                        break;
                                    }
                                }

                                if (alreadyRemoved)
                                {
                                    continue;
                                }

                                //Was this edge already added?
                                var alreadyAdded = false;

                                for (var loopShadowEdge = 0; loopShadowEdge < foundEndges.Count; loopShadowEdge++)
                                {
                                    if (foundEndges[loopShadowEdge].EqualsWithTolerance(actEdge))
                                    {
                                        //Remove the edge because it can't be member of the contour when it is found twice
                                        alreadyAdded = true;
                                        foundEndges.RemoveAt(loopShadowEdge);
                                        edgesToRemove.Add(actEdge);
                                        break;
                                    }
                                }
                                if (alreadyAdded) { continue; }

                                //Add the edge to the result list finally
                                foundEndges.Add(actEdge);
                            }
                        }
                    }
                }
            }

            return foundEndges;
        }
    }
}
