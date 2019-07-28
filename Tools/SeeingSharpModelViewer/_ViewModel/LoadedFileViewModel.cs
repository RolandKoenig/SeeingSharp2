using GalaSoft.MvvmLight;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Objects;
using SeeingSharp.Util;
using System;
using System.Threading.Tasks;

namespace SeeingSharpModelViewer
{
    public class LoadedFileViewModel : ViewModelBase
    {
        // Environment
        private Scene m_scene;

        // Loaded data
        private ResourceLink m_currentFile;
        private ImportOptions m_currentImportOptions;

        // State
        private bool m_isLoading;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedFileViewModel"/> class.
        /// </summary>
        public LoadedFileViewModel(Scene scene)
        {
            scene.EnsureNotNull(nameof(scene));

            m_scene = scene;
        }

        public async Task CloseAsync(bool clearCurrentFileInfo = true)
        {
            await m_scene.ManipulateSceneAsync(manipulator => manipulator.ClearLayer(Scene.DEFAULT_LAYER_NAME));

            if (clearCurrentFileInfo)
            {
                m_currentFile = null;
                m_currentImportOptions = null;
                this.RaisePropertyChanged(nameof(this.CurrentFile));
                this.RaisePropertyChanged(nameof(this.CurrentFileForStatusBar));
                this.RaisePropertyChanged(nameof(this.CurrentImportOptions));
            }
        }

        /// <summary>
        /// Imports a new file by the given <see cref="ResourceLink"/>.
        /// </summary>
        /// <param name="resourceLink">The <see cref="ResourceLink"/> from which to load the resource.</param>
        public async Task ImportNewFileAsync(ResourceLink resourceLink)
        {
            if (this.IsLoading) { return; }

            this.IsLoading = true;
            try
            {
                m_currentFile = resourceLink;
                m_currentImportOptions = GraphicsCore.Current.ImportersAndExporters.CreateImportOptions(m_currentFile);
                base.RaisePropertyChanged(nameof(this.CurrentFile));
                base.RaisePropertyChanged(nameof(this.CurrentFileForStatusBar));
                base.RaisePropertyChanged(nameof(this.CurrentImportOptions));

                await m_scene.ImportAsync(m_currentFile, m_currentImportOptions);
            }
            finally
            {
                this.IsLoading = false;
            }

            this.MessengerInstance.Send(new NewModelLoadedMessage());
        }

        public async Task ReloadCurrentFileAsync()
        {
            if (this.IsLoading) { return; }

            this.IsLoading = true;
            try
            {
                m_currentFile.EnsureNotNull(nameof(m_currentFile));
                m_currentImportOptions.EnsureNotNull(nameof(m_currentImportOptions));

                await this.CloseAsync(
                    clearCurrentFileInfo: false);

                await m_scene.ImportAsync(m_currentFile, m_currentImportOptions);
            }
            finally
            {
                this.IsLoading = false;
            }

            this.MessengerInstance.Send(new NewModelLoadedMessage());
        }

        public ResourceLink CurrentFile
        {
            get { return m_currentFile; }
        }

        public string CurrentFileForStatusBar
        {
            get
            {

                var fileResource = this.CurrentFile as FileSystemResourceLink;
                if (fileResource == null) { return "-"; }
                else
                {
                    return fileResource.FileName;
                }
            }
        }

        public ImportOptions CurrentImportOptions
        {
            get { return m_currentImportOptions; }
        }

        public bool IsLoading
        {
            get { return m_isLoading; }
            set
            {
                if (m_isLoading != value)
                {
                    m_isLoading = value;
                    this.RaisePropertyChanged(nameof(this.IsLoading));
                }
            }
        }
    }
}