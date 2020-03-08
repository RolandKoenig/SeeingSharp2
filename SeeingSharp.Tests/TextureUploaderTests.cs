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

using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia;
using SeeingSharp.Multimedia.Core;
using SeeingSharp.Util;

namespace SeeingSharp.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class TextureUploaderTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Multimedia TextureUploader";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
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
        [TestCategory(TEST_CATEGORY)]
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
        [TestCategory(TEST_CATEGORY)]
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