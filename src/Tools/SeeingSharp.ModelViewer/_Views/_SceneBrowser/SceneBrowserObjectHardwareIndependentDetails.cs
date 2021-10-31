/*
    SeeingSharp and all applications distributed together with it. 
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
using System.ComponentModel;
using SeeingSharp.ModelViewer.Util;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer
{
    public class SceneBrowserObjectHardwareIndependentDetails : PropertyChangedBase
    {
        private const string CATEGORY_PROPERTIES = "Properties (hardware independent)";

        private SceneObjectInfo _sceneObjectInfo;

        public SceneBrowserObjectHardwareIndependentDetails(SceneObjectInfo sceneObjectInfo)
        {
            _sceneObjectInfo = sceneObjectInfo;
        }

        public void RefreshData()
        {
            this.RaisePropertyChanged(string.Empty);
        }

        [Category(CATEGORY_PROPERTIES)]
        public string Type => _sceneObjectInfo.Type.ToString();

        [Category(CATEGORY_PROPERTIES)]
        public int CountChildren => _sceneObjectInfo.OriginalObject.CountChildren;

        [Category(CATEGORY_PROPERTIES)]
        public bool HasParent => _sceneObjectInfo.OriginalObject.HasParent;

        [Category(CATEGORY_PROPERTIES)]
        public bool HasChildren => _sceneObjectInfo.OriginalObject.HasChildren;

        [Category(CATEGORY_PROPERTIES)]
        public bool IsStatic => _sceneObjectInfo.OriginalObject.IsStatic;

        [Category(CATEGORY_PROPERTIES)]
        public string TargetDetailLevel => _sceneObjectInfo.OriginalObject.TargetDetailLevel.ToString();
    }
}
