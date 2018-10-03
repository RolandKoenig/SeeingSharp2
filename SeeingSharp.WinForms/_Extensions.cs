using SeeingSharp.Multimedia.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp
{
    public static class SeeingSharpWinFormsExtensionMethods
    {
        public static SeeingSharpLoader SupportWinForms(this SeeingSharpLoader loader)
        {
            loader.RegisterExtensions(new SeeingSharpWinFormsExtensions());
            return loader;
        }
    }
}
