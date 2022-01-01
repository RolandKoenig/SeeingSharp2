namespace SeeingSharp.Drawing3D.Geometries
{
    public abstract class GeometryFactory
    {
        /// <summary>
        /// Builds a <see cref="Geometry"/> using given parameters (like DetailLevel).
        /// </summary>
        /// <param name="buildOptions">Some generic options for geometry building</param>
        public abstract Geometry BuildGeometry(GeometryBuildOptions buildOptions);
    }
}