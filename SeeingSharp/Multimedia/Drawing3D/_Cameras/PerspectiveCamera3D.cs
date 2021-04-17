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
using System;
using System.Numerics;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public class PerspectiveCamera3D : Camera3DBase
    {
        // Configuration
        private float _aspectRatio;
        private float _fov = (float)Math.PI / 4.0f; // default value for an aspect of 4:3 and an horizontal fov of 90°

        /// <summary>
        /// Gets the current aspect ratio.
        /// </summary>
        public float AspectRatio => _aspectRatio;

        /// <summary>
        /// Gets or sets the field of view value.
        /// </summary>
        public float FieldOfView
        {
            get => _fov;
            set
            {
                _fov = value;
                this.UpdateCamera();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerspectiveCamera3D"/> class.
        /// </summary>
        public PerspectiveCamera3D()
            : base(false)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PerspectiveCamera3D"/> class.
        /// </summary>
        /// <param name="width">Width of the render window.</param>
        /// <param name="height">Height of the render window.</param>
        public PerspectiveCamera3D(int width, int height)
            : base(false, width, height)
        {

        }

        /// <summary>
        /// Calculates the view and projection matrix for this camera.
        /// </summary>
        /// <param name="position">The position of the camera.</param>
        /// <param name="target">The target point of the camera.</param>
        /// <param name="upVector">The current up vector.</param>
        /// <param name="zNear">Distance to the nearest rendered pixel.</param>
        /// <param name="zFar">Distance to the most far rendered pixel.</param>
        /// <param name="screenWidth">The current width of the screen in pixel.</param>
        /// <param name="screenHeight">The current height of the screen in pixel.</param>
        /// <param name="viewMatrix">The calculated view matrix.</param>
        /// <param name="projMatrix">The calculated projection matrix.</param>
        protected override void CalculateViewProjectionMatrices(
            Vector3 position, Vector3 target, Vector3 upVector, float zNear, float zFar, int screenWidth, int screenHeight,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix)
        {
            this._aspectRatio = screenWidth / (float)screenHeight;
            Matrix4x4Ex.CreateLookAtLH(
                ref position, ref target, ref upVector,
                out viewMatrix);
            Matrix4x4Ex.CreatePerspectiveFovLH(
                this._fov,
                this._aspectRatio,
                zNear, zFar, out projMatrix);
        }

        /// <summary>
        /// Calculates the Field of View by focal length in respect to the current film gauge used them and update the camera
        /// </summary>
        /// <param name="filmGauge">physical property of photographic or motion picture film stock which defines its width - see wide high-resolution film gauge https://en.wikipedia.org/wiki/70_mm_film, human eye focal 50 mm, full frame movie 35 mm, default 70 mm</param>
        /// <param name="focalLength">focal length in respect to the <paramref name="filmGauge"/> - as focal length changes, the amount of the subject captured by the lens (the viewing angle) also changes, default 30 mm</param>
        public void SetFieldOfView(float filmGauge = 70, float focalLength = 30)
        {
            //...see http://www.bobatkins.com/photography/technical/field_of_view.html
            var filmHeight = filmGauge / Math.Max(this._aspectRatio, 1);
            var filmSlope = 0.5f * filmHeight / focalLength;
            this._fov = (180.0f / (float)Math.PI) * 2.0f * (float)Math.Atan((0.5f * filmHeight / filmSlope));

            this.UpdateCamera();
        }
    }
}
