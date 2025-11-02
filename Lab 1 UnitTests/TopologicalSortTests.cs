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
        private Spreadsheet _spreadsheet = new Spreadsheet();

        [TestMethod]
        public void TopologicalSort_LinearDependency()
        {
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (0, 1) } },
            { (0, 1), new List<(int, int)> { (0, 2) } },
            { (0, 2), new List<(int, int)>() } 
        };
            var a = (0, 0);
            var b = (0, 1);
            var c = (0, 2);

            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            Assert.IsTrue(cyclic.Count == 0, "Має бути 0 циклічних клітинок.");
            Assert.AreEqual(3, sorted.Count, "Має бути 3 відсортовані клітинки.");
            Assert.IsTrue(sorted.IndexOf(c) < sorted.IndexOf(b));
            Assert.IsTrue(sorted.IndexOf(b) < sorted.IndexOf(a));
        }

        [TestMethod]
        public void TopologicalSort_DiamondDependency()
        {
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (1, 0), (1, 1) } },
            { (1, 0), new List<(int, int)> { (2, 0) } },
            { (1, 1), new List<(int, int)> { (2, 0) } },
            { (2, 0), new List<(int, int)>() }  
        };
            var a = (0, 0);
            var b = (1, 0);
            var c = (1, 1);
            var d = (2, 0);

            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            Assert.IsTrue(cyclic.Count == 0);
            Assert.AreEqual(4, sorted.Count);
            Assert.IsTrue(sorted.IndexOf(d) < sorted.IndexOf(b));
            Assert.IsTrue(sorted.IndexOf(d) < sorted.IndexOf(c));
            Assert.IsTrue(sorted.IndexOf(b) < sorted.IndexOf(a));
            Assert.IsTrue(sorted.IndexOf(c) < sorted.IndexOf(a));
        }

        [TestMethod]
        public void TopologicalSort_SimpleCycle()
        {
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (0, 1) } },
            { (0, 1), new List<(int, int)> { (0, 0) } } 
        };
            var a = (0, 0);
            var b = (0, 1);

            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            Assert.AreEqual(2, cyclic.Count, "Має бути 2 циклічні клітинки.");
            CollectionAssert.AreEquivalent(new List<(int, int)> { a, b }, cyclic);
        }

        [TestMethod]
        public void TopologicalSort_GraphWithCycleAndValidNodes()
        {
            var graph = new Dictionary<(int, int), List<(int, int)>>
        {
            { (0, 0), new List<(int, int)> { (0, 1) } },
            { (0, 1), new List<(int, int)> { (0, 0) } },
            { (0, 3), new List<(int, int)> { (0, 0) } }
        };
            var a = (0, 0);
            var b = (0, 1);
            var d = (0, 3);

            var (sorted, cyclic) = _spreadsheet.TopologicalSort(graph);

            Assert.AreEqual(0, sorted.Count);
            Assert.AreEqual(3, cyclic.Count);
            CollectionAssert.AreEquivalent(new List<(int, int)> { a, b, d }, cyclic);
        }
    }
}
