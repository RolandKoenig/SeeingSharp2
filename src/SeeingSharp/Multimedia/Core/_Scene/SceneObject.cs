using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Numerics;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    public abstract partial class SceneObject : IDisposable, IAnimatableObject
    {
        // Generic members
        private IndexBasedDynamicCollection<VisibilityCheckData> _visibilityData;
        private DetailLevel _targetDetailLevel;
        private bool _isStatic;
        private VisibilityTestMethod _prevVisibilityTestMethod;

        // Some information about parent containers
        private Scene _scene;
        private SceneLayer _sceneLayer;

        // Collections for describing object hierarchies
        private UnsafeList<SceneObject> _children;
        private SceneObject _parent;

        // Members for animations
        private AnimationHandler _animationHandler;

        /// <summary>
        /// Gets current AnimationHandler object.
        /// </summary>
        public virtual AnimationHandler AnimationHandler => _animationHandler;

        /// <summary>
        /// Gets or sets the name of this node.
        /// The name ist just meta information and has no relevance for SeeingSharp.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets an additional data object.
        /// </summary>
        public object Tag1 { get; set; }

        /// <summary>
        /// Gets or sets an additional data object.
        /// </summary>
        public object Tag2 { get; set; }

        /// <summary>
        /// Is this object visible for picking-test?
        /// </summary>
        public bool IsPickingTestVisible
        {
            get;
            set;
        }

        /// <summary>
        /// Gets current scene.
        /// </summary>
        public Scene Scene => _scene;

        /// <summary>
        /// Gets or sets the scene layer.
        /// </summary>
        public SceneLayer SceneLayer => _sceneLayer;

        /// <summary>
        /// Is this object a static object?
        /// </summary>
        public bool IsStatic
        {
            get => _isStatic;
            set
            {
                if (this.Scene != null) { throw new SeeingSharpException("Unable to change IsStatic state when the object is already assigned to a scene!"); }
                _isStatic = value;
            }
        }

        /// <summary>
        /// Gets or sets the target detail level.
        /// </summary>
        public DetailLevel TargetDetailLevel
        {
            get => _targetDetailLevel;
            set
            {
                if (this.Scene != null) { throw new SeeingSharpGraphicsException("Unable to change TargetDetailLevel when object is already added to a scene!"); }
                _targetDetailLevel = value;
            }
        }

        /// <summary>
        /// Does this object have a parent?
        /// </summary>
        public bool HasParent => _parent != null;

        /// <summary>
        /// Gets the parent object.
        /// </summary>
        public SceneObject Parent => _parent;

        /// <summary>
        /// Gets or sets the method how visibility of this object is calculated.
        /// </summary>
        public VisibilityTestMethod VisibilityTestMethod { get; set; }

        /// <summary>
        /// Does this object have any child?
        /// </summary>
        public bool HasChildren
        {
            get
            {
                var children = _children;
                return children != null && children.Count > 0;
            }
        }

        /// <summary>
        /// Gets the total count of direct children of this object.
        /// </summary>
        public int CountChildren => _children.Count;

        /// <summary>
        /// Is it possible to export this object?
        /// </summary>
        public virtual bool IsExportable
        {
            get;
        }

        /// <summary>
        /// Indicates whether transformation data has changed during last update calls.
        /// This member is used for per-view object-filtering to ignore objects which haven't changed their state.
        /// </summary>
        internal bool TransformationChanged;

        /// <summary>
        /// Indicates whether visibility test method has changed.
        /// This member is used for per-view object-filtering to ignore objects which haven't changed their state.
        /// </summary>
        internal bool VisibilityTestMethodChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneObject"/> class.
        /// </summary>
        protected SceneObject()
        {
            this.IsExportable = false;
            this.Name = string.Empty;

            _targetDetailLevel = DetailLevel.All;

            _children = new UnsafeList<SceneObject>();
            _parent = null;

            _animationHandler = new AnimationHandler(this);
            _visibilityData = new IndexBasedDynamicCollection<VisibilityCheckData>();

            TransformationChanged = true;
            this.IsPickingTestVisible = true;
        }

        /// <summary>
        /// Checks whether this object is the parent of the given one.
        /// </summary>
        /// <param name="other">The object to check for.</param>
        /// <returns>true if the given object is a children of this one.</returns>
        public bool IsParentOf(SceneObject other)
        {
            other.EnsureNotNull(nameof(other));

            // Caution: This method must be thread safe because
            //          it is callable on the SceneObject class directly

            var actParent = other._parent;

            while (actParent != null)
            {
                if (actParent == this)
                {
                    return true;
                }

                actParent = actParent._parent;
            }

            return false;
        }

        /// <summary>
        /// Determines whether this object already contains this child (no lower level check).
        /// </summary>
        /// <param name="objectToCheck">The object to check for.</param>
        public bool IsParentOfOnSingleLevel(SceneObject objectToCheck)
        {
            objectToCheck.EnsureNotNull(nameof(objectToCheck));

            // Caution: This method must be thread safe because
            //          it is callable on the SceneObject class directly

            return objectToCheck._parent == this;
        }

        /// <summary>
        /// Queries for all children.
        /// </summary>
        /// <param name="queryDeep">Do also return lower level children?</param>
        public IEnumerable<SceneObject> GetAllChildren(bool queryDeep = true)
        {           
            // Caution: This method must be thread safe because
            //          it is callable on the SceneObject class directly

            var childArray = _children.BackingArray;
            for (var loop = 0; loop < childArray.Length; loop++)
            {
                var actChild = childArray[loop];
                if(actChild == null){ continue; }

                yield return actChild;

                if (queryDeep)
                {
                    foreach (var actLowerChild in actChild.GetAllChildren())
                    {
                        yield return actLowerChild;
                    }
                }
            }
        }

        /// <summary>
        /// Is this object visible in the given view?
        /// </summary>
        /// <param name="viewInfo">The view info to check.</param>
        public bool IsVisible(ViewInformation viewInfo)
        {
            if (viewInfo.ViewIndex < 0) { throw new SeeingSharpGraphicsException("Given ViewInformation object is not associated to any view!"); }
            if (viewInfo.Scene == null) { throw new SeeingSharpGraphicsException("Given ViewInformation object is not attached to any scene!"); }
            if (viewInfo.Scene != this.Scene) { throw new SeeingSharpGraphicsException("Given ViewInformation object is not attached to this scene!"); }

            var checkData = _visibilityData[viewInfo.ViewIndex];

            if (checkData == null)
            {
                return false;
            }

            return checkData.IsVisible;
        }

        /// <summary>
        /// Tries to set the visibility of this object on the given view.
        /// This method can be used to force rendering on the next frame after adding
        /// an object to the scene.
        /// </summary>
        /// <param name="viewInfo">The view on which to set the visibility.</param>
        /// <param name="isVisible">The visibility state to set.</param>
        public bool TrySetInitialVisibility(ViewInformation viewInfo, bool isVisible)
        {
            if (viewInfo.ViewIndex < 0) { return false; }

            var checkData = this.GetVisibilityCheckData(viewInfo);

            if (checkData.IsVisible) { return true; }

            if (checkData.IsVisible != isVisible &&
                checkData.FilterStageData.Count == 0)
            {
                checkData.IsVisible = isVisible;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Fills the given collection with all referenced resources.
        /// </summary>
        /// <param name="resourceCollection">The collection to be filled,</param>
        public virtual void GetReferencedResources(SingleInstanceCollection<Resource> resourceCollection)
        {
        }

        /// <summary>
        /// Loads all resources of the object.
        /// </summary>
        /// <param name="device">Current graphics device.</param>
        /// <param name="resourceDictionary">Current resource dictionary.</param>
        public abstract void LoadResources(EngineDevice device, ResourceDictionary resourceDictionary);

        /// <summary>
        /// Unloads all resources of the object.
        /// </summary>
        public virtual void UnloadResources()
        {

        }

        /// <summary>
        /// Are resources loaded for the given device?
        /// </summary>
        public abstract bool IsLoaded(EngineDevice device);

        /// <summary>
        /// Disposes all unmanaged resources of this object.
        /// </summary>
        public void Dispose()
        {
            this.UnloadResources();
        }

        /// <summary>
        /// Called when this object was added to a scene.
        /// </summary>
        protected virtual void OnAddedToScene(Scene newScene)
        {

        }

        /// <summary>
        /// Called when this object was removed from a scene.
        /// </summary>
        protected virtual void OnRemovedFromScene(Scene oldScene)
        {

        }

        /// <summary>
        /// Processes the given input frame.
        /// </summary>
        /// <param name="inputFrame">The input frame.</param>
        protected virtual void ProcessInputInternal(InputFrame inputFrame) { }

        /// <summary>
        /// Updates the object.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        protected abstract void UpdateInternal(SceneRelatedUpdateState updateState);

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset which called this update method.</param>
        protected abstract void UpdateForViewInternal(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset);

        /// <summary>
        /// Updates all children of this object. Override this to change default behavior.
        /// </summary>
        /// <param name="updateState">The current update state.</param>
        /// <param name="children">The full list of children that should be updated.</param>
        protected virtual void UpdateChildrenInternal(SceneRelatedUpdateState updateState, UnsafeList<SceneObject> children)
        {
            // Trigger updates of all dependencies
            foreach (var actDependency in _children)
            {
                actDependency.Update(updateState);
            }
        }

        /// <summary>
        /// Updates all children of this object (overall update). Override this to change default behavior.
        /// </summary>
        /// <param name="updateState">The current update state.</param>
        /// <param name="children">The full list of children that should be updated.</param>
        protected virtual void UpdateChildrenOverallInternal(SceneRelatedUpdateState updateState, UnsafeList<SceneObject> children)
        {
            // Trigger updates of all dependencies
            foreach (var actDependency in _children)
            {
                actDependency.UpdateOverall(updateState);
            }
        }

        /// <summary>
        /// This method stores all data related to this object into the given <see cref="ExportModelContainer"/>.
        /// </summary>
        /// <param name="modelContainer">The target container.</param>
        /// <param name="exportOptions">Options for export.</param>
        protected virtual void PrepareForExportInternal(
            ExportModelContainer modelContainer,
            ExportOptions exportOptions)
        {

        }

        /// <summary>
        /// This method stores all data related to this object into the given <see cref="ExportModelContainer"/>.
        /// </summary>
        /// <param name="modelContainer">The target container.</param>
        /// <param name="exportOptions">Options for export.</param>
        internal void PrepareForExport(ExportModelContainer modelContainer, ExportOptions exportOptions)
        {
            this.PrepareForExportInternal(modelContainer, exportOptions);
        }

        /// <summary>
        /// Registers the given scene and layer on this object.
        /// </summary>
        /// <param name="scene">The scene.</param>
        /// <param name="sceneLayer">The scene layer.</param>
        internal void SetSceneAndLayer(Scene scene, SceneLayer sceneLayer)
        {
            scene.EnsureNotNull(nameof(scene));
            sceneLayer.EnsureNotNull(nameof(sceneLayer));
            _scene.EnsureNull(nameof(_scene));
            _sceneLayer.EnsureNull(nameof(_sceneLayer));

            _scene = scene;
            _sceneLayer = sceneLayer;

            // Call virtual event
            this.OnAddedToScene(_scene);
        }

        /// <summary>
        /// Deregisters the current scene and layer from this object.
        /// </summary>
        internal void ResetSceneAndLayer()
        {
            _scene.EnsureNotNull(nameof(_scene));
            _sceneLayer.EnsureNotNull(nameof(_sceneLayer));

            // Remember old scene
            var oldScene = _scene;

            // Clear references
            _scene = null;
            _sceneLayer = null;

            // Call virtual event
            this.OnRemovedFromScene(oldScene);
        }

        /// <summary>
        /// Adds the given object as a child.
        /// </summary>
        /// <param name="childToAdd">The object which is be be located under this one within object hierarchy.</param>
        internal void AddChildInternal(SceneObject childToAdd)
        {
            if (childToAdd == this) { throw new SeeingSharpGraphicsException("Cyclic parent/child relationship detected!"); }
            if (childToAdd.Scene != this.Scene) { throw new SeeingSharpGraphicsException("Child must have the same scene!"); }
            if (childToAdd._parent != null) { throw new SeeingSharpGraphicsException("Child has already an owner!"); }
            if (childToAdd.IsParentOf(this)) { throw new SeeingSharpGraphicsException("Cyclic parent/child relationship detected!"); }
            if (_children.Contains(childToAdd)) { throw new SeeingSharpGraphicsException("Child is already added!"); }

            // Create parent/child relation
            _children.Add(childToAdd);
            childToAdd._parent = this;
        }

        /// <summary>
        /// Removes the given object from the list of children.
        /// </summary>
        /// <param name="childToRemove">The object which is to be removed from the list of children.</param>
        internal void RemoveChildInternal(SceneObject childToRemove)
        {
            if (childToRemove.Scene != this.Scene) { throw new ArgumentException("Child must have the same scene!"); }

            // Destroy parent/child relation
            _children.Remove(childToRemove);
            if (childToRemove._parent == this) { childToRemove._parent = this; }
        }

        /// <summary>
        /// Clears current visibility data.
        /// </summary>
        internal void ClearVisibilityStageData()
        {
            foreach (var actCheckData in _visibilityData)
            {
                actCheckData.FilterStageData.Clear();
            }
        }

        /// <summary>
        /// Gets the data object used for visibility checking.
        /// </summary>
        /// <param name="viewInfo">The VisibilityCheckData for this object for the given view.</param>
        internal VisibilityCheckData GetVisibilityCheckData(ViewInformation viewInfo)
        {
            var checkData = _visibilityData[viewInfo.ViewIndex];
            if (checkData == null)
            {
                checkData = _visibilityData.AddObject(
                    new VisibilityCheckData(),
                    viewInfo.ViewIndex);
            }

            return checkData;
        }

        /// <summary>
        /// Picks an object in 3D-World.
        /// </summary>
        /// <param name="rayStart">Start of picking ray.</param>
        /// <param name="rayDirection"></param>
        /// <param name="viewInfo">Information about the view that triggered picking.</param>
        /// <param name="pickingOptions">Some additional options for picking calculations.</param>
        /// <returns>Returns the distance to the object or float.NaN if object is not picked.</returns>
        internal virtual float Pick(Vector3 rayStart, Vector3 rayDirection, ViewInformation viewInfo, PickingOptions pickingOptions)
        {
            return float.NaN;
        }

        /// <summary>
        /// Is this object visible currently?
        /// </summary>
        /// <param name="viewInfo">Information about the view that triggered bounding volume testing.</param>
        /// <param name="boundingFrustum">The bounding frustum to check.</param>
        internal virtual bool IsInBoundingFrustum(ViewInformation viewInfo, ref BoundingFrustum boundingFrustum)
        {
            return true;
        }

        /// <summary>
        /// Registers a layer view subset with the given index.
        /// </summary>
        /// <param name="layerViewSubset">The layer view subset to register.</param>
        internal void RegisterLayerViewSubset(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            _viewRelatedSubscriptions.AddObject(new List<RenderPassSubscription>(), layerViewSubset.ViewIndex);
        }

        /// <summary>
        /// Deregisters a layer view subset with the given index.
        /// </summary>
        /// <param name="layerViewSubset">The layer view subset to deregister.</param>
        internal void DeregisterLayerViewSubset(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            if (_viewRelatedSubscriptions.HasObjectAt(layerViewSubset.ViewIndex))
            {
                _viewRelatedSubscriptions.RemoveObject(layerViewSubset.ViewIndex);
            }
        }

        /// <summary>
        /// Is the given view index registered on this object?
        /// </summary>
        /// <param name="viewIndex">The index to check for.</param>
        internal bool IsLayerViewSubsetRegistered(int viewIndex)
        {
            return _viewRelatedSubscriptions.HasObjectAt(viewIndex);
        }

        /// <summary>
        /// Processes the given input frame.
        /// </summary>
        /// <param name="inputFrame">The input frame.</param>
        internal void ProcessInput(InputFrame inputFrame)
        {
            this.ProcessInputInternal(inputFrame);
        }

        /// <summary>
        /// Updates this object.
        /// </summary>
        /// <param name="updateState">State of update process.</param>
        internal void Update(SceneRelatedUpdateState updateState)
        {
            // Update current animation state
            _animationHandler?.Update(updateState);

            // Check for changed visibility test method
            if (_prevVisibilityTestMethod != this.VisibilityTestMethod)
            {
                _prevVisibilityTestMethod = this.VisibilityTestMethod;
                this.VisibilityTestMethodChanged = true;
            }

            // Update the object
            this.UpdateInternal(updateState);

            // Update all dependencies finally
            if (_children.Count > 0)
            {
                this.UpdateChildrenInternal(updateState, _children);
            }
        }

        /// <summary>
        /// Update logic for overall updates.
        /// This method should be used for update logic that also depends on other object.
        /// UpdateOverall methods are called sequentially object by object.
        /// </summary>
        /// <param name="updateState">Current update state of the scene.</param>
        internal void UpdateOverall(SceneRelatedUpdateState updateState)
        {
            // Update all dependencies finally
            if (_children.Count > 0)
            {
                this.UpdateChildrenOverallInternal(updateState, _children);
            }
        }

        /// <summary>
        /// Updates this object for the given view.
        /// </summary>
        /// <param name="updateState">Current state of the update pass.</param>
        /// <param name="layerViewSubset">The layer view subset which called this update method.</param>
        internal void UpdateForView(SceneRelatedUpdateState updateState, ViewRelatedSceneLayerSubset layerViewSubset)
        {
            this.UpdateForViewInternal(updateState, layerViewSubset);
        }
    }
}