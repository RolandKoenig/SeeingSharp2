using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class CircleGeometryFactory : GeometryFactory
    {
        public float Radius
        {
            get;
            set;
        }

        public float Width
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

        public CircleGeometryFactory()
        {
            this.Radius = 2f;
            this.Width = 0.5f;
            this.Height = 0.1f;
            this.CountOfSegments = 10;
        }

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();
            var mainSurface = result.CreateSurface();
            mainSurface.BuildCircleFullV(Vector3.Zero, this.Radius, this.Width, this.Height, this.CountOfSegments);

            return result;
        }
    }
}