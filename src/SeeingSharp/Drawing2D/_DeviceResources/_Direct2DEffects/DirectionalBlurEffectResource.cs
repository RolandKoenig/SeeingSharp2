using SeeingSharp.Core;
using SeeingSharp.Core.Devices;
using D2D = Vortice.Direct2D1;

namespace SeeingSharp.Drawing2D
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
            
            var dirBlurEffect = new Vortice.Direct2D1.Effects.DirectionalBlur(device.DeviceContextD2D)
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
