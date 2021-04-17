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
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer.Util
{
    public class SceneDetailsFilter : SceneObjectFilter
    {
        private bool _showUnitCube;
        private bool _showGrid;

        public bool ShowUnitCube
        {
            get => _showUnitCube;
            set
            {
                if (_showUnitCube != value)
                {
                    _showUnitCube = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }
        }

        public bool ShowGrid
        {
            get => _showGrid;
            set
            {
                if (_showGrid != value)
                {
                    _showGrid = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }
        }

        public SceneDetailsFilter()
        {
            _showGrid = true;
            _showUnitCube = false;
        }

        /// <inheritdoc />
        public override bool IsObjectVisible(SceneObject input, ViewInformation viewInformation)
        {
            if (!(input.Tag1 is string objectTypeName)) { return true; }

            return objectTypeName switch
            {
                Constants.OBJ_NAME_UNIT_CUBE => this.ShowUnitCube,
                Constants.OBJ_NAME_GRID => this.ShowGrid,
                _ => true
            };
        }
    }
}
