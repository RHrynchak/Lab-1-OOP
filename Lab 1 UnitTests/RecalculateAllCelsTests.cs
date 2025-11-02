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
    public class SpreadsheetRecalculationTests
    {
        [TestMethod]
        public void RecalculateAll_WithDependencyChain()
        {
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "=B1+1");
            sheet.SetCellInput(0, "B", "=C1+10");
            sheet.SetCellInput(0, "C", "10");

            sheet.RecalculateAllCells();

            var resultCell = sheet.GetCell(0, "A");
            Assert.IsInstanceOfType(resultCell.CalculatedValue, typeof(BigInteger));
            Assert.AreEqual(new BigInteger(21), (BigInteger)resultCell.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAll_SimpleCycle()
        {
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "=B1");
            sheet.SetCellInput(0, "B", "=A1");

            sheet.RecalculateAllCells();

            var cellA1 = sheet.GetCell(0, "A");
            var cellB1 = sheet.GetCell(0, "B");
            Assert.AreEqual("#REF!", cellA1.CalculatedValue, "Клітинка A1 має бути позначена як циклічне посилання.");
            Assert.AreEqual("#REF!", cellB1.CalculatedValue, "Клітинка B1 має бути позначена як циклічне посилання.");
        }

        [TestMethod]
        public void RecalculateAll_InvalidReferenceName()
        {
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "=5+1A");

            sheet.RecalculateAllCells();

            var resultCell = sheet.GetCell(0, "A");
            Assert.AreEqual("#REF!", resultCell.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAll_ErrorPropagation()
        {
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "=1/0");
            sheet.SetCellInput(0, "B", "=Z99");
            sheet.SetCellInput(0, "C", "=A1+10");

            sheet.RecalculateAllCells();

            var errorCell = sheet.GetCell(0, "A");
            var dependentCell = sheet.GetCell(0, "B");

            Assert.AreEqual("#ERROR!", errorCell.CalculatedValue);

            Assert.AreEqual("#REF!", dependentCell.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAll_SnapshotPrinciple()
        {
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "1");
            sheet.SetCellInput(0, "B", "=A1");
            sheet.SetCellInput(0, "C", "=A1+B1");

            sheet.RecalculateAllCells();

            var resultCell = sheet.GetCell(0, "C");
            Assert.AreEqual(new BigInteger(2), (BigInteger)resultCell.CalculatedValue);
        }
    }
}
