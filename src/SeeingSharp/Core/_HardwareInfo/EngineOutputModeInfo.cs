using System;
using SharpDX.DXGI;

namespace SeeingSharp.Core
{
    public struct EngineOutputModeInfo : IEquatable<EngineOutputModeInfo>
    {
        public static readonly EngineOutputModeInfo Empty = default;

        internal EngineOutputModeInfo(EngineOutputInfo hostOutput, ModeDescription modeDescription)
        {
            this.HostOutput = hostOutput;
            this.PixelWidth = modeDescription.Width;
            this.PixelHeight = modeDescription.Height;
            this.RefreshRateNumerator = modeDescription.RefreshRate.Numerator;
            this.RefreshRateDenominator = modeDescription.RefreshRate.Denominator;
        }

        public override string ToString()
        {
            return $"{this.PixelWidth} x {this.PixelHeight}  {this.RefreshRateNumerator / this.RefreshRateDenominator} Hz";
        }

        public override int GetHashCode()
        {
            return this.PixelWidth.GetHashCode() + this.PixelHeight.GetHashCode() + this.RefreshRateNumerator.GetHashCode() + this.RefreshRateDenominator.GetHashCode();
        }

        /// <summary>
        /// Determines whether the specified <see cref="SeeingSharp.Color4"/> is equal to this instance.
        /// </summary>
        /// <param name="other">The <see cref="SeeingSharp.Color4"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="SeeingSharp.Color4"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals(EngineOutputModeInfo other)
        {
            return this.PixelWidth == other.PixelWidth && this.PixelHeight == other.PixelHeight && this.RefreshRateNumerator == other.RefreshRateNumerator && this.RefreshRateDenominator == other.RefreshRateDenominator;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="value">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object value)
        {
            if (value == null)
            {
                return false;
            }

            if (!ReferenceEquals(value.GetType(), typeof(EngineOutputInfo)))
            {
                return false;
            }

            var other = (EngineOutputModeInfo)value;
            return this.PixelWidth == other.PixelWidth && this.PixelHeight == other.PixelHeight && this.RefreshRateNumerator == other.RefreshRateNumerator && this.RefreshRateDenominator == other.RefreshRateDenominator;
        }

        public EngineOutputInfo HostOutput { get; }

        public int PixelWidth { get; }

        public int PixelHeight { get; }

        public int PixelCount => this.PixelWidth * this.PixelHeight;

        public int RefreshRateNumerator { get; }

        public int RefreshRateDenominator { get; }
    }
}