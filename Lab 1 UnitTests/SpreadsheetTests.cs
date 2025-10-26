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
        public void Constructor_DefaultValues_CreatesCorrectSize()
        {
            var sheet = new Spreadsheet();
            Assert.AreEqual(50, sheet.RowCount);
            Assert.AreEqual(26, sheet.ColumnCount);
        }

        [TestMethod]
        public void GetCell_ValidCoordinates_ReturnsCell()
        {
            var sheet = new Spreadsheet(10, 10);
            var cell = sheet.GetCell(5, "A");
            Assert.IsNotNull(cell);
        }

        [TestMethod]
        public void GetCell_OutOfBounds_ThrowsArgumentOutOfRangeException()
        {
            var sheet = new Spreadsheet(10, 10);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => sheet.GetCell(10, "A")); // Row is out of bounds
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => sheet.GetCell(0, "K")); // Column is out of bounds (K is 11th col)
        }

        #endregion

        #region Structural Changes Tests (Insert/Delete)

        [TestMethod]
        public void InsertRow_InMiddle_ShiftsLowerCellsDown()
        {
            // Arrange
            var sheet = new Spreadsheet(10, 10);
            sheet.SetCellInput(5, "C", "OriginalValue"); // Cell at (5, C)

            // Act
            sheet.InsertRow(2); // Insert new row at index 2

            // Assert
            Assert.AreEqual(11, sheet.RowCount); // Row count should increase
                                                 // The original cell should now be at row 6
            Assert.AreEqual("OriginalValue", sheet.GetCell(6, "C").Input);
            // The old location should now be part of a new, empty row
            Assert.AreEqual(string.Empty, sheet.GetCell(5, "C").Input);
        }

        [TestMethod]
        public void DeleteRow_FromMiddle_ShiftsLowerCellsUp()
        {
            // Arrange
            var sheet = new Spreadsheet(10, 10);
            sheet.SetCellInput(5, "C", "ValueToShiftUp"); // Cell at (5, C)
            sheet.SetCellInput(2, "A", "ValueToDelete");   // Cell in the row to be deleted

            // Act
            sheet.DeleteRow(2); // Delete row at index 2

            // Assert
            Assert.AreEqual(9, sheet.RowCount); // Row count should decrease
                                                // The cell that was at (5, C) should now be at (4, C)
            Assert.AreEqual("ValueToShiftUp", sheet.GetCell(4, "C").Input);
            Assert.AreEqual(string.Empty, sheet.GetCell(2, "A").Input);
        }

        // Аналогічні тести можна написати для InsertColumn та DeleteColumn

        #endregion

        #region RecalculateAllCells Tests

        [TestMethod]
        public void RecalculateAllCells_WithDependencyChain_CalculatesInCorrectOrder()
        {
            // Цей тест перевіряє, що топологічне сортування працює.
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            // Залежності: A1 -> B1 -> C1
            sheet.SetCellInput(0, "A", "=B1+1");
            sheet.SetCellInput(0, "B", "=C1+10");
            sheet.SetCellInput(0, "C", "10");

            // Act
            sheet.RecalculateAllCells();

            // Assert
            // Очікуємо: C1=10 -> B1=10+10=20 -> A1=20+1=21
            var resultCell = sheet.GetCell(0, "A");
            Assert.IsInstanceOfType(resultCell.CalculatedValue, typeof(BigInteger));
            Assert.AreEqual(new BigInteger(21), (BigInteger)resultCell.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAllCells_SimpleCycle_DetectsAndMarksAsRefError()
        {
            // Цей тест перевіряє, що виявлення циклів працює.
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            // Цикл: A1 -> B1 -> A1
            sheet.SetCellInput(0, "A", "=B1");
            sheet.SetCellInput(0, "B", "=A1");

            // Act
            sheet.RecalculateAllCells();

            // Assert
            var cellA1 = sheet.GetCell(0, "A");
            var cellB1 = sheet.GetCell(0, "B");
            Assert.AreEqual("#REF!", cellA1.CalculatedValue);
            Assert.AreEqual("#REF!", cellB1.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAllCells_InvalidDependencyName_MarksCellAsError()
        {
            // Перевіряє try-catch при побудові графу
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "=InvalidName+1");

            // Act
            sheet.RecalculateAllCells();

            // Assert
            var resultCell = sheet.GetCell(0, "A");
            // Ваш код встановлює #REF! при помилці парсингу залежностей
            Assert.AreEqual("#REF!", resultCell.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAllCells_SnapshotAndIterativeUpdate_CalculatesCorrectly()
        {
            // Цей тест перевіряє, що ваш підхід зі "знімком" та ітеративним оновленням context працює
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            sheet.GetCell(0, "A").Input = "10";
            sheet.SetCellInput(0, "B", "=A1+5");

            // Попереднє значення B1, наприклад, 0
            sheet.GetCell(0, "B").Evaluate(new Dictionary<string, object?>(), 5, 5); // CalculatedValue = 5

            // Змінюємо A1
            sheet.GetCell(0, "A").Input = "100";

            // Act
            sheet.RecalculateAllCells();

            // Assert
            // Очікуємо: B1 = 100 + 5 = 105
            var resultCell = sheet.GetCell(0, "B");
            Assert.IsInstanceOfType(resultCell.CalculatedValue, typeof(BigInteger));
            Assert.AreEqual(new BigInteger(105), (BigInteger)resultCell.CalculatedValue);
        }

        #endregion
    }
}
