﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class ResourceLinkTests
    {
        [TestMethod]
        public void GetFileExtension_StandardFile()
        {
            ResourceLink extCS = new FileSystemResourceLink("C:/Blub/Blub.cs");
            ResourceLink extC = new FileSystemResourceLink("C:/Blub/Blub.c");
            ResourceLink extNull = new FileSystemResourceLink("C:/Club/Blub");
            ResourceLink extVB = new FileSystemResourceLink("C:/Club/Blub.cs.vb");

            Assert.IsTrue(extCS.FileExtension == "cs");
            Assert.IsTrue(extC.FileExtension == "c");
            Assert.IsTrue(extNull.FileExtension == "");
            Assert.IsTrue(extVB.FileExtension == "vb");
        }

        [TestMethod]
        public void GetFileExtension_AssemblyResourceLink()
        {
            ResourceLink extPNG = new AssemblyResourceLink(this.GetType(), "DummyNamespace.DummyFile.png");
            var extJPG = extPNG.GetForAnotherFile("Dummy.jpg");

            Assert.IsTrue(extPNG.FileExtension == "png");
            Assert.IsTrue(extJPG.FileExtension == "jpg");
        }
    }
}
