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

namespace SeeingSharp.Multimedia.Objects._ObjectTypes
{
    #region using

    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class CuboidType : ObjectType
    {
        public CuboidType()
        {
        }

        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            var matProperties = new MaterialProperties
            {
                Key = Material
            };

            var width = Width;
            var halfWidth = width / 2f;

            var height = Height;
            var halfHeight = height / 2f;

            var length = Length;
            var halfLength = length / 2f;

            var result = new VertexStructure();

            result.CreateOrGetExistingSurface(matProperties)
                .BuildCuboidFullV(
                    new Vector3(0, 0, 0),
                    new Vector3(width, 0, 0),
                    new Vector3(0, height, 0),
                    new Vector3(0, 0, length),
                    Color4Ex.Transparent);

            return result;
        }

        /// <summary>
        /// Material of the Cuboid
        /// </summary>
        public NamedOrGenericKey Material
        {
            get;
            set;
        }

        /// <summary>
        /// Width of the Cuboid
        /// </summary>
        public float Width
        {
            get;
            set;
        } = 2f;

        /// <summary>
        /// Length of the Cuboid
        /// </summary>
        public float Length
        {
            get;
            set;
        } = 3f;

        /// <summary>
        /// Height of the Cuboid
        /// </summary>
        public float Height
        {
            get;
            set;
        } = 1f;
    }
}