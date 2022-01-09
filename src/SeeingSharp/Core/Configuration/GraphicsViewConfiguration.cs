using System;
using System.ComponentModel;
using SeeingSharp.Mathematics;
using SeeingSharp.Util;

namespace SeeingSharp.Core.Configuration
{
    public class GraphicsViewConfiguration
    {
        // Constants
        private const bool DEFAULT_SHOW_TEXTURES = true;
        private const bool DEFAULT_WIREFRAME = false;
        private const bool DEFAULT_ANTIALIASING = true;
        private const AntialiasingQualityLevel DEFAULT_ANTIALIASING_QUALITY = AntialiasingQualityLevel.Medium;
        private const float DEFAULT_BORDER_FACTOR = 1f;
        private const float DEFAULT_ACCENTUATION_FACTOR = 0f;
        private const float DEFAULT_AMBIENT_FACTOR = 0.2f;
        private const float DEFAULT_LIGHT_POWER = 0.8f;
        private const float DEFAULT_STRONG_LIGHT_FACTOR = 1.5f;
        private const bool DEFAULT_SWAP_CHAIN_WIDTH_ALPHA = false;

        // Antialiasing configuration
        private bool _antialiasingEnabled;
        private AntialiasingQualityLevel _antialiasingQuality;

        // Most view parameters (Light, Gradient, Accentuation)
        private float _generatedBorderFactor;
        private float _accentuationFactor;
        private float _ambientFactor;
        private float _lightPower;
        private float _strongLightFactor;
        private bool _alphaEnabledSwapChain;

        public bool ViewNeedsRefresh { get; set; }

        /// <summary>
        /// Is wireframe rendering enabled?
        /// </summary>
        [DefaultValue(DEFAULT_WIREFRAME)]
        public bool WireframeEnabled
        {
            get;
            set;
        }

        /// <summary>
        /// Try to enable antialiasing?
        /// </summary>
        [DefaultValue(DEFAULT_ANTIALIASING)]
        public bool AntialiasingEnabled
        {
            get => _antialiasingEnabled;
            set
            {
                if (_antialiasingEnabled != value)
                {
                    _antialiasingEnabled = value;
                    this.ViewNeedsRefresh = true;

                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The quality level for antialiasing (if antialiasing is enabled).
        /// </summary>
        [DefaultValue(DEFAULT_ANTIALIASING_QUALITY)]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => _antialiasingQuality;
            set
            {
                if (_antialiasingQuality != value)
                {
                    _antialiasingQuality = value;
                    this.ViewNeedsRefresh = true;

                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_BORDER_FACTOR)]
        public float GeneratedBorderFactor
        {
            get => _generatedBorderFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_generatedBorderFactor, value))
                {
                    _generatedBorderFactor = value;
                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_ACCENTUATION_FACTOR)]
        public float AccentuationFactor
        {
            get => _accentuationFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_accentuationFactor, value))
                {
                    _accentuationFactor = value;
                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_AMBIENT_FACTOR)]
        public float AmbientFactor
        {
            get => _ambientFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_ambientFactor, value))
                {
                    _ambientFactor = value;
                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_LIGHT_POWER)]
        public float LightPower
        {
            get => _lightPower;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_lightPower, value))
                {
                    _lightPower = value;
                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_STRONG_LIGHT_FACTOR)]
        public float StrongLightFactor
        {
            get => _strongLightFactor;
            set
            {
                if (!EngineMath.EqualsWithTolerance(_strongLightFactor, value))
                {
                    _strongLightFactor = value;
                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_SHOW_TEXTURES)]
        public bool ShowTextures
        {
            get => ShowTexturesInternal;
            set
            {
                if (ShowTexturesInternal != value)
                {
                    ShowTexturesInternal = value;
                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Set this flag to true to enable transparent pixels when view is embedded into Xaml stack.
        /// Only relevant in UWP-Apps!
        /// </summary>
        [Browsable(false)]
        [DefaultValue(DEFAULT_SWAP_CHAIN_WIDTH_ALPHA)]
        public bool AlphaEnabledSwapChain
        {
            get => _alphaEnabledSwapChain;
            set
            {
                if (_alphaEnabledSwapChain != value)
                {
                    _alphaEnabledSwapChain = value;
                    this.ViewNeedsRefresh = true;
                    this.ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets current device configuration.
        /// </summary>
        [Browsable(false)]
        public GraphicsDeviceConfiguration? DeviceConfiguration { get; internal set; }

        /// <summary>
        /// Gets current core configuration.
        /// </summary>
        [Browsable(false)]
        public GraphicsCoreConfiguration? CoreConfiguration => this.DeviceConfiguration?.CoreConfiguration;

        /// <summary>
        /// Internal accessor for ShowTextures variable.
        /// </summary>
        internal bool ShowTexturesInternal;

        /// <summary>
        /// Occurs when any configuration flag has changed.
        /// This event may occur in different threads!
        /// </summary>
        public event EventHandler? ConfigurationChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsViewConfiguration" /> class.
        /// </summary>
        internal GraphicsViewConfiguration()
        {
            this.ShowTextures = DEFAULT_SHOW_TEXTURES;
            ShowTexturesInternal = DEFAULT_SHOW_TEXTURES;
            this.WireframeEnabled = DEFAULT_WIREFRAME;
            this.AntialiasingEnabled = DEFAULT_ANTIALIASING;
            this.AntialiasingQuality = DEFAULT_ANTIALIASING_QUALITY;
            _generatedBorderFactor = DEFAULT_BORDER_FACTOR;
            _accentuationFactor = DEFAULT_ACCENTUATION_FACTOR;
            _ambientFactor = DEFAULT_AMBIENT_FACTOR;
            _lightPower = DEFAULT_LIGHT_POWER;
            _strongLightFactor = DEFAULT_STRONG_LIGHT_FACTOR;
            _alphaEnabledSwapChain = DEFAULT_SWAP_CHAIN_WIDTH_ALPHA;
        }

        public void Reset()
        {
            this.ShowTextures = DEFAULT_SHOW_TEXTURES;
            this.WireframeEnabled = DEFAULT_WIREFRAME;
            this.AntialiasingEnabled = DEFAULT_ANTIALIASING;
            this.AntialiasingQuality = DEFAULT_ANTIALIASING_QUALITY;
            this.GeneratedBorderFactor= DEFAULT_BORDER_FACTOR;
            this.AccentuationFactor = DEFAULT_ACCENTUATION_FACTOR;
            this.AmbientFactor = DEFAULT_AMBIENT_FACTOR;
            this.LightPower = DEFAULT_LIGHT_POWER;
            this.StrongLightFactor = DEFAULT_STRONG_LIGHT_FACTOR;
        }
    }
}
