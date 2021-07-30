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
using PropertyTools.DataAnnotations;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Multimedia.Drawing3D;

namespace SeeingSharp.ModelViewer.Util
{
    public class RenderingOptions : PropertyChangedBase
    {
        private const string CATEGORY_COMMON = "Common";
        private const string CATEGORY_RENDERING = "Rendering";
        private const string CATEGORY_LIGHTING = "Lighting";
        private const string CATEGORY_OBJECTS = "Objects";

        private readonly RenderLoop _renderLoop;
        private readonly SceneDetailsFilter _detailFilter;

        private bool _wireframeEnabled;

        [Category(CATEGORY_COMMON)]
        public DelegateCommand Reset { get; }

        [Category(CATEGORY_OBJECTS)]
        public bool ShowUnitCube
        {
            get => _detailFilter.ShowUnitCube;
            set
            {
                _detailFilter.ShowUnitCube = value;
                this.RaisePropertyChanged(nameof(this.ShowUnitCube));
            }
        }

        [Category(CATEGORY_OBJECTS)]
        public bool ShowGrid
        {
            get => _detailFilter.ShowGrid;
            set
            {
                _detailFilter.ShowGrid = value;
                this.RaisePropertyChanged(nameof(this.ShowGrid));
            }
        }

        [Category(CATEGORY_RENDERING)]
        public CameraMode CameraMode
        {
            get => _renderLoop.Camera is PerspectiveCamera3D ? CameraMode.Perspective : CameraMode.Orthographic;
            set
            {
                switch (value)
                {
                    case CameraMode.Perspective: 
                        _renderLoop.Camera = new PerspectiveCamera3D();
                        break;
                    
                    case CameraMode.Orthographic:
                        _renderLoop.Camera = new OrthographicCamera3D();
                        break;
                }
            }
        }

        [Category(CATEGORY_RENDERING)]
        public bool Wireframe
        {
            get => _wireframeEnabled;
            set
            {
                if (_wireframeEnabled != value)
                {
                    _wireframeEnabled = value;
                    _ = _renderLoop.Scene.ManipulateSceneAsync(manipulator =>
                    {
                        var layerDefault = manipulator.GetLayer(Scene.DEFAULT_LAYER_NAME);
                        layerDefault.WireframeEnabled = value;
                    });
                }
            }
        }

        [Category(CATEGORY_RENDERING)]
        public bool Antialiasing
        {
            get => _renderLoop.Configuration.AntialiasingEnabled;
            set
            {
                _renderLoop.Configuration.AntialiasingEnabled = value;
                this.RaisePropertyChanged(nameof(this.Antialiasing));
            } 
        }

        [Category(CATEGORY_RENDERING)]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => _renderLoop.Configuration.AntialiasingQuality;
            set
            {
                if (value != _renderLoop.Configuration.AntialiasingQuality)
                {
                    _renderLoop.Configuration.AntialiasingQuality = value;
                    this.RaisePropertyChanged(nameof(this.AntialiasingQuality));
                }
            }
        }

        [Category(CATEGORY_LIGHTING)]
        [Slidable(Minimum = 0, Maximum = 1, LargeChange = 0.05, SmallChange = 0.05, TickFrequency = 0.05)]
        [FormatString("N2")]
        public float AmbientFactor
        {
            get => _renderLoop.Configuration.AmbientFactor;
            set
            {
                _renderLoop.Configuration.AmbientFactor = value;
                this.RaisePropertyChanged(nameof(this.AmbientFactor));
            }
        }

        [Category(CATEGORY_LIGHTING)]
        [Slidable(Minimum = 0, Maximum = 1, LargeChange = 0.05, SmallChange = 0.05, TickFrequency = 0.05)]
        [FormatString("N2")]
        public float LightPower
        {
            get => _renderLoop.Configuration.LightPower;
            set
            {
                _renderLoop.Configuration.LightPower = value;
                this.RaisePropertyChanged(nameof(this.LightPower));
            }
        }

        [Category(CATEGORY_LIGHTING)]
        [Slidable(Minimum = 0, Maximum = 3, LargeChange = 0.05, SmallChange = 0.05, TickFrequency = 0.05)]
        [FormatString("N2")]
        public float StrongLightFactor
        {
            get => _renderLoop.Configuration.StrongLightFactor;
            set
            {
                _renderLoop.Configuration.StrongLightFactor = value;
                this.RaisePropertyChanged(nameof(this.StrongLightFactor));
            }
        }

        public RenderingOptions(RenderLoop renderLoop, SceneDetailsFilter detailFilter)
        {
            _renderLoop = renderLoop;
            _detailFilter = detailFilter;

            this.Reset = new DelegateCommand(() =>
            {
                _renderLoop.Configuration.Reset();

                if (!(_renderLoop.Camera is PerspectiveCamera3D))
                {
                    _renderLoop.Camera = new PerspectiveCamera3D();
                }

                this.RaisePropertyChanged(string.Empty);
            });
        }
    }
}
