using System;
using System.Drawing;

namespace SeeingSharp.Views
{
    public static class DpiScalingProviderExtensions
    {
        /// <summary>
        /// Transforms the given coordinate from pixel to dip (device independent pixel).
        /// </summary>
        public static float TransformXCoordinateFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider, 
            float xCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return xCoordinate / Math.Max(currentDpiScaling.ScaleFactorX, 1f);
        }

        /// <summary>
        /// Transforms the given coordinate from pixel to dip (device independent pixel).
        /// </summary>
        public static float TransformYCoordinateFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider,
            float yCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return yCoordinate / Math.Max(currentDpiScaling.ScaleFactorY, 1f);
        }

        /// <summary>
        /// Transforms the given coordinate from dip (device independent pixel) to pixel.
        /// </summary>
        public static float TransformXCoordinateFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            float xCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return xCoordinate * currentDpiScaling.ScaleFactorX;
        }

        /// <summary>
        /// Transforms the given coordinate from dip (device independent pixel) to pixel.
        /// </summary>
        public static float TransformYCoordinateFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            float yCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return yCoordinate * currentDpiScaling.ScaleFactorY;
        }

        /// <summary>
        /// Transforms the given coordinate from pixel to dip (device independent pixel).
        /// </summary>
        public static double TransformXCoordinateFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider, 
            double xCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return xCoordinate / Math.Max(currentDpiScaling.ScaleFactorX, 1f);
        }

        /// <summary>
        /// Transforms the given coordinate from pixel to dip (device independent pixel).
        /// </summary>
        public static double TransformYCoordinateFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider,
            double yCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return yCoordinate / Math.Max(currentDpiScaling.ScaleFactorY, 1f);
        }

        /// <summary>
        /// Transforms the given coordinate from dip (device independent pixel) to pixel.
        /// </summary>
        public static double TransformXCoordinateFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            double xCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return xCoordinate * currentDpiScaling.ScaleFactorX;
        }

        /// <summary>
        /// Transforms the given coordinate from dip (device independent pixel) to pixel.
        /// </summary>
        public static double TransformYCoordinateFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            double yCoordinate)
        {
            var currentDpiScaling = dpiScalingProvider.GetCurrentDpiScaling();
            return yCoordinate * currentDpiScaling.ScaleFactorY;
        }

        /// <summary>
        /// Transforms the given point from pixel to dip (device independent pixel).
        /// </summary>
        public static Point TransformPointFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider,
            Point point)
        {
            return new Point(
                (int)dpiScalingProvider.TransformXCoordinateFromPixelToDip(point.X),
                (int)dpiScalingProvider.TransformYCoordinateFromPixelToDip(point.Y));
        }

        /// <summary>
        /// Transforms the given point from dip (device independent pixel) to pixel.
        /// </summary>
        public static Point TransformPointFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            Point point)
        {
            return new Point(
                (int)dpiScalingProvider.TransformXCoordinateFromDipToPixel(point.X),
                (int)dpiScalingProvider.TransformYCoordinateFromDipToPixel(point.Y));
        }

        /// <summary>
        /// Transforms the given point from pixel to dip (device independent pixel).
        /// </summary>
        public static PointF TransformPointFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider,
            PointF point)
        {
            return new PointF(
                dpiScalingProvider.TransformXCoordinateFromPixelToDip(point.X),
                dpiScalingProvider.TransformYCoordinateFromPixelToDip(point.Y));
        }

        /// <summary>
        /// Transforms the given point from dip (device independent pixel) to pixel.
        /// </summary>
        public static PointF TransformPointFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            PointF point)
        {
            return new PointF(
                dpiScalingProvider.TransformXCoordinateFromDipToPixel(point.X),
                dpiScalingProvider.TransformYCoordinateFromDipToPixel(point.Y));
        }

        /// <summary>
        /// Transforms the given size from pixel to dip (device independent pixel).
        /// </summary>
        public static Size TransformSizeFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider,
            Size size)
        {
            return new Size(
                (int)dpiScalingProvider.TransformXCoordinateFromPixelToDip(size.Width),
                (int)dpiScalingProvider.TransformYCoordinateFromPixelToDip(size.Height));
        }

        /// <summary>
        /// Transforms the given size from dip (device independent pixel) to pixel.
        /// </summary>
        public static Size TransformSizeFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            Size size)
        {
            return new Size(
                (int)dpiScalingProvider.TransformXCoordinateFromDipToPixel(size.Width),
                (int)dpiScalingProvider.TransformYCoordinateFromDipToPixel(size.Height));
        }

        /// <summary>
        /// Transforms the given size from pixel to dip (device independent pixel).
        /// </summary>
        public static SizeF TransformSizeFromPixelToDip(
            this IDpiScalingProvider dpiScalingProvider,
            SizeF size)
        {
            return new SizeF(
                dpiScalingProvider.TransformXCoordinateFromPixelToDip(size.Width),
                dpiScalingProvider.TransformYCoordinateFromPixelToDip(size.Height));
        }

        /// <summary>
        /// Transforms the given size from dip (device independent pixel) to pixel.
        /// </summary>
        public static SizeF TransformSizeFromDipToPixel(
            this IDpiScalingProvider dpiScalingProvider,
            SizeF size)
        {
            return new SizeF(
                dpiScalingProvider.TransformXCoordinateFromDipToPixel(size.Width),
                dpiScalingProvider.TransformYCoordinateFromDipToPixel(size.Height));
        }
    }
}
