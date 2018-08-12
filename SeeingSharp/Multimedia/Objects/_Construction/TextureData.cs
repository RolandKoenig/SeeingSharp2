#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
    public struct TextureData
    {
        private Vector2 m_coordiante1;
        private float m_textureFactor;

        /// <summary>
        /// Initializes a new TextureData structure
        /// </summary>
        public TextureData(Vector2 coord1)
        {
            m_coordiante1 = coord1;
            m_textureFactor = 0f;
        }

        /// <summary>
        /// Copies this structure and changes some data
        /// </summary>
        public TextureData Copy(Vector2 newCoord1)
        {
            TextureData result = this;
            result.m_coordiante1 = newCoord1;
            return result;
        }

        /// <summary>
        /// Gets or sets the texture factor.
        /// This value decides wether a texture is displayed on this vertex or not.
        /// A value greater or equal 0 will show the texture, all negatives will hide it.
        /// </summary>
        public float TextureFactor
        {
            get { return m_textureFactor; }
            set { m_textureFactor = value; }
        }

        /// <summary>
        /// Retrieves or sets first texture coordinate
        /// </summary>
        public Vector2 Coordinate1
        {
            get { return m_coordiante1; }
            set { m_coordiante1 = value; }
        }
    }
}