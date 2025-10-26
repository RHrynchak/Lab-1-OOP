using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Lab_1.Models.Enums;

namespace Lab_1.Models
{
    public class CellFormat
    {
        public string FontName { get; set; }
        public int FontSize { get; set; }
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public string TextColor { get; set; }
        public string BackgroundColor { get; set; }
        public HorizontalAlignment HorizontalAlignment { get; set; }
        public VerticalAlignment VerticalAlignment { get; set; }
        public BorderStyle BorderStyle { get; set; }
        public CellFormat()
        {
            FontName = "Calibri";
            FontSize = 11;
            IsBold = false;
            IsItalic = false;
            IsUnderline = false;
            TextColor = "#000000"; // Black
            BackgroundColor = "#FFFFFF"; // White
            HorizontalAlignment = HorizontalAlignment.Left;
            VerticalAlignment = VerticalAlignment.Center;
            BorderStyle = BorderStyle.Solid;
        }
    }
}
