using System;
using System.Collections.Generic;
using SeeingSharp.Checking;
using SeeingSharp.Core.Devices;
using SeeingSharp.Drawing2D;
using SeeingSharp.Drawing3D;
using SeeingSharp.Drawing3D.Geometries;
using SeeingSharp.Drawing3D.Resources;
using SeeingSharp.Util;

namespace SeeingSharp.Core
{
    public class SceneManipulator
    {
        // Objects that remember all changes on object/resource collections
        private List<SceneObject> _createdObjects;
        private List<NamedOrGenericKey> _createdResources;

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
        public IEnumerable<SceneObject> CreatedObjects => _createdObjects;

        public int CreatedObjectsCount => _createdObjects.Count;

        /// <summary>
        /// Gets a list containing all created resources.
        /// </summary>
        public IEnumerable<NamedOrGenericKey> CreatedResources => _createdResources;

        public int CreatedResourcesCount => _createdResources.Count;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneManipulator"/> class.
        /// </summary>
        /// <param name="owner">The scene which will be manipulated.</param>
        internal SceneManipulator(Scene owner)
        {
            this.Owner = owner;
            this.IsValid = false;

            _createdObjects = new List<SceneObject>();
            _createdResources = new List<NamedOrGenericKey>();
        }

        /// <summary>
        /// Resets all object and resource collections managed locally by
        /// this manipulator object (e. g. property CreatedObjects).
        /// </summary>
        public void ResetObjectAndResourceCollections()
        {
            _createdObjects.Clear();
            _createdResources.Clear();
        }

        /// <summary>
        /// Clears the scene together with all its resources.
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
        /// <typeparam name="TResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The factory method which creates the resource object.</param>
        /// <returns></returns>
        public NamedOrGenericKey AddResource<TResourceType>(Func<EngineDevice, TResourceType> resourceFactory)
            where TResourceType : Resource
        {
            this.CheckValid();

            var result = this.Owner.AddResource(resourceFactory, NamedOrGenericKey.Empty);
            _createdResources.Add(result);
            return result;
        }

        /// <summary>
        /// Adds a resource to the scene
        /// </summary>
        /// <typeparam name="TResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The factory method which creates the resource object.</param>
        /// <param name="resourceKey">The key for the newly generated resource.</param>
        public NamedOrGenericKey AddResource<TResourceType>(Func<EngineDevice, TResourceType> resourceFactory, NamedOrGenericKey resourceKey)
            where TResourceType : Resource
        {
            this.CheckValid();

            var result = this.Owner.AddResource(resourceFactory, resourceKey);
            _createdResources.Add(result);
            return result;
        }

        /// <summary>
        /// Queries all resources of the given resource key.
        /// </summary>
        /// <param name="resourceKey">The key of the resource to get all resource instances for.</param>
        public IEnumerable<T> QueryResources<T>(NamedOrGenericKey resourceKey)
            where T : Resource
        {
            this.CheckValid();

            return this.Owner.QueryResources<T>(resourceKey, this.CheckValid);
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
        /// <typeparam name="TResourceType">The type of the resource.</typeparam>
        /// <param name="resourceFactory">The resource factory.</param>
        /// <param name="resourceKey">The resource key.</param>
        public NamedOrGenericKey AddResourceIfNotCreated<TResourceType>(Func<EngineDevice, TResourceType> resourceFactory, NamedOrGenericKey resourceKey)
            where TResourceType : Resource
        {
            this.CheckValid();

            if (!this.Owner.ContainsResource(resourceKey))
            {
                var result = this.Owner.AddResource(resourceFactory, resourceKey);
                _createdResources.Add(result);
                return result;
            }
            return resourceKey;
        }

        /// <summary>
        /// Manipulates the resource with the given key.
        /// </summary>
        /// <typeparam name="TResourceType">The type of the resource.</typeparam>
        /// <param name="manipulateAction">The action that manipulates the resource.</param>
        /// <param name="resourceKey">The key of the resource to be manipulated.</param>
        public void ManipulateResource<TResourceType>(Action<TResourceType> manipulateAction, NamedOrGenericKey resourceKey)
            where TResourceType : Resource
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
        public T AddObject<T>(T sceneObject)
            where T : SceneObject
        {
            this.CheckValid();

            var result = this.Owner.Add(sceneObject, Scene.DEFAULT_LAYER_NAME);
            _createdObjects.Add(result);
            return result;
        }

        /// <summary>
        /// Adds the given object to the scene.
        /// </summary>
        /// <param name="sceneObject">Object to add.</param>
        /// <param name="layer">Layer on which the object should be added.</param>
        public T AddObject<T>(T sceneObject, string layer)
            where T : SceneObject
        {
            this.CheckValid();

            var result = this.Owner.Add(sceneObject, layer);
            _createdObjects.Add(result);
            return result;
        }

        /// <summary>
        /// Adds the given object as a child.
        /// </summary>
        /// <param name="parent">The object to which to add the child.</param>
        /// <param name="childToAdd">The object which is be be located under this one within object hierarchy.</param>
        public void AddChildObject(SceneObject parent, SceneObject childToAdd)
        {
            parent.EnsureNotNull(nameof(parent));
            childToAdd.EnsureNotNull(nameof(childToAdd));

            parent.AddChildInternal(childToAdd);
        }

        /// <summary>
        /// Adds a text geometry resource.
        /// </summary>
        /// <param name="textToDisplay">The text to be displayed.</param>
        /// <param name="textOptions">All options regarding the text geometry generator.</param>
        /// <param name="realignToCenter">Moves all vertices so that the center is 0.</param>
        public NamedOrGenericKey Add3DTextGeometryResource(string textToDisplay, TextGeometryOptions textOptions, bool realignToCenter = false)
        {
            var newGeometry = new Geometry();
            newGeometry.FirstSurface.BuildTextGeometry(
                textToDisplay,
                textOptions);
            if (realignToCenter) { newGeometry.RealignToCenter(); }

            return this.AddResource(device => new GeometryResource(newGeometry));
        }

        /// <summary>
        /// Adds a new mesh to the scene.
        /// </summary>
        /// <param name="resGeometry">The geometry.</param>
        /// <param name="resMaterials">All materials to be mapped to the geometry.</param>
        public Mesh AddMeshObject(NamedOrGenericKey resGeometry, params NamedOrGenericKey[] resMaterials)
        {
            return this.AddObject(new Mesh(resGeometry, resMaterials));
        }

        /// <summary>
        /// Adds a new mesh to the scene.
        /// </summary>
        /// <param name="resGeometry">The geometry.</param>
        /// <param name="layer">The layer on which to add the object.</param>
        /// <param name="resMaterials">All materials to be mapped to the geometry.</param>
        public Mesh AddMeshObject(NamedOrGenericKey resGeometry, string layer, params NamedOrGenericKey[] resMaterials)
        {
            return this.AddObject(new Mesh(resGeometry, resMaterials), layer);
        }

        /// <summary>
        /// Adds all given scene objects.
        /// </summary>
        /// <param name="sceneObjects">All objects to add.</param>
        /// <param name="layer">Layer on wich the objects should be added.</param>
        public IEnumerable<SceneObject> AddObjectRange(IEnumerable<SceneObject> sceneObjects, string layer)
        {
            foreach (var actObject in sceneObjects)
            {
                this.AddObject(actObject, layer);
            }

            return sceneObjects;
        }

        /// <summary>
        /// Removes the given object from the scene.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        public void RemoveObject(SceneObject sceneObject)
        {
            this.CheckValid();

            this.Owner.Remove(sceneObject);
        }

        /// <summary>
        /// Removes the given object from the scene.
        /// </summary>
        /// <param name="sceneObject">Object to remove.</param>
        /// <param name="layerName">Layer on which the scene object was added.</param>
        public void RemoveObject(SceneObject sceneObject, string layerName)
        {
            this.CheckValid();

            this.Owner.Remove(sceneObject, layerName);
        }

        public void RemoveObjectRange(IEnumerable<SceneObject> sceneObjects)
        {
            this.CheckValid();

            this.Owner.RemoveRange(sceneObjects);
        }

        public void RemoveObjectRange(IEnumerable<SceneObject> sceneObjects, string layerName)
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
        /// <param name="oderId">The order id to set.</param>
        public void SetLayerOrderId(string layer, int oderId)
        {
            this.CheckValid();

            var layerObject = this.Owner.GetLayer(layer);
            this.Owner.SetLayerOrderId(layerObject, oderId);
        }

        /// <summary>
        /// Sets the order id of the given layer.
        /// </summary>
        /// <param name="layerObject">The layer object.</param>
        /// <param name="oderId">The order id to set.</param>
        public void SetLayerOrderId(SceneLayer layerObject, int oderId)
        {
            this.CheckValid();

            this.Owner.SetLayerOrderId(layerObject, oderId);
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
        public IEnumerable<SceneObject> GetObjects(string layerName)
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
        internal void RemoveChildObject(SceneObject parent, SceneObject childToRemove)
        {
            parent.EnsureNotNull(nameof(parent));
            childToRemove.EnsureNotNull(nameof(childToRemove));

            parent.RemoveChildInternal(childToRemove);
        }

        /// <summary>
        /// Queries for all children (also lower level).
        /// </summary>
        /// <param name="sceneObject">The object for which to return all children.</param>
        /// <param name="queryDeep">Do also return lower level children?</param>
        internal IEnumerable<SceneObject> GetAllChildObjects(SceneObject sceneObject, bool queryDeep)
        {
            sceneObject.EnsureNotNull(nameof(sceneObject));

            return sceneObject.GetAllChildren(queryDeep);
        }

        /// <summary>
        /// Checks whether this manipulator is still valid.
        /// </summary>
        private void CheckValid()
        {
            if (!this.IsValid) { throw new SeeingSharpGraphicsException("This scene manipulator is not valid currently!"); }
        }
    }
}