using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class ResourceLinkTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Util ResourceLink";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
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
        [TestCategory(TEST_CATEGORY)]
        public void GetFileExtension_AssemblyResourceLink()
        {
            ResourceLink extPNG = new AssemblyResourceLink(this.GetType(), "DummyNamespace.DummyFile.png");
            ResourceLink extJPG = extPNG.GetForAnotherFile("Dummy.jpg");

            Assert.IsTrue(extPNG.FileExtension == "png");
            Assert.IsTrue(extJPG.FileExtension == "jpg");
        }
    }
}
