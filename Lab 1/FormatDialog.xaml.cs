using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Lab_1.Models;

namespace Lab_1
{
    /// <summary>
    /// Interaction logic for FormatDialog.xaml
    /// </summary>
    public partial class FormatDialog : Window
    {
        public CellFormat Format { get; private set; }
        public double ColumnWidth { get; private set; }
        public double RowHeight { get; private set; }

        private Dictionary<string, Color> _colors = new Dictionary<string, Color>
        {
            { "Чорний", Colors.Black },
            { "Білий", Colors.White },
            { "Червоний", Colors.Red },
            { "Зелений", Colors.Green },
            { "Синій", Colors.Blue },
            { "Жовтий", Colors.Yellow },
            { "Помаранчевий", Colors.Orange },
            { "Фіолетовий", Colors.Purple },
            { "Сірий", Colors.Gray },
            { "Коричневий", Colors.Brown }
        };

        public FormatDialog(CellFormat currentFormat, double columnWidth, double rowHeight)
        {
            InitializeComponent();

            Format = new CellFormat
            {
                FontName = currentFormat.FontName,
                FontSize = currentFormat.FontSize,
                IsBold = currentFormat.IsBold,
                IsItalic = currentFormat.IsItalic,
                IsUnderline = currentFormat.IsUnderline,
                TextColor = currentFormat.TextColor,
                BackgroundColor = currentFormat.BackgroundColor,
                HorizontalAlignment = currentFormat.HorizontalAlignment,
                VerticalAlignment = currentFormat.VerticalAlignment,
                BorderStyle = currentFormat.BorderStyle
            };

            ColumnWidth = columnWidth;
            RowHeight = rowHeight;

            InitializeFontFamilies();
            InitializeColors();
            LoadCurrentFormat();
        }

        private void InitializeFontFamilies()
        {
            var fonts = Fonts.SystemFontFamilies.Select(f => f.Source).OrderBy(f => f).ToList();
            FontFamilyCombo.ItemsSource = fonts;
            FontFamilyCombo.SelectedItem = Format.FontName;
        }

        private void InitializeColors()
        {
            foreach (var color in _colors)
            {
                var textItem = new ComboBoxItem
                {
                    Content = color.Key,
                    Background = new SolidColorBrush(color.Value),
                    Foreground = GetContrastColor(color.Value)
                };
                TextColorCombo.Items.Add(textItem);

                var bgItem = new ComboBoxItem
                {
                    Content = color.Key,
                    Background = new SolidColorBrush(color.Value),
                    Foreground = GetContrastColor(color.Value)
                };
                BackgroundColorCombo.Items.Add(bgItem);
            }
        }

        private Brush GetContrastColor(Color color)
        {
            double brightness = (color.R * 299 + color.G * 587 + color.B * 114) / 1000.0;
            return brightness > 128 ? Brushes.Black : Brushes.White;
        }

        private void LoadCurrentFormat()
        {
            FontSizeTextBox.Text = Format.FontSize.ToString();
            BoldCheckBox.IsChecked = Format.IsBold;
            ItalicCheckBox.IsChecked = Format.IsItalic;
            UnderlineCheckBox.IsChecked = Format.IsUnderline;

            SelectColorByHex(TextColorCombo, Format.TextColor);
            SelectColorByHex(BackgroundColorCombo, Format.BackgroundColor);

            SelectComboBoxItem(HorizontalAlignCombo, Format.HorizontalAlignment.ToString());
            SelectComboBoxItem(VerticalAlignCombo, Format.VerticalAlignment.ToString());

            ColumnWidthTextBox.Text = ColumnWidth.ToString();
            RowHeightTextBox.Text = RowHeight.ToString();
        }

        private void SelectColorByHex(ComboBox combo, string hexColor)
        {
            Color targetColor = (Color)ColorConverter.ConvertFromString(hexColor);

            for (int i = 0; i < combo.Items.Count; i++)
            {
                var item = combo.Items[i] as ComboBoxItem;
                if (item != null && item.Background is SolidColorBrush brush)
                {
                    if (brush.Color == targetColor)
                    {
                        combo.SelectedIndex = i;
                        return;
                    }
                }
            }
            combo.SelectedIndex = 0;
        }

        private void SelectComboBoxItem(ComboBox combo, string tag)
        {
            foreach (ComboBoxItem item in combo.Items)
            {
                if (item.Tag?.ToString() == tag)
                {
                    combo.SelectedItem = item;
                    return;
                }
            }
            combo.SelectedIndex = 0;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Format.FontName = FontFamilyCombo.SelectedItem?.ToString() ?? "Calibri";
                Format.FontSize = int.Parse(FontSizeTextBox.Text);
                Format.IsBold = BoldCheckBox.IsChecked ?? false;
                Format.IsItalic = ItalicCheckBox.IsChecked ?? false;
                Format.IsUnderline = UnderlineCheckBox.IsChecked ?? false;

                if (TextColorCombo.SelectedItem is ComboBoxItem textColorItem &&
                    textColorItem.Background is SolidColorBrush textBrush)
                {
                    Format.TextColor = textBrush.Color.ToString();
                }

                if (BackgroundColorCombo.SelectedItem is ComboBoxItem bgColorItem &&
                    bgColorItem.Background is SolidColorBrush bgBrush)
                {
                    Format.BackgroundColor = bgBrush.Color.ToString();
                }

                if (HorizontalAlignCombo.SelectedItem is ComboBoxItem hAlignItem)
                {
                    Format.HorizontalAlignment = (Models.Enums.HorizontalAlignment)Enum.Parse<HorizontalAlignment>(hAlignItem.Tag.ToString());
                }

                if (VerticalAlignCombo.SelectedItem is ComboBoxItem vAlignItem)
                {
                    Format.VerticalAlignment = (Models.Enums.VerticalAlignment)Enum.Parse<VerticalAlignment>(vAlignItem.Tag.ToString());
                }

                ColumnWidth = double.Parse(ColumnWidthTextBox.Text);
                RowHeight = double.Parse(RowHeightTextBox.Text);

                if (ColumnWidth < 10) ColumnWidth = 10;
                if (RowHeight < 10) RowHeight = 10;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка: {ex.Message}", "Помилка",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
