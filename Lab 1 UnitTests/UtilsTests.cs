using Lab_1.Models;

namespace Lab_1_UnitTests
{
    [TestClass]
    public class SpreadsheetUtilsTests
    {
        [TestMethod]
        public void ToColumnName_Index26()
        {
            int columnIndex = 702;
            string expected = "AAA";

            string actual = SpreadsheetUtils.ToColumnName(columnIndex);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod]
        public void ToColumnIndex_NameAB()
        {
            string columnName = "AB";
            int expected = 27;

            int actual = SpreadsheetUtils.ToColumnIndex(columnName);

            Assert.AreEqual(expected, actual);
        }
    }
}
