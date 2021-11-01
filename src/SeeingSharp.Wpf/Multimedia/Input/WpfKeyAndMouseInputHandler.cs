using System;
using System.Collections.Generic;
using System.Numerics;
using System.Windows;
using System.Windows.Input;
using SeeingSharp.Multimedia.Views;

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
                _rendererElement.PreviewKeyDown += this.OnRendererElement_PreviewKeyDown;
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
                rendererElement.KeyUp -= this.OnRendererElement_KeyUp;
                rendererElement.PreviewKeyDown -= this.OnRendererElement_PreviewKeyDown;

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

        private void OnRendererElement_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (_rendererElement == null) { return; }

            var keyToProcess = e.Key;
            if (keyToProcess == Key.System) { keyToProcess = e.SystemKey;}

            switch (keyToProcess)
            {
                // Discard none
                case Key.None:
                    break;

                // Discard Oem keys
                case Key.Oem1:
                case Key.Oem2:
                case Key.Oem3:
                case Key.Oem4:
                case Key.Oem5:
                case Key.Oem6:
                case Key.Oem7:
                case Key.Oem8:
                case Key.Oem102:
                case Key.OemAttn:
                case Key.OemAuto:
                case Key.OemBackTab:
                case Key.OemClear:
                case Key.OemComma:
                case Key.OemCopy:
                case Key.OemEnlw:
                case Key.OemFinish:
                case Key.OemMinus:
                case Key.OemPeriod:
                case Key.OemPlus:
                    break;

                // Process all other keys
                default:
                    _stateKeyboard.Internals.NotifyKeyDown((WinVirtualKey) KeyInterop.VirtualKeyFromKey(keyToProcess));
                    break;
            }

            e.Handled = true;
        }

        private void OnRendererElement_KeyUp(object sender, KeyEventArgs e)
        {
            if (_rendererElement == null) { return; }

            var keyToProcess = e.Key;
            if (keyToProcess == Key.System) { keyToProcess = e.SystemKey;}

            switch (keyToProcess)
            {
                // Discard none
                case Key.None:
                    break;

                // Discard Oem keys
                case Key.Oem1:
                case Key.Oem2:
                case Key.Oem3:
                case Key.Oem4:
                case Key.Oem5:
                case Key.Oem6:
                case Key.Oem7:
                case Key.Oem8:
                case Key.Oem102:
                case Key.OemAttn:
                case Key.OemAuto:
                case Key.OemBackTab:
                case Key.OemClear:
                case Key.OemComma:
                case Key.OemCopy:
                case Key.OemEnlw:
                case Key.OemFinish:
                case Key.OemMinus:
                case Key.OemPeriod:
                case Key.OemPlus:
                    break;

                // Process all other keys
                default:
                    _stateKeyboard.Internals.NotifyKeyUp((WinVirtualKey)KeyInterop.VirtualKeyFromKey(keyToProcess));
                    break;
            }
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