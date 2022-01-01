using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Drawing3D.ImportExport
{
    internal class ACSurface
    {
        /// <summary>
        /// Is this surface built using polygons?
        /// </summary>
        public bool IsPolygon => (Flags & 0xF0) == 0;

        /// <summary>
        /// Is this surface a closed line?
        /// </summary>
        public bool IsClosedLine => (Flags & 0xF0) == 1;

        /// <summary>
        /// Is this surface a line?
        /// </summary>
        public bool IsLine => (Flags & 0xF0) == 2;

        /// <summary>
        /// Is this surface flat shaded?
        /// </summary>
        public bool IsFlatShaded => (Flags & 16) != 16;

        /// <summary>
        /// Is this surface two sided?
        /// </summary>
        public bool IsTwoSided => (Flags & 32) == 32;

        public int Flags;
        public int Material;
        public List<Vector2> TextureCoordinates;
        public List<int> VertexReferences;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACSurface"/> class.
        /// </summary>
        public ACSurface()
        {
            VertexReferences = new List<int>();
            TextureCoordinates = new List<Vector2>();
        }
    }
}
