using SeeingSharp.SampleContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SeeingSharp.WpfSamples
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

        public SampleMetadata SampleMetadata => m_sample;

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

                    m_bitmapSourceTask = Task.Run(() =>
                    {
                        BitmapImage source = new BitmapImage();
                        source.BeginInit();
                        source.StreamSource = sourceLink.OpenRead();
                        source.EndInit();
                        source.Freeze();

                        m_bitmapSource = source;
                    }).ContinueWith(
                        (task) => RaisePropertyChanged(nameof(BitmapSource)),
                        TaskScheduler.FromCurrentSynchronizationContext());
                }
                return m_bitmapSource;
            }
        }
    }
}
