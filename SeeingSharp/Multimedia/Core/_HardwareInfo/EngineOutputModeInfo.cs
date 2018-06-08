using System;
using System.Collections.Generic;
using System.Text;

// Namespace mappings
using DXGI = SharpDX.DXGI;

namespace SeeingSharp.Multimedia.Core
{
    public struct EngineOutputModeInfo : IEquatable<EngineOutputModeInfo>
    {
        public static readonly EngineOutputModeInfo Empty = default(EngineOutputModeInfo);

        private EngineOutputInfo m_hostOutput;
        private int m_pixelWidth;
        private int m_pixelHeight;
        private int m_refreshRateNumerator;
        private int m_refreshRateDenominator;

        internal EngineOutputModeInfo(EngineOutputInfo hostOutput, DXGI.ModeDescription modeDescription)
        {
            m_hostOutput = hostOutput;
            m_pixelWidth = modeDescription.Width;
            m_pixelHeight = modeDescription.Height;
            m_refreshRateNumerator = modeDescription.RefreshRate.Numerator;
            m_refreshRateDenominator = modeDescription.RefreshRate.Denominator;
        }

        public override string ToString()
        {
            return $"{m_pixelWidth} x {m_pixelHeight}  {m_refreshRateNumerator / m_refreshRateDenominator} Hz";
        }

        public override int GetHashCode()
        {
            return m_pixelWidth.GetHashCode() +
                m_pixelHeight.GetHashCode() +
                m_refreshRateNumerator.GetHashCode() +
                m_refreshRateDenominator.GetHashCode();
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
            return
                (m_pixelWidth == other.m_pixelWidth) &&
                (m_pixelHeight == other.m_pixelHeight) &&
                (m_refreshRateNumerator == other.m_refreshRateNumerator) &&
                (m_refreshRateDenominator == other.m_refreshRateDenominator);
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
                return false;

            if (!ReferenceEquals(value.GetType(), typeof(EngineOutputInfo)))
                return false;

            return Equals((EngineOutputInfo)value);
        }

        public EngineOutputInfo HostOutput
        {
            get { return m_hostOutput; }
        }

        public int PixelWidth
        {
            get { return m_pixelWidth; }
        }

        public int PixelHeight
        {
            get { return m_pixelHeight; }
        }

        public int PixelCount
        {
            get { return m_pixelWidth * m_pixelHeight; }
        }

        public int RefreshRateNumerator
        {
            get { return m_refreshRateNumerator; }
        }

        public int RefreshRateDenominator
        {
            get { return m_refreshRateDenominator; }
        }
    }
}
