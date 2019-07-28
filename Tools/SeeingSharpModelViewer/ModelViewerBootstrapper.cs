using GalaSoft.MvvmLight.Ioc;
using SeeingSharp.Multimedia.Views;
using System.Threading.Tasks;

namespace SeeingSharpModelViewer
{
    public static class ModelViewerBootstrapper
    {
        /// <summary>
        /// Executes the background action behind this item.
        /// </summary>
        /// <param name="app">The current application instance.</param>
        public static async Task ExecuteAsync()
        {
            MainWindowViewModel modelVM = null;
            using (var dummyRenderTarget = new MemoryRenderTarget(128, 128))
            {
                modelVM = new MainWindowViewModel();

                // Assign main scene and camera to the dummy render target
                dummyRenderTarget.Scene = modelVM.Scene;
                dummyRenderTarget.Camera = modelVM.Camera;

                // Initialize the scen
                await modelVM.InitializeAsync();

                // Perform some dummy renderings
                await dummyRenderTarget.AwaitRenderAsync();
                await dummyRenderTarget.AwaitRenderAsync();
            }

            // All went well, so register the main viewmode globally
            SimpleIoc.Default.Register(() => modelVM);
        }
    }
}