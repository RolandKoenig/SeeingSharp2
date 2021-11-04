using System.Numerics;

namespace SeeingSharp.Drawing3D.Primitives
{
    public class CylinderGeometryFactory : GeometryFactory
    {
        public float Radius { get; set; } = 0.5f;

        public float Height { get; set; } = 1f;

        public int CountOfSegments { get; set; } = 10;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();
            var mainSurface = result.CreateSurface();
            mainSurface.BuildCylinder(Vector3.Zero, this.Radius, this.Height, this.CountOfSegments);

            return result;
        }
    }
}