using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Multimedia.Views
{
    public enum WpfSeeingSharpCompositionMode
    {
        /// <summary>
        /// Seeingsharp composes its rendering with Wpf over hardware.
        /// </summary>
        OverHardware,

        /// <summary>
        /// Seeingsharp composes its rendering with Wpf over software based fallback solution.
        /// </summary>
        FallbackOverSoftware,

        /// <summary>
        /// Currently no composition active.
        /// </summary>
        None
    }
}
