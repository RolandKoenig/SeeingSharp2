using SeeingSharp.Multimedia.Core;

namespace SeeingSharp
{
    public static class SeeingSharpWpfExtensionMethods
    {
        public static SeeingSharpLoader SupportWpf(this SeeingSharpLoader loader)
        {
            loader.RegisterExtension(new SeeingSharpWpfExtensions());
            return loader;
        }
    }
}