using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Util;

namespace SeeingSharp.Drawing3D.ImportExport
{
    [SupportedFileFormat("ac", "AC3D native format")]
    public class ACImporter : IModelImporter
    {
        /// <summary>
        /// Imports a model from the given file.
        /// </summary>
        /// <param name="sourceFile">The source file to be loaded.</param>
        /// <param name="importOptions">Some configuration for the importer.</param>
        public ImportedModelContainer ImportModel(ResourceLink sourceFile, ImportOptions importOptions)
        {
            // Get import options
            if (importOptions is not ACImportOptions acImportOptions)
            {
                throw new SeeingSharpException("Invalid import options for ACImporter!");
            }

            // Create result container
            var modelContainer = new ImportedModelContainer(sourceFile, acImportOptions);

            // Read the AC file
            var fileInfo = ACFileLoader.LoadFile(sourceFile.OpenInputStream());
            var geometry = ACFileLoader.GenerateGeometry(fileInfo);

            // Generate GeometryResource
            var resGeometry = modelContainer.GetResourceKey("Geometry", "Main");
            modelContainer.AddResource(new ImportedResourceInfo(
                resGeometry,
                _ => new GeometryResource(geometry)));

            // Generate Material resources
            var materialKeys = new NamedOrGenericKey[fileInfo.Materials.Count];
            for (var loop = 0; loop < materialKeys.Length; loop++)
            {
                var actACMaterial = fileInfo.Materials[loop];
                materialKeys[loop] = modelContainer.GetResourceKey("Material", actACMaterial.Name);

                modelContainer.AddResource(new ImportedResourceInfo(
                    materialKeys[loop],
                    _ => new StandardMaterialResource
                    {
                        UseVertexColors = false,
                        MaterialDiffuseColor = actACMaterial.Diffuse
                    }));
            }

            // Create the mesh
            modelContainer.AddObject(new Mesh(resGeometry, materialKeys));

            modelContainer.FinishLoading(geometry.GenerateBoundingBox());

            return modelContainer;
        }

        /// <summary>
        /// Creates a default import options object for this importer.
        /// </summary>
        public ImportOptions CreateDefaultImportOptions()
        {
            return new ACImportOptions();
        }
    }
}
