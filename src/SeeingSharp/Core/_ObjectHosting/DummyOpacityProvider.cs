namespace SeeingSharp.Core
{
    public class DummyOpacityProvider : IEngineOpacityProvider
    {
        /// <summary>
        /// Gets or sets the opacity of this object.
        /// </summary>
        public float Opacity
        {
            get;
            set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyOpacityProvider"/> class.
        /// </summary>
        public DummyOpacityProvider()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DummyOpacityProvider"/> class.
        /// </summary>
        public DummyOpacityProvider(float opacity)
        {
            this.Opacity = opacity;
        }
    }
}