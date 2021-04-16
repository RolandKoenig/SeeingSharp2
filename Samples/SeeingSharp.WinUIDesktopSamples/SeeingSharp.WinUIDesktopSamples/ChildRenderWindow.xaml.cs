using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using SeeingSharp.SampleContainer;

namespace SeeingSharp.WinUIDesktopSamples
{
    public sealed partial class ChildRenderWindow : Window
    {
        public bool DiscardPresent
        {
            get;
            set;
        }

        public ChildRenderWindow()
        {
            this.InitializeComponent();
        }

        public Task ClearAsync()
        {
            return Task.Delay(10);
        }

        public Task SetRenderingDataAsync(SampleBase sample)
        {
            return Task.Delay(10);
        }

        public void TriggerRenderControlFadeInAnimation()
        {

        }
    }
}
