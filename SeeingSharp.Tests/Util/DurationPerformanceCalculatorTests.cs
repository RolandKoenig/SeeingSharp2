using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests.Util
{
    [TestClass]
    public class DurationPerformanceCalculatorTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Util PerformanceCalculator Duration";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Test_DurationCalculation_Default()
        {
            var calculator = new DurationPerformanceCalculator("Dummy", maxHistoricalItems: 1000);

            var start = new DateTime(2020, 1, 29, 0, 0, 0, 0);
            for (var loop = 0; loop < 2000; loop++)
            {
                var actTimeStamp = start.AddSeconds(loop);
                calculator.NotifyActivityDuration((100 + (loop % 20 - 10)) * 10000L, actTimeStamp);
            }

            Assert.IsTrue(calculator.RawDataEntries == 1000);

            DurationPerformanceResult calcResult1 = null;
            DurationPerformanceResult calcResult2 = null;
            calculator.Calculate(
                ref calcResult1,
                start.AddSeconds(1500), start.AddSeconds(1600));
            calculator.Calculate(
                ref calcResult2,
                start.AddSeconds(1600), start.AddSeconds(1700));

            Assert.IsNotNull(calcResult1);
            Assert.IsNotNull(calcResult2);
            Assert.IsTrue(calcResult1.SumAverageMS > 99);
            Assert.IsTrue(calcResult1.SumAverageMS < 101);
            Assert.IsTrue(calcResult2.SumAverageMS > 99);
            Assert.IsTrue(calcResult2.SumAverageMS < 101);
            Assert.IsTrue(calculator.RawDataEntries == 300);
        }
    }
}
