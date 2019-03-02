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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SharpDX;

namespace SeeingSharp.Multimedia.Components
{
    public abstract class FocusedCameraComponent
        : SceneComponent<FocusedCameraComponent.PerSceneContext>
    {
        // Constants
        private const float SINGLE_ROTATION_H = EngineMath.RAD_180DEG / 100f;
        private const float SINGLE_ROTATION_V = EngineMath.RAD_180DEG / 100f;

        // Configuration
        private Vector2 m_hvRotation;
        
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
                CameraDistance = CameraDistanceInitial,
                CameraHVRotation = m_hvRotation
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
        /// <param name="componentContext">A context variable containing all createded objects during call of Attach.</param>
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
                    // Handle keyboard
                    var actKeyboardState = actInputState as KeyboardState;

                    if (actKeyboardState != null)
                    {
                        UpdateForKeyboard(componentContext, actCamera, actKeyboardState);
                        continue;
                    }

                    // Handle mouse (or pointer)
                    var mouseState = actInputState as MouseOrPointerState;

                    if (mouseState != null)
                    {
                        UpdateForMouse(componentContext, actCamera, mouseState);
                    }
                }
            }

            // Ensure that our values are in allowed ranges
            var maxRad = EngineMath.RAD_90DEG * 0.99f;
            var minRad = EngineMath.RAD_90DEG * -0.99f;
            componentContext.CameraHVRotation.X = componentContext.CameraHVRotation.X % EngineMath.RAD_360DEG;

            if (componentContext.CameraDistance < CameraDistanceMin) { componentContext.CameraDistance = CameraDistanceMin; }
            if (componentContext.CameraDistance > CameraDistanceMax) { componentContext.CameraDistance = CameraDistanceMax; }
            if (componentContext.CameraHVRotation.Y <= minRad) { componentContext.CameraHVRotation.Y = minRad; }
            if (componentContext.CameraHVRotation.Y >= maxRad) { componentContext.CameraHVRotation.Y = maxRad; }

            // Update camera position and rotation
            var cameraOffset = Vector3.UnitX;
            cameraOffset = Vector3.TransformNormal(
                cameraOffset,
                Matrix.RotationY(componentContext.CameraHVRotation.X));
            cameraOffset = Vector3.TransformNormal(
                cameraOffset,
                Matrix.RotationAxis(Vector3.Cross(cameraOffset, Vector3.UnitY), componentContext.CameraHVRotation.Y));

            var focusedLocation = GetFocusedLocation();
            actCamera.Position = focusedLocation + cameraOffset * componentContext.CameraDistance;
            actCamera.Target = focusedLocation;
        }

        /// <summary>
        /// Update camera for keyboard input.
        /// </summary>
        private static void UpdateForKeyboard(
            PerSceneContext componentContext, Camera3DBase actCamera,
            KeyboardState actKeyboardState)
        {
            foreach (var actKey in actKeyboardState.KeysDown)
            {
                switch (actKey)
                {
                    case WinVirtualKey.Up:
                    case WinVirtualKey.W:
                    case WinVirtualKey.NumPad8:
                        componentContext.CameraHVRotation = componentContext.CameraHVRotation +
                            new Vector2(0f, SINGLE_ROTATION_V);
                        break;

                    case WinVirtualKey.Down:
                    case WinVirtualKey.S:
                    case WinVirtualKey.NumPad2:
                        componentContext.CameraHVRotation = componentContext.CameraHVRotation -
                            new Vector2(0f, SINGLE_ROTATION_V);
                        break;

                    case WinVirtualKey.Left:
                    case WinVirtualKey.A:
                    case WinVirtualKey.NumPad4:
                        componentContext.CameraHVRotation = componentContext.CameraHVRotation -
                            new Vector2(SINGLE_ROTATION_H, 0f);
                        break;

                    case WinVirtualKey.Right:
                    case WinVirtualKey.D:
                    case WinVirtualKey.NumPad6:
                        componentContext.CameraHVRotation = componentContext.CameraHVRotation +
                            new Vector2(SINGLE_ROTATION_H, 0f);
                        break;

                    case WinVirtualKey.Q:
                    case WinVirtualKey.NumPad3:
                        componentContext.CameraDistance = componentContext.CameraDistance * 1.05f;
                        break;

                    case WinVirtualKey.E:
                    case WinVirtualKey.NumPad9:
                        componentContext.CameraDistance = componentContext.CameraDistance * 0.95f;
                        break;
                }
            }
        }

        /// <summary>
        /// Update camera for mouse input.
        /// </summary>
        private static void UpdateForMouse(
            PerSceneContext componentContext, Camera3DBase actCamera,
            MouseOrPointerState mouseState)
        {
            // Handle mouse move
            if (mouseState.MoveDistanceDip != Vector2.Zero)
            {
                var moving = mouseState.MoveDistanceDip;

                if (mouseState.IsButtonDown(MouseButton.Left) &&
                    mouseState.IsButtonDown(MouseButton.Right))
                {
                    var multiplyer = 1.05f;

                    if (moving.Y < 0f)
                    {
                        multiplyer = 0.95f;
                    }

                    componentContext.CameraDistance = componentContext.CameraDistance * multiplyer;
                }
                else if (mouseState.IsButtonDown(MouseButton.Left) ||
                         mouseState.IsButtonDown(MouseButton.Right))
                {
                    componentContext.CameraHVRotation = componentContext.CameraHVRotation +
                        new Vector2(
                            SINGLE_ROTATION_H * (moving.X / 4f),
                            SINGLE_ROTATION_V * (moving.Y / 4f));
                }
            }

            // Handle mouse wheel
            if (mouseState.WheelDelta != 0)
            {
                var multiplyer = 0.95f - Math.Abs(mouseState.WheelDelta) / 1000f;

                if (mouseState.WheelDelta < 0)
                {
                    multiplyer = 1.05f + Math.Abs(mouseState.WheelDelta) / 1000f;
                }

                componentContext.CameraDistance = componentContext.CameraDistance * multiplyer;
            }
        }

        protected abstract Vector3 GetFocusedLocation();

        /// <summary>
        /// Initializes a new instance of the <see cref="FocusedPointCameraComponent"/> class.
        /// </summary>
        public FocusedCameraComponent()
        {
            CameraDistanceInitial = 4f;
            CameraDistanceMin = 3f;
            CameraDistanceMax = 10f;
            m_hvRotation = new Vector2(
                EngineMath.RAD_45DEG,
                EngineMath.RAD_45DEG);
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
            get => m_hvRotation.X;
            set => m_hvRotation.X = value;
        }

        /// <summary>
        /// Initial vertical rotation of the camera (degrees).
        /// </summary>
        public float CameraVRotationInitial
        {
            get => m_hvRotation.Y;
            set => m_hvRotation.Y = value;
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