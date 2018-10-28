using Microsoft.VisualStudio.TestTools.UnitTesting;
using SharpDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class MathTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Mathematics";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Movemenet_SpeedBased()
        {
            MovementSpeed palletConveyorSpeed = new MovementSpeed(0.3f);
            MovementSpeed trayConveyorSpeed = new MovementSpeed(0.8f);

            MovementAnimationHelper animHelper1 = new MovementAnimationHelper(palletConveyorSpeed, new Vector3(5f, 0f, 0f));
            MovementAnimationHelper animHelper2 = new MovementAnimationHelper(palletConveyorSpeed, new Vector3(3f, 4f, 2f));
            MovementAnimationHelper animHelper3 = new MovementAnimationHelper(trayConveyorSpeed, new Vector3(5f, 0f, 0f));

            Assert.IsTrue((int)animHelper1.MovementTime.TotalMilliseconds == 16667);
            Assert.IsTrue((int)animHelper2.MovementTime.TotalMilliseconds == 17951);
            Assert.IsTrue((int)animHelper3.MovementTime.TotalMilliseconds == 6250);
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Movement_SpeedBased_FixedTime()
        {
            Vector3 movementVector = new Vector3(10f, 0f, 8f);
            MovementSpeed movementSpeed = new MovementSpeed(movementVector, TimeSpan.FromSeconds(5.0));

            MovementAnimationHelper animHelper = new MovementAnimationHelper(movementSpeed, movementVector);

            Assert.IsTrue(animHelper.MovementTime == TimeSpan.FromSeconds(5.0));
            Assert.IsTrue(animHelper.AccelerationTime == TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime == TimeSpan.Zero);
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Movement_AccDecFull_ShortRun()
        {
            // Configuration
            MovementSpeed movementSpeed = new MovementSpeed(1.5f, 0.8f, -0.4f);
            Vector3 movementVector = new Vector3(0f, 0f, -0.14f);

            // Action
            MovementAnimationHelper animHelper = new MovementAnimationHelper(movementSpeed, movementVector);

            Vector3 pos = animHelper.GetPartialMoveDistance(animHelper.MovementTime);

            // Asserts
            Assert.IsTrue(animHelper.AccelerationTime > TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime > TimeSpan.Zero);
            Assert.IsTrue(animHelper.DecelerationTime > animHelper.AccelerationTime);
            Assert.IsTrue(animHelper.FullSpeedTime == TimeSpan.Zero);
            Assert.IsTrue(animHelper.MovementTime < TimeSpan.FromSeconds(15.0));
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void Movement_AccDecFull_LongRun()
        {
            // Configuration
            MovementSpeed movementSpeed = new MovementSpeed(1.5f, 0.6f, -0.5f);
            Vector3 movementVector = new Vector3(10f, 0f, 8f);

            // Action
            MovementAnimationHelper animHelper = new MovementAnimationHelper(movementSpeed, movementVector);

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
