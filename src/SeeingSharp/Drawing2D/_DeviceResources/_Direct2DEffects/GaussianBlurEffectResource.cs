using SeeingSharp.Core;
using SharpDX.Direct2D1.Effects;
using D2D = SharpDX.Direct2D1;

namespace SeeingSharp.Drawing2D
{
    public class GaussianBlurEffectResource : EffectResource
    {
        public float StandardDeviation
        {
            get;
            set;
        }

        public GaussianBlurEffectResource(IImage sourceImage)
            : base(sourceImage)
        {
            this.StandardDeviation = 1f;
        }

        /// <summary>
        /// Builds the effect.
        /// </summary>
        /// <param name="device">The device on which to load the effect instance.</param>
        protected override D2D.Effect BuildEffect(EngineDevice device)
        {
            var blurEffect = new GaussianBlur(device.DeviceContextD2D)
            {
                BorderMode = D2D.BorderMode.Soft,
                Optimization = D2D.GaussianBlurOptimization.Quality,
                StandardDeviation = this.StandardDeviation
            };

            return blurEffect;
        }
    }
}