[Home](README.md)

## Handling input in general
You are free to use default input handling methods of the ui framework of your choice - just as you would do without SeeingSharp. This is no problem because SeeingSharp does not block this in any way.

SeeingSharp also offers an input api which acts as an abstraction layer between the supported ui frameworks. Instead of offering input events, SeeingSharp provides a stream of InputFrames. Each InputFrame describes what input came in on the associated view. The actual input per InputFrame is defined by a collection of InputState objects. An InputState object can be one of the following.

|State type            |Description
|:---------------------|:----------------
|KeyboardState         |Here you can check which keys are down or hit.
|MouseOrPointerState   |Here you can check the state of the user's mouse or pointer. The state is defined by the position and button states.
|GamepadState          |Here you can check the state of the user's gamepad. This state is based on Microsofts DirectInput api.

**But be carefull**: InputFrame objects will be reused by SeeingSharp regularly, so do not cache them yourself. 

## Capture InputFrames globally
To perform custom logic based on InputFrames you can use the GraphicsCore.Current.MainLoop.GenericInput event. All InputFrames will flow through this event. 

## Capture InputFrames on a Custom2DDrawingLayer
You can also capture InputFrames inside a Custom2DDrawingLayer. The best option is to derive from Custom2DDrawingLayer and override the Update method. See following short sample for handling keyboard input states inside an overriden Update method.

```csharp
protected override void Update(UpdateState updateState)
{
    base.Update(updateState);

    updateState.HandleKeyboardInput((keyboardState) =>
    {
        if (keyboardState.IsKeyHit(WinVirtualKey.A))
        {
            // Some logic when user hit A
        }
    }
}
```

## Capture InputFrames on a SceneObject
This is just the same as for Custom2DRawingLayouer. You must derive from one on the SceneObject classes (like Mesh) and override the Update method.