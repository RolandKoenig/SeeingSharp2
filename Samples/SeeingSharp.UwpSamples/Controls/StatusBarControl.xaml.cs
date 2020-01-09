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
using SeeingSharp.Multimedia.Views;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace SeeingSharp.UwpSamples.Controls
{
    public sealed partial class StatusBarControl : UserControl
    {
        public static readonly DependencyProperty CtrlRendererProperty =
            DependencyProperty.Register(nameof(CtrlRenderer), typeof(SeeingSharpRenderPanel), typeof(StatusBarControl), new PropertyMetadata(null));

        public StatusBarControl()
        {
            this.InitializeComponent();
        }

        public SeeingSharpRenderPanel CtrlRenderer
        {
            get => (SeeingSharpRenderPanel)this.GetValue(CtrlRendererProperty);
            set => this.SetValue(CtrlRendererProperty, value);
        }
    }
}
