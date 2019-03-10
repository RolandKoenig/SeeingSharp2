using System;
using System.Collections.Generic;
using System.Text;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public struct TriangleCorner
    {
        public TriangleCorner(int index)
            : this()
        {
            this.Index = index;
        }

        /// <summary>
        /// The index of this <see cref="TriangleCorner"/>
        /// </summary>
        public int Index;
    }
}
