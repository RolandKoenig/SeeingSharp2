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
#region using

// Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

#endregion

namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Checking;
    using Drawing2D;
    using Drawing3D;
    using SeeingSharp.Util;
    using SharpDX;

    #endregion

    public partial class Scene
    {
        #region Constants
        public const string DEFAULT_LAYER_NAME = "Default";
        #endregion

        #region Misc
        private bool m_initialized;
        private string m_name;
        private List<SceneLayer> m_sceneLayers;

        #endregion

        #region Some other logical parts of the scene object
        private SceneComponentFlyweight m_sceneComponents;
        #endregion

        #region Members for 3D rendering
        private CBPerFrame m_perFrameData;
        #endregion

        #region Members for 2D rendering

        private List<Custom2DDrawingLayer> m_drawing2DLayers;
        #endregion Members for 2D rendering

        #region Async update actions
        private ThreadSaveQueue<Action> m_asyncInvokesBeforeUpdate;
        private ThreadSaveQueue<Action> m_asyncInvokesUpdateBesideRendering;
        #endregion Async update actions

        #region Resource keys
        private NamedOrGenericKey KEY_SCENE_RENDER_PARAMETERS = GraphicsCore.GetNextGenericResourceKey();
        #endregion Resource keys

        #region Some runtime values
        private IndexBasedDynamicCollection<ResourceDictionary> m_registeredResourceDicts;
        private IndexBasedDynamicCollection<ViewInformation> m_registeredViews;
        private IndexBasedDynamicCollection<SceneRenderParameters> m_renderParameters;
        #endregion Some runtime values

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene" /> class.
        /// </summary>
        /// <param name="name">The global name of this scene.</param>
        /// <param name="registerOnMessenger">
        /// Do register this scene for application messaging?
        /// If true, then the caller has to ensure that the name is only used once
        /// across the currently executed application.
        /// </param>
        public Scene()
        {
            m_perFrameData = new CBPerFrame();

            TransformMode2D = Graphics2DTransformMode.Custom;
            CustomTransform2D = Matrix3x2.Identity;
            VirtualScreenSize2D = new Size2F();

            m_sceneComponents = new SceneComponentFlyweight(this);

            m_sceneLayers = new List<SceneLayer>();
            m_sceneLayers.Add(new SceneLayer(DEFAULT_LAYER_NAME, this));
            Layers = new ReadOnlyCollection<SceneLayer>(m_sceneLayers);

            m_drawing2DLayers = new List<Custom2DDrawingLayer>();

            m_asyncInvokesBeforeUpdate = new ThreadSaveQueue<Action>();
            m_asyncInvokesUpdateBesideRendering = new ThreadSaveQueue<Action>();

            m_registeredResourceDicts = new IndexBasedDynamicCollection<ResourceDictionary>();
            m_registeredViews = new IndexBasedDynamicCollection<ViewInformation>();
            m_renderParameters = new IndexBasedDynamicCollection<SceneRenderParameters>();

            this.CachedUpdateState = new SceneRelatedUpdateState(this);

            // Try to initialize this scene object
            InitializeResourceDictionaries();
        }

        /// <summary>
        /// Waits until the given object is visible on the given view.
        /// </summary>
        /// <param name="sceneObject">The scene object to be checked.</param>
        /// <param name="viewInfo">The view on which to check.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        public Task WaitUntilVisibleAsync(SceneObject sceneObject, ViewInformation viewInfo, CancellationToken cancelToken = default(CancellationToken))
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));
            sceneObject.EnsureNotNull(nameof(viewInfo));

            return WaitUntilVisibleAsync(
                new SceneObject[] { sceneObject },
                viewInfo, cancelToken);
        }

        /// <summary>
        /// Gets a full list of <see cref="SceneObjectInfo"/>  objects describing the contents of this scene.
        /// </summary>
        /// <param name="layer">Only return objects of this layer (empty means all layers).</param>
        public async Task<List<SceneObjectInfo>> GetSceneObjectInfoAsync(string layer = "")
        {
            List<SceneObjectInfo> result = new List<SceneObjectInfo>(16);
            await this.PerformBesideRenderingAsync(() =>
            {
                foreach(var actLayer in m_sceneLayers)
                {
                    // Layer filter
                    if((!string.IsNullOrEmpty(layer)) &&
                       (actLayer.Name != layer))
                    {
                        continue;
                    }

                    foreach(var actSceneObject in actLayer.ObjectsInternal)
                    {
                        if (actSceneObject.HasChilds) { continue; }
                        result.Add(new Core.SceneObjectInfo(actSceneObject, buildFullChildTree: true));
                    }
                }
            });

            return result;
        }

        /// <summary>
        /// Gets a <see cref="SceneObjectInfo"/>  object describing the given <see cref="SceneObject"/> .
        /// </summary>
        /// <param name="sceneObject">The <see cref="SceneObject"/> to describe.</param>
        public async Task<SceneObjectInfo> GetSceneObjectInfoAsync(SceneObject sceneObject)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));
            sceneObject.EnsureObjectOfScene(this, nameof(sceneObject));

            SceneObjectInfo result = null;
            await this.PerformBesideRenderingAsync(() =>
            {
                result = new SceneObjectInfo(sceneObject, buildFullChildTree: true);
            });

            return result;
        }

        /// <summary>
        /// Waits until the given object is visible.
        /// </summary>
        /// <param name="sceneObjects">The scene objects to check for.</param>
        /// <param name="viewInfo">The view on which to check for visibility.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        public Task WaitUntilVisibleAsync(IEnumerable<SceneObject> sceneObjects, ViewInformation viewInfo, CancellationToken cancelToken = default(CancellationToken))
        {
            sceneObjects.EnsureNotNull(nameof(sceneObjects));
            viewInfo.EnsureNotNull(nameof(viewInfo));

            TaskCompletionSource<object> taskComplSource = new TaskCompletionSource<object>();

            // Define the poll action (polling is done inside scene update
            Action pollAction = null;
            pollAction = () =>
            {
                if (AreAllObjectsVisible(sceneObjects, viewInfo))
                {
                    taskComplSource.SetResult(null);
                }
                else if (cancelToken.IsCancellationRequested)
                {
                    taskComplSource.SetCanceled();
                }
                else
                {
                    m_asyncInvokesBeforeUpdate.Enqueue(pollAction);
                }
            };

            // Register first call of the polling action
            m_asyncInvokesBeforeUpdate.Enqueue(pollAction);

            return taskComplSource.Task;
        }

        /// <summary>
        /// Ares all given scene objects visible currently?
        /// </summary>
        /// <param name="sceneObjects">The scene objects.</param>
        /// <param name="viewInfo">The view information.</param>
        public bool AreAllObjectsVisible(IEnumerable<SceneObject> sceneObjects, ViewInformation viewInfo)
        {
            sceneObjects.EnsureNotNull(nameof(sceneObjects));
            viewInfo.EnsureNotNull(nameof(viewInfo));

            foreach (var actObject in sceneObjects)
            {
                if (!actObject.IsVisible(viewInfo)) { return false; }
            }
            return true;
        }

        /// <summary>
        /// Attaches the given component to this scene.
        /// </summary>
        /// <param name="component">The component to be attached.</param>
        /// <param name="sourceView">The view which attaches the component.</param>
        public void AttachComponent(SceneComponentBase component, ViewInformation sourceView = null)
        {
            m_sceneComponents.AttachComponent(component, sourceView);
        }

        /// <summary>
        /// Detaches the given component from this scene.
        /// </summary>
        /// <param name="component">The component to be detached.</param>
        /// <param name="sourceView">The view which attached the component initially.</param>
        public void DetachComponent(SceneComponentBase component, ViewInformation sourceView = null)
        {
            m_sceneComponents.AttachComponent(component, sourceView);
        }

        /// <summary>
        /// Detaches all currently attached components.
        /// </summary>
        /// <param name="sourceView">The view from which we've to detach all components.</param>
        public void DetachAllComponents(ViewInformation sourceView = null)
        {
            m_sceneComponents.DetachAllComponents(sourceView);
        }

        /// <summary>
        /// Triggers scene manipulation using the given lambda action.
        /// The action gets processed directly before scene update process.
        ///
        /// Be carefull: The action is called by worker-threads of SeeingSharp!
        /// </summary>
        /// <param name="manipulatorAction">The action which is able to manipulate the scene.</param>
        public Task ManipulateSceneAsync(Action<SceneManipulator> manipulatorAction)
        {
            manipulatorAction.EnsureNotNull(nameof(manipulatorAction));

            var manipulator = new SceneManipulator(this);

            return this.PerformBeforeUpdateAsync(() =>
            {
                try
                {
                    manipulator.IsValid = true;
                    manipulatorAction(manipulator);
                }
                finally
                {
                    manipulator.IsValid = false;
                }
            });
        }

        /// <summary>
        /// Adds a resource to the scene
        /// </summary>
        /// <typeparam name="ResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The factory method which creates the resource object.</param>
        /// <param name="resourceKey">The key for the newly generated resource.</param>
        /// <returns></returns>
        internal NamedOrGenericKey AddResource<ResourceType>(Func<ResourceType> resourceFactory, NamedOrGenericKey resourceKey)
            where ResourceType : Resource
        {
            resourceFactory.EnsureNotNull(nameof(resourceFactory));

            InitializeResourceDictionaries();

            if (resourceKey == NamedOrGenericKey.Empty)
            {
                resourceKey = GraphicsCore.GetNextGenericResourceKey();
            }

            foreach (var actResourceDict in m_registeredResourceDicts)
            {
                actResourceDict.AddResource(resourceKey, resourceFactory());
            }

            return resourceKey;
        }

        /// <summary>
        /// Does a resource with the given key exist?
        /// </summary>
        /// <param name="resourceKey">The key to check for.</param>
        internal bool ContainsResource(NamedOrGenericKey resourceKey)
        {
            if (resourceKey == NamedOrGenericKey.Empty)
            {
                throw new ArgumentException("Given resource key is empty!");
            }

            foreach (var actResourceDict in m_registeredResourceDicts)
            {
                if (actResourceDict.ContainsResource(resourceKey))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Manipulates the resource with the given key.
        /// </summary>
        /// <typeparam name="ResourceType">The type of the resource.</typeparam>
        /// <param name="manipulateAction">The action that manipulates the resource.</param>
        /// <param name="resourceKey">The key of the resource to be manipulated.</param>
        internal void ManipulateResource<ResourceType>(Action<ResourceType> manipulateAction, NamedOrGenericKey resourceKey)
            where ResourceType : Resource
        {
            manipulateAction.EnsureNotNull(nameof(manipulateAction));

            InitializeResourceDictionaries();

            foreach (var actResourceDict in m_registeredResourceDicts)
            {
                var actResource = actResourceDict.GetResource<ResourceType>(resourceKey);

                if (actResource == null)
                {
                    throw new SeeingSharpGraphicsException("Resource " + resourceKey + " of type " + typeof(ResourceType).FullName + " not found on device " + actResourceDict.Device.AdapterDescription + "!");
                }

                manipulateAction(actResource);
            }
        }

        /// <summary>
        /// Removes the resource with the given key.
        /// </summary>
        /// <param name="resourceKey">The key of the resource to be deleted.</param>
        internal void RemoveResource(NamedOrGenericKey resourceKey)
        {
            InitializeResourceDictionaries();

            foreach (var actResourceDict in m_registeredResourceDicts)
            {
                actResourceDict.RemoveResource(resourceKey);
            }
        }

        /// <summary>
        /// Picks an object in 3D space.
        /// </summary>
        internal List<SceneObject> Pick(Vector3 rayStart, Vector3 rayDirection, ViewInformation viewInformation, PickingOptions pickingOptions)
        {
            rayDirection.EnsureNormalized(nameof(rayDirection));
            viewInformation.EnsureNotNull(nameof(viewInformation));
            pickingOptions.EnsureNotNull(nameof(pickingOptions));

            // Query for all objects below the cursor
            List<Tuple<SceneObject, float>> pickedObjects = new List<Tuple<SceneObject, float>>();

            foreach (var actLayer in m_sceneLayers)
            {
                foreach (var actObject in actLayer.Objects)
                {
                    if (!actObject.IsVisible(viewInformation)) { continue; }
                    if (!actObject.IsPickingTestVisible) { continue; }

                    float actDistance = actObject.Pick(rayStart, rayDirection, viewInformation, pickingOptions);

                    if (!float.IsNaN(actDistance))
                    {
                        pickedObjects.Add(Tuple.Create(actObject, actDistance));
                    }
                }
            }

            // Return all picked object in correct order
            return pickedObjects
                .OrderBy((actObject) => actObject.Item2)
                .Convert((actObject) => actObject.Item1)
                .ToList();
        }

        /// <summary>
        /// Triggers new filter logic for the given scene object.
        /// </summary>
        /// <param name="sceneObjectLocal">The object to trigger filter logic for.</param>
        internal void TriggerNewFilter(SceneObject sceneObjectLocal)
        {
            sceneObjectLocal.EnsureNotNull(nameof(sceneObjectLocal));

            sceneObjectLocal.ClearVisibilityStageData();
        }

        /// <summary>
        /// Adds the given drawing layer.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        internal void AddDrawingLayer(Custom2DDrawingLayer drawingLayer)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));

            if (!m_drawing2DLayers.Contains(drawingLayer))
            {
                m_drawing2DLayers.Add(drawingLayer);
            }
        }

        /// <summary>
        /// Removes the given drawing layer.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        internal void RemoveDrawingLayer(Custom2DDrawingLayer drawingLayer)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));

            while (m_drawing2DLayers.Remove(drawingLayer)) { }
        }

        /// <summary>
        /// Adds the given object to the scene.
        /// </summary>
        /// <param name="sceneObject">Object to add.</param>
        /// <param name="layer">Layer on wich the object should be added.</param>
        internal T Add<T>(T sceneObject, string layer)
            where T : SceneObject
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));
            layer.EnsureNotNullOrEmpty(nameof(layer));

            InitializeResourceDictionaries();

            var layerObject = GetLayer(layer);

            if (!layerObject.AddObject(sceneObject))
            {
                return null;
            }

            return sceneObject;
        }

        /// <summary>
        /// Registers the given view on this scene object.
        /// This method is meant to be called by RenderLoop class.
        /// </summary>
        /// <param name="viewInformation">The view to register.</param>
        internal void RegisterView(ViewInformation viewInformation)
        {
            viewInformation.EnsureNotNull(nameof(viewInformation));

            bool isFirstView = m_registeredViews.Count == 0;

            InitializeResourceDictionaries();

            // Register device on this scene if not done before
            //  -> This registration is forever, no deregister is made!
            var givenDevice = viewInformation.Device;

            if (!m_registeredResourceDicts.HasObjectAt(givenDevice.DeviceIndex))
            {
                throw new SeeingSharpGraphicsException("ResourceDictionary of device " + givenDevice.AdapterDescription + " not loaded in this scene!");
            }

            // Check for already done registration of this view
            // If there is any, then caller made an error
            if (m_registeredViews.Contains(viewInformation))
            {
                throw new SeeingSharpGraphicsException("The given view is already registered on this scene!");
            }

            // Register this view on this scene and on all layers
            int viewIndex = m_registeredViews.AddObject(viewInformation);

            foreach (var actLayer in m_sceneLayers)
            {
                actLayer.RegisterView(viewIndex, viewInformation, m_registeredResourceDicts[givenDevice.DeviceIndex]);
            }

            viewInformation.ViewIndex = viewIndex;

            // Mark this scene for deletion if we don't have any other view registered
            if (isFirstView)
            {
                GraphicsCore.Current.MainLoop.DeregisterSceneForUnload(this);
            }
        }

        /// <summary>
        /// Is the given view registered on this scene?
        /// </summary>
        /// <param name="viewInformation">The view to check for.</param>
        internal bool IsViewRegistered(ViewInformation viewInformation)
        {
            viewInformation.EnsureNotNull(nameof(viewInformation));

            return m_registeredViews.Contains(viewInformation);
        }

        /// <summary>
        /// Deregisters the given view from this scene object.
        /// This method is meant to be called by RenderLoop class.
        /// </summary>
        /// <param name="viewInformation">The view to deregister.</param>
        internal void DeregisterView(ViewInformation viewInformation)
        {
            viewInformation.EnsureNotNull(nameof(viewInformation));

            // Check for registration
            // If there is no one, then the caller made an error
            if (!m_registeredViews.Contains(viewInformation))
            {
                throw new SeeingSharpGraphicsException("The given view is not registered on this scene!");
            }

            // Deregister the view on this scene and all layers
            int viewIndex = m_registeredViews.IndexOf(viewInformation);
            m_registeredViews.RemoveObject(viewInformation);

            foreach (var actLayer in m_sceneLayers)
            {
                actLayer.DeregisterView(viewIndex, viewInformation);
            }

            // Clear view index
            viewInformation.ViewIndex = -1;

            // Mark this scene for deletion if we don't have any other view registered
            if ((m_registeredViews.Count <= 0) &&
                (!this.DiscardAutomaticUnload))
            {
                GraphicsCore.Current.MainLoop.RegisterSceneForUnload(this);
            }
        }

        /// <summary>
        /// Adds a new layer with the given name.
        /// </summary>
        /// <param name="name">Name of the layer.</param>
        internal SceneLayer AddLayer(string name)
        {
            name.EnsureNotNullOrEmpty(nameof(name));

            var currentLayer = TryGetLayer(name);

            if (currentLayer != null)
            {
                throw new ArgumentException("There is already a SceneLayer with the given name!", "name");
            }

            // Create the new layer
            var newLayer = new SceneLayer(name, this)
            {
                OrderID = m_sceneLayers.Max((actLayer) => actLayer.OrderID) + 1
            };

            m_sceneLayers.Add(newLayer);

            // Sort local layer list
            SortLayers();

            // Register all views on the newsly generated layer
            foreach (var actViewInfo in m_registeredViews)
            {
                newLayer.RegisterView(
                    m_registeredViews.IndexOf(actViewInfo),
                    actViewInfo,
                    m_registeredResourceDicts[actViewInfo.Device.DeviceIndex]);
            }

            return newLayer;
        }

        /// <summary>
        /// Removes the layer with the given name.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        internal void RemoveLayer(string layerName)
        {
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            var layerToRemove = TryGetLayer(layerName);

            if (layerToRemove != null)
            {
                RemoveLayer(layerToRemove);
            }
        }

        /// <summary>
        /// Sets the order id of the given layer.
        /// </summary>
        /// <param name="layer">The layer to modify.</param>
        /// <param name="newOrderID">the new order id.</param>
        internal void SetLayerOrderID(SceneLayer layer, int newOrderID)
        {
            layer.EnsureNotNull(nameof(layer));

            if (!m_sceneLayers.Contains(layer)) { throw new ArgumentException("This scene does not contain the given layer!"); }

            // Change the order id
            layer.OrderID = newOrderID;

            // Sort local layer list
            this.SortLayers();
        }

        /// <summary>
        /// Removes the given layer from the scene.
        /// </summary>
        /// <param name="layer">Layer to remove.</param>
        internal void RemoveLayer(SceneLayer layer)
        {
            layer.EnsureNotNull(nameof(layer));

            if (layer == null) { throw new ArgumentNullException("layer"); }
            if (layer.Scene != this) { throw new ArgumentException("Given layer does not belong to this scene!", "layer"); }
            if (layer.Name == DEFAULT_LAYER_NAME) { throw new ArgumentNullException("Unable to remove the default layer!", "layer"); }

            layer.UnloadResources();
            m_sceneLayers.Remove(layer);

            // Sort local layer list
            SortLayers();
        }

        /// <summary>
        /// Clears the layer with the given name.
        /// </summary>
        /// <param name="layerName">The name of the layer.</param>
        internal void ClearLayer(string layerName)
        {
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            var layerToClear = GetLayer(layerName);
            ClearLayer(layerToClear);
        }

        /// <summary>
        /// Clears the given layer.
        /// </summary>
        /// <param name="layer">The layer to be cleared.</param>
        internal void ClearLayer(SceneLayer layer)
        {
            layer.EnsureNotNull(nameof(layer));

            if (layer == null) { throw new ArgumentNullException("layer"); }
            if (layer.Scene != this) { throw new ArgumentException("Given layer does not belong to this scene!", "layer"); }

            layer.ClearObjects();
        }

        /// <summary>
        /// Gets the layer with the given name.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        internal SceneLayer TryGetLayer(string layerName)
        {
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            if (string.IsNullOrEmpty(layerName))
            {
                throw new ArgumentException("Given layer name is not valid!", "layerName");
            }

            foreach (var actLayer in m_sceneLayers)
            {
                if (actLayer.Name == layerName)
                {
                    return actLayer;
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the layer with the given name.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        internal SceneLayer GetLayer(string layerName)
        {
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            var result = TryGetLayer(layerName);

            if (result == null)
            {
                throw new ArgumentException($"Layer {layerName} not found!");
            }

            return result;
        }

        /// <summary>
        /// Clears the scene.
        /// </summary>
        /// <param name="clearResources">Clear all resources too?</param>
        internal void Clear(bool clearResources)
        {
            // Clear all layers
            for (var loop = 0; loop < m_sceneLayers.Count; loop++)
            {
                var actLayer = m_sceneLayers[loop];
                actLayer.ClearObjects();

                if (actLayer.Name != DEFAULT_LAYER_NAME)
                {
                    m_sceneLayers.RemoveAt(loop);
                    loop--;
                }
            }

            // Clears all 2D drawing layers
            m_drawing2DLayers.Clear();

            // Clear all resources
            if (clearResources)
            {
                foreach (var actDictionary in m_registeredResourceDicts)
                {
                    actDictionary.Clear();
                }

                m_renderParameters.Clear();

                for (var loop = 0; loop < m_sceneLayers.Count; loop++)
                {
                    SceneLayer actLayer = m_sceneLayers[loop];
                    actLayer.ClearResources();
                }
            }
        }

        internal void RemoveRange(IEnumerable<SceneObject> sceneObjects)
        {
            sceneObjects.EnsureNotNull(nameof(sceneObjects));

            foreach(var actObject in sceneObjects)
            {
                this.Remove(actObject);
            }
        }

        internal void RemoveRange(IEnumerable<SceneObject> sceneObjects, string layerName)
        {
            sceneObjects.EnsureNotNull(nameof(sceneObjects));

            foreach (var actObject in sceneObjects)
            {
                this.Remove(actObject, layerName);
            }
        }

        /// <summary>
        /// Removes the given object from the scene.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        internal void Remove(SceneObject sceneObject)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));

            foreach (var actLayer in m_sceneLayers)
            {
                actLayer.RemoveObject(sceneObject);
            }
        }

        /// <summary>
        /// Removes the given object from the scene.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        /// <param name="layerName">Layer on wich the scene object was added.</param>
        internal void Remove(SceneObject sceneObject, string layerName)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            var layerObject = GetLayer(layerName);
            layerObject.RemoveObject(sceneObject);
        }

        /// <summary>
        /// Performs the given action before updating the scene.
        /// (given action gets called by update thread, no other actions on the scene during this time.)
        /// </summary>
        /// <param name="actionToInvoke">The action to be invoked.</param>
        public Task PerformBeforeUpdateAsync(Action actionToInvoke)
        {
            actionToInvoke.EnsureNotNull(nameof(actionToInvoke));

            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            m_asyncInvokesBeforeUpdate.Enqueue(() =>
            {
                try
                {
                    actionToInvoke();

                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            });

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Performs the given action beside rendering process.
        /// (given action gets called by update thread while render threads are rendering.)
        /// </summary>
        /// <param name="actionToInvoke">The action to be invoked.</param>
        public Task PerformBesideRenderingAsync(Action actionToInvoke)
        {
            actionToInvoke.EnsureNotNull(nameof(actionToInvoke));

            TaskCompletionSource<bool> taskCompletionSource = new TaskCompletionSource<bool>();

            m_asyncInvokesUpdateBesideRendering.Enqueue(() =>
            {
                try
                {
                    actionToInvoke();

                    taskCompletionSource.TrySetResult(true);
                }
                catch (Exception ex)
                {
                    taskCompletionSource.TrySetException(ex);
                }
            });

            return taskCompletionSource.Task;
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        internal void Update(SceneRelatedUpdateState updateState)
        {
            m_perFrameData.Time = m_perFrameData.Time + updateState.UpdateTimeMilliseconds;
            if (m_perFrameData.Time > SeeingSharpConstants.MAX_PER_FRAME_TIME_VALUE)
            {
                m_perFrameData.Time = m_perFrameData.Time % SeeingSharpConstants.MAX_PER_FRAME_TIME_VALUE;
            }

            // Update all scene components first
            //  These may trigger some further manipulation actions
            m_sceneComponents.UpdateSceneComponents(updateState);

            // Invoke all async action attached to this scene
            int asyncActionsBeforeUpdateCount = m_asyncInvokesBeforeUpdate.Count;

            if (asyncActionsBeforeUpdateCount > 0)
            {
                Action actAsyncAction = null;
                int actIndex = 0;

                while ((actIndex < asyncActionsBeforeUpdateCount) &&
                       m_asyncInvokesBeforeUpdate.Dequeue(out actAsyncAction))
                {
                    actAsyncAction();
                    actIndex++;
                }
            }

            // Render all renderable resources
            foreach (var actResourceDict in m_registeredResourceDicts)
            {
                foreach (var actRenderableResource in actResourceDict.RenderableResources)
                {
                    if (actRenderableResource.IsLoaded)
                    {
                        actRenderableResource.Update(updateState);
                    }
                }
            }

            // Update all standard objects.
            foreach (var actLayer in m_sceneLayers)
            {
                actLayer.Update(updateState);
            }
        }

        /// <summary>
        /// Updates the scene (called beside rendering).
        /// </summary>
        internal void UpdateBesideRender(SceneRelatedUpdateState updateState)
        {
            // Invoke all async action attached to this scene
            Action actAsyncAction = null;

            while (m_asyncInvokesUpdateBesideRendering.Dequeue(out actAsyncAction))
            {
                actAsyncAction();
            }

            // Reset all filter flags before continue to next step
            foreach (var actView in m_registeredViews)
            {
                foreach (var actFilter in actView.Filters)
                {
                    actFilter.ConfigurationChanged = actFilter.ConfigurationChangedUI;
                    actFilter.ConfigurationChangedUI = false;
                }
            }

            // Performs update logic beside rendering
            foreach (var actLayer in m_sceneLayers)
            {
                actLayer.UpdateBesideRender(updateState);
            }
        }

        /// <summary>
        /// Performs some resource handling actions before rendering the frame.
        /// The main point here is executing Render on renderable resources.
        /// </summary>
        /// <param name="renderState">State of the render.</param>
        internal void HandleRenderResources(RenderState renderState)
        {
            //renderState.LastRenderBlockID = -1;

            // Get current resource dictionary
            var resources = m_registeredResourceDicts[renderState.DeviceIndex];

            if (resources == null)
            {
                throw new SeeingSharpGraphicsException("Unable to render scene: Resource dictionary for current device not found!");
            }

            // Unload all resources that are marked for unloading
            resources.UnloadAllMarkedResources();

            // Render all renderable resources first
            // (ensure here that we don't corrup device state)
            foreach (var actRenderableResource in resources.RenderableResources)
            {
                if (actRenderableResource.IsLoaded)
                {
                    actRenderableResource.Render(renderState);
                }
            }
        }

        /// <summary>
        /// Renders the scene to the given context.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        internal void Render(RenderState renderState)
        {
            // Get current resource dictionary
            var resources = m_registeredResourceDicts[renderState.DeviceIndex];

            if (resources == null)
            {
                throw new SeeingSharpGraphicsException("Unable to render scene: Resource dictionary for current device not found!");
            }

            // Apply default states on the device
            var defaultResource = resources.DefaultResources;
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            deviceContext.OutputMerger.BlendState = defaultResource.DefaultBlendState;
            deviceContext.OutputMerger.DepthStencilState = defaultResource.DepthStencilStateDefault;

            // Set initial rasterizer state
            if(renderState.ViewInformation.ViewConfiguration.WireframeEnabled)
            {
                deviceContext.Rasterizer.State = defaultResource.RasterStateWireframe;
            }
            else
            {
                deviceContext.Rasterizer.State = defaultResource.RasterStateDefault;
            }

            // Get or create RenderParamters object on scene level
            var renderParameters = m_renderParameters[renderState.DeviceIndex];

            if (renderParameters == null)
            {
                renderParameters = resources.GetResourceAndEnsureLoaded<SceneRenderParameters>(
                    KEY_SCENE_RENDER_PARAMETERS,
                    () => new SceneRenderParameters());
                m_renderParameters.AddObject(renderParameters, renderState.DeviceIndex);
            }

            // Update current view index
            renderState.ViewIndex = m_registeredViews.IndexOf(renderState.ViewInformation);

            // Update render parameters
            renderParameters.UpdateValues(renderState, m_perFrameData);

            using (renderState.PushScene(this, resources))
            {
                renderParameters.Apply(renderState);

                //Prepare rendering on each layer
                foreach (var actLayer in m_sceneLayers)
                {
                    actLayer.PrepareRendering(renderState);
                }

                //Render all layers in current order
                foreach (var actLayer in m_sceneLayers)
                {
                    if (actLayer.CountObjects > 0)
                    {
                        actLayer.Render(renderState);
                    }
                }
            }
        }

        /// <summary>
        /// Renders all 2D overlay components of the scene.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        internal void Render2DOverlay(RenderState renderState)
        {
            var graphics = renderState.Graphics2D;

            graphics.PushTransformSettings(new Graphics2DTransformSettings()
            {
                CustomTransform = CustomTransform2D,
                TransformMode = TransformMode2D,
                VirtualScreenSize = VirtualScreenSize2D
            });

            try
            {
                // Get current resource dictionary
                var resources = m_registeredResourceDicts[renderState.DeviceIndex];

                if (resources == null)
                {
                    throw new SeeingSharpGraphicsException("Unable to render scene: Resource dictionary for current device not found!");
                }

                // Start rendering
                using (renderState.PushScene(this, resources))
                {
                    //Render all layers in current order
                    foreach (var actLayer in m_sceneLayers)
                    {
                        if (actLayer.CountObjects > 0)
                        {
                            actLayer.Render2DOverlay(renderState);
                        }
                    }
                }

                // Render drawing layers
                foreach (var actDrawingLayer in m_drawing2DLayers)
                {
                    actDrawingLayer.Draw2DInternal(renderState.Graphics2D);
                }
            }
            finally
            {
                graphics.PopTransformSettings();
            }
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            //Unload resources of all scene objects
            foreach (var actLayer in m_sceneLayers)
            {
                actLayer.UnloadResources();
            }

            //Unload resources of all resources
            foreach (var actResourceDict in m_registeredResourceDicts)
            {
                actResourceDict.UnloadResources();
            }
        }

        /// <summary>
        /// Initializes this scene object.
        /// </summary>
        private void InitializeResourceDictionaries()
        {
            if (m_initialized) { return; }

            if (!GraphicsCore.IsLoaded)
            {
                return;
            }

            // Create all ResourceDictionary objects
            foreach (var actDevice in GraphicsCore.Current.LoadedDevices)
            {
                m_registeredResourceDicts.AddObject(
                    new ResourceDictionary(actDevice),
                    actDevice.DeviceIndex);
            }

            m_initialized = true;
        }

        /// <summary>
        /// Sort local layer list.
        /// </summary>
        private void SortLayers()
        {
            m_sceneLayers.Sort((left, right) => left.OrderID.CompareTo(right.OrderID));
        }

        /// <summary>
        /// Gets a collection containing all layers.
        /// </summary>
        internal ReadOnlyCollection<SceneLayer> Layers { get; }

        /// <summary>
        /// Gets total count of objects within the scene.
        /// </summary>
        public int CountObjects
        {
            get
            {
                int result = 0;

                foreach (var actLayer in m_sceneLayers)
                {
                    result += actLayer.CountObjects;
                }

                return result;
            }
        }

        public int CountAttachedComponents => m_sceneComponents.CountAttached;

        /// <summary>
        /// Gets total count of resources.
        /// </summary>
        public int CountResources
        {
            get
            {
                var firstResourceDict = m_registeredResourceDicts.FirstOrDefault();

                if (firstResourceDict != null)
                {
                    return firstResourceDict.Count;
                }
                else
                {
                    return 0;
                }
            }
        }

        /// <summary>
        /// Gets total count of layers.
        /// </summary>
        public int CountLayers => m_sceneLayers.Count;

        /// <summary>
        /// Gets the total count of view objects registered on this scene.
        /// </summary>
        public int CountViews => m_registeredViews.Count;

        /// <summary>
        /// Gets or sets a value indicating whether this scene is in pause mode.
        /// </summary>
        public bool IsPaused
        {
            get;
            set;
        }

        public Graphics2DTransformMode TransformMode2D { get; set; }

        public Size2F VirtualScreenSize2D { get; set; }

        public Matrix3x2 CustomTransform2D { get; set; }

        internal SceneRelatedUpdateState CachedUpdateState
        {
            get;
            private set;
        }

        /// <summary>
        /// Discard automatic scene unloading.
        /// (normally the scene gets cleared automatically after the last view
        /// gets deregistered).
        /// </summary>
        public bool DiscardAutomaticUnload
        {
            get;
            set;
        }
    }
}