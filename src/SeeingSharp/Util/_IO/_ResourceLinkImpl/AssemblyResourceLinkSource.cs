using System.IO;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    public class AssemblyResourceLinkSource : ResourceLink
    {
        private AssemblyResourceLink _resourceLink;

        /// <summary>
        /// Gets the file extension of the resource we target to.
        /// </summary>
        public override string FileExtension => this.GetExtensionFromFileName(_resourceLink.ResourceFile);

        public override string FileNameWithExtension => _resourceLink.ResourceFile;

        /// <summary>
        /// Are async operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsAsync => true;

        /// <summary>
        /// Are synchronous operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsSync => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="AssemblyResourceLinkSource" /> class.
        /// </summary>
        /// <param name="resourceLink">The link to the resource.</param>
        public AssemblyResourceLinkSource(AssemblyResourceLink resourceLink)
        {
            resourceLink.EnsureNotNull(nameof(resourceLink));

            _resourceLink = resourceLink;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="AssemblyResourceLink"/> to <see cref="AssemblyResourceLinkSource"/>.
        /// </summary>
        public static implicit operator AssemblyResourceLinkSource(AssemblyResourceLink resourceLink)
        {
            return new AssemblyResourceLinkSource(resourceLink);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return "Assembly-Resource: " + _resourceLink;
        }

        /// <summary>
        /// Gets an object pointing to a file at the same location (e. g. the same directory).
        /// </summary>
        /// <param name="newFileName">The new file name for which to get the ResourceLink object.</param>
        /// <param name="subdirectories">The subdirectory path to the file (optional). This parameter may not be supported by all ResourceLink implementations!</param>
        public override ResourceLink GetForAnotherFile(string newFileName, params string[] subdirectories)
        {
            newFileName.EnsureNotNullOrEmptyOrWhiteSpace(nameof(newFileName));

            return new AssemblyResourceLinkSource(
                _resourceLink.GetForAnotherFile(newFileName, subdirectories));
        }

        /// <summary>
        /// Opens an output stream to the current stream source.
        /// </summary>
        public override Stream OpenOutputStream()
        {
            throw new SeeingSharpException("Unable to get an output stream to an assembly resource!");
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
            return _resourceLink.OpenRead();
        }

        public override bool Exists()
        {
            return _resourceLink.IsValid();
        }
    }
}
