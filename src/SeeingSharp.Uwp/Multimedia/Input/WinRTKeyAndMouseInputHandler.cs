using System;
using System.Collections.Generic;
using System.Numerics;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Views;

namespace SeeingSharp.Multimedia.Input
{
    internal class WinRTKeyAndMouseInputHandler : IInputHandler
    {
        private const float MOVEMENT = 0.3f;
        private const float ROTATION = 0.01f;
        private static readonly Dictionary<VirtualKey, WinVirtualKey> s_keyMappingDict;

        // State variables for camera movement
        private PointerPoint _lastDragPoint;

        // Objects from outside
        private SeeingSharpPanelPainter _painter;
        private Panel _targetPanel;
        private IInputEnabledView _viewInterface;
        private RenderLoop _renderLoop;
        private CoreWindow _coreWindow;
        private CoreDispatcher _dispatcher;

        // Local resources
        private Button _dummyButtonForFocus;
        private bool _hasFocus;

        // Input states
        private MouseOrPointerState _stateMouseOrPointer;
        private KeyboardState _stateKeyboard;

        /// <summary>
        /// Initializes the <see cref="WinRTKeyAndMouseInputHandler"/> class.
        /// </summary>
        static WinRTKeyAndMouseInputHandler()
        {
            s_keyMappingDict = new Dictionary<VirtualKey, WinVirtualKey>();
            foreach (VirtualKey actVirtualKey in Enum.GetValues(typeof(VirtualKey)))
            {
                var actVirtualKeyCode = (short)actVirtualKey;
                var actWinVirtualKey = (WinVirtualKey)actVirtualKeyCode;
                s_keyMappingDict[actVirtualKey] = actWinVirtualKey;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WinRTKeyAndMouseInputHandler"/> class.
        /// </summary>
        public WinRTKeyAndMouseInputHandler()
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

            _dispatcher = _painter.Dispatcher;
            if (_dispatcher == null) { throw new ArgumentException("Unable to get CoreDispatcher from target panel!"); }

            // Delegate start logic to UI thread
            _ = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // Register all events
                _targetPanel = _painter.TargetPanel;
                _targetPanel.PointerExited += this.OnTargetPanel_PointerExited;
                _targetPanel.PointerEntered += this.OnTargetPanel_PointerEntered;
                _targetPanel.PointerWheelChanged += this.OnTargetPanel_PointerWheelChanged;
                _targetPanel.PointerPressed += this.OnTargetPanel_PointerPressed;
                _targetPanel.PointerReleased += this.OnTargetPanel_PointerReleased;
                _targetPanel.PointerMoved += this.OnTargetPanel_PointerMoved;

                // Create the dummy button for focus management
                //  see posts on: https://social.msdn.microsoft.com/Forums/en-US/54e4820d-d782-45d9-a2b1-4e3a13340788/set-focus-on-swapchainpanel-control?forum=winappswithcsharp
                _dummyButtonForFocus = new Button
                {
                    Content = "Button",
                    Width = 0,
                    Height = 0,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top,
                    BorderThickness = new Thickness(0.0)
                };

                _dummyButtonForFocus.KeyDown += this.OnDummyButtonForFocus_KeyDown;
                _dummyButtonForFocus.KeyUp += this.OnDummyButtonForFocus_KeyUp;
                _dummyButtonForFocus.LostFocus += this.OnDummyButtonForFocus_LostFocus;
                _dummyButtonForFocus.GotFocus += this.OnDummyButtonForFocus_GotFocus;
                _targetPanel.Children.Insert(0, _dummyButtonForFocus);

                _coreWindow = CoreWindow.GetForCurrentThread();
                _coreWindow.KeyDown += this.OnCoreWindow_KeyDown;
                _coreWindow.KeyUp += this.OnCoreWindow_KeyUp;

                // Set focus on the target
                _dummyButtonForFocus.Focus(FocusState.Programmatic);
            });
        }

        /// <summary>
        /// Stops input handling.
        /// </summary>
        public void Stop()
        {
            _hasFocus = false;
            if (_painter == null) { return; }
            if (_dispatcher == null) { return; }

            // Deregister all events on UI thread
            var dummyButtonForFocus = _dummyButtonForFocus;
            var coreWindow = _coreWindow;
            _ = _dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // RemoveObject the dummy button
                if (dummyButtonForFocus != null)
                {
                    dummyButtonForFocus.KeyDown -= this.OnDummyButtonForFocus_KeyDown;
                    dummyButtonForFocus.KeyUp -= this.OnDummyButtonForFocus_KeyUp;
                    dummyButtonForFocus.LostFocus -= this.OnDummyButtonForFocus_LostFocus;
                    dummyButtonForFocus.GotFocus -= this.OnDummyButtonForFocus_GotFocus;

                    _targetPanel.Children.Remove(dummyButtonForFocus);
                }

                // Deregister all events
                _targetPanel.PointerExited -= this.OnTargetPanel_PointerExited;
                _targetPanel.PointerEntered -= this.OnTargetPanel_PointerEntered;
                _targetPanel.PointerWheelChanged -= this.OnTargetPanel_PointerWheelChanged;
                _targetPanel.PointerPressed -= this.OnTargetPanel_PointerPressed;
                _targetPanel.PointerReleased -= this.OnTargetPanel_PointerReleased;
                _targetPanel.PointerMoved -= this.OnTargetPanel_PointerMoved;

                _targetPanel = null;

                // Deregister events from CoreWindow
                coreWindow.KeyDown -= this.OnCoreWindow_KeyDown;
                coreWindow.KeyUp -= this.OnCoreWindow_KeyUp;
            });

            // set all references to zero
            _dummyButtonForFocus = null;
            _painter = null;
            _coreWindow = null;
            _dispatcher = null;
        }

        /// <summary>
        /// Queries all current input states.
        /// </summary>
        public void GetInputStates(List<InputStateBase> target)
        {
            target.Add(_stateMouseOrPointer);
            target.Add(_stateKeyboard);
        }

        private void OnDummyButtonForFocus_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (_painter == null) { return; }

            // This enables bubbling of the keyboard event
            e.Handled = false;
        }

        private void OnDummyButtonForFocus_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (_painter == null) { return; }

            // This enables bubbling of the keyboard event
            e.Handled = false;
        }

        private void OnDummyButtonForFocus_LostFocus(object sender, RoutedEventArgs e)
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

        private void OnDummyButtonForFocus_GotFocus(object sender, RoutedEventArgs e)
        {
            if (_painter == null) { return; }

            _stateKeyboard.Internals.NotifyFocusGot();
            _hasFocus = true;
        }

        private void OnTargetPanel_PointerReleased(object sender, PointerRoutedEventArgs e)
        {
            if (_painter == null) { return; }
            if (_targetPanel == null) { return; }

            // Set focus on target
            _dummyButtonForFocus?.Focus(FocusState.Programmatic);

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
            _dummyButtonForFocus?.Focus(FocusState.Programmatic);

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