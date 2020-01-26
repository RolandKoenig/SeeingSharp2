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
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Drawing3D
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
            if (!(importOptions is ACImportOptions acImportOptions))
            {
                throw new SeeingSharpException("Invalid import options for ACImporter!");
            }

            // Create result container
            var modelContainer = new ImportedModelContainer(acImportOptions);

            // Read the AC file
            var fileInfo = ACFileLoader.LoadFile(sourceFile.OpenInputStream());
            var geometry = ACFileLoader.GenerateGeometry(fileInfo);

            // Generate GeometryResource
            var resGeometry = modelContainer.GetResourceKey("Geometry", "Main");
            modelContainer.ImportedResources.Add(new ImportedResourceInfo(
                resGeometry,
                device => new GeometryResource(geometry)));

            // Generate Material resources
            var materialKeys = new NamedOrGenericKey[fileInfo.Materials.Count];
            for (var loop = 0; loop < materialKeys.Length; loop++)
            {
                var actACMaterial = fileInfo.Materials[loop];
                materialKeys[loop] = modelContainer.GetResourceKey("Material", actACMaterial.Name);

                modelContainer.ImportedResources.Add(new ImportedResourceInfo(
                    materialKeys[loop],
                    (device) => new StandardMaterialResource()
                    {
                        UseVertexColors = false,
                        MaterialDiffuseColor = actACMaterial.Diffuse,
                    }));
            }

            // Create the mesh
            modelContainer.Objects.Add(new Mesh(resGeometry, materialKeys));

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
