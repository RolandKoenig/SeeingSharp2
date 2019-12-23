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
    public struct GeometryData
    {
        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position)
        {
            Position = position;
            Normal = Vector3.Zero;
            Tangent = Vector3.Zero;
            Color = Color4.White;
            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position, Color4 color)
        {
            Position = position;
            Normal = Vector3.Zero;
            Color = color;
            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position, Vector3 normal, Color4 color)
        {
            Position = position;
            Normal = normal;
            Color = color;
            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Initializes a new geometry data structure
        /// </summary>
        public GeometryData(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
            Color = Color4.White;
            Tangent = Vector3.Zero;
            Binormal = Vector3.Zero;
        }

        /// <summary>
        /// Copies this structure and replaces the given values
        /// </summary>
        public GeometryData Copy(Vector3 newPosition)
        {
            var result = this;
            result.Position = newPosition;
            return result;
        }

        /// <summary>
        /// Copies this structure and replaces the given values
        /// </summary>
        public GeometryData Copy(Vector3 newPosition, Vector3 newNormal)
        {
            var result = this;
            result.Position = newPosition;
            result.Normal = newNormal;
            return result;
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
    }
}