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

using System.Runtime.InteropServices;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public static readonly Vertex Empty;

        private GeometryData m_geoData;
        private TextureData m_textureData;

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position)
        {
            m_geoData = new GeometryData(position);
            m_textureData = new TextureData();
            Animation = new AnimationData();
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color)
        {
            m_geoData = new GeometryData(position, color);
            m_textureData = new TextureData();
            Animation = new AnimationData();
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color, Vector2 texCoord1)
        {
            m_geoData = new GeometryData(position, color);
            m_textureData = new TextureData(texCoord1);
            Animation = new AnimationData();
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color, Vector2 texCoord1, Vector3 normal)
        {
            m_geoData = new GeometryData(position, normal, color);
            m_textureData = new TextureData(texCoord1);
            Animation = new AnimationData();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return m_geoData.Position.ToString();
        }

        /// <summary>
        /// Copies this vertex and sets the new position
        /// </summary>
        public Vertex Copy(Vector3 newPosition)
        {
            var result = this;
            result.m_geoData = result.m_geoData.Copy(newPosition);
            return result;
        }

        /// <summary>
        /// Copies this vertex and changes the texture coordinate of the result.
        /// </summary>
        /// <param name="actTexCoord">The texture coordinate to be set.</param>
        public Vertex Copy(Vector2 actTexCoord)
        {
            var result = this;
            result.m_textureData.Coordinate1 = actTexCoord;
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
            result.m_geoData = result.m_geoData.Copy(newPosition);
            result.m_textureData = result.m_textureData.Copy(newTexCoord1);
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets a new position, normal and texture coordinate
        /// </summary>
        public Vertex Copy(Vector3 newPosition, Vector3 newNormal, Vector2 newTexCoord1)
        {
            var result = this;
            result.m_geoData = result.m_geoData.Copy(newPosition, newNormal);
            result.m_textureData = result.m_textureData.Copy(newTexCoord1);
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets the new values
        /// </summary>
        public Vertex Copy(Vector3 newPosition, Vector3 newNormal)
        {
            var result = this;
            result.m_geoData = result.m_geoData.Copy(newPosition, newNormal);
            return result;
        }

        /// <summary>
        /// Retrieves or sets geometry data
        /// </summary>
        public GeometryData Geometry
        {
            get => m_geoData;
            set => m_geoData = value;
        }

        /// <summary>
        /// Gets or sets all animation related data of the vertex.
        /// </summary>
        public AnimationData Animation { get; set; }

        /// <summary>
        /// Retrieves or sets texture data
        /// </summary>
        public TextureData Texture
        {
            get => m_textureData;
            set => m_textureData = value;
        }

        /// <summary>
        /// Gets or sets the position
        /// </summary>
        public Vector3 Position
        {
            get => m_geoData.Position;
            set => m_geoData.Position = value;
        }

        /// <summary>
        /// Gets or sets the normal
        /// </summary>
        public Vector3 Normal
        {
            get => m_geoData.Normal;
            set => m_geoData.Normal = value;
        }

        /// <summary>
        /// Gets or sets the tangent vector.
        /// </summary>
        public Vector3 Tangent
        {
            get => m_geoData.Tangent;
            set => m_geoData.Tangent = value;
        }

        /// <summary>
        /// Gets or sets the binormal vector.
        /// </summary>
        public Vector3 Binormal
        {
            get => m_geoData.Binormal;
            set => m_geoData.Binormal = value;
        }

        /// <summary>
        /// Gets or sets the texture coordinate
        /// </summary>
        public Vector2 TexCoord
        {
            get => m_textureData.Coordinate1;
            set => m_textureData.Coordinate1 = value;
        }

        /// <summary>
        /// Gets or sets the texture factor.
        /// This value decides whether a texture is displayed on this vertex or not.
        /// A value greater or equal 0 will show the texture, all negatives will hide it.
        /// </summary>
        public float TextureFactor
        {
            get => m_textureData.TextureFactor;
            set => m_textureData.TextureFactor = value;
        }

        /// <summary>
        /// Gets or sets the diffuse color
        /// </summary>
        public Color4 Color
        {
            get => m_geoData.Color;
            set => m_geoData.Color = value;
        }
    }
}