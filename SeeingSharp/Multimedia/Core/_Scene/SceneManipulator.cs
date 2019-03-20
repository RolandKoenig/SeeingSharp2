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
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Drawing2D;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using SharpDX;

namespace SeeingSharp.Multimedia.Core
{
    public class SceneManipulator
    {
        // Objects that remember all changes on object/resource collections
        private List<SceneObject> m_createdObjects;
        private List<NamedOrGenericKey> m_createdResources;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneManipulator"/> class.
        /// </summary>
        /// <param name="owner">The scene which will be manipulated.</param>
        internal SceneManipulator(Scene owner)
        {
            this.Owner = owner;
            this.IsValid = false;

            m_createdObjects = new List<SceneObject>();
            m_createdResources = new List<NamedOrGenericKey>();
        }

        /// <summary>
        /// Resets all object and resource collections managed locally by
        /// this manipulator object (e. g. property CreatedObjects).
        /// </summary>
        public void ResetObjectAndResourceCollections()
        {
            m_createdObjects.Clear();
            m_createdResources.Clear();
        }

        /// <summary>
        /// Clears the scene.
        /// </summary>
        public void Clear()
        {
            this.CheckValid();

            this.Owner.Clear(true);
        }

        /// <summary>
        /// Clears the scene.
        /// </summary>
        /// <param name="clearResource">Do also clear all resources.</param>
        public void Clear(bool clearResource)
        {
            this.CheckValid();

            this.Owner.Clear(clearResource);
        }

        /// <summary>
        /// Adds a resource to the scene
        /// </summary>
        /// <typeparam name="ResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The factory method which creates the resource object.</param>
        /// <returns></returns>
        public NamedOrGenericKey AddResource<ResourceType>(Func<ResourceType> resourceFactory)
            where ResourceType : Resource
        {
            this.CheckValid();

            var result = this.Owner.AddResource(resourceFactory, NamedOrGenericKey.Empty);
            m_createdResources.Add(result);
            return result;
        }

        /// <summary>
        /// Adds a resource to the scene
        /// </summary>
        /// <typeparam name="ResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The factory method which creates the resource object.</param>
        /// <param name="resourceKey">The key for the newly generated resource.</param>
        /// <returns></returns>
        public NamedOrGenericKey AddResource<ResourceType>(Func<ResourceType> resourceFactory, NamedOrGenericKey resourceKey)
            where ResourceType : Resource
        {
            this.CheckValid();

            var result = this.Owner.AddResource(resourceFactory, resourceKey);
            m_createdResources.Add(result);
            return result;
        }

        /// <summary>
        /// Is there already a resource with the given key?
        /// </summary>
        /// <param name="resourceKey">The resource key to check for.</param>
        public bool ContainsResource(NamedOrGenericKey resourceKey)
        {
            this.CheckValid();

            return this.Owner.ContainsResource(resourceKey);
        }

        /// <summary>
        /// Adds the given resource if it was not created before.
        /// </summary>
        /// <typeparam name="ResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The resource factory.</param>
        /// <param name="resourceKey">The resource key.</param>
        public NamedOrGenericKey AddResourceIfNotCreated<ResourceType>(Func<ResourceType> resourceFactory, NamedOrGenericKey resourceKey)
            where ResourceType : Resource
        {
            this.CheckValid();

            if (!this.Owner.ContainsResource(resourceKey))
            {
                var result = this.Owner.AddResource(resourceFactory, resourceKey);
                m_createdResources.Add(result);
                return result;
            }
            return resourceKey;
        }

        /// <summary>
        /// Manipulates the resource with the given key.
        /// </summary>
        /// <typeparam name="ResourceType">The type of the resource.</typeparam>
        /// <param name="manipulateAction">The action that manipulates the resource.</param>
        /// <param name="resourceKey">The key of the resource to be manipulated.</param>
        public void ManipulateResource<ResourceType>(Action<ResourceType> manipulateAction, NamedOrGenericKey resourceKey)
            where ResourceType : Resource
        {
            this.CheckValid();

            this.Owner.ManipulateResource(manipulateAction, resourceKey);
        }

        /// <summary>
        /// Removes the resource with the given key.
        /// </summary>
        /// <param name="resourceKey">The key of the resource to be deleted.</param>
        public void RemoveResource(NamedOrGenericKey resourceKey)
        {
            this.CheckValid();

            this.Owner.RemoveResource(resourceKey);
        }

        /// <summary>
        /// Triggers new filter logic for the given scene object.
        /// </summary>
        /// <param name="sceneObjectLocal">The object to trigger filter logic for.</param>
        public void TriggerNewFilter(SceneObject sceneObjectLocal)
        {
            this.CheckValid();

            this.Owner.TriggerNewFilter(sceneObjectLocal);
        }

        /// <summary>
        /// Adds the given drawing layer.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        public void AddDrawingLayer(Custom2DDrawingLayer drawingLayer)
        {
            this.CheckValid();

            this.Owner.AddDrawingLayer(drawingLayer);
        }

        /// <summary>
        /// Adds the given drawing layer.
        /// </summary>
        /// <param name="drawingAction">The action that draws the scene.</param>
        public void AddDrawingLayer(Action<Graphics2D> drawingAction)
        {
            this.AddDrawingLayer(new Custom2DDrawingLayer(drawingAction));
        }

        /// <summary>
        /// Removes the given drawing layer.
        /// </summary>
        /// <param name="drawingLayer">The drawing layer.</param>
        public void RemoveDrawingLayer(Custom2DDrawingLayer drawingLayer)
        {
            this.CheckValid();

            this.Owner.RemoveDrawingLayer(drawingLayer);
        }

        /// <summary>
        /// Adds the given object to the scene.
        /// </summary>
        /// <param name="sceneObject">Object to add.</param>
        public T Add<T>(T sceneObject)
            where T : SceneObject
        {
            this.CheckValid();

            var result = this.Owner.Add(sceneObject, Scene.DEFAULT_LAYER_NAME);
            m_createdObjects.Add(result);
            return result;
        }

        /// <summary>
        /// Adds the given object to the scene.
        /// </summary>
        /// <param name="sceneObject">Object to add.</param>
        /// <param name="layer">Layer on which the object should be added.</param>
        public T Add<T>(T sceneObject, string layer)
            where T : SceneObject
        {
            this.CheckValid();

            var result = this.Owner.Add(sceneObject, layer);
            m_createdObjects.Add(result);
            return result;
        }

        /// <summary>
        /// Adds the given object as a child.
        /// </summary>
        /// <param name="parent">The object to which to add the child.</param>
        /// <param name="childToAdd">The object which is be be located under this one within object hierarchy.</param>
        public void AddChild(SceneObject parent, SceneObject childToAdd)
        {
            parent.EnsureNotNull(nameof(parent));
            childToAdd.EnsureNotNull(nameof(childToAdd));

            parent.AddChildInternal(childToAdd);
        }

        /// <summary>
        /// Adds a text object displaying the given text.
        /// </summary>
        /// <param name="textToDisplay">The text to be displayed.</param>
        /// <param name="textOptions">All options regarding the text geometry generator.</param>
        /// <param name="realignToCenter">Moves all vertices so that the center is 0.</param>
        /// <param name="layer">The layer on which to add the object.</param>
        public GenericObject Add3DText(string textToDisplay, TextGeometryOptions textOptions, bool realignToCenter = false, string layer = Scene.DEFAULT_LAYER_NAME)
        {
            var newGeometry = new Geometry();
            newGeometry.FirstSurface.BuildTextGeometry(
                textToDisplay,
                textOptions);

            if (realignToCenter)
            {
                newGeometry.RealignToCenter();
            }

            var resTextGeometry = this.AddResource(() => new GeometryResource(newGeometry));
            return this.AddGeneric(resTextGeometry, layer);
        }

        /// <summary>
        /// Adds a new generic object targeting to the given geometry resource.
        /// </summary>
        /// <param name="geometryResource">The geometry to be used.</param>
        public GenericObject AddGeneric(NamedOrGenericKey geometryResource)
        {
            return this.Add(new GenericObject(geometryResource));
        }

        /// <summary>
        /// Adds a new generic object targeting to the given geometry resource.
        /// </summary>
        /// <param name="geometryResource">The geometry to be used.</param>
        /// <param name="layer">The layer on which to add the object.</param>
        public GenericObject AddGeneric(NamedOrGenericKey geometryResource, string layer)
        {
            return this.Add(new GenericObject(geometryResource), layer);
        }

        /// <summary>
        /// Adds a new generic object targeting to the given geometry resource.
        /// </summary>
        /// <param name="geometryResource">The geometry to be used.</param>
        /// <param name="position">The position for the created object.</param>
        public GenericObject AddGeneric(NamedOrGenericKey geometryResource, Vector3 position)
        {
            var newGenericObject = new GenericObject(geometryResource)
            {
                Position = position
            };

            return this.Add(newGenericObject);
        }

        /// <summary>
        /// Adds a new generic object targeting to the given geometry resource.
        /// </summary>
        /// <param name="geometryResource">The geometry to be used.</param>
        /// <param name="position">The position for the created object.</param>
        /// <param name="layer">The layer on which to add the object.</param>
        public GenericObject AddGeneric(NamedOrGenericKey geometryResource, Vector3 position, string layer)
        {
            var newGenericObject = new GenericObject(geometryResource)
            {
                Position = position
            };

            return this.Add(newGenericObject, layer);
        }

        /// <summary>
        /// Adds all given scene objects.
        /// </summary>
        /// <param name="sceneObjects">All objects to add.</param>
        public IEnumerable<SceneObject> AddRange(IEnumerable<SceneObject> sceneObjects)
        {
            foreach (var actObject in sceneObjects)
            {
                this.Add(actObject);
            }

            return sceneObjects;
        }

        /// <summary>
        /// Adds all given scene objects.
        /// </summary>
        /// <param name="sceneObjects">All objects to add.</param>
        /// <param name="layer">Layer on wich the objects should be added.</param>
        public IEnumerable<SceneObject> AddRange(IEnumerable<SceneObject> sceneObjects, string layer)
        {
            foreach (var actObject in sceneObjects)
            {
                this.Add(actObject, layer);
            }

            return sceneObjects;
        }

        /// <summary>
        /// Removes the given object from the scene.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        public void Remove(SceneObject sceneObject)
        {
            this.CheckValid();

            this.Owner.Remove(sceneObject);
        }

        /// <summary>
        /// Removes the given object from the scene.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        /// <param name="layerName">Layer on which the scene object was added.</param>
        public void Remove(SceneObject sceneObject, string layerName)
        {
            this.CheckValid();

            this.Owner.Remove(sceneObject, layerName);
        }

        public void RemoveRange(IEnumerable<SceneObject> sceneObjects)
        {
            this.CheckValid();

            this.Owner.RemoveRange(sceneObjects);
        }

        public void RemoveRange(IEnumerable<SceneObject> sceneObjects, string layerName)
        {
            this.CheckValid();

            this.Owner.RemoveRange(sceneObjects, layerName);
        }

        /// <summary>
        /// Adds a new layer with the given name.
        /// </summary>
        /// <param name="name">Name of the layer.</param>
        public SceneLayer AddLayer(string name)
        {
            this.CheckValid();

            return this.Owner.AddLayer(name);
        }

        /// <summary>
        /// Removes the layer with the given name.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        public void RemoveLayer(string layerName)
        {
            this.CheckValid();

            this.Owner.RemoveLayer(layerName);
        }

        /// <summary>
        /// Removes the given layer from the scene.
        /// </summary>
        /// <param name="layer">Layer to remove.</param>
        public void RemoveLayer(SceneLayer layer)
        {
            this.CheckValid();

            this.Owner.RemoveLayer(layer);
        }

        /// <summary>
        /// Sets the order id of the given layer.
        /// </summary>
        /// <param name="layer">The name of the layer.</param>
        /// <param name="oderID">The order id to set.</param>
        public void SetLayerOrderID(string layer, int oderID)
        {
            this.CheckValid();

            var layerObject = this.Owner.GetLayer(layer);
            this.Owner.SetLayerOrderID(layerObject, oderID);
        }

        /// <summary>
        /// Sets the order id of the given layer.
        /// </summary>
        /// <param name="layerObject">The layer object.</param>
        /// <param name="oderID">The order id to set.</param>
        public void SetLayerOrderID(SceneLayer layerObject, int oderID)
        {
            this.CheckValid();

            this.Owner.SetLayerOrderID(layerObject, oderID);
        }

        /// <summary>
        /// Clears the layer with the given name.
        /// </summary>
        /// <param name="layerName">The name of the layer.</param>
        public void ClearLayer(string layerName)
        {
            this.CheckValid();

            this.Owner.ClearLayer(layerName);
        }

        /// <summary>
        /// Clears the given layer.
        /// </summary>
        /// <param name="layer">The layer to be cleared.</param>
        public void ClearLayer(SceneLayer layer)
        {
            this.CheckValid();

            this.Owner.ClearLayer(layer);
        }

        /// <summary>
        /// Gets all objects of the given layer.
        /// </summary>
        /// <param name="layerName">The name of the layer.</param>
        public IEnumerable<SceneObject> GetSceneObjects(string layerName)
        {
            return this.Owner.GetLayer(layerName).Objects;
        }

        /// <summary>
        /// Gets the layer with the given name.
        /// </summary>
        /// <param name="layerName">Name of the layer.</param>
        public SceneLayer GetLayer(string layerName)
        {
            this.CheckValid();
            return this.Owner.GetLayer(layerName);
        }

        public bool ContainsLayer(string layerName)
        {
            this.CheckValid();
            return this.Owner.TryGetLayer(layerName) != null;
        }

        public SceneLayer TryGetLayer(string layerName)
        {
            this.CheckValid();
            return this.Owner.TryGetLayer(layerName);
        }

        /// <summary>
        /// Removes the given object from the list of children.
        /// </summary>
        /// <param name="parent">The object from which to remove the child.</param>
        /// <param name="childToRemove">The object which is to be removed from the list of children.</param>
        internal void RemoveChild(SceneObject parent, SceneObject childToRemove)
        {
            parent.EnsureNotNull(nameof(parent));
            childToRemove.EnsureNotNull(nameof(childToRemove));

            parent.RemoveChildInternal(childToRemove);
        }

        /// <summary>
        /// Queries for all children (also lower level).
        /// </summary>
        internal IEnumerable<SceneObject> GetAllChildren(SceneObject sceneObject)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));

            return sceneObject.GetAllChildrenInternal();
        }

        /// <summary>
        /// Checks whether this manipulator is still valid.
        /// </summary>
        private void CheckValid()
        {
            if (!this.IsValid) { throw new SeeingSharpGraphicsException("This scene manipulator is not valid currently!"); }
        }

        /// <summary>
        /// Gets the owner of this manipulator object.
        /// </summary>
        public Scene Owner { get; }

        /// <summary>
        /// Is this manipulator still valid?
        /// </summary>
        public bool IsValid { get; internal set; }

        /// <summary>
        /// Gets a list containing all created objects.
        /// </summary>
        public IEnumerable<SceneObject> CreatedObjects => m_createdObjects;

        public int CreatedObjectsCount => m_createdObjects.Count;

        /// <summary>
        /// Gets a list containing all created resources.
        /// </summary>
        public IEnumerable<NamedOrGenericKey> CreatedResources => m_createdResources;

        public int CreatedResourcesCount => m_createdResources.Count;
    }
}