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

            // Sort sample groups
            Dictionary<string, int> groupOrder = new Dictionary<string, int>();
            groupOrder[nameof(Basics3D)] = 1;
            groupOrder[nameof(Postprocessing)] = 2;
            Func<string, int> tryGetGroupOrderID = (groupName) =>
            {
                groupOrder.TryGetValue(groupName, out int orderID);
                return orderID;
            };
            this.SampleGroups.Sort((left, right) => tryGetGroupOrderID(left.GroupName).CompareTo(tryGetGroupOrderID(right.GroupName)));

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
