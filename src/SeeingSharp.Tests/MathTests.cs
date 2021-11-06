using System;
using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Mathematics;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class MathTests
    {
        [TestMethod]
        public void Movemenet_SpeedBased()
        {
            var palletConveyorSpeed = new MovementSpeed(0.3f);
            var trayConveyorSpeed = new MovementSpeed(0.8f);

            var animHelper1 = new MovementAnimationHelper(palletConveyorSpeed, new Vector3(5f, 0f, 0f));
            var animHelper2 = new MovementAnimationHelper(palletConveyorSpeed, new Vector3(3f, 4f, 2f));
            var animHelper3 = new MovementAnimationHelper(trayConveyorSpeed, new Vector3(5f, 0f, 0f));

            Assert.IsTrue((int)Math.Round(animHelper1.MovementTime.TotalMilliseconds) == 16667);
            Assert.IsTrue((int)Math.Round(animHelper2.MovementTime.TotalMilliseconds) == 17951);
            Assert.IsTrue((int)Math.Round(animHelper3.MovementTime.TotalMilliseconds) == 6250);
        }

        [TestMethod]
        public void Movement_SpeedBased_FixedTime()
        {
            var movementVector = new Vector3(10f, 0f, 8f);
            var movementSpeed = new MovementSpeed(movementVector, TimeSpan.FromSeconds(5.0));

            var animHelper = new MovementAnimationHelper(movementSpeed, movementVector);

            Assert.IsTrue(animHelper.MovementTime == TimeSpan.FromSeconds(5.0));
            Assert.IsTrue(animHelper.AccelerationTime == TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime == TimeSpan.Zero);
        }

        [TestMethod]
        public void Movement_AccDecFull_ShortRun()
        {
            // Configuration
            var movementSpeed = new MovementSpeed(1.5f, 0.8f, -0.4f);
            var movementVector = new Vector3(0f, 0f, -0.14f);

            // Action
            var animHelper = new MovementAnimationHelper(movementSpeed, movementVector);

            // Asserts
            Assert.IsTrue(animHelper.AccelerationTime > TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime > TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime > animHelper.AccelerationTime);
            Assert.IsTrue(animHelper.FullSpeedTime == TimeSpan.Zero);
            Assert.IsTrue(animHelper.MovementTime < TimeSpan.FromSeconds(15.0));
        }

        [TestMethod]
        public void Movement_AccDecFull_LongRun()
        {
            // Configuration
            var movementSpeed = new MovementSpeed(1.5f, 0.6f, -0.5f);
            var movementVector = new Vector3(10f, 0f, 8f);

            // Action
            var animHelper = new MovementAnimationHelper(movementSpeed, movementVector);

            // Asserts
            Assert.IsTrue(animHelper.AccelerationTime > TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime > TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime > animHelper.AccelerationTime);
            Assert.IsTrue(animHelper.FullSpeedTime > animHelper.AccelerationTime);
            Assert.IsTrue(animHelper.FullSpeedTime > animHelper.DecelerationTime);
            Assert.IsTrue(animHelper.MovementTime < TimeSpan.FromSeconds(15.0));
        }
    }
}