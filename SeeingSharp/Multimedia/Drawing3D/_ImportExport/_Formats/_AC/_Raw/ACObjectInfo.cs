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
    internal class ACObjectInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ACObjectInfo"/> class.
        /// </summary>
        public ACObjectInfo()
        {
            Children = new List<ACObjectInfo>();
            Surfaces = new List<ACSurface>();
            Vertices = new List<ACVertex>();
            Rotation = Matrix4x4.Identity;
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets total count of all child objects
        /// </summary>
        public int CountAllChildObjects()
        {
            var result = 0;

            foreach (var actObj in Children)
            {
                result += actObj.CountAllChildObjects();
            }

            return result;
        }

        public List<ACObjectInfo> Children;
        public int KidCount;
        public string Name;
        public Matrix4x4 Rotation;
        public List<ACSurface> Surfaces;
        public string Texture;
        public Vector2 TextureRepeat;
        public Vector3 Translation;
        public ACObjectType Type;
        public string Url;
        public List<ACVertex> Vertices;
    }
}
