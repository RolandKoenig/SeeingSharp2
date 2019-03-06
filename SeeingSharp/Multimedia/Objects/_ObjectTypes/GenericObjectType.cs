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

    using SeeingSharp.Util;

    #endregion

    public class GenericObjectType : ObjectType
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericObjectType"/> class.
        /// </summary>
        /// <param name="vertexStructure">The vertex structures.</param>
        public GenericObjectType(VertexStructure vertexStructure)
        {
            VertexStructure = vertexStructure;
            VertexStructureLowDetail = vertexStructure;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericObjectType"/> class.
        /// </summary>
        /// <param name="vertexStructure">The vertex structures.</param>
        /// <param name="vertexStructureLowDetail">The vertex structures for low detail level.</param>
        public GenericObjectType(VertexStructure vertexStructure, VertexStructure vertexStructureLowDetail)
        {
            VertexStructure = vertexStructure;
            VertexStructureLowDetail = vertexStructureLowDetail;
        }

        /// <summary>
        /// Builds the structure.
        /// </summary>
        /// <param name="buildOptions">Some generic options for structure building</param>
        public override VertexStructure BuildStructure(StructureBuildOptions buildOptions)
        {
            if (buildOptions.IsHighDetail)
            {
                return VertexStructure;
            }

            return VertexStructureLowDetail;
        }

        /// <summary>
        /// Applies the given material to all contained vertex structures.
        /// </summary>
        /// <param name="materialToApply">The materials to apply.</param>
        public void ApplyMaterialForAll(NamedOrGenericKey materialToApply)
        {
            foreach (var actSurface in VertexStructure.Surfaces)
            {
                actSurface.Material = materialToApply;
            }

            foreach (var actSurface in VertexStructureLowDetail.Surfaces)
            {
                actSurface.Material = materialToApply;
            }
        }

        /// <summary>
        /// Converts all occurrences of the given material to another material.
        /// </summary>
        /// <param name="materialNameOld">The material to be converted.</param>
        /// <param name="materialNameNew">The new material to be converted to.</param>
        public void ConvertMaterial(NamedOrGenericKey materialNameOld, NamedOrGenericKey materialNameNew)
        {
            foreach (var actSurface in VertexStructure.Surfaces)
            {
                if (actSurface.Material == materialNameOld)
                {
                    actSurface.Material = materialNameNew;
                }
            }

            foreach (var actSurface in VertexStructureLowDetail.Surfaces)
            {
                if (actSurface.Material == materialNameOld)
                {
                    actSurface.Material = materialNameNew;
                }
            }
        }

        /// <summary>
        /// Gets the array containing all loaded vertex structures.
        /// </summary>
        public VertexStructure VertexStructure { get; }

        /// <summary>
        /// Gets an array containing all loaded vertex structures for low detail level.
        /// </summary>
        public VertexStructure VertexStructureLowDetail { get; }
    }
}