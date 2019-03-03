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

        public Vector3 Normal;

        public Color4 Color;

        public Vector3 Tangent;

        public Vector3 Binormal;

        /// <summary>
        /// Gets or sets the texture factor.
        /// This value decides whether a texture is displayed on this vertex or not.
        /// A value greater or equal 0 will show the texture, all negatives will hide it.
        /// </summary>
        public float TextureFactor;

        public Vector2 TextureCoordinate1;
    }
}
