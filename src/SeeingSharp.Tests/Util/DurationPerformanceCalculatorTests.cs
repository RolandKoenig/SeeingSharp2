/*
    SeeingSharp and all applications distributed together with it. 
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
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests.Util
{
    [TestClass]
    public class DurationPerformanceCalculatorTests
    {
        [TestMethod]
        public void Test_DurationCalculation_Default()
        {
            var calculator = new DurationPerformanceCalculator("Dummy", 1000);

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
            Assert.IsTrue(calcResult1.SumAverageMs > 99);
            Assert.IsTrue(calcResult1.SumAverageMs < 101);
            Assert.IsTrue(calcResult2.SumAverageMs > 99);
            Assert.IsTrue(calcResult2.SumAverageMs < 101);
            Assert.IsTrue(calculator.RawDataEntries == 300);
        }
    }
}
