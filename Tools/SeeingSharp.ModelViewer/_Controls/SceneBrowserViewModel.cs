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
using System.Threading.Tasks;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer
{
    public class SceneBrowserViewModel
    {
        private Func<Task<IEnumerable<SceneObjectInfo>>> _sceneObjectGetter;

        public ObservableCollection<SceneObjectInfo> SceneObjectInfos { get; } = new ObservableCollection<SceneObjectInfo>();

        public DelegateCommand Command_Refresh { get; }

        public SceneBrowserViewModel(Scene scene)
            : this(() => scene
                .GetSceneObjectInfoAsync()
                .ContinueWith(task => (IEnumerable<SceneObjectInfo>)task.Result))
        {
            this.RefreshData();
        }

        public SceneBrowserViewModel(Func<Task<IEnumerable<SceneObjectInfo>>> sceneObjectGetter)
        {
            this.Command_Refresh = new DelegateCommand(this.RefreshData);

            _sceneObjectGetter = sceneObjectGetter;
        }

        public async void RefreshData()
        {
            var currentSceneInfos = await _sceneObjectGetter();

            this.SceneObjectInfos.Clear();
            foreach (var actObjectInfo in currentSceneInfos)
            {
                this.SceneObjectInfos.Add(actObjectInfo);
            }
        }
    }
}
