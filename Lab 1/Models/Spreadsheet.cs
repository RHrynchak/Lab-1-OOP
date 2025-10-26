using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Lab_1.Models.Enums;

[assembly: InternalsVisibleTo("Lab 1 UnitTests")]

namespace Lab_1.Models
{
    public class Spreadsheet
    {
        public int RowCount { get; private set; }
        public int ColumnCount { get; private set; }
        private Dictionary<(int row, int column), Cell> _cells = new();
        private Dictionary<int, RowMetaData> _rowMetaData = new();
        private Dictionary<int, ColumnMetaData> _columnMetaData = new();
        private const double DefaultRowHeight = 20.0;
        private const double DefaultColumnWidth = 80.0;
        private static readonly Cell _emptyReadOnlyCell = new Cell();
        public Spreadsheet(int rowCount = 50, int columnCount = 26)
        {
            RowCount = rowCount > 0 ? rowCount : 1;
            ColumnCount = columnCount > 0 ? columnCount : 1;
        }
        public Cell GetCell(int row, string column)
        {
            int colIndex = SpreadsheetUtils.ToColumnIndex(column);
            if ( row < 0 || row >= RowCount || colIndex < 0 || colIndex >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException($"Coordinates ({row}, {colIndex}) are out of range.");
            }
            if (!_cells.ContainsKey((row, colIndex)))
            {
                _cells[(row, colIndex)] = new Cell();
            }
            return _cells[(row, colIndex)];
        }
        public Cell GetReadOnlyCell(int row, string column)
        {
            int colIndex = SpreadsheetUtils.ToColumnIndex(column);
            if (row < 0 || row >= RowCount || colIndex < 0 || colIndex >= ColumnCount)
            {
                return _emptyReadOnlyCell;
            }
            if (!_cells.ContainsKey((row, colIndex)))
            {
                return _emptyReadOnlyCell;
            }
            return _cells[(row, colIndex)];
        }
        public void SetCellInput(int row, string column, string input)
        {
            GetCell(row, column).Input = input;
        }
        public void SetCellFormat(int row, string column, CellFormat format)
        {
            GetCell(row, column).Format = format;
        }
        public double GetRowHeight(int row)
        {
            if (_rowMetaData.ContainsKey(row))
            {
                return _rowMetaData[row].Height;
            }
            return DefaultRowHeight;
        }
        public void SetRowHeight(int row, double height)
        {
            if (!_rowMetaData.ContainsKey(row))
            {
                _rowMetaData[row] = new RowMetaData();
            }
            _rowMetaData[row].Height = height;
        }
        public bool IsRowHidden(int row)
        {
            if (_rowMetaData.ContainsKey(row))
            {
                return _rowMetaData[row].IsHidden;
            }
            return false;
        }
        public void SetRowHidden(int row, bool isHidden)
        {
            if (!_rowMetaData.ContainsKey(row))
            {
                _rowMetaData[row] = new RowMetaData();
            }
            _rowMetaData[row].IsHidden = isHidden;
        }
        public double GetColumnWidth(string column)
        {
            int colIndex = SpreadsheetUtils.ToColumnIndex(column);
            if (_columnMetaData.ContainsKey(colIndex))
            {
                return _columnMetaData[colIndex].Width;
            }
            return DefaultColumnWidth;
        }
        public void SetColumnWidth(string column, double width)
        {
            int colIndex = SpreadsheetUtils.ToColumnIndex(column);
            if (!_columnMetaData.ContainsKey(colIndex))
            {
                _columnMetaData[colIndex] = new ColumnMetaData();
            }
            _columnMetaData[colIndex].Width = width;
        }
        public bool IsColumnHidden(string column)
            {
                int colIndex = SpreadsheetUtils.ToColumnIndex(column);
                if (_columnMetaData.ContainsKey(colIndex))
                {
                    return _columnMetaData[colIndex].IsHidden;
                }
                return false;
        }
        public void SetColumnHidden(string column, bool isHidden)
        {
            int colIndex = SpreadsheetUtils.ToColumnIndex(column);
            if (!_columnMetaData.ContainsKey(colIndex))
            {
                _columnMetaData[colIndex] = new ColumnMetaData();
            }
            _columnMetaData[colIndex].IsHidden = isHidden;
        }

        private string updateCellReference( string cellReference, int shiftIndex, bool isRowShift, int shiftAmount )
        {
            var (refRow, refCol) = SpreadsheetUtils.NameToCoordinates(cellReference);
            if (isRowShift)
            {
                if (refRow >= shiftIndex)
                {
                    int newRow = refRow + shiftAmount;
                    if (newRow < 0) return "#REF!";
                    return SpreadsheetUtils.ToColumnName(refCol) + (newRow + 1).ToString();
                }
            }
            else
            {
                if (refCol >= shiftIndex)
                {
                    int newCol = refCol + shiftAmount;
                    if (newCol < 0) return "#REF!";
                    return SpreadsheetUtils.ToColumnName(newCol) + (refRow + 1).ToString();
                }
            }
            return cellReference;
        }
        private string RebuildFormulaFromTokens(List<Token> tokens, int shiftIndex, bool isRowShift, int shiftAmount )
        {
            var newFormulaParts = new List<string>();
            foreach (var token in tokens)
            {
                if (token.Type == TokenType.CellReference)
                {
                    string updatedRef = updateCellReference(token.Value, shiftIndex, isRowShift, shiftAmount);
                    newFormulaParts.Add(updatedRef);
                }
                else
                {
                    newFormulaParts.Add(token.Value);
                }
            }
            return "=" + string.Join(" ", newFormulaParts);
        }
        public void InsertRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex > RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), "Row index is out of range.");
            }

            foreach ( var cell in _cells.Values )
            {
                if (cell.HasFormula)
                {
                    try
                    {
                        var tokens = new Lexer(cell.Input.Substring(1)).Tokenise();
                        cell.Input = RebuildFormulaFromTokens(tokens, rowIndex, true, 1);
                    }
                    catch (Exception)
                    {
                        cell.SetErrorValue("#REF!");
                    }
                }
            }

            var newCells = new Dictionary<(int row, int column), Cell>();
            foreach ( var item in _cells )
            {
                if ( item.Key.row >= rowIndex )
                {
                    newCells[(item.Key.row + 1, item.Key.column)] = item.Value;
                }
                else
                {
                    newCells[item.Key] = item.Value;
                }
            }
            _cells = newCells;

            var newRowMetaData = new Dictionary<int, RowMetaData>();
            foreach ( var item in _rowMetaData )
            {
                if ( item.Key >= rowIndex )
                {
                    newRowMetaData[item.Key + 1] = item.Value;
                }
                else
                {
                    newRowMetaData[item.Key] = item.Value;
                }
            }
            _rowMetaData = newRowMetaData;

            RowCount++;
        }
        public void InsertColumn(string columnName)
        {
            int colIndex = SpreadsheetUtils.ToColumnIndex(columnName);
            if (colIndex < 0 || colIndex > ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), "Column index is out of range.");
            }
            
            foreach (var cell in _cells.Values)
            {
                if (cell.HasFormula)
                {
                    try
                    {
                        var tokens = new Lexer(cell.Input.Substring(1)).Tokenise();
                        cell.Input = RebuildFormulaFromTokens(tokens, colIndex, false, 1);
                    }
                    catch (Exception)
                    {
                        cell.SetErrorValue("#REF!");
                    }
                }
            }

            var newCells = new Dictionary<(int row, int column), Cell>();
            foreach (var item in _cells)
            {
                if (item.Key.column >= colIndex)
                {
                    newCells[(item.Key.row, item.Key.column + 1)] = item.Value;
                }
                else
                {
                    newCells[item.Key] = item.Value;
                }
            }
            _cells = newCells;

            var newColumnMetaData = new Dictionary<int, ColumnMetaData>();
            foreach (var item in _columnMetaData)
            {
                if (item.Key >= colIndex)
                {
                    newColumnMetaData[item.Key + 1] = item.Value;
                }
                else
                {
                    newColumnMetaData[item.Key] = item.Value;
                }
            }
            _columnMetaData = newColumnMetaData;

            ColumnCount++;
        }
        public void DeleteRow(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= RowCount)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), "Row index is out of range.");
            }
            
            foreach (var cell in _cells.Values)
            {
                if (cell.HasFormula)
                {
                    try
                    {
                        var tokens = new Lexer(cell.Input.Substring(1)).Tokenise();
                        cell.Input = RebuildFormulaFromTokens(tokens, rowIndex, true, -1);
                    }
                    catch (Exception)
                    {
                        cell.SetErrorValue("#REF!");
                    }
                }
            }

            var newCells = new Dictionary<(int row, int column), Cell>();
            foreach (var item in _cells)
            {
                if (item.Key.row < rowIndex)
                {
                    newCells[item.Key] = item.Value;
                }
                else if (item.Key.row > rowIndex)
                {
                    newCells[(item.Key.row - 1, item.Key.column)] = item.Value;
                }
            }
            _cells = newCells;

            var newRowMetaData = new Dictionary<int, RowMetaData>();
            foreach (var item in _rowMetaData)
            {
                if (item.Key < rowIndex)
                {
                    newRowMetaData[item.Key] = item.Value;
                }
                else if (item.Key > rowIndex)
                {
                    newRowMetaData[item.Key - 1] = item.Value;
                }
            }
            _rowMetaData = newRowMetaData;
            RowCount--;
        }
        public void DeleteColumn(string columnName)
        {
            int colIndex = SpreadsheetUtils.ToColumnIndex(columnName);
            if (colIndex < 0 || colIndex >= ColumnCount)
            {
                throw new ArgumentOutOfRangeException(nameof(columnName), "Column index is out of range.");
            }
            
            foreach (var cell in _cells.Values)
            {
                if (cell.HasFormula)
                {
                    try
                    {
                        var tokens = new Lexer(cell.Input.Substring(1)).Tokenise();
                        cell.Input = RebuildFormulaFromTokens(tokens, colIndex, false, -1);
                    }
                    catch (Exception)
                    {
                        cell.SetErrorValue("#REF!");
                    }
                }
            }

            var newCells = new Dictionary<(int row, int column), Cell>();
            foreach (var item in _cells)
            {
                if (item.Key.column < colIndex)
                {
                    newCells[item.Key] = item.Value;
                }
                else if (item.Key.column > colIndex)
                {
                    newCells[(item.Key.row, item.Key.column - 1)] = item.Value;
                }
            }
            _cells = newCells;

            var newColumnMetaData = new Dictionary<int, ColumnMetaData>();
            foreach (var item in _columnMetaData)
            {
                if (item.Key < colIndex)
                {
                    newColumnMetaData[item.Key] = item.Value;
                }
                else if (item.Key > colIndex)
                {
                    newColumnMetaData[item.Key - 1] = item.Value;
                }
            }
            _columnMetaData = newColumnMetaData;

            ColumnCount--;
        }

        internal (List<(int row, int column)> sorted, List<(int row, int column)> cyclic) TopologicalSort(Dictionary<(int, int), List<(int, int)>> graph)
        {
            var sorted = new List<(int row, int column)>();
            var dependencyCount = new Dictionary<(int, int), int>();
            var reversedGraph = new Dictionary<(int, int), List<(int, int)>>();
            foreach (var node in graph.Keys)
            {
                dependencyCount[node] = 0;
                reversedGraph[node] = new List<(int, int)>();
            }

            foreach (var node in graph.Keys)
            {
                foreach (var dependency in graph[node])
                {
                    if (graph.ContainsKey(dependency))
                    {
                        dependencyCount[node]++;
                        reversedGraph[dependency].Add(node);
                    }
                }
            }

            var noDependencyNodes = new Queue<(int row, int column)>();
            foreach (var node in dependencyCount)
            {
                if (node.Value == 0)
                {
                    noDependencyNodes.Enqueue(node.Key);
                }
            }

            while (noDependencyNodes.Count > 0)
            {
                var node = noDependencyNodes.Dequeue();
                sorted.Add(node);
                foreach (var neighbour in reversedGraph[node])
                {
                    dependencyCount[neighbour]--;
                    if (dependencyCount[neighbour] == 0)
                    {
                        noDependencyNodes.Enqueue(neighbour);
                    }
                }
            }

            var cyclic = new List<(int row, int column)>();
            foreach (var node in dependencyCount)
            {
                if (node.Value > 0)
                {
                    cyclic.Add(node.Key);
                }
            }
            return (sorted, cyclic);
        }

        public void RecalculateAllCells()
        {
            var graph = new Dictionary<(int, int), List<(int, int)>>();
            var cellsWithFormulas = new List<(int, int)>();
            foreach (var item in _cells)
            {
                var cellCoords = item.Key;
                var cell = item.Value;
                if (cell.HasFormula)
                {
                    cellsWithFormulas.Add(cellCoords);
                    graph[cellCoords] = new List<(int, int)>();
                    try
                    {
                        var dependencies = SpreadsheetUtils.GetDependencies(cell.Input.Substring(1));
                        foreach (var dependency in dependencies)
                        {
                            var (depRow, depCol) = SpreadsheetUtils.NameToCoordinates(dependency);
                            graph[cellCoords].Add((depRow, depCol));
                        }
                    }
                    catch (Exception)
                    {
                        cell.SetErrorValue("#REF!");
                    }
                }
                else
                {
                    cell.Evaluate(new Dictionary<string, object?>(), 0, 0);
                }
            }

            var (calculationOrder, cyclicCells) = TopologicalSort(graph);

            foreach (var coord in cyclicCells)
            {
                if (_cells.ContainsKey(coord))
                {
                    GetCell(coord.row, SpreadsheetUtils.ToColumnName(coord.column)).SetErrorValue("#REF!");
                }
            }

            var context = new Dictionary<string, object?>();
            foreach (var item in _cells)
            {
                string cellName = SpreadsheetUtils.ToColumnName(item.Key.column) + (item.Key.row + 1).ToString();
                context[cellName] = item.Value.CalculatedValue;
            }

            foreach (var coord in calculationOrder)
            {
                var cell = GetCell(coord.row, SpreadsheetUtils.ToColumnName(coord.column));
                cell.Evaluate(context, RowCount, ColumnCount);
                string cellName = SpreadsheetUtils.ToColumnName(coord.column) + (coord.row + 1).ToString();
                context[cellName] = cell.CalculatedValue;
            }
        }
    }
}
