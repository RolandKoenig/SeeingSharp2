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

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Core
{
    public class SceneLayer
    {
        // View related members
        private IndexBasedDynamicCollection<ViewRelatedSceneLayerSubset> _viewSubsets;

        // All generic members
        private Queue<SceneObject> _sceneObjectsForSingleUpdateCall;
        private UnsafeList<SceneObject> _sceneObjectsNotStatic;
        private List<SceneObject> _sceneObjectsNotSpacial;
        private bool _isInUpdate;
        private bool _isInUpdateBeside;

        /// <summary>
        /// Gets the name of this layer.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets parent scene.
        /// </summary>
        public Scene Scene { get; }

        /// <summary>
        /// Gets or sets an integer which controls the order
        /// </summary>
        public int OrderId
        {
            get;
            internal set;
        }

        /// <summary>
        /// Allow picking on this layer?
        /// </summary>
        public bool AllowPick
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the resource key of the postprocess effect.
        /// </summary>
        public NamedOrGenericKey PostprocessEffectKey
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is rendering enabled.
        /// </summary>
        /// <value>
        /// <c>true</c> if this instance is rendering enabled; otherwise, <c>false</c>.
        /// </value>
        public bool IsRenderingEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Clear the depth buffer after rendering?
        /// </summary>
        public bool ClearDepthBufferAfterRendering
        {
            get;
            set;
        }

        /// <summary>
        /// Clear the depth buffer before rendering?
        /// </summary>
        public bool ClearDepthBufferBeforeRendering
        {
            get;
            set;
        }

        /// <summary>
        /// Gets a collection containing all objects.
        /// </summary>
        public ReadOnlyCollection<SceneObject> Objects { get; }

        /// <summary>
        /// Gets total count of objects within the scene.
        /// </summary>
        public int CountObjects => this.ObjectsInternal.Count;

        /// <summary>
        /// Gets a list containing all scene objects (internal accessor to the complete list).
        /// </summary>
        internal UnsafeList<SceneObject> ObjectsInternal { get; }

        /// <summary>
        /// Gets a list containing all spacial objects.
        /// </summary>
        internal List<SceneSpacialObject> SpacialObjects { get; }

        /// <summary>
        /// Creates a new SceneLayer object for the given scene.
        /// </summary>
        /// <param name="name">The name of the layer.</param>
        /// <param name="parentScene">Parent scene.</param>
        internal SceneLayer(string name, Scene parentScene)
        {
            this.Name = name;
            this.Scene = parentScene;

            //Create list holding all information for individual views
            _viewSubsets = new IndexBasedDynamicCollection<ViewRelatedSceneLayerSubset>();

            //Create standard collections
            this.ObjectsInternal = new UnsafeList<SceneObject>();
            this.Objects = new ReadOnlyCollection<SceneObject>(this.ObjectsInternal);

            //Create specialized collections
            this.SpacialObjects = new List<SceneSpacialObject>(1024);
            _sceneObjectsNotSpacial = new List<SceneObject>(1024);
            _sceneObjectsNotStatic = new UnsafeList<SceneObject>(1024);
            _sceneObjectsForSingleUpdateCall = new Queue<SceneObject>(1024);

            this.AllowPick = true;
            this.IsRenderingEnabled = true;
            this.ClearDepthBufferAfterRendering = false;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Layer " + this.Name;
        }

        /// <summary>
        /// Registers the given view on this layer.
        /// </summary>
        internal void RegisterView(int viewIndex, ViewInformation viewInformation, ResourceDictionary resourceDictionary)
        {
            var newLayerViewSubset = new ViewRelatedSceneLayerSubset(this, viewInformation, resourceDictionary, viewIndex);

            _viewSubsets.AddObject(
                newLayerViewSubset,
                viewIndex);

            newLayerViewSubset.RegisterObjectRange(this.ObjectsInternal);
        }

        /// <summary>
        /// Copies local specialized collections into new array (only copies references..).
        /// </summary>
        /// <param name="spacialObjects">An array containing all spacial objects.</param>
        /// <param name="notSpacialObjects">An array containing all not spacial objects.</param>
        internal void GetSpecializedCollectionsCopy(out SceneSpacialObject[] spacialObjects, out SceneObject[] notSpacialObjects)
        {
            spacialObjects = this.SpacialObjects.ToArray();
            notSpacialObjects = _sceneObjectsNotSpacial.ToArray();
        }

        /// <summary>
        /// Deregisters the given view on this layer.
        /// </summary>
        /// <param name="viewIndex">The index which this view has on the current scene.</param>
        /// <param name="viewInformation">The ViewInformation object describing the view.</param>
        internal void DeregisterView(int viewIndex, ViewInformation viewInformation)
        {
            // Dispose the layer subset (removes all its resources)
            var viewSubset = _viewSubsets[viewIndex];

            if (viewSubset != null)
            {
                viewSubset.ClearAllSubscriptions(this.ObjectsInternal);
                viewSubset.Dispose();
            }

            // Remote the subset
            _viewSubsets.RemoveObject(viewIndex);
        }

        /// <summary>
        /// Adds the given object to the layer.
        /// </summary>
        /// <param name="sceneObject">Object to add.</param>
        internal bool AddObject(SceneObject sceneObject)
        {
            if (_isInUpdate || _isInUpdateBeside) { throw new InvalidOperationException("Unable to manipulate object list while SceneLayout is on updating!"); }
            if (sceneObject == null) { throw new ArgumentNullException(nameof(sceneObject)); }
            if (sceneObject.Scene == this.Scene) { return false; }
            if (sceneObject.Scene != null) { throw new ArgumentException("Given object does already belong to another scene!", nameof(sceneObject)); }
            if (sceneObject.SceneLayer == this) { return false; }
            if (sceneObject.SceneLayer != null) { throw new ArgumentException("Given object does already belong to another scene layer!", nameof(sceneObject)); }

            this.ObjectsInternal.Add(sceneObject);
            sceneObject.SetSceneAndLayer(this.Scene, this);

            // Append object to specialized collections
            if (sceneObject is SceneSpacialObject spacialObject)
            {
                this.SpacialObjects.Add(spacialObject);
            }
            else
            {
                _sceneObjectsNotSpacial.Add(sceneObject);
            }

            // Handle static / non static objects
            if (sceneObject.IsStatic)
            {
                _sceneObjectsForSingleUpdateCall.Enqueue(sceneObject);
            }
            else
            {
                _sceneObjectsNotStatic.Add(sceneObject);
            }

            // Register the given object on all view subsets
            foreach (var actViewSubset in _viewSubsets)
            {
                actViewSubset.RegisterObject(sceneObject);
            }

            return true;
        }

        /// <summary>
        /// Clears this layer.
        /// </summary>
        internal void ClearObjects()
        {
            // Clear objects on all view subsets
            foreach (var actViewSubset in _viewSubsets)
            {
                actViewSubset.ClearAllSubscriptions(this.ObjectsInternal);
            }

            foreach (var actObject in this.ObjectsInternal)
            {
                actObject.UnloadResources();
                actObject.ResetSceneAndLayer();
            }

            this.ObjectsInternal.Clear();

            // Clear specialized collections
            _sceneObjectsNotSpacial.Clear();
            this.SpacialObjects.Clear();
            _sceneObjectsNotStatic.Clear();
        }

        /// <summary>
        /// Clears all resources used by this layer.
        /// </summary>
        internal void ClearResources()
        {
            // Clear objects on all view subsets
            foreach (var actViewSubset in _viewSubsets)
            {
                actViewSubset.ClearResources();
            }
        }

        /// <summary>
        /// Removes the given object from the layer.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        internal void RemoveObject(SceneObject sceneObject)
        {
            if (_isInUpdate || _isInUpdateBeside) { throw new InvalidOperationException("Unable to manipulate object list while SceneLayout is on updating!"); }

            if (this.ObjectsInternal.Contains(sceneObject))
            {
                sceneObject.UnloadResources();

                this.ObjectsInternal.Remove(sceneObject);
                sceneObject.ResetSceneAndLayer();

                // RemoveObject object from specialized collections
                if (sceneObject is SceneSpacialObject spacialObject)
                {
                    this.SpacialObjects.Remove(spacialObject);
                }
                else
                {
                    _sceneObjectsNotSpacial.Remove(sceneObject);
                }

                // RemoveObject object form non-static collection
                if (!sceneObject.IsStatic)
                {
                    _sceneObjectsNotStatic.Remove(sceneObject);
                }

                // RemoveObject this object on all view subsets
                foreach (var actViewSubset in _viewSubsets)
                {
                    actViewSubset.DeregisterObject(sceneObject);
                }
            }
        }

        /// <summary>
        /// Unloads all resources.
        /// </summary>
        internal void UnloadResources()
        {
            //Unload resources of all scene objects
            foreach (var actObject in this.ObjectsInternal)
            {
                actObject.UnloadResources();
            }
        }

        /// <summary>
        /// Prepares rendering (Loads all needed resources).
        /// </summary>
        /// <param name="renderState">Current render state.</param>
        internal void PrepareRendering(RenderState renderState)
        {
            List<SceneObject> invalidObjects = null;

            // Load all resources
            var sceneObjectArrayLength = this.ObjectsInternal.Count;
            var sceneObjectArray = this.ObjectsInternal.BackingArray;
            for (var loop = 0; loop < sceneObjectArrayLength; loop++)
            {
                var actObject = sceneObjectArray[loop];

                try
                {
                    // Load all resources of the object
                    if (!actObject.IsLoaded(renderState.Device))
                    {
                        actObject.LoadResources(renderState.Device, renderState.CurrentResources);
                    }
                }
                catch (Exception ex)
                {
                    // Publish exception info
                    GraphicsCore.PublishInternalExceptionInfo(
                        ex, InternalExceptionLocation.Loading3DObject);

                    //Build list of invalid objects
                    if (invalidObjects == null)
                    {
                        invalidObjects = new List<SceneObject>();
                    }

                    invalidObjects.Add(actObject);
                }
            }

            //RemoveObject all invalid objects
            if (invalidObjects != null)
            {
                this.HandleInvalidObjects(invalidObjects);
            }
        }

        /// <summary>
        /// Updates the layer.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        internal void Update(SceneRelatedUpdateState updateState)
        {
            updateState.SceneLayer = this;
            _isInUpdate = true;
            try
            {
                // Update all objects which are registered for initial update call
                var initialUpdateCallCount = _sceneObjectsForSingleUpdateCall.Count;
                if (initialUpdateCallCount > 0)
                {
                    var initialUpdateCallItems = _sceneObjectsForSingleUpdateCall.GetBackingArray();
                    for (var loop = 0; loop < initialUpdateCallCount; loop++)
                    {
                        initialUpdateCallItems[loop].Update(updateState);
                    }
                }

                // Call default update method for each object
                var updateListLength = _sceneObjectsNotStatic.Count;
                var updateList = _sceneObjectsNotStatic.BackingArray;
                for (var actIndex = 0; actIndex < updateListLength; actIndex++)
                {
                    if (!updateList[actIndex].HasParent)
                    {
                        updateList[actIndex].Update(updateState);
                    }
                }

                // Call overall updates on all objects
                for (var loop = 0; loop < updateListLength; loop++)
                {
                    if (!updateList[loop].HasParent)
                    {
                        updateList[loop].UpdateOverall(updateState);
                    }
                }

                // Now update all view specific references
                foreach (var actViewSubset in _viewSubsets)
                {
                    actViewSubset.UpdateForView(updateState);
                }
            }
            finally
            {
                updateState.SceneLayer = null;
                _isInUpdate = false;
            }
        }

        /// <summary>
        /// Performs "update-beside-render" for this layer.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        internal void UpdateBesideRender(SceneRelatedUpdateState updateState)
        {
            updateState.SceneLayer = this;
            _isInUpdateBeside = true;
            try
            {
                // Now update all view specific references
                foreach (var actViewSubset in _viewSubsets)
                {
                    actViewSubset.UpdateBesideRender(updateState, _sceneObjectsForSingleUpdateCall);
                }
            }
            finally
            {
                updateState.SceneLayer = null;
                _isInUpdateBeside = false;

                // Some work after whole update processes
                _sceneObjectsForSingleUpdateCall.Clear();
            }
        }

        /// <summary>
        /// Renders the scene to the given context.
        /// </summary>
        internal void Render(RenderState renderState)
        {
            if (!this.IsRenderingEnabled)
            {
                return;
            }

            //Delegate render call to corresponding view subset
            var viewSubset = _viewSubsets[renderState.ViewIndex];

            if (viewSubset != null)
            {
                _viewSubsets[renderState.ViewIndex].Render(renderState);
            }
        }

        /// <summary>
        /// Renders the 2D overlay to the given context.
        /// </summary>
        /// <param name="renderState">State of the render.</param>
        internal void Render2DOverlay(RenderState renderState)
        {
            //Delegate render call to corresponding view subset
            var viewSubset = _viewSubsets[renderState.ViewIndex];

            if (viewSubset != null)
            {
                _viewSubsets[renderState.ViewIndex].Render2DOverlay(renderState);
            }
        }

        /// <summary>
        /// Handles invalid objects.
        /// </summary>
        /// <param name="invalidObjects">List containing all invalid objects to handle.</param>
        private void HandleInvalidObjects(List<SceneObject> invalidObjects)
        {
            foreach (var actObject in invalidObjects)
            {
                //Unload the object if it is loaded
                try
                {
                    actObject.UnloadResources();
                }
                catch (Exception ex)
                {
                    // Publish exception info
                    GraphicsCore.PublishInternalExceptionInfo(
                        ex, InternalExceptionLocation.UnloadingInvalid3DObject);
                }

                //RemoveObject this object from this layer
                this.RemoveObject(actObject);
            }
        }
    }
}