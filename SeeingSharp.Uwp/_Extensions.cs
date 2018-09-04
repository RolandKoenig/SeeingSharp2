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
        public static SeeingSharpInitializer SupportWinForms(this SeeingSharpInitializer initializer)
        {
            initializer.RegisterExtensions(new SeeingSharpWinFormsExtensions());
            return initializer;
        }
    }
}
