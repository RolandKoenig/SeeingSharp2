using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp
{
    public static class SeeingSharpUwpExtensionMethods
    {
        public static SeeingSharpLoader SupportUwp(this SeeingSharpLoader loader)
        {
            loader.RegisterExtensions(new SeeingSharpUwpExtensions());
            return loader;
        }
    }
}
