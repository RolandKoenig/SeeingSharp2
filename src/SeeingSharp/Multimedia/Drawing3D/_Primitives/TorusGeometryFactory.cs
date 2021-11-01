using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class TorusGeometryFactory : GeometryFactory
    {
        public int TDiv { get; set; } = 30;

        public int PDiv { get; set; } = 30;

        public float TorusDiameter { get; set; } = 3f;

        public float TubeDiameter { get; set; } = 0.5f;

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();

            var mainSurface = result.CreateSurface();
            mainSurface.BuildTorus(this.TDiv, this.PDiv, this.TorusDiameter, this.TubeDiameter);

            return result;
        }
    }
}
