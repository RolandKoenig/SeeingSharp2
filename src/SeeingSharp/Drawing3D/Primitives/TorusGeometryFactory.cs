using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Drawing3D.Geometries;

namespace SeeingSharp.Drawing3D.Primitives
{
    public class TorusGeometryFactory : GeometryFactory
    {
        public int TDiv { get; set; } = 30;

        public int PDiv { get; set; } = 30;

        public float TorusRadius { get; set; } = 0.45f;

        public float TubeRadius { get; set; } = 0.1f;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            var mainSurface = result.CreateSurface();
            mainSurface.BuildTorus(this.TDiv, this.PDiv, this.TorusRadius, this.TubeRadius);

            return result;
        }
    }
}
