using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Tests.AssemblyResourcesAndShaderTests
{
    [TestClass]
    public class AssemblyResourceLinkTests
    {
        private const string TEST_CATEGORY = "SeeingSharp Core AssemblyResourceLink";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_OpenForReading_RelativeNamespace()
        {
            AssemblyResourceLink resLink = new AssemblyResourceLink(
                this.GetType(),
                "Dummy",
                "CommonPixelShader.hlsl");

            bool foundIncludeLine = false;
            using (var inStream = resLink.OpenRead())
            using (var inStreamReader = new StreamReader(inStream))
            {
                string actLine;
                while(null != (actLine = inStreamReader.ReadLine()))
                {
                    if (actLine.StartsWith("#include"))
                    {
                        foundIncludeLine = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(foundIncludeLine);
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_OpenForReading_AnotherFile()
        {
            AssemblyResourceLink resLink = new AssemblyResourceLink(
                this.GetType(),
                "Dummy",
                "CommonPixelShader.hlsl");
            resLink = resLink.GetForAnotherFile("CommonPixelShader.hlsl", "..", "Dummy2");

            bool foundIncludeLine = false;
            using (var inStream = resLink.OpenRead())
            using (var inStreamReader = new StreamReader(inStream))
            {
                string actLine;
                while (null != (actLine = inStreamReader.ReadLine()))
                {
                    if (actLine.StartsWith("#include"))
                    {
                        foundIncludeLine = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(foundIncludeLine);
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_OpenForReading_FullNamespace()
        {
            AssemblyResourceLink resLink = new AssemblyResourceLink(
                this.GetType().Assembly,
                "SeeingSharp.Tests.AssemblyResourcesAndShaderTests.Dummy",
                "CommonPixelShader.hlsl");

            bool foundIncludeLine = false;
            using (var inStream = resLink.OpenRead())
            using (var inStreamReader = new StreamReader(inStream))
            {
                string actLine;
                while (null != (actLine = inStreamReader.ReadLine()))
                {
                    if (actLine.StartsWith("#include"))
                    {
                        foundIncludeLine = true;
                        break;
                    }
                }
            }

            Assert.IsTrue(foundIncludeLine);
        }
    }
}
