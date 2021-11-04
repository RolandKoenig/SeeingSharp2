using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp;
using SeeingSharp.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TextureUploaderTests
    {
        [TestMethod]
        public void SimpleUpload_ColorBitmap()
        {
            var coreConfig = new GraphicsCoreConfiguration();
            coreConfig.DebugEnabled = true;

            var testResource = new AssemblyResourceLink(
                typeof(TextureUploaderTests), "Resources.TextureUploader", "TestTexture.png");

            using (var engineFactory = new EngineFactory(coreConfig))
            using (var device = EngineDevice.CreateSoftwareDevice(engineFactory))
            using (var colorTexture = GraphicsHelper.CreateTexture(device, testResource))
            using (var texUploader = TextureUploader.ConstructUsingPropertiesFromTexture(device, colorTexture))
            using (var uploaded = texUploader.UploadToMemoryMappedTexture<int>(colorTexture))
            {
                Assert.IsTrue(new Color4(uploaded[224, 326]).EqualsWithTolerance(Color4.RedColor));
                Assert.IsTrue(new Color4(uploaded[10, 10]).EqualsWithTolerance(Color4.White));
                Assert.IsTrue(new Color4(uploaded[561, 261]).EqualsWithTolerance(new Color4(0, 38, 255)));
                Assert.IsTrue(new Color4(uploaded[538, 669]).EqualsWithTolerance(new Color4(255, 0, 220)));
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SeeingSharpGraphicsException))]
        public void SimpleUpload_ColorBitmap_WrongUploadFormat()
        {
            var coreConfig = new GraphicsCoreConfiguration();
            coreConfig.DebugEnabled = true;

            var testResource = new AssemblyResourceLink(
                typeof(TextureUploaderTests), "Resources.TextureUploader", "TestTexture.png");

            using (var engineFactory = new EngineFactory(coreConfig))
            using (var device = EngineDevice.CreateSoftwareDevice(engineFactory))
            using (var colorTexture = GraphicsHelper.CreateTexture(device, testResource))
            using (var texUploader = TextureUploader.ConstructUsingPropertiesFromTexture(device, colorTexture))
            using (texUploader.UploadToMemoryMappedTexture<Vector3>(colorTexture))
            {

            }
        }

        [TestMethod]
        [ExpectedException(typeof(SeeingSharpGraphicsException))]
        public void SimpleUpload_ColorBitmap_WrongTextureSize()
        {
            var coreConfig = new GraphicsCoreConfiguration();
            coreConfig.DebugEnabled = true;

            var testResource = new AssemblyResourceLink(
                typeof(TextureUploaderTests), "Resources.TextureUploader", "TestTexture.png");

            using (var engineFactory = new EngineFactory(coreConfig))
            using (var device = EngineDevice.CreateSoftwareDevice(engineFactory))
            using (var colorTexture = GraphicsHelper.CreateTexture(device, testResource))
            using (var texUploader = new TextureUploader(device, 100, 100, GraphicsHelper.Internals.DEFAULT_TEXTURE_FORMAT, true))
            using (texUploader.UploadToMemoryMappedTexture<int>(colorTexture))
            {

            }
        }
    }
}