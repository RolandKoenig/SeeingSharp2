using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SeeingSharp.Tests
{
    [TestClass]
    public class BitmapComparisonTests
    {
        [TestMethod]
        public void BitmapComparison_Positive()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "FlatShadedObject.png"))
            {
                Assert.IsTrue(
                    EngineMath.EqualsWithTolerance(BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap), 0f));
            }
        }

        [TestMethod]
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
        public void BitmapComparison_Negative_Inverted_Image()
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
        public void BitmapComparison_Negative_BlackWhite()
        {
            using (var leftBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "WhiteScreen.png"))
            using (var rightBitmap = TestUtilities.LoadBitmapFromResource("BitmapComparison", "BlackScreen.png"))
            {
                var comparisonResult = BitmapComparison.CalculatePercentageDifference(leftBitmap, rightBitmap);
                Assert.IsTrue(EngineMath.EqualsWithTolerance(comparisonResult, 1.0f));
            }
        }

        [TestMethod]
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