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
    public abstract class Resource : IDisposable
    {
        private EngineDevice m_device;
        private NamedOrGenericKey m_key;
        private bool m_markedForReloading;
        private ResourceDictionary m_resourceDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        protected Resource()
        {
            this.ResourceType = this.GetType();
            m_key = new NamedOrGenericKey();
        }

        /// <summary>
        /// Fills the given collection with all referenced resources.
        /// </summary>
        /// <param name="resourceCollection">The collection to be filled,</param>
        public virtual void GetReferencedResources(SingleInstanceCollection<Resource> resourceCollection)
        {

        }

        /// <summary>
        /// Disposes this object (unloads all resources).
        /// </summary>
        public void Dispose()
        {
            this.UnloadResource();
        }

        /// <summary>
        /// Loads all resource.
        /// </summary>
        /// <param name="device">The device on which to load all resources.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected abstract void LoadResourceInternal(EngineDevice device, ResourceDictionary resources);

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        /// <param name="device">The device on which the resources where loaded.</param>
        /// <param name="resources">The current ResourceDictionary.</param>
        protected abstract void UnloadResourceInternal(EngineDevice device, ResourceDictionary resources);

        /// <summary>
        /// Triggers reloading of the resource.
        /// </summary>
        protected void ReloadResource()
        {
            m_markedForReloading = true;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        internal void LoadResource()
        {
            if (m_resourceDictionary == null) { throw new SeeingSharpGraphicsException("Unable to load resource: Resource " + m_key + " hos no registered ResourceDictionary!"); }
            if (m_device == null) { throw new SeeingSharpGraphicsException("Unable to load resource: Resource " + m_key + " hos no registered Device!"); }

            try
            {
                this.LoadResourceInternal(m_device, m_resourceDictionary);
            }
            finally
            {
                m_markedForReloading = false;
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        internal void UnloadResource()
        {
            if (m_resourceDictionary == null) { throw new SeeingSharpGraphicsException("Unable to unload resource: Resource " + m_key + " hos no registered ResourceDictionary!"); }
            if (m_device == null) { throw new SeeingSharpGraphicsException("Unable to unload resource: Resource " + m_key + " hos no registered Device!"); }

            this.UnloadResourceInternal(m_device, m_resourceDictionary);
        }

        /// <summary>
        /// Is the resource loaded?
        /// </summary>
        public abstract bool IsLoaded
        {
            get;
        }

        /// <summary>
        /// Is this resource marked for reloading.
        /// </summary>
        public bool IsMarkedForReloading
        {
            get
            {
                if (!this.IsLoaded) { return false; }
                return m_markedForReloading;
            }
        }

        /// <summary>
        /// Gets the key of the resource.
        /// </summary>
        public NamedOrGenericKey Key
        {
            get => m_key;
            internal set
            {
                if (!m_key.IsEmpty) { throw new SeeingSharpGraphicsException("Unable to change key because there is already a valid key set!"); }
                m_key = value;
            }
        }

        /// <summary>
        /// Is the resource key empty?
        /// </summary>
        public bool IsKeyEmpty => m_key.IsEmpty;

        /// <summary>
        /// Gets the parent ResourceDictionary object.
        /// </summary>
        public ResourceDictionary Dictionary
        {
            get => m_resourceDictionary;
            internal set
            {
                m_resourceDictionary = value;
                if (m_resourceDictionary != null) { m_device = m_resourceDictionary.Device; }
                else { m_device = null; }
            }
        }

        /// <summary>
        /// Gets the type of this resource.
        /// </summary>
        public Type ResourceType { get; }
    }
}