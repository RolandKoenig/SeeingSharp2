using SeeingSharp.SampleContainer;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Media.Imaging;
using Windows.Storage.Streams;
using Windows.Storage;
using SeeingSharp.Util;

namespace SeeingSharp.UwpSamples
{
    public class SampleViewModel : ViewModelBase
    {
        private SampleMetadata m_sample;
        private BitmapSource m_bitmapSource;
        private Task m_bitmapSourceTask;

        public SampleViewModel(SampleMetadata sample)
        {
            m_sample = sample;
        }
        
        private async Task LoadSampleImageAsync(AssemblyResourceLink sourceLink)
        {
            BitmapImage source = new BitmapImage();
            using (var randomAccessStream = sourceLink.OpenRead().AsRandomAccessStream())
            {
                await source.SetSourceAsync(randomAccessStream);
            }
            m_bitmapSource = source;

            RaisePropertyChanged(nameof(BitmapSource));
        }

        public SampleMetadata Sample => m_sample;

        public string Name => m_sample.Name;

        public string Group => m_sample.Group;

        public BitmapSource BitmapSource
        {
            get
            {
                if((m_bitmapSource == null) && (m_bitmapSourceTask == null))
                {
                    var sourceLink = m_sample.TryGetSampleImageLink();
                    if(sourceLink == null) { return null; }

                    m_bitmapSourceTask = LoadSampleImageAsync(sourceLink);
                }
                return m_bitmapSource;
            }
        }
    }
}
