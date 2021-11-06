namespace SeeingSharp.Core.Configuration
{
    public class GraphicsDeviceConfiguration
    {
        /// <summary>
        /// Gets or sets the texture quality level.
        /// </summary>
        public TextureQuality TextureQuality
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the geometry quality level.
        /// </summary>
        public GeometryQuality GeometryQuality
        {
            get;
            set;
        }

        /// <summary>
        /// Gets current core configuration.
        /// </summary>
        public GraphicsCoreConfiguration CoreConfiguration { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsDeviceConfiguration" /> class.
        /// </summary>
        /// <param name="coreConfig">The core configuration object.</param>
        public GraphicsDeviceConfiguration(GraphicsCoreConfiguration coreConfig)
        {
            this.CoreConfiguration = coreConfig;
        }
    }
}