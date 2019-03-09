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
using SharpDX;

namespace SeeingSharp.Multimedia.Components
{
    public class FreeMovingCameraComponent : SceneComponent
    {
        // Constants
        private const float MOVEMENT = 0.3f;
        private const float ROTATION = 0.01f;

        /// <summary>
        /// Attaches this component to a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
        /// It may also be called from multiple scenes in parallel or simply withoud previous Detach call.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected override void Attach(SceneManipulator manipulator, ViewInformation correspondingView)
        {

        }

        /// <summary>
        /// Detaches this component from a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
        /// It may also be called from multiple scenes in parallel.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected override void Detach(SceneManipulator manipulator, ViewInformation correspondingView)
        {
            // nothing to be detached here
        }

        /// <summary>
        /// This update method gets called on each update pass for each scenes
        /// this component is attached to.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        /// <param name="correspondingView">The view which attached this component (may be null).</param>
        protected override void Update(SceneRelatedUpdateState updateState, ViewInformation correspondingView)
        {
            var actCamera = correspondingView.Camera;

            if (actCamera == null)
            {
                return;
            }

            foreach (var actInputFrame in updateState.InputFrames)
            {
                var isControlKeyDown = false;
                foreach (var actInputState in actInputFrame.GetInputStates(correspondingView))
                {
                    switch (actInputState)
                    {
                        // Handle keyboard
                        case KeyboardState actKeyboardState:
                            UpdateForKeyboard(actCamera, actKeyboardState, out isControlKeyDown);
                            continue;

                        // Handle mouse (or pointer)
                        case MouseOrPointerState mouseState:
                            UpdateForMouse(actCamera, isControlKeyDown, mouseState);
                            continue;
                    }
                }
            }
        }

        /// <summary>
        /// Update camera for keyboard input.
        /// </summary>
        private static void UpdateForKeyboard(
            Camera3DBase actCamera, KeyboardState actKeyboardState,
            out bool isControlKeyDown)
        {
            // Define multiplyer
            var multiplier = 1f;
            isControlKeyDown = false;

            if (actKeyboardState.IsKeyDown(WinVirtualKey.ControlKey) ||
                actKeyboardState.IsKeyDown(WinVirtualKey.LControlKey) ||
                actKeyboardState.IsKeyDown(WinVirtualKey.RControlKey))
            {
                multiplier = 2f;
                isControlKeyDown = true;
            }

            foreach (var actKey in actKeyboardState.KeysDown)
            {
                switch (actKey)
                {
                    case WinVirtualKey.Up:
                    case WinVirtualKey.W:
                        actCamera.Zoom(MOVEMENT * multiplier);
                        break;

                    case WinVirtualKey.Down:
                    case WinVirtualKey.S:
                        actCamera.Zoom(-MOVEMENT * multiplier);
                        break;

                    case WinVirtualKey.Left:
                    case WinVirtualKey.A:
                        actCamera.Strave(-MOVEMENT * multiplier);
                        break;

                    case WinVirtualKey.Right:
                    case WinVirtualKey.D:
                        actCamera.Strave(MOVEMENT * multiplier);
                        break;

                    case WinVirtualKey.Q:
                    case WinVirtualKey.NumPad3:
                        actCamera.Move(new Vector3(0f, -MOVEMENT * multiplier, 0f));
                        break;

                    case WinVirtualKey.E:
                    case WinVirtualKey.NumPad9:
                        actCamera.Move(new Vector3(0f, MOVEMENT * multiplier, 0f));
                        break;

                    case WinVirtualKey.NumPad4:
                        actCamera.Rotate(ROTATION, 0f);
                        break;

                    case WinVirtualKey.NumPad2:
                        actCamera.Rotate(0f, -ROTATION);
                        break;

                    case WinVirtualKey.NumPad6:
                        actCamera.Rotate(-ROTATION, 0f);
                        break;

                    case WinVirtualKey.NumPad8:
                        actCamera.Rotate(0f, ROTATION);
                        break;
                }
            }
        }

        /// <summary>
        /// Update camera for mouse input.
        /// </summary>
        private static void UpdateForMouse(Camera3DBase actCamera, bool isControlKeyDown, MouseOrPointerState mouseState)
        {
            // Handle mouse move
            if (mouseState.MoveDistanceDip != Vector2.Zero)
            {
                var moving = mouseState.MoveDistanceDip;

                if (mouseState.IsButtonDown(MouseButton.Left) &&
                    mouseState.IsButtonDown(MouseButton.Right))
                {
                    actCamera.Zoom(moving.Y / -50f);
                }
                else if (mouseState.IsButtonDown(MouseButton.Left))
                {
                    actCamera.Strave(moving.X / 50f);
                    actCamera.UpDown(-moving.Y / 50f);
                }
                else if (mouseState.IsButtonDown(MouseButton.Right))
                {
                    actCamera.Rotate(-moving.X / 200f, -moving.Y / 200f);
                }
            }

            // Handle mouse wheel
            if (mouseState.WheelDelta != 0)
            {
                var multiplier = 1f;
                if (isControlKeyDown) { multiplier = 2f; }
                actCamera.Zoom(mouseState.WheelDelta / 100f * multiplier);
            }
        }

        public override string ComponentGroup => SeeingSharpConstants.COMPONENT_GROUP_CAMERA;

        public override bool IsViewSpecific => true;
    }
}
