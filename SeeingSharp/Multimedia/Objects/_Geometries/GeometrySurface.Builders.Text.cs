/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
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

using System;
using SeeingSharp.Multimedia.Core;
using SharpDX;
using DWrite = SharpDX.DirectWrite;

namespace SeeingSharp.Multimedia.Objects
{
    public partial class GeometrySurface
    {
        /// <summary>
        /// Builds the text geometry for the given string.
        /// </summary>
        /// <param name="stringToBuild">The string to build within the geometry.</param>
        public void BuildTextGeometry(string stringToBuild)
        {
            BuildTextGeometry(stringToBuild, TextGeometryOptions.Default);
        }

        /// <summary>
        /// Builds the text geometry for the given string.
        /// </summary>
        /// <param name="stringToBuild">The string to build within the geometry.</param>
        /// <param name="geometryOptions">Some configuration for geometry creation.</param>
        public void BuildTextGeometry(string stringToBuild, TextGeometryOptions geometryOptions)
        {
            var writeFactory = GraphicsCore.Current.FactoryDWrite;

            //TODO: Cache font objects

            // Get font properties
            var fontWeight = geometryOptions.FontWeight;
            var fontStyle = geometryOptions.FontStyle;

            // Create the text layout object
            try
            {
                var textLayout = new DWrite.TextLayout(
                    writeFactory, stringToBuild,
                    new DWrite.TextFormat(
                        writeFactory, geometryOptions.FontFamily, fontWeight, fontStyle, geometryOptions.FontSize),
                        float.MaxValue, float.MaxValue, 1f, true);

                // Render the text using the geometry text renderer
                using (var textRenderer = new GeometryTextRenderer(this, geometryOptions))
                {
                    textLayout.Draw(textRenderer, 0f, 0f);
                }
            }
            catch (Exception ex)
            {
                GraphicsCore.PublishInternalExceptionInfo(ex, InternalExceptionLocation.CreateTextGeometry);
            }
        }

        /// <summary>
        /// Builds a plain polygon using the given coordinates.
        /// </summary>
        /// <param name="coordinates">The coordinates to build the polygon from.</param>
        public void BuildPlainPolygon(Vector3[] coordinates)
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
            var baseIndex = Owner.CountVertices;

            for (var loopCoordinates = 0; loopCoordinates < coordinates.Length; loopCoordinates++)
            {
                Owner.AddVertex(new Vertex(coordinates[loopCoordinates]));
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

                    AddTriangle(index1 + baseIndex, index2 + baseIndex, index3 + baseIndex);
                }
            }
        }
    }
}