using System.Collections.Generic;

namespace SeeingSharp.Core
{
    public interface ISeeingSharpExtensionProvider
    {
        IEnumerable<ISeeingSharpExtensions> Extensions { get; }
    }
}
