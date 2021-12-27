using System.Drawing;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Core;
using SeeingSharp.Util;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D
{
    public class EllipseGeometryResource : Geometry2DResourceBase
    {
        // Configuration
        private Vector2 _center;
        private float _radiusX;
        private float _radiusY;

        // Resources
        private D2D.ID2D1EllipseGeometry _geometry;

        public override bool IsDisposed => _geometry == null;

        public Vector2 Center => _center;

        public float RadiusX => _radiusX;

        public float RadiusY => _radiusY;

        /// <summary>
        /// Initializes a new instance of the <see cref="EllipseGeometryResource"/> class.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="radiusX">The radius in x direction.</param>
        /// <param name="radiusY">The radius in y direction.</param>
        public EllipseGeometryResource(Vector2 center, float radiusX, float radiusY)
        {
            this.SetContent(center, radiusX, radiusY);
        }

        /// <summary>
        /// Sets the content to all lines in the given polygon.
        /// </summary>
        /// <param name="center">The center of the ellipse.</param>
        /// <param name="radiusX">The radius in x direction.</param>
        /// <param name="radiusY">The radius in y direction.</param>
        public unsafe void SetContent(Vector2 center, float radiusX, float radiusY)
        {
            radiusX.EnsurePositiveOrZero(nameof(radiusX));
            radiusY.EnsurePositiveOrZero(nameof(radiusY));

            _center = center;
            _radiusX = radiusX;
            _radiusY = radiusY;

            SeeingSharpUtil.SafeDispose(ref _geometry);

            _geometry = GraphicsCore.Current.FactoryD2D.CreateEllipseGeometry(
                new D2D.Ellipse(
                    *(PointF*)&center,
                    radiusX, radiusY));
        }

        public override void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _geometry);
        }

        /// <summary>
        /// Gets the geometry object.
        /// </summary>
        internal override D2D.ID2D1Geometry GetGeometry()
        {
            this.EnsureNotNullOrDisposed(nameof(_geometry));

            return _geometry;
        }
    }
}
