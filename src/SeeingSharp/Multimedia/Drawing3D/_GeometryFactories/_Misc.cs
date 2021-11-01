using System;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public struct GeometryBuildOptions
    {
        public GeometryBuildOptions(DetailLevel detailLevel)
        {
            this.TargetDetailLevel = detailLevel;
        }

        public DetailLevel TargetDetailLevel { get; }

        public bool IsHighDetail => (this.TargetDetailLevel & DetailLevel.High) == DetailLevel.High;
    }

    [Flags]
    public enum ExtrudeGeometryOptions
    {
        None = 0,

        /// <summary>
        /// Changes the origin of the geometry so that it is in the center.
        /// </summary>
        ChangeOriginToCenter = 1,

        /// <summary>
        /// Scales the geometry so that it has the size 1 x 1.
        /// </summary>
        RescaleToUnitSize = 2
    }
}