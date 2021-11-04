using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Core;
using SeeingSharp.Drawing3D;

namespace SeeingSharp.ModelViewer
{
    public class SceneBrowserViewModel : PropertyChangedBase
    {
        private Func<IEnumerable<SceneObjectInfo>> _sceneObjectGetter;
        private SceneObjectInfo? _selectedObject;
        private SceneBrowserObjectViewModel? _selectedObjectViewModel;

        public ObservableCollection<SceneObjectInfo> SceneObjectInfos { get; } = new ObservableCollection<SceneObjectInfo>();

        public DelegateCommand Command_Refresh { get; }

        public SceneObjectInfo? SelectedObject
        {
            get => _selectedObject;
            set
            {
                if (_selectedObject != value)
                {
                    _selectedObject = value;
                    _selectedObjectViewModel =
                        _selectedObject != null ? new SceneBrowserObjectViewModel(_selectedObject) : null;
                    this.RaisePropertyChanged(nameof(this.SelectedObject));
                    this.RaisePropertyChanged(nameof(this.SelectedObjectViewModel));
                }
            }
        }

        public SceneBrowserObjectViewModel? SelectedObjectViewModel => _selectedObjectViewModel;

        public SceneBrowserViewModel(Scene scene)
            : this(() => scene.GetSceneObjectInfos())
        {
            
        }

        public SceneBrowserViewModel(Func<IEnumerable<SceneObjectInfo>> sceneObjectGetter)
        {
            this.Command_Refresh = new DelegateCommand(this.RefreshSceneTree);

            _sceneObjectGetter = sceneObjectGetter;

            this.RefreshSceneTree();
        }

        /// <summary>
        /// Gets the current collection of objects from the scene and rebuilds local scene tree.
        /// </summary>
        public void RefreshSceneTree()
        {
            var currentSceneInfos = _sceneObjectGetter();

            this.SceneObjectInfos.Clear();
            this.SelectedObject = null;

            var importRoot = this.FindImportRoot(currentSceneInfos);
            if (importRoot == null) { return; }

            foreach (var actObjectInfo in importRoot.Children)
            {
                this.SceneObjectInfos.Add(actObjectInfo);
            }
        }

        /// <summary>
        /// Refreshes local scene tree.
        /// </summary>
        public void RefreshData()
        {
            if (this.SelectedObjectViewModel != null)
            {
                foreach (var actSubViewModel in this.SelectedObjectViewModel.HardwareDependentDetails)
                {
                    actSubViewModel.RefreshData();
                }

                this.SelectedObjectViewModel.HardwareIndependentDetails.RefreshData();
            }
        }

        /// <summary>
        /// Helper method for finding the root of the imported model.
        /// </summary>
        private SceneObjectInfo? FindImportRoot(IEnumerable<SceneObjectInfo> sceneObjectInfos)
        {
            foreach (var actSceneObjectInfo in sceneObjectInfos)
            {
                if (actSceneObjectInfo.Name.StartsWith(ImportedModelContainer.IMPORT_ROOT_NODE_NAME_PREFIX))
                {
                    return actSceneObjectInfo;
                }

                var recursiveResult = this.FindImportRoot(actSceneObjectInfo.Children);
                if (recursiveResult != null)
                {
                    return recursiveResult;
                }
            }
            return null;
        }
    }
}
