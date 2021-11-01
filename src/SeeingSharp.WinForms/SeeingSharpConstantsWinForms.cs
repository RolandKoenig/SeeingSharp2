using System;

namespace SeeingSharp
{
    public class SeeingSharpConstantsWinForms
    {
        // Constants for design-time
        public const string DESIGNER_CATEGORY_RENDERER = "Renderer";

        // Misc
        public static readonly TimeSpan MOUSE_CLICK_MAX_TIME = TimeSpan.FromMilliseconds(200.0);
        public static readonly TimeSpan THROTTLED_VIEW_RECREATION_TIME_ON_RESIZE = TimeSpan.FromSeconds(0.5);
    }
}