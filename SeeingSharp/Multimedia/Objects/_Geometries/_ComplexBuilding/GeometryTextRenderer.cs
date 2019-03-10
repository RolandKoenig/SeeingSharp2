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

using System.Collections.Generic;
using System.Linq;
using SeeingSharp.Multimedia.Core;
using SharpDX.Direct2D1;
using SharpDX.DirectWrite;
using SDX = SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    internal class GeometryTextRenderer : TextRendererBase
    {
        private TextGeometryOptions m_geometryOptions;
        private GeometrySurface m_targetSurface;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextRenderer" /> class.
        /// </summary>
        public GeometryTextRenderer(GeometrySurface targetSurface, TextGeometryOptions textGeometryOptions)
        {
            m_targetSurface = targetSurface;
            m_geometryOptions = textGeometryOptions;
        }

        /// <summary>
        /// IDWriteTextLayout::Draw calls this function to instruct the client to render a run of glyphs.
        /// </summary>
        /// <param name="clientDrawingContext">The application-defined drawing context passed to  <see cref="M:SharpDX.DirectWrite.TextLayout.Draw_(System.IntPtr,System.IntPtr,System.Single,System.Single)" />.</param>
        /// <param name="baselineOriginX">The pixel location (X-coordinate) at the baseline origin of the glyph run.</param>
        /// <param name="baselineOriginY">The pixel location (Y-coordinate) at the baseline origin of the glyph run.</param>
        /// <param name="measuringMode">The measuring method for glyphs in the run, used with the other properties to determine the rendering mode.</param>
        /// <param name="glyphRun">Pointer to the glyph run instance to render.</param>
        /// <param name="glyphRunDescription">A pointer to the optional glyph run description instance which contains properties of the characters  associated with this run.</param>
        /// <param name="clientDrawingEffect">Application-defined drawing effects for the glyphs to render. Usually this argument represents effects such as the foreground brush filling the interior of text.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.
        /// </returns>
        /// <unmanaged>HRESULT DrawGlyphRun([None] void* clientDrawingContext,[None] FLOAT baselineOriginX,[None] FLOAT baselineOriginY,[None] DWRITE_MEASURING_MODE measuringMode,[In] const DWRITE_GLYPH_RUN* glyphRun,[In] const DWRITE_GLYPH_RUN_DESCRIPTION* glyphRunDescription,[None] IUnknown* clientDrawingEffect)</unmanaged>
        /// <remarks>
        /// The <see cref="M:SharpDX.DirectWrite.TextLayout.Draw_(System.IntPtr,System.IntPtr,System.Single,System.Single)" /> function calls this callback function with all the information about glyphs to render. The application implements this callback by mostly delegating the call to the underlying platform's graphics API such as {{Direct2D}} to draw glyphs on the drawing context. An application that uses GDI can implement this callback in terms of the <see cref="M:SharpDX.DirectWrite.BitmapRenderTarget.DrawGlyphRun(System.Single,System.Single,SharpDX.Direct2D1.MeasuringMode,SharpDX.DirectWrite.GlyphRun,SharpDX.DirectWrite.RenderingParams,SharpDX.Color4)" /> method.
        /// </remarks>
        public override SDX.Result DrawGlyphRun(
            object clientDrawingContext, float baselineOriginX, float baselineOriginY,
            MeasuringMode measuringMode, GlyphRun glyphRun, GlyphRunDescription glyphRunDescription, SDX.ComObject clientDrawingEffect)
        {
            if (glyphRun.Indices == null ||
                glyphRun.Indices.Length == 0)
            {
                return SDX.Result.Ok;
            }

            var dWriteFactory = GraphicsCore.Current.FactoryDWrite;
            var d2DFactory = GraphicsCore.Current.FactoryD2D;

            // Extrude geometry data out of given glyph run
            var geometryExtruder = new SimplePolygon2DGeometrySink(new SDX.Vector2(baselineOriginX, baselineOriginY));

            using (var pathGeometry = new PathGeometry(d2DFactory))
            {
                // Write all geometry data into a standard PathGeometry object
                using (var geoSink = pathGeometry.Open())
                {
                    glyphRun.FontFace.GetGlyphRunOutline(
                        glyphRun.FontSize,
                        glyphRun.Indices,
                        glyphRun.Advances,
                        glyphRun.Offsets,
                        glyphRun.Offsets?.Length ?? 0,
                        glyphRun.IsSideways,
                        glyphRun.BidiLevel % 2 == 1,
                        geoSink);
                    geoSink.Close();
                }

                // Simplify written geometry and write it into own structure
                pathGeometry.Simplify(GeometrySimplificationOption.Lines, m_geometryOptions.SimplificationFlatternTolerance, geometryExtruder);
            }

            // Structure for caching the result
            var tempStructure = new Geometry();
            var tempSurface = tempStructure.CreateSurface();

            // Create the text surface
            if (m_geometryOptions.MakeSurface)
            {
                // Separate polygons by clock direction
                // Order polygons as needed for further hole finding algorithm
                IEnumerable<Polygon2D> fillingPolygons = geometryExtruder.GeneratedPolygons
                    .Where(actPolygon => actPolygon.EdgeOrder == EdgeOrder.CounterClockwise)
                    .OrderBy(actPolygon => actPolygon.BoundingBox.Size.X * actPolygon.BoundingBox.Size.Y);
                var holePolygons = geometryExtruder.GeneratedPolygons
                    .Where(actPolygon => actPolygon.EdgeOrder == EdgeOrder.Clockwise)
                    .OrderByDescending(actPolygon => actPolygon.BoundingBox.Size.X * actPolygon.BoundingBox.Size.Y)
                    .ToList();

                // Build geometry for all polygons
                foreach (var actFillingPolygon in fillingPolygons)
                {
                    // Find all corresponding holes
                    var actFillingPolygonBounds = actFillingPolygon.BoundingBox;
                    IEnumerable<Polygon2D> correspondingHoles = holePolygons
                        .Where(actHolePolygon => actHolePolygon.BoundingBox.IsContainedBy(actFillingPolygonBounds))
                        .ToList();

                    // Two steps here:
                    // - Merge current filling polygon and all its holes.
                    // - Remove found holes from current hole list
                    var polygonForRendering = actFillingPolygon;
                    var polygonForTriangulation = actFillingPolygon.Clone();
                    var cutPoints = new List<SDX.Vector2>();

                    foreach (var actHole in correspondingHoles)
                    {
                        holePolygons.Remove(actHole);
                        polygonForRendering = polygonForRendering.MergeWithHole(actHole, Polygon2DMergeOptions.Default, cutPoints);
                        polygonForTriangulation = polygonForTriangulation.MergeWithHole(actHole, new Polygon2DMergeOptions { MakeMergepointSpaceForTriangulation = true });
                    }

                    var actBaseIndex = tempStructure.CountVertices;
                    var edgeOrder = polygonForRendering.EdgeOrder;
                    var edgeSize = edgeOrder == EdgeOrder.CounterClockwise ? 0.1f : 0.4f;

                    // Append all vertices to temporary Geometry
                    for (var loop = 0; loop < polygonForRendering.Vertices.Count; loop++)
                    {
                        // Calculate 3d location and texture coordinate
                        var actVertexLocation = new SDX.Vector3(
                            polygonForRendering.Vertices[loop].X,
                            0f,
                            polygonForRendering.Vertices[loop].Y);
                        var actTexCoord = new SDX.Vector2(
                            (polygonForRendering.Vertices[loop].X - polygonForRendering.BoundingBox.Location.X) / polygonForRendering.BoundingBox.Size.X,
                            (polygonForRendering.Vertices[loop].Y - polygonForRendering.BoundingBox.Location.Y) / polygonForRendering.BoundingBox.Size.Y);

                        if (float.IsInfinity(actTexCoord.X) || float.IsNaN(actTexCoord.X))
                        {
                            actTexCoord.X = 0f;
                        }

                        if (float.IsInfinity(actTexCoord.Y) || float.IsNaN(actTexCoord.Y))
                        {
                            actTexCoord.Y = 0f;
                        }

                        // Append the vertex to the result
                        tempStructure.AddVertex(
                            new Vertex(
                                actVertexLocation,
                                m_geometryOptions.SurfaceVertexColor,
                                actTexCoord,
                                new SDX.Vector3(0f, 1f, 0f)));
                    }

                    // Generate cubes on each vertex if requested
                    if (m_geometryOptions.GenerateCubesOnVertices)
                    {
                        for (var loop = 0; loop < polygonForRendering.Vertices.Count; loop++)
                        {
                            var colorToUse = Color4Ex.GreenColor;
                            var pointRenderSize = 0.1f;

                            if (cutPoints.Contains(polygonForRendering.Vertices[loop]))
                            {
                                colorToUse = Color4Ex.RedColor;
                                pointRenderSize = 0.15f;
                            }

                            var actVertexLocation = new SDX.Vector3(
                                polygonForRendering.Vertices[loop].X,
                                0f,
                                polygonForRendering.Vertices[loop].Y);
                            tempSurface.BuildCube24V(actVertexLocation, pointRenderSize, colorToUse);
                        }
                    }

                    // Triangulate the polygon
                    var triangleIndices = polygonForTriangulation.TriangulateUsingCuttingEars();
                    if (triangleIndices == null) { continue; }

                    // Append all triangles to the temporary structure
                    using (var indexEnumerator = triangleIndices.GetEnumerator())
                    {
                        while (indexEnumerator.MoveNext())
                        {
                            var index1 = indexEnumerator.Current;
                            var index2 = 0;
                            var index3 = 0;

                            if (indexEnumerator.MoveNext()) { index2 = indexEnumerator.Current; } else { break; }
                            if (indexEnumerator.MoveNext()) { index3 = indexEnumerator.Current; } else { break; }

                            tempSurface.AddTriangle(
                                actBaseIndex + index3,
                                actBaseIndex + index2,
                                actBaseIndex + index1);
                        }
                    }
                }
            }

            // Make volumetric outlines
            var triangleCountWithoutSide = tempSurface.CountTriangles;

            if (m_geometryOptions.MakeVolumetricText)
            {
                var volumetricTextDepth = m_geometryOptions.VolumetricTextDepth;

                if(m_geometryOptions.VerticesScaleFactor > 0f)
                {
                    volumetricTextDepth = volumetricTextDepth / m_geometryOptions.VerticesScaleFactor;
                }

                // Add all side surfaces
                foreach (var actPolygon in geometryExtruder.GeneratedPolygons)
                {
                    foreach (var actLine in actPolygon.Lines)
                    {
                        tempSurface.BuildRect4V(
                            new SDX.Vector3(actLine.StartPosition.X, -volumetricTextDepth, actLine.StartPosition.Y),
                            new SDX.Vector3(actLine.EndPosition.X, -volumetricTextDepth, actLine.EndPosition.Y),
                            new SDX.Vector3(actLine.EndPosition.X, 0f, actLine.EndPosition.Y),
                            new SDX.Vector3(actLine.StartPosition.X, 0f, actLine.StartPosition.Y),
                            m_geometryOptions.VolumetricSideSurfaceVertexColor);
                    }
                }
            }

            // Do also make back surface?
            if (m_geometryOptions.MakeBackSurface)
            {
                for (var loop = 0; loop < triangleCountWithoutSide; loop++)
                {
                    var triangle = tempSurface.Triangles[loop];
                    var vertex0 = tempStructure.Vertices[triangle.Index1];
                    var vertex1 = tempStructure.Vertices[triangle.Index2];
                    var vertex2 = tempStructure.Vertices[triangle.Index3];
                    var changeVector = new SDX.Vector3(0f, -m_geometryOptions.VolumetricTextDepth, 0f);

                    tempSurface.AddTriangle(
                        vertex2.Copy(vertex2.Position - changeVector, SDX.Vector3.Negate(vertex2.Normal)),
                        vertex1.Copy(vertex1.Position - changeVector, SDX.Vector3.Negate(vertex1.Normal)),
                        vertex0.Copy(vertex0.Position - changeVector, SDX.Vector3.Negate(vertex0.Normal)));
                }
            }

            // TODO: Make this configurable
            tempStructure.ToggleCoordinateSystem();

            // Scale the text using given scale factor
            if (m_geometryOptions.VerticesScaleFactor > 0f)
            {
                var scaleMatrix = SDX.Matrix.Scaling(
                    m_geometryOptions.VerticesScaleFactor,
                    m_geometryOptions.VerticesScaleFactor,
                    m_geometryOptions.VerticesScaleFactor);

                var transformMatrix = new Matrix4Stack(scaleMatrix);
                transformMatrix.TransformLocal(m_geometryOptions.VertexTransform);

                tempStructure.UpdateVerticesUsingRelocationFunc(actVector => SDX.Vector3.Transform(actVector, transformMatrix.Top).ToXYZ());
            }

            // Calculate all normals before adding to target structure
            if (m_geometryOptions.CalculateNormals)
            {
                tempStructure.CalculateNormalsFlat();
            }

            // Merge temporary structure to target structure
            m_targetSurface.AddStructure(tempStructure);

            return SDX.Result.Ok;
        }

        /// <summary>
        /// IDWriteTextLayout::Draw calls this application callback when it needs to draw an inline object.
        /// </summary>
        /// <param name="clientDrawingContext">The application-defined drawing context passed to IDWriteTextLayout::Draw.</param>
        /// <param name="originX">X-coordinate at the top-left corner of the inline object.</param>
        /// <param name="originY">Y-coordinate at the top-left corner of the inline object.</param>
        /// <param name="inlineObject">The application-defined inline object set using IDWriteTextFormat::SetInlineObject.</param>
        /// <param name="isSideways">A Boolean flag that indicates whether the object's baseline runs alongside the baseline axis of the line.</param>
        /// <param name="isRightToLeft">A Boolean flag that indicates whether the object is in a right-to-left context, hinting that the drawing may want to mirror the normal image.</param>
        /// <param name="clientDrawingEffect">Application-defined drawing effects for the glyphs to render. Usually this argument represents effects such as the foreground brush filling the interior of a line.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.
        /// </returns>
        /// <unmanaged>HRESULT DrawInlineObject([None] void* clientDrawingContext,[None] FLOAT originX,[None] FLOAT originY,[None] IDWriteInlineObject* inlineObject,[None] BOOL isSideways,[None] BOOL isRightToLeft,[None] IUnknown* clientDrawingEffect)</unmanaged>
        public override SDX.Result DrawInlineObject(object clientDrawingContext, float originX, float originY, InlineObject inlineObject, bool isSideways, bool isRightToLeft, SDX.ComObject clientDrawingEffect)
        {
            return SDX.Result.Ok;
        }

        /// <summary>
        /// IDWriteTextLayout::Draw calls this function to instruct the client to draw a strikethrough.
        /// </summary>
        /// <param name="clientDrawingContext">The application-defined drawing context passed to  IDWriteTextLayout::Draw.</param>
        /// <param name="baselineOriginX">The pixel location (X-coordinate) at the baseline origin of the run where strikethrough applies.</param>
        /// <param name="baselineOriginY">The pixel location (Y-coordinate) at the baseline origin of the run where strikethrough applies.</param>
        /// <param name="strikethrough">Pointer to  a structure containing strikethrough logical information.</param>
        /// <param name="clientDrawingEffect">Application-defined effect to apply to the strikethrough.  Usually this argument represents effects such as the foreground brush filling the interior of a line.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.
        /// </returns>
        /// <unmanaged>HRESULT DrawStrikethrough([None] void* clientDrawingContext,[None] FLOAT baselineOriginX,[None] FLOAT baselineOriginY,[In] const DWRITE_STRIKETHROUGH* strikethrough,[None] IUnknown* clientDrawingEffect)</unmanaged>
        /// <remarks>
        /// A single strikethrough can be broken into multiple calls, depending on how the formatting changes attributes. Strikethrough is not averaged across font sizes/styles changes. To get an appropriate starting pixel position, add strikethrough::offset to the baseline. Like underlines, the x coordinate will always be passed as the left side, regardless of text directionality.
        /// </remarks>
        public override SDX.Result DrawStrikethrough(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Strikethrough strikethrough, SDX.ComObject clientDrawingEffect)
        {
            return SDX.Result.Ok;
        }

        /// <summary>
        /// IDWriteTextLayout::Draw calls this function to instruct the client to draw an underline.
        /// </summary>
        /// <param name="clientDrawingContext">The application-defined drawing context passed to  IDWriteTextLayout::Draw.</param>
        /// <param name="baselineOriginX">The pixel location (X-coordinate) at the baseline origin of the run where underline applies.</param>
        /// <param name="baselineOriginY">The pixel location (Y-coordinate) at the baseline origin of the run where underline applies.</param>
        /// <param name="underline">Pointer to  a structure containing underline logical information.</param>
        /// <param name="clientDrawingEffect">Application-defined effect to apply to the underline. Usually this argument represents effects such as the foreground brush filling the interior of a line.</param>
        /// <returns>
        /// If the method succeeds, it returns S_OK. Otherwise, it returns an HRESULT error code.
        /// </returns>
        /// <unmanaged>HRESULT DrawUnderline([None] void* clientDrawingContext,[None] FLOAT baselineOriginX,[None] FLOAT baselineOriginY,[In] const DWRITE_UNDERLINE* underline,[None] IUnknown* clientDrawingEffect)</unmanaged>
        /// <remarks>
        /// A single underline can be broken into multiple calls, depending on how the formatting changes attributes. If font sizes/styles change within an underline, the thickness and offset will be averaged weighted according to characters. To get an appropriate starting pixel position, add underline::offset to the baseline. Otherwise there will be no spacing between the text. The x coordinate will always be passed as the left side, regardless of text directionality. This simplifies drawing and reduces the problem of round-off that could potentially cause gaps or a double stamped alpha blend. To avoid alpha overlap, round the end points to the nearest device pixel.
        /// </remarks>
        public override SDX.Result DrawUnderline(object clientDrawingContext, float baselineOriginX, float baselineOriginY, ref Underline underline, SDX.ComObject clientDrawingEffect)
        {
            return SDX.Result.Ok;
        }
    }
}