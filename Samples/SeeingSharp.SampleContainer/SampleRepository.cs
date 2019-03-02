#region License information
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
#endregion

using System.Collections.Generic;
using System.Reflection;

namespace SeeingSharp.SampleContainer
{
    public class SampleRepository
    {
        public void LoadSampleData()
        {
            // Search for all samples
            var sampleGroups = new Dictionary<string, SampleGroupMetadata>();

            foreach (var actType in Assembly.GetExecutingAssembly().GetTypes())
            {
                var sampleDesc = actType.GetCustomAttribute<SampleDescriptionAttribute>();
                if (sampleDesc == null)
                {
                    continue;
                }

                if (!sampleGroups.TryGetValue(sampleDesc.SampleGroupName, out var sampleGroup))
                {
                    sampleGroup = new SampleGroupMetadata(sampleDesc.SampleGroupName);
                    sampleGroups.Add(sampleGroup.GroupName, sampleGroup);
                    SampleGroups.Add(sampleGroup);
                }

                sampleGroup.Samples.Add(new SampleMetadata(sampleDesc, actType));
            }

            // Sort sample groups
            var groupOrder = new Dictionary<string, int>();
            groupOrder[nameof(Basics3D)] = 1;
            groupOrder[nameof(Primitives3D)] = 2;
            groupOrder[nameof(Basics2D)] = 3;
            groupOrder[nameof(Postprocessing)] = 4;

            int TryGetGroupOrderId(string groupName)
            {
                groupOrder.TryGetValue(groupName, out var orderID);
                return orderID;
            }
            SampleGroups.Sort((left, right) => TryGetGroupOrderId(left.GroupName).CompareTo(TryGetGroupOrderId(right.GroupName)));

            // Sort samples
            foreach (var actSampleGroup in SampleGroups)
            {
                actSampleGroup.Samples.Sort((left, right) => left.OrderId.CompareTo(right.OrderId));
            }
        }

        public SampleRepository()
        {
            SampleGroups = new List<SampleGroupMetadata>();
        }

        public List<SampleGroupMetadata> SampleGroups
        {
            get;
            private set;
        }
    }
}