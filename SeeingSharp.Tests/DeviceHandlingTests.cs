using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Multimedia.Core;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class DeviceHandlingTests
    {
        private const string TEST_CATEGORY = "SeeingSharp Core DeviceHandling";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Check_QueryAdapters()
        {
            using (EngineHardwareInfo hardwareInfo = new EngineHardwareInfo())
            {
                Assert.IsTrue(hardwareInfo.Adapters.Count > 0);
                Assert.IsNotNull(hardwareInfo.SoftwareAdapter);
            }
        }
    }
}
