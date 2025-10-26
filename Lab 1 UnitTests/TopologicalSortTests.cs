using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab_1.Models;

namespace Lab_1_UnitTests
{
    [TestClass]
    public class TopologicalSortTests
    {
        private Spreadsheet _spreadsheet = new Spreadsheet(); // Потрібен для виклику internal методу

        [TestMethod]
        public void TopologicalSort_LinearDependency_ReturnsCorrectOrder()
        {
            // Arrange: Створюємо граф A -> B -> C
            // Очікуваний порядок: C, B, A
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (0, 1) } }, // A(0,0) -> B(0,1)
            { (0, 1), new List<(int, int)> { (0, 2) } }, // B(0,1) -> C(0,2)
            { (0, 2), new List<(int, int)>() }          // C(0,2) -> (нічого)
        };
            var a = (0, 0);
            var b = (0, 1);
            var c = (0, 2);

            // Act
            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            // Assert
            Assert.IsTrue(cyclic.Count == 0, "Має бути 0 циклічних клітинок.");
            Assert.AreEqual(3, sorted.Count, "Має бути 3 відсортовані клітинки.");
            // Перевіряємо, що залежності йдуть перед тими, хто від них залежить
            Assert.IsTrue(sorted.IndexOf(c) < sorted.IndexOf(b));
            Assert.IsTrue(sorted.IndexOf(b) < sorted.IndexOf(a));
        }

        [TestMethod]
        public void TopologicalSort_DiamondDependency_ReturnsCorrectOrder()
        {
            // Arrange: Граф "діамант": A -> B, A -> C, B -> D, C -> D
            // Очікуваний порядок: D, потім B і C (у будь-якому порядку), потім A
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (1, 0), (1, 1) } }, // A -> B, C
            { (1, 0), new List<(int, int)> { (2, 0) } },          // B -> D
            { (1, 1), new List<(int, int)> { (2, 0) } },          // C -> D
            { (2, 0), new List<(int, int)>() }                   // D -> (нічого)
        };
            var a = (0, 0);
            var b = (1, 0);
            var c = (1, 1);
            var d = (2, 0);

            // Act
            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            // Assert
            Assert.IsTrue(cyclic.Count == 0);
            Assert.AreEqual(4, sorted.Count);
            // D має бути перед B і C
            Assert.IsTrue(sorted.IndexOf(d) < sorted.IndexOf(b));
            Assert.IsTrue(sorted.IndexOf(d) < sorted.IndexOf(c));
            // B і C мають бути перед A
            Assert.IsTrue(sorted.IndexOf(b) < sorted.IndexOf(a));
            Assert.IsTrue(sorted.IndexOf(c) < sorted.IndexOf(a));
        }

        [TestMethod]
        public void TopologicalSort_SimpleCycle_DetectsCycle()
        {
            // Arrange: Створюємо простий цикл A -> B -> A
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (0, 1) } }, // A -> B
            { (0, 1), new List<(int, int)> { (0, 0) } }  // B -> A
        };
            var a = (0, 0);
            var b = (0, 1);

            // Act
            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            // Assert
            Assert.AreEqual(2, cyclic.Count, "Має бути 2 циклічні клітинки.");
            // AreEquivalent перевіряє, що колекції містять однакові елементи, ігноруючи порядок
            CollectionAssert.AreEquivalent(new List<(int, int)> { a, b }, cyclic);
        }

        [TestMethod]
        public void TopologicalSort_GraphWithCycleAndValidNodes_SortsValidAndDetectsCycle()
        {
            // Arrange: D -> (A -> B -> A)
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (0, 1) } }, // A -> B
            { (0, 1), new List<(int, int)> { (0, 0) } }, // B -> A
            { (0, 3), new List<(int, int)> { (0, 0) } }  // D -> A
        };
            var a = (0, 0);
            var b = (0, 1);
            var d = (0, 3);

            // Act
            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            // Assert
            // Жодна клітинка не може бути відсортована, бо D залежить від циклу
            Assert.AreEqual(0, sorted.Count);
            Assert.AreEqual(3, cyclic.Count);
            CollectionAssert.AreEquivalent(new List<(int, int)> { a, b, d }, cyclic);
        }
    }
}
