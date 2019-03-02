#region License information
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
#endregion

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests
{
    #region using
    #endregion

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
            ResourceLink extPNG = new AssemblyResourceLink(GetType(), "DummyNamespace.DummyFile.png");
            var extJPG = extPNG.GetForAnotherFile("Dummy.jpg");

            Assert.IsTrue(extPNG.FileExtension == "png");
            Assert.IsTrue(extJPG.FileExtension == "jpg");
        }
    }
}
