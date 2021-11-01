using System.IO;
using System.Threading.Tasks;
using SeeingSharp.Checking;

namespace SeeingSharp.Util
{
    public class FileSystemResourceLink : ResourceLink
    {
        /// <summary>
        /// Gets the file extension of the resource we target to.
        /// </summary>
        public override string FileExtension => this.GetExtensionFromFileName(this.FilePath);

        public override string FileNameWithExtension => Path.GetFileName(this.FilePath);

        /// <summary>
        /// Gets the path to the file.
        /// </summary>
        public string FilePath { get; }

        public string FileName => Path.GetFileName(this.FilePath);

        /// <summary>
        /// Are async operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsAsync => true;

        /// <summary>
        /// Are synchronous operations supported on this ResourceLink?
        /// </summary>
        public override bool SupportsSync => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="FileSystemResourceLink" /> class.
        /// </summary>
        /// <param name="filePath">The path to the physical file.</param>
        public FileSystemResourceLink(string filePath)
        {
            filePath.EnsureNotNullOrEmptyOrWhiteSpace(nameof(filePath));

            this.FilePath = filePath;
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
            return "File-Resource: " + this.FilePath;
        }

        public override bool Exists()
        {
            return File.Exists(this.FilePath);
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
            var subdirectoryPath = string.Empty;
            for (var loop = 0; loop < subdirectories.Length; loop++)
            {
                subdirectoryPath += subdirectories[loop] + "\\";
            }

            // Return new ResourceLink pointing to the other file
            var directoryName = Path.GetDirectoryName(this.FilePath);
            if (!string.IsNullOrEmpty(directoryName))
            {
                return new FileSystemResourceLink(Path.Combine(directoryName, subdirectoryPath + newFileName));
            }
            return new FileSystemResourceLink(newFileName);
        }

        /// <summary>
        /// Opens an output stream to the current stream source.
        /// </summary>
        public override Stream OpenOutputStream()
        {
            return new FileStream(this.FilePath, FileMode.Create, FileAccess.Write);
        }

        /// <summary>
        /// Opens the input stream to the described resource.
        /// </summary>
        public override Task<Stream> OpenInputStreamAsync()
        {
            return Task.FromResult<Stream>(File.OpenRead(this.FilePath));
        }

        /// <summary>
        /// Opens a stream to the resource.
        /// </summary>
        public override Stream OpenInputStream()
        {
            return File.OpenRead(this.FilePath);
        }
    }
}