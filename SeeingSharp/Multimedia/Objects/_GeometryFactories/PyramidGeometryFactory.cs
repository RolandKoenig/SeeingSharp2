using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public class PyramidGeometryFactory : GeometryFactory
    {
        public override Geometry BuildStructure(GeometryBuildOptions buildOptions)
        {
            var matProperties = new MaterialProperties
            {
                Key = Material
            };

            var result = new Geometry();
            result.CreateOrGetExistingSurface(matProperties)
                .BuildPyramidFullV(
                    new Vector3(0f, 0f, 0f),
                    this.Width,
                    this.Height,
                    Color4Ex.Transparent);

            return result;
        }

        public NamedOrGenericKey Material
        {
            get;
            set;
        }

        public float Width
        {
            get;
            set;
        } = 1f;

        public float Height
        {
            get;
            set;
        } = 1f;
    }
}
