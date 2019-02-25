#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using Checking;

    #endregion

    /// <summary>
    /// A base class for components which we can easily attach to a scene. 
    /// </summary>
    public abstract class SceneComponent : SceneComponentBase
    {
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

        /// <summary>
        /// Attaches this component to a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
        /// It may also be called from multiple scenes in parallel or simply withoud previous Detach call.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected abstract void Attach(SceneManipulator manipulator, ViewInformation correspondingView);

        /// <summary>
        /// Detaches this component from a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
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
    }

    /// <summary>
    /// A base class for components which we can easily attach to a scene. 
    /// </summary>
    /// <typeparam name="TContextType">An object of this type holds all members hold per scene.</typeparam>
    public abstract class SceneComponent<TContextType> : SceneComponentBase
        where TContextType : class
    {
        internal override object AttachInternal(SceneManipulator manipulator, ViewInformation correspondingView)
        {
            return this.Attach(manipulator, correspondingView);
        }

        internal override void DetachInternal(SceneManipulator manipulator, ViewInformation correspondingView, object componentContext)
        {
            TContextType componentContextCasted = componentContext as TContextType;
            componentContextCasted.EnsureNotNull(nameof(componentContext));

            this.Detach(manipulator, correspondingView, componentContextCasted);
        }

        internal override void UpdateInternal(SceneRelatedUpdateState updateState, ViewInformation correspondingView, object componentContext)
        {
            TContextType componentContextCasted = componentContext as TContextType;
            componentContextCasted.EnsureNotNull(nameof(componentContext));

            this.Update(updateState, correspondingView, componentContextCasted);
        }

        /// <summary>
        /// Attaches this component to a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
        /// It may also be called from multiple scenes in parallel or simply withoud previous Detach call.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="correspondingView">The view which attached this component.</param>
        protected abstract TContextType Attach(SceneManipulator manipulator, ViewInformation correspondingView);

        /// <summary>
        /// Detaches this component from a scene.
        /// Be careful, this method gets called from a background thread of seeing#!
        /// It may also be called from multiple scenes in parallel.
        /// </summary>
        /// <param name="manipulator">The manipulator of the scene we attach to.</param>
        /// <param name="componentContext">A context variable containing all createded objects during call of Attach.</param>
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
    }
}
