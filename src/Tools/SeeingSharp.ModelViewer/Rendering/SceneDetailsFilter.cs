using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.ModelViewer.Rendering
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
            return input.Name switch
            {
                Constants.OBJ_NAME_UNIT_CUBE => this.ShowUnitCube,
                Constants.OBJ_NAME_GRID => this.ShowGrid,
                _ => true
            };
        }
    }
}
