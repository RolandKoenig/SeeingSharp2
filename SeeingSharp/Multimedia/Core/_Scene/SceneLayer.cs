#region License information (SeeingSharp and all based games/applications)
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp (sourcecode)
     - http://www.rolandk.de/wp (the autors homepage, german)
    Copyright (C) 2016 Roland König (RolandK)
    
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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using SeeingSharp.Util;

//Some namespace mappings
using D3D11 = SharpDX.Direct3D11;

namespace SeeingSharp.Multimedia.Core
{
    public class SceneLayer
    {
        #region All generic members
        private Queue<SceneObject> m_sceneObjectsForSingleUpdateCall;
        private List<SceneObject> m_sceneObjectsNotStatic;
        private List<SceneObject> m_sceneObjects;
        private List<SceneObject> m_sceneObjectsNotSpacial;
        private List<SceneSpacialObject> m_sceneObjectsSpacial;
        private ReadOnlyCollection<SceneObject> m_sceneObjectsPublic;
        private Scene m_scene;
        private string m_name;
        private bool m_isInUpdate;
        private bool m_isInUpdateBeside;
        #endregion

        #region View related members
        private IndexBasedDynamicCollection<ViewRelatedSceneLayerSubset> m_viewSubsets;
        #endregion

        /// <summary>
        /// Creates a new SceneLayer object for the given scene.
        /// </summary>
        /// <param name="name">The name of the layer.</param>
        /// <param name="parentScene">Parent scene.</param>
        internal SceneLayer(string name, Scene parentScene)
        {
            m_name = name;
            m_scene = parentScene;

            //Create list holding all information for individual views
            m_viewSubsets = new IndexBasedDynamicCollection<ViewRelatedSceneLayerSubset>();

            //Create standard collections
            m_sceneObjects = new List<SceneObject>();
            m_sceneObjectsPublic = new ReadOnlyCollection<SceneObject>(m_sceneObjects);

            //Create specialized collections
            m_sceneObjectsSpacial = new List<SceneSpacialObject>(1024);
            m_sceneObjectsNotSpacial = new List<SceneObject>(1024);
            m_sceneObjectsNotStatic = new List<SceneObject>(1024);
            m_sceneObjectsForSingleUpdateCall = new Queue<SceneObject>(1024);

            this.AllowPick = true;
            this.IsRenderingEnabled = true;
            this.ClearDepthBufferAfterRendering = false;
        }

        /// <summary>
        /// Registers the given view on this layer.
        /// </summary>
        internal void RegisterView(int viewIndex, ViewInformation viewInformation, ResourceDictionary resourceDictionary)
        {
            ViewRelatedSceneLayerSubset newLayerViewSubset = new ViewRelatedSceneLayerSubset(this, viewInformation, resourceDictionary, viewIndex);

            m_viewSubsets.AddObject(
                newLayerViewSubset,
                viewIndex);

            newLayerViewSubset.RegisterObjectRange(m_sceneObjects.ToArray());
        }

        /// <summary>
        /// Copies local specialized collections into new array (only copies references..).
        /// </summary>
        /// <param name="spacialObjects">An array containing all spacial objects.</param>
        /// <param name="notSpacialObjects">An array containing all not spacial objects.</param>
        internal void GetSpecializedCollectionsCopy(out SceneSpacialObject[] spacialObjects, out SceneObject[] notSpacialObjects)
        {
            spacialObjects = m_sceneObjectsSpacial.ToArray();
            notSpacialObjects = m_sceneObjectsNotSpacial.ToArray();
        }

        /// <summary>
        /// Deregisters the given view on this layer.
        /// </summary>
        /// <param name="viewIndex">The index which this view has on the current scene.</param>
        /// <param name="viewInformation">The ViewInformation object describing the view.</param>
        internal void DeregisterView(int viewIndex, ViewInformation viewInformation)
        {
            // Dispose the layer subset (removes all its resources)
            ViewRelatedSceneLayerSubset viewSubset = m_viewSubsets[viewIndex];
            if (viewSubset != null)
            {
                viewSubset.ClearAllSubscriptions(m_sceneObjects);
                viewSubset.Dispose();
            }

            // Remote the subset
            m_viewSubsets.RemoveObject(viewIndex);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return "Layer " + m_name;
        }

        /// <summary>
        /// Adds the given object to the layer.
        /// </summary>
        /// <param name="sceneObject">Object to add.</param>
        internal bool AddObject(SceneObject sceneObject)
        {
            if (m_isInUpdate || m_isInUpdateBeside) { throw new InvalidOperationException("Unable to manipulate object list while SceneLayout is on updating!"); }
            if (sceneObject == null) { throw new ArgumentNullException("sceneObject"); }
            if (sceneObject.Scene == m_scene) { return false; }
            if (sceneObject.Scene != null) { throw new ArgumentException("Given object does already belong to another scene!", "sceneObject"); }
            if (sceneObject.SceneLayer == this) { return false; }
            if (sceneObject.SceneLayer != null) { throw new ArgumentException("Given object does already belong to another scene layer!", "sceneObject"); }

            m_sceneObjects.Add(sceneObject);
            sceneObject.SetSceneAndLayer(m_scene, this);

            //Append object to specialized collections
            SceneSpacialObject spacialObject = sceneObject as SceneSpacialObject;
            if (spacialObject != null)
            {
                m_sceneObjectsSpacial.Add(spacialObject);
            }
            else
            {
                m_sceneObjectsNotSpacial.Add(sceneObject);
            }

            // Handle static / non static objects
            if(sceneObject.IsStatic)
            {
                m_sceneObjectsForSingleUpdateCall.Enqueue(sceneObject);
            }
            else
            {
                m_sceneObjectsNotStatic.Add(sceneObject);
            }

            //Register the given object on all view subsets
            foreach (ViewRelatedSceneLayerSubset actViewSubset in m_viewSubsets)
            {
                actViewSubset.RegisterObjectRange(sceneObject);
            }

            return true;
        }

        /// <summary>
        /// Clears this layer.
        /// </summary>
        internal void ClearObjects()
        {
            // Clear objects on all view subsets
            foreach (ViewRelatedSceneLayerSubset actViewSubset in m_viewSubsets)
            {
                actViewSubset.ClearAllSubscriptions(m_sceneObjects);
            }

            foreach (SceneObject actObject in m_sceneObjects)
            {
                actObject.UnloadResources();
                actObject.ResetSceneAndLayer();
            }
            m_sceneObjects.Clear();

            // Clear specialized collections
            m_sceneObjectsNotSpacial.Clear();
            m_sceneObjectsSpacial.Clear();
            m_sceneObjectsNotStatic.Clear();
        }

        /// <summary>
        /// Clears all resources used by this layer.
        /// </summary>
        internal void ClearResources()
        {
            // Clear objects on all view subsets
            foreach (ViewRelatedSceneLayerSubset actViewSubset in m_viewSubsets)
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
            if (m_isInUpdate || m_isInUpdateBeside) { throw new InvalidOperationException("Unable to manipulate object list while SceneLayout is on updating!"); }

            if (m_sceneObjects.Contains(sceneObject))
            {
                sceneObject.UnloadResources();

                m_sceneObjects.Remove(sceneObject);
                sceneObject.ResetSceneAndLayer();

                // Remove object from specialized collections
                SceneSpacialObject spacialObject = sceneObject as SceneSpacialObject;
                if (spacialObject != null)
                {
                    m_sceneObjectsSpacial.Remove(spacialObject);
                }
                else
                {
                    m_sceneObjectsNotSpacial.Remove(sceneObject);
                }

                // Remove object form non-static collection
                if(!sceneObject.IsStatic)
                {
                    m_sceneObjectsNotStatic.Remove(sceneObject);
                }

                // Remove this object on all view subsets
                foreach (ViewRelatedSceneLayerSubset actViewSubset in m_viewSubsets)
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
            foreach (SceneObject actObject in m_sceneObjects)
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
            int sceneObjectArrayLength = m_sceneObjects.Count;
            SceneObject[] sceneObjectArray = m_sceneObjects.GetBackingArray();
            for (int loop = 0; loop < sceneObjectArrayLength; loop++)
            {
                SceneObject actObject = sceneObjectArray[loop];

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
                    GraphicsCore.PublishInternalExceptionInfo(
                        ex, InternalExceptionLocation.Loading3DObject);

                    //Build list of invalid objects
                    if (invalidObjects == null) { invalidObjects = new List<SceneObject>(); }
                    invalidObjects.Add(actObject);
                }
            }

            //Remove all invalid objects
            if (invalidObjects != null)
            {
                HandleInvalidObjects(invalidObjects);
            }
        }

        /// <summary>
        /// Updates the layer.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        internal void Update(SceneRelatedUpdateState updateState)
        {
            updateState.SceneLayer = this;
            m_isInUpdate = true;
            try
            {
                // Update all objects which are registered for initial update call
                int initialUpdateCallCount = m_sceneObjectsForSingleUpdateCall.Count;
                if(initialUpdateCallCount > 0)
                {
                    SceneObject[] initialUpdateCallItems = m_sceneObjectsForSingleUpdateCall.GetBackingArray();
                    for(int loop=0; loop<initialUpdateCallCount; loop++)
                    {
                        initialUpdateCallItems[loop].Update(updateState);
                    }
                }

                // Call default update method for each object
                int updateListLength = m_sceneObjectsNotStatic.Count;
                SceneObject[] updateList = m_sceneObjectsNotStatic.GetBackingArray();
                for(int actIndex = 0; actIndex < updateListLength; actIndex++)
                {
                    if (!updateList[actIndex].HasParent)
                    {
                        updateList[actIndex].Update(updateState);
                    }
                }

                // Call overall updates on all objects
                for (int loop = 0; loop < updateListLength; loop++)
                {
                    if (!updateList[loop].HasParent)
                    {
                        updateList[loop].UpdateOverall(updateState);
                    }
                }

                // Now update all view specific references
                foreach (var actViewSubset in m_viewSubsets)
                {
                    actViewSubset.UpdateForView(updateState);
                }
            }
            finally
            {
                updateState.SceneLayer = null;
                m_isInUpdate = false;
            }
        }

        /// <summary>
        /// Performs "update-beside-render" for this layer.
        /// </summary>
        /// <param name="updateState">Current update state.</param>
        internal void UpdateBesideRender(SceneRelatedUpdateState updateState)
        {
            updateState.SceneLayer = this;
            m_isInUpdateBeside = true;
            try
            {
                // Now update all view specific references
                foreach (var actViewSubset in m_viewSubsets)
                {
                    actViewSubset.UpdateBesideRender(updateState, m_sceneObjectsForSingleUpdateCall);
                }
            }
            finally
            {
                updateState.SceneLayer = null;
                m_isInUpdateBeside = false;

                // Some work after whole update processes
                m_sceneObjectsForSingleUpdateCall.Clear();
            }
        }

        /// <summary>
        /// Renders the scene to the given context.
        /// </summary>
        internal void Render(RenderState renderState)
        {
            if (!this.IsRenderingEnabled) { return; }

            //Delegate render call to corresponding view subset
            ViewRelatedSceneLayerSubset viewSubset = m_viewSubsets[renderState.ViewIndex];
            if (viewSubset != null)
            {
                m_viewSubsets[renderState.ViewIndex].Render(renderState);
            }
        }

        /// <summary>
        /// Renders the 2D overlay to the given context.
        /// </summary>
        /// <param name="renderState">State of the render.</param>
        internal void Render2DOverlay(RenderState renderState)
        {
            //Delegate render call to corresponding view subset
            ViewRelatedSceneLayerSubset viewSubset = m_viewSubsets[renderState.ViewIndex];
            if (viewSubset != null)
            {
                m_viewSubsets[renderState.ViewIndex].Render2DOverlay(renderState);
            }
        }

        /// <summary>
        /// Handles invalid objects.
        /// </summary>
        /// <param name="invalidObjects">List containing all invalid objects to handle.</param>
        private void HandleInvalidObjects(List<SceneObject> invalidObjects)
        {
            foreach (SceneObject actObject in invalidObjects)
            {
                //Unload the object if it is loaded
                try { actObject.UnloadResources(); }
                catch (Exception ex)
                {
                    GraphicsCore.PublishInternalExceptionInfo(
                        ex, InternalExceptionLocation.UnloadingInvalid3DObject);
                }

                //Remove this object from this layer
                this.RemoveObject(actObject);
            }
        }

        /// <summary>
        /// Gets the name of this layer.
        /// </summary>
        public string Name
        {
            get { return m_name; }
        }

        /// <summary>
        /// Gets parent scene.
        /// </summary>
        public Scene Scene
        {
            get { return m_scene; }
        }

        /// <summary>
        /// Gets or sets an integer which controls the order 
        /// </summary>
        public int OrderID
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
        public ReadOnlyCollection<SceneObject> Objects
        {
            get { return m_sceneObjectsPublic; }
        }

        /// <summary>
        /// Gets a list containing all scene objects (internal accessor to the complete list).
        /// </summary>
        internal List<SceneObject> ObjectsInternal
        {
            get { return m_sceneObjects; }
        }

        /// <summary>
        /// Gets a list containing all spacial objects.
        /// </summary>
        internal List<SceneSpacialObject> SpacialObjects
        {
            get { return m_sceneObjectsSpacial; }
        }

        /// <summary>
        /// Gets total count of objects within the scene.
        /// </summary>
        public int CountObjects
        {
            get { return m_sceneObjects.Count; }
        }
    }
}
