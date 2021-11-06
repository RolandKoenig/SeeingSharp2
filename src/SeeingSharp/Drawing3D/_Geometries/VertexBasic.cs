using System.Numerics;
using System.Runtime.InteropServices;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Drawing3D
{
    [StructLayout(LayoutKind.Sequential)]
    public struct VertexBasic
    {
        public static readonly VertexBasic Empty;

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public VertexBasic(Vector3 position)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = Color4.Transparent;
            TexCoord1 = Vector2.Zero;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public VertexBasic(Vector3 position, Color4 color)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = color;
            TexCoord1 = Vector2.Zero;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public VertexBasic(Vector3 position, Color4 color, Vector2 texCoord1)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = color;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public VertexBasic(Vector3 position, Vector2 texCoord1)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = Color4.Transparent;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public VertexBasic(Vector3 position, Color4 color, Vector2 texCoord1, Vector3 normal)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public VertexBasic(Vector3 position, Vector2 texCoord1, Vector3 normal)
        {
            Position = position;
            Normal = normal;
            Color = Color4.Transparent;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;
        }

        public override string ToString()
        {
            return Position.ToString();
        }

        /// <summary>
        /// Copies this vertex and sets the new position
        /// </summary>
        public VertexBasic Copy(Vector3 newPosition)
        {
            var result = this;
            result.Position = newPosition;
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets a new position and texture coordinate
        /// </summary>
        public VertexBasic Copy(Vector3 newPosition, Vector2 newTexCoord1)
        {
            var result = this;
            result.Position = newPosition;
            result.TexCoord1 = newTexCoord1;
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets a new position, normal and texture coordinate
        /// </summary>
        public VertexBasic Copy(Vector3 newPosition, Vector3 newNormal, Vector2 newTexCoord1)
        {
            var result = this;
            result.Position = newPosition;
            result.Normal = newNormal;
            result.TexCoord1 = newTexCoord1;
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets the new values
        /// </summary>
        public VertexBasic Copy(Vector3 newPosition, Vector3 newNormal)
        {
            var result = this;
            result.Position = newPosition;
            result.Normal = newNormal;
            return result;
        }

        /// <summary>
        /// Subdivides two vertices.
        /// </summary>
        public static void SubdivideVertices(ref VertexBasic v0, ref VertexBasic v1, out VertexBasic m0)
        {
            m0 = new VertexBasic();
            m0.Position = (v0.Position + v1.Position) * 0.5f;
            m0.Normal = Vector3.Normalize((v0.Normal + v1.Normal) * 0.5f);
            m0.TexCoord1 = (v0.TexCoord1 + v1.TexCoord1) * 0.5f;
            m0.Color = (v0.Color + v1.Color) * 0.5f;
            m0.TextureFactor = (v0.TextureFactor + v1.TextureFactor) * 0.5f;
        }

        /// <summary>
        /// Retrieves or sets the position of the vertex
        /// </summary>
        public Vector3 Position;

        /// <summary>
        /// Retrieves or sets the normal of the vertex
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Retrieves or sets the color of the vertex
        /// </summary>
        public Color4 Color;

        /// <summary>
        /// Gets or sets the texture factor.
        /// This value decides whether a texture is displayed on this vertex or not.
        /// A value greater or equal 0 will show the texture, all negatives will hide it.
        /// </summary>
        public float TextureFactor;

        /// <summary>
        /// Retrieves or sets first texture coordinate
        /// </summary>
        public Vector2 TexCoord1;
    }
}