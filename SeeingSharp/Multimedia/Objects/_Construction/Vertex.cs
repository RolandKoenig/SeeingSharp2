#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
using System.Runtime.InteropServices;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Vertex
    {
        public static readonly Vertex Empty = new Vertex();

        private GeometryData m_geoData;
        private TextureData m_textureData;
        private AnimationData m_animationData;

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position)
        {
            m_geoData = new GeometryData(position);
            m_textureData = new TextureData();
            m_animationData = new AnimationData();
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color)
        {
            m_geoData = new GeometryData(position, color);
            m_textureData = new TextureData();
            m_animationData = new AnimationData();
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color, Vector2 texCoord1)
        {
            m_geoData = new GeometryData(position, color);
            m_textureData = new TextureData(texCoord1);
            m_animationData = new AnimationData();
        }

        /// <summary>
        /// Creates a new vertex
        /// </summary>
        public Vertex(Vector3 position, Color4 color, Vector2 texCoord1, Vector3 normal)
        {
            m_geoData = new GeometryData(position, normal, color);
            m_textureData = new TextureData(texCoord1);
            m_animationData = new AnimationData();
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
            Vertex result = this;
            result.m_geoData = result.m_geoData.Copy(newPosition);
            return result;
        }

        /// <summary>
        /// Copies this vertex and changes the texture coordinate of the result.
        /// </summary>
        /// <param name="actTexCoord">The texture coordinate to be set.</param>
        public Vertex Copy(Vector2 actTexCoord)
        {
            Vertex result = this;
            result.m_textureData.Coordinate1 = actTexCoord;
            return result;
        }

        internal Vertex Copy(Color4 Color4)
        {
            Vertex result = this;
            result.Color = Color4;
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets a new position and texture coordinate
        /// </summary>
        public Vertex Copy(Vector3 newPosition, Vector2 newTexCoord1)
        {
            Vertex result = this;
            result.m_geoData = result.m_geoData.Copy(newPosition);
            result.m_textureData = result.m_textureData.Copy(newTexCoord1);
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets a new position, normal and texture coordinate
        /// </summary>
        public Vertex Copy(Vector3 newPosition, Vector3 newNormal, Vector2 newTexCoord1)
        {
            Vertex result = this;
            result.m_geoData = result.m_geoData.Copy(newPosition, newNormal);
            result.m_textureData = result.m_textureData.Copy(newTexCoord1);
            return result;
        }

        /// <summary>
        /// Copies this vertex and sets the new values
        /// </summary>
        public Vertex Copy(Vector3 newPosition, Vector3 newNormal)
        {
            Vertex result = this;
            result.m_geoData = result.m_geoData.Copy(newPosition, newNormal);
            return result;
        }

        /// <summary>
        /// Retrieves or sets geometry data
        /// </summary>
        public GeometryData Geometry
        {
            get { return m_geoData; }
            set { m_geoData = value; }
        }

        /// <summary>
        /// Gets or sets all animation related data of the vertex.
        /// </summary>
        public AnimationData Animation
        {
            get { return m_animationData; }
            set { m_animationData = value; }
        }

        /// <summary>
        /// Retrieves or sets texture data
        /// </summary>
        public TextureData Texture
        {
            get { return m_textureData; }
            set { m_textureData = value; }
        }

        /// <summary>
        /// Gets or sets the position
        /// </summary>
        public Vector3 Position
        {
            get { return m_geoData.Position; }
            set { m_geoData.Position = value; }
        }

        /// <summary>
        /// Gets or sets the normal
        /// </summary>
        public Vector3 Normal
        {
            get { return m_geoData.Normal; }
            set { m_geoData.Normal = value; }
        }

        /// <summary>
        /// Gets or sets the tangent vector.
        /// </summary>
        public Vector3 Tangent
        {
            get { return m_geoData.Tangent; }
            set { m_geoData.Tangent = value; }
        }

        /// <summary>
        /// Gets or sets the binormal vector.
        /// </summary>
        public Vector3 Binormal
        {
            get { return m_geoData.Binormal; }
            set { m_geoData.Binormal = value; }
        }

        /// <summary>
        /// Gets or sets the texture coordinate
        /// </summary>
        public Vector2 TexCoord
        {
            get { return m_textureData.Coordinate1; }
            set { m_textureData.Coordinate1 = value; }
        }

        /// <summary>
        /// Gets or sets the texture factor.
        /// This value decides wether a texture is displayed on this vertex or not.
        /// A value greater or equal 0 will show the texture, all negatives will hide it.
        /// </summary>
        public float TextureFactor
        {
            get { return m_textureData.TextureFactor; }
            set { m_textureData.TextureFactor = value; }
        }

        /// <summary>
        /// Gets or sets the diffuse color
        /// </summary>
        public Color4 Color
        {
            get { return m_geoData.Color; }
            set { m_geoData.Color = value; }
        }
    }
}