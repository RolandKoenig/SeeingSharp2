[Home](README.md)

## Object filtering
Object visibility is controlled by object filters by default. You can add one or more object filters per view. Each object inside the scene has to pass each object filter before it gets rendered for the corresponding view. The most common example of an object filter is the SceneViewboxObjectFilter. This one builds a box (or pyramid, depending on the camera's projection node) representing the area which the view sees. All objects in the scene are testet against this box - if one is inside the box or on the border, then it will be rendered.

An object filter can be added to a view simply by the following code. Note that the variable _renderLoop represents the RenderLoop object, which you can access from each view control (property RenderLoop).
```csharp
_renderLoop.ObjectFilters.Add(new SceneViewboxObjectFilter());
``` 

You can also write a custom object filter by deriving from SceneObjectFilter. A very simple implementation may look like the following code. This filter says that each object is visible. The more interesting part is the property UpdateEachFrame. This one controls whether visibility should be testet on each frame - independent on other changes. If you return false here (default), then visibility is only testet if something changed (object added, transformation changed, filter changed, ...) and the result value gets cached. The SceneViewboxObjectFilter returns true on the UpdateEachFrame property because it has to react on each possible changes like camera movement, view resize, ...
```csharp
public class CustomObjectFilter : SceneObjectFilter
{
    /// <inheritdoc />
    public override bool IsObjectVisible(SceneObject input, ViewInformation viewInformation)
    {
        return true;
    }

    /// <inheritdoc />
    public override bool UpdateEachFrame => false;
}
```

It is important for performance in which order you add object filters to the view. If an object is not visible on one filter, then the following will not be executed for that object. SeeingSharp also caches visibility results for each filter separately. This means that if the last object filter has the UpdateEachFrame property set to true, then ony this one gets executed per frame (not prev. filters, because results are cached).


## Explicit method
You can control object's visiblity directly by the property VisibilityTestMethod of SceneObject. The default value is VisibilityTestMethod.ByObjectFilters. Other possibilities are VisibilityTestMethod.ForceHidden and VisibilityTestMethod.ForceVisible. Therefore, you can bypass object filter logic with the following line of code.
```csharp
_mesh.VisibilityTestMethod = VisibilityTestMethod.ForceHidden;
```
This line sets the object to be hidden. Object filters are not executed for this until you reset it to VisibilityTestMethod.ByObjectFilters.

