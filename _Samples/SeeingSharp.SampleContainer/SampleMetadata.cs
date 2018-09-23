using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer
{
    public class SampleMetadata
    {
        private SampleDescriptionAttribute m_description;
        private Type m_sampleType;

        public SampleMetadata()
        {

        }

        public SampleMetadata(SampleDescriptionAttribute description, Type sampleType)
        {
            m_description = description;
            m_sampleType = sampleType;

            this.Name = m_description.SampleName;
            this.OrderId = m_description.OrderID;
            this.Group = m_description.SampleGroupName;
            this.SourceCodeUrl = m_description.SourceCodeUrl;
        }

        public SampleBase CreateSampleObject()
        {
            if(m_sampleType == null)
            {
                throw new ApplicationException($"No sample type given!");
            }

            var result = Activator.CreateInstance(m_sampleType) as SampleBase;
            if(result == null) { throw new ApplicationException($"Sample type {m_sampleType.FullName} is not derived from {nameof(SampleBase)}!"); }

            return result;
        }

        public AssemblyResourceLink TryGetSampleImageLink()
        {
            if (m_description == null) { return null; }
            if (string.IsNullOrWhiteSpace((m_description.SampleImageFileName))) { return null; }

            return new AssemblyResourceLink(
                m_sampleType,
                m_description.SampleImageFileName);
        }

        public string Name
        {
            get;
            set;
        } = string.Empty;

        public int OrderId
        {
            get;
            set;
        } = 0;

        public string Group
        {
            get;
            set;
        } = string.Empty;

        public string SourceCodeUrl
        {
            get;
            set;
        } = string.Empty;
    }
}
