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

using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Objects
{
    public class GenericGeometryFactory : GeometryFactory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GenericGeometryFactory"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public GenericGeometryFactory(Geometry geometry)
        {
            this.Geometry = geometry;
            this.GeometryLowDetail = geometry;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GenericGeometryFactory"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="geometryLowDetail">The geometry for low detail level.</param>
        public GenericGeometryFactory(Geometry geometry, Geometry geometryLowDetail)
        {
            this.Geometry = geometry;
            this.GeometryLowDetail = geometryLowDetail;
        }

        /// <summary>
        /// Builds the geometry.
        /// </summary>
        /// <param name="buildOptions">Some generic options for geometry building</param>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            if (buildOptions.IsHighDetail) { return this.Geometry; }
            return this.GeometryLowDetail;
        }

        /// <summary>
        /// Applies the given material to all contained surfaces.
        /// </summary>
        /// <param name="materialToApply">The materials to apply.</param>
        public void ApplyMaterialForAll(NamedOrGenericKey materialToApply)
        {
            foreach (var actSurface in this.Geometry.Surfaces)
            {
                actSurface.Material = materialToApply;
            }

            foreach (var actSurface in this.GeometryLowDetail.Surfaces)
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
            foreach (var actSurface in this.Geometry.Surfaces)
            {
                if (actSurface.Material == materialNameOld)
                {
                    actSurface.Material = materialNameNew;
                }
            }

            foreach (var actSurface in this.GeometryLowDetail.Surfaces)
            {
                if (actSurface.Material == materialNameOld)
                {
                    actSurface.Material = materialNameNew;
                }
            }
        }

        /// <summary>
        /// Gets the containing geometry.
        /// </summary>
        public Geometry Geometry { get; }

        /// <summary>
        /// Gets the containing geometry for low detail level.
        /// </summary>
        public Geometry GeometryLowDetail { get; }
    }
}