using System;

namespace SeeingSharp.Multimedia.Core
{
    public abstract class SceneObjectFilter
    {
        /// <summary>
        /// Do update this filter on each frame?
        /// </summary>
        public virtual bool UpdateEachFrame => false;

        /// <summary>
        /// Has filter configuration changed?
        /// </summary>
        internal bool ConfigurationChangedInternal;

        internal ulong ConfigurationCheckedCycleID;

        /// <summary>
        /// Has filter configuration changed? (temporary flag on UI because of thread synchronization)
        /// </summary>
        public bool ConfigurationChanged
        {
            get;
            internal set;
        }

        protected SceneObjectFilter()
        {
            this.ConfigurationChanged = true;
        }

        /// <summary>
        /// Checks for visibility of the given object.
        /// </summary>
        /// <param name="input">The object to be checked..</param>
        /// <param name="viewInformation">A reference to the view on which to check for visibility.</param>
        public abstract bool IsObjectVisible(SceneObject input, ViewInformation viewInformation);

        /// <summary>
        /// Sets some informational data telling the filter where it is used.
        /// </summary>
        /// <param name="layerToFilter">The SceneLayer that gets filtered.</param>
        /// <param name="viewInformation">The information object of the corresponding view.</param>
        public virtual void SetEnvironmentData(SceneLayer layerToFilter, ViewInformation viewInformation)
        {

        }

        /// <summary>
        /// Raises the FilterConfigurationChanged event.
        /// </summary>
        public void NotifyFilterConfigurationChanged()
        {
            this.ConfigurationChanged = true;
        }
    }
}
