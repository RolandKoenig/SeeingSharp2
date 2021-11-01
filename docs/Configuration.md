## Overview
There are 4 objects which control most of the configuration of Seeing#:
* **SeeingSharpLoadSettings**: Configured on load time when GraphicsCore.Loader.Load() is called
* **GraphicsCoreConfiguration**: Contains flags which affect all of SeeingSharp. It is accessible through GraphicsCore.Current.Configuration
* **GraphicsDeviceConfiguration**: Controls some properties of devices, e. g. in which detail level they load geometry resources. It is accessible through the Configuration property of the EngineDevice class
* **GraphicsViewConfiguration**: This one controls some view configuration like antialiasing mode, texture quality, light intensity and much more. It is accessible through the Configuration property of the RenderLoop class or the corresponding view control (e. g. SeeingSharpRendererElement for Wpf)

## Configure startup using SeeingSharpLoadSettings
The most common case here is to enable DirectX debug mode. You may want to enable that because there is some strange behavior during rendering and need further debug output from the application. Don't forget to also enable native code debugging in your application. Be careful: The required debug dlls of DirectX are not installed on common user machines.
```csharp´
GraphicsCore.Loader
    .SupportWpf()
    .EnableDirectXDebugMode()
    .Load();
```

## Global configuration using GraphicsCoreConfiguration
Currently, there is not much to do here. The only properties that are provided through GraphicsCoreConfiguration are read only (like DirectX debug mode which is set during loading). This may change in the future...

## Per device configuration using GraphicsDeviceConfiguration
In the current version of Seeing# you find two properties here:
* **TextureQuality**: On TextureResources you have to possibility to provide a low- and a high-quality version of the texture. The TextureqQuality property on the GraphicsDeviceConfiguration controls which of both gets loaded
* **GeometryQuality**: Here very similar like for TextureQuality. Also, when you implement a custom GeometryFactory, then you get passed the configured geometry quality level on the device, which loads the geometry.

## Per view configuration using GraphicsViewConfiguration
You find most of the view related configuration properties here. Best here is to try them out, they effect the corresponding view directly.