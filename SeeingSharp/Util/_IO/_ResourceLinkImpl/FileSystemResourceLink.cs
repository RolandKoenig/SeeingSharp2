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

namespace SeeingSharp.Util
{
    #region using

    using System.IO;
    using System.Threading.Tasks;
    using Checking;

    #endregion

    public class FileSystemResourceLink : ResourceLink
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemResourceLink" /> class.
        /// </summary>
        /// <param name="filePath">The path to the physical file.</param>
        public FileSystemResourceLink(string filePath)
        {
            filePath.EnsureNotNullOrEmptyOrWhiteSpace(nameof(filePath));

            FilePath = filePath;
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
            return "File-Resource: " + FilePath;
        }

        public override bool Exists()
        {
            return File.Exists(FilePath);
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
            string directoryName = Path.GetDirectoryName(FilePath);
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
            return new FileStream(FilePath, FileMode.Create, FileAccess.Write);
        }

        /// <summary>
        /// Opens the input stream to the described resource.
        /// </summary>
        public override Task<Stream> OpenInputStreamAsync()
        {
            return Task.FromResult<Stream>(File.OpenRead(FilePath));
        }

        /// <summary>
        /// Opens a stream to the resource.
        /// </summary>
        public override Stream OpenInputStream()
        {
            return File.OpenRead(FilePath);
        }

        /// <summary>
        /// Gets the file extension of the resource we target to.
        /// </summary>
        public override string FileExtension
        {
            get { return base.GetExtensionFromFileName(FilePath); }
        }

        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        public string FilePath { get; }

        public string FileName
        {
            get { return Path.GetFileName(FilePath); }
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