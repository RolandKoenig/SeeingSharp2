using SeeingSharp.Core;

namespace SeeingSharp
{
    public static class SeeingSharpUwpExtensionMethods
    {
        public static SeeingSharpLoader SupportWinUI(this SeeingSharpLoader loader)
        {
            loader.RegisterExtension(new SeeingSharpWinUIExtensions());
            return loader;
        }
    }
}