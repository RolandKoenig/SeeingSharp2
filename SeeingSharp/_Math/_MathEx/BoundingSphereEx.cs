using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public static class BoundingSphereEx
    {
        public static void Transform(this BoundingSphere boundingSphere, Matrix matrix)
        {
            Vector3 center = boundingSphere.Center;
            Vector3 otherPoint = center + new Vector3(boundingSphere.Radius, 0f, 0f);

            boundingSphere.Center = Vector3.Transform(center, matrix).ToXYZ();
            boundingSphere.Radius = (Vector3.Transform(otherPoint, matrix).ToXYZ() - boundingSphere.Center).Length();
        }
    }
}
