using System;
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using SeeingSharp.Util;

namespace SeeingSharp.WpfSamples
{
    public static class DesignData
    {
        public static MainWindowViewModel MainWindowVM
        {
            get
            {
                var sampleRepo = new SampleRepository();

                for (var loop = 1; loop < 5; loop++)
                {
                    var actGroupName = $"DummyGroup {loop}";
                    var actGroup = new SampleGroupMetadata(actGroupName);
                    actGroup.Samples.Add(new SampleMetadata(
                        new SampleDescriptionAttribute("DummySample1", 1, actGroupName),
                        typeof(DummySampleClass)));
                    actGroup.Samples.Add(new SampleMetadata(
                        new SampleDescriptionAttribute("DummySample2", 2, actGroupName),
                        typeof(DummySampleClass)));
                    actGroup.Samples.Add(new SampleMetadata(
                        new SampleDescriptionAttribute("DummySample3", 3, actGroupName),
                        typeof(DummySampleClass)));
                    sampleRepo.SampleGroups.Add(actGroup);
                }

                var result = new MainWindowViewModel(sampleRepo, null);
                return result;
            }
        }

        public static PerformanceOverviewViewModel PerformanceOverviewVM
        {
            get
            {
                var perfAnalyzer = new PerformanceAnalyzer(TimeSpan.FromSeconds(1.0));
                //var random = new Random(Environment.TickCount);
                //for (var loop = 0; loop < 5; loop++)
                //{
                //    var calculator = new DurationPerformanceCalculator($"Test-Calculator ({loop + 1})");

                //    //perfAnalyzer.UIDurationKpisCurrents.Add(new DurationPerformanceResult(
                //    //    calculator, DateTime.UtcNow,
                //    //    TimeSpan.FromMilliseconds(random.NextDouble() * 30).Ticks,
                //    //    TimeSpan.FromMilliseconds(31.0).Ticks,
                //    //    TimeSpan.FromMilliseconds(1.0).Ticks));
                //}

                return new PerformanceOverviewViewModel(perfAnalyzer);
            }
        }

        private class DummySampleClass : SampleBase
        {
            public override Task OnStartupAsync(RenderLoop mainRenderLoop, SampleSettings sampleSettings)
            {
                return Task.FromResult<object>(null);
            }

            public override Task OnInitRenderingWindowAsync(RenderLoop mainOrChildRenderLoop)
            {
                return Task.FromResult<object>(null);
            }
        }
    }
}