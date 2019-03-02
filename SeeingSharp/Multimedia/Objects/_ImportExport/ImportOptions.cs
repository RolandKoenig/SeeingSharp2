#region License information
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
#endregion

using SeeingSharp.Multimedia.Core;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    #region using
    #endregion

    public class ImportOptions
    {
        /// <summary>
        /// Gets the transform matrix for coordinate system.
        /// </summary>
        /// <returns></returns>
        public Matrix GetTransformMatrixForCoordinateSystem()
        {
            switch (ResourceCoordinateSystem)
            {
                case CoordinateSystem.LeftHanded_UpY:
                    return Matrix.Identity;

                case CoordinateSystem.LeftHanded_UpZ:
                    return
                        Matrix.Scaling(1f, -1f, 1f) *
                        Matrix.RotationX(-EngineMath.RAD_90DEG); ;

                case CoordinateSystem.RightHanded_UpY:
                    return Matrix.Scaling(new Vector3(1f, 1f, -1f));

                case CoordinateSystem.RightHanded_UpZ:
                    return
                        Matrix.RotationX(-EngineMath.RAD_90DEG);
            }

            return Matrix.Identity;
        }

        /// <summary>
        /// Should triangle order be changes by the import logic?
        /// </summary>
        public bool IsChangeTriangleOrderNeeded()
        {
            switch (ResourceCoordinateSystem)
            {
                case CoordinateSystem.LeftHanded_UpY:
                case CoordinateSystem.RightHanded_UpZ:
                    return false;

                case CoordinateSystem.LeftHanded_UpZ:
                case CoordinateSystem.RightHanded_UpY:
                    return true;

                default:
                    throw new SeeingSharpGraphicsException(string.Format(
                        "Unknown coordinate system {0}!",
                        ResourceCoordinateSystem));
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportOptions"/> class.
        /// </summary>
        public ImportOptions()
        {
            ResourceCoordinateSystem = CoordinateSystem.LeftHanded_UpY;
            ResizeFactor = 1f;
            TwoSidedSurfaces = false;
        }

        /// <summary>
        /// Gets or sets the resize factor.
        /// This is needed to transform coordinate from one measure unit to another.
        /// Default is 1.
        /// </summary>
        public float ResizeFactor
        {
            get;
            set;
        }

        /// <summary>
        /// The resource may have a different coordinate system.
        /// This property ensures that the coordinate system is mapped correctly to the one that
        /// Seeing# uses. Default is LeftHanded_UpY.
        /// </summary>
        public CoordinateSystem ResourceCoordinateSystem
        {
            get;
            set;
        }

        /// <summary>
        /// Needed some times. This generates a front and a back side for each loaded surface.
        /// Default is false.
        /// </summary>
        public bool TwoSidedSurfaces
        {
            get;
            set;
        }

        public bool ToggleTriangleIndexOrder
        {
            get;
            set;
        }
    }
}
