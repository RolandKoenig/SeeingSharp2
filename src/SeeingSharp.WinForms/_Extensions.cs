using SeeingSharp.Multimedia.Core;

namespace SeeingSharp
{
    public static class SeeingSharpWinFormsExtensionMethods
    {
        public static SeeingSharpLoader SupportWinForms(this SeeingSharpLoader loader)
        {
            loader.RegisterExtension(new SeeingSharpWinFormsExtensions());
            return loader;
        }
    }
}