using System.Collections.Generic;
using SeeingSharp.SampleContainer.Util;
using SeeingSharp.Util;

namespace SeeingSharp.WpfSamples
{
    public class PerformanceOverviewViewModel : PropertyChangedBase
    {
        private PerformanceAnalyzer _performanceAnalyzer;

        public List<DurationPerformanceResult> DurationResults { get; }

        public PerformanceOverviewViewModel(PerformanceAnalyzer performanceAnalyzer)
        {
            _performanceAnalyzer = performanceAnalyzer;
            this.DurationResults = new List<DurationPerformanceResult>();
        }

        public void TriggerRefresh()
        {
            this.DurationResults.Clear();
            foreach (var actResult in _performanceAnalyzer.GetCurrentResults())
            {
                this.DurationResults.Add(actResult);
            }
            this.DurationResults.Sort((left, right) => -left.SumAverageMsDouble.CompareTo(right.SumAverageMsDouble));
        }
    }
}
