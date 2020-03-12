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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using System;
using System.Numerics;

namespace SeeingSharp.Multimedia.Components
{
    public abstract class FocusedCameraComponent
        : SceneComponent<FocusedCameraComponent.PerSceneContext>
    {
        private const float ORTHO_ZOO_FACTOR_START = 1500f;
        private const float ORTHO_ZOO_FACTOR_PER_CAMERA_DISTANCE = 250f;

        // Constants
        private const float SINGLE_ROTATION_H = EngineMath.RAD_180DEG / 100f;
        private const float SINGLE_ROTATION_V = EngineMath.RAD_180DEG / 100f;

        // Configuration
        private Vector2 _hvRotation;

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusedPointCameraComponent"/> class.
        /// </summary>
        protected FocusedCameraComponent()
        {
            this.CameraDistanceInitial = 4f;
            this.CameraDistanceMin = 1f;
            this.CameraDistanceMax = 5f;
            _hvRotation = new Vector2(
                EngineMath.RAD_45DEG,
                EngineMath.RAD_45DEG);
        }

        /// <summary>
        /// Attaches this component to a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
        /// It may also be called from multiple scenes in parallel or simply withoud previous Detach call.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        /// <returns></returns>
        protected override PerSceneContext Attach(SceneManipulator manipulator, ViewInformation correspondingView)
        {
            var result = new PerSceneContext
            {
                CameraDistance = this.CameraDistanceInitial,
                CameraHVRotation = _hvRotation
            };

            return result;
        }

        /// <summary>
        /// Detaches this component from a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
        /// It may also be called from multiple scenes in parallel.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        /// <param name="componentContext">A context variable containing all created objects during call of Attach.</param>
        protected override void Detach(SceneManipulator manipulator, ViewInformation correspondingView, PerSceneContext componentContext)
        {

        }

        protected override void Update(SceneRelatedUpdateState updateState, ViewInformation correspondingView, PerSceneContext componentContext)
        {
            var actCamera = correspondingView.Camera;
            if (actCamera == null)
            {
                return;
            }

            foreach (var actInputFrame in updateState.InputFrames)
            {
                foreach (var actInputState in actInputFrame.GetInputStates(correspondingView))
                {
                    switch (actInputState)
                    {
                        // Handle keyboard
                        case KeyboardState actKeyboardState:
                            UpdateForKeyboard(componentContext, actCamera, actKeyboardState);
                            continue;

                        // Handle mouse (or pointer)
                        case MouseOrPointerState mouseState:
                            UpdateForMouse(componentContext, actCamera, mouseState);
                            continue;
                    }
                }
            }

            // Ensure that our values are in allowed ranges
            const float MAX_RAD = EngineMath.RAD_90DEG * 0.99f;
            const float MIN_RAD = EngineMath.RAD_90DEG * -0.99f;
            componentContext.CameraHVRotation.X %= EngineMath.RAD_360DEG;

            if (componentContext.CameraDistance < this.CameraDistanceMin) { componentContext.CameraDistance = this.CameraDistanceMin; }
            if (componentContext.CameraDistance > this.CameraDistanceMax) { componentContext.CameraDistance = this.CameraDistanceMax; }
            if (componentContext.CameraHVRotation.Y <= MIN_RAD) { componentContext.CameraHVRotation.Y = MIN_RAD; }
            if (componentContext.CameraHVRotation.Y >= MAX_RAD) { componentContext.CameraHVRotation.Y = MAX_RAD; }

            // Update camera position and rotation
            var cameraOffset = Vector3.UnitX;
            cameraOffset = Vector3.TransformNormal(
                cameraOffset,
                Matrix4x4.CreateRotationY(componentContext.CameraHVRotation.X));
            cameraOffset = Vector3.TransformNormal(
                cameraOffset,
                Matrix4x4.CreateFromAxisAngle(Vector3.Cross(cameraOffset, Vector3.UnitY), componentContext.CameraHVRotation.Y));

            var focusedLocation = this.GetFocusedLocation();
            actCamera.Position = focusedLocation + cameraOffset * componentContext.CameraDistance;
            actCamera.Target = focusedLocation;

            if(actCamera is OrthographicCamera3D orthoCamera)
            {
                orthoCamera.ZoomFactor = ORTHO_ZOO_FACTOR_START - (componentContext.CameraDistance * ORTHO_ZOO_FACTOR_PER_CAMERA_DISTANCE);
            }
        }

        protected abstract Vector3 GetFocusedLocation();

        /// <summary>
        /// Update camera for keyboard input.
        /// </summary>
        private static void UpdateForKeyboard(PerSceneContext componentContext, Camera3DBase camera, KeyboardState actKeyboardState)
        {
            foreach (var actKey in actKeyboardState.KeysDown)
            {
                switch (actKey)
                {
                    case WinVirtualKey.Up:
                    case WinVirtualKey.W:
                    case WinVirtualKey.NumPad8:
                        componentContext.CameraHVRotation += new Vector2(0f, SINGLE_ROTATION_V);
                        break;

                    case WinVirtualKey.Down:
                    case WinVirtualKey.S:
                    case WinVirtualKey.NumPad2:
                        componentContext.CameraHVRotation -= new Vector2(0f, SINGLE_ROTATION_V);
                        break;

                    case WinVirtualKey.Left:
                    case WinVirtualKey.A:
                    case WinVirtualKey.NumPad4:
                        componentContext.CameraHVRotation -= new Vector2(SINGLE_ROTATION_H, 0f);
                        break;

                    case WinVirtualKey.Right:
                    case WinVirtualKey.D:
                    case WinVirtualKey.NumPad6:
                        componentContext.CameraHVRotation += new Vector2(SINGLE_ROTATION_H, 0f);
                        break;

                    case WinVirtualKey.Q:
                    case WinVirtualKey.NumPad3:
                        if (camera.IsOrthographic)
                        {
                            componentContext.CameraDistance += 0.1f;
                        }
                        else
                        {
                            componentContext.CameraDistance *= 1.05f;
                        }
                        break;

                    case WinVirtualKey.E:
                    case WinVirtualKey.NumPad9:
                        if (camera.IsOrthographic)
                        {
                            componentContext.CameraDistance -= 0.1f;
                        }
                        else
                        {
                            componentContext.CameraDistance *= 0.95f;
                        }
                        break;
                }
            }
        }

        /// <summary>
        /// Update camera for mouse input.
        /// </summary>
        private static void UpdateForMouse(PerSceneContext componentContext, Camera3DBase camera, MouseOrPointerState mouseState)
        {
            // Handle mouse move
            if (mouseState.MoveDistanceDip != Vector2.Zero)
            {
                var moving = mouseState.MoveDistanceDip;

                if (mouseState.IsButtonDown(MouseButton.Left) &&
                    mouseState.IsButtonDown(MouseButton.Right))
                {
                    if (camera.IsOrthographic)
                    {
                        var multiplier = 1.05f;
                        if (moving.Y < 0f)
                        {
                            multiplier = 0.95f;
                        }
                        componentContext.CameraDistance *= multiplier;
                    }
                    else
                    {
                        componentContext.CameraDistance -= (moving.Y / 100f);
                    }
                }
                else if (mouseState.IsButtonDown(MouseButton.Left) ||
                         mouseState.IsButtonDown(MouseButton.Right))
                {
                    componentContext.CameraHVRotation += new Vector2(
                        SINGLE_ROTATION_H * (moving.X / 4f),
                        SINGLE_ROTATION_V * (moving.Y / 4f));
                }
            }

            // Handle mouse wheel
            if (mouseState.WheelDelta != 0)
            {
                if (camera.IsOrthographic)
                {
                    componentContext.CameraDistance -= (mouseState.WheelDelta / 500f);
                }
                else
                {
                    var multiplier = 0.95f - Math.Abs(mouseState.WheelDelta) / 1000f;
                    if (mouseState.WheelDelta < 0)
                    {
                        multiplier = 1.05f + Math.Abs(mouseState.WheelDelta) / 1000f;
                    }
                    componentContext.CameraDistance *= multiplier;
                }
            }
        }

        public float CameraDistanceInitial
        {
            get;
            set;
        }

        public float CameraDistanceMin
        {
            get;
            set;
        }

        public float CameraDistanceMax
        {
            get;
            set;
        }

        /// <summary>
        /// Initial horizontal rotation of the camera (degrees).
        /// </summary>
        public float CameraHRotationInitial
        {
            get => _hvRotation.X;
            set => _hvRotation.X = value;
        }

        /// <summary>
        /// Initial vertical rotation of the camera (degrees).
        /// </summary>
        public float CameraVRotationInitial
        {
            get => _hvRotation.Y;
            set => _hvRotation.Y = value;
        }

        public override string ComponentGroup => SeeingSharpConstants.COMPONENT_GROUP_CAMERA;

        public override bool IsViewSpecific => true;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        public class PerSceneContext
        {
            public float CameraDistance;
            public Vector2 CameraHVRotation;
        }
    }
}