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
        public static SeeingSharpInitializer SupportWpf(this SeeingSharpInitializer initializer)
        {
            initializer.RegisterExtensions(new SeeingSharpWpfExtensions());
            return initializer;
        }
    }
}
