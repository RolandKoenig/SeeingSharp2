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
using SeeingSharp.Multimedia.Views;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Input;

namespace SeeingSharp.Multimedia.Input
{
    internal class WpfKeyAndMouseInputHandler : IInputHandler
    {
        // Target objects
        private SeeingSharpRendererElement _rendererElement;

        // Helper
        private bool _lastDragPointValid;
        private System.Windows.Point _lastDragPoint;

        // Input states
        private MouseOrPointerState _stateMouseOrPointer;
        private KeyboardState _stateKeyboard;

        /// <summary>
        /// Initializes a new instance of the <see cref="WpfKeyAndMouseInputHandler"/> class.
        /// </summary>
        public WpfKeyAndMouseInputHandler()
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
            return new[]
            {
                typeof(SeeingSharpRendererElement)
            };
        }

        /// <summary>
        /// Starts input handling.
        /// </summary>
        /// <param name="viewObject">The view object (e. g. Direct3D11Canvas).</param>
        public void Start(IInputEnabledView viewObject)
        {
            _rendererElement = viewObject as SeeingSharpRendererElement;
            if (_rendererElement == null) { throw new ArgumentException("Unable to handle given view object!"); }

            // Register all events needed for mouse camera dragging
            _rendererElement.Dispatcher?.BeginInvoke(new Action(() =>
            {
                _rendererElement.MouseWheel += this.OnRendererElement_MouseWheel;
                _rendererElement.MouseDown += this.OnRendererElement_MouseDown;
                _rendererElement.MouseUp += this.OnRendererElement_MouseUp;
                _rendererElement.MouseMove += this.OnRendererElement_MouseMove;
                _rendererElement.MouseLeave += this.OnRendererElement_MouseLeave;
                _rendererElement.GotFocus += this.OnRenderElement_GotFocus;
                _rendererElement.LostFocus += this.OnRendererElement_LostFocus;
                _rendererElement.LostKeyboardFocus += this.OnRendererElement_LostKeyboardFocus;
                _rendererElement.PreviewMouseUp += this.OnRendererElement_PreviewMouseUp;
                _rendererElement.KeyUp += this.OnRendererElement_KeyUp;
                _rendererElement.KeyDown += this.OnRendererElement_KeyDown;
            }));
        }

        /// <summary>
        /// Stops input handling.
        /// </summary>
        public void Stop()
        {
            // Deregister all events
            var rendererElement = _rendererElement;
            rendererElement?.Dispatcher?.BeginInvoke(new Action(() =>
            {
                rendererElement.MouseWheel -= this.OnRendererElement_MouseWheel;
                rendererElement.MouseDown -= this.OnRendererElement_MouseDown;
                rendererElement.MouseUp -= this.OnRendererElement_MouseUp;
                rendererElement.MouseMove -= this.OnRendererElement_MouseMove;
                rendererElement.MouseLeave -= this.OnRendererElement_MouseLeave;
                rendererElement.LostFocus -= this.OnRendererElement_LostFocus;
                rendererElement.LostKeyboardFocus -= this.OnRendererElement_LostKeyboardFocus;
                rendererElement.GotFocus -= this.OnRenderElement_GotFocus;
                rendererElement.PreviewMouseUp -= this.OnRendererElement_PreviewMouseUp;
            }));

            _rendererElement = null;

            _stateKeyboard = new KeyboardState();
            _stateMouseOrPointer = new MouseOrPointerState();
        }

        /// <summary>
        /// Queries all current input states.
        /// </summary>
        public void GetInputStates(List<InputStateBase> target)
        {
            target.Add(_stateMouseOrPointer);
            target.Add(_stateKeyboard);
        }

        private void OnRendererElement_KeyDown(object sender, KeyEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _stateKeyboard.Internals.NotifyKeyDown((WinVirtualKey)KeyInterop.VirtualKeyFromKey(e.Key));
        }

        private void OnRendererElement_KeyUp(object sender, KeyEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _stateKeyboard.Internals.NotifyKeyUp((WinVirtualKey)KeyInterop.VirtualKeyFromKey(e.Key));
        }

        /// <summary>
        /// Called when user uses the mouse wheel for zooming.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnRendererElement_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _stateMouseOrPointer.Internals.NotifyMouseWheel(e.Delta);
        }

        private void OnRenderElement_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _stateKeyboard.Internals.NotifyFocusGot();
        }

        private void OnRendererElement_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _stateKeyboard.Internals.NotifyFocusLost();
        }

        private void OnRendererElement_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _stateKeyboard.Internals.NotifyFocusLost();
        }

        private void OnRendererElement_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _rendererElement.Focus();

            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Left);
                    break;

                case System.Windows.Input.MouseButton.Middle:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Middle);
                    break;

                case System.Windows.Input.MouseButton.Right:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Right);
                    break;

                case System.Windows.Input.MouseButton.XButton1:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Extended1);
                    break;

                case System.Windows.Input.MouseButton.XButton2:
                    _stateMouseOrPointer.Internals.NotifyButtonDown(MouseButton.Extended2);
                    break;
            }
        }

        private void OnRendererElement_MouseLeave(object sender, MouseEventArgs e)
        {
            if (_rendererElement == null) { return; }

            _stateMouseOrPointer.Internals.NotifyInside(false);

            _lastDragPointValid = false;
            _lastDragPoint = new System.Windows.Point();
        }

        private void OnRendererElement_MouseMove(object sender, MouseEventArgs e)
        {
            if (_rendererElement == null)
            {
                return;
            }

            _stateMouseOrPointer.Internals.NotifyInside(true);

            var currentPosition = e.GetPosition(_rendererElement);

            if (_lastDragPointValid)
            {
                var moveDistance = currentPosition - _lastDragPoint;
                var renderSize = _rendererElement.RenderSize;
                _stateMouseOrPointer.Internals.NotifyMouseLocation(
                    new Vector2((float)currentPosition.X, (float)currentPosition.Y),
                    new Vector2((float)moveDistance.X, (float)moveDistance.Y),
                    new Vector2((float)renderSize.Width, (float)renderSize.Height));
            }

            _lastDragPointValid = true;
            _lastDragPoint = currentPosition;
        }

        private void OnRendererElement_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_rendererElement == null) { return; }

            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Left);
                    break;

                case System.Windows.Input.MouseButton.Middle:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Middle);
                    break;

                case System.Windows.Input.MouseButton.Right:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Right);
                    break;

                case System.Windows.Input.MouseButton.XButton1:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended1);
                    break;

                case System.Windows.Input.MouseButton.XButton2:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended2);
                    break;
            }
        }

        private void OnRendererElement_PreviewMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (_rendererElement == null) { return; }

            switch (e.ChangedButton)
            {
                case System.Windows.Input.MouseButton.Left:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Left);
                    break;

                case System.Windows.Input.MouseButton.Middle:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Middle);
                    break;

                case System.Windows.Input.MouseButton.Right:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Right);
                    break;

                case System.Windows.Input.MouseButton.XButton1:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended1);
                    break;

                case System.Windows.Input.MouseButton.XButton2:
                    _stateMouseOrPointer.Internals.NotifyButtonUp(MouseButton.Extended2);
                    break;
            }
        }
    }
}