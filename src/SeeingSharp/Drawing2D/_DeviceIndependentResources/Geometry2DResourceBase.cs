using System;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D
{
    public abstract class Geometry2DResourceBase : IDisposable, ICheckDisposed
    {
        public abstract bool IsDisposed
        {
            get;
        }

        /// <summary>
        /// A very simple collision check between this geometry and the given one.
        /// </summary>
        /// <param name="other">The other geometry.</param>
        public bool IntersectsWith(Geometry2DResourceBase other)
        {
            this.EnsureNotNullOrDisposed("this");
            other.EnsureNotNullOrDisposed(nameof(other));

            var geometryThis = this.GetGeometry();
            var geometryOther = other.GetGeometry();

            return geometryThis.CompareWithGeometry(geometryOther) != D2D.GeometryRelation.Disjoint;
        }

        /// <summary>
        /// A very simple collision check between this geometry and the given one.
        /// </summary>
        /// <param name="other">The other geometry.</param>
        /// <param name="otherTransform">The matrix which is used to transform the given geometry before checking.</param>
        public bool IntersectsWith(Geometry2DResourceBase other, Matrix3x2 otherTransform)
        {
            this.EnsureNotNullOrDisposed("this");
            other.EnsureNotNullOrDisposed(nameof(other));

            var geometryThis = this.GetGeometry();
            var geometryOther = other.GetGeometry();

            return geometryThis.CompareWithGeometry(geometryOther, otherTransform, 1f) != D2D.GeometryRelation.Disjoint;
        }

        public abstract void Dispose();

        /// <summary>
        /// Gets the geometry object.
        /// </summary>
        internal abstract D2D.ID2D1Geometry GetGeometry();
    }
}
