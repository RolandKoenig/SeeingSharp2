using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp.Multimedia.Core
{
    public interface ISeeingSharpExtensionProvider
    {
        IEnumerable<ISeeingSharpExtensions> Extensions { get; }
    }
}
