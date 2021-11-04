namespace SeeingSharp.Drawing3D.Primitives
{
    public class SphereGeometryFactory : GeometryFactory
    {
        public int TDiv { get; set; } = 30;

        public int PDiv { get; set; } = 30;

        public float Radius { get; set; } = 0.5f;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            var mainSurface = result.CreateSurface();
            mainSurface.BuildShpere(this.TDiv, this.PDiv, this.Radius);

            return result;
        }
    }
}