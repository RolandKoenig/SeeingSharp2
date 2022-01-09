using System;
using System.Linq;
using System.Collections.Generic;
using System.Numerics;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D.Primitives;
using SeeingSharp.Mathematics;
using SharpGen.Runtime;
using Vortice.DCommon;
using D2D = Vortice.Direct2D1;
using DWrite = Vortice.DirectWrite;

namespace SeeingSharp.Drawing3D.Geometries
{
    internal class GeometryTextRenderer : DWrite.TextRendererBase
    {
        private TextGeometryOptions _geometryOptions;
        private GeometrySurface _targetSurface;

        /// <summary>
        /// Initializes a new instance of the <see cref="GeometryTextRenderer" /> class.
        /// </summary>
        public GeometryTextRenderer(GeometrySurface targetSurface, TextGeometryOptions textGeometryOptions)
        {
            _targetSurface = targetSurface;
            _geometryOptions = textGeometryOptions;
        }

        /// <summary>
        /// IDWriteTextLayout::Draw calls this function to instruct the client to render a run of glyphs.
        /// </summary>
        /// <param name="clientDrawingContext">The application-defined drawing context passed to  <see cref="M:Vortice.DirectWrite.TextLayout.Draw_(System.IntPtr,System.IntPtr,System.Single,System.Single)" />.</param>
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
        /// The <see cref="M:Vortice.DirectWrite.TextLayout.Draw_(System.IntPtr,System.IntPtr,System.Single,System.Single)" /> function calls this callback function with all the information about glyphs to render. The application implements this callback by mostly delegating the call to the underlying platform's graphics API such as {{Direct2D}} to draw glyphs on the drawing context. An application that uses GDI can implement this callback in terms of the <see cref="M:Vortice.DirectWrite.BitmapRenderTarget.DrawGlyphRun(System.Single,System.Single,SharpDX.Direct2D1.MeasuringMode,Vortice.DirectWrite.GlyphRun,Vortice.DirectWrite.RenderingParams,SharpDX.Color4)" /> method.
        /// </remarks>
        public override void DrawGlyphRun(
            IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY,
            MeasuringMode measuringMode, DWrite.GlyphRun glyphRun, DWrite.GlyphRunDescription glyphRunDescription, IUnknown clientDrawingEffect)
        {
            if (glyphRun.Indices == null ||
                glyphRun.Indices.Length == 0)
            {
                return;
            }

            GraphicsCore.EnsureGraphicsSupportLoaded();
            var d2DFactory = GraphicsCore.Current.FactoryD2D!;

            // Extrude geometry data out of given glyph run
            var geometryExtruder = new SimplePolygon2DGeometrySink(new Vector2(baselineOriginX, baselineOriginY));

            using (var pathGeometry = d2DFactory.CreatePathGeometry())
            {
                // Write all geometry data into a standard PathGeometry object
                using (var geoSink = pathGeometry.Open())
                {
                    glyphRun.FontFace!.GetGlyphRunOutline(
                        glyphRun.FontSize,
                        glyphRun.Indices,
                        glyphRun.Advances,
                        glyphRun.Offsets,
                        glyphRun.IsSideways,
                        glyphRun.BidiLevel % 2 == 1,
                        geoSink);
                    geoSink.Close();
                }

                // Simplify written geometry and write it into own structure
                pathGeometry.Simplify(D2D.GeometrySimplificationOption.Lines, _geometryOptions.SimplificationFlatternTolerance, geometryExtruder);
            }

            // Geometry for caching the result
            var tempGeometry = new Geometry();
            var tempSurface = tempGeometry.CreateSurface();

            // Create the text surface
            if (_geometryOptions.MakeSurface)
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
                    // - RemoveObject found holes from current hole list
                    var polygonForRendering = actFillingPolygon;
                    var polygonForTriangulation = actFillingPolygon.Clone();
                    var cutPoints = new List<Vector2>();

                    foreach (var actHole in correspondingHoles)
                    {
                        holePolygons.Remove(actHole);
                        polygonForRendering = polygonForRendering.MergeWithHole(actHole, Polygon2DMergeOptions.DEFAULT, cutPoints);
                        polygonForTriangulation = polygonForTriangulation.MergeWithHole(actHole, new Polygon2DMergeOptions { MakeMergepointSpaceForTriangulation = true });
                    }

                    var actBaseIndex = tempGeometry.CountVertices;

                    // Append all vertices to temporary Geometry
                    for (var loop = 0; loop < polygonForRendering.Vertices.Count; loop++)
                    {
                        // Calculate 3d location and texture coordinate
                        var actVertexLocation = new Vector3(
                            polygonForRendering.Vertices[loop].X,
                            0f,
                            polygonForRendering.Vertices[loop].Y);
                        var actTexCoord = new Vector2(
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
                        tempGeometry.AddVertex(
                            new VertexBasic(
                                actVertexLocation,
                                _geometryOptions.SurfaceVertexColor,
                                actTexCoord,
                                new Vector3(0f, 1f, 0f)));
                    }

                    // Generate cubes on each vertex if requested
                    if (_geometryOptions.GenerateCubesOnVertices)
                    {
                        for (var loop = 0; loop < polygonForRendering.Vertices.Count; loop++)
                        {
                            var colorToUse = Color4.GreenColor;
                            var pointRenderSize = 0.1f;

                            if (cutPoints.Contains(polygonForRendering.Vertices[loop]))
                            {
                                colorToUse = Color4.RedColor;
                                pointRenderSize = 0.15f;
                            }

                            var actVertexLocation = new Vector3(
                                polygonForRendering.Vertices[loop].X,
                                0f,
                                polygonForRendering.Vertices[loop].Y);
                            tempSurface.BuildCube(actVertexLocation, pointRenderSize).SetVertexColor(colorToUse);
                        }
                    }

                    // Triangulate the polygon
                    var triangleIndices = polygonForTriangulation.TriangulateUsingCuttingEars();
                    if (triangleIndices == null) { continue; }

                    // Append all triangles to the temporary geometry
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

            if (_geometryOptions.MakeVolumetricText)
            {
                var volumetricTextDepth = _geometryOptions.VolumetricTextDepth;

                if (_geometryOptions.VerticesScaleFactor > 0f)
                {
                    volumetricTextDepth = volumetricTextDepth / _geometryOptions.VerticesScaleFactor;
                }

                // AddObject all side surfaces
                foreach (var actPolygon in geometryExtruder.GeneratedPolygons)
                {
                    foreach (var actLine in actPolygon.Lines)
                    {
                        tempSurface.BuildRect(
                            new Vector3(actLine.StartPosition.X, -volumetricTextDepth, actLine.StartPosition.Y),
                            new Vector3(actLine.EndPosition.X, -volumetricTextDepth, actLine.EndPosition.Y),
                            new Vector3(actLine.EndPosition.X, 0f, actLine.EndPosition.Y),
                            new Vector3(actLine.StartPosition.X, 0f, actLine.StartPosition.Y))
                            .SetVertexColor(_geometryOptions.VolumetricSideSurfaceVertexColor);
                    }
                }
            }

            // Do also make back surface?
            if (_geometryOptions.MakeBackSurface)
            {
                for (var loop = 0; loop < triangleCountWithoutSide; loop++)
                {
                    var triangle = tempSurface.Triangles[loop];
                    var vertex0 = tempGeometry.Vertices[triangle.Index1];
                    var vertex1 = tempGeometry.Vertices[triangle.Index2];
                    var vertex2 = tempGeometry.Vertices[triangle.Index3];
                    var changeVector = new Vector3(0f, -_geometryOptions.VolumetricTextDepth, 0f);

                    tempSurface.AddTriangle(
                        vertex2.Copy(vertex2.Position - changeVector, Vector3.Negate(vertex2.Normal)),
                        vertex1.Copy(vertex1.Position - changeVector, Vector3.Negate(vertex1.Normal)),
                        vertex0.Copy(vertex0.Position - changeVector, Vector3.Negate(vertex0.Normal)));
                }
            }

            // Toggle coordinate system becomes text input comes in opposite direction
            tempGeometry.ToggleCoordinateSystem();

            // Scale the text using given scale factor
            if (_geometryOptions.VerticesScaleFactor > 0f)
            {
                var scaleMatrix = Matrix4x4.CreateScale(
                    _geometryOptions.VerticesScaleFactor,
                    _geometryOptions.VerticesScaleFactor,
                    _geometryOptions.VerticesScaleFactor);

                var transformMatrix = new Matrix4Stack(scaleMatrix);
                transformMatrix.TransformLocal(_geometryOptions.VertexTransform);

                tempGeometry.UpdateVerticesUsingTranslation(actVector => Vector3.Transform(actVector, transformMatrix.Top));
            }

            // Calculate all normals before adding to target geometry
            if (_geometryOptions.CalculateNormals)
            {
                tempGeometry.CalculateNormalsFlat();
            }

            // Merge temporary geometry to target geometry
            _targetSurface.AddGeometry(tempGeometry);
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
        public override void DrawInlineObject(IntPtr clientDrawingContext, float originX, float originY, DWrite.IDWriteInlineObject inlineObject, RawBool isSideways, RawBool isRightToLeft, IUnknown clientDrawingEffect)
        {

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
        public override void DrawStrikethrough(IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, ref DWrite.Strikethrough strikethrough, IUnknown clientDrawingEffect)
        {

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
        public override void DrawUnderline(IntPtr clientDrawingContext, float baselineOriginX, float baselineOriginY, ref DWrite.Underline underline, IUnknown clientDrawingEffect)
        {

        }
    }
}