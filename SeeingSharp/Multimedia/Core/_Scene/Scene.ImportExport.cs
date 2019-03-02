#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Collections.Generic;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    #region using
    #endregion

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
            await PerformBeforeUpdateAsync(() =>
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
                foreach(var actObject in objectsToExport)
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
            return ImportAsync(objSource, null);
        }

        /// <summary>
        /// Imports all objects from the given source.
        /// </summary>
        /// <param name="objSource">The source to load from.</param>
        /// <param name="importOptions">All options for import logic.</param>
        public async Task<IEnumerable<SceneObject>> ImportAsync(ResourceLink objSource, ImportOptions importOptions)
        {
            var result = new List<SceneObject>();

            // Import all data
            var modelContainer = await GraphicsCore.Current.ImportersAndExporters
                .ImportAsync(objSource, importOptions);

            // Append all data to the scene
            await ManipulateSceneAsync(manipulator =>
            {
                // Add all resources first
                foreach(var actResourceInfo in modelContainer.ImportedResources)
                {
                    manipulator.AddResource(
                        actResourceInfo.ResourceFactory,
                        actResourceInfo.ResourceKey);
                }

                // Add all objects
                foreach(var actObject in modelContainer.Objects)
                {
                    manipulator.Add(actObject);
                    result.Add(actObject);
                }

                // Apply parent/child relationships
                foreach(var actDependencyInfo in modelContainer.ParentChildRelationships)
                {
                    manipulator.AddChild(
                        actDependencyInfo.Item1,
                        actDependencyInfo.Item2);
                }
            });

            return result;
        }
    }
}
