#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Input
{
    class WinRTKeyAndMouseInputHandler : IInputHandler
    {
        private const float MOVEMENT = 0.3f;
        private const float ROTATION = 0.01f;
        private static readonly Dictionary<VirtualKey, WinVirtualKey> s_keyMappingDict;

        #region objects from outside
        private SeeingSharpPanelPainter m_painter;
        private IInputEnabledView m_viewInterface;
        private RenderLoop m_renderLoop;
        private CoreWindow m_coreWindow;
        private CoreDispatcher m_dispatcher;
        #endregion

        #region local resources
        private Button m_dummyButtonForFocus;
        private bool m_hasFocus;
        #endregion

        #region Input states
        private MouseOrPointerState m_stateMouseOrPointer;
        private KeyboardState m_stateKeyboard;
        #endregion

        #region state variables for camera movement
        private PointerPoint m_lastDragPoint;
        #endregion

        /// <summary>
        /// Initializes the <see cref="WinRTKeyAndMouseInputHandler"/> class.
        /// </summary>
        static WinRTKeyAndMouseInputHandler()
        {
            s_keyMappingDict = new Dictionary<VirtualKey, WinVirtualKey>();
            foreach(VirtualKey actVirtualKey in Enum.GetValues(typeof(VirtualKey)))
            {
                short actVirtualKeyCode = (short)actVirtualKey;
                WinVirtualKey actWinVirtualKey = (WinVirtualKey)actVirtualKeyCode;
                s_keyMappingDict[actVirtualKey] = actWinVirtualKey;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRTKeyAndMouseInputHandler"/> class.
        /// </summary>
        public WinRTKeyAndMouseInputHandler()
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
            return new Type[]
            {
                typeof(SeeingSharpPanelPainter),
            };
        }

        /// <summary>
        /// Starts input handling.
        /// </summary>
        /// <param name="viewObject">The view object (e. g. Direct3D11Canvas).</param>
        public void Start(IInputEnabledView viewObject)
        {
            m_painter = viewObject as SeeingSharpPanelPainter;
            if (m_painter == null) { throw new ArgumentException("Unable to handle given view object!"); }

            m_viewInterface = m_painter as IInputEnabledView;
            if (m_viewInterface == null) { throw new ArgumentException("Unable to handle given view object!"); }

            m_renderLoop = m_viewInterface.RenderLoop;
            if (m_renderLoop == null) { throw new ArgumentException("Unable to handle given view object!"); }

            m_dispatcher = m_painter.Disptacher;
            if(m_dispatcher == null) { throw new ArgumentException("Unable to get CoreDisptacher from target panel!"); }

            // Deligate start logic to UI thread
            var uiTask = m_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Register all events
                m_painter.TargetPanel.PointerExited += OnTargetPanel_PointerExited;
                m_painter.TargetPanel.PointerEntered += OnTargetPanel_PointerEntered;
                m_painter.TargetPanel.PointerWheelChanged += OnTargetPanel_PointerWheelChanged;
                m_painter.TargetPanel.PointerPressed += OnTargetPanel_PointerPressed;
                m_painter.TargetPanel.PointerReleased += OnTargetPanel_PointerReleased;
                m_painter.TargetPanel.PointerMoved += OnTargetPanel_PointerMoved;

                // Create the dummy button for focus management
                //  see posts on: https://social.msdn.microsoft.com/Forums/en-US/54e4820d-d782-45d9-a2b1-4e3a13340788/set-focus-on-swapchainpanel-control?forum=winappswithcsharp
                m_dummyButtonForFocus = new Button();
                m_dummyButtonForFocus.Content = "Button";
                m_dummyButtonForFocus.Width = 0;
                m_dummyButtonForFocus.Height = 0;
                m_dummyButtonForFocus.HorizontalAlignment = HorizontalAlignment.Left;
                m_dummyButtonForFocus.VerticalAlignment = VerticalAlignment.Top;
                m_dummyButtonForFocus.KeyDown += OnDummyButtonForFocus_KeyDown;
                m_dummyButtonForFocus.KeyUp += OnDummyButtonForFocus_KeyUp;
                m_dummyButtonForFocus.LostFocus += OnDummyButtonForFocus_LostFocus;
                m_dummyButtonForFocus.GotFocus += OnDummyButtonForFocus_GotFocus;
                m_painter.TargetPanel.Children.Add(m_dummyButtonForFocus);

                m_coreWindow = CoreWindow.GetForCurrentThread();
                m_coreWindow.KeyDown += OnCoreWindow_KeyDown;
                m_coreWindow.KeyUp += OnCoreWindow_KeyUp;

                // Set focus on the target 
                m_dummyButtonForFocus.Focus(FocusState.Programmatic);
            });
        }

        /// <summary>
        /// Stops input handling.
        /// </summary>
        public void Stop()
        {
            m_hasFocus = false;
            if(m_painter == null) { return; }
            if(m_dispatcher == null) { return; }

            // Deregister all events on UI thread
            Button dummyButtonForFocus = m_dummyButtonForFocus;
            SeeingSharpPanelPainter painter = m_painter;
            CoreWindow coreWindow = m_coreWindow;
            var uiTask = m_dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Remove the dummy button
                if (dummyButtonForFocus != null)
                {
                    dummyButtonForFocus.KeyDown -= OnDummyButtonForFocus_KeyDown;
                    dummyButtonForFocus.KeyUp -= OnDummyButtonForFocus_KeyUp;
                    dummyButtonForFocus.LostFocus -= OnDummyButtonForFocus_LostFocus;
                    dummyButtonForFocus.GotFocus -= OnDummyButtonForFocus_GotFocus;

                    painter.TargetPanel.Children.Remove(dummyButtonForFocus);
                }

                // Deregister all events
                painter.TargetPanel.PointerExited -= OnTargetPanel_PointerExited;
                painter.TargetPanel.PointerEntered -= OnTargetPanel_PointerEntered;
                painter.TargetPanel.PointerWheelChanged -= OnTargetPanel_PointerWheelChanged;
                painter.TargetPanel.PointerPressed -= OnTargetPanel_PointerPressed;
                painter.TargetPanel.PointerReleased -= OnTargetPanel_PointerReleased;
                painter.TargetPanel.PointerMoved -= OnTargetPanel_PointerMoved;

                // Deregister events from CoreWindow
                coreWindow.KeyDown -= OnCoreWindow_KeyDown;
                coreWindow.KeyUp -= OnCoreWindow_KeyUp;
            });

            // set all references to zero
            m_dummyButtonForFocus = null;
            m_painter = null;
            m_coreWindow = null;
            m_dispatcher = null;
        }

        /// <summary>
        /// Querries all current input states.
        /// </summary>
        public IEnumerable<InputStateBase> GetInputStates()
        {
            yield return m_stateMouseOrPointer;
            yield return m_stateKeyboard;
        }

        private void OnDummyButtonForFocus_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if(m_painter == null) { return; }

            // This enables bubbling of the keyboard event
            e.Handled = false;
        }

        private void OnDummyButtonForFocus_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (m_painter == null) { return; }

            // This enables bubbling of the keyboard event
            e.Handled = false;
        }

        private void OnDummyButtonForFocus_LostFocus(object sender, RoutedEventArgs e)
        {
            if (m_painter == null) { return; }

            m_stateKeyboard.Internals.NotifyFocusLost();

            m_hasFocus = false;
        }

        private void OnCoreWindow_KeyDown(CoreWindow sender, KeyEventArgs e)
        {
            if (m_painter == null) { return; }
            if (!m_hasFocus) { return; }
            if (!s_keyMappingDict.ContainsKey(e.VirtualKey)) { return; }

            m_stateKeyboard.Internals.NotifyKeyDown(s_keyMappingDict[e.VirtualKey]);
        }

        private void OnCoreWindow_KeyUp(CoreWindow sender, KeyEventArgs e)
        {
            if (m_painter == null) { return; }
            if (!m_hasFocus) { return; }
            if (!s_keyMappingDict.ContainsKey(e.VirtualKey)) { return; }

            m_stateKeyboard.Internals.NotifyKeyUp(s_keyMappingDict[e.VirtualKey]);
        }

        private void OnDummyButtonForFocus_GotFocus(object sender, RoutedEventArgs e)
        {
            if (m_painter == null) { return; }

            m_stateKeyboard.Internals.NotifyFocusGot();
            m_hasFocus = true;
        }

        private void OnTargetPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (m_painter == null) { return; }

            // Set focus on target
            if (m_dummyButtonForFocus != null)
            {
                m_dummyButtonForFocus.Focus(FocusState.Programmatic);
            }

            // Track mouse/pointer state
            PointerPoint currentPoint = e.GetCurrentPoint(m_painter.TargetPanel);
            PointerPointProperties pointProperties = currentPoint.Properties;
            if (pointProperties.IsPrimary)
            {
                m_stateMouseOrPointer.Internals.NotifyButtonStates(
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
            if (m_painter == null) { return; }

            // Set focus on target
            if (m_dummyButtonForFocus != null)
            {
                m_dummyButtonForFocus.Focus(FocusState.Programmatic);
            }

            // Track mouse/pointer state
            PointerPoint currentPoint = e.GetCurrentPoint(m_painter.TargetPanel);
            PointerPointProperties pointProperties = currentPoint.Properties;
            if (pointProperties.IsPrimary)
            {
                m_stateMouseOrPointer.Internals.NotifyButtonStates(
                    pointProperties.IsLeftButtonPressed,
                    pointProperties.IsMiddleButtonPressed,
                    pointProperties.IsRightButtonPressed,
                    pointProperties.IsXButton1Pressed,
                    pointProperties.IsXButton2Pressed);
            }
            m_lastDragPoint = currentPoint;

            // Needed here because we loose focus again by default on left mouse button
            e.Handled = true;
        }

        private void OnTargetPanel_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (m_painter == null) { return; }

            // Calculate move distance
            PointerPoint currentPoint = e.GetCurrentPoint(m_painter.TargetPanel);
            if(m_lastDragPoint == null) { m_lastDragPoint = currentPoint; }
            Vector2 moveDistance = new Vector2(
                (float)(currentPoint.Position.X - m_lastDragPoint.Position.X),
                (float)(currentPoint.Position.Y - m_lastDragPoint.Position.Y));
            Vector2 currentLocation = new Vector2(
                (float)currentPoint.Position.X,
                (float)currentPoint.Position.Y);

            // Track mouse/pointer state
            PointerPointProperties pointProperties = currentPoint.Properties;
            if (pointProperties.IsPrimary)
            {
                m_stateMouseOrPointer.Internals.NotifyButtonStates(
                    pointProperties.IsLeftButtonPressed,
                    pointProperties.IsMiddleButtonPressed,
                    pointProperties.IsRightButtonPressed,
                    pointProperties.IsXButton1Pressed,
                    pointProperties.IsXButton2Pressed);

                var actSize = m_painter.ActualSize;
                m_stateMouseOrPointer.Internals.NotifyMouseLocation(
                    currentLocation, moveDistance, actSize.ToVector2());
            }

            // Store last drag point
            m_lastDragPoint = currentPoint;
        }

        private void OnTargetPanel_PointerWheelChanged(object sender, PointerRoutedEventArgs e)
        {
            if (m_painter == null) { return; }
            if (!m_hasFocus) { return; }

            // Track mouse/pointer state
            PointerPoint currentPoint = e.GetCurrentPoint(m_painter.TargetPanel);
            PointerPointProperties pointProperties = currentPoint.Properties;
            int wheelDelta = pointProperties.MouseWheelDelta;
            if (pointProperties.IsPrimary)
            {
                m_stateMouseOrPointer.Internals.NotifyButtonStates(
                    pointProperties.IsLeftButtonPressed,
                    pointProperties.IsMiddleButtonPressed,
                    pointProperties.IsRightButtonPressed,
                    pointProperties.IsXButton1Pressed,
                    pointProperties.IsXButton2Pressed);
                m_stateMouseOrPointer.Internals.NotifyMouseWheel(wheelDelta);
            }
        }

        /// <summary>
        /// Called when mouse leaves the target panel.
        /// </summary>
        private void OnTargetPanel_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (m_painter == null) { return; }

            m_stateMouseOrPointer.Internals.NotifyInside(false);
        }

        private void OnTargetPanel_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (m_painter == null) { return; }

            m_stateMouseOrPointer.Internals.NotifyInside(true);
        }
    }
}