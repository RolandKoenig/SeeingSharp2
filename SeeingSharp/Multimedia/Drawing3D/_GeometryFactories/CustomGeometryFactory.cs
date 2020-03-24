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

using System;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class CustomGeometryFactory : GeometryFactory
    {
        /// <summary>
        /// Gets the containing geometry.
        /// </summary>
        public Geometry Geometry { get; }

        /// <summary>
        /// Gets the containing geometry for low detail level.
        /// </summary>
        public Geometry GeometryLowDetail { get; }

        public Func<GeometryBuildOptions, Geometry> BuilderFunction { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGeometryFactory"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public CustomGeometryFactory(Geometry geometry)
        {
            this.Geometry = geometry;
            this.GeometryLowDetail = geometry;
            this.BuilderFunction = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGeometryFactory"/> class.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="geometryLowDetail">The geometry for low detail level.</param>
        public CustomGeometryFactory(Geometry geometry, Geometry geometryLowDetail)
        {
            this.Geometry = geometry;
            this.GeometryLowDetail = geometryLowDetail;
            this.BuilderFunction = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CustomGeometryFactory"/> class.
        /// </summary>
        /// <param name="builderFunc">The function which generates the <seealso cref="Geometry"/> object.</param>
        public CustomGeometryFactory(Func<GeometryBuildOptions, Geometry> builderFunc)
        {
            this.Geometry = null;
            this.GeometryLowDetail = null;
            this.BuilderFunction = builderFunc;
        }

        /// <summary>
        /// Builds the geometry.
        /// </summary>
        /// <param name="buildOptions">Some generic options for geometry building</param>
        public override Geometry BuildGeometry(GeometryBuildOptions buildOptions)
        {
            if (this.BuilderFunction != null)
            {
                return this.BuilderFunction(buildOptions);
            }

            if (buildOptions.IsHighDetail) { return this.Geometry; }
            return this.GeometryLowDetail;
        }
    }
}