/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
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
using SharpDX.DXGI;
using System;

namespace SeeingSharp.Multimedia.Core
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