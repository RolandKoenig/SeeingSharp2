using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests.AssemblyResourcesAndShaderTests
{
    [TestClass]
    public class ShaderCompileTests
    {
        [TestMethod]
        public void Check_ReadAndMergeFromAssemblyResources()
        {
            var resLink = new AssemblyResourceLink(this.GetType(),
                "Dummy",
                "CommonPixelShader.hlsl");

            var singleShaderFileBuilder = new StringBuilder(1024);
            SingleShaderFileBuilder.ReadShaderFileAndResolveIncludes(resLink, singleShaderFileBuilder);
            var result = singleShaderFileBuilder.ToString();

            Assert.IsTrue(result.Contains("constants.hlsl"));
            Assert.IsTrue(result.Contains("struct VSInputStandard"));
            Assert.IsTrue(result.Contains("float4 ApplyColorBorders(float4 inputColor, float distanceToCamera, float2 texCoord)"));
        }
    }
}