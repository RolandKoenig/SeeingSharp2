using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.Multimedia.Objects
{
    public class CylinderType : ObjectType
    {
        public CylinderType()
        {
            this.Radius = 0.5f;
            this.Height = 1f;
            this.CountOfSegments = 10;
        }

        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            VertexStructure result = new VertexStructure();

            var mainSurface = result.CreateSurface();
            mainSurface.BuildCylinderFullV(Vector3.Zero, this.Radius, this.Height, this.CountOfSegments, Color4Ex.Transparent);

            return result;
        }

        public float Radius
        {
            get;
            set;
        }

        public float Height
        {
            get;
            set;
        }

        public int CountOfSegments
        {
            get;
            set;
        }
    }
}
