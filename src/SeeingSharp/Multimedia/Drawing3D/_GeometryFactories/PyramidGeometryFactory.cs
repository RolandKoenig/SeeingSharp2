﻿using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class PyramidGeometryFactory : GeometryFactory
    {
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

        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            var result = new Geometry();
            result.CreateSurface()
                .BuildPyramidFullV(
                    new Vector3(0f, 0f, 0f),
                    this.Width,
                    this.Height);

            return result;
        }
    }
}
