
/* Unmerged change from project 'SeeingSharp.WpfCoreSamples'
Before:
using System;
After:
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.Util;
using System;
*/
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.Util;
using System.Collections.
/* Unmerged change from project 'SeeingSharp.WpfCoreSamples'
Before:
using System.Threading.Tasks;
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.Util;
After:
using System.Threading.Tasks;
*/
ObjectModel;

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
