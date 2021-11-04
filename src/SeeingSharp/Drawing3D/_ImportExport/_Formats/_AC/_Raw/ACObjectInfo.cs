using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Drawing3D
{
    internal class ACObjectInfo
    {
        public List<ACObjectInfo> Children;
        public int KidCount;
        public string Name;
        public Matrix4x4 Rotation;
        public List<ACSurface> Surfaces;
        public string Texture;
        public Vector2 TextureRepeat;
        public Vector3 Translation;
        public ACObjectType Type;
        public string Url;
        public List<ACVertex> Vertices;

        /// <summary>
        /// Initializes a new instance of the <see cref="ACObjectInfo"/> class.
        /// </summary>
        public ACObjectInfo()
        {
            Children = new List<ACObjectInfo>();
            Surfaces = new List<ACSurface>();
            Vertices = new List<ACVertex>();
            Rotation = Matrix4x4.Identity;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets total count of all child objects
        /// </summary>
        public int CountAllChildObjects()
        {
            var result = 0;

            foreach (var actObj in Children)
            {
                result += actObj.CountAllChildObjects();
            }

            return result;
        }
    }
}
