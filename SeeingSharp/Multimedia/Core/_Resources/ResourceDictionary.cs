#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using SeeingSharp.Util;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
    public class ResourceDictionary : IEnumerable<Resource>
    {
        private EngineDevice m_device;
        private List<IRenderableResource> m_renderableResources;
        private ReadOnlyCollection<IRenderableResource> m_renderableResourcesPublic;
        private Dictionary<NamedOrGenericKey, ResourceInfo> m_resources;
        private ThreadSaveQueue<Resource> m_resourcesMarkedForUnloading;
        private int m_lastRenderBlockID;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceDictionary"/> class.
        /// </summary>
        internal ResourceDictionary(EngineDevice device)
        {
            m_device = device;
            this.DeviceIndex = m_device.DeviceIndex;

            m_lastRenderBlockID = -1;

            m_renderableResources = new List<IRenderableResource>();
            m_renderableResourcesPublic = new ReadOnlyCollection<IRenderableResource>(m_renderableResources);

            m_resources = new Dictionary<NamedOrGenericKey, ResourceInfo>();
            m_resourcesMarkedForUnloading = new ThreadSaveQueue<Resource>();
        }

        /// <summary>
        /// Gets the next render block id.
        /// </summary>
        internal int GetNextRenderBlockID()
        {
            if (m_lastRenderBlockID == Int32.MaxValue) { m_lastRenderBlockID = 0; }
            else { m_lastRenderBlockID = m_lastRenderBlockID + 1; }

            return m_lastRenderBlockID;
        }

        /// <summary>
        /// Creates the default resource for the given type name..
        /// </summary>
        /// <typeparam name="T">The type for which the generic resource should be created.</typeparam>
        internal static T CreateDefaultResource<T>()
            where T : Resource
        {
            Type resourceType = typeof(T);
            T result = null;

            // Try to create default resources
            if (resourceType == typeof(MaterialResource)) { result = new SimpleColoredMaterialResource() as T; }
            else if (resourceType == typeof(TextureResource))
            {
#if DESKTOP
                result = new LinearGradientTextureResource(
                    Color4.White,
                    Color4.LightGray,
                    GradientDirection.Directional,
                    32, 32) as T;
#else
                result = new StandardTextureResource(
                    new AssemblyResourceLink(
                        typeof(ResourceDictionary),
                        "SeeingSharp.Multimedia.Resources.Textures.Blank_16x16.png")) as T;
#endif
            }
            else if(resourceType == typeof(GeometryResource))
            {
                VertexStructure dummyStructure = new VertexStructure();
                dummyStructure.FirstSurface.BuildCube24V(
                    Vector3.Zero,
                    new Vector3(1f, 1f, 1f),
                    Color4.White);
                result = new GeometryResource(dummyStructure) as T;
            }

            //Try to create the resource using the standard constructor
            if (result == null)
            {
#if DESKTOP
                ConstructorInfo standardConstructor = resourceType.GetConstructor(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                    null, Type.EmptyTypes, null);
#else
                ConstructorInfo standardConstructor = 
                    resourceType.GetTypeInfo().DeclaredConstructors
                    .FirstOrDefault((actConstructor) => actConstructor.GetParameters().Length <= 0);
#endif
                if (standardConstructor != null)
                {
                    result = Activator.CreateInstance(resourceType) as T;
                }
            }

            if (result == null) { throw new SeeingSharpGraphicsException("Unable to create default resource for resource type " + resourceType.FullName); }
            return result;
        }

        /// <summary>
        /// Clears all resources.
        /// </summary>
        internal void Clear()
        {
            foreach (ResourceInfo actResource in m_resources.Values)
            {
                if (actResource.Resource.IsLoaded)
                {
                    actResource.Resource.UnloadResource();
                    actResource.Resource.Dictionary = null;
                }
            }

            m_renderableResources.Clear();
            m_resources.Clear();
        }

        /// <summary>
        /// Adds the given resource to the dictionary.
        /// </summary>
        /// <param name="resource">The key of the resource.</param>
        internal ResourceType AddResource<ResourceType>(ResourceType resource)
            where ResourceType : Resource
        {
            return AddResource(NamedOrGenericKey.Empty, resource);
        }

        /// <summary>
        /// Adds the given resource to the dictionary.
        /// </summary>
        /// <param name="resource">The resource to add.</param>
        /// <param name="resourceKey">The key of the resource.</param>
        internal ResourceType AddResource<ResourceType>(NamedOrGenericKey resourceKey, ResourceType resource)
            where ResourceType : Resource
        {
            //Perform some checks
            if (resource == null) { throw new ArgumentNullException("resource"); }
            if (resource.Dictionary != null)
            {
                if (resource.Dictionary == this) { return resource; }
                if (resource.Dictionary != this) { throw new ArgumentException("Given resource belongs to another ResourceDictionary!", "resource"); }
            }

            //Check given keys
            if ((!resource.IsKeyEmpty) && (!resourceKey.IsEmpty) && (resource.Key != resourceKey))
            {
                throw new ArgumentException("Unable to override existing key on given resource!");
            }

            //Remove another resource with the same name
            if (!resource.IsKeyEmpty)
            {
                RemoveResource(resource.Key);
            }
            if (!resourceKey.IsEmpty)
            {
                RemoveResource(resourceKey);
            }

            //Apply a valid key on the given resource object
            if (resource.Key.IsEmpty)
            {
                if (resourceKey.IsEmpty) { resource.Key = GraphicsCore.GetNextGenericResourceKey(); }
                else { resource.Key = resourceKey; }
            }

            //Add the resource
            ResourceInfo newResource = new ResourceInfo(resource);
            m_resources[resource.Key] = newResource;
            if (newResource.RenderableResource != null) { m_renderableResources.Add(newResource.RenderableResource); }

            //Register this dictionary on the resource
            resource.Dictionary = this;

            return resource;
        }

        /// <summary>
        /// Unloads all resources that are marked for unloading.
        /// </summary>
        internal void UnloadAllMarkedResources()
        {
            foreach(var actResource in m_resourcesMarkedForUnloading.DequeueAll())
            {
                if (!actResource.IsKeyEmpty)
                {
                    this.RemoveResource(actResource.Key);
                }
            }
        }

        /// <summary>
        /// Marks the given resource for unloading.
        /// </summary>
        /// <param name="resourceToUnload">The resource to be unloaded.</param>
        internal void MarkForUnloading(Resource resourceToUnload)
        {
            m_resourcesMarkedForUnloading.Enqueue(resourceToUnload);
        }

        ///// <summary>
        ///// Adds a new material for texture drawing pointing to the given texture resource.
        ///// </summary>
        ///// <param name="textureResourceKey">The key of the texture the material is pointing to.</param>
        //public SimpleColoredMaterialResource AddSimpleTexturedMaterial(NamedOrGenericKey textureResourceKey)
        //{
        //    return this.AddResource(new SimpleColoredMaterialResource(textureResourceKey));
        //}

        ///// <summary>
        ///// Adds a new material for texture drawing pointing to the given texture resource.
        ///// </summary>
        ///// <param name="resourceKey">The key of the resource.</param>
        ///// <param name="textureResourceKey">The key of the texture the material is pointing to.</param>
        //public SimpleColoredMaterialResource AddSimpleTexturedMaterial(NamedOrGenericKey resourceKey, NamedOrGenericKey textureResourceKey)
        //{
        //    return this.AddResource(resourceKey, new SimpleColoredMaterialResource(textureResourceKey));
        //}

        ///// <summary>
        ///// Adds a new material for drawing a simple colored mesh.
        ///// </summary>
        //public SimpleColoredMaterialResource AddSimpleColoredMaterial()
        //{
        //    return this.AddResource(new SimpleColoredMaterialResource());
        //}

        ///// <summary>
        ///// Adds a new material for drawing a simple colored mesh.
        ///// </summary>
        ///// <param name="resourceKey">The key of the resource.</param>
        //public SimpleColoredMaterialResource AddSimpleColoredMaterial(NamedOrGenericKey resourceKey)
        //{
        //    return this.AddResource(resourceKey, new SimpleColoredMaterialResource());
        //}

        /// <summary>
        /// Adds the given resource to the dictionary and loads it directly.
        /// </summary>
        /// <param name="resourceKey">The key of the resource.</param>
        /// <param name="resource">The resource to add.</param>
        internal ResourceType AddAndLoadResource<ResourceType>(NamedOrGenericKey resourceKey, ResourceType resource)
            where ResourceType : Resource
        {
            AddResource(resourceKey, resource);
            if (!resource.IsLoaded) { resource.LoadResource(); }

            return resource;
        }

        /// <summary>
        /// Removes the resource with the givenkey.
        /// </summary>
        /// <param name="key">The key to check.</param>
        internal void RemoveResource(NamedOrGenericKey key)
        {
            if (m_resources.ContainsKey(key))
            {
                ResourceInfo resourceInfo = m_resources[key];

                //Unload the resource
                if (resourceInfo.Resource.IsLoaded) { resourceInfo.Resource.UnloadResource(); }
                if (resourceInfo.RenderableResource != null) { m_renderableResources.Remove(resourceInfo.RenderableResource); }

                //Remove the resource
                m_resources.Remove(key);

                resourceInfo.Resource.Dictionary = null;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
        /// </returns>
        public IEnumerator<Resource> GetEnumerator()
        {
            return new ResourceEnumerator(m_resources.Values.GetEnumerator());
        }

        /// <summary>
        /// Gets the resource with the given key. CreateMethod will be called to create
        /// the resource if it is not available yet.
        /// </summary>
        /// <param name="resourceKey">Key of the resource.</param>
        /// <param name="createMethod">Method wich creates the resource.</param>
        internal T GetResource<T>(NamedOrGenericKey resourceKey, Func<T> createMethod)
            where T : Resource
        {
            if (m_resources.ContainsKey(resourceKey))
            {
                T result = m_resources[resourceKey].Resource as T;

                if(result != null) { return result; }
                else { m_resources.Remove(resourceKey); }
            }

            T newResource = createMethod();
            if (newResource == null) { return null; }

            AddResource(resourceKey, newResource);
            return newResource;
        }

        /// <summary>
        /// Gets the resource with the gien name.
        /// </summary>
        /// <typeparam name="T">Type of the resource.</typeparam>
        /// <param name="resourceKey">Key of the resource.</param>
        internal T GetResource<T>(NamedOrGenericKey resourceKey)
            where T : Resource
        {
            T result = null;
            if (!ContainsResource(resourceKey))
            {
                // Try to query for existing default resources if given key is empty
                if(resourceKey.IsEmpty)
                {
                    resourceKey = new NamedOrGenericKey(typeof(T).FullName);
                    if (ContainsResource(resourceKey)) { result = GetResource<T>(resourceKey); }
                }

                // Create a default resource if still nothing found
                if (result == null)
                {
                    result = CreateDefaultResource<T>();
                    this.AddResource<T>(resourceKey, result);
                }
            }
            else
            {
                // Resource does exist, so return existing
                var currentResource = m_resources[resourceKey].Resource;
                result = currentResource as T;
                if((currentResource != null) && (result == null))
                {
                    throw new SeeingSharpGraphicsException("Resource type mismatch: Behind the requested key " + resourceKey + " is a resource of another type (requested: " + typeof(T).FullName + ", current: " + currentResource.GetType().FullName + ")");
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the resource with the given key. CreateMethod will be called to create
        /// the resource if it is not available yet.
        /// </summary>
        /// <param name="resourceKey">Key of the resource.</param>
        /// <param name="createMethod">Method wich creates the resource.</param>
        internal T GetResourceAndEnsureLoaded<T>(NamedOrGenericKey resourceKey, Func<T> createMethod)
            where T : Resource
        {
            T resource = GetResource(resourceKey, createMethod);
            if (!resource.IsLoaded) { resource.LoadResource(); }
            return resource;
        }

        /// <summary>
        /// Gets the resource with the given key.
        /// </summary>
        /// <typeparam name="T">Type of the resource.</typeparam>
        /// <param name="resourceKey">Key of the resource.</param>
        internal T GetResourceAndEnsureLoaded<T>(NamedOrGenericKey resourceKey)
            where T : Resource
        {
            T resource = GetResource<T>(resourceKey);
            if (!resource.IsLoaded) { resource.LoadResource(); }
            return resource;
        }

        /// <summary>
        /// Loads all resources.
        /// </summary>
        public void LoadResources()
        {
            List<ResourceInfo> allResources = new List<ResourceInfo>(m_resources.Values);
            foreach (ResourceInfo actResourceInfo in allResources)
            {
                //Load the resource
                if (!actResourceInfo.Resource.IsLoaded)
                {
                    actResourceInfo.Resource.LoadResource();
                }

                //Reload the resource
                if (actResourceInfo.Resource.IsMarkedForReloading)
                {
                    actResourceInfo.Resource.UnloadResource();
                    actResourceInfo.Resource.LoadResource();
                }
            }
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        public void UnloadResources()
        {
            foreach (ResourceInfo actResourceInfo in m_resources.Values)
            {
                if (actResourceInfo.Resource.IsLoaded)
                {
                    actResourceInfo.Resource.UnloadResource();
                }
            }
        }

        /// <summary>
        /// Is there a resource with the given key?
        /// </summary>
        /// <param name="key">Key of the resource.</param>
        public bool ContainsResource(NamedOrGenericKey key)
        {
            return m_resources.ContainsKey(key);
        }

        /// <summary>
        /// Gets the device this resource dictionary belongs to.
        /// </summary>
        public EngineDevice Device
        {
            get { return m_device; }
        }

        /// <summary>
        /// Gets the device index.
        /// </summary>
        internal int DeviceIndex;

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new ResourceEnumerator(m_resources.Values.GetEnumerator());
        }

        /// <summary>
        /// Gets the resource with the given key.
        /// </summary>
        /// <param name="key">Key of the resource.</param>
        public Resource this[NamedOrGenericKey key]
        {
            get { return m_resources[key].Resource; }
        }

        /// <summary>
        /// Gets an enumeration containing all renderable resources.
        /// </summary>
        public ReadOnlyCollection<IRenderableResource> RenderableResources
        {
            get { return m_renderableResourcesPublic; }
        }

        /// <summary>
        /// Gets a reference to default resources object.
        /// </summary>
        public DefaultResources DefaultResources
        {
            get
            {
                return GetResourceAndEnsureLoaded<DefaultResources>(DefaultResources.RESOURCE_KEY);
            }
        }

        /// <summary>
        /// Gets total count of resource.
        /// </summary>
        public int Count
        {
            get { return m_resources.Count; }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ResourceInfo
        {
            public Resource Resource;
            public IRenderableResource RenderableResource;

            /// <summary>
            /// Initializes a new instance of the <see cref="ResourceInfo"/> class.
            /// </summary>
            /// <param name="resource">The resource.</param>
            public ResourceInfo(Resource resource)
            {
                this.Resource = resource;
                this.RenderableResource = resource as IRenderableResource;
            }
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        private class ResourceEnumerator : IEnumerator<Resource>
        {
            private IEnumerator<ResourceInfo> m_resourceInfoEnumerator;

            /// <summary>
            /// Initializes a new instance of the <see cref="ResourceEnumerator"/> class.
            /// </summary>
            /// <param name="resourceInfoEnumerator">The resource info enumerator.</param>
            public ResourceEnumerator(IEnumerator<ResourceInfo> resourceInfoEnumerator)
            {
                m_resourceInfoEnumerator = resourceInfoEnumerator;
            }

            /// <summary>
            /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                m_resourceInfoEnumerator.Dispose();
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns>
            /// true if the enumerator was successfully advanced to the next element; false if the enumerator has passed the end of the collection.
            /// </returns>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public bool MoveNext()
            {
                return m_resourceInfoEnumerator.MoveNext();
            }

            /// <summary>
            /// Sets the enumerator to its initial position, which is before the first element in the collection.
            /// </summary>
            /// <exception cref="T:System.InvalidOperationException">
            /// The collection was modified after the enumerator was created.
            /// </exception>
            public void Reset()
            {
                m_resourceInfoEnumerator.Reset();
            }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            public Resource Current
            {
                get { return m_resourceInfoEnumerator.Current.Resource; }
            }

            /// <summary>
            /// Gets the element in the collection at the current position of the enumerator.
            /// </summary>
            /// <value></value>
            /// <returns>
            /// The element in the collection at the current position of the enumerator.
            /// </returns>
            object IEnumerator.Current
            {
                get { return Current; }
            }
        }
    }
}