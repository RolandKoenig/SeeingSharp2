namespace SeeingSharp.Multimedia.Core
{
    internal class VisibilityCheckFilterStageData
    {
        /// <summary>
        /// Was this filter executed for this object?
        /// </summary>
        internal bool HasExecuted;

        /// <summary>
        /// Has the object passed this filter stage?
        /// </summary>
        internal bool HasPassed;
    }
}
