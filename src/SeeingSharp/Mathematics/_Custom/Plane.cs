using System.Numerics;

namespace SeeingSharp.Mathematics
{
    public partial struct Plane
    {
        /// <summary>
        /// Calculates the distance from this plane to the given point.
        /// </summary>
        /// <param name="point">The point to calculate the distance to.</param>
        public float Distance(ref Vector3 point)
        {
            var distance = Vector3.Dot(Normal, point);
            distance += D;

            return distance;
        }
    }
}
