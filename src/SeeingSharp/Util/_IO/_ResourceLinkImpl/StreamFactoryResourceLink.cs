using System;
using System.IO;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    public class StreamFactoryResourceLink : ResourceLink
    {
        private string _fileName;
        private Func<Stream> _streamFactory;

        /// <summary>
        /// Gets the file extension of the resource we target to.
        /// </summary>
        public override string FileExtension => this.GetExtensionFromFileName(_fileName);

        public override string FileNameWithExtension => _fileName;

        /// <summary>
        /// Are async operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsAsync => true;

        /// <summary>
        /// Are synchronous operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsSync => true;

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

            _streamFactory = streamFactory;
            _fileName = fileName;
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
            return "Factory-Resource: " + _fileName;
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
            var result = _streamFactory();

            if (!result.CanWrite)
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
            return Task.FromResult(this.OpenInputStream());
        }

        /// <summary>
        /// Opens a stream to the resource.
        /// </summary>
        public override Stream OpenInputStream()
        {
            var result = _streamFactory();

            if (!result.CanRead)
            {
                result.Dispose();
                throw new SeeingSharpException("Can not read from created stream!");
            }

            return result;
        }
    }
}
