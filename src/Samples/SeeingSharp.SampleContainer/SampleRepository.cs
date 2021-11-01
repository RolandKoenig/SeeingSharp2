using System.Collections.Generic;
using System.Reflection;

namespace SeeingSharp.SampleContainer
{
    public class SampleRepository
    {
        public List<SampleGroupMetadata> SampleGroups
        {
            get;
        }

        public SampleRepository()
        {
            this.SampleGroups = new List<SampleGroupMetadata>();
        }

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
                    this.SampleGroups.Add(sampleGroup);
                }

                sampleGroup.Samples.Add(new SampleMetadata(sampleDesc, actType));
            }

            // Sort sample groups
            var groupOrder = new Dictionary<string, int>();
            groupOrder[nameof(Basics3D)] = 1;
            groupOrder[nameof(Primitives3D)] = 2;
            groupOrder[nameof(MassScenes)] = 3;
            groupOrder[nameof(Basics2D)] = 4;
            groupOrder[nameof(Postprocessing)] = 5;

            int TryGetGroupOrderId(string groupName)
            {
                groupOrder.TryGetValue(groupName, out var orderId);
                return orderId;
            }

            this.SampleGroups.Sort((left, right) => TryGetGroupOrderId(left.GroupName).CompareTo(TryGetGroupOrderId(right.GroupName)));

            // Sort samples
            foreach (var actSampleGroup in this.SampleGroups)
            {
                actSampleGroup.Samples.Sort((left, right) => left.OrderId.CompareTo(right.OrderId));
            }
        }
    }
}