using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public class MainWindowVM : PropertyChangedBase
    {
        private RenderLoop m_renderLoop;
        private string m_loadedFile;

        public event EventHandler<OpenFileDialogEventArgs>? OpenFileDialogRequest;

        public MainWindowVM(RenderLoop renderLoop)
        {
            m_renderLoop = renderLoop;
            m_loadedFile = string.Empty;

            this.Command_OpenFile = new DelegateCommand(this.OpenFile);
            this.Command_CloseFile = new DelegateCommand(async () => await this.LoadSceneInternalAsync(null, null));
            this.Command_Exit = new DelegateCommand(() => Environment.Exit(0));

            this.UpdateTitle();
        }

        public Task LoadInitialScene()
        {
            m_renderLoop.SceneComponents.Add(
                new FreeMovingCameraComponent());

            return this.LoadSceneInternalAsync(null, null);
        }

        private async void OpenFile()
        {
            var eArgs = new OpenFileDialogEventArgs(GraphicsCore.Current.ImportersAndExporters.GetOpenFileDialogFilter());
            this.OpenFileDialogRequest?.Invoke(this, eArgs);
            if(!string.IsNullOrEmpty(eArgs.SelectedFile))
            {
                var modelLink = (ResourceLink)eArgs.SelectedFile;
                var importOptions = GraphicsCore.Current.ImportersAndExporters.CreateImportOptions(modelLink);

                await this.LoadSceneInternalAsync(modelLink, importOptions);
            }
        }

        private async Task LoadSceneInternalAsync(ResourceLink? modelLink, ImportOptions? importOptions)
        {
            // Reset the scene
            await SceneHelper.ResetScene(m_renderLoop);

            // Load the given model
            if (modelLink == null)
            {
                m_loadedFile = string.Empty;
            }
            else
            {
                m_loadedFile = modelLink.ToString() ?? "";
                if (importOptions == null)
                {
                    importOptions = GraphicsCore.Current.ImportersAndExporters.CreateImportOptions(modelLink);
                }

                try
                {
                    await m_renderLoop.Scene.ImportAsync(modelLink, importOptions);
                }
                catch (Exception)
                {
                    await SceneHelper.ResetScene(m_renderLoop);
                    m_loadedFile = string.Empty;
                    throw;
                }
                
                await m_renderLoop.WaitForNextFinishedRenderAsync();

                await m_renderLoop.MoveCameraToDefaultLocationAsync(
                    -EngineMath.RAD_45DEG,
                    EngineMath.RAD_45DEG);
            }

            this.UpdateTitle();
        }

        private void UpdateTitle()
        { 
            var titleBuilder = new StringBuilder(128);
            titleBuilder.Append("SeeingSharp 2 ModelViewer");
            if (!string.IsNullOrEmpty(m_loadedFile))
            {
                titleBuilder.Append($" - {m_loadedFile}");
            }

            this.AppTitle = titleBuilder.ToString();
            this.RaisePropertyChanged(nameof(this.AppTitle));
        }

        public DelegateCommand Command_OpenFile { get; }

        public DelegateCommand Command_CloseFile { get; }

        public DelegateCommand Command_Exit { get; }

        public string AppTitle { get; set; } = string.Empty;
    }
}
