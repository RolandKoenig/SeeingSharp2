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

        public SampleMetadata(SampleDescriptionAttribute description, Type sampleType)
        {
            m_description = description;
            m_sampleType = sampleType;
        }

        public SampleBase CreateSampleObject()
        {
            var result = Activator.CreateInstance(m_sampleType) as SampleBase;
            if(result == null) { throw new ApplicationException($"Sample type {m_sampleType.FullName} is not derived from {nameof(SampleBase)}!"); }

            return result;
        }

        public AssemblyResourceLink TryGetSampleImageLink()
        {
            if (string.IsNullOrWhiteSpace((m_description.SampleImageFileName))) { return null; }

            return new AssemblyResourceLink(
                m_sampleType,
                m_description.SampleImageFileName);
        }

        public string Name => m_description.SampleName;

        public int OrderId => m_description.OrderID;

        public string Group => m_description.SampleGroupName;

        public string SourceCodeUrl => m_description.SourceCodeUrl;
    }
}
