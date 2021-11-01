namespace SeeingSharp.Multimedia.Views
{
    public enum WpfSeeingSharpCompositionMode
    {
        /// <summary>
        /// SeeingSharp composes its rendering with Wpf over hardware.
        /// </summary>
        OverHardware,

        /// <summary>
        /// SeeingSharp composes its rendering with Wpf over software based fallback solution.
        /// </summary>
        FallbackOverSoftware,

        /// <summary>
        /// Currently no composition active.
        /// </summary>
        None
    }
}