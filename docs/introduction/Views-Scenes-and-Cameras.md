[Home](../README.md)

## Views
A view in Seeing# is an area, where Seeing# renders a Scene to. Normally a view is defined by a Control in one of the UI libraries of .Net. See the following table for the names of the Controls per UI library. Additionally there exists another type of view without any binding to any UI library: The MemoryRenderTarget type. This one is meant for situations without UI, such as UnitTesting, rendering a video or server side rendering for a web page. You can use these types just as you expect to use any other Control in the corresponding UI library. 


|UI library|Control                                                 |
|:---------|:-------------------------------------------------------|
|WPF       |SeeingSharp.Multimedia.Views.SeeingSharpRendererElement |
|WinForms  |SeeingSharp.Multimedia.Views.SeeingSharpRendererControl |
|UWP       |SeeingSharp.Multimedia.Views.SeeingSharpRenderPanel     |
|-         |SeeingSharp.Multimedia.Views.MemoryRenderTarget         |

Each of these types has a member of the type RenderLoop (the property is named just like that). The RenderLoop type contains all common functionality across all view types. So if you need to further interact with rendering logic itself, the property RenderLoop of the view's object is a good entry point for that. 

## Scenes
A scene in Seeing# is a collection of objects to be rendered and their hardware resources which they need for rendering. Seeing# can handle as much scenes are needed for the application. A Scene object is created by each view by default but you are free to change the scene to which a view is bound to. Manipulating a scene is easy by using the corresponding Add and Remove methods. But: All manipulations on the scene itself have to be done asynchronous as shown in the following code-snipped.

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

The reason behind this is the asynchronous nature of Seeing#. All rendering and other calculations are done using threads from the ThreadPool. Therefore you can only change the scene's internal collections in defined time slots. The simplest way to do this is by using the method ManipulateSceneAsync of the scene where the given manipulator can perform all manipulations needed. 

Note: When you add a Resource with the AddResource method then you have to pass a delegate. This is because this delegate gets invoked per rendering device which want to access this resource.

## Cameras
In Seeing# you have access to a PerspectiveCamera or an OrthographicCamera. An instance of the PerspectiveCamera type is created by default for each view you create but you are free to change the camera by the Camera property of a view. Seeing# uses the left handed coordinate system.