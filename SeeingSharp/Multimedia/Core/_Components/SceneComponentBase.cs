#region License information
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
#endregion
namespace SeeingSharp.Multimedia.Core
{
    /// <summary>
    /// Base class for all scene components.
    /// </summary>
    public abstract class SceneComponentBase
    {
        internal abstract object AttachInternal(SceneManipulator manipulator, ViewInformation correspondingView);

        internal abstract void DetachInternal(SceneManipulator manipulator, ViewInformation correspondingView, object componentContext);

        internal abstract void UpdateInternal(SceneRelatedUpdateState updateState, ViewInformation correspondingView, object componentContext);

        /// <summary>
        /// If not null or empty, this property indicates which components to the same thing.
        /// If you attach a component to a scene where another component with the same group
        /// is active, this other component gets detached automatically.
        /// 
        /// This feature was developed initially for various camera controls which
        /// should not be activ simultaneously.
        /// </summary>
        public virtual string ComponentGroup => string.Empty;

        /// <summary>
        /// Is this component specific for one view?
        /// </summary>
        public virtual bool IsViewSpecific => false;
    }
}