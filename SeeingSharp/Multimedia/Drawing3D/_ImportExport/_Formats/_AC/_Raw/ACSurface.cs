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
using System.Collections.Generic;
using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    internal class ACSurface
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACSurface"/> class.
        /// </summary>
        public ACSurface()
        {
            VertexReferences = new List<int>();
            TextureCoordinates = new List<Vector2>();
        }

        /// <summary>
        /// Is this surface built using polygons?
        /// </summary>
        public bool IsPolygon => (Flags & 0xF0) == 0;

        /// <summary>
        /// Is this surface a closed line?
        /// </summary>
        public bool IsClosedLine => (Flags & 0xF0) == 1;

        /// <summary>
        /// Is this surface a line?
        /// </summary>
        public bool IsLine => (Flags & 0xF0) == 2;

        /// <summary>
        /// Is this surface flat shaded?
        /// </summary>
        public bool IsFlatShaded => (Flags & 16) != 16;

        /// <summary>
        /// Is this surface two sided?
        /// </summary>
        public bool IsTwoSided => (Flags & 32) == 32;

        public int Flags;
        public int Material;
        public List<Vector2> TextureCoordinates;
        public List<int> VertexReferences;
    }
}
