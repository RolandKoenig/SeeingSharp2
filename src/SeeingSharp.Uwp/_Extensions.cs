using SeeingSharp.Multimedia.Core;

namespace SeeingSharp
{
    public static class SeeingSharpUwpExtensionMethods
    {
        public static SeeingSharpLoader SupportUwp(this SeeingSharpLoader loader)
        {
            loader.RegisterExtension(new SeeingSharpUwpExtensions());
            return loader;
        }
    }
}