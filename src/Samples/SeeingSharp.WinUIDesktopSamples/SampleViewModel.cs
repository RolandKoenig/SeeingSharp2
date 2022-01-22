using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Media.Imaging;
using SeeingSharp.SampleContainer;
using SeeingSharp.Util;

namespace SeeingSharp.WinUIDesktopSamples
{
    public class SampleViewModel : ViewModelBase
    {
        private BitmapSource? _bitmapSource;
        private Task? _bitmapSourceTask;

        public SampleMetadata SampleMetadata { get; }

        public string Name => this.SampleMetadata.Name;

        public string Group => this.SampleMetadata.Group;

        public BitmapSource? BitmapSource
        {
            get
            {
                if (_bitmapSource == null && _bitmapSourceTask == null)
                {
                    var sourceLink = this.SampleMetadata.TryGetSampleImageLink();
                    if (sourceLink == null) { return null; }

                    _bitmapSourceTask = this.LoadSampleImageAsync(sourceLink);
                }
                return _bitmapSource;
            }
        }

        public SampleViewModel(SampleMetadata sample)
        {
            this.SampleMetadata = sample;
        }

        private async Task LoadSampleImageAsync(AssemblyResourceLink sourceLink)
        {
            var source = new BitmapImage();

            using (var randomAccessStream = sourceLink.OpenRead().AsRandomAccessStream())
            {
                await source.SetSourceAsync(randomAccessStream);
            }

            _bitmapSource = source;

            this.RaisePropertyChanged(nameof(this.BitmapSource));
        }
    }
}