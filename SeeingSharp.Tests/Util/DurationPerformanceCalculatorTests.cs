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
            var calculator = new DurationPerformanceCalculator("Test", maxHistoricalItems: 1000);

            var start = new DateTime(2020, 1, 29, 0, 0, 0, 0);
            for (var loop = 0; loop < 2000; loop++)
            {
                var actTimeStamp = start.AddSeconds(loop);
                calculator.NotifyActivityDuration(100 + (loop % 20 - 10), actTimeStamp);
            }
            
            Assert.IsTrue(calculator.RawDataEntries == 1000);

            //var calcResult = calculator.Calculate(
            //    start.AddSeconds(1500), start.AddSeconds(1500), start.AddSeconds(1600),
            //    TimeSpan.FromSeconds(100));

            
        }

    }
}
