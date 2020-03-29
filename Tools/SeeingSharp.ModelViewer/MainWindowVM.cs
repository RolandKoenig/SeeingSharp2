/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/

using System;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
                }
            }
        }

        public bool ControlsEnabled => !this.IsLoading;

        public Visibility LoadingWindowVisibility => this.IsLoading ? Visibility.Visible : Visibility.Collapsed;

        public event EventHandler<OpenFileDialogEventArgs>? OpenFileDialogRequest;

        public MainWindowVM(RenderLoop renderLoop)
        {
            _renderLoop = renderLoop;
            _loadedFile = string.Empty;

            this.Command_OpenFile = new DelegateCommand(this.OpenFile);
            this.Command_CloseFile = new DelegateCommand(async () => await this.LoadSceneInternalAsync(null, null));
            this.Command_Exit = new DelegateCommand(() => Environment.Exit(0));

            var sceneDetailFilter = new SceneDetailsFilter();
            renderLoop.Filters.Add(sceneDetailFilter);

            this.OptionsRendering = new RenderingOptions(renderLoop, sceneDetailFilter);

            this.UpdateTitle();
        }

        public async Task LoadInitialScene()
        {
            _renderLoop.SceneComponents.Add(
                new FocusedPointCameraComponent
                {
                    CameraDistanceMin = 0.1f,
                    CameraDistanceMax = 5f,
                    CameraHRotationInitial = 2f
                });

            await _renderLoop.Register2DDrawingLayerAsync(
                new PerformanceMeasureDrawingLayer(10f));

            await this.LoadSceneInternalAsync(null, null);
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
            this.IsLoading = true;
            try
            {
                // Reset the scene
                await SceneHelper.ResetScene(_renderLoop);

                // Load the given model
                if (modelLink == null)
                {
                    _loadedFile = string.Empty;
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
                        await _renderLoop.Scene.ImportAsync(modelLink, importOptions);
                    }
                    catch (Exception)
                    {
                        await SceneHelper.ResetScene(_renderLoop);
                        _loadedFile = string.Empty;
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
