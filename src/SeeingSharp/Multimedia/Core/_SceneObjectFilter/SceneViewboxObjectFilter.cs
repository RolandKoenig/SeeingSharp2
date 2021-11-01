using System.Numerics;

namespace SeeingSharp.Multimedia.Core
{

    public class SceneViewboxObjectFilter : SceneObjectFilter
    {
        // Values for viewbox clipping
        private ViewInformation _viewInfo;
        private BoundingFrustum _boundingFrustum;

        // Values for y-filter
        private bool _enableYFilter;
        private float _yFilterMin;
        private float _yFilterMax;

        /// <summary>
        /// Gets or sets a value indicating whether the y-filter is enabled.
        /// </summary>
        public bool EnableYFilter
        {
            get => _enableYFilter;
            set
            {
                if (_enableYFilter != value)
                {
                    _enableYFilter = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the minimum value of the y-filter.
        /// </summary>
        public float YFilterMin
        {
            get => _yFilterMin;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_yFilterMin, value))
                {
                    _yFilterMin = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the maximum value of the y-filter.
        /// </summary>
        public float YFilterMax
        {
            get => _yFilterMax;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_yFilterMax, value))
                {
                    _yFilterMax = value;
                    this.NotifyFilterConfigurationChanged();
                }
            }
        }

        /// <summary>
        /// Should this filter be updated on each frame?
        /// </summary>
        public override bool UpdateEachFrame => true;

        /// <summary>
        /// Initializes a new instance of the <see cref="SceneViewboxObjectFilter"/> class.
        /// </summary>
        public SceneViewboxObjectFilter()
        {
            // Default configuration of the y-filter
            _enableYFilter = false;
            _yFilterMin = 0f;
            _yFilterMax = 10f;
        }

        /// <summary>
        /// Sets current environment data.
        /// </summary>
        /// <param name="layerToFilter">The SceneLayer that gets filtered.</param>
        /// <param name="viewInformation">The information object of the corresponding view.</param>
        public override void SetEnvironmentData(SceneLayer layerToFilter, ViewInformation viewInformation)
        {
            _viewInfo = viewInformation;
            _boundingFrustum = viewInformation.CameraBoundingFrustum;
        }

        /// <summary>
        /// Checks for visibility of the given object.
        /// </summary>
        /// <param name="input">The object to be checked..</param>
        /// <param name="viewInfo">The view on which to check for visibility.</param>
        public override bool IsObjectVisible(SceneObject input, ViewInformation viewInfo)
        {
            if (_viewInfo == null)
            {
                return true;
            }

            // Perform viewbox clipping
            if (!input.IsInBoundingFrustum(_viewInfo, ref _boundingFrustum))
            {
                return false;
            }

            // Handle Y-Filter
            if (_enableYFilter &&
                !EngineMath.EqualsWithTolerance(_yFilterMin, _yFilterMax) &&
                _yFilterMax > _yFilterMin &&
                _yFilterMax - _yFilterMin > 0.1f)
            {
                if (input is SceneSpacialObject spacialObject)
                {
                    // Get the bounding box of the object
                    var boundingBox = spacialObject.TryGetBoundingBox(viewInfo);

                    if (boundingBox.IsEmpty())
                    {
                        boundingBox = new BoundingBox(spacialObject.Position, spacialObject.Position + new Vector3(0.1f, 0.1f, 0.1f));
                    }

                    // Perform some checks based on the bounding box
                    if (boundingBox.Maximum.Y < _yFilterMin)
                    {
                        return false;
                    }

                    if (boundingBox.Minimum.Y > _yFilterMax)
                    {
                        return false;
                    }
                }
            }

            // Object is visible
            return true;
        }
    }
}
