﻿namespace SeeingSharp.Drawing3D.Geometries
{
    public struct TriangleCorner
    {
        public TriangleCorner(int index)
            : this()
        {
            Index = index;
        }

        /// <summary>
        /// The index of this <see cref="TriangleCorner"/>
        /// </summary>
        public int Index;
    }
}
