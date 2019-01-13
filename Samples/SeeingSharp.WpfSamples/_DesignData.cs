﻿#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
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