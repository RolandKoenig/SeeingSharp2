using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    public class SeeingSharpInitializer
    {
        private List<ISeeingSharpExtensions> m_extensions;

        internal SeeingSharpInitializer()
        {
            m_extensions = new List<ISeeingSharpExtensions>();
        }

        public void RegisterExtensions(ISeeingSharpExtensions extensions)
        {
            extensions.EnsureNotNull(nameof(extensions));

            m_extensions.Add(extensions);
        }

        public IEnumerable<ISeeingSharpExtensions> Extensions
        {
            get => m_extensions;
        }
    }
}
