namespace SeeingSharp.Multimedia.Drawing3D.Primitives
{
    public class SphereGeometryFactory : GeometryFactory
    {
        public int TDiv
        {
            get;
            set;
        }

        public int PDiv
        {
            get;
            set;
        }

        public float Radius
        {
            get;
            set;
        } = 0.5f;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            var mainSurface = result.CreateSurface();
            mainSurface.BuildShpere(this.TDiv, this.PDiv, this.Radius);

            return result;
        }
    }
}