#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
namespace SeeingSharp.Multimedia.Objects
{
    #region using

    using Core;
    using Drawing3D;
    using SeeingSharp.Util;

    #endregion

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
            var acImportOptions = importOptions as ACImportOptions;

            if (acImportOptions == null)
            {
                throw new SeeingSharpException("Invalid import options for ACImporter!");
            }

            var result = new ImportedModelContainer(acImportOptions);

            // Get needed resource key
            var resGeometry = GraphicsCore.GetNextGenericResourceKey();

            // Load and fill result object
            var objType = ACFileLoader.ImportObjectType(sourceFile);
            result.ImportedResources.Add(new ImportedResourceInfo(
                resGeometry,
                () => new GeometryResource(objType)));
            result.Objects.Add(new GenericObject(resGeometry));

            return result;
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
