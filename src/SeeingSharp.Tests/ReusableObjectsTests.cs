using System.IO;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class ReusableObjectsTests
    {
        [TestMethod]
        public void Check_ReusableStringBuilder()
        {
            var cache = new ReusableStringBuilders();

            Assert.IsTrue(cache.Count == 0);
            StringBuilder? rememberedStringBuilder = null;
            using (cache.UseStringBuilder(out var stringBuilderFirst, 256))
            {
                stringBuilderFirst.AppendLine("Test01");
                stringBuilderFirst.AppendLine("Test02");
                rememberedStringBuilder = stringBuilderFirst;
            }

            Assert.IsTrue(cache.Count == 1);
            using (cache.UseStringBuilder(out var stringBuilderSecond))
            {
                Assert.IsTrue(stringBuilderSecond.Length == 0);
                Assert.IsTrue(stringBuilderSecond.Capacity == 256);
                Assert.IsTrue(stringBuilderSecond == rememberedStringBuilder);
            }

            Assert.IsTrue(cache.Count == 1);
        }

        [TestMethod]
        public void Check_ReusableMemoryStream()
        {
            var cache = new ReusableMemoryStreams();

            Assert.IsTrue(cache.Count == 0);
            MemoryStream? rememberedMemoryStreamFirst = null;
            using (cache.UseMemoryStream(out var memoryStreamFirst, 256))
            using (var streamWriter = new StreamWriter(memoryStreamFirst, Encoding.UTF8, 1024, true))
            {
                streamWriter.WriteLine("Test01");
                streamWriter.WriteLine("Test02");
                rememberedMemoryStreamFirst = memoryStreamFirst;
            }

            Assert.IsTrue(cache.Count == 1);
            MemoryStream? rememberedMemoryStreamSecond = null;
            using (cache.UseMemoryStream(out var memoryStreamSecond))
            {
                Assert.IsTrue(memoryStreamSecond.Length == 0);
                Assert.IsTrue(memoryStreamSecond.Capacity == 256);
                Assert.IsTrue(memoryStreamSecond == rememberedMemoryStreamFirst);
                rememberedMemoryStreamSecond = memoryStreamSecond;
            }

            Assert.IsTrue(rememberedMemoryStreamFirst.GetBuffer() == rememberedMemoryStreamSecond.GetBuffer());
            Assert.IsTrue(cache.Count == 1);
        }
    }
}