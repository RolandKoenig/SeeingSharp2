using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
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
