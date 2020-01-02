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
using SeeingSharp.Util;
using SharpDX.Direct2D1;
using SharpDX.Mathematics.Interop;
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    internal class SimplePolygon2DGeometrySink : DummyComObject, GeometrySink
    {
        private List<Vector2> m_currentPolygonBuilder;
        private Vector2 m_origin;
        private List<Polygon2D> m_polygons2D;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimplePolygon2DGeometrySink" /> class.
        /// </summary>
        public SimplePolygon2DGeometrySink(Vector2 origin)
        {
            m_origin = origin;
            m_polygons2D = new List<Polygon2D>();
            m_currentPolygonBuilder = new List<Vector2>(500);
        }

        /// <summary>
        /// Adds a single arc to the path geometry.
        /// </summary>
        /// <param name="arc">The arc segment to add to the figure.</param>
        /// <unmanaged>void AddArc([In] const D2D1_ARC_SEGMENT* arc)</unmanaged>
        /// <exception cref="SeeingSharpException"></exception>
        public void AddArc(ArcSegment arc)
        {
            throw new SeeingSharpGraphicsException("Geometry type 'Arc' not supported for text geometry building!");
        }

        /// <summary>
        /// Creates  a cubic Bezier curve between the current point and the specified endpoint.
        /// </summary>
        /// <param name="bezier">A structure that describes the control points and endpoint of the Bezier curve to add.</param>
        /// <unmanaged>void AddBezier([In] const D2D1_BEZIER_SEGMENT* bezier)</unmanaged>
        /// <exception cref="SeeingSharpException"></exception>
        public void AddBezier(BezierSegment bezier)
        {
            throw new SeeingSharpGraphicsException("Geometry type 'Bezier' not supported for text geometry building!");
        }

        /// <summary>
        /// Creates a line segment between the current point and the specified end point and adds it to the geometry sink.
        /// </summary>
        /// <param name="point">The end point of the line to draw.</param>
        /// <unmanaged>void AddLine([None] D2D1_POINT_2F point)</unmanaged>
        public void AddLine(RawVector2 point)
        {
            m_currentPolygonBuilder.Add(new Vector2(point.X, point.Y) - m_origin);
        }

        /// <summary>
        /// Creates  a quadratic Bezier curve between the current point and the specified endpoint.
        /// </summary>
        /// <param name="bezier">A structure that describes the control point and the endpoint of the quadratic Bezier curve to add.</param>
        /// <unmanaged>void AddQuadraticBezier([In] const D2D1_QUADRATIC_BEZIER_SEGMENT* bezier)</unmanaged>
        /// <exception cref="SeeingSharpException"></exception>
        public void AddQuadraticBezier(QuadraticBezierSegment bezier)
        {
            throw new SeeingSharpGraphicsException("Geometry type 'QuadricBezier' not supported for text geometry building!");
        }

        /// <summary>
        /// Adds a sequence of quadratic Bezier segments as an array in a single call.
        /// </summary>
        /// <param name="beziers">An array of a sequence of quadratic Bezier segments.</param>
        /// <unmanaged>void AddQuadraticBeziers([In, Buffer] const D2D1_QUADRATIC_BEZIER_SEGMENT* beziers,[None] UINT beziersCount)</unmanaged>
        /// <exception cref="SeeingSharpException"></exception>
        public void AddQuadraticBeziers(QuadraticBezierSegment[] beziers)
        {
            throw new SeeingSharpGraphicsException("Geometry type 'QuadricBeziers' not supported for text geometry building!");
        }

        /// <summary>
        /// Creates a sequence of cubic Bezier curves and adds them to the geometry sink.
        /// </summary>
        /// <param name="beziers">A pointer to an array of Bezier segments that describes the Bezier curves to create. A curve is drawn from the geometry sink's current point (the end point of the last segment drawn or the location specified by {{BeginFigure}}) to the end point of the first Bezier segment in the array. if the array contains additional Bezier segments, each subsequent Bezier segment uses the end point of the preceding Bezier segment as its start point.</param>
        /// <unmanaged>void AddBeziers([In, Buffer] const D2D1_BEZIER_SEGMENT* beziers,[None] UINT beziersCount)</unmanaged>
        /// <exception cref="SeeingSharpException"></exception>
        public void AddBeziers(BezierSegment[] beziers)
        {
            throw new SeeingSharpGraphicsException("Geometry type 'Beziers' not supported for text geometry building!");
        }

        /// <summary>
        /// Creates a sequence of lines using the specified points and adds them to the geometry sink.
        /// </summary>
        /// <param name="pointsRef">A pointer to an array of one or more points that describe the lines to draw. A line is drawn from the geometry sink's current point (the end point of the last segment drawn or the location specified by {{BeginFigure}}) to the first point in the array. if the array contains additional points, a line is drawn from the first point to the second point in the array, from the second point to the third point, and so on.</param>
        /// <unmanaged>void AddLines([In, Buffer] const D2D1_POINT_2F* points,[None] UINT pointsCount)</unmanaged>
        public void AddLines(RawVector2[] pointsRef)
        {
            for (var loop = 0; loop < pointsRef.Length; loop++)
            {
                m_currentPolygonBuilder.Add(new Vector2(pointsRef[loop].X, pointsRef[loop].Y) - m_origin);
            }
        }

        /// <summary>
        /// Starts a new figure at the specified point.
        /// </summary>
        /// <param name="startPoint">The point at which to begin the new figure.</param>
        /// <param name="figureBegin">Whether the new figure should be hollow or filled.</param>
        /// <unmanaged>void BeginFigure([None] D2D1_POINT_2F startPoint,[None] D2D1_FIGURE_BEGIN figureBegin)</unmanaged>
        /// <remarks>
        /// If this method is called while a figure is currently in progress, the interface is invalidated and all future methods will fail.
        /// </remarks>
        public void BeginFigure(RawVector2 startPoint, FigureBegin figureBegin)
        {
            m_currentPolygonBuilder.Clear();
            m_currentPolygonBuilder.Add(new Vector2(startPoint.X, startPoint.Y) - m_origin);
        }

        /// <summary>
        /// Closes the geometry sink, indicates whether it is in an error state, and resets the sink's error state.
        /// </summary>
        /// <unmanaged>HRESULT Close()</unmanaged>
        /// <remarks>
        /// Do not close the geometry sink while a figure is still in progress; doing so puts the geometry sink in an error state. For the close operation to be successful, there must be one {{EndFigure}} call for each call to {{BeginFigure}}.After calling this method, the geometry sink might not be usable. Direct2D implementations of this interface do not allow the geometry sink to be modified after it is closed, but other implementations might not impose this restriction.
        /// </remarks>
        public void Close()
        {
        }

        /// <summary>
        /// Ends the current figure; optionally, closes it.
        /// </summary>
        /// <param name="figureEnd">A value that indicates whether the current figure is closed. If the figure is closed, a line is drawn between the current point and the start point specified by {{BeginFigure}}.</param>
        /// <unmanaged>void EndFigure([None] D2D1_FIGURE_END figureEnd)</unmanaged>
        /// <remarks>
        /// Calling this method without a matching call to {{BeginFigure}} places the geometry sink in an error state; subsequent calls are ignored, and the overall failure will be returned when the {{Close}} method is called.
        /// </remarks>
        public void EndFigure(FigureEnd figureEnd)
        {
            if (m_currentPolygonBuilder.Count >= 3)
            {
                m_polygons2D.Add(new Polygon2D(m_currentPolygonBuilder.ToArray()));
                m_currentPolygonBuilder.Clear();
            }
        }

        /// <summary>
        /// Specifies the method used to determine which points are inside the geometry described by this geometry sink  and which points are outside.
        /// </summary>
        /// <param name="fillMode">The method used to determine whether a given point is part of the geometry.</param>
        /// <unmanaged>void SetFillMode([None] D2D1_FILL_MODE fillMode)</unmanaged>
        /// <remarks>
        /// The fill mode defaults to D2D1_FILL_MODE_ALTERNATE. To set the fill mode, call SetFillMode before the first call to {{BeginFigure}}. Not doing will put the geometry sink in an error state.
        /// </remarks>
        public void SetFillMode(FillMode fillMode)
        {

        }

        /// <summary>
        /// Specifies stroke and join options to be applied to new segments added to the geometry sink.
        /// </summary>
        /// <param name="vertexFlags">Stroke and join options to be applied to new segments added to the geometry sink.</param>
        /// <unmanaged>void SetSegmentFlags([None] D2D1_PATH_SEGMENT vertexFlags)</unmanaged>
        /// <remarks>
        /// After this method is called, the specified segment flags are applied to each segment subsequently added to the sink. The segment flags are applied to every additional segment until this method is called again and a different set of segment flags is specified.
        /// </remarks>
        public void SetSegmentFlags(PathSegment vertexFlags)
        {

        }

        /// <summary>
        /// Gets a collection containing all generated polygons.
        /// </summary>
        public IEnumerable<Polygon2D> GeneratedPolygons => m_polygons2D;
    }
}