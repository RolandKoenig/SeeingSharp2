namespace SeeingSharp.Drawing3D.Primitives
{
    public class PyramidGeometryFactory : GeometryFactory
    {
        public float Width { get; set; } = 1f;

        public float Height { get; set; } = 1f;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();
            result.CreateSurface()
                .BuildPyramid(this.Width, this.Height);

            return result;
        }
    }
}
