using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class CubeGeometryFactory : GeometryFactory
    {
        public float Width { get; set; } = 1f;

        public float Height { get; set; } = 1f;

        public float Depth { get; set; } = 1f;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();
            result.CreateSurface()
                .BuildCube24V(
                    new Vector3(-(this.Width / 2f), -(this.Height / 2f), -(this.Depth / 2f)),
                    new Vector3(this.Width, this.Height, this.Depth));

            return result;
        }
    }
}