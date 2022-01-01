using SeeingSharp.Core.Devices;
using Vortice.Direct2D1.Effects;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D.Resources
{
    public class DirectionalBlurEffectResource : EffectResource
    {
        public float Angle
        {
            get;
            set;
        }

        public float StandardDeviation
        {
            get;
            set;
        }

        public DirectionalBlurEffectResource(IImage sourceImage)
            : base(sourceImage)
        {
            this.StandardDeviation = 1f;
            this.Angle = 0f;
        }

        /// <summary>
        /// Builds the effect.
        /// </summary>
        /// <param name="device">The device on which to load the effect instance.</param>
        protected override D2D.ID2D1Effect BuildEffect(EngineDevice device)
        {
            
            var dirBlurEffect = new DirectionalBlur(device.DeviceContextD2D)
            {
                Angle = this.Angle,
                BorderMode = D2D.BorderMode.Soft,
                StandardDeviation = this.StandardDeviation,
                Optimization = D2D.DirectionalBlurOptimization.Balanced
            };

            return dirBlurEffect;
        }
    }
}
