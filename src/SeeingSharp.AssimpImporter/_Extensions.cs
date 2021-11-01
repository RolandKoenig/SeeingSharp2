using SeeingSharp.Multimedia.Core;

namespace SeeingSharp
{
    public static class SeeingSharpWpfExtensionMethods
    {
        public static SeeingSharpLoader RegisterAssimpImporter(this SeeingSharpLoader loader)
        {
            loader.RegisterModelImporter(new AssimpImporter.AssimpImporter());
            return loader;
        }
    }
}