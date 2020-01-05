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
using SeeingSharp.Multimedia.Core;
using System;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public struct GeometryBuildOptions
    {
        public GeometryBuildOptions(DetailLevel detailLevel)
        {
            this.TargetDetailLevel = detailLevel;
        }

        public DetailLevel TargetDetailLevel { get; }

        public bool IsHighDetail => (this.TargetDetailLevel & DetailLevel.High) == DetailLevel.High;
    }

    [Flags]
    public enum ExtrudeGeometryOptions
    {
        None = 0,

        /// <summary>
        /// Changes the origin of the geometry so that it is in the center.
        /// </summary>
        ChangeOriginToCenter = 1,

        /// <summary>
        /// Scales the geometry so that it has the size 1 x 1.
        /// </summary>
        RescaleToUnitSize = 2
    }
}