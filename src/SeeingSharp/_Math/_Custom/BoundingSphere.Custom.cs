using System.Numerics;

namespace SeeingSharp
{
    public partial struct BoundingSphere
    {
        public static BoundingSphere Empty;

        public void Transform(Matrix4x4 matrix)
        {
            if(this == Empty){ return; }

            var center = Center;
            var otherPoint = center + new Vector3(Radius, 0f, 0f);

            Center = Vector3.Transform(center, matrix);
            Radius = (Vector3.Transform(otherPoint, matrix) - Center).Length();
        }
    }
}
