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

using System;
using System.IO;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    public abstract class ResourceLink
    {
        /// <summary>
        /// Gets the file extension of the resource we target to.
        /// </summary>
        public abstract string FileExtension
        {
            get;
        }

        public abstract string FileNameWithExtension { get; }

        /// <summary>
        /// Are async operations supported on this ResourceLink?
        /// </summary>
        public abstract bool SupportsAsync
        {
            get;
        }

        /// <summary>
        /// Are synchronous operations supported on this ResourceLink?
        /// </summary>
        public abstract bool SupportsSync
        {
            get;
        }

        /// <summary>
        /// Writes the contents of this resource to the given dummy file.
        /// </summary>
        /// <param name="fileName">Name of the dummy file.</param>
        public void WriteAllBytesToDummyFile(string fileName)
        {
            using (var inStream = this.OpenInputStream())

            using (Stream outStream = new FileStream(fileName, FileMode.Create, FileAccess.Write))
            {
                var buffer = new byte[1024];
                var readBytes = inStream.Read(buffer, 0, buffer.Length);
                while (readBytes > 0)
                {
                    outStream.Write(buffer, 0, readBytes);
                    readBytes = inStream.Read(buffer, 0, buffer.Length);
                }
            }
        }

        /// <summary>
        /// Reads the complete resource to a new string.
        /// </summary>
        public string ReadCompleteToString()
        {
            using (var inStream = this.OpenInputStream())

            using (var inStreamReader = new StreamReader(inStream))
            {
                return inStreamReader.ReadToEnd();
            }
        }

        /// <summary>
        /// Reads the complete resource to a new string.
        /// </summary>
        public async Task<string> ReadCompleteToStringAsync()
        {
            using (var inStream = await this.OpenInputStreamAsync())

            using (var inStreamReader = new StreamReader(inStream))
            {
                return await inStreamReader.ReadToEndAsync();
            }
        }

        /// <summary>
        /// Does the resource behind this link already exist?
        /// </summary>
        public abstract bool Exists();

        /// <summary>
        /// Gets an object pointing to a file at the same location (e. g. the same directory).
        /// </summary>
        /// <param name="newFileName">The new file name for which to get the ResourceLink object.</param>
        /// <param name="subdirectories">The subdirectory path to the file (optional). This parameter may not be supported by all ResourceLink implementations!</param>
        public abstract ResourceLink GetForAnotherFile(string newFileName, params string[] subdirectories);

        /// <summary>
        /// Opens an output stream to the current stream source.
        /// </summary>
        public abstract Stream OpenOutputStream();

        /// <summary>
        /// Opens the input stream to the described resource.
        /// </summary>
        public abstract Task<Stream> OpenInputStreamAsync();

        /// <summary>
        /// Opens a stream to the resource.
        /// </summary>
        public abstract Stream OpenInputStream();

        public static implicit operator ResourceLink(AssemblyResourceLink streamFactory)
        {
            return new AssemblyResourceLinkSource(streamFactory);
        }

        public static implicit operator ResourceLink(Func<Stream> streamFactory)
        {
            return new StreamFactoryResourceLink(streamFactory);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="ResourceLink"/>.
        /// </summary>
        public static implicit operator ResourceLink(string fileName)
        {
            return new FileSystemResourceLink(fileName);
        }

        /// <summary>
        /// Gets the name of the extension from the given file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        protected string GetExtensionFromFileName(string fileName)
        {
            fileName.EnsureNotNullOrEmptyOrWhiteSpace(nameof(fileName));

            // Try to read format out of the file name
            if (!string.IsNullOrEmpty(fileName))
            {
                var indexLastDot = fileName.LastIndexOf('.');
                if (indexLastDot < 0) { return string.Empty; }
                if (fileName.Length < indexLastDot + 1) { return string.Empty; }

                return fileName.Substring(indexLastDot + 1).ToLower();
            }
            return string.Empty;
        }
    }
}
