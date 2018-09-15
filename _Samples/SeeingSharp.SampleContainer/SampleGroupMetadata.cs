using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace SeeingSharp.SampleContainer
{
    public class SampleGroupMetadata
    {
        public SampleGroupMetadata(string groupName)
        {
            this.Samples = new List<SampleMetadata>();

            this.GroupName = groupName;
        }

        public string GroupName
        {
            get;
            private set;
        }

        public List<SampleMetadata> Samples
        {
            get;
            private set;
        }
    }
}
