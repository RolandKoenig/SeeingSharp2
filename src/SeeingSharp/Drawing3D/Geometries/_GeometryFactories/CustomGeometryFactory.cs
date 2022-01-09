using System;

namespace SeeingSharp.Drawing3D.Geometries
{
    public class CustomGeometryFactory : GeometryFactory
    {
        /// <summary>
        /// Gets the containing geometry.
        /// </summary>
        public Geometry? Geometry { get; }

        /// <summary>
        /// Gets the containing geometry for low detail level.
        /// </summary>
        public Geometry? GeometryLowDetail { get; }

        public Func<GeometryBuildOptions, Geometry>? BuilderFunction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGeometryFactory"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public CustomGeometryFactory(Geometry geometry)
        {
            this.Geometry = geometry;
            this.GeometryLowDetail = geometry;
            this.BuilderFunction = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGeometryFactory"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="geometryLowDetail">The geometry for low detail level.</param>
        public CustomGeometryFactory(Geometry geometry, Geometry geometryLowDetail)
        {
            this.Geometry = geometry;
            this.GeometryLowDetail = geometryLowDetail;
            this.BuilderFunction = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGeometryFactory"/> class.
        /// </summary>
        /// <param name="builderFunc">The function which generates the <seealso cref="Geometry"/> object.</param>
        public CustomGeometryFactory(Func<GeometryBuildOptions, Geometry> builderFunc)
        {
            this.Geometry = null;
            this.GeometryLowDetail = null;
            this.BuilderFunction = builderFunc;
        }

        /// <summary>
        /// Builds the geometry.
        /// </summary>
        /// <param name="buildOptions">Some generic options for geometry building</param>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            if (this.BuilderFunction != null)
            {
                return this.BuilderFunction(buildOptions);
            }

            if (buildOptions.IsHighDetail) { return this.Geometry!; }
            return this.GeometryLowDetail!;
        }
    }
}