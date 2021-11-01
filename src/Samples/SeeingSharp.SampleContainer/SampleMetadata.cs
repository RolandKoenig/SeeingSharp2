using System;
using SeeingSharp.Util;

namespace SeeingSharp.SampleContainer
{
    public class SampleMetadata
    {
        private SampleDescriptionAttribute _description;
        private Type _sampleType;

        public string Name
        {
            get;
            set;
        }

        public int OrderId
        {
            get;
            set;
        }

        public string Group
        {
            get;
            set;
        }

        public string SourceCodeUrl
        {
            get;
            set;
        }

        public SampleMetadata(SampleDescriptionAttribute description, Type sampleType)
        {
            _description = description;
            _sampleType = sampleType;

            this.Name = _description.SampleName;
            this.OrderId = _description.OrderId;
            this.Group = _description.SampleGroupName;
            this.SourceCodeUrl = _description.SourceCodeUrl;
        }

        public SampleBase CreateSampleObject()
        {
            if (_sampleType == null)
            {
                throw new ApplicationException("No sample type given!");
            }

            var result = Activator.CreateInstance(_sampleType) as SampleBase;
            if (result == null) { throw new ApplicationException($"Sample type {_sampleType.FullName} is not derived from {nameof(SampleBase)}!"); }

            return result;
        }

        public SampleSettings CreateSampleSettingsObject()
        {
            var settingsType = _description.SettingsType;

            if (settingsType == null)
            {
                return new SampleSettings();
            }

            var result = Activator.CreateInstance(settingsType) as SampleSettings;

            if (result == null)
            {
                throw new ApplicationException($"SampleSettings type {_sampleType.FullName} is not derived from {nameof(SampleSettings)}!");
            }

            return result;
        }

        public AssemblyResourceLink TryGetSampleImageLink()
        {
            if (_description == null) { return null; }
            if (string.IsNullOrWhiteSpace(_description.SampleImageFileName)) { return null; }

            return new AssemblyResourceLink(
                _sampleType,
                _description.SampleImageFileName);
        }
    }
}