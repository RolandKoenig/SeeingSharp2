#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    public class FileSystemResourceLink : ResourceLink
    {
        private string m_filePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemResourceLink" /> class.
        /// </summary>
        /// <param name="filePath">The path to the physical file.</param>
        public FileSystemResourceLink(string filePath)
        {
            filePath.EnsureNotNullOrEmptyOrWhiteSpace(nameof(filePath));

            m_filePath = filePath;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="FileSystemResourceLink"/>.
        /// </summary>
        public static implicit operator FileSystemResourceLink(string fileName)
        {
            return new FileSystemResourceLink(fileName);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return "File-Resource: " + m_filePath;
        }

        public override bool Exists()
        {
            return File.Exists(m_filePath);
        }

        /// <summary>
        /// Gets an object pointing to a file at the same location (e. g. the same directory).
        /// </summary>
        /// <param name="newFileName">The new file name for which to get the ResourceLink object.</param>
        /// <param name="subdirectories">The subdirectory path to the file (optional). This parameter may not be supported by all ResourceLink implementations!</param>
        public override ResourceLink GetForAnotherFile(string newFileName, params string[] subdirectories)
        {
            newFileName.EnsureNotNullOrEmptyOrWhiteSpace(nameof(newFileName));

            // Build subdirectory path
            string subdirectoryPath = string.Empty;
            for (int loop = 0; loop < subdirectories.Length; loop++)
            {
                subdirectoryPath += subdirectories[loop] + "\\";
            }

            // Return new ResourceLink pointing to the other file
            string directoryName = Path.GetDirectoryName(m_filePath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                return new FileSystemResourceLink(Path.Combine(directoryName, subdirectoryPath + newFileName));
            }
            else
            {
                return new FileSystemResourceLink(newFileName);
            }
        }

        /// <summary>
        /// Opens an output stream to the current stream source.
        /// </summary>
        public override Stream OpenOutputStream()
        {
            return new FileStream(m_filePath, FileMode.Create, FileAccess.Write);
        }

        /// <summary>
        /// Opens the input stream to the described resource.
        /// </summary>
        public override Task<Stream> OpenInputStreamAsync()
        {
            return Task.FromResult<Stream>(File.OpenRead(m_filePath));
        }

        /// <summary>
        /// Opens a stream to the resource.
        /// </summary>
        public override Stream OpenInputStream()
        {
            return File.OpenRead(m_filePath);
        }

        /// <summary>
        /// Gets the file extension of the resource we target to.
        /// </summary>
        public override string FileExtension
        {
            get { return base.GetExtensionFromFileName(m_filePath); }
        }

        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        public string FilePath
        {
            get { return m_filePath; }
        }

        public string FileName
        {
            get { return Path.GetFileName(m_filePath); }
        }

        /// <summary>
        /// Are async operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsAsync
        {
            get { return true; }
        }

        /// <summary>
        /// Are synchronous operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsSync
        {
            get { return true; }
        }
    }
}