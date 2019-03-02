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
namespace SeeingSharp.Multimedia.Drawing3D
{
    #region using

    using System;
    using SharpDX;

    #endregion

    public class OrthographicCamera3D : Camera3DBase
    {
        #region Constants
        private const float ZOOM_FACTOR_MIN = 0.1f;
        #endregion

        #region Configuration
        private float m_zoomFactor = 10f;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="OrthographicCamera3D"/> class.
        /// </summary>
        public OrthographicCamera3D()
            : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OrthographicCamera3D"/> class.
        /// </summary>
        /// <param name="width">Width of the renderwindow.</param>
        /// <param name="height">Height of the renderwindow.</param>
        public OrthographicCamera3D(int width, int height)
            : base(width, height)
        {
        }

        /// <summary>
        /// Applies the data from the given ViewPoint object.
        /// </summary>
        /// <param name="camera3DViewPoint">The ViewPoint to be applied.</param>
        public override void ApplyViewPoint(Camera3DViewPoint camera3DViewPoint)
        {
            base.ApplyViewPoint(camera3DViewPoint);

            m_zoomFactor = camera3DViewPoint.OrthographicZoomFactor;
        }

        /// <summary>
        /// Gets a ViewPoint out of this camera object.
        /// </summary>
        public override Camera3DViewPoint GetViewPoint()
        {
            var result = base.GetViewPoint();
            result.OrthographicZoomFactor = m_zoomFactor;
            return result;
        }

        /// <summary>
        /// Calculates the view and projection matrix for this camera.
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="target">The target point of the camera.</param>
        /// <param name="upVector">The current up vector.</param>
        /// <param name="zNear">Distance to the nearest rendered pixel.</param>
        /// <param name="zFar">Distance to the farest rendered pixel.</param>
        /// <param name="screenWidth">The current width of the screen in pixel.</param>
        /// <param name="screenHeight">The current height of the screen in pixel.</param>
        /// <param name="viewMatrix">The calculated view matrix.</param>
        /// <param name="projMatrix">The calculated projection matrix.</param>
        protected override void CalculateViewProjectionMatrices(
            Vector3 position, Vector3 target, Vector3 upVector, float zNear, float zFar, int screenWidth, int screenHeight,
            out Matrix viewMatrix, out Matrix projMatrix)
        {
            if (m_zoomFactor <= ZOOM_FACTOR_MIN) { m_zoomFactor = ZOOM_FACTOR_MIN; }

            MatrixEx.CreateOrthoLH(
                (float)ScreenWidth / m_zoomFactor, (float)screenHeight / m_zoomFactor,
                -Math.Abs(Math.Max(zNear, zFar)),
                Math.Abs(Math.Max(zNear, zFar)),
                out projMatrix);
            MatrixEx.CreateLookAtLH(
                ref position, ref target, ref upVector,
                out viewMatrix);
        }

        /// <summary>
        /// Zooms the camera into or out along the actual target-vector.
        /// </summary>
        /// <param name="dist">The Distance you want to zoom.</param>
        public override void Zoom(float dist)
        {
            while (Math.Abs(dist) > 1f)
            {
                m_zoomFactor = m_zoomFactor + (float)Math.Sign(dist) * (m_zoomFactor / 10f);
                dist = dist - (float)Math.Sign(dist);
            }
            m_zoomFactor = m_zoomFactor + dist * m_zoomFactor;

            base.UpdateCamera();
        }

        /// <summary>
        /// Gets or sets the zoom factor.
        /// </summary>
        public float ZoomFactor
        {
            get { return m_zoomFactor; }
            set
            {
                if (m_zoomFactor != value)
                {
                    m_zoomFactor = value;
                    base.UpdateCamera();
                }
            }
        }
    }
}