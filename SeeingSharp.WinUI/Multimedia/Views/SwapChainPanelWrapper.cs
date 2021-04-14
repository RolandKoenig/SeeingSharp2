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
using System.Runtime.InteropServices;
using Windows.Foundation;
using Windows.UI.Core;
using Microsoft.System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;
using WinRT;

namespace SeeingSharp.Multimedia.Views
{
    internal class SwapChainPanelWrapper : IDisposable
    {
        // UI objects (SwapChainBackgroundPanel)
        private SwapChainBackgroundPanel _bgPanel;
        private ISwapChainBackgroundPanelNative _bgPanelNative;
        private WinUIDesktopInterop.ISwapChainBackgroundPanelNative _bgPanelNativeDesktop;

        // UI objects (SwapChainPanel)
        private SwapChainPanel _panel;
        private ISwapChainPanelNative _panelNative;
        private WinUIDesktopInterop.ISwapChainPanelNative _panelNativeDesktop;

        // Configuration
        private float _currentDpiX;
        private float _currentDpiY;

        public Size RenderSize => this.Panel.RenderSize;

        public Size ActualSize => new Size(this.Panel.ActualWidth, this.Panel.ActualHeight);

        public double ActualWidth => this.Panel.ActualWidth;

        public double ActualHeight => this.Panel.ActualHeight;

        public bool IsPanelLoaded => this.Panel.IsLoaded;

        public Panel Panel
        {
            get
            {
                if (_bgPanel != null) { return _bgPanel; }
                if (_panel != null) { return _panel; }
                throw new ObjectDisposedException(nameof(SwapChainPanelWrapper));
            }
        }

        /// <summary>
        /// Sets the SwapChain object of the panel wrapped by this object.
        /// </summary>
        public SwapChain SwapChain
        {
            set
            {
                if (_panelNative != null) { _panelNative.SwapChain = value; }
                else if (_panelNativeDesktop != null) { _panelNativeDesktop.SetSwapChain(value?.NativePointer ?? IntPtr.Zero); }
                else if (_bgPanelNative != null) { _bgPanelNative.SwapChain = value; }
                else if (_bgPanelNativeDesktop != null) { _bgPanelNativeDesktop.SetSwapChain(value?.NativePointer ?? IntPtr.Zero); }
                else
                {
                    throw new ObjectDisposedException(nameof(SwapChainPanelWrapper));
                }
            }
        }

        public bool CompositionRescalingNeeded => _panel != null;

        public double CompositionScaleX
        {
            get
            {
                if (_panel != null) { return _panel.CompositionScaleX; }
                return _currentDpiX / 96.0;
            }
        }

        public double CompositionScaleY
        {
            get
            {
                if (_panel != null) { return _panel.CompositionScaleY; }
                return _currentDpiY / 96.0;
            }
        }

        public DispatcherQueue DispatcherQueue
        {
            get
            {
                if (_bgPanel != null) { return _bgPanel.DispatcherQueue; }
                if (_panel != null) { return _panel.DispatcherQueue; }
                throw new ObjectDisposedException(nameof(SwapChainPanelWrapper));
            }
        }

        public Visibility Visibility
        {
            get
            {
                if (_bgPanel != null) { return _bgPanel.Visibility; }
                if (_panel != null) { return _panel.Visibility; }
                throw new ObjectDisposedException(nameof(SwapChainPanelWrapper));
            }
        }
        public event EventHandler CompositionScaleChanged;
        public event EventHandler<RoutedEventArgs> Loaded;
        public event EventHandler<SizeChangedEventArgs> SizeChanged;

        // Forwarded events
        public event EventHandler<RoutedEventArgs> Unloaded;

        public SwapChainPanelWrapper()
        {
            //var displayInfo = DisplayInformation.GetForCurrentView();
            //_currentDpiX = displayInfo.LogicalDpi;
            //_currentDpiY = displayInfo.LogicalDpi;
            _currentDpiX = 96f;
            _currentDpiY = 96f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapChainPanelWrapper"/> class.
        /// </summary>
        /// <param name="bgPanel">The panel for which to create the wrapper.</param>
        public SwapChainPanelWrapper(SwapChainBackgroundPanel bgPanel)
            : this()
        {
            _bgPanel = bgPanel;
            try
            {
                _bgPanelNative = ComObject.As<ISwapChainBackgroundPanelNative>(bgPanel);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode != Result.NoInterface) { throw; }
                _bgPanelNativeDesktop = bgPanel.As<WinUIDesktopInterop.ISwapChainBackgroundPanelNative>();
            }

            _bgPanel.SizeChanged += this.OnAnyPanel_SizeChanged;
            _bgPanel.Loaded += this.OnAnyPanel_Loaded;
            _bgPanel.Unloaded += this.OnAnyPanel_Unloaded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapChainPanelWrapper"/> class.
        /// </summary>
        /// <param name="panel">The panel for which to create the wrapper.</param>
        public SwapChainPanelWrapper(SwapChainPanel panel)
            : this()
        {
            _panel = panel;
            try
            {
                _panelNative = ComObject.As<ISwapChainPanelNative>(panel);
            }
            catch (SharpDXException ex)
            {
                if (ex.ResultCode != Result.NoInterface) { throw; }
                _panelNativeDesktop = panel.As<WinUIDesktopInterop.ISwapChainPanelNative>();
            }

            _panel.SizeChanged += this.OnAnyPanel_SizeChanged;
            _panel.Loaded += this.OnAnyPanel_Loaded;
            _panel.Unloaded += this.OnAnyPanel_Unloaded;
            _panel.CompositionScaleChanged += this.OnPanelCompositionScaleChanged;
        }

        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref _panelNative);
            _panelNativeDesktop = null;

            SeeingSharpUtil.SafeDispose(ref _bgPanelNative);
            _bgPanelNative = null;
        }

        private void OnAnyPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            this.Unloaded?.Invoke(sender, e);
        }

        private void OnAnyPanel_Loaded(object sender, RoutedEventArgs e)
        {
            this.Loaded?.Invoke(sender, e);
        }

        private void OnAnyPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.SizeChanged?.Invoke(sender, e);
        }

        private void OnPanelCompositionScaleChanged(SwapChainPanel sender, object args)
        {
            this.CompositionScaleChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
