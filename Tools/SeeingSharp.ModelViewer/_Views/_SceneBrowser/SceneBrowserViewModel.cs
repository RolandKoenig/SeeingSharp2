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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Multimedia.Core;

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
            foreach (var actObjectInfo in currentSceneInfos)
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
    }
}
