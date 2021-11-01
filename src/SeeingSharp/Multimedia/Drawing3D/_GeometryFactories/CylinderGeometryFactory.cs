using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class CylinderGeometryFactory : GeometryFactory
    {
        public float Radius
        {
            get;
            set;
        }

        public float Height
        {
            get;
            set;
        }

        public int CountOfSegments
        {
            get;
            set;
        }

        public CylinderGeometryFactory()
        {
            this.Radius = 0.5f;
            this.Height = 1f;
            this.CountOfSegments = 10;
        }

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();
            var mainSurface = result.CreateSurface();
            mainSurface.BuildCylinderFullV(Vector3.Zero, this.Radius, this.Height, this.CountOfSegments);

            return result;
        }
    }
}