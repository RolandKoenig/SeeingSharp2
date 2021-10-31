/*
    SeeingSharp and all applications distributed together with it. 
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
using System.Numerics;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class ImportOptions
    {
        /// <summary>
        /// The resource may have a different coordinate system.
        /// This property ensures that the coordinate system is mapped correctly to the one that
        /// SeeingSharp uses. Default is LeftHanded_UpY.
        /// </summary>
        public CoordinateSystem ResourceCoordinateSystem
        {
            get;
            set;
        }

        /// <summary>
        /// True if the model should be scaled and translated so it fits in a cube with a side with of 1.
        /// </summary>
        public bool FitToCube
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImportOptions"/> class.
        /// </summary>
        public ImportOptions()
        {
            this.ResourceCoordinateSystem = CoordinateSystem.LeftHanded_UpY;
            this.FitToCube = true;
        }

        /// <summary>
        /// Gets the transform matrix for coordinate system.
        /// </summary>
        /// <returns></returns>
        public Matrix4x4 GetTransformMatrixForCoordinateSystem()
        {
            switch (this.ResourceCoordinateSystem)
            {
                case CoordinateSystem.LeftHanded_UpY:
                    return Matrix4x4.Identity;

                case CoordinateSystem.LeftHanded_UpZ:
                    return
                        Matrix4x4.CreateScale(1f, -1f, 1f) *
                        Matrix4x4.CreateRotationX(-EngineMath.RAD_90DEG);

                case CoordinateSystem.RightHanded_UpY:
                    return Matrix4x4.CreateScale(new Vector3(1f, 1f, -1f));

                case CoordinateSystem.RightHanded_UpZ:
                    return
                        Matrix4x4.CreateRotationX(-EngineMath.RAD_90DEG);
            }

            return Matrix4x4.Identity;
        }

        /// <summary>
        /// Should triangle order be changes by the import logic?
        /// </summary>
        public bool IsChangeTriangleOrderNeeded()
        {
            switch (this.ResourceCoordinateSystem)
            {
                case CoordinateSystem.LeftHanded_UpY:
                case CoordinateSystem.RightHanded_UpZ:
                    return false;

                case CoordinateSystem.LeftHanded_UpZ:
                case CoordinateSystem.RightHanded_UpY:
                    return true;

                default:
                    throw new SeeingSharpGraphicsException($"Unknown coordinate system {this.ResourceCoordinateSystem}!");
            }
        }
    }
}
