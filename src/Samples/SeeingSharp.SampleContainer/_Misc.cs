using System;

namespace SeeingSharp.SampleContainer
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SampleDescriptionAttribute : Attribute
    {
        public string SampleName
        {
            get;
        }

        public int OrderId
        {
            get;
        }

        public string SampleGroupName
        {
            get;
        }

        public string SampleImageFileName
        {
            get;
        }

        public string SourceCodeUrl
        {
            get;
        }

        public Type? SettingsType
        {
            get;
        }

        public SampleDescriptionAttribute(string sampleName, int orderId, string sampleGroupName, string sampleImageFileName = "", string sourceCodeUrl = "", Type? settingsType = null)
        {
            this.SampleName = sampleName;
            this.OrderId = orderId;
            this.SampleGroupName = sampleGroupName;
            this.SampleImageFileName = sampleImageFileName;
            this.SourceCodeUrl = sourceCodeUrl;
            this.SettingsType = settingsType;
        }
    }
}