/*
    Seeing# and all applications distributed together with it. 
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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SeeingSharp.Tests.Util;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class BitmapComparisonTests
    {
        public const string TEST_CATEGORY = "SeeingSharp Util BitmapComparison";

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void BitmapComparison_Positive()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            {
                Assert.IsTrue(
                    BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap) == 0f);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void BitmapComparison_Negative_Without_Model()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "ClearedScreen.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            {
                var comparisonResult = BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap);
                Assert.IsTrue(comparisonResult > 0.25f);
                Assert.IsTrue(comparisonResult < 0.6f);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void BitmapComparison_Negative_Inversed_Image()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject_Negative.png"))
            {
                var comparisonResult = BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap);
                Assert.IsTrue(comparisonResult > 0.9f);
                Assert.IsTrue(comparisonResult <= 1.0f);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void BitmapComparison_Negative_BlackWhite()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "WhiteScreen.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "BlackScreen.png"))
            {
                var comparisonResult = BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap);
                Assert.IsTrue(comparisonResult == 1.0f);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void BitmapComparison_Negative_Enlighted()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject_Enlighted.png"))
            {
                var comparisonResult = BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap);
                Assert.IsTrue(comparisonResult > 0.1);
                Assert.IsTrue(comparisonResult < 0.4);
            }
        }

        [TestMethod]
        [TestCategory(TEST_CATEGORY)]
        public void BitmapComparison_Negative_Smaller()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject_Smaller.png"))
            {
                var comparisonResult = BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap);
                Assert.IsTrue(comparisonResult > 0.1);
                Assert.IsTrue(comparisonResult < 0.4);
            }
        }
    }
}