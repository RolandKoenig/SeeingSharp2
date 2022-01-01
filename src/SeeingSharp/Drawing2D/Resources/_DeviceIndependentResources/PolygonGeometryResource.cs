using System.Drawing;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D.Resources
{
    public class PolygonGeometryResource : Geometry2DResourceBase
    {
        // resources
        private D2D.ID2D1PathGeometry _d2dGeometry;

        public override bool IsDisposed => _d2dGeometry == null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonGeometryResource"/> class.
        /// </summary>
        public PolygonGeometryResource()
        {
            _d2dGeometry = GraphicsCore.Current.FactoryD2D.CreatePathGeometry();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PolygonGeometryResource"/> class.
        /// </summary>
        /// <param name="polygon">The data which populates the geometry.</param>
        public PolygonGeometryResource(Polygon2D polygon)
            : this()
        {
            this.SetContent(polygon);
        }

        /// <summary>
        /// Sets the content to all lines in the given polygon.
        /// </summary>
        /// <param name="polygon">The polygon.</param>
        public unsafe void SetContent(Polygon2D polygon)
        {
            polygon.EnsureNotNull(nameof(polygon));
            polygon.Vertices.EnsureMoreThanZeroElements($"{nameof(polygon)}.{nameof(polygon.Vertices)}");

            using (var geoSink = _d2dGeometry.Open())
            {
                var vertices = polygon.Vertices;

                // Start the figure
                var startPoint = vertices[0];
                geoSink.BeginFigure(
                    *(PointF*)&startPoint,
                    D2D.FigureBegin.Filled);

                // AddObject all lines
                var vertexCount = vertices.Count;

                for (var loop = 1; loop < vertexCount; loop++)
                {
                    var actVectorOrig = vertices[loop];
                    geoSink.AddLine(*(PointF*)&actVectorOrig);
                }

                // End the figure
                geoSink.EndFigure(D2D.FigureEnd.Closed);
                geoSink.Close();
            }
        }

        /// <summary>
        /// Does this geometry intersect with the given one?
        /// </summary>
        /// <param name="otherGeometry">The other geometry.</param>
        public bool Intersects(PolygonGeometryResource otherGeometry)
        {
            this.EnsureNotNullOrDisposed("this");
            otherGeometry.EnsureNotNullOrDisposed(nameof(otherGeometry));

            var relation = _d2dGeometry.CompareWithGeometry(otherGeometry._d2dGeometry);

            return
                relation != D2D.GeometryRelation.Unknown &&
                relation != D2D.GeometryRelation.Disjoint;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public override void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _d2dGeometry);
        }

        /// <summary>
        /// Gets the geometry object.
        /// </summary>
        internal override D2D.ID2D1Geometry GetGeometry()
        {
            return _d2dGeometry;
        }
    }
}
