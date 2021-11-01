using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests.Util
{
    [TestClass]
    public class RingBufferTests
    {
        [TestMethod]
        public void Test_RingBuffer_WriteSome()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 5; loop++)
            {
                testRingBuffer.Add(loop);
            }

            Assert.IsTrue(testRingBuffer.Count == 5);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
            Assert.IsTrue(testRingBuffer[4] == 4);
            Assert.IsTrue(testRingBuffer[2] == 2);
        }

        [TestMethod]
        public void Test_RingBuffer_WriteSome_ByRef()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 5; loop++)
            {
                ref var actRef = ref testRingBuffer.AddByRef();
                actRef = loop;
            }

            Assert.IsTrue(testRingBuffer.Count == 5);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
            Assert.IsTrue(testRingBuffer[4] == 4);
            Assert.IsTrue(testRingBuffer[2] == 2);
        }

        [TestMethod]
        public void Test_RingBuffer_WriteTwice()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 10; loop++)
            {
                testRingBuffer.Add(loop);
                testRingBuffer.Add(loop);
            }

            Assert.IsTrue(testRingBuffer.Count == 10);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
            Assert.IsTrue(testRingBuffer[9] == 9);
            Assert.IsTrue(testRingBuffer[0] == 5);
        }

        [TestMethod]
        public void Test_RingBuffer_WriteTwice_ByRef()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 10; loop++)
            {
                ref var actRef = ref testRingBuffer.AddByRef();
                actRef = loop;

                actRef = ref testRingBuffer.AddByRef();
                actRef = loop;
            }

            Assert.IsTrue(testRingBuffer.Count == 10);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
            Assert.IsTrue(testRingBuffer[9] == 9);
            Assert.IsTrue(testRingBuffer[0] == 5);
        }

        [TestMethod]
        public void Test_RingBuffer_WriteTwice_AndRemoveFirst()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 10; loop++)
            {
                testRingBuffer.Add(loop);
                testRingBuffer.Add(loop);
            }
            testRingBuffer.RemoveFirst();
            testRingBuffer.RemoveFirst();

            Assert.IsTrue(testRingBuffer.Count == 8);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
            Assert.IsTrue(testRingBuffer[7] == 9);
            Assert.IsTrue(testRingBuffer[0] == 6);
        }

        [TestMethod]
        public void Test_RingBuffer_RemoveFirst_SomeTimes()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 15; loop++) // Add 15 times
            {
                testRingBuffer.Add(loop);
            }
            for (var loop = 0; loop < 5; loop++) // Remove 5 times
            {
                testRingBuffer.RemoveFirst();
            }

            Assert.IsTrue(testRingBuffer.Count == 5);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
            Assert.IsTrue(testRingBuffer[4] == 14);
            Assert.IsTrue(testRingBuffer[0] == 10);
        }

        [TestMethod]
        public void Test_RingBuffer_RemoveFirst_All()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 15; loop++) // Add 15 times
            {
                testRingBuffer.Add(loop);
            }
            for (var loop = 0; loop < 10; loop++) // Remove 5 times
            {
                testRingBuffer.RemoveFirst();
            }

            Assert.IsTrue(testRingBuffer.Count == 0);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Test_RingBuffer_RemoveFirst_ToMuch()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 15; loop++) // Add 15 times
            {
                testRingBuffer.Add(loop);
            }
            for (var loop = 0; loop < 11; loop++) // Remove 5 times
            {
                testRingBuffer.RemoveFirst();
            }
        }

        [TestMethod]
        public void Test_RingBuffer_Clear()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 5; loop++)
            {
                testRingBuffer.Add(loop);
            }
            testRingBuffer.Clear();

            Assert.IsTrue(testRingBuffer.Count == 0);
            Assert.IsTrue(testRingBuffer.MaxCapacity == 10);
        }

        [TestMethod]
        [ExpectedException(typeof(IndexOutOfRangeException))]
        public void Test_RingBuffer_Get_WrongIndex()
        {
            var testRingBuffer = new RingBuffer<int>(10);

            for (var loop = 0; loop < 5; loop++)
            {
                testRingBuffer.Add(loop);
            }

            // ReSharper disable once UnusedVariable
            var test = testRingBuffer[6];
        }
    }
}
