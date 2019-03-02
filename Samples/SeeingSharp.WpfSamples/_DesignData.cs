/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
using System.Threading.Tasks;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.SampleContainer;

namespace SeeingSharp.WpfSamples
{
    public static class DesignData
    {
        public static MainWindowViewModel MainWindowVM
        {
            get
            {
                var sampleRepo = new SampleRepository();

                for(var loop =1; loop<5; loop++)
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

        private class DummySampleClass : SampleBase
        {
            public override Task OnStartupAsync(RenderLoop targetRenderLoop, SampleSettings sampleSettings)
            {
                return Task.FromResult<object>(null);
            }
        }
    }
}