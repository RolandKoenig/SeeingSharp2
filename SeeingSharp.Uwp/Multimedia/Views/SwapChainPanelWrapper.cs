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
using Windows.Foundation;
using Windows.Graphics.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using SeeingSharp.Util;
using SharpDX;
using SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Views
{
    internal class SwapChainPanelWrapper : IDisposable
    {
        // UI objects
        private SwapChainBackgroundPanel m_bgPanel;
        private ISwapChainBackgroundPanelNative m_bgPanelNative;
        private SwapChainPanel m_panel;
        private ISwapChainPanelNative m_panelNative;

        // Configuration
        private float m_currentDpiX;
        private float m_currentDpiY;
        public event EventHandler CompositionScaleChanged;
        public event EventHandler<RoutedEventArgs> Loaded;
        public event EventHandler<SizeChangedEventArgs> SizeChanged;

        // Forwarded events
        public event EventHandler<RoutedEventArgs> Unloaded;

        public SwapChainPanelWrapper()
        {
            var displayInfo = DisplayInformation.GetForCurrentView();
            m_currentDpiX = displayInfo.LogicalDpi;
            m_currentDpiY = displayInfo.LogicalDpi;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapChainPanelWrapper"/> class.
        /// </summary>
        /// <param name="bgPanel">The panel for which to create the wrapper.</param>
        public SwapChainPanelWrapper(SwapChainBackgroundPanel bgPanel)
            : this()
        {
            m_bgPanel = bgPanel;
            m_bgPanelNative = ComObject.As<ISwapChainBackgroundPanelNative>(m_bgPanel);

            m_bgPanel.SizeChanged += OnAnyPanel_SizeChanged;
            m_bgPanel.Loaded += OnAnyPanel_Loaded;
            m_bgPanel.Unloaded += OnAnyPanel_Unloaded;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SwapChainPanelWrapper"/> class.
        /// </summary>
        /// <param name="panel">The panel for which to create the wrapper.</param>
        public SwapChainPanelWrapper(SwapChainPanel panel)
            : this()
        {
            m_panel = panel;
            m_panelNative = ComObject.As<ISwapChainPanelNative>(m_panel);

            m_panel.SizeChanged += OnAnyPanel_SizeChanged;
            m_panel.Loaded += OnAnyPanel_Loaded;
            m_panel.Unloaded += OnAnyPanel_Unloaded;
            m_panel.CompositionScaleChanged += OnPanelCompositionScaleChanged;
        }

        /// <summary>
        /// Führt anwendungsspezifische Aufgaben durch, die mit der Freigabe, der Zurückgabe oder dem Zurücksetzen von nicht verwalteten Ressourcen zusammenhängen.
        /// </summary>
        public void Dispose()
        {
            SeeingSharpUtil.SafeDispose(ref m_bgPanelNative);
            SeeingSharpUtil.SafeDispose(ref m_panelNative);
        }

        private void OnAnyPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            Unloaded?.Invoke(sender, e);
        }

        private void OnAnyPanel_Loaded(object sender, RoutedEventArgs e)
        {
            Loaded?.Invoke(sender, e);
        }

        private void OnAnyPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            SizeChanged?.Invoke(sender, e);
        }

        private void OnPanelCompositionScaleChanged(SwapChainPanel sender, object args)
        {
            CompositionScaleChanged?.Invoke(this, EventArgs.Empty);
        }

        public Size RenderSize => Panel.RenderSize;

        public Size ActualSize => new Size(Panel.ActualWidth, Panel.ActualHeight);

        public double ActualWidth => Panel.ActualWidth;

        public double ActualHeight => Panel.ActualHeight;

        public Panel Panel
        {
            get
            {
                if (m_bgPanel != null) { return m_bgPanel; }
                if (m_panel != null) { return m_panel; }
                throw new ObjectDisposedException("SwapChainPanelWrapper");
            }
        }

        /// <summary>
        /// Sets the SwapChain object of the panel wrapped by this object.
        /// </summary>
        public SwapChain SwapChain
        {
            set
            {
                if (m_bgPanelNative != null) { m_bgPanelNative.SwapChain = value; }
                else if (m_panelNative != null) { m_panelNative.SwapChain = value; }
                else
                {
                    throw new ObjectDisposedException("SwapChainPanelWrapper");
                }
            }
        }

        public bool CompositionRescalingNeeded => m_panel != null;

        public double CompositionScaleX
        {
            get
            {
                if (m_panel != null) { return m_panel.CompositionScaleX; }
                return m_currentDpiX / 96.0;
            }
        }

        public double CompositionScaleY
        {
            get
            {
                if (m_panel != null) { return m_panel.CompositionScaleY; }
                return m_currentDpiY / 96.0;
            }
        }

        public CoreDispatcher Dispatcher
        {
            get
            {
                if(m_bgPanel != null) { return m_bgPanel.Dispatcher; }
                if(m_panel != null) { return m_panel.Dispatcher; }
                throw new ObjectDisposedException("SwapChainPanelWrapper");
            }
        }
    }
}
