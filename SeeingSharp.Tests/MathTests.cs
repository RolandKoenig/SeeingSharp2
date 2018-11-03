#region License information
/*
    Seeing# and all games/applications distributed together with it. 
	Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
    Copyright (C) 2018 Roland König (RolandK)
    
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
#endregion
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
