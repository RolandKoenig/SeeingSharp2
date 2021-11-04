namespace SeeingSharp.Drawing3D.Primitives
{
    public class GeosphereGeometryFactory : GeometryFactory
    {
        public int CountSubdivisions { get; set; } = 3;

        public float Radius { get; set; } = 0.5f;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            var mainSurface = result.CreateSurface();
            mainSurface.BuildGeosphere(this.Radius, this.CountSubdivisions);

            return result;
        }
    }
}