using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D
{
    public interface IModelImporter
    {
        /// <summary>
        /// Imports a model from the given file.
        /// </summary>
        /// <param name="importOptions">Some configuration for the importer.</param>
        /// <param name="sourceFile">The source file to be loaded.</param>
        ImportedModelContainer ImportModel(ResourceLink sourceFile, ImportOptions importOptions);

        /// <summary>
        /// Creates a default import options object for this importer.
        /// </summary>
        ImportOptions CreateDefaultImportOptions();
    }
}