using System.Numerics;

namespace SeeingSharp.Mathematics
{
    public partial struct Ray
    {
        /// <summary>
        /// Transforms the ray
        /// </summary>
        public void Transform(Matrix4x4 transformMatrix)
        {
            Position = Vector3.Transform(Position, transformMatrix);
            Direction = Vector3.TransformNormal(Direction, transformMatrix);
        }
    }
}
