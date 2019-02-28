#region License information
/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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
#endregion

namespace SeeingSharp.Tests.AssemblyResourcesAndShaderTests
{
    #region using

    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using SeeingSharp.Util;

    #endregion

    [TestClass]
    public class ShaderCompileTests
    {
        private const string TEST_CATEGORY = "SeeingSharp Core ShaderCompile";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_ReadAndMergeFromAssemblyResources()
        {
            var resLink = new AssemblyResourceLink(
                this.GetType(),
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