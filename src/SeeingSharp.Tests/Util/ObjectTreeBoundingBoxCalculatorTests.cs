using System.Numerics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Util;

namespace SeeingSharp.Tests.Util
{
    [TestClass]
    public class ObjectTreeBoundingBoxCalculatorTests
    {
        [TestMethod]
        public void Test_DefaultUsage()
        {
            var calculator = new ObjectTreeBoundingBoxCalculator(); 
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));
            calculator.AddCoordinate(new Vector3(1f, 1f, 1f));

            Assert.IsTrue(calculator.CanCreateBoundingBox);

            var boundingBox = calculator.CreateBoundingBox();

            Assert.IsTrue(boundingBox.Minimum.Equals(new Vector3(-1f, 0f, -1f)));
            Assert.IsTrue(boundingBox.Maximum.Equals(new Vector3(1f, 1f, 1f)));
        }

        [TestMethod]
        public void Test_Empty()
        {
            var calculator = new ObjectTreeBoundingBoxCalculator(); 

            Assert.IsFalse(calculator.CanCreateBoundingBox);
        }

        [TestMethod]
        public void Test_SingleCoordinate()
        {
            var calculator = new ObjectTreeBoundingBoxCalculator(); 
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));

            Assert.IsFalse(calculator.CanCreateBoundingBox);
        }

        [TestMethod]
        public void Test_MoreEqualCoordinates()
        {
            var calculator = new ObjectTreeBoundingBoxCalculator(); 
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));

            Assert.IsFalse(calculator.CanCreateBoundingBox);
        }

        [TestMethod]
        [ExpectedException(typeof(SeeingSharpException))]
        public void Test_MoreEqualCoordinates_CreateBoxException()
        {
            var calculator = new ObjectTreeBoundingBoxCalculator(); 
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));
            calculator.AddCoordinate(new Vector3(-1f, 0f, -1f));

            Assert.IsFalse(calculator.CanCreateBoundingBox);

            calculator.CreateBoundingBox();
        }
    }
}
