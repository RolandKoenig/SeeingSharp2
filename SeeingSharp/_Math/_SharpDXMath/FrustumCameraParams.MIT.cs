using System;
using System.Runtime.InteropServices;
using System.Numerics;

namespace SeeingSharp
{
    /// <summary>
    /// Frustum camera parameters.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    public struct FrustumCameraParams
    {
        /// <summary>
        /// Position of the camera.
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Looking at direction of the camera.
        /// </summary>
        public Vector3 LookAtDir;

        /// <summary>
        /// Up direction.
        /// </summary>
        public Vector3 UpDir;

        /// <summary>
        /// Field of view.
        /// </summary>
        public float FOV;

        /// <summary>
        /// Z near distance.
        /// </summary>
        public float ZNear;

        /// <summary>
        /// Z far distance.
        /// </summary>
        public float ZFar;

        /// <summary>
        /// Aspect ratio.
        /// </summary>
        public float AspectRatio;
    }
}