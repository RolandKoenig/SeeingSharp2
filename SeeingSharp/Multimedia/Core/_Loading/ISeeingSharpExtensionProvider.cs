using System.Collections.Generic;

namespace SeeingSharp.Multimedia.Core
{
    public interface ISeeingSharpExtensionProvider
    {
        IEnumerable<ISeeingSharpExtensions> Extensions { get; }
    }
}
