﻿// This code is ported from SharpDX
// https://github.com/sharpdx/SharpDX

using System;
using System.Globalization;
using System.Runtime.InteropServices;

namespace SeeingSharp.Util.SdxTK
{
    /// <summary>
    /// A FourCC descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    public struct FourCC : IEquatable<FourCC>, IFormattable
    {
        /// <summary>
        /// Empty FourCC.
        /// </summary>
        public static readonly FourCC Empty = new FourCC(0);

        private uint value;

        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="fourCC">The fourCC value as a string .</param>
        public FourCC(string fourCC)
        {
            if (fourCC.Length != 4)
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "Invalid length for FourCC(\"{0}\". Must be be 4 characters long ", fourCC), "fourCC");
            this.value = ((uint)fourCC[3]) << 24 | ((uint)fourCC[2]) << 16 | ((uint)fourCC[1]) << 8 | ((uint)fourCC[0]);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="byte1">The byte1.</param>
        /// <param name="byte2">The byte2.</param>
        /// <param name="byte3">The byte3.</param>
        /// <param name="byte4">The byte4.</param>
        public FourCC(char byte1, char byte2, char byte3, char byte4)
        {
            this.value = ((uint)byte4) << 24 | ((uint)byte3) << 16 | ((uint)byte2) << 8 | ((uint)byte1);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="fourCC">The fourCC value as an uint.</param>
        public FourCC(uint fourCC)
        {
            this.value = fourCC;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="fourCC">The fourCC value as an int.</param>
        public FourCC(int fourCC)
        {
            this.value = unchecked((uint)fourCC);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SeeingSharp.Util.SdxTK.FourCC"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator uint(FourCC d)
        {
            return d.value;
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SeeingSharp.Util.SdxTK.FourCC"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator int(FourCC d)
        {
            return unchecked((int)d.value);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="SeeingSharp.Util.SdxTK.FourCC"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FourCC(uint d)
        {
            return new FourCC(d);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="SeeingSharp.Util.SdxTK.FourCC"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FourCC(int d)
        {
            return new FourCC(d);
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="SeeingSharp.Util.SdxTK.FourCC"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(FourCC d)
        {
            return d.ToString();
        }

        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="SeeingSharp.Util.SdxTK.FourCC"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FourCC(string d)
        {
            return new FourCC(d);
        }

        public override string ToString()
        {
            return string.Format("{0}", new string(new[]
                                  {
                                      (char) (value & 0xFF),
                                      (char) ((value >> 8) & 0xFF),
                                      (char) ((value >> 16) & 0xFF),
                                      (char) ((value >> 24) & 0xFF),
                                  }));
        }

        public bool Equals(FourCC other)
        {
            return value == other.value;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is FourCC && Equals((FourCC)obj);
        }

        public override int GetHashCode()
        {
            return (int)value;
        }

        /// <summary>
        /// Provides a custom string representation of the FourCC descriptor.
        /// </summary>
        /// <remarks>
        /// The general format "G" is equivalent to the parameterless.
        /// <see cref="FourCC.ToString()"/>. The special format "I" returns a
        /// string representation which can be used to construct a Media
        /// Foundation format GUID. It is equivalent to "X08".
        /// </remarks>
        /// <param name="format">The format descriptor, which can be "G" (empty
        /// or <c>null</c> is equivalent to "G"), "I" or any valid standard
        /// number format.</param>
        /// <param name="formatProvider">The format provider for formatting
        /// numbers.</param>
        /// <returns>The requested string representation.</returns>
        /// <exception cref="System.FormatException">In case of
        /// <paramref name="format"/> is not "G", "I" or a valid number
        /// format.</exception>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
            {
                format = "G";
            }
            if (formatProvider == null)
            {
                formatProvider = CultureInfo.CurrentCulture;
            }

            switch (format.ToUpperInvariant())
            {
                case "G":
                    return this.ToString();

                case "I":
                    return this.value.ToString("X08", formatProvider);

                default:
                    return this.value.ToString(format, formatProvider);
            }
        }

        public static bool operator ==(FourCC left, FourCC right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FourCC left, FourCC right)
        {
            return !left.Equals(right);
        }
    }
}
