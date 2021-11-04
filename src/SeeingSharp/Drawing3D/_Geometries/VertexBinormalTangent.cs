using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Drawing3D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexBinormalTangent
    {
        public static readonly VertexBinormalTangent Empty;

        public override string ToString()
        {
            return $"Tangent: {Tangent} ; Binormal: {Binormal}";
        }

        /// <summary>
        /// Gets or sets the tangent vector.
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// Gets or sets the binormal vector.
        /// </summary>
        public Vector3 Binormal;
    }
}