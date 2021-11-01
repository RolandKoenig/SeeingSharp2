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
