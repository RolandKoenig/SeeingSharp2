﻿/*
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
using System.Numerics;
using Windows.UI.Core;
using Microsoft.UI.Input;
using Microsoft.System;
using Microsoft.UI.Input.Experimental;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Views;

namespace SeeingSharp.Multimedia.Input
{
    internal class WinUIKeyAndMouseInputHandler : IInputHandler
    {
        private const float MOVEMENT = 0.3f;
        private const float ROTATION = 0.01f;
        private static readonly Dictionary<Windows.System.VirtualKey, WinVirtualKey> s_keyMappingDict;

        // State variables for camera movement
        private ExpPointerPoint _lastDragPoint;

        // Objects from outside
        private SeeingSharpPanelPainter _painter;
        private Panel _targetPanel;
        private IInputEnabledView _viewInterface;
        private RenderLoop _renderLoop;
        //private Window _coreWindow;
        private DispatcherQueue _dispatcherQueue;

        // Local resources
        private bool _hasFocus;

        // Input states
        private MouseOrPointerState _stateMouseOrPointer;
        private KeyboardState _stateKeyboard;

        /// <summary>
        /// Initializes the <see cref="WinUIKeyAndMouseInputHandler"/> class.
        /// </summary>
        static WinUIKeyAndMouseInputHandler()
        {
            s_keyMappingDict = new Dictionary<Windows.System.VirtualKey, WinVirtualKey>();
            foreach (Windows.System.VirtualKey actVirtualKey in Enum.GetValues(typeof(Windows.System.VirtualKey)))
            {
                var actVirtualKeyCode = (short)actVirtualKey;
                var actWinVirtualKey = (WinVirtualKey)actVirtualKeyCode;
                s_keyMappingDict[actVirtualKey] = actWinVirtualKey;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinUIKeyAndMouseInputHandler"/> class.
        /// </summary>
        public WinUIKeyAndMouseInputHandler()
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
                typeof(SeeingSharpPanelPainter)
            };
        }

        /// <summary>
        /// Starts input handling.
        /// </summary>
        /// <param name="viewObject">The view object (e. g. Direct3D11Canvas).</param>
        public void Start(IInputEnabledView viewObject)
        {
            _painter = viewObject as SeeingSharpPanelPainter;
            if (_painter == null) { throw new ArgumentException("Unable to handle given view object!"); }

            _viewInterface = _painter;
            if (_viewInterface == null) { throw new ArgumentException("Unable to handle given view object!"); }

            _renderLoop = _viewInterface.RenderLoop;
            if (_renderLoop == null) { throw new ArgumentException("Unable to handle given view object!"); }

            _dispatcherQueue = _painter.DispatcherQueue;
            if (_dispatcherQueue == null) { throw new ArgumentException("Unable to get CoreDispatcher from target panel!"); }

            // Delegate start logic to UI thread
            var queued = _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                // Register all events
                _targetPanel = _painter.TargetPanel;
                _targetPanel.PointerExited += this.OnTargetPanel_PointerExited;
                _targetPanel.PointerEntered += this.OnTargetPanel_PointerEntered;
                _targetPanel.PointerWheelChanged += this.OnTargetPanel_PointerWheelChanged;
                _targetPanel.PointerPressed += this.OnTargetPanel_PointerPressed;
                _targetPanel.PointerReleased += this.OnTargetPanel_PointerReleased;
                _targetPanel.PointerMoved += this.OnTargetPanel_PointerMoved;

                _targetPanel.KeyDown += this.OnTargetPanel_KeyDown;
                _targetPanel.KeyUp += this.OnTargetPanel_KeyUp;
                _targetPanel.LostFocus += this.OnTargetPanel_LostFocus;
                _targetPanel.GotFocus += this.OnTargetPanel_GotFocus;
                
                //_coreWindow = CoreWindow.GetForCurrentThread();
                //_coreWindow.KeyDown += this.OnCoreWindow_KeyDown;
                //_coreWindow.KeyUp += this.OnCoreWindow_KeyUp;

                // Set focus on the target
                _targetPanel.Focus(FocusState.Programmatic);
            });
            if (!queued)
            {
                throw new SeeingSharpException(
                    $"Unable to attach {nameof(WinUIKeyAndMouseInputHandler)} to view {viewObject}!");
            }
        }

        /// <summary>
        /// Stops input handling.
        /// </summary>
        public void Stop()
        {
            _hasFocus = false;
            if (_painter == null) { return; }
            if (_dispatcherQueue == null) { return; }

            // Deregister all events on UI thread
            var painter = _painter;
            //var coreWindow = _coreWindow;

            var uiTask = _dispatcherQueue.TryEnqueue(DispatcherQueuePriority.Normal, () =>
            {
                // Deregister all events
                _targetPanel.PointerExited -= this.OnTargetPanel_PointerExited;
                _targetPanel.PointerEntered -= this.OnTargetPanel_PointerEntered;
                _targetPanel.PointerWheelChanged -= this.OnTargetPanel_PointerWheelChanged;
                _targetPanel.PointerPressed -= this.OnTargetPanel_PointerPressed;
                _targetPanel.PointerReleased -= this.OnTargetPanel_PointerReleased;
                _targetPanel.PointerMoved -= this.OnTargetPanel_PointerMoved;

                _targetPanel.KeyDown -= this.OnTargetPanel_KeyDown;
                _targetPanel.KeyUp -= this.OnTargetPanel_KeyUp;
                _targetPanel.LostFocus -= this.OnTargetPanel_LostFocus;
                _targetPanel.GotFocus -= this.OnTargetPanel_GotFocus;

                _targetPanel = null;

                //// Deregister events from CoreWindow
                //coreWindow.KeyDown -= this.OnCoreWindow_KeyDown;
                //coreWindow.KeyUp -= this.OnCoreWindow_KeyUp;
            });

            // set all references to zero
            _painter = null;
            //_coreWindow = null;
            _dispatcherQueue = null;
        }

        /// <summary>
        /// Queries all current input states.
        /// </summary>
        public void GetInputStates(List<InputStateBase> target)
        {
            target.Add(_stateMouseOrPointer);
            target.Add(_stateKeyboard);
        }

        private void OnTargetPanel_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_painter == null) { return; }

            // This enables bubbling of the keyboard event
            e.Handled = false;
        }

        private void OnTargetPanel_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_painter == null) { return; }

            // This enables bubbling of the keyboard event
            e.Handled = false;
        }

        private void OnTargetPanel_LostFocus(object sender, RoutedEventArgs e)
        {
            if (_painter == null) { return; }

            _stateKeyboard.Internals.NotifyFocusLost();

            _hasFocus = false;
        }

        private void OnCoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            if (_painter == null) { return; }
            if (!_hasFocus) { return; }
            if (!s_keyMappingDict.ContainsKey(e.VirtualKey)) { return; }

            _stateKeyboard.Internals.NotifyKeyDown(s_keyMappingDict[e.VirtualKey]);
        }

        private void OnCoreWindow_KeyUp(CoreWindow sender, KeyEventArgs e)
        {
            if (_painter == null) { return; }
            if (!_hasFocus) { return; }
            if (!s_keyMappingDict.ContainsKey(e.VirtualKey)) { return; }

            _stateKeyboard.Internals.NotifyKeyUp(s_keyMappingDict[e.VirtualKey]);
        }

        private void OnTargetPanel_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_painter == null) { return; }

            _stateKeyboard.Internals.NotifyFocusGot();
            _hasFocus = true;
        }

        private void OnTargetPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_painter == null) { return; }
            if (_targetPanel == null) { return; }

            // Track mouse/pointer state
            var currentPoint = e.GetCurrentPoint(_targetPanel);
            var pointProperties = currentPoint.Properties;

            if (pointProperties.IsPrimary)
            {
                _stateMouseOrPointer.Internals.NotifyButtonStates(
                    pointProperties.IsLeftButtonPressed,
                    pointProperties.IsMiddleButtonPressed,
                    pointProperties.IsRightButtonPressed,
                    pointProperties.IsXButton1Pressed,
                    pointProperties.IsXButton2Pressed);
            }

            // Needed here because we loose focus again by default on left mouse button
            e.Handled = true;
        }

        private void OnTargetPanel_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_painter == null) { return; }
            if (_targetPanel == null) { return; }

            // Set focus on target
            _targetPanel.Focus(FocusState.Programmatic);

            // Track mouse/pointer state
            var currentPoint = e.GetCurrentPoint(_targetPanel);
            var pointProperties = currentPoint.Properties;

            if (pointProperties.IsPrimary)
            {
                _stateMouseOrPointer.Internals.NotifyButtonStates(
                    pointProperties.IsLeftButtonPressed,
                    pointProperties.IsMiddleButtonPressed,
                    pointProperties.IsRightButtonPressed,
                    pointProperties.IsXButton1Pressed,
                    pointProperties.IsXButton2Pressed);
            }

            _lastDragPoint = currentPoint;

            // Needed here because we loose focus again by default on left mouse button
            e.Handled = true;
        }

        private void OnTargetPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (_painter == null) { return; }
            if (_targetPanel == null) { return; }

            // Calculate move distance
            var currentPoint = e.GetCurrentPoint(_targetPanel);

            if (_lastDragPoint == null)
            {
                _lastDragPoint = currentPoint;
            }

            var moveDistance = new Vector2(
                (float)(currentPoint.Position.X - _lastDragPoint.Position.X),
                (float)(currentPoint.Position.Y - _lastDragPoint.Position.Y));
            var currentLocation = new Vector2(
                (float)currentPoint.Position.X,
                (float)currentPoint.Position.Y);

            // Track mouse/pointer state
            var pointProperties = currentPoint.Properties;

            if (pointProperties.IsPrimary)
            {
                _stateMouseOrPointer.Internals.NotifyButtonStates(
                    pointProperties.IsLeftButtonPressed,
                    pointProperties.IsMiddleButtonPressed,
                    pointProperties.IsRightButtonPressed,
                    pointProperties.IsXButton1Pressed,
                    pointProperties.IsXButton2Pressed);

                var actSize = _painter.ActualSize;
                _stateMouseOrPointer.Internals.NotifyMouseLocation(
                    currentLocation, moveDistance, actSize.ToVector2());
            }

            // Store last drag point
            _lastDragPoint = currentPoint;
        }

        private void OnTargetPanel_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (_painter == null) { return; }
            if (!_hasFocus) { return; }
            if (_targetPanel == null) { return; }

            // Track mouse/pointer state
            var currentPoint = e.GetCurrentPoint(_targetPanel);
            var pointProperties = currentPoint.Properties;
            var wheelDelta = pointProperties.MouseWheelDelta;

            if (pointProperties.IsPrimary)
            {
                _stateMouseOrPointer.Internals.NotifyButtonStates(
                    pointProperties.IsLeftButtonPressed,
                    pointProperties.IsMiddleButtonPressed,
                    pointProperties.IsRightButtonPressed,
                    pointProperties.IsXButton1Pressed,
                    pointProperties.IsXButton2Pressed);
                _stateMouseOrPointer.Internals.NotifyMouseWheel(wheelDelta);
            }
        }

        /// <summary>
        /// Called when mouse leaves the target panel.
        /// </summary>
        private void OnTargetPanel_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (_painter == null) { return; }

            _stateMouseOrPointer.Internals.NotifyInside(false);
        }

        private void OnTargetPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (_painter == null) { return; }

            _stateMouseOrPointer.Internals.NotifyInside(true);
        }
    }
}