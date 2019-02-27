#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using System.Runtime.InteropServices;
    using SharpDX;

    #endregion

    [StructLayout(LayoutKind.Sequential)]
    public struct GeometryData
    {
        private Vector3 m_position;
        private Vector3 m_normal;
        private Vector3 m_tangent;
        private Vector3 m_binormal;
        private Color4 m_color;

        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position)
        {
            m_position = position;
            m_normal = Vector3.Zero;
            m_tangent = Vector3.Zero;
            m_color = Color4.White;
            m_tangent = Vector3.Zero;
            m_binormal = Vector3.Zero;
        }

        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position, Color4 color)
        {
            m_position = position;
            m_normal = Vector3.Zero;
            m_color = color;
            m_tangent = Vector3.Zero;
            m_binormal = Vector3.Zero;
        }

        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position, Vector3 normal, Color4 color)
        {
            m_position = position;
            m_normal = normal;
            m_color = color;
            m_tangent = Vector3.Zero;
            m_binormal = Vector3.Zero;
        }

        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position, Vector3 normal)
        {
            m_position = position;
            m_normal = normal;
            m_color = Color4.White;
            m_tangent = Vector3.Zero;
            m_binormal = Vector3.Zero;
        }

        /// <summary>
        /// Copies this structure and replaces the given values
        /// </summary>
        public GeometryData Copy(Vector3 newPosition)
        {
            var result = this;
            result.m_position = newPosition;
            return result;
        }

        /// <summary>
        /// Copies this structure and replaces the given values
        /// </summary>
        public GeometryData Copy(Vector3 newPosition, Vector3 newNormal)
        {
            var result = this;
            result.m_position = newPosition;
            result.m_normal = newNormal;
            return result;
        }

        /// <summary>
        /// Retrieves or sets the position of the vertex
        /// </summary>
        public Vector3 Position
        {
            get { return m_position; }
            set { m_position = value; }
        }

        /// <summary>
        /// Retrieves or sets the normal of the vertex
        /// </summary>
        public Vector3 Normal
        {
            get { return m_normal; }
            set { m_normal = value; }
        }

        /// <summary>
        /// Retrieves or sets the color of the vertex
        /// </summary>
        public Color4 Color
        {
            get { return m_color; }
            set { m_color = value; }
        }

        /// <summary>
        /// Gets or sets the tangent vector.
        /// </summary>
        public Vector3 Tangent
        {
            get { return m_tangent; }
            set { m_tangent = value; }
        }

        /// <summary>
        /// Gets or sets the binormal vector.
        /// </summary>
        public Vector3 Binormal
        {
            get { return m_binormal; }
            set { m_binormal = value; }
        }
    }
}