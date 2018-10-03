using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.SampleContainer
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SampleDescriptionAttribute : Attribute
    {
        public SampleDescriptionAttribute(string sampleName, int orderID, string sampleGroupName, string sampleImageFileName = "", string sourceCodeUrl = "", Type settingsType = null)
        {
            this.SampleName = sampleName;
            this.OrderID = orderID;
            this.SampleGroupName = sampleGroupName;
            this.SampleImageFileName = sampleImageFileName;
            this.SourceCodeUrl = sourceCodeUrl;
            this.SettingsType = settingsType;
        }

        public string SampleName
        {
            get;
            private set;
        }

        public int OrderID
        {
            get;
            private set;
        }

        public string SampleGroupName
        {
            get;
            private set;
        }

        public string SampleImageFileName
        {
            get;
            private set;
        }

        public string SourceCodeUrl
        {
            get;
            private set;
        }

        public Type SettingsType
        {
            get;
            private set;
        }
    }
}
