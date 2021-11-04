[Home](../README.md)

## The bootstrapping process
Before using any rendering functionality from Seeing# you have to invoke the bootstrapping process. Bootstrapping is available through the GraphicsCore.Loader class. Bootstrapping can only happen once. If you invoke the process the second time, then you get an exception. The following code-snipped shows this for the base functionality of Seeing#. 

```csharp
GraphicsCore.Loader
    .Load();
```

The bootstrapping process is designed to be extensible in some ways. It is possible to include support for different UI frameworks as mentioned below. Also other functionality from different assemblies can be attached to the bootstrapping process by calling the appropriate method. The following methods are available by default:

|Method                  |Assembly                                          |Description
|------------------------|:-------------------------------------------------|:-----------------------------------
|SupportWpf              |SeeingSharp.Wpf or SeeingSharp.WpfCore            |Adds support for WPF
|SupportWinForms         |SeeingSharp.WinForms or SeeingSharp.WinFormsCore  |Adds support for Win.Forms
|SupportUwp              |SeeingSharp.Uwp                                   |Adds support for Uwp
|EnableDirectXDebugMode  |SeeingSharp                                       |Enables native debug mode of DirectX. Be careful with that: Most of consumer hardware is missing the required DirectX debug dlls. Seeing# renders a "Debug-Mode" hint into each view when debug mode is enabled.
|Configure               |SeeingSharp                                       |Allows to change some configuration flags before loading Seeing#

## Bootstrapping with WPF support
The best place for bootstrapping Seeing# in a WPF application is the App.xaml.cs file. Following code-snipped shows when to call the SupportWpf method. Don't forget to include the SeeingSharp.Wpf or SeeingSharp.WpfCore assembly and include the namespace SeeingSharp.Core.

```csharp
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        GraphicsCore.Loader
            .SupportWpf()
            .Load();

        base.OnStartup(e);
    }
}
```

## Bootstrapping with Windows.Forms support
The best place for bootstrapping Seeing# in a Windows.Forms application is the Program.cs file. Following code-snipped shows when to call the SupportWinForms method. Don't forget to include the SeeingSharp.WinForms or SeeingSharp.WinFormsCore assembly and include the namespace SeeingSharp.Core.

```csharp
internal static class Program
{
    [STAThread]
    private static void Main()
    {
        GraphicsCore.Loader
            .SupportWinForms()
            .Load();

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainWindow());
    }
}
```
## Bootstrapping with UWP (Universal Windows Platform)
The best place for bootstrapping Seeing# in an UWP application is the App.xaml.cs file. Following code-snipped shows when to call the SupportUwp method. Don't forget to include the SeeingSharp.Uwp assembly and include the namespace SeeingSharp.Core. Additionally we have to handle the Suspending and Resuming events of the Application class.

```csharp
sealed partial class App : Application
{
    public App()
    {
        this.InitializeComponent();

        this.Suspending += this.OnSuspending;
        this.Resuming += this.OnResuming;
    }

    protected override void OnLaunched(LaunchActivatedEventArgs e)
    {
        // Initialize graphics
        GraphicsCore.Loader
            .SupportUwp()
            .Load();

        // ...
    }

    // ...

    private async void OnSuspending(object sender, SuspendingEventArgs e)
    {
        var deferral = e.SuspendingOperation.GetDeferral();

        if (GraphicsCore.IsLoaded)
        {
            await GraphicsCore.Current.SuspendAsync();
        }

        deferral.Complete();
    }

    private void OnResuming(object sender, object e)
    {
        GraphicsCore.Current.Resume();
    }
}
```
