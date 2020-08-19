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
