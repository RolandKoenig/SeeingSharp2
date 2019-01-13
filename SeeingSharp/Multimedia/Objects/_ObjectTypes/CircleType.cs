using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.Multimedia.Objects
{
    public class CircleType : ObjectType
    {
        public CircleType()
        {
            this.Radius = 2f;
            this.Width = 0.5f;
            this.Height = 0.1f;
            this.SegmentCount = 10;
        }

        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            VertexStructure result = new VertexStructure();

            var mainSurface = result.CreateSurface();
            mainSurface.BuildCircleFullV(Vector3.Zero, this.Radius, this.Width, this.Height, this.SegmentCount, Color4Ex.Transparent);

            return result;
        }

        public float Radius
        {
            get;
            set;
        }

        public float Width
        {
            get;
            set;
        }

        public float Height
        {
            get;
            set;
        }

        public int SegmentCount
        {
            get;
            set;
        }
    }
}
