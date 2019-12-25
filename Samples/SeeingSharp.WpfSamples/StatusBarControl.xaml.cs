using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using SeeingSharp.Multimedia.Views;

namespace SeeingSharp.WpfSamples
{
    /// <summary>
    /// Interaction logic for StatusBarControl.xaml
    /// </summary>
    public partial class StatusBarControl : UserControl
    {
        public static readonly DependencyProperty CtrlRendererProperty =
            DependencyProperty.Register(nameof(CtrlRenderer), typeof(SeeingSharpRendererElement), typeof(StatusBarControl), new PropertyMetadata(null));

        public StatusBarControl()
        {
            InitializeComponent();
        }

        public SeeingSharpRendererElement CtrlRenderer
        {
            get => (SeeingSharpRendererElement)this.GetValue(CtrlRendererProperty);
            set => this.SetValue(CtrlRendererProperty, value);
        }
    }
}
