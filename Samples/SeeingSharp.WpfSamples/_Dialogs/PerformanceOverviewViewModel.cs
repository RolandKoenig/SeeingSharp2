using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.Util;

namespace SeeingSharp.WpfSamples
{
    public class PerformanceOverviewViewModel : PropertyChangedBase
    {
        private PerformanceAnalyzer m_performanceAnalyzer;

        public PerformanceOverviewViewModel(PerformanceAnalyzer performanceAnalyzer)
        {
            m_performanceAnalyzer = performanceAnalyzer;
        }

        public ObservableCollection<DurationPerformanceResult> DurationKpis =>
            m_performanceAnalyzer.UIDurationKpisCurrents;

    }
}
