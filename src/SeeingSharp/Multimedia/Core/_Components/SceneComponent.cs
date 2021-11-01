using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// A base class for components which we can easily attach to a scene.
    /// </summary>
    public abstract class SceneComponent : SceneComponentBase
    {
        /// <summary>
        /// Attaches this component to a scene.
        /// Be careful, this method gets called from a background thread of SeeingSharp!
        /// It may also be called from multiple scenes in parallel or simply without previous Detach call.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected abstract void Attach(SceneManipulator manipulator, ViewInformation correspondingView);

        /// <summary>
        /// Detaches this component from a scene.
        /// Be careful, this method gets called from a background thread of SeeingSharp!
        /// It may also be called from multiple scenes in parallel.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected abstract void Detach(SceneManipulator manipulator, ViewInformation correspondingView);

        /// <summary>
        /// This update method gets called on each update pass for each scenes
        /// this component is attached to.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        /// <param name="correspondingView">The view which attached this component (may be null).</param>
        protected virtual void Update(SceneRelatedUpdateState updateState, ViewInformation correspondingView)
        {
        }

        internal override object AttachInternal(SceneManipulator manipulator, ViewInformation correspondingView)
        {
            this.Attach(manipulator, correspondingView);
            return null;
        }

        internal override void DetachInternal(SceneManipulator manipulator, ViewInformation correspondingView, object componentContext)
        {
            this.Detach(manipulator, correspondingView);
        }

        internal override void UpdateInternal(SceneRelatedUpdateState updateState, ViewInformation correspondingView, object componentContext)
        {
            this.Update(updateState, correspondingView);
        }
    }

    /// <summary>
    /// A base class for components which we can easily attach to a scene.
    /// </summary>
    /// <typeparam name="TContextType">An object of this type holds all members hold per scene.</typeparam>
    public abstract class SceneComponent<TContextType> : SceneComponentBase
        where TContextType : class
    {
        /// <summary>
        /// Attaches this component to a scene.
        /// Be careful, this method gets called from a background thread of SeeingSharp!
        /// It may also be called from multiple scenes in parallel or simply without previous Detach call.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected abstract TContextType Attach(SceneManipulator manipulator, ViewInformation correspondingView);

        /// <summary>
        /// Detaches this component from a scene.
        /// Be careful, this method gets called from a background thread of SeeingSharp!
        /// It may also be called from multiple scenes in parallel.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="componentContext">A context variable containing all created objects during call of Attach.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected abstract void Detach(SceneManipulator manipulator, ViewInformation correspondingView, TContextType componentContext);

        /// <summary>
        /// This update method gets called on each update pass for each scenes
        /// this component is attached to.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        /// <param name="correspondingView">The view which attached this component (may be null).</param>
        /// <param name="componentContext">The current context generating during Attach call.</param>
        protected virtual void Update(SceneRelatedUpdateState updateState, ViewInformation correspondingView, TContextType componentContext)
        {
        }

        internal override object AttachInternal(SceneManipulator manipulator, ViewInformation correspondingView)
        {
            return this.Attach(manipulator, correspondingView);
        }

        internal override void DetachInternal(SceneManipulator manipulator, ViewInformation correspondingView, object componentContext)
        {
            var componentContextCasted = componentContext as TContextType;
            componentContextCasted.EnsureNotNull(nameof(componentContext));

            this.Detach(manipulator, correspondingView, componentContextCasted);
        }

        internal override void UpdateInternal(SceneRelatedUpdateState updateState, ViewInformation correspondingView, object componentContext)
        {
            var componentContextCasted = componentContext as TContextType;
            componentContextCasted.EnsureNotNull(nameof(componentContext));

            this.Update(updateState, correspondingView, componentContextCasted);
        }
    }
}