using System;
using System.Collections.Generic;
using System.Reflection;

namespace SeeingSharp.SampleContainer
{
    public class SampleRepository
    {
        public SampleRepository()
        {
            SampleGroups = new List<SampleGroupMetadata>();
        }

        public void LoadSampleData()
        {
            // Search for all samples
            var sampleGroups = new Dictionary<string, SampleGroupMetadata>();
            foreach (Type actType in Assembly.GetExecutingAssembly().GetTypes())
            {
                var sampleDesc = actType.GetCustomAttribute<SampleDescriptionAttribute>();
                if (sampleDesc == null) { continue; }

                if (!sampleGroups.TryGetValue(sampleDesc.SampleGroupName, out var sampleGroup))
                {
                    sampleGroup = new SampleGroupMetadata(sampleDesc.SampleGroupName);
                    sampleGroups.Add(sampleGroup.GroupName, sampleGroup);
                    SampleGroups.Add(sampleGroup);
                }

                sampleGroup.Samples.Add(new SampleMetadata(sampleDesc, actType));
            }

            // Sort samples
            foreach (var actSampleGroup in SampleGroups)
            {
                actSampleGroup.Samples.Sort((left, right) => left.OrderId.CompareTo((right.OrderId)));
            }
        }

        public List<SampleGroupMetadata> SampleGroups
        {
            get;
            private set;
        }
    }
}
