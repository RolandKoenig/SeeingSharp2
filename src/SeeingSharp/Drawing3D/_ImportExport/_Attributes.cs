using System;

namespace SeeingSharp.Drawing3D
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class SupportedFileFormatAttribute : Attribute
    {
        public string ShortFormatName
        {
            get;
        }

        public string ShortDescription
        {
            get;
        }

        public SupportedFileFormatAttribute(string shortFormatName, string shortDescription)
        {
            this.ShortFormatName = shortFormatName;
            this.ShortDescription = shortDescription;
        }
    }
}
