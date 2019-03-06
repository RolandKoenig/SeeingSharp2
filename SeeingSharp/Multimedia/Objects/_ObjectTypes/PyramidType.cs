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

    public class PyramidType : ObjectType
    {
        public PyramidType()
        {
        }

        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            var matProperties = new MaterialProperties
            {
                Key = Material
            };

            var width = Width;
            var height = Height;
            var halfSize = width / 2f;
            var result = new VertexStructure();

            result.CreateOrGetExistingSurface(matProperties)
                .BuildPyramidFullV(
                    new Vector3(-halfSize, -halfSize, -halfSize),
                    new Vector3(width, width, width),
                    height,
                    Color4Ex.Transparent);

            return result;
        }

        public NamedOrGenericKey Material
        {
            get;
            set;
        }

        public float Width
        {
            get;
            set;
        } = 1f;

        public float Height
        {
            get;
            set;
        } = 1f;
    }
}