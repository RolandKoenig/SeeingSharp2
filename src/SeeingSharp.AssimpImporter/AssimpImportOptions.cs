using SeeingSharp.Drawing3D.ImportExport;

namespace SeeingSharp.AssimpImporter
{
    public class AssimpImportOptions : ImportOptions
    {
        public ConfigureTextureDelegate? ConfigureTextureAction { get; set; }
    }
}
