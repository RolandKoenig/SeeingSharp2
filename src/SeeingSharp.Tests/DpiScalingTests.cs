using System.Drawing;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Core;
using SeeingSharp.Views;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class DpiScalingTests
    {
        [TestMethod]
        public void ScalePointFFromPixelToDpi()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var pointPixel = new PointF(750f, 1500f);
            var pointDpi = dpiScalingProvider.TransformPointFromPixelToDip(pointPixel);

            // Assert
            Assert.AreEqual(500f, pointDpi.X);
            Assert.AreEqual(1000f, pointDpi.Y);
        }

        [TestMethod]
        public void ScalePointFFromDpiToPixel()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var pointDpi = new PointF(500f, 1000f);
            var pointPixel = dpiScalingProvider.TransformPointFromDipToPixel(pointDpi);

            // Assert
            Assert.AreEqual(750f, pointPixel.X);
            Assert.AreEqual(1500f, pointPixel.Y);
        }

        [TestMethod]
        public void ScalePointFromPixelToDpi()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var pointPixel = new Point(750, 1500);
            var pointDpi = dpiScalingProvider.TransformPointFromPixelToDip(pointPixel);

            // Assert
            Assert.AreEqual(500, pointDpi.X);
            Assert.AreEqual(1000, pointDpi.Y);
        }

        [TestMethod]
        public void ScalePointFromDpiToPixel()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var pointDpi = new Point(500, 1000);
            var pointPixel = dpiScalingProvider.TransformPointFromDipToPixel(pointDpi);

            // Assert
            Assert.AreEqual(750, pointPixel.X);
            Assert.AreEqual(1500, pointPixel.Y);
        }

        [TestMethod]
        public void ScaleSizeFFromPixelToDpi()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var sizePixel = new SizeF(750f, 1500f);
            var sizeDpi = dpiScalingProvider.TransformSizeFromPixelToDip(sizePixel);

            // Assert
            Assert.AreEqual(500f, sizeDpi.Width);
            Assert.AreEqual(1000f, sizeDpi.Height);
        }

        [TestMethod]
        public void ScaleSizeFFromDpiToPixel()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var sizeDpi = new SizeF(500f, 1000f);
            var sizePixel = dpiScalingProvider.TransformSizeFromDipToPixel(sizeDpi);

            // Assert
            Assert.AreEqual(750f, sizePixel.Width);
            Assert.AreEqual(1500f, sizePixel.Height);
        }

        [TestMethod]
        public void ScaleSizeFromPixelToDpi()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var sizePixel = new Size(750, 1500);
            var sizeDpi = dpiScalingProvider.TransformSizeFromPixelToDip(sizePixel);

            // Assert
            Assert.AreEqual(500, sizeDpi.Width);
            Assert.AreEqual(1000, sizeDpi.Height);
        }

        [TestMethod]
        public void ScaleSizeFromDpiToPixel()
        {
            // Arrange
            var dpiScaling = DpiScaling.GetByScaleFactors(1.5f, 1.5f);
            var dpiScalingProvider = A.Fake<IDpiScalingProvider>();
            A.CallTo(() => dpiScalingProvider.GetCurrentDpiScaling()).Returns(dpiScaling);

            // Act
            var sizeDpi = new Size(500, 1000);
            var sizePixel = dpiScalingProvider.TransformSizeFromDipToPixel(sizeDpi);

            // Assert
            Assert.AreEqual(750, sizePixel.Width);
            Assert.AreEqual(1500, sizePixel.Height);
        }
    }
}
