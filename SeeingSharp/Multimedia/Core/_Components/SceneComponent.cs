/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
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
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// A base class for components which we can easily attach to a scene.
    /// </summary>
    public abstract class SceneComponent : SceneComponentBase
    {
        internal override object AttachInternal(SceneManipulator manipulator, ViewInformation correspondingView)
        {
            Attach(manipulator, correspondingView);
            return null;
        }

        internal override void DetachInternal(SceneManipulator manipulator, ViewInformation correspondingView, object componentContext)
        {
            Detach(manipulator, correspondingView);
        }

        internal override void UpdateInternal(SceneRelatedUpdateState updateState, ViewInformation correspondingView, object componentContext)
        {
            Update(updateState, correspondingView);
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
            return Attach(manipulator, correspondingView);
        }

        internal override void DetachInternal(SceneManipulator manipulator, ViewInformation correspondingView, object componentContext)
        {
            var componentContextCasted = componentContext as TContextType;
            componentContextCasted.EnsureNotNull(nameof(componentContext));

            Detach(manipulator, correspondingView, componentContextCasted);
        }

        internal override void UpdateInternal(SceneRelatedUpdateState updateState, ViewInformation correspondingView, object componentContext)
        {
            var componentContextCasted = componentContext as TContextType;
            componentContextCasted.EnsureNotNull(nameof(componentContext));

            Update(updateState, correspondingView, componentContextCasted);
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