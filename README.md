# SeeingSharp2
### Screenshots
![alt tag](_Misc/WikiImages/SeeingSharp_Samples.png)

### Common information
Seeing# is a 3D/2D rendering library for C# powered by Direct3D. It is meant for desktop applications (Win.Forms, Wpf) or Windows Store Apps.
The base library is separated into the following projects:
 - SeeingSharp
 - SeeingSharp.Uwp
 - SeeingSharp.Wpf
 - SeeingSharp.WinForms
 
The first one is the core library which contains all logic for 2D/3D-rendering. The others contain classes to integrate SeeingSharp into 
the particular gui framework. 
 
### Features
 - Full integration into Windows.Forms, WPF and WinRT
 - Heavy multithreading (all calculations and rendering is done in background threads)
 - Working with multiple graphics devices at once (dynamically configure the target device per view)
 - Working with multiple scenegraphs at once (dynamically configure the current scene per view)
 - Flexible postprocessing mechanism
 - Support for software rendering using WARP technology
 - Integration of Direct2D directly into the 3D render process
 - Build custom 3D models by code
 - Import external 3D models 
 - And much more..
