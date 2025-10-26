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
        public void RecalculateAll_WithDependencyChain_CalculatesInCorrectOrder()
        {
            // ✅ ЦЕЙ ТЕСТ ТЕПЕР МАЄ ПРОЙТИ!
            // Він перевіряє, що топологічне сортування працює.
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            // Залежності: A1 -> B1 -> C1
            sheet.SetCellInput(0, "A", "=B1+1");   // A1
            sheet.SetCellInput(0, "B", "=C1+10");   // B1
            sheet.SetCellInput(0, "C", "10");     // C1

            // Act
            sheet.RecalculateAllCells();

            // Assert
            // Очікуємо: C1=10 -> B1=10*2=20 -> A1=20+1=21
            var resultCell = sheet.GetCell(0, "A"); // A1
            Assert.IsInstanceOfType(resultCell.CalculatedValue, typeof(BigInteger));
            Assert.AreEqual(new BigInteger(21), (BigInteger)resultCell.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAll_SimpleCycle_DetectsAndMarksAsRefError()
        {
            // ✅ ЦЕЙ ТЕСТ ТЕЖ МАЄ ПРОЙТИ!
            // Він перевіряє, що ваш алгоритм правильно знаходить цикли.
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            // Цикл: A1 -> B1 -> A1
            sheet.SetCellInput(0, "A", "=B1"); // A1
            sheet.SetCellInput(0, "B", "=A1"); // B1

            // Act
            sheet.RecalculateAllCells();

            // Assert
            var cellA1 = sheet.GetCell(0, "A");
            var cellB1 = sheet.GetCell(0, "B");
            Assert.AreEqual("#REF!", cellA1.CalculatedValue, "Клітинка A1 має бути позначена як циклічне посилання.");
            Assert.AreEqual("#REF!", cellB1.CalculatedValue, "Клітинка B1 має бути позначена як циклічне посилання.");
        }

        [TestMethod]
        public void RecalculateAll_InvalidReferenceName_MarksCellAsRefErrorDuringGraphBuild()
        {
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "=5+1A"); // Неправильний формат посилання

            // Act
            sheet.RecalculateAllCells();

            // Assert
            var resultCell = sheet.GetCell(0, "A");
            // Ваша логіка try-catch при побудові графу має зловити цю помилку
            Assert.AreEqual("#REF!", resultCell.CalculatedValue);
        }

        [TestMethod]
        public void RecalculateAll_ErrorPropagation_FailsToPropagate()
        {
            // ❗️ ЦЕЙ ТЕСТ, СКОРІШ ЗА ВСЕ, ПРОВАЛИТЬСЯ.
            // Він демонструє, що ваш context не поширює помилки далі.
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "=1/0"); // Припустимо, це дасть #ERROR! (потрібно додати оператор ділення)
                                              // Або використаємо посилання за межі, яке ви вже обробляєте:
            sheet.SetCellInput(0, "B", "=Z99");   // Це буде #REF!
            sheet.SetCellInput(0, "C", "=A1+10"); // B1 залежить від помилкової A1

            // Act
            sheet.RecalculateAllCells();

            // Assert
            var errorCell = sheet.GetCell(0, "A");
            var dependentCell = sheet.GetCell(0, "B");

            Assert.AreEqual("#ERROR!", errorCell.CalculatedValue);

            // Перевіряємо, що помилка поширилась.
            // Ваш код, скоріш за все, обчислить це як 0 + 10 = 10, бо context не містить помилок.
            Assert.AreEqual("#REF!", dependentCell.CalculatedValue,
                "Тест провалився, бо помилки не поширюються. Context має містити і помилки теж, а CellReferenceNode має їх обробляти.");
        }

        [TestMethod]
        public void RecalculateAll_SnapshotPrinciple_FailsWithMutatingContext()
        {
            // ❗️ ЦЕЙ ТЕСТ ТАКОЖ ПРОВАЛИТЬСЯ.
            // Він демонструє проблему зміни контексту під час обчислення.
            // Arrange
            var sheet = new Spreadsheet(5, 5);
            sheet.SetCellInput(0, "A", "1");      // A1 = 1
            sheet.SetCellInput(0, "B", "=A1");    // B1 = A1
            sheet.SetCellInput(0, "C", "=A1+B1"); // C1 = A1+B1

            // Уявімо, що ваш foreach обробляє в такому порядку: C1, B1, A1 (довільний)
            // Початковий context: A1=null, B1=null, C1=null

            // Act
            sheet.RecalculateAllCells();
            // Очікуваний результат: C1 = 1 + 1 = 2.
            // Ваш результат:
            // 1. Обчислюється C1. Context для A1 і B1 порожній. C1 = 0+0 = 0. Context оновлюється: C1=0.
            // 2. Обчислюється B1. Context для A1 порожній. B1 = 0. Context оновлюється: B1=0.
            // 3. Обчислюється A1. A1 = 1.
            // Фінальний результат для C1 залишиться 0, а не 2.

            // Assert
            var resultCell = sheet.GetCell(0, "C");
            Assert.AreEqual(new BigInteger(2), (BigInteger)resultCell.CalculatedValue,
                "Тест провалився, бо context змінюється під час циклу обчислень, порушуючи принцип 'знімка'.");
        }
    }
}
