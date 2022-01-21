using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests.AssemblyResourcesAndShaderTests
{
    [TestClass]
    public class AssemblyResourceLinkTests
    {
        [TestMethod]
        public void Check_OpenForReading_RelativeNamespace()
        {
            var resLink = new AssemblyResourceLink(this.GetType(),
                "Dummy",
                "CommonPixelShader.hlsl");

            var foundIncludeLine = false;
            using (var inStream = resLink.OpenRead())
            using (var inStreamReader = new StreamReader(inStream))
            {
                string? actLine;
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
        public void Check_OpenForReading_AnotherFile()
        {
            var resLink = new AssemblyResourceLink(this.GetType(),
                "Dummy",
                "CommonPixelShader.hlsl");
            resLink = resLink.GetForAnotherFile("CommonPixelShader.hlsl", "..", "Dummy2");

            var foundIncludeLine = false;
            using (var inStream = resLink.OpenRead())
            using (var inStreamReader = new StreamReader(inStream))
            {
                string? actLine;
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
        public void Check_OpenForReading_FullNamespace()
        {
            var resLink = new AssemblyResourceLink(this.GetType().Assembly,
                "SeeingSharp.Tests.AssemblyResourcesAndShaderTests.Dummy",
                "CommonPixelShader.hlsl");

            var foundIncludeLine = false;
            using (var inStream = resLink.OpenRead())
            using (var inStreamReader = new StreamReader(inStream))
            {
                string? actLine;
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