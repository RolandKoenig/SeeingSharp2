using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
                    this.RaiseFilterConfigurationChanged();
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
                    this.RaiseFilterConfigurationChanged();
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
