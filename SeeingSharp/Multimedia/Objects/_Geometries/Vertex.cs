/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp.Multimedia.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public static readonly Vertex Empty;

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = Color4.Transparent;
            TexCoord1 = Vector2.Zero;
            TextureFactor = 0f;

            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = color;
            TexCoord1 = Vector2.Zero;
            TextureFactor = 0f;

            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color, Vector2 texCoord1)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = color;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;

            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Vector2 texCoord1)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = Color4.Transparent;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;

            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color, Vector2 texCoord1, Vector3 normal)
        {
            Position = position;
            Normal = normal;
            Color = color;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;

            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Vector2 texCoord1, Vector3 normal)
        {
            Position = position;
            Normal = normal;
            Color = Color4.Transparent;
            TexCoord1 = texCoord1;
            TextureFactor = 0f;

            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        public override string ToString()
        {
            return Position.ToString();
        }

        /// <summary>
        /// Copies this vertex and sets the new position
        /// </summary>
        public Vertex Copy(Vector3 newPosition)
        {
            var result = this;
            result.Position = newPosition;
            return result;
        }

        /// <summary>
        /// Copies this vertex and changes the texture coordinate of the result.
        /// </summary>
        /// <param name="newTexCoord1">The texture coordinate to be set.</param>
        public Vertex Copy(Vector2 newTexCoord1)
        {
            var result = this;
            result.TexCoord1 = newTexCoord1;
            return result;
        }

        internal Vertex Copy(Color4 Color4)
        {
            var result = this;
            result.Color = Color4;
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets a new position and texture coordinate
        /// </summary>
        public Vertex Copy(Vector3 newPosition, Vector2 newTexCoord1)
        {
            var result = this;
            result.Position = newPosition;
            result.TexCoord1 = newTexCoord1;
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets a new position, normal and texture coordinate
        /// </summary>
        public Vertex Copy(Vector3 newPosition, Vector3 newNormal, Vector2 newTexCoord1)
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
        public Vertex Copy(Vector3 newPosition, Vector3 newNormal)
        {
            var result = this;
            result.Position = newPosition;
            result.Normal = newNormal;
            return result;
        }

        /// <summary>
        /// Subdivides two vertices.
        /// </summary>
        public static void SubdivideVertices(ref Vertex v0, ref Vertex v1, out Vertex m0)
        {
            m0 = new Vertex();
            m0.Position = (v0.Position + v1.Position) * 0.5f;
            m0.Normal = Vector3.Normalize((v0.Normal + v1.Normal) * 0.5f);
            m0.TexCoord1 = (v0.TexCoord1 + v1.TexCoord1) * 0.5f;
            m0.Color = (v0.Color + v1.Color) * 0.5f;
            m0.TextureFactor = (v0.TextureFactor + v1.TextureFactor) * 0.5f;
            m0.Binormal = Vector3.Normalize((v0.Binormal + v1.Binormal) * 0.5f);
            m0.Tangent = Vector3.Normalize((v0.Tangent + v1.Tangent) * 0.5f);
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
        /// Gets or sets the tangent vector.
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// Gets or sets the binormal vector.
        /// </summary>
        public Vector3 Binormal;

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