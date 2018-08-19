using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Tests.AssemblyResourcesAndShaderTests
{
    [TestClass]
    public class ShaderCompileTests
    {
        private const string TEST_CATEGORY = "SeeingSharp Core ShaderCompile";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_ReadAndMergeFromAssemblyResources()
        {
            AssemblyResourceLink resLink = new AssemblyResourceLink(
                this.GetType(),
                "Dummy",
                "CommonPixelShader.hlsl");

            StringBuilder singleShaderFileBuilder = new StringBuilder(1024);
            SingleShaderFileBuilder.ReadShaderFileAndResolveIncludes(resLink, singleShaderFileBuilder);
            string result = singleShaderFileBuilder.ToString();

            Assert.IsTrue(result.Contains("constants.hlsl"));
            Assert.IsTrue(result.Contains("struct VSInputStandard"));
            Assert.IsTrue(result.Contains("float4 ApplyColorBorders(float4 inputColor, float distanceToCamera, float2 texCoord)"));
        }
    }
}
