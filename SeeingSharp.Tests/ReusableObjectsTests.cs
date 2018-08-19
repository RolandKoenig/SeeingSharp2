using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            using (cache.UseStringBuilder(out StringBuilder stringBuilderFirst, 256))
            {
                stringBuilderFirst.AppendLine("Test01");
                stringBuilderFirst.AppendLine("Test02");
                rememberedStringBuilder = stringBuilderFirst;
            }

            Assert.IsTrue(cache.Count == 1);
            using(cache.UseStringBuilder(out StringBuilder stringBuilderSecond))
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
            using (cache.UseMemoryStream(out MemoryStream memoryStreamFirst, 256))
            using (var streamWriter = new StreamWriter(memoryStreamFirst))
            {
                streamWriter.WriteLine("Test01");
                streamWriter.WriteLine("Test02");
                rememberedMemoryStreamFirst = memoryStreamFirst;
            }

            Assert.IsTrue(cache.Count == 1);
            MemoryStream rememberedMemoryStreamSecond = null;
            using (cache.UseMemoryStream(out MemoryStream memoryStreamSecond))
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
