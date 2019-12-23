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
using SeeingSharp.Util;
using System;

namespace SeeingSharp.Multimedia.Core
{
    public abstract class SceneObjectFilter
    {
        /// <summary>
        /// An event that notifies changed filter configuration.
        /// </summary>
        public event EventHandler FilterConfigurationChanged;

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
        protected void RaiseFilterConfigurationChanged()
        {
            ConfigurationChangedUI = true;
            this.FilterConfigurationChanged.Raise(this, EventArgs.Empty);
        }

        /// <summary>
        /// Do update this filter on each frame?
        /// </summary>
        public virtual bool UpdateEachFrame => false;

        /// <summary>
        /// Has filter configuration changed?
        /// </summary>
        internal bool ConfigurationChanged;

        /// <summary>
        /// Has filter configuration changed? (temporary flag on UI because of thread synchronization)
        /// </summary>
        internal bool ConfigurationChangedUI;
    }
}
