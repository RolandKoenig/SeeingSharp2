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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Input;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{
    public abstract partial class SceneObject : IDisposable, IAnimatableObject
    {
        // Generic members
        private IndexBasedDynamicCollection<VisibilityCheckData> m_visibilityData;
        private DetailLevel m_targetDetailLevel;
        private bool m_isStatic;

        // Some information about parent containers
        private Scene m_scene;
        private SceneLayer m_sceneLayer;

        // Collections for describing object hierarchies
        private List<SceneObject> m_children;
        private SceneObject m_parent;

        // Members for animations
        private AnimationHandler m_animationHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneObject"/> class.
        /// </summary>
        protected SceneObject()
        {
            m_targetDetailLevel = DetailLevel.All;

            m_children = new List<SceneObject>();
            m_parent = null;

            m_animationHandler = new AnimationHandler(this);
            m_visibilityData = new IndexBasedDynamicCollection<VisibilityCheckData>();

            //Create a dynamic container for custom data
            this.CustomData = new ExpandoObject();

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

            var actParent = other.m_parent;

            while (actParent != null)
            {
                if (actParent == this)
                {
                    return true;
                }

                actParent = actParent.m_parent;
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

            return objectToCheck.m_parent == this;
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

            var checkData = m_visibilityData[viewInfo.ViewIndex];

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
        protected virtual void UpdateChildrenInternal(SceneRelatedUpdateState updateState, List<SceneObject> children)
        {
            // Trigger updates of all dependencies
            foreach (var actDependency in m_children)
            {
                actDependency.Update(updateState);
            }
        }

        /// <summary>
        /// Updates all children of this object (overall update). Override this to change default behavior.
        /// </summary>
        /// <param name="updateState">The current update state.</param>
        /// <param name="children">The full list of children that should be updated.</param>
        protected virtual void UpdateChildrenOverallInternal(SceneRelatedUpdateState updateState, List<SceneObject> children)
        {
            // Trigger updates of all dependencies
            foreach (var actDependency in m_children)
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
            m_scene.EnsureNull(nameof(m_scene));
            m_sceneLayer.EnsureNull(nameof(m_sceneLayer));

            m_scene = scene;
            m_sceneLayer = sceneLayer;

            // Call virtual event
            this.OnAddedToScene(m_scene);
        }

        /// <summary>
        /// Deregisters the current scene and layer from this object.
        /// </summary>
        internal void ResetSceneAndLayer()
        {
            m_scene.EnsureNotNull(nameof(m_scene));
            m_sceneLayer.EnsureNotNull(nameof(m_sceneLayer));

            // Remember old scene
            var oldScene = m_scene;

            // Clear references
            m_scene = null;
            m_sceneLayer = null;

            // Call virtual event
            this.OnRemovedFromScene(oldScene);
        }

        /// <summary>
        /// Queries for all children (also lower level).
        /// </summary>
        internal IEnumerable<SceneObject> GetAllChildrenInternal()
        {
            foreach (var actChild in m_children)
            {
                yield return actChild;

                foreach (var actLowerChild in actChild.GetAllChildrenInternal())
                {
                    yield return actLowerChild;
                }
            }
        }

        /// <summary>
        /// Adds the given object as a child.
        /// </summary>
        /// <param name="childToAdd">The object which is be be located under this one within object hierarchy.</param>
        internal void AddChildInternal(SceneObject childToAdd)
        {
            if (childToAdd == this) { throw new SeeingSharpGraphicsException("Cyclic parent/child relationship detected!"); }
            if (childToAdd.Scene != this.Scene) { throw new SeeingSharpGraphicsException("Child must have the same scene!"); }
            if (childToAdd.m_parent != null) { throw new SeeingSharpGraphicsException("Child has already an owner!"); }
            if (childToAdd.IsParentOf(this)) { throw new SeeingSharpGraphicsException("Cyclic parent/child relationship detected!"); }
            if (m_children.Contains(childToAdd)) { throw new SeeingSharpGraphicsException("Child is already added!"); }

            // Create parent/child relation
            m_children.Add(childToAdd);
            childToAdd.m_parent = this;
        }

        /// <summary>
        /// Removes the given object from the list of children.
        /// </summary>
        /// <param name="childToRemove">The object which is to be removed from the list of children.</param>
        internal void RemoveChildInternal(SceneObject childToRemove)
        {
            if (childToRemove.Scene != this.Scene) { throw new ArgumentException("Child must have the same scene!"); }

            // Destroy parent/child relation
            m_children.Remove(childToRemove);
            if (childToRemove.m_parent == this) { childToRemove.m_parent = this; }
        }

        /// <summary>
        /// Clears current visibility data.
        /// </summary>
        internal void ClearVisibilityStageData()
        {
            foreach (var actCheckData in m_visibilityData)
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
            var checkData = m_visibilityData[viewInfo.ViewIndex];
            if (checkData == null)
            {
                checkData = m_visibilityData.AddObject(
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
            m_viewRelatedSubscriptions.AddObject(new List<RenderPassSubscription>(), layerViewSubset.ViewIndex);
        }

        /// <summary>
        /// Deregisters a layer view subset with the given index.
        /// </summary>
        /// <param name="layerViewSubset">The layer view subset to deregister.</param>
        internal void DeregisterLayerViewSubset(ViewRelatedSceneLayerSubset layerViewSubset)
        {
            if (m_viewRelatedSubscriptions.HasObjectAt(layerViewSubset.ViewIndex))
            {
                m_viewRelatedSubscriptions.RemoveObject(layerViewSubset.ViewIndex);
            }
        }

        /// <summary>
        /// Is the given view index registered on this object?
        /// </summary>
        /// <param name="viewIndex">The index to check for.</param>
        internal bool IsLayerViewSubsetRegistered(int viewIndex)
        {
            return m_viewRelatedSubscriptions.HasObjectAt(viewIndex);
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
            m_animationHandler?.Update(updateState);

            // Update the object
            this.UpdateInternal(updateState);

            // Update all dependencies finally
            if (m_children.Count > 0)
            {
                this.UpdateChildrenInternal(updateState, m_children);
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
            if (m_children.Count > 0)
            {
                this.UpdateChildrenOverallInternal(updateState, m_children);
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

        /// <summary>
        /// Gets current AnimationHandler object.
        /// </summary>
        public virtual AnimationHandler AnimationHandler => m_animationHandler;

        /// <summary>
        /// Gets a dynamic container for custom data.
        /// </summary>
        public dynamic CustomData { get; }

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
        public Scene Scene => m_scene;

        /// <summary>
        /// Gets or sets the scene layer.
        /// </summary>
        public SceneLayer SceneLayer => m_sceneLayer;

        /// <summary>
        /// Is this object a static object?
        /// </summary>
        public bool IsStatic
        {
            get => m_isStatic;
            set
            {
                if (this.Scene != null) { throw new SeeingSharpException("Unable to change IsStatic state when the object is already assigned to a scene!"); }
                m_isStatic = value;
            }
        }

        /// <summary>
        /// Gets or sets the target detail level.
        /// </summary>
        public DetailLevel TargetDetailLevel
        {
            get => m_targetDetailLevel;
            set
            {
                if (this.Scene != null) { throw new SeeingSharpGraphicsException("Unable to change TargetDetailLevel when object is already added to a scene!"); }
                m_targetDetailLevel = value;
            }
        }

        /// <summary>
        /// Does this object have a parent?
        /// </summary>
        public bool HasParent => m_parent != null;

        /// <summary>
        /// Gets the parent object.
        /// </summary>
        public SceneObject Parent => m_parent;

        /// <summary>
        /// Does this object have any child?
        /// </summary>
        public bool HasChildren
        {
            get
            {
                var children = m_children;
                return children != null && children.Count > 0;
            }
        }

        /// <summary>
        /// Gets the total count of direct children of this object.
        /// </summary>
        public int CountChildren => m_children.Count;

        /// <summary>
        /// Is it possible to export this object?
        /// </summary>
        public virtual bool IsExportable
        {
            get;
        }

        /// <summary>
        /// Indicates whether transformation data has changed during last update calls.
        /// This member is used for viewbox-culling to ignore objects which haven't changed their state.
        /// </summary>
        internal bool TransformationChanged;
    }
}