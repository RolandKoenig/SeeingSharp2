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
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Numerics;
using D3D = SharpDX.Direct3D;

namespace SeeingSharp.Multimedia.Core
{
    public class ViewRelatedSceneLayerSubset : IDisposable
    {
        private const int DEFAULT_PASS_SUBSCRIPTION_LENGTH = 1024;

        // State members
        private bool _disposed;

        // Temporary collections
        private List<Tuple<SceneObject, bool, bool>> _tmpChangedVisibilities;

        // Configuration member
        private Scene _scene;
        private SceneLayer _sceneLayer;
        private EngineDevice _device;
        private ResourceDictionary _resources;

        // Special members for subscribe/unsubscribe pass logic
        private bool _isSubscribeUnsubscribeAllowed;
        private Action _changedVisibilitiesAction;

        // Objects that raises exceptions during render
        private Dictionary<SceneObject, object> _invalidObjects;
        private Queue<SceneObject> _invalidObjectsToDeregister;

        // Resources for rendering
        private RenderPassLineRender _renderPassLineRender;
        private RenderPassDefaultTransparent _renderPassTransparent;
        private RenderPass2DOverlay _renderPass2DOverlay;
        private ViewRenderParameters _renderParameters;

        // Subscription collections
        // All collections needed to link all scene objects to corresponding render passes
        // => This collections are updated using UpdateForView logic
        private Dictionary<RenderPassInfo, PassSubscriptionProperties> _objectsPerPassDict;
        private List<PassSubscriptionProperties> _objectsPerPass;
        private PassSubscriptionProperties _objectsPassPlainRender;
        private PassSubscriptionProperties _objectsPassLineRender;
        private PassSubscriptionProperties _objectsPassTransparentRender;
        private PassSubscriptionProperties _objectsPassSpriteBatchRender;
        private PassSubscriptionProperties _objectsPass2DOverlay;
        private bool _anythingUnsubscribed;

        /// <summary>
        /// Initializes a new instance of the <see cref="ViewRelatedSceneLayerSubset" /> class.
        /// </summary>
        internal ViewRelatedSceneLayerSubset(SceneLayer sceneLayer, ViewInformation viewInformation, ResourceDictionary resources, int viewIndex)
        {
            _scene = sceneLayer.Scene;
            _sceneLayer = sceneLayer;
            this.ViewInformation = viewInformation;
            _device = this.ViewInformation.Device;
            _resources = resources;
            ViewIndex = viewIndex;

            _invalidObjects = new Dictionary<SceneObject, object>();
            _invalidObjectsToDeregister = new Queue<SceneObject>();

            // Create temporary collections
            _tmpChangedVisibilities = new List<Tuple<SceneObject, bool, bool>>();

            // Create all specialized render pass lists
            _objectsPassPlainRender = new PassSubscriptionProperties();
            _objectsPassLineRender = new PassSubscriptionProperties();
            _objectsPassTransparentRender = new PassSubscriptionProperties();
            _objectsPassSpriteBatchRender = new PassSubscriptionProperties();
            _objectsPass2DOverlay = new PassSubscriptionProperties();

            // Create dictionary for fast access to all render pass list
            _objectsPerPassDict = new Dictionary<RenderPassInfo, PassSubscriptionProperties>
            {
                [RenderPassInfo.PASS_PLAIN_RENDER] = _objectsPassPlainRender,
                [RenderPassInfo.PASS_LINE_RENDER] = _objectsPassLineRender,
                [RenderPassInfo.PASS_TRANSPARENT_RENDER] = _objectsPassTransparentRender,
                [RenderPassInfo.PASS_SPRITE_BATCH] = _objectsPassSpriteBatchRender,
                [RenderPassInfo.PASS_2D_OVERLAY] = _objectsPass2DOverlay
            };

            _objectsPerPass = new List<PassSubscriptionProperties>(_objectsPerPassDict.Values);

            _anythingUnsubscribed = false;

            // Create and load all render pass relevant resources
            this.RefreshDeviceDependentResources();
        }

        public void Dispose()
        {
            if (_disposed) { return; }

            _renderParameters = null;
            _renderPassLineRender = null;
            _renderPassTransparent = null;

            _disposed = true;
        }

        /// <summary>
        /// Registers the given object on this view subset.
        /// </summary>
        /// <param name="sceneObject">The scene object to be registered.</param>
        internal void RegisterObject(SceneObject sceneObject)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ViewRelatedLayerSubset");
            }

            if (_invalidObjects.ContainsKey(sceneObject))
            {
                return;
            }

            if ((sceneObject.TargetDetailLevel & _device.SupportedDetailLevel) == _device.SupportedDetailLevel)
            {
                sceneObject.RegisterLayerViewSubset(this);
            }
        }

        /// <summary>
        /// Registers the given collection of objects on this view subset.
        /// </summary>
        /// <param name="sceneObjects">The scene objects to be registered.</param>
        internal void RegisterObjectRange(UnsafeList<SceneObject> sceneObjects)
        {
            if (_disposed)
            {
                throw new ObjectDisposedException("ViewRelatedLayerSubset");
            }

            var length = sceneObjects.Count;
            var backingArray = sceneObjects.BackingArray;
            for (var loop = 0; loop < length; loop++)
            {
                var actSceneObject = backingArray[loop];
                if(actSceneObject == null){ continue; }

                if (_invalidObjects.ContainsKey(actSceneObject))
                {
                    continue;
                }

                if ((actSceneObject.TargetDetailLevel & _device.SupportedDetailLevel) == _device.SupportedDetailLevel)
                {
                    actSceneObject.RegisterLayerViewSubset(this);
                }
            }
        }

        /// <summary>
        /// Clears all resources created by this view subset.
        /// </summary>
        internal void ClearResources()
        {
            _renderParameters = null;
            _renderPass2DOverlay = null;
            _renderPassLineRender = null;
            _renderPassTransparent = null;
        }

        /// <summary>
        /// Deregisters the given object form this view subset.
        /// </summary>
        /// <param name="sceneObject">The object to be removed.</param>
        internal void DeregisterObject(SceneObject sceneObject)
        {
            if (_disposed) { throw new ObjectDisposedException("ViewRelatedLayerSubset"); }

            // Perform unsubscription here (allowed because this call comes from update process)
            _isSubscribeUnsubscribeAllowed = true;
            try
            {
                if (sceneObject.IsLayerViewSubsetRegistered(ViewIndex))
                {
                    sceneObject.UnsubsribeFromAllPasses(this);
                    sceneObject.DeregisterLayerViewSubset(this);
                }
            }
            finally
            {
                _isSubscribeUnsubscribeAllowed = false;
            }
        }

        /// <summary>
        /// Clears all subscriptions
        /// </summary>
        /// <param name="allObjects">A collection containing all objects of the current layer.</param>
        internal void ClearAllSubscriptions(UnsafeList<SceneObject> allObjects)
        {
            if (_disposed) { throw new ObjectDisposedException("ViewRelatedLayerSubset"); }

            // Clear all subscriptions on the SceneObject instances
            var length = allObjects.Count;
            for (var loop = 0; loop < length; loop++)
            {
                allObjects[loop].ClearSubscriptionsWithoutUnsubscribeCall(this);
                allObjects[loop].DeregisterLayerViewSubset(this);
            }

            // Clear local subscription information
            foreach (var actSubscriptionList in _objectsPerPassDict.Values)
            {
                actSubscriptionList.Subscriptions.Clear();
            }
        }

        /// <summary>
        /// Executes view update using the given update state object.
        /// </summary>
        /// <param name="updateState">The update state.</param>
        internal void UpdateForView(SceneRelatedUpdateState updateState)
        {
            if (_disposed) { throw new ObjectDisposedException("ViewRelatedLayerSubset"); }

            var anyOrderChanges = false;
            var camera = this.ViewInformation.Camera;
            _objectsPassTransparentRender.Subscriptions.Sort((left, right) =>
            {
                var leftSpacial = left.SceneObject as SceneSpacialObject;
                var rightSpacial = right.SceneObject as SceneSpacialObject;

                if (leftSpacial != null && rightSpacial != null)
                {
                    var leftDistance = (camera.Position - leftSpacial.Position).LengthSquared();
                    var rightDistance = (camera.Position - rightSpacial.Position).LengthSquared();
                    anyOrderChanges = true;
                    return rightDistance.CompareTo(leftDistance);
                }
                if (leftSpacial != null)
                {
                    anyOrderChanges = true; return -1;
                }
                if (rightSpacial != null)
                {
                    anyOrderChanges = true; return 1;
                }
                {
                    return 0;
                }
            });

            if (anyOrderChanges)
            {
                // Synchronize ordering changes with corresponding scene object
                for (var loop = 0; loop < _objectsPassTransparentRender.Subscriptions.Count; loop++)
                {
                    var actSubscription = _objectsPassTransparentRender.Subscriptions[loop];
                    actSubscription.SubscriptionIndex = loop;
                    actSubscription.SceneObject.UpdateSubscription(actSubscription, this);
                    _objectsPassTransparentRender.Subscriptions[loop] = actSubscription;
                }
            }

            // Update all objects related to this view
            _isSubscribeUnsubscribeAllowed = true;

            try
            {
                // Update subscriptions based on visibility check result
                if (_changedVisibilitiesAction != null)
                {
                    _changedVisibilitiesAction();
                    _changedVisibilitiesAction = null;
                }

                // Unsubscribe all invalid objects
                while (_invalidObjectsToDeregister.Count > 0)
                {
                    var actInvalidObject = _invalidObjectsToDeregister.Dequeue();
                    actInvalidObject.UnsubsribeFromAllPasses(this);
                }

                // Update subscriptions based on object state
                var allObjects = _sceneLayer.ObjectsInternal;
                var allObjectsLength = allObjects.Count;
                var visibleObjectCount = this.ViewInformation.Owner.VisibleObjectCountInternal;
                for (var loop = 0; loop < allObjectsLength; loop++)
                {
                    var actSceneObject = allObjects[loop];

                    if (_invalidObjects.ContainsKey(actSceneObject)) { continue; }

                    if (actSceneObject.IsLayerViewSubsetRegistered(ViewIndex) &&
                        actSceneObject.IsVisible(this.ViewInformation))
                    {
                        actSceneObject.UpdateForView(updateState, this);
                        visibleObjectCount++;
                    }
                }
                this.ViewInformation.Owner.VisibleObjectCountInternal = visibleObjectCount;
            }
            finally
            {
                _isSubscribeUnsubscribeAllowed = false;
            }

            // Reorganize subscriptions if there is anything unsubscribed
            if (_anythingUnsubscribed)
            {
                _anythingUnsubscribed = false;
                foreach (var actPassProperties in _objectsPerPass)
                {
                    if (actPassProperties.UnsubscribeCallCount <= 0) { continue; }

                    // Variables for consistency checking
                    var givenUnsubscribeCount = actPassProperties.UnsubscribeCallCount;
                    var trueUnsubscribeCount = 0;

                    // Handle case where we have unsubscribed some
                    //  => Build new subscription list and ignore all with 'IsSubscribed' == false
                    var newSubscriptionList = actPassProperties.SubscriptionsTemp;
                    for (var loop = 0; loop < actPassProperties.Subscriptions.Count; loop++)
                    {
                        var actSubscription = actPassProperties.Subscriptions[loop];

                        if (!actSubscription.IsSubscribed)
                        {
                            actSubscription.SceneObject.ClearSubscriptionsWithoutUnsubscribeCall(this, actSubscription);
                            trueUnsubscribeCount++;
                            continue;
                        }

                        // AddObject this item to new subscription list
                        actSubscription.SubscriptionIndex = newSubscriptionList.Count;
                        newSubscriptionList.Add(actSubscription);

                        actSubscription.SceneObject.UpdateSubscription(actSubscription, this);
                    }
                    actPassProperties.SubscriptionsTemp = actPassProperties.Subscriptions;
                    actPassProperties.SubscriptionsTemp.Clear();
                    actPassProperties.Subscriptions = newSubscriptionList;
                    actPassProperties.UnsubscribeCallCount = 0;

                    // Check for consistency: Does unsubscribe-count match true unsubscriptions using IsSubscribed flag
                    if (givenUnsubscribeCount != trueUnsubscribeCount)
                    {
                        throw new SeeingSharpException("Inconsistency: Given unsubscribe count does not mach true count of unsubscriptions!");
                    }
                }
            }
        }

        /// <summary>
        /// Update logic beside rendering.
        /// </summary>
        /// <param name="updateState">The update state.</param>
        /// <param name="sceneObjectsForSingleUpdateCall">A collection of scene objects for a single update call. These are normally a list of newly inserted static objects.</param>
        internal void UpdateBesideRender(SceneRelatedUpdateState updateState, Queue<SceneObject> sceneObjectsForSingleUpdateCall)
        {
            var filters = this.ViewInformation.FiltersInternal;
            _tmpChangedVisibilities.Clear();

            // Perform some pre-logic on filters
            var anyFilterChanged = false;

            foreach (var actFilter in filters)
            {
                actFilter.SetEnvironmentData(_sceneLayer, this.ViewInformation);

                if (actFilter.ConfigurationChanged)
                {
                    anyFilterChanged = true;
                }
            }

            // Check whether we have to update all objects
            var refreshAllObjects = this.ViewInformation.Camera.StateChanged || anyFilterChanged;

            // Perform viewbox culling for all standard objects
            var allObjects = _sceneLayer.ObjectsInternal;
            var allObjectsLength = allObjects.Count;
            if (allObjectsLength > 0)
            {
                var allObjectsArray = allObjects.BackingArray;
                for (var loop = 0; loop < allObjectsLength; loop++)
                {
                    var actObject = allObjectsArray[loop];

                    // Don't handle static objects here if we don't want to handle them
                    if (!refreshAllObjects)
                    {
                        if (actObject.IsStatic) { continue; }
                        if (!actObject.TransformationChanged) { continue; }
                    }

                    // Perform culling
                    this.PerformViewboxCulling(actObject, filters);
                }
            }

            // Update objects which are passed for a single update call (normally newly inserted static objects)
            var singleUpdateCallCount = sceneObjectsForSingleUpdateCall.Count;
            if (!refreshAllObjects && singleUpdateCallCount > 0)
            {
                var singleUpdateArray = sceneObjectsForSingleUpdateCall.GetBackingArray();
                for (var loop = 0; loop < singleUpdateCallCount; loop++)
                {
                    // Perform culling
                    this.PerformViewboxCulling(singleUpdateArray[loop], filters);
                }
            }

            // Handle changed visibility in standard update logic
            if (_tmpChangedVisibilities.Count > 0)
            {
                var startingScene = _scene;

                _changedVisibilitiesAction = () =>
                {
                    if (_disposed) { return; }
                    if (startingScene != _scene) { return; }

                    foreach (var actChangedVisibility in _tmpChangedVisibilities)
                    {
                        // Check whether this object is still here..
                        if (actChangedVisibility.Item1.Scene != _scene) { continue; }
                        if (actChangedVisibility.Item1.SceneLayer != _sceneLayer) { continue; }

                        // Handle changed visibility
                        this.HandleObjectVisibilityChanged(
                            actChangedVisibility.Item1,
                            actChangedVisibility.Item3);
                    }
                };
            }
        }

        /// <summary>
        /// Renders this view subset.
        /// </summary>
        /// <param name="renderState">The RenderState object holding all relevant data for current render pass</param>
        internal void Render(RenderState renderState)
        {
            if (_disposed) { throw new ObjectDisposedException("ViewRelatedLayerSubset"); }

            // Skip rendering if there is nothing to do..
            if (_objectsPassLineRender.Subscriptions.Count == 0 &&
               _objectsPassPlainRender.Subscriptions.Count == 0 &&
               _objectsPassSpriteBatchRender.Subscriptions.Count == 0 &&
               _objectsPassTransparentRender.Subscriptions.Count == 0)
            {
                return;
            }

            List<SceneObject> invalidObjects = null;
            var resources = renderState.CurrentResources;

            if (renderState.Device != _device) { throw new SeeingSharpGraphicsException("Rendering of a ViewRelatedSceneLayoutSubset is called with a wrong device object!"); }
            if (renderState.CurrentResources != _resources) { throw new SeeingSharpGraphicsException("Rendering of a ViewRelatedSceneLayoutSubset is called with a wrong ResourceDictionary object!"); }

            // Get current view configuration
            var viewConfiguration = this.ViewInformation.ViewConfiguration;

            // Update device dependent resources here
            this.RefreshDeviceDependentResources();

            // Update render parameters
            var cbPerView = new CBPerView
            {
                Accentuation = viewConfiguration.AccentuationFactor,
                BorderFactor = viewConfiguration.GeneratedBorderFactor,
                Ambient = viewConfiguration.AmbientFactor,
                CameraPosition = this.ViewInformation.Camera.Position,
                ScreenPixelSize = this.ViewInformation.Camera.GetScreenSize(),
                LightPower = viewConfiguration.LightPower,
                StrongLightFactor = viewConfiguration.StrongLightFactor,
                ViewProj = Matrix4x4.Transpose(this.ViewInformation.Camera.ViewProjection),
                View = Matrix4x4.Transpose(this.ViewInformation.Camera.View),
            };

            _renderParameters.UpdateValues(renderState, cbPerView);

            // Query for postprocess effect
            var postprocessEffect = _renderParameters.GetPostprocessEffect(
                _sceneLayer.PostprocessEffectKey,
                resources);

            // Clear current depth buffer
            if (_sceneLayer.ClearDepthBufferBeforeRendering)
            {
                renderState.ClearCurrentDepthBuffer();
            }

            // Perform main render pass logic
            _renderParameters.Apply(renderState);

            var passId = 0;
            var continueWithNextPass = true;
            var layerName = _sceneLayer.Name;
            while (continueWithNextPass)
            {
                // Notify state before rendering
                postprocessEffect?.NotifyBeforeRender(renderState, layerName, passId);

                try
                {
                    // All following objects are build using only triangle lists
                    _device.DeviceImmediateContextD3D11.InputAssembler.PrimitiveTopology =
                        D3D.PrimitiveTopology.TriangleList;

                    // Perform all plain renderings
                    this.RenderPass(null, _objectsPassPlainRender, renderState, ref invalidObjects);

                    // Notify state after plain rendering
                    postprocessEffect?.NotifyAfterRenderPlain(renderState, layerName, passId);

                    // Render all lines
                    this.RenderPass(
                        _renderPassLineRender, _objectsPassLineRender,
                        renderState, ref invalidObjects);
                    renderState.ApplyMaterial(null);

                    // Perform all transparent renderings
                    this.RenderPass(
                        _renderPassTransparent, _objectsPassTransparentRender,
                        renderState, ref invalidObjects);
                }
                finally
                {
                    // Notify state after rendering
                    if (postprocessEffect != null)
                    {
                        continueWithNextPass = postprocessEffect.NotifyAfterRender(renderState, layerName, passId);
                    }
                    else { continueWithNextPass = false; }

                    // Increment passID value
                    if (continueWithNextPass)
                    {
                        renderState.DumpCurrentRenderTargetsIfActivated(_sceneLayer.Name, passId, "End");
                        passId++;
                    }
                }
            }

            // Clear current depth buffer
            if (_sceneLayer.ClearDepthBufferAfterRendering)
            {
                renderState.ClearCurrentDepthBuffer();
            }

            // Dump render state on end of the layer
            renderState.DumpCurrentRenderTargetsIfActivated(_sceneLayer.Name, passId, "End");

            // RemoveObject all invalid objects
            if (invalidObjects != null)
            {
                this.HandleInvalidObjects(invalidObjects);
            }
        }

        /// <summary>
        /// Subscribes the given object to the given render pass.
        /// </summary>
        internal RenderPassSubscription SubscribeForPass(
            RenderPassInfo passInfo,
            SceneObject sceneObject, Action<RenderState> renderMethod,
            int zOrder)
        {
            if (!_isSubscribeUnsubscribeAllowed)
            {
                throw new SeeingSharpException("Subscription is not allowed currently!");
            }

            var subscriptionProperties = _objectsPerPassDict[passInfo];

            // Append new subscription to subscription list
            var subscriptions = subscriptionProperties.Subscriptions;
            var subscriptionsCount = subscriptions.Count;
            var newSubscription = new RenderPassSubscription(this, passInfo, sceneObject, renderMethod, zOrder);

            if (!passInfo.IsSorted)
            {
                // No sort, so put the new subscription to the end of the collection
                newSubscription.SubscriptionIndex = subscriptionsCount;
                subscriptions.Add(newSubscription);
            }
            else
            {
                // Perform BinaryInsert to the correct position
                var newIndex = SeeingSharpUtil.BinaryInsert(subscriptions, newSubscription, SubscriptionZOrderComparer.Instance);

                // Increment all subscription indices after the inserted position
                subscriptionsCount++;
                for (var loop = newIndex; loop < subscriptionsCount; loop++)
                {
                    var actSubscription = subscriptions[loop];
                    if (actSubscription.SubscriptionIndex != loop)
                    {
                        actSubscription.SubscriptionIndex = loop;
                        subscriptions[loop] = actSubscription;

                        actSubscription.SceneObject.UpdateSubscription(actSubscription, this);
                    }
                }
                newSubscription = subscriptions[newIndex];
            }

            return newSubscription;
        }

        /// <summary>
        /// Unsubscribes the given object from the given render pass.
        /// </summary>
        internal void UnsubscribeForPass(RenderPassSubscription subscription)
        {
            if (!_isSubscribeUnsubscribeAllowed) { throw new SeeingSharpException("Subscription is not allowed currently!"); }

            if (subscription.IsSubscribed)
            {
                // Set unsubscribed flag
                _anythingUnsubscribed = true;

                // Register unsubscription call
                var subscriptionInfo = _objectsPerPassDict[subscription.RenderPass];

                subscriptionInfo.UnsubscribeCallCount++;

                // Update subscription info and reinsert it on the pass collection
                subscription.IsSubscribed = false;
                subscriptionInfo.Subscriptions[subscription.SubscriptionIndex] = subscription;

                // Post changes to the scene object holding this info too
                subscription.SceneObject.UpdateSubscription(subscription, this);
            }
        }

        /// <summary>
        /// Renders the 2D overlay of this view subset.
        /// </summary>
        /// <param name="renderState">The RenderState object holding all relevant data for current render pass</param>
        internal void Render2DOverlay(RenderState renderState)
        {
            if (_disposed) { throw new ObjectDisposedException("ViewRelatedLayerSubset"); }
            if (_objectsPass2DOverlay.Subscriptions.Count == 0) { return; }

            // Render all 2D objects
            List<SceneObject> invalidObjects = null;
            this.RenderPass(
                _renderPass2DOverlay, _objectsPass2DOverlay,
                renderState, ref invalidObjects);

            // RemoveObject all invalid objects
            if (invalidObjects != null)
            {
                this.HandleInvalidObjects(invalidObjects);
            }
        }

        /// <summary>
        /// Main method for object filtering. This method checks whether an object is visible or not.
        /// </summary>
        /// <param name="actObject">The object to be tested.</param>
        /// <param name="filters">All currently active filters.</param>
        private void PerformViewboxCulling(SceneObject actObject, IReadOnlyList<SceneObjectFilter> filters)
        {
            if (!actObject.IsLayerViewSubsetRegistered(ViewIndex)) { return; }
            if (_invalidObjects.ContainsKey(actObject)) { return; }

            // Get visibility check data about current object
            var checkData = actObject.GetVisibilityCheckData(this.ViewInformation);
            if (checkData == null) { return; }

            // Execute all filters in configured order step by step
            var filterCount = filters.Count;
            var previousFilterExecuted = false;
            var previousFilterResult = true;
            VisibilityCheckFilterStageData lastFilterStageData = null;
            for (var actFilterIndex = 0; actFilterIndex < filterCount; actFilterIndex++)
            {
                var actFilter = filters[actFilterIndex];

                // Get data about current filter stage
                var filterStageData = checkData.FilterStageData[actFilterIndex];
                if (filterStageData == null)
                {
                    filterStageData = checkData.FilterStageData.AddObject(
                        new VisibilityCheckFilterStageData(),
                        actFilterIndex);
                }

                // Remember last filter stage data
                lastFilterStageData = filterStageData;

                // Execute filter if needed
                if (!filterStageData.HasExecuted ||     // <-- Execute the filter if it was not executed for this object before
                    actFilter.ConfigurationChanged ||   // <-- Execute the filter if its configuration has changed
                    previousFilterExecuted ||           // <-- Execute the filter if one of the previous was executed
                    actFilter.UpdateEachFrame)          // <-- Execute the filter if it requests it on each frame (e. g. clipping filter)
                {
                    if (previousFilterResult)
                    {
                        // Re-Filter this object because any above condition has passed and
                        // this object successfully past the previous filter
                        var isObjectVisible = actFilter.IsObjectVisible(actObject, this.ViewInformation);

                        filterStageData.HasExecuted = true;
                        filterStageData.HasPassed = isObjectVisible;

                        previousFilterResult = isObjectVisible;
                        previousFilterExecuted = true;
                    }
                    else
                    {
                        // Set this object to invisible because previous filter has thrown out
                        // this object
                        filterStageData.HasExecuted = true;
                        filterStageData.HasPassed = false;
                    }
                }
                else
                {
                    previousFilterResult = filterStageData.HasPassed;
                    previousFilterExecuted = false;
                }
            }

            // Handle changed visibility of the object
            var oldVisible = checkData.IsVisible;
            var newVisible = lastFilterStageData?.HasPassed ?? true;
            if (oldVisible != newVisible)
            {
                checkData.IsVisible = newVisible;
                _tmpChangedVisibilities.Add(Tuple.Create(actObject, oldVisible, newVisible));
            }
        }

        /// <summary>
        /// Handles changed object visibility.
        /// This method is called from default update thread.
        /// </summary>
        /// <param name="sceneObject">The scene object to be handled.</param>
        /// <param name="newVisibility">New visibility flag value.</param>
        private void HandleObjectVisibilityChanged(SceneObject sceneObject, bool newVisibility)
        {
            if (!newVisibility)
            {
                // Deregister this object completely from the local layer subset
                sceneObject.UnsubsribeFromAllPasses(this);
            }
        }

        /// <summary>
        /// Refreshes device dependent resources of this class.
        /// </summary>
        private void RefreshDeviceDependentResources()
        {
            if (_renderParameters == null ||
                !_renderParameters.IsLoaded)
            {
                _renderParameters = _resources.AddAndLoadResource(
                    GraphicsCore.GetNextGenericResourceKey(),
                    new ViewRenderParameters());
            }

            if (_renderPassTransparent == null ||
                !_renderPassTransparent.IsLoaded)
            {
                _renderPassTransparent = _resources.GetResourceAndEnsureLoaded(
                    new NamedOrGenericKey(typeof(RenderPassDefaultTransparent)),
                    () => new RenderPassDefaultTransparent());
            }

            if (_renderPassLineRender == null ||
                !_renderPassLineRender.IsLoaded)
            {
                _renderPassLineRender = _resources.GetResourceAndEnsureLoaded(
                    new NamedOrGenericKey(typeof(RenderPassLineRender)),
                    () => new RenderPassLineRender());
            }

            if (_renderPass2DOverlay == null ||
                !_renderPass2DOverlay.IsLoaded)
            {
                _renderPass2DOverlay = _resources.GetResourceAndEnsureLoaded(
                    new NamedOrGenericKey(typeof(RenderPass2DOverlay)),
                    () => new RenderPass2DOverlay());
            }
        }

        /// <summary>
        /// Rendering logic for lines renderings.
        /// </summary>
        private void RenderPass(
            RenderPassBase renderPass, PassSubscriptionProperties subscriptions,
            RenderState renderState, ref List<SceneObject> invalidObjects)
        {
            if (subscriptions.Subscriptions.Count > 0)
            {
                if (renderPass != null)
                {
                    //Ensure loaded resources for transparency pass
                    if (!renderPass.IsLoaded)
                    {
                        renderPass.LoadResource();
                    }

                    //Render all subscriptions
                    renderPass.Apply(renderState);
                }
                try
                {
                    var subscriptionCount = subscriptions.Subscriptions.Count;

                    for (var loopPass = 0; loopPass < subscriptionCount; loopPass++)
                    {
                        var actSubscription = subscriptions.Subscriptions[loopPass];

                        try
                        {
                            actSubscription.RenderMethod(renderState);
                        }
                        catch (Exception ex)
                        {
                            // Publish exception info
                            GraphicsCore.PublishInternalExceptionInfo(
                                ex, InternalExceptionLocation.Rendering3DObject);

                            // Mark this object as invalid
                            if (invalidObjects == null)
                            {
                                invalidObjects = new List<SceneObject>();
                            }

                            invalidObjects.Add(actSubscription.SceneObject);
                        }
                    }
                }
                finally
                {
                    renderPass?.Discard(renderState);
                }
            }
        }

        /// <summary>
        /// Handles invalid objects.
        /// </summary>
        /// <param name="invalidObjects">List containing all invalid objects to handle.</param>
        private void HandleInvalidObjects(List<SceneObject> invalidObjects)
        {
            // Register the given objects as invalid
            foreach (var actInvalidObject in invalidObjects)
            {
                _invalidObjects.Add(actInvalidObject, null);
                _invalidObjectsToDeregister.Enqueue(actInvalidObject);
            }
        }

        /// <summary>
        /// Gets the corresponding ViewInformation object.
        /// </summary>
        public ViewInformation ViewInformation { get; }

        /// <summary>
        /// Gets or sets the index of this view subset within the scene.
        /// </summary>
        public int ViewIndex;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper class holding some information about subscriptions per pass.
        /// </summary>
        private class PassSubscriptionProperties
        {
            internal List<RenderPassSubscription> Subscriptions = new List<RenderPassSubscription>(DEFAULT_PASS_SUBSCRIPTION_LENGTH);

            /// <summary>
            /// A cached temporary collection which is used then updating the Subscription property
            /// </summary>
            internal List<RenderPassSubscription> SubscriptionsTemp = new List<RenderPassSubscription>(DEFAULT_PASS_SUBSCRIPTION_LENGTH);

            /// <summary>
            /// Total count of calls to Unsubscribe in this pass
            /// </summary>
            internal int UnsubscribeCallCount;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Helper for sorting render pass subscriptions by z order.
        /// </summary>
        private class SubscriptionZOrderComparer : IComparer<RenderPassSubscription>
        {
            public static readonly SubscriptionZOrderComparer Instance = new SubscriptionZOrderComparer();

            public int Compare(RenderPassSubscription x, RenderPassSubscription y)
            {
                return x.ZOrder.CompareTo(y.ZOrder);
            }
        }
    }
}