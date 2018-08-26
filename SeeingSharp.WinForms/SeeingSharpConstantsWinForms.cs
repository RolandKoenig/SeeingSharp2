using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp
{
    public class SeeingSharpConstantsWinForms
    {
        #region Constants for design-time
        public const string DESIGNER_CATEGORY_RENDERER = "Renderer";
        public const string DESIGNER_CATEGORY_CAMERA = "Camera";
        #endregion

        #region Misc
        public static readonly TimeSpan MOUSE_CLICK_MAX_TIME = TimeSpan.FromMilliseconds(200.0);
        public static readonly TimeSpan THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE = TimeSpan.FromSeconds(0.5);
        #endregion
    }
}
