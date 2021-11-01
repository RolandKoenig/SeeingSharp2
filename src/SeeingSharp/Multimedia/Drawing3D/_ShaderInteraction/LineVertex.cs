using System.Numerics;
using System.Runtime.InteropServices;
using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// A structure for sending line vertex data to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LineVertex
    {
        // Constants
        public static readonly int Size = Marshal.SizeOf<LineVertex>();
        public static readonly D3D11.InputElement[] InputElements = {
            new D3D11.InputElement("POSITION", 0, Format.R32G32B32_Float, 0, 0)
        };

        // Vertex data
        public Vector3 Position;

        /// <summary>
        /// Initializes a new instance of the <see cref="LineVertex" /> struct.
        /// </summary>
        /// <param name="position">The position.</param>
        public LineVertex(Vector3 position)
        {
            Position = position;
        }
    }
}
