using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Multimedia.Views;
using SeeingSharp.SampleContainer;
using SharpDX;

// Die Elementvorlage "Leere Seite" wird unter https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x407 dokumentiert.

namespace SeeingSharp.UwpSamples
{
    /// <summary>
    /// Eine leere Seite, die eigenständig verwendet oder zu der innerhalb eines Rahmens navigiert werden kann.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SeeingSharpPanelPainter m_panelPainter;

        public MainPage()
        {
            this.InitializeComponent();

            this.Loaded += OnLoaded;
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            m_panelPainter = new SeeingSharpPanelPainter(CtrlSwapChain);

            SampleRepository sampleRepo = new SampleRepository();
            await sampleRepo.SampleGroups
                .First()
                .Samples
                .First()
                .CreateSampleObject().OnStartupAsync(m_panelPainter.RenderLoop);
        }
    }
}
