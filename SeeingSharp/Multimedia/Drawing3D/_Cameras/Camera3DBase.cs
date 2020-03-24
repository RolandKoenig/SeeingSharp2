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
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Drawing3D
{
    public abstract class Camera3DBase : IAnimatableObject
    {
        // Configuration
        private Vector3 _position = new Vector3(0, 0, 0);
        private Vector3 _relativeTarget = new Vector3(0, 0, 1);
        private Vector3 _upVector = new Vector3(0, 1, 0);
        private float _hRotation;
        private float _vRotation;

        // State
        private Vector3 _lastBigStepPos = new Vector3(0, 0, 0);
        private Vector3 _up;
        private Vector3 _right;
        private Vector3 _look;
        private Matrix4x4 _view;
        private Matrix4x4 _project;
        private Matrix4x4 _viewProj;

        // Additional parameters
        private float _zNear = 0.1f;
        private float _zFar = 500f;

        /// <summary>
        /// Gets the current AnimationHandler for this camera.
        /// </summary>
        public AnimationHandler AnimationHandler { get; }

        public bool InvertScreenMove { get; set; }

        /// <summary>
        /// Retrieves the view-matrix.
        /// </summary>
        public Matrix4x4 View => _view;

        /// <summary>
        /// Gets the view-projection matrix.
        /// </summary>
        public Matrix4x4 ViewProjection => _viewProj;

        /// <summary>
        /// Retrieves or sets the direction / target.
        /// </summary>
        public Vector3 Direction => _look;

        /// <summary>
        /// Retrieves a vector, which is targeting right.
        /// </summary>
        public Vector3 Right => _right;

        /// <summary>
        /// Retrieves a vector, which is targeting upwards.
        /// </summary>
        public Vector3 Up => _up;

        /// <summary>
        /// Gets or sets the vector that points up.
        /// </summary>
        public Vector3 UpVector
        {
            get => _upVector;
            set
            {
                if (_upVector != value)
                {
                    _upVector = value;
                    this.UpdateCamera();
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
            get => new Vector2(_hRotation, _vRotation);
            set
            {
                var v = value;

                _hRotation = v.X;
                _vRotation = v.Y;

                if (_vRotation >= (float)Math.PI / 2f) { _vRotation = (float)Math.PI / 2f - 0.001f; }
                if (_vRotation <= -(float)Math.PI / 2f) { _vRotation = -(float)Math.PI / 2f + 0.001f; }

                _relativeTarget.X = (float)(1f * Math.Cos(_vRotation) * Math.Cos(_hRotation));
                _relativeTarget.Y = (float)(1f * Math.Sin(_vRotation));
                _relativeTarget.Z = (float)(1f * Math.Cos(_vRotation) * Math.Sin(_hRotation));

                this.UpdateCamera();
            }
        }

        /// <summary>
        /// Gets or sets the target position.
        /// </summary>
        public Vector3 Target
        {
            get => _relativeTarget + _position;
            set => this.RelativeTarget = value - _position;
        }

        /// <summary>
        /// Gets or sets the relative target position.
        /// </summary>
        public Vector3 RelativeTarget
        {
            get => _relativeTarget;
            set
            {
                _relativeTarget = Vector3.Normalize(value);

                // Update horizontal and vertical rotation
                Vector3Ex.ToHVRotation(_relativeTarget, out _hRotation, out _vRotation);

                this.UpdateCamera();
            }
        }

        /// <summary>
        /// Gets or sets the minimum distance of rendered pixels.
        /// </summary>
        public float ZNear
        {
            get => _zNear;
            set
            {
                _zNear = value;
                this.UpdateCamera();
            }
        }

        /// <summary>
        /// Gets or sets the maximum distance of rendered pixels.
        /// </summary>
        public float ZFar
        {
            get => _zFar;
            set
            {
                _zFar = value;
                this.UpdateCamera();
            }
        }

        /// <summary>
        /// Retrieves or sets the position of the camera.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                _position = value;

                this.UpdateCamera();
            }
        }

        /// <summary>
        /// Retrieves projection-matrix.
        /// </summary>
        public Matrix4x4 Projection => _project;

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

        public bool IsOrthographic => IsOrthopraphicInternal;

        internal bool IsOrthopraphicInternal;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera3DBase"/> class.
        /// </summary>
        protected Camera3DBase(bool isOrthographic)
            : this(isOrthographic, 100, 100)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera3DBase"/> class.
        /// </summary>
        /// <param name="isOrthographic">Is this camera an orthographic camera?</param>
        /// <param name="width">Width of the render window.</param>
        /// <param name="height">Height of the render window.</param>
        protected Camera3DBase(bool isOrthographic, int width, int height)
        {
            IsOrthopraphicInternal = isOrthographic;
            this.ScreenWidth = width;
            this.ScreenHeight = height;

            this.AnimationHandler = new AnimationHandler(this);

            this.UpdateCamera();
        }

        /// <summary>
        /// Applies the data from the given ViewPoint object.
        /// </summary>
        /// <param name="camera3DViewPoint">The ViewPoint to be applied.</param>
        public virtual void ApplyViewPoint(Camera3DViewPoint camera3DViewPoint)
        {
            this.Position = camera3DViewPoint.Position;
            this.TargetRotation = camera3DViewPoint.Rotation;
        }

        /// <summary>
        /// Gets a ViewPoint out of this camera object.
        /// </summary>
        public virtual Camera3DViewPoint GetViewPoint()
        {
            var result = new Camera3DViewPoint
            {
                Position = this.Position,
                Rotation = this.TargetRotation
            };

            return result;
        }

        /// <summary>
        /// Updates the camera.
        /// </summary>
        public void UpdateCamera()
        {
            var newTarget = _relativeTarget;
            newTarget = Vector3.Add(newTarget, _position);

            if (EngineMath.EqualsWithTolerance(this.ScreenHeight, 0f))
            {
                this.ScreenHeight = 1;
            }

            // Calculate matrices
            this.CalculateViewProjectionMatrices(
                _position, newTarget, _upVector, _zNear, _zFar, this.ScreenWidth, this.ScreenHeight,
                out _view, out _project);
            _viewProj = _view * _project;

            // Calculate right, up and look vectors
            _right.X = _view.M11;
            _right.Y = _view.M21;
            _right.Z = _view.M31;
            _up.X = _view.M12;
            _up.Y = _view.M22;
            _up.Z = _view.M32;
            _look.X = _view.M13;
            _look.Y = _view.M23;
            _look.Z = _view.M33;

            this.StateChanged = true;
        }

        /// <summary>
        /// Sets the size of the screen.
        /// </summary>
        /// <param name="width">Width of the render window.</param>
        /// <param name="height">Height of the render window.</param>
        public void SetScreenSize(int width, int height)
        {
            this.ScreenWidth = width;
            this.ScreenHeight = height;

            this.UpdateCamera();
        }

        /// <summary>
        /// Gets the current screen size.
        /// </summary>
        public Vector2 GetScreenSize()
        {
            return new Vector2(this.ScreenWidth, this.ScreenHeight);
        }

        /// <summary>
        /// Rotates the target around the position of the camera.
        /// </summary>
        /// <param name="hRot">horizontal rotation.</param>
        /// <param name="vRot">vertical rotation.</param>
        public void Rotate(float hRot, float vRot)
        {
            _hRotation += hRot;
            _vRotation += vRot;

            if (_vRotation >= (float)Math.PI / 2f) { _vRotation = (float)Math.PI / 2f - 0.001f; }
            if (_vRotation <= -(float)Math.PI / 2f) { _vRotation = -(float)Math.PI / 2f + 0.001f; }

            _relativeTarget.X = (float)(1f * Math.Cos(_vRotation) * Math.Cos(_hRotation));
            _relativeTarget.Y = (float)(1f * Math.Sin(_vRotation));
            _relativeTarget.Z = (float)(1f * Math.Cos(_vRotation) * Math.Sin(_hRotation));

            this.UpdateCamera();
        }

        /// <summary>
        /// Zooms the camera into or out along the actual target-vector.
        /// </summary>
        /// <param name="dist">The Distance you want to zoom.</param>
        public virtual void Zoom(float dist)
        {
            _position.X += dist * _relativeTarget.X;
            _position.Y += dist * _relativeTarget.Y;
            _position.Z += dist * _relativeTarget.Z;

            this.UpdateCamera();
        }

        /// <summary>
        /// Moves the camera position.
        /// </summary>
        /// <param name="x">moving in x direction.</param>
        /// <param name="z">moving in z direction.</param>
        public void Move(float x, float z)
        {
            _position.X += x;
            _position.Z += z;

            this.UpdateCamera();
        }

        public void Move(Vector3 moveVector)
        {
            _position = _position + moveVector;

            this.UpdateCamera();
        }

        /// <summary>
        /// Moves the Camera up and down.
        /// </summary>
        public void UpDown(float points)
        {
            if (!this.InvertScreenMove)
            {
                _position.X = _position.X + _up.X * points;
                _position.Y = _position.Y + _up.Y * points;
                _position.Z = _position.Z + _up.Z * points;
            }
            else
            {
                _position.X = _position.X - _up.X * points;
                _position.Y = _position.Y - _up.Y * points;
                _position.Z = _position.Z - _up.Z * points;
            }

            this.UpdateCamera();
        }

        /// <summary>
        /// Moves the camera up and down.
        /// </summary>
        public void UpDownWithoutMoving(float points)
        {
            _position.Y = _position.Y + _up.Y * points;

            this.UpdateCamera();
        }

        /// <summary>
        /// Straves the camera.
        /// </summary>
        public void Strave(float points)
        {
            if (!this.InvertScreenMove)
            {
                _position.X = _position.X + _right.X * points;
                _position.Y = _position.Y + _right.Y * points;
                _position.Z = _position.Z + _right.Z * points;
            }
            else
            {
                _position.X = _position.X - _right.X * points;
                _position.Y = _position.Y - _right.Y * points;
                _position.Z = _position.Z - _right.Z * points;
            }

            this.UpdateCamera();
        }

        /// <summary>
        /// Straves the camera.
        /// </summary>
        public void StraveAtPlane(float points)
        {
            _position.X = _position.X + _right.X * points;
            _position.Z = _position.Z + _right.Z * points;

            this.UpdateCamera();
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
        protected abstract void CalculateViewProjectionMatrices(
            Vector3 position, Vector3 target, Vector3 upVector, float zNear, float zFar, int screenWidth, int screenHeight,
            out Matrix4x4 viewMatrix, out Matrix4x4 projMatrix);
    }
}