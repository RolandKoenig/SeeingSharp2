using System.Windows;
using Microsoft.Win32;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MainWindowVM? _viewModel;

        public MainWindow()
        {
            this.InitializeComponent();

            // Initialize Viewmodel
            if (GraphicsCore.IsLoaded)
            {
                _viewModel = new MainWindowVM(CtrlRenderer.RenderLoop);
                _viewModel.OpenFileDialogRequest += this.OnViewModel_OpenFileDialogRequest;
                this.DataContext = _viewModel;

                this.AllowDrop = true;
                this.Loaded += this.OnLoaded;

                // Drag / drop of external files
                this.DragOver += this.OnDragOver;
                this.Drop += this.OnDrop;
            }
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            if (_viewModel == null) { e.Effects = DragDropEffects.None; }

            var fileNames = (string[])e.Data.GetData(DataFormats.FileDrop, true);
            if ((fileNames == null) ||
                (fileNames.Length != 1) ||
                (string.IsNullOrEmpty(fileNames[0])))
            {
                e.Effects = DragDropEffects.None;
                e.Handled = true;
            }
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            if (_viewModel == null) { return; }

            var fileNames = (string[]) e.Data.GetData(DataFormats.FileDrop, true);
            if ((fileNames == null) ||
                (fileNames.Length != 1) ||
                (string.IsNullOrEmpty(fileNames[0])))
            {
                return;
            }

            await _viewModel.LoadFileAsync(fileNames[0]);
        }

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            if (_viewModel == null) { return; }

            await _viewModel.LoadInitialScene();
        }

        private void OnViewModel_OpenFileDialogRequest(object? sender, OpenFileDialogEventArgs e)
        {
            var dlgOpenFile = new OpenFileDialog();
            dlgOpenFile.Filter = e.FilterString;
            if (true == dlgOpenFile.ShowDialog(this))
            {
                e.SelectedFile = dlgOpenFile.FileName;
            }
            else
            {
                e.SelectedFile = string.Empty;
            }
        }
    }
}
