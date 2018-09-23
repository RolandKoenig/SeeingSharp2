using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.WpfSamples
{
    public static class DesignData
    {
        public static MainWindowViewModel MainWindowVM
        {
            get
            {
                SampleRepository sampleRepo = new SampleRepository();
                for(int loop=1; loop<5; loop++)
                {
                    string actGroupName = $"DummyGroup {loop}";
                    SampleGroupMetadata actGroup = new SampleGroupMetadata(actGroupName);
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

                return new MainWindowViewModel(sampleRepo);
            }
        }

        private class DummySampleClass : SampleBase
        {
            public override Task OnStartupAsync(RenderLoop targetRenderLoop)
            {
                return Task.FromResult<object>(null);
            }
        }
    }
}
