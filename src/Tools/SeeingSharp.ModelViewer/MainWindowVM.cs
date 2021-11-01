using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using SeeingSharp.ModelViewer.Rendering;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Multimedia.Components;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;

namespace SeeingSharp.ModelViewer
{
    public class MainWindowVM : PropertyChangedBase
    {
        private RenderLoop _renderLoop;
        private string _loadedFile;
        private ImportedModelContainer? _loadedModel;
        private bool _isLoading;

        public DelegateCommand Command_OpenFile { get; }

        public DelegateCommand Command_CloseFile { get; }

        public DelegateCommand Command_Exit { get; }

        public string AppTitle { get; set; } = string.Empty;

        public RenderingOptions OptionsRendering { get; }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    this.RaisePropertyChanged(nameof(this.IsLoading));
                    this.RaisePropertyChanged(nameof(this.ControlsEnabled));
                    this.RaisePropertyChanged(nameof(this.LoadingWindowVisibility));
                    this.RaisePropertyChanged(nameof(this.IsCloseEnabled));
                }
            }
        }

        public bool ControlsEnabled => !this.IsLoading;

        public bool IsCloseEnabled => (!this.IsLoading) && (!string.IsNullOrEmpty((_loadedFile)));

        public Visibility LoadingWindowVisibility => this.IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public SceneBrowserViewModel SceneBrowserViewModel { get; }

        public event EventHandler<OpenFileDialogEventArgs>? OpenFileDialogRequest;

        public MainWindowVM(RenderLoop renderLoop)
        {
            _renderLoop = renderLoop;
            _loadedModel = null;
            _loadedFile = string.Empty;

            this.SceneBrowserViewModel = new SceneBrowserViewModel(renderLoop.Scene);

            this.Command_OpenFile = new DelegateCommand(this.OnCommand_OpenFile);
            this.Command_CloseFile = new DelegateCommand(async () => await this.LoadSceneInternalAsync(null, null));
            this.Command_Exit = new DelegateCommand(() => Environment.Exit(0));

            var sceneDetailFilter = new SceneDetailsFilter();
            renderLoop.ObjectFilters.Add(sceneDetailFilter);

            this.OptionsRendering = new RenderingOptions(renderLoop, sceneDetailFilter);

            this.UpdateTitle();
        }

        public Task LoadFileAsync(ResourceLink modelLink)
        {
            var importOptions = GraphicsCore.Current.ImportersAndExporters.CreateImportOptions(modelLink);
            return this.LoadSceneInternalAsync(modelLink, importOptions);
        }

        public Task LoadInitialScene()
        {
            _renderLoop.SceneComponents.Add(
                new FocusedPointCameraComponent
                {
                    CameraDistanceMin = 0.1f,
                    CameraDistanceMax = 5f,
                    CameraHRotationInitial = 2f
                });

            return this.LoadSceneInternalAsync(null, null);
        }

        private async void OnCommand_OpenFile()
        {
            var eArgs = new OpenFileDialogEventArgs(GraphicsCore.Current.ImportersAndExporters.GetOpenFileDialogFilter());
            this.OpenFileDialogRequest?.Invoke(this, eArgs);
            if(!string.IsNullOrEmpty(eArgs.SelectedFile))
            {
                await this.LoadFileAsync(eArgs.SelectedFile);
            }
        }

        private async Task LoadSceneInternalAsync(ResourceLink? modelLink, ImportOptions? importOptions)
        {
            this.IsLoading = true;
            try
            {
                // Reset the scene
                await SceneHelper.ResetScene(_renderLoop);

                // Load the given model
                if (modelLink == null)
                {
                    _loadedFile = string.Empty;
                    _loadedModel = null;
                }
                else
                {
                    _loadedFile = modelLink.ToString() ?? "";
                    if (importOptions == null)
                    {
                        importOptions = GraphicsCore.Current.ImportersAndExporters.CreateImportOptions(modelLink);
                    }

                    try
                    {
                        _loadedModel = await GraphicsCore.Current.ImportersAndExporters
                            .ImportAsync(modelLink, importOptions);

                        await SceneHelper.AddModelToScene(_renderLoop, _loadedModel);
                    }
                    catch (Exception)
                    {
                        await SceneHelper.ResetScene(_renderLoop);
                        _loadedFile = string.Empty;
                        _loadedModel = null;
                        throw;
                    }
                
                    await _renderLoop.WaitForNextFinishedRenderAsync();

                    await _renderLoop.MoveCameraToDefaultLocationAsync(
                        -EngineMath.RAD_45DEG,
                        EngineMath.RAD_45DEG);
                }
            }
            finally
            {
                this.IsLoading = false;
            }

            this.SceneBrowserViewModel.RefreshSceneTree();

            this.UpdateTitle();
        }

        private void UpdateTitle()
        { 
            var titleBuilder = new StringBuilder(128);
            titleBuilder.Append("SeeingSharp 2 ModelViewer");
            if (!string.IsNullOrEmpty(_loadedFile))
            {
                titleBuilder.Append($" - {_loadedFile}");
            }

            this.AppTitle = titleBuilder.ToString();
            this.RaisePropertyChanged(nameof(this.AppTitle));
        }
    }
}
