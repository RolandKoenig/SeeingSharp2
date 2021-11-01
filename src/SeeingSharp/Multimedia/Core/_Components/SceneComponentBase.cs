namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// Base class for all scene components.
    /// </summary>
    public abstract class SceneComponentBase
    {
        /// <summary>
        /// If not null or empty, this property indicates which components to the same thing.
        /// If you attach a component to a scene where another component with the same group
        /// is active, this other component gets detached automatically.
        /// 
        /// This feature was developed initially for various camera controls which
        /// should not be active simultaneously.
        /// </summary>
        public virtual string ComponentGroup => string.Empty;

        /// <summary>
        /// Is this component specific for one view?
        /// </summary>
        public virtual bool IsViewSpecific => false;

        internal abstract object AttachInternal(SceneManipulator manipulator, ViewInformation correspondingView);

        internal abstract void DetachInternal(SceneManipulator manipulator, ViewInformation correspondingView, object componentContext);

        internal abstract void UpdateInternal(SceneRelatedUpdateState updateState, ViewInformation correspondingView, object componentContext);
    }
}