using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using SeeingSharp.Checking;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public partial class Scene
    {
        // Constants
        public const string DEFAULT_LAYER_NAME = "Default";

        // Resource keys
        private NamedOrGenericKey _keySceneRenderParameters = GraphicsCore.GetNextGenericResourceKey();

        // Members for 2D rendering
        private List<Custom2DDrawingLayer> _drawing2DLayers;

        // Members for 3D rendering
        private CBPerFrame _perFrameData;

        // Some other logical parts of the scene object
        private SceneComponentFlyweight _sceneComponents;

        // Async update actions
        private ConcurrentQueue<Action> _asyncInvokesBeforeUpdate;
        private ConcurrentQueue<Action> _asyncInvokesUpdateBesideRendering;

        // Some runtime values
        private IndexBasedDynamicCollection<ResourceDictionary> _registeredResourceDicts;
        private IndexBasedDynamicCollection<ViewInformation> _registeredViews;
        private IndexBasedDynamicCollection<SceneRenderParameters> _renderParameters;

        // Misc
        private bool _initialized;
        private UnsafeList<SceneLayer> _sceneLayers;

        /// <summary>
        /// Gets total count of objects within the scene.
        /// </summary>
        public int CountObjects
        {
            get
            {
                var result = 0;

                foreach (var actLayer in _sceneLayers)
                {
                    result += actLayer.CountObjects;
                }

                return result;
            }
        }

        public int CountAttachedComponents => _sceneComponents.CountAttached;

        /// <summary>
        /// Gets total count of resources.
        /// </summary>
        public int CountResources
        {
            get
            {
                var firstResourceDict = _registeredResourceDicts.FirstOrDefault();

                if (firstResourceDict != null)
                {
                    return firstResourceDict.ResourceCount;
                }
                return 0;
            }
        }

        /// <summary>
        /// Gets total count of layers.
        /// </summary>
        public int CountLayers => _sceneLayers.Count;

        /// <summary>
        /// Gets the total count of view objects registered on this scene.
        /// </summary>
        public int CountViews => _registeredViews.Count;

        /// <summary>
        /// Gets or sets a value indicating whether this scene is in pause mode.
        /// </summary>
        public bool IsPaused
        {
            get;
            set;
        }

        public Graphics2DTransformMode TransformMode2D { get; set; }

        public SizeF VirtualScreenSize2D { get; set; }

        public Matrix3x2 CustomTransform2D { get; set; }

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

        /// <summary>
        /// Gets a collection containing all layers.
        /// </summary>
        internal ReadOnlyCollection<SceneLayer> Layers { get; }

        internal SceneRelatedUpdateState CachedUpdateState
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Scene" /> class.
        /// </summary>
        public Scene()
        {
            _perFrameData = new CBPerFrame();

            this.TransformMode2D = Graphics2DTransformMode.Custom;
            this.CustomTransform2D = Matrix3x2.Identity;
            this.VirtualScreenSize2D = new SizeF();

            _sceneComponents = new SceneComponentFlyweight(this);

            _sceneLayers = new UnsafeList<SceneLayer>();
            _sceneLayers.Add(new SceneLayer(DEFAULT_LAYER_NAME, this));
            this.Layers = new ReadOnlyCollection<SceneLayer>(_sceneLayers);

            _drawing2DLayers = new List<Custom2DDrawingLayer>();

            _asyncInvokesBeforeUpdate = new ConcurrentQueue<Action>();
            _asyncInvokesUpdateBesideRendering = new ConcurrentQueue<Action>();

            _registeredResourceDicts = new IndexBasedDynamicCollection<ResourceDictionary>();
            _registeredViews = new IndexBasedDynamicCollection<ViewInformation>();
            _renderParameters = new IndexBasedDynamicCollection<SceneRenderParameters>();

            this.CachedUpdateState = new SceneRelatedUpdateState();

            // Try to initialize this scene object
            this.InitializeResourceDictionaries();
        }

        /// <summary>
        /// Gets the total count of resources on the given device.
        /// </summary>
        /// <param name="device">The device for which to check.</param>
        public int GetResourceCount(EngineDevice device)
        {
            return _registeredResourceDicts[device.DeviceIndex]?.ResourceCount ?? 0;
        }

        /// <summary>
        /// Waits until the given object is visible on the given view.
        /// </summary>
        /// <param name="sceneObject">The scene object to be checked.</param>
        /// <param name="viewInfo">The view on which to check.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        public Task WaitUntilVisibleAsync(SceneObject sceneObject, ViewInformation viewInfo, CancellationToken cancelToken = default)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));
            sceneObject.EnsureNotNull(nameof(viewInfo));

            return this.WaitUntilVisibleAsync(
                new[] { sceneObject },
                viewInfo, cancelToken);
        }

        /// <summary>
        /// Gets a full list of <see cref="SceneObjectInfo"/> objects describing the contents of this scene.
        /// </summary>
        /// <param name="layer">Only return objects of this layer (empty means all layers).</param>
        public IEnumerable<SceneObjectInfo> GetSceneObjectInfos(string layer = "")
        {
            var layerArray = _sceneLayers.BackingArray;
            for (var loop = 0; loop < layerArray.Length; loop++)
            {
                var actLayer = layerArray[loop];
                if(actLayer == null) { continue; }

                if ((!string.IsNullOrEmpty(layer)) &&
                    (actLayer.Name != layer))
                {
                    continue;
                }

                var objectArrayInLayer = actLayer.ObjectsInternal.BackingArray;
                for (var loopObject = 0; loopObject < objectArrayInLayer.Length; loopObject++)
                {
                    var actObject = objectArrayInLayer[loopObject];
                    if(actObject == null){ continue; }
                    if(actObject.HasParent){ continue; }

                    yield return new SceneObjectInfo(actObject);
                }
            }
        }

        /// <summary>
        /// Waits until the given object is visible.
        /// </summary>
        /// <param name="sceneObjects">The scene objects to check for.</param>
        /// <param name="viewInfo">The view on which to check for visibility.</param>
        /// <param name="cancelToken">The cancellation token.</param>
        public Task WaitUntilVisibleAsync(IEnumerable<SceneObject> sceneObjects, ViewInformation viewInfo, CancellationToken cancelToken = default)
        {
            sceneObjects.EnsureNotNull(nameof(sceneObjects));
            viewInfo.EnsureNotNull(nameof(viewInfo));

            var taskComplSource = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);

            // Define the poll action (polling is done inside scene update
            void PollAction()
            {
                if (this.AreAllObjectsVisible(sceneObjects, viewInfo))
                {
                    taskComplSource.SetResult(null);
                }
                else if (cancelToken.IsCancellationRequested)
                {
                    taskComplSource.SetCanceled();
                }
                else
                {
                    _asyncInvokesBeforeUpdate.Enqueue(PollAction);
                }
            }

            // Register first call of the polling action
            _asyncInvokesBeforeUpdate.Enqueue(PollAction);

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
        public void AttachComponent(SceneComponentBase component, ViewInformation? sourceView = null)
        {
            _sceneComponents.AttachComponent(component, sourceView);
        }

        /// <summary>
        /// Detaches the given component from this scene.
        /// </summary>
        /// <param name="component">The component to be detached.</param>
        /// <param name="sourceView">The view which attached the component initially.</param>
        public void DetachComponent(SceneComponentBase component, ViewInformation? sourceView = null)
        {
            _sceneComponents.AttachComponent(component, sourceView);
        }

        /// <summary>
        /// Detaches all currently attached components.
        /// </summary>
        /// <param name="sourceView">The view from which we've to detach all components.</param>
        public void DetachAllComponents(ViewInformation? sourceView = null)
        {
            _sceneComponents.DetachAllComponents(sourceView);
        }

        /// <summary>
        /// Triggers scene manipulation using the given lambda action.
        /// The action gets processed directly before scene update process.
        ///
        /// Be careful: The action is called by worker-threads of SeeingSharp!
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
        /// Performs the given action before updating the scene.
        /// (given action gets called by update thread, no other actions on the scene during this time.)
        /// </summary>
        /// <param name="actionToInvoke">The action to be invoked.</param>
        public Task PerformBeforeUpdateAsync(Action actionToInvoke)
        {
            actionToInvoke.EnsureNotNull(nameof(actionToInvoke));

            var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _asyncInvokesBeforeUpdate.Enqueue(() =>
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

            var taskCompletionSource = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            _asyncInvokesUpdateBesideRendering.Enqueue(() =>
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
        /// Queries all loaded resources of the given resource key.
        /// </summary>
        internal IEnumerable<T> QueryResources<T>(NamedOrGenericKey resourceKey, Action checkValidAction)
            where T : Resource
        {
            checkValidAction.Invoke();

            foreach (var actResourceDict in _registeredResourceDicts)
            {
                if(!actResourceDict.ContainsResource(resourceKey)){ continue; }

                yield return actResourceDict.GetResource<T>(resourceKey);

                checkValidAction?.Invoke();
            }
        }

        /// <summary>
        /// Adds a resource to the scene
        /// </summary>
        /// <typeparam name="TResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The factory method which creates the resource object.</param>
        /// <param name="resourceKey">The key for the newly generated resource.</param>
        internal NamedOrGenericKey AddResource<TResourceType>(Func<EngineDevice, TResourceType> resourceFactory, NamedOrGenericKey resourceKey)
            where TResourceType : Resource
        {
            resourceFactory.EnsureNotNull(nameof(resourceFactory));

            this.InitializeResourceDictionaries();

            if (resourceKey == NamedOrGenericKey.Empty)
            {
                resourceKey = GraphicsCore.GetNextGenericResourceKey();
            }

            foreach (var actResourceDict in _registeredResourceDicts)
            {
                actResourceDict.AddResource(resourceKey, resourceFactory(actResourceDict.Device));
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

            foreach (var actResourceDict in _registeredResourceDicts)
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
        /// <typeparam name="TResourceType">The type of the resource.</typeparam>
        /// <param name="manipulateAction">The action that manipulates the resource.</param>
        /// <param name="resourceKey">The key of the resource to be manipulated.</param>
        internal void ManipulateResource<TResourceType>(Action<TResourceType> manipulateAction, NamedOrGenericKey resourceKey)
            where TResourceType : Resource
        {
            manipulateAction.EnsureNotNull(nameof(manipulateAction));

            this.InitializeResourceDictionaries();

            foreach (var actResourceDict in _registeredResourceDicts)
            {
                var actResource = actResourceDict.GetResource<TResourceType>(resourceKey);

                if (actResource == null)
                {
                    throw new SeeingSharpGraphicsException("Resource " + resourceKey + " of type " + typeof(TResourceType).FullName + " not found on device " + actResourceDict.Device.AdapterDescription + "!");
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
            this.InitializeResourceDictionaries();

            foreach (var actResourceDict in _registeredResourceDicts)
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
            var pickedObjects = new List<Tuple<SceneObject, float>>();

            foreach (var actLayer in _sceneLayers)
            {
                foreach (var actObject in actLayer.Objects)
                {
                    if (!actObject.IsVisible(viewInformation)) { continue; }
                    if (!actObject.IsPickingTestVisible) { continue; }

                    var actDistance = actObject.Pick(rayStart, rayDirection, viewInformation, pickingOptions);

                    if (!float.IsNaN(actDistance))
                    {
                        pickedObjects.Add(Tuple.Create(actObject, actDistance));
                    }
                }
            }

            // Return all picked object in correct order
            return pickedObjects
                .OrderBy(actObject => actObject.Item2)
                .Convert(actObject => actObject.Item1)
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

            if (!_drawing2DLayers.Contains(drawingLayer))
            {
                _drawing2DLayers.Add(drawingLayer);
            }
        }

        /// <summary>
        /// Removes the given drawing layer.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        internal void RemoveDrawingLayer(Custom2DDrawingLayer drawingLayer)
        {
            drawingLayer.EnsureNotNull(nameof(drawingLayer));

            while (_drawing2DLayers.Remove(drawingLayer)) { }
        }

        /// <summary>
        /// Adds the given object to the scene.
        /// </summary>
        /// <param name="sceneObject">Object to add.</param>
        /// <param name="layer">Layer on which the object should be added.</param>
        internal void Add<T>(T sceneObject, string layer)
            where T : SceneObject
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));
            layer.EnsureNotNullOrEmpty(nameof(layer));

            this.InitializeResourceDictionaries();

            var layerObject = this.GetLayer(layer);
            layerObject.AddObject(sceneObject);
        }

        /// <summary>
        /// Registers the given view on this scene object.
        /// This method is meant to be called by RenderLoop class.
        /// </summary>
        /// <param name="viewInformation">The view to register.</param>
        internal void RegisterView(ViewInformation viewInformation)
        {
            viewInformation.EnsureNotNull(nameof(viewInformation));

            var isFirstView = _registeredViews.Count == 0;

            this.InitializeResourceDictionaries();

            // Register device on this scene if not done before
            //  -> This registration is forever, no deregister is made!
            var givenDevice = viewInformation.Device;

            var resourceDictionary = _registeredResourceDicts[givenDevice!.DeviceIndex];
            if (resourceDictionary == null)
            {
                throw new SeeingSharpGraphicsException("ResourceDictionary of device " + givenDevice.AdapterDescription + " not loaded in this scene!");
            }

            // Check for already done registration of this view
            // If there is any, then caller made an error
            if (_registeredViews.Contains(viewInformation))
            {
                throw new SeeingSharpGraphicsException("The given view is already registered on this scene!");
            }

            // Register this view on this scene and on all layers
            var viewIndex = _registeredViews.AddObject(viewInformation);

            foreach (var actLayer in _sceneLayers)
            {
                actLayer.RegisterView(viewIndex, viewInformation, resourceDictionary);
            }

            viewInformation.ViewIndex = viewIndex;

            // Mark this scene for deletion if we don't have any other view registered
            if (isFirstView)
            {
                GraphicsCore.Current.MainLoop!.DeregisterSceneForUnload(this);
            }
        }

        /// <summary>
        /// Is the given view registered on this scene?
        /// </summary>
        /// <param name="viewInformation">The view to check for.</param>
        internal bool IsViewRegistered(ViewInformation viewInformation)
        {
            viewInformation.EnsureNotNull(nameof(viewInformation));

            return _registeredViews.Contains(viewInformation);
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
            if (!_registeredViews.Contains(viewInformation))
            {
                throw new SeeingSharpGraphicsException("The given view is not registered on this scene!");
            }

            // Deregister the view on this scene and all layers
            var viewIndex = _registeredViews.IndexOf(viewInformation);
            _registeredViews.RemoveObject(viewInformation);

            foreach (var actLayer in _sceneLayers)
            {
                actLayer.DeregisterView(viewIndex, viewInformation);
            }

            // Clear view index
            viewInformation.ViewIndex = -1;

            // Mark this scene for deletion if we don't have any other view registered
            if (_registeredViews.Count <= 0 &&
                !this.DiscardAutomaticUnload)
            {
                GraphicsCore.Current.MainLoop!.RegisterSceneForUnload(this);
            }
        }

        /// <summary>
        /// Adds a new layer with the given name.
        /// </summary>
        /// <param name="name">Name of the layer.</param>
        internal SceneLayer AddLayer(string name)
        {
            name.EnsureNotNullOrEmpty(nameof(name));

            var currentLayer = this.TryGetLayer(name);
            if (currentLayer != null)
            {
                throw new ArgumentException("There is already a SceneLayer with the given name!", nameof(name));
            }

            // Create the new layer
            var newLayer = new SceneLayer(name, this)
            {
                OrderId = _sceneLayers.Max(actLayer => actLayer.OrderId) + 1
            };

            _sceneLayers.Add(newLayer);

            // Sort local layer list
            this.SortLayers();

            // Register all views on the newly generated layer
            foreach (var actViewInfo in _registeredViews)
            {
                var resourceDictionary = _registeredResourceDicts[actViewInfo.Device!.DeviceIndex];
                if (resourceDictionary == null)
                {
                    throw new SeeingSharpGraphicsException("ResourceDictionary of device " + actViewInfo.Device.AdapterDescription + " not loaded in this scene!");
                }

                newLayer.RegisterView(
                    _registeredViews.IndexOf(actViewInfo),
                    actViewInfo,
                    resourceDictionary);
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

            var layerToRemove = this.TryGetLayer(layerName);

            if (layerToRemove != null)
            {
                this.RemoveLayer(layerToRemove);
            }
        }

        /// <summary>
        /// Sets the order id of the given layer.
        /// </summary>
        /// <param name="layer">The layer to modify.</param>
        /// <param name="newOrderId">the new order id.</param>
        internal void SetLayerOrderId(SceneLayer layer, int newOrderId)
        {
            layer.EnsureNotNull(nameof(layer));

            if (!_sceneLayers.Contains(layer)) { throw new ArgumentException("This scene does not contain the given layer!"); }

            // Change the order id
            layer.OrderId = newOrderId;

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

            if (layer == null) { throw new ArgumentNullException(nameof(layer)); }
            if (layer.Scene != this) { throw new ArgumentException("Given layer does not belong to this scene!", nameof(layer)); }
            if (layer.Name == DEFAULT_LAYER_NAME) { throw new ArgumentNullException(nameof(layer), "Unable to remove the default layer!"); }

            layer.UnloadResources();
            _sceneLayers.Remove(layer);

            // Sort local layer list
            this.SortLayers();
        }

        /// <summary>
        /// Clears the layer with the given name.
        /// </summary>
        /// <param name="layerName">The name of the layer.</param>
        internal void ClearLayer(string layerName)
        {
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            var layerToClear = this.GetLayer(layerName);
            this.ClearLayer(layerToClear);
        }

        /// <summary>
        /// Clears the given layer.
        /// </summary>
        /// <param name="layer">The layer to be cleared.</param>
        internal void ClearLayer(SceneLayer layer)
        {
            layer.EnsureNotNull(nameof(layer));

            if (layer == null) { throw new ArgumentNullException(nameof(layer)); }
            if (layer.Scene != this) { throw new ArgumentException("Given layer does not belong to this scene!", nameof(layer)); }

            layer.ClearObjects();
        }

        /// <summary>
        /// Gets the layer with the given name.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        internal SceneLayer? TryGetLayer(string layerName)
        {
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            if (string.IsNullOrEmpty(layerName))
            {
                throw new ArgumentException("Given layer name is not valid!", nameof(layerName));
            }

            foreach (var actLayer in _sceneLayers)
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

            var result = this.TryGetLayer(layerName);

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
            for (var loop = 0; loop < _sceneLayers.Count; loop++)
            {
                var actLayer = _sceneLayers[loop];
                actLayer.ClearObjects();

                if (actLayer.Name != DEFAULT_LAYER_NAME)
                {
                    _sceneLayers.RemoveAt(loop);
                    loop--;
                }
            }

            // Clears all 2D drawing layers
            _drawing2DLayers.Clear();

            // Clear all resources
            if (clearResources)
            {
                foreach (var actDictionary in _registeredResourceDicts)
                {
                    actDictionary.Clear();
                }

                _renderParameters.Clear();

                for (var loop = 0; loop < _sceneLayers.Count; loop++)
                {
                    var actLayer = _sceneLayers[loop];
                    actLayer.ClearResources();
                }
            }
        }

        internal void RemoveRange(IEnumerable<SceneObject> sceneObjects)
        {
            sceneObjects.EnsureNotNull(nameof(sceneObjects));

            foreach (var actObject in sceneObjects)
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

            foreach (var actLayer in _sceneLayers)
            {
                actLayer.RemoveObject(sceneObject);
            }
        }

        /// <summary>
        /// Removes the given object from the scene.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        /// <param name="layerName">Layer on which the scene object was added.</param>
        internal void Remove(SceneObject sceneObject, string layerName)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));
            layerName.EnsureNotNullOrEmpty(nameof(layerName));

            var layerObject = this.GetLayer(layerName);
            layerObject.RemoveObject(sceneObject);
        }

        /// <summary>
        /// Updates the scene.
        /// </summary>
        internal void Update(SceneRelatedUpdateState updateState)
        {
            _perFrameData.Time = _perFrameData.Time + updateState.UpdateTimeMilliseconds;
            if (_perFrameData.Time > SeeingSharpConstants.MAX_PER_FRAME_TIME_VALUE)
            {
                _perFrameData.Time = _perFrameData.Time % SeeingSharpConstants.MAX_PER_FRAME_TIME_VALUE;
            }

            // Update all scene components first
            //  These may trigger some further manipulation actions
            _sceneComponents.UpdateSceneComponents(updateState);

            // Invoke all async action attached to this scene
            var asyncActionsBeforeUpdateCount = _asyncInvokesBeforeUpdate.Count;
            if (asyncActionsBeforeUpdateCount > 0)
            {
                var actIndex = 0;
                while (actIndex < asyncActionsBeforeUpdateCount &&
                       _asyncInvokesBeforeUpdate.TryDequeue(out var actAsyncAction))
                {
                    actAsyncAction();
                    actIndex++;
                }
            }

            // Render all renderable resources
            foreach (var actResourceDict in _registeredResourceDicts)
            {
                foreach (var actRenderableResource in actResourceDict.UpdatableResources)
                {
                    actRenderableResource.Update(updateState.UpdateState);
                }
            }

            // Update all standard objects.
            foreach (var actLayer in _sceneLayers)
            {
                actLayer.Update(updateState);
            }

            // Update overlays
            foreach (var actOverlay in _drawing2DLayers)
            {
                actOverlay.UpdateInternal(updateState.UpdateState);
            }
        }

        /// <summary>
        /// Updates states of object filters. This is done before UpdateBesideRender for all scenes synchronously.
        /// </summary>
        internal void UpdateObjectFilterState(UpdateState updateState)
        {
            // Reset all filter flags before continue to next step
            foreach (var actView in _registeredViews)
            {
                foreach (var actFilter in actView.FiltersInternal)
                {
                    // Check whether we've done this already on another scene/view on the same pass
                    if(actFilter.ConfigurationCheckedCycleID == updateState.MainLoopCycleId){ continue; }

                    actFilter.ConfigurationCheckedCycleID = updateState.MainLoopCycleId;
                    actFilter.ConfigurationChangedInternal = actFilter.ConfigurationChanged;
                    actFilter.ConfigurationChanged = false;
                }
            }
        }

        /// <summary>
        /// Updates the scene (called beside rendering).
        /// </summary>
        internal void UpdateBesideRender(SceneRelatedUpdateState updateState)
        {
            // Invoke all async action attached to this scene
            var prevCount = _asyncInvokesUpdateBesideRendering.Count;
            var actIndex = 0;
            while (actIndex < prevCount &&
                   _asyncInvokesUpdateBesideRendering.TryDequeue(out var actAsyncAction))
            {
                actAsyncAction();
                actIndex++;
            }

            // Performs update logic beside rendering
            foreach (var actLayer in _sceneLayers)
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
            // Get current resource dictionary
            var resources = _registeredResourceDicts[renderState.DeviceIndex];

            if (resources == null)
            {
                throw new SeeingSharpGraphicsException("Unable to render scene: Resource dictionary for current device not found!");
            }

            // Unload all resources that are marked for unloading
            resources.UnloadAllMarkedResources();

            // Render all renderable resources first
            // (ensure here that we don't corrupt device state)
            var currentDevice = renderState.Device;
            foreach (var actRenderableResource in resources.RenderableResources)
            {
                if (actRenderableResource.IsLoaded &&
                    !currentDevice.IsLost)
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
            var resources = _registeredResourceDicts[renderState.DeviceIndex];
            if (resources == null)
            {
                throw new SeeingSharpGraphicsException("Unable to render scene: Resource dictionary for current device not found!");
            }

            // Handle after device lost
            resources.ReloadAfterReloadedDevice();

            // Apply default states on the device
            var defaultResource = resources.DefaultResources;
            var deviceContext = renderState.Device.DeviceImmediateContextD3D11;
            deviceContext.OMSetBlendState(defaultResource.DefaultBlendState);
            deviceContext.OMSetDepthStencilState(defaultResource.DepthStencilStateDefault);

            // Set initial rasterizer state
            if (renderState.ViewInformation!.ViewConfiguration.WireframeEnabled)
            {
                deviceContext.RSSetState(defaultResource.RasterStateWireframe);
            }
            else
            {
                deviceContext.RSSetState(defaultResource.RasterStateDefault);
            }

            // Get or create RenderParameters object on scene level
            var renderParameters = _renderParameters[renderState.DeviceIndex];

            if (renderParameters == null)
            {
                renderParameters = resources.GetResourceAndEnsureLoaded(
                    _keySceneRenderParameters,
                    () => new SceneRenderParameters());
                _renderParameters.AddObject(renderParameters, renderState.DeviceIndex);
            }

            // Update current view index
            renderState.ViewIndex = _registeredViews.IndexOf(renderState.ViewInformation);

            // Update render parameters
            renderParameters.UpdateValues(renderState, _perFrameData);

            renderState.PushScene(this, resources);
            try
            {
                renderParameters.Apply(renderState);

                //Prepare rendering on each layer
                foreach (var actLayer in _sceneLayers)
                {
                    actLayer.PrepareRendering(renderState);
                }

                //Render all layers in current order
                foreach (var actLayer in _sceneLayers)
                {
                    if (actLayer.CountObjects > 0)
                    {
                        actLayer.Render(renderState);
                    }
                }
            }
            finally
            {
                renderState.PopScene();
            }
        }

        /// <summary>
        /// Renders all 2D overlay components of the scene.
        /// </summary>
        /// <param name="renderState">The current render state.</param>
        internal void Render2DOverlay(RenderState renderState)
        {
            var graphics = renderState.Graphics2D!;

            graphics.PushTransformSettings(new Graphics2DTransformSettings
            {
                CustomTransform = this.CustomTransform2D,
                TransformMode = this.TransformMode2D,
                VirtualScreenSize = this.VirtualScreenSize2D
            });

            try
            {
                // Get current resource dictionary
                var resources = _registeredResourceDicts[renderState.DeviceIndex];
                if (resources == null)
                {
                    throw new SeeingSharpGraphicsException("Unable to render scene: Resource dictionary for current device not found!");
                }

                // Start rendering
                renderState.PushScene(this, resources);
                try
                {
                    //Render all layers in current order
                    foreach (var actLayer in _sceneLayers)
                    {
                        if (actLayer.CountObjects > 0)
                        {
                            actLayer.Render2DOverlay(renderState);
                        }
                    }
                }
                finally
                {
                    renderState.PopScene();
                }

                // Render drawing layers
                foreach (var actDrawingLayer in _drawing2DLayers)
                {
                    actDrawingLayer.Draw2DInternal(renderState.Graphics2D!);
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
            foreach (var actLayer in _sceneLayers)
            {
                actLayer.UnloadResources();
            }

            //Unload resources of all resources
            foreach (var actResourceDict in _registeredResourceDicts)
            {
                actResourceDict.UnloadResources();
            }
        }

        /// <summary>
        /// Initializes this scene object.
        /// </summary>
        private void InitializeResourceDictionaries()
        {
            if (_initialized) { return; }

            if (!GraphicsCore.IsLoaded)
            {
                return;
            }

            // Create all ResourceDictionary objects
            foreach (var actDevice in GraphicsCore.Current.LoadedDevices)
            {
                _registeredResourceDicts.AddObject(
                    new ResourceDictionary(actDevice),
                    actDevice.DeviceIndex);
            }

            _initialized = true;
        }

        /// <summary>
        /// Sort local layer list.
        /// </summary>
        private void SortLayers()
        {
            _sceneLayers.Sort((left, right) => (left?.OrderId ?? 0).CompareTo((right?.OrderId ?? 0)));
        }
    }
}