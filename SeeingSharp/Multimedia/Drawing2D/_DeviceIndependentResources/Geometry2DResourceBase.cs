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
using SeeingSharp.Checking;
using SeeingSharp.Util;
using SharpDX;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Multimedia.Drawing2D
{
    public abstract class Geometry2DResourceBase : IDisposable, ICheckDisposed
    {
        /// <summary>
        /// A very simple collision check between this geometry and the given one.
        /// </summary>
        /// <param name="other">The other geometry.</param>
        public bool IntersectsWith(Geometry2DResourceBase other)
        {
            this.EnsureNotNullOrDisposed("this");
            other.EnsureNotNullOrDisposed(nameof(other));

            var geometryThis = GetGeometry();
            var geometryOther = other.GetGeometry();

            return geometryThis.Compare(geometryOther) != D2D.GeometryRelation.Disjoint;
        }

        /// <summary>
        /// A very simple collision check between this geometry and the given one.
        /// </summary>
        /// <param name="other">The other geometry.</param>
        /// <param name="otherTransform">The matrix which is used to transform the given geometry bevore checking.</param>
        public bool IntersectsWith(Geometry2DResourceBase other, Matrix3x2 otherTransform)
        {
            this.EnsureNotNullOrDisposed("this");
            other.EnsureNotNullOrDisposed(nameof(other));

            var geometryThis = GetGeometry();
            var geometryOther = other.GetGeometry();

            return geometryThis.Compare(geometryOther, otherTransform, 1f) != D2D.GeometryRelation.Disjoint;
        }

        public abstract void Dispose();

        /// <summary>
        /// Gets the geometry object.
        /// </summary>
        internal abstract D2D.Geometry GetGeometry();

        public abstract bool IsDisposed
        {
            get;
        }
    }
}
