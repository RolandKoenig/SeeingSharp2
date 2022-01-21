// This code is ported from SharpDX.Toolkit
// see: https://github.com/sharpdx/Toolkit

using System;

#nullable disable

namespace SeeingSharp.Util.SdxTK
{
    internal static class MipMapHelper
    {
        /// <summary>
        /// Calculates the number of miplevels for a Texture 2D.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="mipLevels">A <see cref="MipMapCount"/>, set to true to calculates all mipmaps, to false to calculate only 1 miplevel, or > 1 to calculate a specific amount of levels.</param>
        /// <returns>The number of miplevels.</returns>
        public static int CalculateMipLevels(int width, int height, MipMapCount mipLevels)
        {
            if (mipLevels > 1)
            {
                var maxMips = CountMips(width, height);
                if (mipLevels > maxMips)
                {
                    throw new InvalidOperationException($"MipLevels must be <= {maxMips}");
                }
            }
            else if (mipLevels == 0)
            {
                mipLevels = CountMips(width, height);
            }
            else
            {
                mipLevels = 1;
            }
            return mipLevels;
        }

        /// <summary>
        /// Calculates the number of miplevels for a Texture 2D.
        /// </summary>
        /// <param name="width">The width of the texture.</param>
        /// <param name="height">The height of the texture.</param>
        /// <param name="depth">The depth of the texture.</param>
        /// <param name="mipLevels">A <see cref="MipMapCount"/>, set to true to calculates all mipmaps, to false to calculate only 1 miplevel, or > 1 to calculate a specific amount of levels.</param>
        /// <returns>The number of miplevels.</returns>
        public static int CalculateMipLevels(int width, int height, int depth, MipMapCount mipLevels)
        {
            if (mipLevels > 1)
            {
                if (!IsPow2(width) || !IsPow2(height) || !IsPow2(depth))
                {
                    throw new InvalidOperationException("Width/Height/Depth must be power of 2");
                }

                var maxMips = CountMips(width, height, depth);
                if (mipLevels > maxMips)
                {
                    throw new InvalidOperationException($"MipLevels must be <= {maxMips}");
                }
            }
            else if (mipLevels == 0)
            {
                if (!IsPow2(width) || !IsPow2(height) || !IsPow2(depth))
                {
                    throw new InvalidOperationException("Width/Height/Depth must be power of 2");
                }

                mipLevels = CountMips(width, height, depth);
            }
            else
            {
                mipLevels = 1;
            }
            return mipLevels;
        }

        private static bool IsPow2(int x)
        {
            return x != 0 && (x & (x - 1)) == 0;
        }

        private static int CountMips(int width, int height)
        {
            var mipLevels = 1;

            while (height > 1 || width > 1)
            {
                ++mipLevels;

                if (height > 1)
                {
                    height >>= 1;
                }

                if (width > 1)
                {
                    width >>= 1;
                }
            }

            return mipLevels;
        }

        private static int CountMips(int width, int height, int depth)
        {
            var mipLevels = 1;

            while (height > 1 || width > 1 || depth > 1)
            {
                ++mipLevels;

                if (height > 1)
                {
                    height >>= 1;
                }

                if (width > 1)
                {
                    width >>= 1;
                }

                if (depth > 1)
                {
                    depth >>= 1;
                }
            }

            return mipLevels;
        }
    }
}
