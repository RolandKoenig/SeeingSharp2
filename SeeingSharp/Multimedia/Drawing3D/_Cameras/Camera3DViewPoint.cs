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
using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    /// <summary>
    /// This class represents a specific viewpoint within the 3D world.
    /// </summary>
    public class Camera3DViewPoint
    {
        public Camera3DType CameraType
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the CameraType in string form.
        /// </summary>
        public string CameraTypeString
        {
            get => this.CameraType.ToString();
            set
            {
                if (Enum.TryParse(value, out Camera3DType valueParsed))
                {
                    this.CameraType = valueParsed;
                }
            }
        }

        /// <summary>
        /// Gets or sets the position of the ViewPoint.
        /// </summary>
        public Vector3 Position
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the rotation of the ViewPoint.
        /// </summary>
        public Vector2 Rotation
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the zoom factor if we have a orthographic camera.
        /// </summary>
        public float OrthographicZoomFactor
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera3DViewPoint"/> class.
        /// </summary>
        public Camera3DViewPoint()
        {
            this.OrthographicZoomFactor = 10f;
            this.CameraType = Camera3DType.Perspective;
        }
    }
}
