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

using System;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Multimedia.Input
{
    /// <summary>
    /// Base class for all input states.
    /// </summary>
    public abstract class InputStateBase
    {
        /// <summary>
        /// The view object this input state was queried on.
        /// Null, if this InputState does not depend on a view.
        /// </summary>
        public ViewInformation RelatedView { get; internal set; }

        /// <summary>
        /// The view index this input state was queried on.
        /// -1, if this InputState does not depend on a view.
        /// </summary>
        public int ViewIndex
        {
            get
            {
                if (this.RelatedView == null) { return -1; }
                return this.RelatedView.ViewIndex;
            }
        }

        internal Type CurrentType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputStateBase"/> class.
        /// </summary>
        protected InputStateBase()
        {
            this.CurrentType = this.GetType();
        }

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        protected abstract void CopyAndResetForUpdatePassInternal(InputStateBase targetState);

        /// <summary>
        /// Copies this object and then resets it
        /// in preparation of the next update pass.
        /// Called by update-render loop.
        /// </summary>
        internal void CopyAndResetForUpdatePass(InputStateBase targetState)
        {
            this.CopyAndResetForUpdatePassInternal(targetState);
        }
    }
}