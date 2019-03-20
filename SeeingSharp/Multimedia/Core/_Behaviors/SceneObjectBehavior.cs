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
namespace SeeingSharp.Multimedia.Core
{
    public abstract class SceneObjectBehavior
    {
        private SceneObject m_host;

        /// <summary>
        /// Update logic for per object updates.
        /// Be careful: This method is called in parallel with these methods on other objects.
        /// For methods that depend on other object, use UpdateOverall.
        /// </summary>
        /// <param name="updateState">Current update state of the scene.</param>
        public abstract void Update(SceneRelatedUpdateState updateState);

        /// <summary>
        /// Update logic for overall updates.
        /// This method should be used for update logic that also depends on other object.
        /// UpdateOverall methods are called sequentially object by object.
        /// </summary>
        /// <param name="updateState">Current update state of the scene.</param>
        public abstract void UpdateOverall(SceneRelatedUpdateState updateState);

        /// <summary>
        /// Called when the current host object has changed.
        /// </summary>
        /// <param name="previousHostObject">The previous host object.</param>
        /// <param name="newHostObject">The newly assigned host object.</param>
        protected virtual void OnHostObjectChanged(SceneObject previousHostObject, SceneObject newHostObject)
        {

        }

        /// <summary>
        /// Sets the host object for this behavior.
        /// </summary>
        /// <param name="hostObject">The object that hosts this behavior.</param>
        internal void SetHostObject(SceneObject hostObject)
        {
            var oldHostObject = m_host;
            m_host = hostObject;

            this.OnHostObjectChanged(oldHostObject, m_host);
        }

        /// <summary>
        /// Gets the host object for this behavior.
        /// </summary>
        public SceneObject HostObject => m_host;
    }
}
