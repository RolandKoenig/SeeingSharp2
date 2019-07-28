using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.CommandWpf;
using SeeingSharp.Checking;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;
using SeeingSharp.Util;
using System.Threading.Tasks;

namespace SeeingSharpModelViewer
{
    public class MainWindowViewModel : ViewModelBase
    {
        // Main references
        private Camera3DBase m_camera;
        private Scene m_scene;

        // Child viewmodels
        private LoadedFileViewModel m_loadedFileVM;
        private MiscGraphicsObjectsViewModel m_miscObjectsVM;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
        /// </summary>
        public MainWindowViewModel()
        {
            m_scene = new Scene();
            m_scene.DiscardAutomaticUnload = true;

            m_camera = new PerspectiveCamera3D();

            m_loadedFileVM = new LoadedFileViewModel(m_scene);
            m_miscObjectsVM = new MiscGraphicsObjectsViewModel(m_scene);

            this.CommandOpen = new RelayCommand(this.OnCommandOpen_Execute);
            this.CommandClose = new RelayCommand(this.OnCommandClose_Execute, this.OnCommandClose_CanExecute);
            this.CommandReload = new RelayCommand(this.OnCommandReload_Execute, this.OnCommandReload_CanExecute);
        }

        /// <summary>
        /// Creates the test data for the designer.
        /// </summary>
        public static MainWindowViewModel CreateTestDataForDesigner()
        {
            return new MainWindowViewModel();
        }

        public Task InitializeAsync()
        {
            return m_miscObjectsVM.InitializeAsync();
        }

        private bool OnCommandClose_CanExecute()
        {
            return m_loadedFileVM.CurrentFile != null;
        }

        private async void OnCommandClose_Execute()
        {
            await m_loadedFileVM.CloseAsync();
        }

        private async void OnCommandOpen_Execute()
        {
            var msgQueryViewService = new QueryForViewServiceMessage(typeof(ICommonDialogsViewService));
            this.MessengerInstance.Send(msgQueryViewService);

            var dialogService = (ICommonDialogsViewService)msgQueryViewService.ViewService;
            dialogService.EnsureNotNull(nameof(dialogService));

            ResourceLink fileToOpen = dialogService.PickFileByDialog(
                GraphicsCore.Current.ImportersAndExporters.GetOpenFileDialogFilter());
            if (fileToOpen == null) { return; }

            await m_loadedFileVM.ImportNewFileAsync(fileToOpen);
        }

        private bool OnCommandReload_CanExecute()
        {
            return m_loadedFileVM.CurrentFile != null;
        }

        private async void OnCommandReload_Execute()
        {
            await m_loadedFileVM.ReloadCurrentFileAsync();
        }

        public Camera3DBase Camera
        {
            get { return m_camera; }
        }

        public RelayCommand CommandClose
        {
            get;
            private set;
        }

        public RelayCommand CommandOpen
        {
            get;
            private set;
        }

        public RelayCommand CommandReload
        {
            get;
            private set;
        }

        public LoadedFileViewModel LoadedFile
        {
            get { return m_loadedFileVM; }
        }

        public MiscGraphicsObjectsViewModel MiscObjects
        {
            get { return m_miscObjectsVM; }
        }

        public Scene Scene
        {
            get { return m_scene; }
        }
    }
}