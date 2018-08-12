#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
    public class StreamFactoryResourceLink : ResourceLink
    {
        private Func<Stream> m_streamFactory;
        private string m_fileName;

        /// <summary>
        /// Initializes a new instance of the <see cref="StreamFactoryResourceLink" /> class.
        /// </summary>
        /// <param name="streamFactory">The factory method which creates the stream object.</param>
        /// <param name="fileName">The name of the virtual file.</param>
        public StreamFactoryResourceLink(
            Func<Stream> streamFactory,
            string fileName = "unknown.dat")
        {
            streamFactory.EnsureNotNull(nameof(streamFactory));

            m_streamFactory = streamFactory;
            m_fileName = fileName;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="Func{Stream}"/> to <see cref="StreamFactoryResourceLink"/>.
        /// </summary>
        public static implicit operator StreamFactoryResourceLink(Func<Stream> streamFactory)
        {
            return new StreamFactoryResourceLink(streamFactory);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return "Factory-Resource: " + m_fileName;
        }

        public override bool Exists()
        {
            return true;
        }

        /// <summary>
        /// Gets an object pointing to a file at the same location (e. g. the same directory).
        /// </summary>
        /// <param name="newFileName">The new file name for which to get the ResourceLink object.</param>
        /// <param name="subdirectories">The subdirectory path to the file (optional). This parameter may not be supported by all ResourceLink implementations!</param>
        public override ResourceLink GetForAnotherFile(string newFileName, params string[] subdirectories)
        {
            throw new SeeingSharpException("Unable to read another file on a stream factory source!");
        }

        /// <summary>
        /// Opens an output stream to the current stream source.
        /// </summary>
        public override Stream OpenOutputStream()
        {
            Stream result = m_streamFactory();
            if(!result.CanWrite)
            {
                result.Dispose();
                throw new SeeingSharpException("Can not write to created stream!");
            }
            return result;
        }

        /// <summary>
        /// Opens the input stream to the described resource.
        /// </summary>
        public override Task<Stream> OpenInputStreamAsync()
        {
            return Task.Factory.StartNew(() => OpenInputStream());
        }

        /// <summary>
        /// Opens a stream to the resource.
        /// </summary>
        public override Stream OpenInputStream()
        {
            Stream result = m_streamFactory();
            if (!result.CanRead)
            {
                result.Dispose();
                throw new SeeingSharpException("Can not read from created stream!");
            }
            return result;
        }

        /// <summary>
        /// Gets the file extension of the resource we target to.
        /// </summary>
        public override string FileExtension
        {
            get { return base.GetExtensionFromFileName(m_fileName); }
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
