using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Lab_1.Models.Enums;

namespace Lab_1.Models
{
    public class SpreadsheetFileData
    {
        public int RowCount { get; set; }
        public int ColumnCount { get; set; }
        public List<CellData> Cells { get; set; } = new();
        public Dictionary<int, RowMetadata> RowMetadata { get; set; } = new();
        public Dictionary<int, ColumnMetadata> ColumnMetadata { get; set; } = new();
    }

    public class CellData
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Input { get; set; } = string.Empty;
        public CellFormatData? Format { get; set; }
    }

    public class CellFormatData
    {
        public string FontName { get; set; } = "Calibri";
        public int FontSize { get; set; } = 11;
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public string TextColor { get; set; } = "#000000";
        public string BackgroundColor { get; set; } = "#FFFFFF";
        public string HorizontalAlignment { get; set; } = "Left";
        public string VerticalAlignment { get; set; } = "Center";
        public string BorderStyle { get; set; } = "Solid";
    }

    public class RowMetadata
    {
        public double Height { get; set; } = 20.0;
        public bool IsHidden { get; set; } = false;
    }

    public class ColumnMetadata
    {
        public double Width { get; set; } = 80.0;
        public bool IsHidden { get; set; } = false;
    }

    public static class SpreadsheetSerializer
    {
        public static void SaveToFile(Spreadsheet spreadsheet, string filePath)
        {
            var data = new SpreadsheetFileData
            {
                RowCount = spreadsheet.RowCount,
                ColumnCount = spreadsheet.ColumnCount
            };

            for (int row = 0; row < spreadsheet.RowCount; row++)
            {
                for (int col = 0; col < spreadsheet.ColumnCount; col++)
                {
                    string colName = SpreadsheetUtils.ToColumnName(col);
                    var cell = spreadsheet.GetReadOnlyCell(row, colName);

                    if (!string.IsNullOrEmpty(cell.Input))
                    {
                        var cellData = new CellData
                        {
                            Row = row,
                            Column = col,
                            Input = cell.Input,
                            Format = ConvertFormat(cell.Format)
                        };
                        data.Cells.Add(cellData);
                    }
                }
            }

            for (int row = 0; row < spreadsheet.RowCount; row++)
            {
                double height = spreadsheet.GetRowHeight(row);
                bool isHidden = spreadsheet.IsRowHidden(row);
                if (height != 20.0 || isHidden)
                {
                    data.RowMetadata[row] = new RowMetadata { Height = height, IsHidden = isHidden };
                }
            }

            for (int col = 0; col < spreadsheet.ColumnCount; col++)
            {
                string colName = SpreadsheetUtils.ToColumnName(col);
                double width = spreadsheet.GetColumnWidth(colName);
                bool isHidden = spreadsheet.IsColumnHidden(colName);
                if (width != 80.0 || isHidden)
                {
                    data.ColumnMetadata[col] = new ColumnMetadata { Width = width, IsHidden = isHidden };
                }
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(data, options);
            File.WriteAllText(filePath, json);
        }

        public static Spreadsheet LoadFromFile(string filePath)
        {
            string json = File.ReadAllText(filePath);
            var data = JsonSerializer.Deserialize<SpreadsheetFileData>(json);

            if (data == null)
                throw new Exception("Не вдалося завантажити файл");

            var spreadsheet = new Spreadsheet(data.RowCount, data.ColumnCount);

            foreach (var cellData in data.Cells)
            {
                string colName = SpreadsheetUtils.ToColumnName(cellData.Column);
                spreadsheet.SetCellInput(cellData.Row, colName, cellData.Input);

                if (cellData.Format != null)
                {
                    spreadsheet.SetCellFormat(cellData.Row, colName, ConvertFormatBack(cellData.Format));
                }
            }

            foreach (var kvp in data.RowMetadata)
            {
                spreadsheet.SetRowHeight(kvp.Key, kvp.Value.Height);
                spreadsheet.SetRowHidden(kvp.Key, kvp.Value.IsHidden);
            }

            foreach (var kvp in data.ColumnMetadata)
            {
                string colName = SpreadsheetUtils.ToColumnName(kvp.Key);
                spreadsheet.SetColumnWidth(colName, kvp.Value.Width);
                spreadsheet.SetColumnHidden(colName, kvp.Value.IsHidden);
            }

            return spreadsheet;
        }

        private static CellFormatData ConvertFormat(CellFormat format)
        {
            return new CellFormatData
            {
                FontName = format.FontName,
                FontSize = format.FontSize,
                IsBold = format.IsBold,
                IsItalic = format.IsItalic,
                IsUnderline = format.IsUnderline,
                TextColor = format.TextColor,
                BackgroundColor = format.BackgroundColor,
                HorizontalAlignment = format.HorizontalAlignment.ToString(),
                VerticalAlignment = format.VerticalAlignment.ToString(),
                BorderStyle = format.BorderStyle.ToString()
            };
        }

        private static CellFormat ConvertFormatBack(CellFormatData data)
        {
            return new CellFormat
            {
                FontName = data.FontName,
                FontSize = data.FontSize,
                IsBold = data.IsBold,
                IsItalic = data.IsItalic,
                IsUnderline = data.IsUnderline,
                TextColor = data.TextColor,
                BackgroundColor = data.BackgroundColor,
                HorizontalAlignment = Enum.Parse<HorizontalAlignment>(data.HorizontalAlignment),
                VerticalAlignment = Enum.Parse<VerticalAlignment>(data.VerticalAlignment),
                BorderStyle = Enum.Parse<BorderStyle>(data.BorderStyle)
            };
        }
    }
}
