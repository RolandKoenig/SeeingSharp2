#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Util;
using SeeingSharp.Checking;
using SharpDX;

// Namespace mappings
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

            D2D.Geometry geometryThis = this.GetGeometry();
            D2D.Geometry geometryOther = other.GetGeometry();

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

            D2D.Geometry geometryThis = this.GetGeometry();
            D2D.Geometry geometryOther = other.GetGeometry();

            return geometryThis.Compare(geometryOther, otherTransform, 1f) != D2D.GeometryRelation.Disjoint;
        }

        /// <summary>
        /// Gets the geometry object.
        /// </summary>
        internal abstract D2D.Geometry GetGeometry();

        public abstract bool IsDisposed
        {
            get;
        }

        public abstract void Dispose();
    }
}
