﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.WpfSamples
{
    #region using

    using System.Windows;
    using System.Windows.Controls;
    using SampleContainer;

    #endregion

    /// <summary>
    /// Interaktionslogik für MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool m_isChangingSample;
        private SampleBase m_actSample;
        private SampleMetadata m_actSampleInfo;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
            {
                return;
            }

            var sampleRepo = new SampleRepository();
            sampleRepo.LoadSampleData();
            DataContext = new MainWindowViewModel(sampleRepo, CtrlRenderer.RenderLoop);
        }

        private void OnSelectedSampleChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!(DataContext is MainWindowViewModel viewModel))
            {
                return;
            }

            var selectedSample = viewModel.SelectedSample;

            if (selectedSample == null)
            {
                return;
            }

            ApplySample(selectedSample.SampleMetadata, viewModel.SampleSettings);
        }

        /// <summary>
        /// Applies the given sample.
        /// </summary>
        private async void ApplySample(SampleMetadata sampleInfo, SampleSettings sampleSettings)
        {
            if (m_isChangingSample)
            {
                return;
            }

            m_isChangingSample = true;

            try
            {
                if (m_actSampleInfo == sampleInfo) { return; }

                // Clear previous sample
                if (m_actSampleInfo != null)
                {
                    await CtrlRenderer.RenderLoop.Scene.ManipulateSceneAsync((manipulator) =>
                    {
                        manipulator.Clear(true);
                    });

                    await CtrlRenderer.RenderLoop.Clear2DDrawingLayersAsync();
                    m_actSample.NotifyClosed();
                }

                // Reset members
                m_actSample = null;
                m_actSampleInfo = null;

                // Apply new sample
                if (sampleInfo != null)
                {
                    var sampleObject = sampleInfo.CreateSampleObject();
                    await sampleObject.OnStartupAsync(CtrlRenderer.RenderLoop, sampleSettings);

                    m_actSample = sampleObject;
                    m_actSampleInfo = sampleInfo;
                }

                // Wait for next finished rendering
                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();

                await CtrlRenderer.RenderLoop.WaitForNextFinishedRenderAsync();
            }
            finally
            {
                m_isChangingSample = false;
            }
        }
    }
}