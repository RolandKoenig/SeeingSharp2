using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D.Primitives
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
                .BuildCube(this.Width, this.Height, this.Depth);

            return result;
        }
    }
}