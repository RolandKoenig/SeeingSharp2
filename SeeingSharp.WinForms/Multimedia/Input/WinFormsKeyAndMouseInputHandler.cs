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
using System;
using System.Collections.Generic;
using System.Numerics;
using GDI = System.Drawing;
using WinForms = System.Windows.Forms;

namespace SeeingSharp.Multimedia.Input
{
    internal class WinFormsKeyAndMouseInputHandler : IInputHandler
    {
        private const float MOVEMENT = 0.3f;
        private const float ROTATION = 0.01f;

        private static readonly Dictionary<WinForms.Keys, WinVirtualKey> s_keyMappingDict;

        // References to the view
        private WinForms.Control _currentControl;
        private RenderLoop _renderLoop;
        private IInputEnabledView _focusHandler;

        // Input states
        private MouseOrPointerState _stateMouseOrPointer;
        private KeyboardState _stateKeyboard;

        // Some helper variables
        private GDI.Point _lastMousePoint;
        private bool _isMouseInside;

        /// <summary>
        /// Initializes the <see cref="WinFormsKeyAndMouseInputHandler"/> class.
        /// </summary>
        static WinFormsKeyAndMouseInputHandler()
        {
            // First look for all key codes we have
            var supportedKeyCodes = new Dictionary<int, WinVirtualKey>();
            foreach (WinVirtualKey actVirtualKey in Enum.GetValues(typeof(WinVirtualKey)))
            {
                supportedKeyCodes[(int)actVirtualKey] = actVirtualKey;
            }

            // Build the mapping dictionary
            s_keyMappingDict = new Dictionary<WinForms.Keys, WinVirtualKey>();
            foreach (WinForms.Keys actKeyMember in Enum.GetValues(typeof(WinForms.Keys)))
            {
                var actKeyCode = (int)actKeyMember;
                if (supportedKeyCodes.ContainsKey(actKeyCode))
                {
                    s_keyMappingDict[actKeyMember] = supportedKeyCodes[actKeyCode];
                }
                else
                {
                    s_keyMappingDict[actKeyMember] = WinVirtualKey.None;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinFormsKeyAndMouseInputHandler"/> class.
        /// </summary>
        public WinFormsKeyAndMouseInputHandler()
        {
            _stateMouseOrPointer = new MouseOrPointerState();
            _stateMouseOrPointer.Internals.Type = MouseOrPointerType.Mouse;

            _stateKeyboard = new KeyboardState();
        }

        /// <summary>
        /// Gets a list containing all supported view types.
        /// </summary>
        public Type[] GetSupportedViewTypes()
        {
            return new[] { typeof(WinForms.Control), typeof(IInputControlHost) };
        }

        /// <summary>
        /// Starts input handling.
        /// </summary>
        /// <param name="viewObject">The view object (e. g. Direct3D11Canvas).</param>
        public void Start(IInputEnabledView viewObject)
        {
            _currentControl = viewObject as WinForms.Control;

            if (_currentControl == null)
            {
                var inputControlHost = viewObject as IInputControlHost;
                _currentControl = inputControlHost?.GetWinFormsInputControl();

                if (_currentControl == null)
                {
                    throw new ArgumentException("Unable to handle given view object!");
                }
            }

            _focusHandler = viewObject;

            if (_focusHandler == null)
            {
                throw new ArgumentException("Unable to handle given view object!");
            }

            _renderLoop = _focusHandler.RenderLoop;

            if (_renderLoop == null)
            {
                throw new ArgumentException("Unable to handle given view object!");
            }

            // Perform event registrations on UI thread
            viewObject.RenderLoop.UiSynchronizationContext.Post(arg =>
            {
                if (_currentControl == null)
                {
                    return;
                }

                _currentControl.MouseEnter += this.OnMouseEnter;
                _currentControl.MouseClick += this.OnMouseClick;
                _currentControl.MouseUp += this.OnMouseUp;
                _currentControl.MouseDown += this.OnMouseDown;
                _currentControl.MouseLeave += this.OnMouseLeave;
                _currentControl.MouseMove += this.OnMouseMove;
                _currentControl.MouseWheel += this.OnMouseWheel;
                _currentControl.KeyUp += this.OnKeyUp;
                _currentControl.KeyDown += this.OnKeyDown;
                _currentControl.LostFocus += this.OnLostFocus;
                _currentControl.GotFocus += this.OnGotFocus;

                // Handle initial focus state
                if (_currentControl.Focused || _currentControl.ContainsFocus)
                {
                    _stateKeyboard.Internals.NotifyFocusGot();
                }
            }, null);
        }

        /// <summary>
        /// Stops input handling.
        /// </summary>
        public void Stop()
        {
            // Perform event deregistrations on UI thread
            if (_currentControl != null)
            {
                var currentControl = _currentControl;

                var removeEventRegistrationsAction = new Action(() =>
                {
                    if (currentControl == null) { return; }

                    currentControl.MouseEnter -= this.OnMouseEnter;
                    currentControl.MouseClick -= this.OnMouseClick;
                    currentControl.MouseLeave -= this.OnMouseLeave;
                    currentControl.MouseMove -= this.OnMouseMove;
                    currentControl.MouseWheel -= this.OnMouseWheel;
                    currentControl.MouseUp -= this.OnMouseUp;
                    currentControl.MouseDown -= this.OnMouseDown;
                    currentControl.KeyUp -= this.OnKeyUp;
                    currentControl.KeyDown -= this.OnKeyDown;
                    currentControl.LostFocus -= this.OnLostFocus;
                    currentControl.GotFocus -= this.OnGotFocus;
                });

                if (_currentControl.IsHandleCreated) { _currentControl.BeginInvoke(removeEventRegistrationsAction); }
                else { removeEventRegistrationsAction(); }
            }

            // Set local references to zero
            _currentControl = null;
            _focusHandler = null;
            _renderLoop = null;
        }

        /// <summary>
        /// Queries all current input states.
        /// </summary>
        public void GetInputStates(List<InputStateBase> target)
        {
            target.Add(_stateMouseOrPointer);
            target.Add(_stateKeyboard);
        }

        /// <summary>
        /// Called when the mouse enters the screen.
        /// </summary>
        private void OnMouseEnter(object sender, EventArgs e)
        {
            if (_currentControl == null) { return; }

            _lastMousePoint = _currentControl.PointToClient(WinForms.Cursor.Position);
            _isMouseInside = true;

            _stateMouseOrPointer.Internals.NotifyInside(true);
        }

        /// <summary>
        /// Called when user clicks on this panel.
        /// </summary>
        private void OnMouseClick(object sender, WinForms.MouseEventArgs e)
        {
            _currentControl?.Focus();
        }

        private void OnMouseDown(object sender, WinForms.MouseEventArgs e)
        {
            if (_currentControl == null) { return; }

            switch (e.Button)
            {
                case WinForms.MouseButtons.Left:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Left);
                    break;

                case WinForms.MouseButtons.Middle:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Middle);
                    break;

                case WinForms.MouseButtons.Right:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Right);
                    break;

                case WinForms.MouseButtons.XButton1:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Extended1);
                    break;

                case WinForms.MouseButtons.XButton2:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Extended2);
                    break;
            }
        }

        private void OnMouseUp(object sender, WinForms.MouseEventArgs e)
        {
            if (_currentControl == null) { return; }

            switch (e.Button)
            {
                case WinForms.MouseButtons.Left:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Left);
                    break;

                case WinForms.MouseButtons.Middle:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Middle);
                    break;

                case WinForms.MouseButtons.Right:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Right);
                    break;

                case WinForms.MouseButtons.XButton1:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended1);
                    break;

                case WinForms.MouseButtons.XButton2:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended2);
                    break;
            }
        }

        /// <summary>
        /// Called when the mouse leaves the screen.
        /// </summary>
        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (_currentControl == null)
            {
                return;
            }

            _lastMousePoint = GDI.Point.Empty;
            _isMouseInside = false;

            _stateMouseOrPointer.Internals.NotifyInside(false);
        }

        /// <summary>
        /// Called when the mouse moves on the screen.
        /// </summary>
        private void OnMouseMove(object sender, WinForms.MouseEventArgs e)
        {
            if (_currentControl == null)
            {
                return;
            }

            if (_isMouseInside)
            {
                var moving = new GDI.Point(e.X - _lastMousePoint.X, e.Y - _lastMousePoint.Y);
                _lastMousePoint = e.Location;

                _stateMouseOrPointer.Internals.NotifyMouseLocation(
                    new Vector2(e.X, e.Y),
                    new Vector2(moving.X, moving.Y),
                    Vector2Ex.FromSize2(_renderLoop.ViewInformation.CurrentViewSize));
            }
        }

        /// <summary>
        /// Called when mouse wheel is used.
        /// </summary>
        private void OnMouseWheel(object sender, WinForms.MouseEventArgs e)
        {
            if (_currentControl == null)
            {
                return;
            }

            if (_isMouseInside)
            {
                _stateMouseOrPointer.Internals.NotifyMouseWheel(e.Delta);
            }
        }

        /// <summary>
        /// Called when a key is up
        /// </summary>
        private void OnKeyUp(object sender, WinForms.KeyEventArgs e)
        {
            if (_currentControl == null)
            {
                return;
            }

            // Notify event to keyboard state
            var actKeyCode = s_keyMappingDict[e.KeyCode];

            if (actKeyCode != WinVirtualKey.None)
            {
                _stateKeyboard.Internals.NotifyKeyUp(actKeyCode);
            }
        }

        /// <summary>
        /// Called when a key is down.
        /// </summary>
        private void OnKeyDown(object sender, WinForms.KeyEventArgs e)
        {
            if (_currentControl == null)
            {
                return;
            }

            // Notify event to keyboard state
            var actKeyCode = s_keyMappingDict[e.KeyCode];

            if (actKeyCode != WinVirtualKey.None)
            {
                _stateKeyboard.Internals.NotifyKeyDown(actKeyCode);
            }
        }

        /// <summary>
        /// Called when the focus on the target control is lost.
        /// </summary>
        private void OnLostFocus(object sender, EventArgs e)
        {
            if (_currentControl == null) { return; }

            _stateKeyboard.Internals.NotifyFocusLost();
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            if (_currentControl == null) { return; }

            _stateKeyboard.Internals.NotifyFocusGot();
        }
    }
}