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
using System.Collections.Generic;
using SeeingSharp.Multimedia.Core;
using SharpDX;
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
        private WinForms.Control m_currentControl;
        private RenderLoop m_renderLoop;
        private IInputEnabledView m_focusHandler;

        // Input states
        private MouseOrPointerState m_stateMouseOrPointer;
        private KeyboardState m_stateKeyboard;

        // Some helper variables
        private GDI.Point m_lastMousePoint;
        private bool m_isMouseInside;

        /// <summary>
        /// Initializes the <see cref="WinFormsKeyAndMouseInputHandler"/> class.
        /// </summary>
        static WinFormsKeyAndMouseInputHandler()
        {
            // First look for all key codes we have
            var supportedKeyCodes = new Dictionary<int, WinVirtualKey>();
            foreach(WinVirtualKey actVirtualKey in Enum.GetValues(typeof(WinVirtualKey)))
            {
                supportedKeyCodes[(int)actVirtualKey] = actVirtualKey;
            }

            // Build the mapping dictionary
            s_keyMappingDict = new Dictionary<WinForms.Keys, WinVirtualKey>();
            foreach (WinForms.Keys actKeyMember in Enum.GetValues(typeof(WinForms.Keys)))
            {
                var actKeyCode = (int)actKeyMember;
                if(supportedKeyCodes.ContainsKey(actKeyCode))
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
            m_stateMouseOrPointer = new MouseOrPointerState();
            m_stateMouseOrPointer.Internals.Type = MouseOrPointerType.Mouse;

            m_stateKeyboard = new KeyboardState();
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
            m_currentControl = viewObject as WinForms.Control;

            if (m_currentControl == null)
            {
                var inputControlHost = viewObject as IInputControlHost;
                m_currentControl = inputControlHost?.GetWinFormsInputControl();

                if (m_currentControl == null)
                {
                    throw new ArgumentException("Unable to handle given view object!");
                }
            }

            m_focusHandler = viewObject;

            if (m_focusHandler == null)
            {
                throw new ArgumentException("Unable to handle given view object!");
            }

            m_renderLoop = m_focusHandler.RenderLoop;

            if (m_renderLoop == null)
            {
                throw new ArgumentException("Unable to handle given view object!");
            }

            // Perform event registrations on UI thread
            viewObject.RenderLoop.UISynchronizationContext.Post(arg =>
            {
                if (m_currentControl == null)
                {
                    return;
                }

                m_currentControl.MouseEnter += OnMouseEnter;
                m_currentControl.MouseClick += OnMouseClick;
                m_currentControl.MouseUp += OnMouseUp;
                m_currentControl.MouseDown += OnMouseDown;
                m_currentControl.MouseLeave += OnMouseLeave;
                m_currentControl.MouseMove += OnMouseMove;
                m_currentControl.MouseWheel += OnMouseWheel;
                m_currentControl.KeyUp += OnKeyUp;
                m_currentControl.KeyDown += OnKeyDown;
                m_currentControl.LostFocus += OnLostFocus;
                m_currentControl.GotFocus += OnGotFocus;

                // Handle initial focus state
                if (m_currentControl.Focused || m_currentControl.ContainsFocus)
                {
                    m_stateKeyboard.Internals.NotifyFocusGot();
                }
            }, null);
        }

        /// <summary>
        /// Stops input handling.
        /// </summary>
        public void Stop()
        {
            // Perform event deregistrations on UI thread
            if (m_currentControl != null)
            {
                var currentControl = m_currentControl;

                var removeEventRegistrationsAction = new Action(() =>
                {
                    if(currentControl == null) { return; }

                    currentControl.MouseEnter -= OnMouseEnter;
                    currentControl.MouseClick -= OnMouseClick;
                    currentControl.MouseLeave -= OnMouseLeave;
                    currentControl.MouseMove -= OnMouseMove;
                    currentControl.MouseWheel -= OnMouseWheel;
                    currentControl.MouseUp -= OnMouseUp;
                    currentControl.MouseDown -= OnMouseDown;
                    currentControl.KeyUp -= OnKeyUp;
                    currentControl.KeyDown -= OnKeyDown;
                    currentControl.LostFocus -= OnLostFocus;
                    currentControl.GotFocus -= OnGotFocus;
                });

                if (m_currentControl.IsHandleCreated) { m_currentControl.BeginInvoke(removeEventRegistrationsAction); }
                else { removeEventRegistrationsAction(); }
            }

            // Set local references to zero
            m_currentControl = null;
            m_focusHandler = null;
            m_renderLoop = null;
        }

        /// <summary>
        /// Queries all current input states.
        /// </summary>
        public IEnumerable<InputStateBase> GetInputStates()
        {
            yield return m_stateMouseOrPointer;
            yield return m_stateKeyboard;
        }

        /// <summary>
        /// Called when the mouse enters the screen.
        /// </summary>
        private void OnMouseEnter(object sender, EventArgs e)
        {
            if(m_currentControl == null) { return; }

            m_lastMousePoint = m_currentControl.PointToClient(WinForms.Cursor.Position);
            m_isMouseInside = true;

            m_stateMouseOrPointer.Internals.NotifyInside(true);
        }

        /// <summary>
        /// Called when user clicks on this panel.
        /// </summary>
        private void OnMouseClick(object sender, WinForms.MouseEventArgs e)
        {
            m_currentControl?.Focus();
        }

        private void OnMouseDown(object sender, WinForms.MouseEventArgs e)
        {
            if (m_currentControl == null) { return; }

            switch (e.Button)
            {
                case WinForms.MouseButtons.Left:
                    m_stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Left);
                    break;

                case WinForms.MouseButtons.Middle:
                    m_stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Middle);
                    break;

                case WinForms.MouseButtons.Right:
                    m_stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Right);
                    break;

                case WinForms.MouseButtons.XButton1:
                    m_stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Extended1);
                    break;

                case WinForms.MouseButtons.XButton2:
                    m_stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Extended2);
                    break;
            }
        }

        private void OnMouseUp(object sender, WinForms.MouseEventArgs e)
        {
            if (m_currentControl == null) { return; }

            switch (e.Button)
            {
                case WinForms.MouseButtons.Left:
                    m_stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Left);
                    break;

                case WinForms.MouseButtons.Middle:
                    m_stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Middle);
                    break;

                case WinForms.MouseButtons.Right:
                    m_stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Right);
                    break;

                case WinForms.MouseButtons.XButton1:
                    m_stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended1);
                    break;

                case WinForms.MouseButtons.XButton2:
                    m_stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended2);
                    break;
            }
        }

        /// <summary>
        /// Called when the mouse leaves the screen.
        /// </summary>
        private void OnMouseLeave(object sender, EventArgs e)
        {
            if (m_currentControl == null)
            {
                return;
            }

            m_lastMousePoint = GDI.Point.Empty;
            m_isMouseInside = false;

            m_stateMouseOrPointer.Internals.NotifyInside(false);
        }

        /// <summary>
        /// Called when the mouse moves on the screen.
        /// </summary>
        private void OnMouseMove(object sender, WinForms.MouseEventArgs e)
        {
            if (m_currentControl == null)
            {
                return;
            }

            if (m_isMouseInside)
            {
                var moving = new GDI.Point(e.X - m_lastMousePoint.X, e.Y - m_lastMousePoint.Y);
                m_lastMousePoint = e.Location;

                m_stateMouseOrPointer.Internals.NotifyMouseLocation(
                    new Vector2(e.X, e.Y),
                    new Vector2(moving.X, moving.Y),
                    Vector2Ex.FromSize2(m_renderLoop.ViewInformation.CurrentViewSize));
            }
        }

        /// <summary>
        /// Called when mouse wheel is used.
        /// </summary>
        private void OnMouseWheel(object sender, WinForms.MouseEventArgs e)
        {
            if (m_currentControl == null)
            {
                return;
            }

            if (m_isMouseInside)
            {
                m_stateMouseOrPointer.Internals.NotifyMouseWheel(e.Delta);
            }
        }

        /// <summary>
        /// Called when a key is up
        /// </summary>
        private void OnKeyUp(object sender, WinForms.KeyEventArgs e)
        {
            if (m_currentControl == null)
            {
                return;
            }

            // Notify event to keyboard state
            var actKeyCode = s_keyMappingDict[e.KeyCode];

            if (actKeyCode != WinVirtualKey.None)
            {
                m_stateKeyboard.Internals.NotifyKeyUp(actKeyCode);
            }
        }

        /// <summary>
        /// Called when a key is down.
        /// </summary>
        private void OnKeyDown(object sender, WinForms.KeyEventArgs e)
        {
            if (m_currentControl == null)
            {
                return;
            }

            // Notify event to keyboard state
            var actKeyCode = s_keyMappingDict[e.KeyCode];

            if (actKeyCode != WinVirtualKey.None)
            {
                m_stateKeyboard.Internals.NotifyKeyDown(actKeyCode);
            }
        }

        /// <summary>
        /// Called when the focus on the target control is lost.
        /// </summary>
        private void OnLostFocus(object sender, EventArgs e)
        {
            if (m_currentControl == null) { return; }

            m_stateKeyboard.Internals.NotifyFocusLost();
        }

        private void OnGotFocus(object sender, EventArgs e)
        {
            if (m_currentControl == null) { return; }

            m_stateKeyboard.Internals.NotifyFocusGot();
        }
    }
}