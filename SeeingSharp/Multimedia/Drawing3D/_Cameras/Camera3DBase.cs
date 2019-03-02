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
using SeeingSharp.Multimedia.Core;
using SharpDX;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class Camera3DBase : IAnimatableObject
    {
        // Configuration
        private Vector3 m_position = new Vector3(0, 0, 0);
        private Vector3 m_relativeTarget = new Vector3(0, 0, 1);
        private Vector3 m_upVector = new Vector3(0, 1, 0);
        private float m_hRotation;
        private float m_vRotation;

        // State
        private Vector3 m_lastBigStepPos = new Vector3(0, 0, 0);
        private Vector3 m_up;
        private Vector3 m_right;
        private Vector3 m_look;
        private Matrix m_view;
        private Matrix m_project;
        private Matrix m_viewProj;

        // Additional parameters
        private float m_zNear = 0.1f;
        private float m_zFar = 500f;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera3DBase"/> class.
        /// </summary>
        public Camera3DBase()
            : this(100, 100)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera3DBase"/> class.
        /// </summary>
        /// <param name="width">Width of the renderwindow.</param>
        /// <param name="height">Height of the renderwindow.</param>
        public Camera3DBase(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;

            AnimationHandler = new AnimationHandler(this);

            UpdateCamera();
        }

        /// <summary>
        /// Applies the data from the given ViewPoint object.
        /// </summary>
        /// <param name="camera3DViewPoint">The ViewPoint to be applied.</param>
        public virtual void ApplyViewPoint(Camera3DViewPoint camera3DViewPoint)
        {
            Position = camera3DViewPoint.Position;
            TargetRotation = camera3DViewPoint.Rotation;
        }

        /// <summary>
        /// Gets a ViewPoint out of this camera object.
        /// </summary>
        public virtual Camera3DViewPoint GetViewPoint()
        {
            var result = new Camera3DViewPoint
            {
                Position = Position,
                Rotation = TargetRotation
            };

            return result;
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        public void UpdateCamera()
        {
            var newTarget = m_relativeTarget;
            newTarget = Vector3.Add(newTarget, m_position);

            if (ScreenHeight == 0f)
            {
                ScreenHeight = 1;
            }

            // Calculate matrices
            CalculateViewProjectionMatrices(
                m_position, newTarget, m_upVector, m_zNear, m_zFar, ScreenWidth, ScreenHeight,
                out m_view, out m_project);
            m_viewProj = m_view * m_project;

            // Calculate right, up and look vectors
            m_right.X = m_view.M11;
            m_right.Y = m_view.M21;
            m_right.Z = m_view.M31;
            m_up.X = m_view.M12;
            m_up.Y = m_view.M22;
            m_up.Z = m_view.M32;
            m_look.X = m_view.M13;
            m_look.Y = m_view.M23;
            m_look.Z = m_view.M33;

            StateChanged = true;
        }

        /// <summary>
        /// Sets the size of the screen.
        /// </summary>
        /// <param name="width">Width of the renderwindow.</param>
        /// <param name="height">Height of the renderwindow.</param>
        public void SetScreenSize(int width, int height)
        {
            ScreenWidth = width;
            ScreenHeight = height;

            UpdateCamera();
        }

        /// <summary>
        /// Gets the current screen size.
        /// </summary>
        public Vector2 GetScreenSize()
        {
            return new Vector2(ScreenWidth, ScreenHeight);
        }

        /// <summary>
        /// Rotates the target around the position of the camera.
        /// </summary>
        /// <param name="hRot">horizontal rotation.</param>
        /// <param name="vRot">vertical rotation.</param>
        public void Rotate(float hRot, float vRot)
        {
            m_hRotation += hRot;
            m_vRotation += vRot;

            if (m_vRotation >= (float)Math.PI / 2f) { m_vRotation = (float)Math.PI / 2f - 0.001f; }
            if (m_vRotation <= -(float)Math.PI / 2f) { m_vRotation = -(float)Math.PI / 2f + 0.001f; }

            m_relativeTarget.X = (float)(1f * Math.Cos(m_vRotation) * Math.Cos(m_hRotation));
            m_relativeTarget.Y = (float)(1f * Math.Sin(m_vRotation));
            m_relativeTarget.Z = (float)(1f * Math.Cos(m_vRotation) * Math.Sin(m_hRotation));

            UpdateCamera();
        }

        /// <summary>
        /// Zooms the camera into or out along the actual target-vector.
        /// </summary>
        /// <param name="dist">The Distance you want to zoom.</param>
        public virtual void Zoom(float dist)
        {
            m_position.X += dist * m_relativeTarget.X;
            m_position.Y += dist * m_relativeTarget.Y;
            m_position.Z += dist * m_relativeTarget.Z;

            UpdateCamera();
        }

        /// <summary>
        /// Moves the camera position.
        /// </summary>
        /// <param name="x">moving in x direction.</param>
        /// <param name="z">moving in z direction.</param>
        public void Move(float x, float z)
        {
            m_position.X += x;
            m_position.Z += z;

            UpdateCamera();
        }

        public void Move(Vector3 moveVector)
        {
            m_position = m_position + moveVector;

            UpdateCamera();
        }

        /// <summary>
        /// Moves the Camera up and down.
        /// </summary>
        public void UpDown(float points)
        {
            if (!InvertScreenMove)
            {
                m_position.X = m_position.X + m_up.X * points;
                m_position.Y = m_position.Y + m_up.Y * points;
                m_position.Z = m_position.Z + m_up.Z * points;
            }
            else
            {
                m_position.X = m_position.X - m_up.X * points;
                m_position.Y = m_position.Y - m_up.Y * points;
                m_position.Z = m_position.Z - m_up.Z * points;
            }

            UpdateCamera();
        }

        /// <summary>
        /// Moves the camera up and down.
        /// </summary>
        public void UpDownWithoutMoving(float points)
        {
            m_position.Y = m_position.Y + m_up.Y * points;

            UpdateCamera();
        }

        /// <summary>
        /// Straves the camera.
        /// </summary>
        public void Strave(float points)
        {
            if (!InvertScreenMove)
            {
                m_position.X = m_position.X + m_right.X * points;
                m_position.Y = m_position.Y + m_right.Y * points;
                m_position.Z = m_position.Z + m_right.Z * points;
            }
            else
            {
                m_position.X = m_position.X - m_right.X * points;
                m_position.Y = m_position.Y - m_right.Y * points;
                m_position.Z = m_position.Z - m_right.Z * points;
            }

            UpdateCamera();
        }

        /// <summary>
        /// Streaves the camera.
        /// </summary>
        public void StraveAtPlane(float points)
        {
            m_position.X = m_position.X + m_right.X * points;
            m_position.Z = m_position.Z + m_right.Z * points;

            UpdateCamera();
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
        protected abstract void CalculateViewProjectionMatrices(
            Vector3 position, Vector3 target, Vector3 upVector, float zNear, float zFar, int screenWidth, int screenHeight,
            out Matrix viewMatrix, out Matrix projMatrix);

        /// <summary>
        /// Gets the current AnimationHandler for this camera.
        /// </summary>
        public AnimationHandler AnimationHandler { get; }

        public bool InvertScreenMove { get; set; }

        /// <summary>
        /// Retrieves the view-matrix.
        /// </summary>
        public Matrix View => m_view;

        /// <summary>
        /// Gets the view-projection matrix.
        /// </summary>
        public Matrix ViewProjection => m_viewProj;

        /// <summary>
        /// Retrieves or sets the direction / target.
        /// </summary>
        public Vector3 Direction => m_look;

        /// <summary>
        /// Retrieves a vector, wich is targeting right.
        /// </summary>
        public Vector3 Right => m_right;

        /// <summary>
        /// Retrieves a vector, wich is targiting upwards.
        /// </summary>
        public Vector3 Up => m_up;

        /// <summary>
        /// Gets or sets the vector that points up.
        /// </summary>
        public Vector3 UpVector
        {
            get => m_upVector;
            set
            {
                if (m_upVector != value)
                {
                    m_upVector = value;
                    UpdateCamera();
                }
            }
        }

        /// <summary>
        /// Retrieves or sets the rotation-angles of the target.
        /// The first element of the vector is hRot, the second is vRot.
        /// 
        /// The angles are radian values!
        /// </summary>
        public Vector2 TargetRotation
        {
            get => new Vector2(m_hRotation, m_vRotation);
            set
            {
                var v = value;

                m_hRotation = v.X;
                m_vRotation = v.Y;

                if (m_vRotation >= (float)Math.PI / 2f) { m_vRotation = (float)Math.PI / 2f - 0.001f; }
                if (m_vRotation <= -(float)Math.PI / 2f) { m_vRotation = -(float)Math.PI / 2f + 0.001f; }

                m_relativeTarget.X = (float)(1f * Math.Cos(m_vRotation) * Math.Cos(m_hRotation));
                m_relativeTarget.Y = (float)(1f * Math.Sin(m_vRotation));
                m_relativeTarget.Z = (float)(1f * Math.Cos(m_vRotation) * Math.Sin(m_hRotation));

                UpdateCamera();
            }
        }

        /// <summary>
        /// Gets or sets the target position.
        /// </summary>
        public Vector3 Target
        {
            get => m_relativeTarget + m_position;
            set => RelativeTarget = value - m_position;
        }

        /// <summary>
        /// Gets or sets the relative target position.
        /// </summary>
        public Vector3 RelativeTarget
        {
            get => m_relativeTarget;
            set
            {
                m_relativeTarget = Vector3.Normalize(value);

                //Update horizontal and vertical rotation
                Vector3Ex.ToHVRotation(m_relativeTarget, out m_hRotation, out m_vRotation);

                UpdateCamera();
            }
        }

        /// <summary>
        /// Gets or sets the minimum distance of rendered pixels.
        /// </summary>
        public float ZNear
        {
            get => m_zNear;
            set
            {
                m_zNear = value;
                UpdateCamera();
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance of rendered pixels.
        /// </summary>
        public float ZFar
        {
            get => m_zFar;
            set
            {
                m_zFar = value;
                UpdateCamera();
            }
        }

        /// <summary>
        /// Retrieves or sets the position of the camera.
        /// </summary>
        public Vector3 Position
        {
            get => m_position;
            set
            {
                m_position = value;

                UpdateCamera();
            }
        }

        /// <summary>
        /// Retrieves projection-matrix.
        /// </summary>
        public Matrix Projection => m_project;

        /// <summary>
        /// Width of the screen.
        /// </summary>
        public int ScreenWidth { get; set; }

        /// <summary>
        /// Height of the screen.
        /// </summary>
        public int ScreenHeight { get; set; }

        /// <summary>
        /// Did the state of the camera change last time?
        /// Set this flag to false to reset the value.
        /// </summary>
        public bool StateChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the currently associated RenderLoop object.
        /// </summary>
        public object AssociatedRenderLoop { get; internal set; }
    }
}