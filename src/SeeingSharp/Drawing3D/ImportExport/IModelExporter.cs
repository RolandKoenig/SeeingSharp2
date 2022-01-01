using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.ImportExport
{
    public interface IModelExporter
    {
        /// <summary>
        /// Exports the model(s) defined in the given model container to the given model file.
        /// </summary>
        /// <param name="modelContainer">The model(s) to export.</param>
        /// <param name="targetFile">The path to the target file.</param>
        /// <param name="exportOptions">Some configuration for the exporter.</param>
        void ExportModelAsync(ExportModelContainer modelContainer, ResourceLink targetFile, ExportOptions exportOptions);
    }
}
