using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp
{
    public static class SeeingSharpWpfExtensionMethods
    {
        public static SeeingSharpLoader SupportWpf(this SeeingSharpLoader loader)
        {
            loader.RegisterExtensions(new SeeingSharpWpfExtensions());
            return loader;
        }
    }
}
