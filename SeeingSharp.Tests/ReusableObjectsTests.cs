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

using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class ReusableObjectsTests
    {
        private const string TEST_CATEGORY = "SeeingSharp Util Reusable Objects";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_ReusableStringBuilder()
        {
            var cache = new ReusableStringBuilders();

            Assert.IsTrue(cache.Count == 0);
            StringBuilder rememberedStringBuilder = null;

            using (cache.UseStringBuilder(out var stringBuilderFirst, 256))
            {
                stringBuilderFirst.AppendLine("Test01");
                stringBuilderFirst.AppendLine("Test02");
                rememberedStringBuilder = stringBuilderFirst;
            }

            Assert.IsTrue(cache.Count == 1);

            using(cache.UseStringBuilder(out var stringBuilderSecond))
            {
                Assert.IsTrue(stringBuilderSecond.Length == 0);
                Assert.IsTrue(stringBuilderSecond.Capacity == 256);
                Assert.IsTrue(stringBuilderSecond == rememberedStringBuilder);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_ReusableMemoryStream()
        {
            var cache = new ReusableMemoryStreams();

            Assert.IsTrue(cache.Count == 0);
            MemoryStream rememberedMemoryStreamFirst = null;
            using (cache.UseMemoryStream(out var memoryStreamFirst, 256))

            using (var streamWriter = new StreamWriter(memoryStreamFirst))
            {
                streamWriter.WriteLine("Test01");
                streamWriter.WriteLine("Test02");
                rememberedMemoryStreamFirst = memoryStreamFirst;
            }

            Assert.IsTrue(cache.Count == 1);
            MemoryStream rememberedMemoryStreamSecond = null;

            using (cache.UseMemoryStream(out var memoryStreamSecond))
            {
                Assert.IsTrue(memoryStreamSecond.Length == 0);
                Assert.IsTrue(memoryStreamSecond.Capacity == 256);
                Assert.IsTrue(memoryStreamSecond != rememberedMemoryStreamFirst);
                rememberedMemoryStreamSecond = memoryStreamSecond;
            }

            Assert.IsTrue(rememberedMemoryStreamFirst.GetBuffer() == rememberedMemoryStreamSecond.GetBuffer());
        }
    }
}