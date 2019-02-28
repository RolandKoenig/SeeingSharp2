#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion
namespace SeeingSharp.Multimedia.Core
{
    #region using

    using System;
    using System.ComponentModel;
    using System.Xml.Serialization;
    using SeeingSharp.Util;

    #endregion

    public class GraphicsViewConfiguration
    {
        #region Constants
        private const bool DEFAULT_SHOW_TEXTURES = true;
        private const bool DEFAULT_WIREFRAME = false;
        private const bool DEFAULT_ANTIALIASING = true;
        private const AntialiasingQualityLevel DEFAULT_ANTIALIASING_QUALITY = AntialiasingQualityLevel.Medium;
        private const float DEFAULT_BORDER_FACTOR = 1f;
        private const float DEFAULT_GRADIENT_FACTOR = 1f;
        private const float DEFAULT_ACCENTUATION_FACTOR = 0f;
        private const float DEFAULT_AMBIENT_FACTOR = 0.2f;
        private const float DEFAULT_LIGHT_POWER = 0.8f;
        private const float DEFAULT_STRONG_LIGHT_FACTOR = 1.5f;
        private const bool DEFAULT_SWAP_CHAIN_WIDTH_ALPHA = false;
        #endregion

        #region Generic
        #endregion

        #region Antialiasing configuration
        private bool m_antialiasingEnabled;
        private AntialiasingQualityLevel m_antialiasingQuality;
        #endregion

        #region Most view parameters (Light, Gradient, Accentuation)
        private float m_generatedColorGradientFactor;
        private float m_generatedBorderFactor;
        private float m_accentuationFactor;
        private float m_ambientFactor;
        private float m_lightPower;
        private float m_strongLightFactor;
        private bool m_alphaEnabledSwapChain;
        #endregion

        /// <summary>
        /// Occurs when any configuration flag has changed.
        /// This event may occure in different threads!
        /// </summary>
        public event EventHandler ConfigurationChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsViewConfiguration" /> class.
        /// </summary>
        internal GraphicsViewConfiguration()
        {
            ShowTextures = DEFAULT_SHOW_TEXTURES;
            ShowTexturesInternal = DEFAULT_SHOW_TEXTURES;
            WireframeEnabled = DEFAULT_WIREFRAME;
            AntialiasingEnabled = DEFAULT_ANTIALIASING;
            AntialiasingQuality = DEFAULT_ANTIALIASING_QUALITY;
            m_generatedBorderFactor = DEFAULT_BORDER_FACTOR;
            m_generatedColorGradientFactor = DEFAULT_GRADIENT_FACTOR;
            m_accentuationFactor = DEFAULT_ACCENTUATION_FACTOR;
            m_ambientFactor = DEFAULT_AMBIENT_FACTOR;
            m_lightPower = DEFAULT_LIGHT_POWER;
            m_strongLightFactor = DEFAULT_STRONG_LIGHT_FACTOR;
            m_alphaEnabledSwapChain = DEFAULT_SWAP_CHAIN_WIDTH_ALPHA;
        }

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
            get => m_antialiasingEnabled;
            set
            {
                if (m_antialiasingEnabled != value)
                {
                    m_antialiasingEnabled = value;
                    ViewNeedsRefresh = true;

                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// The quality level for antialiasing (if antialiasing is enabled).
        /// </summary>
        [DefaultValue(DEFAULT_ANTIALIASING_QUALITY)]
        public AntialiasingQualityLevel AntialiasingQuality
        {
            get => m_antialiasingQuality;
            set
            {
                if (m_antialiasingQuality != value)
                {
                    m_antialiasingQuality = value;
                    ViewNeedsRefresh = true;

                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_GRADIENT_FACTOR)]
        public float GeneratedColorGradientFactor
        {
            get => m_generatedColorGradientFactor;
            set
            {
                if (EngineMath.EqualsWithTolerance(m_generatedColorGradientFactor, value))
                {
                    m_generatedColorGradientFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_BORDER_FACTOR)]
        public float GeneratedBorderFactor
        {
            get => m_generatedBorderFactor;
            set
            {
                if (EngineMath.EqualsWithTolerance(m_generatedBorderFactor, value))
                {
                    m_generatedBorderFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_ACCENTUATION_FACTOR)]
        public float AccentuationFactor
        {
            get => m_accentuationFactor;
            set
            {
                if (EngineMath.EqualsWithTolerance(m_accentuationFactor, value))
                {
                    m_accentuationFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_AMBIENT_FACTOR)]
        public float AmbientFactor
        {
            get => m_ambientFactor;
            set
            {
                if (EngineMath.EqualsWithTolerance(m_ambientFactor, value))
                {
                    m_ambientFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_LIGHT_POWER)]
        public float LightPower
        {
            get => m_lightPower;
            set
            {
                if (EngineMath.EqualsWithTolerance(m_lightPower, value))
                {
                    m_lightPower = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        [DefaultValue(DEFAULT_STRONG_LIGHT_FACTOR)]
        public float StrongLightFactor
        {
            get => m_strongLightFactor;
            set
            {
                if (EngineMath.EqualsWithTolerance(m_strongLightFactor, value))
                {
                    m_strongLightFactor = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Internal accessor for ShowTextures variable.
        /// </summary>
        internal bool ShowTexturesInternal;

        [XmlAttribute]
        [DefaultValue(DEFAULT_SHOW_TEXTURES)]
        public bool ShowTextures
        {
            get => ShowTexturesInternal;
            set
            {
                if (ShowTexturesInternal != value)
                {
                    ShowTexturesInternal = value;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Set this flag to true to enable transparent pixels when view is embedded into Xaml stack.
        /// Only relevant in UWP-Apps!
        /// </summary>
        [Browsable(false)]
        [XmlAttribute]
        [DefaultValue(DEFAULT_SWAP_CHAIN_WIDTH_ALPHA)]
        public bool AlphaEnabledSwapChain
        {
            get => m_alphaEnabledSwapChain;
            set
            {
                if(m_alphaEnabledSwapChain != value)
                {
                    m_alphaEnabledSwapChain = value;
                    ViewNeedsRefresh = true;
                    ConfigurationChanged.Raise(this, EventArgs.Empty);
                }
            }
        }

        /// <summary>
        /// Gets current device configuration.
        /// </summary>
        [Browsable(false)]
        public GraphicsDeviceConfiguration DeviceConfiguration { get; internal set; }

        /// <summary>
        /// Gets current core configuration.
        /// </summary>
        [Browsable(false)]
        public GraphicsCoreConfiguration CoreConfiguration => DeviceConfiguration?.CoreConfiguration;
    }
}
