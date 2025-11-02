using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Lab_1.Models;

namespace Lab_1_UnitTests
{
    [TestClass]
    public class SpreadsheetTests
    {
        #region Constructor and GetCell Tests

        [TestMethod]
        public void Constructor_DefaultValues()
        {
            var sheet = new Spreadsheet();
            Assert.AreEqual(50, sheet.RowCount);
            Assert.AreEqual(26, sheet.ColumnCount);
        }

        [TestMethod]
        public void GetCell_ValidCoordinates()
        {
            var sheet = new Spreadsheet(10, 10);
            var cell = sheet.GetCell(5, "A");
            Assert.IsNotNull(cell);
        }

        [TestMethod]
        public void GetCell_OutOfBounds()
        {
            var sheet = new Spreadsheet(10, 10);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => sheet.GetCell(10, "A"));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => sheet.GetCell(0, "K"));
        }

        #endregion

        #region Structural Changes Tests (Insert/Delete)

        [TestMethod]
        public void InsertRow_InMiddle()
        {
            var sheet = new Spreadsheet(10, 10);
            sheet.SetCellInput(5, "C", "OriginalValue");

            sheet.InsertRow(2);

            Assert.AreEqual(11, sheet.RowCount);
            Assert.AreEqual("OriginalValue", sheet.GetCell(6, "C").Input);
            Assert.AreEqual(string.Empty, sheet.GetCell(5, "C").Input);
        }

        [TestMethod]
        public void DeleteRow_FromMiddle()
        {
            var sheet = new Spreadsheet(10, 10);
            sheet.SetCellInput(5, "C", "ValueToShiftUp");
            sheet.SetCellInput(2, "A", "ValueToDelete");

            sheet.DeleteRow(2);

            Assert.AreEqual(9, sheet.RowCount); 
            Assert.AreEqual("ValueToShiftUp", sheet.GetCell(4, "C").Input);
            Assert.AreEqual(string.Empty, sheet.GetCell(2, "A").Input);
        }

        #endregion
    }
}
