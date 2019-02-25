#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
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

    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using Core;
    using SeeingSharp.Util;

    #endregion

    public class ImporterExporterRepository
    {
        private Dictionary<string, SupportedFileFormatAttribute> m_infoByFileType;
        private Dictionary<string, IModelImporter> m_importersByFileType;
        private Dictionary<string, IModelExporter> m_exportersByFileType;
        private List<IModelImporter> m_importers;
        private List<IModelExporter> m_exporters;

        /// <summary>
        /// Prevents a default instance of the <see cref="ImporterExporterRepository"/> class from being created.
        /// </summary>
        internal ImporterExporterRepository(SeeingSharpLoader loader)
        {
            m_importers =
                (from actExtension in loader.Extensions
                 from actImporter in actExtension.CreateModelImporters()
                 select actImporter).ToList();
            m_exporters =
                (from actExtension in loader.Extensions
                 from actExporter in actExtension.CreateModelExporters()
                 select actExporter).ToList();

            m_infoByFileType = new Dictionary<string, SupportedFileFormatAttribute>();

            // Get format support on each importer
            m_importersByFileType = new Dictionary<string, IModelImporter>();
            foreach(IModelImporter actImporter in m_importers)
            {
                foreach(var actSupportedFile in actImporter
                    .GetType().GetTypeInfo()
                    .GetCustomAttributes<SupportedFileFormatAttribute>())
                {
                    string actFileFormat = actSupportedFile.ShortFormatName.ToLower();
                    m_importersByFileType[actFileFormat] = actImporter;
                    m_infoByFileType[actFileFormat] = actSupportedFile;
                }
            }

            // Get format support on each exporter
            m_exportersByFileType = new Dictionary<string, IModelExporter>();
            foreach (IModelExporter actExporter in m_exporters)
            {
                foreach (var actSupportedFile in actExporter
                    .GetType().GetTypeInfo()
                    .GetCustomAttributes<SupportedFileFormatAttribute>())
                {
                    string actFileFormat = actSupportedFile.ShortFormatName.ToLower();
                    m_exportersByFileType[actFileFormat] = actExporter;
                    m_infoByFileType[actFileFormat] = actSupportedFile;
                }
            }
        }

        /// <summary>
        /// Gets a collection containing all supported import formats.
        /// </summary>
        public IEnumerable<SupportedFileFormatAttribute> GetSupportedImportFormats()
        {
            return m_infoByFileType.Values;
        }

        /// <summary>
        /// Gets the filter string for an open file dialog.
        /// </summary>
        public string GetOpenFileDialogFilter()
        {
            if (m_infoByFileType.Count <= 0) { return string.Empty; }

            // Build the filter string for the open file dialog
            StringBuilder filterBuilder = new StringBuilder(1024);

            // Write first item (all formats)
            filterBuilder.Append("All supported files|");
            bool isFirst = true;
            foreach (var actSupportedFormat in this.GetSupportedImportFormats())
            {
                if (!isFirst) { filterBuilder.Append(';'); }
                filterBuilder.Append("*." + actSupportedFormat.ShortFormatName);
                isFirst = false;
            }

            // Write next items (each format separated)
            foreach (var actSupportedFormat in this.GetSupportedImportFormats())
            {
                filterBuilder.Append('|');

                filterBuilder.Append("." + actSupportedFormat.ShortFormatName);
                filterBuilder.Append(" (" + actSupportedFormat.ShortDescription + ")");
                filterBuilder.Append("|*." + actSupportedFormat.ShortFormatName);
            }

            return filterBuilder.ToString();
        }

        /// <summary>
        /// Creates an ImportOptions object by the given source.
        /// </summary>
        /// <param name="source">The source of the resource.</param>
        public ImportOptions CreateImportOptions(ResourceLink source)
        {
            IModelImporter importer = GetImporterBySource(source);
            return importer.CreateDefaultImportOptions();
        }

        /// <summary>
        /// Creates an ImportOptions object by the given source.
        /// </summary>
        /// <param name="fileExtension">The extension of the file to be imported.</param>
        public ImportOptions CreateImportOptionsByFileType(string fileExtension)
        {
            IModelImporter importer = GetImporterByFileType(fileExtension);
            return importer.CreateDefaultImportOptions();
        }

        /// <summary>
        /// Imports model(s) from the given source.
        /// </summary>
        /// <param name="source">The source where to load all objects from..</param>
        public Task<ImportedModelContainer> ImportAsync(ResourceLink source)
        {
            return ImportAsync(source, null);
        }

        /// <summary>
        /// Imports model(s) from the given source.
        /// </summary>
        /// <param name="source">The source where to load all objects from..</param>
        /// <param name="importOptions">The import options.</param>
        public Task<ImportedModelContainer> ImportAsync(ResourceLink source, ImportOptions importOptions)
        {
            IModelImporter importer = GetImporterBySource(source);
            if (importOptions == null) { importOptions = importer.CreateDefaultImportOptions(); }

            // Start the loading task
            return Task.Factory.StartNew<ImportedModelContainer>(() =>
            {
                return importer.ImportModel(source, importOptions);
            });
        }

        /// <summary>
        /// Gets a ModelImporter by the given source.
        /// </summary>
        /// <param name="source">The source of the resource.</param>
        private IModelImporter GetImporterBySource(ResourceLink source)
        {
            // Query for file extension
            string fileExtension = source.FileExtension;
            if (string.IsNullOrEmpty(fileExtension))
            {
                throw new SeeingSharpGraphicsException(string.Format(
                    "Unable to query for file extension from source {0}", source));
            }
            return GetImporterByFileType(fileExtension);
        }

        /// <summary>
        /// Gets a ModelImporter by the given file extension.
        /// </summary>
        /// <param name="fileExtension">The file extension of the resource to load.</param>
        private IModelImporter GetImporterByFileType(string fileExtension)
        {
            fileExtension = fileExtension.ToLower().Replace(".", "");

            // Query for importer object
            IModelImporter importer = null;
            if (!m_importersByFileType.TryGetValue(fileExtension, out importer))
            {
                throw new SeeingSharpGraphicsException(string.Format(
                    "No importer found for file type {0}", fileExtension));
            }
            return importer;
        }
    }
}
