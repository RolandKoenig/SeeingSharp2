using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    internal class VisibilityCheckData
    {
        /// <summary>
        /// All data the the corresponding filter stages.
        /// </summary>
        internal IndexBasedDynamicCollection<VisibilityCheckFilterStageData> FilterStageData;

        /// <summary>
        /// Is this object visible?
        /// </summary>
        internal bool IsVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="VisibilityCheckData"/> class.
        /// </summary>
        internal VisibilityCheckData()
        {
            FilterStageData = new IndexBasedDynamicCollection<VisibilityCheckFilterStageData>();
        }
    }
}
