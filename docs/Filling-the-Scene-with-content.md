[Home](README.md)

## Simple sample
As described in the previous article, following snipped can be used to add a simple cube to the scene. To find out more about this sample, then jump back to [Views, Scenes and Cameras](https://github.com/RolandKoenig/SeeingSharp2/wiki/Views,-Scenes-and-Cameras)
```csharp
await view.Scene.ManipulateSceneAsync(manipulator =>
{
    var resGeometry = manipulator.AddResource(
        device => new GeometryResource(new CubeGeometryFactory()));
    var resMaterial = manipulator.AddStandardMaterialResource();

    var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
    newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
    newMesh.Scaling = new Vector3(2f, 2f, 2f);
    newMesh.Color = Color4Ex.RedColor;
});
```
## Resources and Objects
### Add resources
Lets look into the above sample in more detail. In the first two lines we are creating the needed resources like this:
```csharp
var resGeometry = manipulator.AddResource(
    device => new GeometryResource(new CubeGeometryFactory()));
var resMaterial = manipulator.AddStandardMaterialResource();
```
Both methods are helper methods and are wrapping the following:
```csharp
var resGeometry = manipulator.AddResource(
    device => new GeometryResource(new CubeGeometryFactory()));
var resMaterial = manipulator.AddResource(
    device => new StandardMaterialResource());
```
Looks like to be very simple, we create resource objects and are adding them to the scene. But wait.. we are adding factory methods to the scene, not the resource objects themselves. This is because the resource objects will be created per rendering device. On each machine you have at least two different rendering devices: A software device based on [WARP](https://docs.microsoft.com/en-us/windows/win32/direct3darticles/directx-warp) and a true hardware device. You may have additional hardware devices for example when you have a notebook with integrated and dedicated graphics hardware. Or you are using an external graphics card connected to your machine through Thunderbold (this is my case currently when I'am working at home).

Note: Resources are loaded as needed for rendering. If you never render the scene on another rendering device, then no resources will be loaded on that device.

## Resource types
### Base types
Now lets look at what kind of resources we have. All resource types are derived from the Resource class. So far, the following base types are the most important when working with Seeing#:

|Base type                 |Description                                                                                   |
|:-------------------------|:---------------------------------------------------------------------------------------------|
|GeometryResource          |Contains all geometry of an object (surfaces, triangles, etc.)                                |
|MaterialResource          |It controls how a geometry gets rendered                                                    |
|TextureResource           |A texture is an image which can be rendered on a geometry when it is referenced by a material |
|PostprocessEffectResource |This one controls a postprocessing effect which is applied on rendering a SceneLayer|

### Materials
The MaterialResource type is abstract. The following resource types are derived from it:
* **StandardMaterialResource**: The most default material type. Use this one if you want to render the Geometry using vertex colors or assigned TextureResource(s)
* **SingleForcedColorMaterialResource**: A more special case. This one forces a given color to be used for each pixel rendered

### Textures
The TextureResource type is abstract. The following resource types are derived from it:
* **StandardTextureResource**: The most default texture type. Use this one if you have an external texture file (e. g. a bitmap) or an inmemory block of pixel data
* **Direct2DOneTimeRenderTextureResource**: Use this one if you want to draw the texture yourself. The rendering method is called one time when the texture resources gets loaded
* **Direct2DTextureResource**: Use this one if you want to draw the texture yourself. The rendering method is called on each frame

## Resource keys
Looking back to the definition of resources we have one more thing to mention. Look at following code:
```csharp
var resGeometry = manipulator.AddResource(
    device => new GeometryResource(new CubeGeometryFactory()));
var resMaterial = manipulator.AddResource(
    device => new StandardMaterialResource());
```
This is equivalent to:
```csharp
NamedOrGenericKey resGeometry = manipulator.AddResource(
    device => new GeometryResource(new CubeGeometryFactory()));
NamedOrGenericKey resMaterial = manipulator.AddResource(
    device => new StandardMaterialResource());
```
Right, we don't get a resource object back when adding it. Instead, a NamedOrGenericKey is returned. Seeing# uses this structure to reference resources. As the type name says the key itself can be named (a string) or can be generic (see GraphicsCore.GetNextGenericResourceKey). A generic key is generated by default.

So why we get a key and not the resource object itself? The reason is simply the fact, that Seeing# can render on any graphics device on the machine. The resource object itself is generated per device by need. If you have more views which use different devices, then you have more than one resource objects for the same resource and key. 

## Add objects
Now look to the second part of the sample:
```csharp
var newMesh = manipulator.AddMeshObject(resGeometry, resMaterial);
newMesh.RotationEuler = new Vector3(0f, EngineMath.RAD_90DEG / 2f, 0f);
newMesh.Scaling = new Vector3(2f, 2f, 2f);
newMesh.Color = Color4Ex.RedColor;
```
Here we create a Mesh. This one defines the actual 3D object in the scene. It references the geometry and the material(s). The method manipulator.AddMesh is a helper method and equivalent to following code:
```csharp
var newMesh = new Mesh(resGeometry, resMaterial);
manipulator.AddObject(newMesh);
```
The Mesh object holds additional properties like Position, Scaling, Color, etc. Use them to configure where the object is, how big it is and so on. You are also free to create more Meshes referencing the same geometry and material