using System.Collections.Generic;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Drawing3D.ImportExport;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public partial class Scene
    {
        public async Task<ExportModelContainer> PrepareForExportAsync(
            IEnumerable<SceneObject> objectsToExport,
            ExportOptions exportOptions)
        {
            objectsToExport.EnsureNotNull(nameof(objectsToExport));
            objectsToExport.EnsureMoreThanZeroElements(nameof(objectsToExport));

            // Create result object container
            var result = new ExportModelContainer();

            // Fill export container beside rendering
            //  (there it is ensured that no one changes the scene)
            await this.PerformBeforeUpdateAsync(() =>
            {
                // First step: Register all objects which we want to export
                foreach (var actObject in objectsToExport)
                {
                    actObject.EnsureObjectOfScene(this, nameof(objectsToExport));

                    if (!actObject.IsExportable)
                    {
                        continue;
                    }

                    result.RegisterOriginalObject(actObject);
                }

                // Second step: Store all needed data into the container
                foreach (var actObject in objectsToExport)
                {
                    if (!actObject.IsExportable)
                    {
                        continue;
                    }

                    actObject.PrepareForExport(result, exportOptions);
                }
            });

            // Return the container
            return result;
        }

        /// <summary>
        /// Imports all objects from the given source.
        /// </summary>
        /// <param name="objSource">The source to load from.</param>
        public Task<IEnumerable<SceneObject>> ImportAsync(ResourceLink objSource)
        {
            return this.ImportAsync(objSource, null);
        }

        /// <summary>
        /// Imports all objects from the given source.
        /// </summary>
        /// <param name="objSource">The source to load from.</param>
        /// <param name="importOptions">All options for import logic.</param>
        public async Task<IEnumerable<SceneObject>> ImportAsync(ResourceLink objSource, ImportOptions? importOptions)
        {
            GraphicsCore.EnsureGraphicsSupportLoaded();

            var modelContainer = await GraphicsCore.Current.ImportersAndExporters!
                .ImportAsync(objSource, importOptions);

            return await this.ImportAsync(modelContainer);
        }

        /// <summary>
        /// Imports all objects from the given source.
        /// </summary>
        public async Task<IEnumerable<SceneObject>> ImportAsync(ImportedModelContainer modelContainer)
        {
            if (!modelContainer.IsFinished ||
                !modelContainer.IsValid)
            {
                throw new SeeingSharpException($"Given {nameof(ImportedModelContainer)} is not finished or valid!");
            }

            // Append all data to the scene
            var result = new List<SceneObject>(modelContainer.Objects.Count);
            await this.ManipulateSceneAsync(manipulator =>
            {
                // AddObject all resources first
                foreach (var actResourceInfo in modelContainer.ImportedResources)
                {
                    manipulator.AddResource(
                        actResourceInfo.ResourceFactory,
                        actResourceInfo.ResourceKey);
                }

                // AddObject all objects
                foreach (var actObject in modelContainer.Objects)
                {
                    manipulator.AddObject(actObject);
                    result.Add(actObject);
                }

                // Apply parent/child relationships
                foreach (var actDependencyInfo in modelContainer.ParentChildRelationships)
                {
                    manipulator.AddChildObject(
                        actDependencyInfo.Parent,
                        actDependencyInfo.Child);
                }
            });
            return result;
        }
    }
}
