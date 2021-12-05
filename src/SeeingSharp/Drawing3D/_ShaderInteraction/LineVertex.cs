using System.Numerics;
using System.Runtime.InteropServices;
using DXGI = Vortice.DXGI;
using D3D11 = Vortice.Direct3D11;

namespace SeeingSharp.Drawing3D
{
    /// <summary>
    /// A structure for sending line vertex data to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal struct LineVertex
    {
        // Constants
        public static readonly int Size = Marshal.SizeOf<LineVertex>();
        public static readonly D3D11.InputElementDescription[] InputElements = {
            new D3D11.InputElementDescription("POSITION", 0, DXGI.Format.R32G32B32_Float, 0, 0)
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
