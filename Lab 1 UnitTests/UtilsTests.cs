using Lab_1.Models;

namespace Lab_1_UnitTests
{
    [TestClass]
    public class SpreadsheetUtilsTests
    {
        [TestMethod]
        public void ToColumnName_Index26_ReturnsAA()
        {
            // Arrange
            int columnIndex = 702;
            string expected = "AAA";

            // Act
            string actual = SpreadsheetUtils.ToColumnName(columnIndex);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToColumnIndex_NameAB_Returns27()
        {
            // Arrange
            string columnName = "AB";
            int expected = 27;

            // Act
            int actual = SpreadsheetUtils.ToColumnIndex(columnName);

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
