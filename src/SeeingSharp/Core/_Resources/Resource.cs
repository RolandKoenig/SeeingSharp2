using System;
using SeeingSharp.Core.Devices;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public abstract class Resource : IDisposable
    {
        private EngineDevice _device;
        private NamedOrGenericKey _key;
        private bool _markedForReloading;
        private ResourceDictionary _resourceDictionary;

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
                return _markedForReloading;
            }
        }

        /// <summary>
        /// Gets the key of the resource.
        /// </summary>
        public NamedOrGenericKey Key
        {
            get => _key;
            internal set
            {
                if (!_key.IsEmpty) { throw new SeeingSharpGraphicsException("Unable to change key because there is already a valid key set!"); }
                _key = value;
            }
        }

        /// <summary>
        /// Is the resource key empty?
        /// </summary>
        public bool IsKeyEmpty => _key.IsEmpty;

        /// <summary>
        /// Gets the parent ResourceDictionary object.
        /// </summary>
        public ResourceDictionary Dictionary
        {
            get => _resourceDictionary;
            internal set
            {
                _resourceDictionary = value;
                if (_resourceDictionary != null) { _device = _resourceDictionary.Device; }
                else { _device = null; }
            }
        }

        /// <summary>
        /// Gets the type of this resource.
        /// </summary>
        public Type ResourceType { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Resource"/> class.
        /// </summary>
        protected Resource()
        {
            this.ResourceType = this.GetType();
            _key = new NamedOrGenericKey();
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
            _markedForReloading = true;
        }

        /// <summary>
        /// Loads the resource.
        /// </summary>
        internal void LoadResource()
        {
            if (_resourceDictionary == null) { throw new SeeingSharpGraphicsException("Unable to load resource: Resource " + _key + " hos no registered ResourceDictionary!"); }
            if (_device == null) { throw new SeeingSharpGraphicsException("Unable to load resource: Resource " + _key + " hos no registered Device!"); }

            try
            {
                this.LoadResourceInternal(_device, _resourceDictionary);
            }
            finally
            {
                _markedForReloading = false;
            }
        }

        /// <summary>
        /// Unloads the resource.
        /// </summary>
        internal void UnloadResource()
        {
            if (_resourceDictionary == null) { throw new SeeingSharpGraphicsException("Unable to unload resource: Resource " + _key + " hos no registered ResourceDictionary!"); }
            if (_device == null) { throw new SeeingSharpGraphicsException("Unable to unload resource: Resource " + _key + " hos no registered Device!"); }

            this.UnloadResourceInternal(_device, _resourceDictionary);
        }
    }
}