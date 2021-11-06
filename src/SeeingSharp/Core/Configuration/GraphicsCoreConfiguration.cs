namespace SeeingSharp.Core.Configuration
{
    public class GraphicsCoreConfiguration
    {
        /// <summary>
        /// Can enable debug mode for all created devices.
        /// This value can only be manipulated when loading SeeingSharp.
        /// </summary>
        public bool DebugEnabled
        {
            get;
            internal set;
        }

        /// <summary>
        /// False in most cases. This property can be used to simulate an initialization error when loading Direct2D.
        /// This value can only be manipulated when loading SeeingSharp.
        /// </summary>
        public bool ThrowD2DInitDeviceError
        {
            get;
            internal set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsCoreConfiguration" /> class.
        /// </summary>
        public GraphicsCoreConfiguration()
        {
            this.DebugEnabled = false;
        }
    }
}
