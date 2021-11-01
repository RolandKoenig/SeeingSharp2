using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SeeingSharp.SampleContainer;
using SeeingSharp.SampleContainer.Util;

namespace SeeingSharp.WpfSamples
{
    public class SampleViewModel : PropertyChangedBase
    {
        private BitmapSource _bitmapSource;
        private Task _bitmapSourceTask;

        public SampleMetadata SampleMetadata { get; }

        public string Name => this.SampleMetadata.Name;

        public string Group => this.SampleMetadata.Group;

        public BitmapSource BitmapSource
        {
            get
            {
                if (_bitmapSource == null && _bitmapSourceTask == null)
                {
                    var sourceLink = this.SampleMetadata.TryGetSampleImageLink();

                    if (sourceLink == null)
                    {
                        return null;
                    }

                    _bitmapSourceTask = Task.Run(() =>
                    {
                        var source = new BitmapImage();
                        source.BeginInit();
                        source.StreamSource = sourceLink.OpenRead();
                        source.EndInit();
                        source.Freeze();

                        _bitmapSource = source;
                    }).ContinueWith(
                        task => this.RaisePropertyChanged(nameof(this.BitmapSource)),
                        TaskScheduler.FromCurrentSynchronizationContext());
                }

                return _bitmapSource;
            }
        }

        public SampleViewModel(SampleMetadata sample)
        {
            this.SampleMetadata = sample;
        }
    }
}