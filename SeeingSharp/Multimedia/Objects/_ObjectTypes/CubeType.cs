﻿#region License information
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

    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public class CubeType : ObjectType
    {
        public CubeType()
        {
        }

        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            var matProperties = new MaterialProperties
            {
                Key = Material
            };

            var size = Size;
            var halfSize = size / 2f;

            var result = new VertexStructure();
            result.CreateOrGetExistingSurface(matProperties)
                .BuildCube24V(
                    new Vector3(-halfSize, -halfSize, -halfSize),
                    new Vector3(size, size, size),
                    Color4Ex.Transparent);

            return result;
        }

        public NamedOrGenericKey Material
        {
            get;
            set;
        }

        public float Size
        {
            get;
            set;
        } = 1f;
    }
}