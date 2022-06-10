using System;
using System.Collections.Generic;
using System.Text;
using SeeingSharp.Core;

namespace SeeingSharp.Views
{
    public interface IDpiScalingProvider
    {
        /// <summary>
        /// Gets information about current dpi scaling.
        /// The result may change depending on location of the view or by user preferences.
        /// </summary>
        DpiScaling GetCurrentDpiScaling();
    }
}
