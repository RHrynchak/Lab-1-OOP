using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Lab_1.Models;
using Microsoft.Win32;

namespace Lab_1.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Spreadsheet _spreadsheet;
        private bool _showValues = true;
        private bool _showFormulas = false;
        private string _formulaBarText = string.Empty;
        private string _selectedCellName = "A1";
        private string _statusText = "Готово";
        private CellViewModel? _selectedCell;
        string _currentFilePath = string.Empty;

        public MainViewModel()
        {
            _spreadsheet = new Spreadsheet(50, 26);

            InitializeCommands();
            InitializeGrid();
            RecalculateAll();
        }

        #region Properties

        public ObservableCollection<RowViewModel> Rows { get; } = new();
        public ObservableCollection<ColumnHeaderViewModel> ColumnHeaders { get; } = new();
        public ObservableCollection<RowHeaderViewModel> RowHeaders { get; } = new();

        public bool ShowValues
        {
            get => _showValues;
            set
            {
                if (_showValues != value)
                {
                    _showValues = value;
                    if (value) ShowFormulas = false;
                    OnPropertyChanged();
                    UpdateAllCellsDisplay();
                }
            }
        }

        public bool ShowFormulas
        {
            get => _showFormulas;
            set
            {
                if (_showFormulas != value)
                {
                    _showFormulas = value;
                    if (value) ShowValues = false;
                    OnPropertyChanged();
                    UpdateAllCellsDisplay();
                }
            }
        }

        public string FormulaBarText
        {
            get => _formulaBarText;
            set
            {
                if (_formulaBarText != value)
                {
                    _formulaBarText = value;
                    OnPropertyChanged();

                    if (_selectedCell != null)
                    {
                        _selectedCell.EditText = value;
                        UpdateCellInput(_selectedCell.Row, _selectedCell.Column, value);
                    }
                }
            }
        }

        public string SelectedCellName
        {
            get => _selectedCellName;
            set
            {
                if (_selectedCellName != value)
                {
                    _selectedCellName = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusText
        {
            get => _statusText;
            set
            {
                if (_statusText != value)
                {
                    _statusText = value;
                    OnPropertyChanged();
                }
            }
        }

        public int RowCount => _spreadsheet.RowCount;
        public int ColumnCount => _spreadsheet.ColumnCount;

        #endregion

        #region Commands

        public ICommand NewCommand { get; private set; }
        public ICommand OpenCommand { get; private set; }
        public ICommand SaveCommand { get; private set; }
        public ICommand SaveAsCommand { get; private set; }
        public ICommand OpenFromDriveCommand { get; private set; }
        public ICommand SaveToDriveCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }

        public ICommand CutCommand { get; private set; }
        public ICommand CopyCommand { get; private set; }
        public ICommand PasteCommand { get; private set; }

        public ICommand InsertRowCommand { get; private set; }
        public ICommand DeleteRowCommand { get; private set; }
        public ICommand InsertColumnCommand { get; private set; }
        public ICommand DeleteColumnCommand { get; private set; }
        public ICommand RecalculateCommand { get; private set; }
        public ICommand FormatCellCommand { get; private set; }

        public ICommand ShowHelpCommand { get; private set; }
        public ICommand ShowAboutCommand { get; private set; }

        public ICommand SelectCellCommand { get; private set; }
        public ICommand EditCellCommand { get; private set; }

        private void InitializeCommands()
        {
            NewCommand = new RelayCommand(ExecuteNew);
            OpenCommand = new RelayCommand(ExecuteOpen);
            SaveCommand = new RelayCommand(ExecuteSave);
            SaveAsCommand = new RelayCommand(ExecuteSaveAs);
            OpenFromDriveCommand = new RelayCommand(ExecuteOpenFromDrive);
            SaveToDriveCommand = new RelayCommand(ExecuteSaveToDrive);
            ExitCommand = new RelayCommand(ExecuteExit);

            CutCommand = new RelayCommand(ExecuteCut);
            CopyCommand = new RelayCommand(ExecuteCopy);
            PasteCommand = new RelayCommand(ExecutePaste);

            InsertRowCommand = new RelayCommand(ExecuteInsertRow);
            DeleteRowCommand = new RelayCommand(ExecuteDeleteRow);
            InsertColumnCommand = new RelayCommand(ExecuteInsertColumn);
            DeleteColumnCommand = new RelayCommand(ExecuteDeleteColumn);
            RecalculateCommand = new RelayCommand(ExecuteRecalculate);
            FormatCellCommand = new RelayCommand(ExecuteFormatCell);

            ShowHelpCommand = new RelayCommand(ExecuteShowHelp);
            ShowAboutCommand = new RelayCommand(ExecuteShowAbout);

            SelectCellCommand = new RelayCommand<CellViewModel>(ExecuteSelectCell);
            EditCellCommand = new RelayCommand<CellViewModel>(ExecuteEditCell);
        }

        #endregion

        #region Command Implementations

        private void ExecuteNew()
        {
            var result = MessageBox.Show(
                "Створити нову таблицю? Незбережені зміни будуть втрачені.",
                "Підтвердження",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _currentFilePath = null;
                _spreadsheet = new Spreadsheet(50, 26);
                Mouse.OverrideCursor = Cursors.Wait;
                InitializeGrid();
                RecalculateAll();
                StatusText = "Створено нову таблицю";
                Mouse.OverrideCursor = null;
            }
        }

        private void ExecuteOpen()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Файли таблиць (*.spr)|*.spr|Всі файли (*.*)|*.*",
                Title = "Відкрити таблицю"
            };

            if (dialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    _spreadsheet = SpreadsheetSerializer.LoadFromFile(dialog.FileName);
                    _currentFilePath = dialog.FileName;
                    InitializeGrid();
                    RecalculateAll();
                    StatusText = $"Відкрито: {Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при відкритті файлу: {ex.Message}",
                        "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteSave()
        {
            if (string.IsNullOrEmpty(_currentFilePath))
            {
                ExecuteSaveAs();
            }
            else
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    SpreadsheetSerializer.SaveToFile(_spreadsheet, _currentFilePath);
                    StatusText = $"Збережено: {Path.GetFileName(_currentFilePath)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при збереженні файлу: {ex.Message}",
                        "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteSaveAs()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Файли таблиць (*.spr)|*.spr|Всі файли (*.*)|*.*",
                Title = "Зберегти таблицю як",
                DefaultExt = ".spr"
            };

            if (dialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    SpreadsheetSerializer.SaveToFile(_spreadsheet, dialog.FileName);
                    _currentFilePath = dialog.FileName;
                    StatusText = $"Збережено: {Path.GetFileName(dialog.FileName)}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка при збереженні файлу: {ex.Message}",
                        "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteOpenFromDrive()
        {
            try
            {
                var dialog = new GoogleDriveDialog(GoogleDriveDialog.DialogMode.Open);

                if (dialog.ShowDialog() == true && !string.IsNullOrEmpty(dialog.LocalFilePath))
                {
                    Mouse.OverrideCursor = Cursors.Wait;
                    try
                    {
                        _spreadsheet = SpreadsheetSerializer.LoadFromFile(dialog.LocalFilePath);
                        _currentFilePath = dialog.LocalFilePath;
                        InitializeGrid();
                        RecalculateAll();
                        StatusText = $"Відкрито з Google Drive: {Path.GetFileName(dialog.LocalFilePath)}";
                    }
                    finally
                    {
                        Mouse.OverrideCursor = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка відкриття з Google Drive:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteSaveToDrive()
        {
            try
            {
                string tempPath;

                if (string.IsNullOrEmpty(_currentFilePath))
                {
                    tempPath = Path.Combine(Path.GetTempPath(), "spreadsheet_temp.spr");
                }
                else
                {
                    tempPath = _currentFilePath;
                }

                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    SpreadsheetSerializer.SaveToFile(_spreadsheet, tempPath);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }

                var dialog = new GoogleDriveDialog(GoogleDriveDialog.DialogMode.Save)
                {
                    LocalFilePath = tempPath
                };

                if (dialog.ShowDialog() == true)
                {
                    StatusText = "Збережено на Google Drive";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка збереження на Google Drive:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ExecuteExit()
        {
            Application.Current.Shutdown();
        }

        private void ExecuteCut()
        {
            ExecuteCopy();
            if (_selectedCell != null)
            {
                FormulaBarText = string.Empty;
                StatusText = "Виріжено";
            }
        }

        private void ExecuteCopy()
        {
            if (_selectedCell != null)
            {
                Clipboard.SetText(_selectedCell.EditText);
                StatusText = "Скопійовано";
            }
        }

        private void ExecutePaste()
        {
            if (_selectedCell != null && Clipboard.ContainsText())
            {
                string text = Clipboard.GetText();
                UpdateCellInput(_selectedCell.Row, _selectedCell.Column, text);
                StatusText = "Вставлено";
            }
        }

        private void ExecuteInsertRow()
        {
            if (_selectedCell != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    _spreadsheet.InsertRow(_selectedCell.Row);
                    InitializeGrid();
                    RecalculateAll();
                    StatusText = $"Вставлено рядок {_selectedCell.Row + 1}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteDeleteRow()
        {
            if (_selectedCell != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    _spreadsheet.DeleteRow(_selectedCell.Row);
                    InitializeGrid();
                    RecalculateAll();
                    StatusText = $"Видалено рядок {_selectedCell.Row + 1}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteInsertColumn()
        {
            if (_selectedCell != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    string colName = SpreadsheetUtils.ToColumnName(_selectedCell.Column);
                    _spreadsheet.InsertColumn(colName);
                    InitializeGrid();
                    RecalculateAll();
                    StatusText = $"Вставлено стовпець {colName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteDeleteColumn()
        {
            if (_selectedCell != null)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    string colName = SpreadsheetUtils.ToColumnName(_selectedCell.Column);
                    _spreadsheet.DeleteColumn(colName);
                    InitializeGrid();
                    RecalculateAll();
                    StatusText = $"Видалено стовпець {colName}";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Помилка: {ex.Message}", "Помилка",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteRecalculate()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                RecalculateAll();
                StatusText = "Перераховано";
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void ExecuteFormatCell()
        {
            if (_selectedCell == null) return;

            string colName = SpreadsheetUtils.ToColumnName(_selectedCell.Column);
            var cell = _spreadsheet.GetCell(_selectedCell.Row, colName);
            double colWidth = _spreadsheet.GetColumnWidth(colName);
            double rowHeight = _spreadsheet.GetRowHeight(_selectedCell.Row);

            var dialog = new FormatDialog(cell.Format, colWidth, rowHeight);
            if (dialog.ShowDialog() == true)
            {
                Mouse.OverrideCursor = Cursors.Wait;
                try
                {
                    _spreadsheet.SetCellFormat(_selectedCell.Row, colName, dialog.Format);

                    _spreadsheet.SetColumnWidth(colName, dialog.ColumnWidth);
                    _spreadsheet.SetRowHeight(_selectedCell.Row, dialog.RowHeight);

                    InitializeGrid();
                    RecalculateAll();

                    StatusText = "Форматування застосовано";
                }
                finally
                {
                    Mouse.OverrideCursor = null;
                }
            }
        }

        private void ExecuteShowHelp()
        {
            string help = @"ІНСТРУКЦІЯ КОРИСТУВАЧА

ОСНОВНІ МОЖЛИВОСТІ:
  Введення даних і формул у комірки
  Формули починаються з '='
  Посилання на комірки: A1, B5, тощо

ОПЕРАЦІЇ:
  +, -          Додавання, віднімання (включно з унарними)
  =, <, >       Порівняння
  and, or, eqv  Логічні операції
  not           Логічне заперечення
  ()            Дужки для пріоритету
  
ПРІОРИТЕТ ОПЕРАЦІЙ (ВІД ВИСОКОГО ДО НИЗЬКОГО):
  1. Унарні + та -, not
  2. +, -
  3. порівняння
  4. and
  5. or
  
ПРИКЛАДИ ФОРМУЛ:
  =A1+B2
  =5+A1-10
  =(A1>5) and (B2<10)
  =not(A1=B1)
  =-A1+B2

РЕЖИМИ ВІДОБРАЖЕННЯ:
  Значення - показує результати обчислень
  Вирази - показує формули в комірках

ОПЕРАЦІЇ З ТАБЛИЦЕЮ:
  Вставка/видалення рядків
  Вставка/видалення стовпців
  Форматування комірок (шрифт, колір, розмір)";

            MessageBox.Show(help, "Допомога", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteShowAbout()
        {
            string about = @"ЕЛЕКТРОННА ТАБЛИЦЯ
Версія hz kakaya

Програма для роботи з електронними таблицями
з підтримкою формул та виразів.

Підтримувані операції:
  Арифметичні: +, - (у тому числі унарні)
  Порівняння: =, <, >
  Логічні: and, or, not
  Цілі числа довільної довжини
  Посилання на комірки
  Дужки для пріоритету операцій

Особливості:
  Синтаксична перевірка виразів
  Автоматичне оновлення формул при вставці/видаленні рядків і стовпців
  Виявлення циклічних залежностей
  Два режими відображення

2025 Лабораторна робота ООП";

            MessageBox.Show(about, "Про програму", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ExecuteSelectCell(CellViewModel cell)
        {
            if (_selectedCell != null)
            {
                _selectedCell.IsSelected = false;
                if (_selectedCell.IsEditing)
                {
                    _selectedCell.IsEditing = false;
                    UpdateCellInput(_selectedCell.Row, _selectedCell.Column, _selectedCell.EditText);
                }
            }

            _selectedCell = cell;
            _selectedCell.IsSelected = true;

            string colName = SpreadsheetUtils.ToColumnName(cell.Column);
            SelectedCellName = $"{colName}{cell.Row + 1}";

            FormulaBarText = cell.EditText;
        }

        private void ExecuteEditCell(CellViewModel cell)
        {
            if (_selectedCell != null && _selectedCell != cell)
            {
                ExecuteSelectCell(cell);
            }

            _selectedCell.IsEditing = true;
        }

        #endregion

        #region Helper Methods

        private void InitializeGrid()
        {
            Mouse.OverrideCursor = Cursors.Wait;
            try
            {
                Rows.Clear();
                ColumnHeaders.Clear();
                RowHeaders.Clear();

                int maxVisibleRows = Math.Min(_spreadsheet.RowCount, 100);
                int maxVisibleCols = Math.Min(_spreadsheet.ColumnCount, 50);

                for (int col = 0; col < maxVisibleCols; col++)
                {
                    string colName = SpreadsheetUtils.ToColumnName(col);
                    double width = _spreadsheet.GetColumnWidth(colName);
                    ColumnHeaders.Add(new ColumnHeaderViewModel { Name = colName, Width = width });
                }

                for (int row = 0; row < maxVisibleRows; row++)
                {
                    double height = _spreadsheet.GetRowHeight(row);
                    RowHeaders.Add(new RowHeaderViewModel { Number = row + 1, Height = height });
                }

                for (int row = 0; row < maxVisibleRows; row++)
                {
                    var rowVm = new RowViewModel();
                    for (int col = 0; col < maxVisibleCols; col++)
                    {
                        string colName = SpreadsheetUtils.ToColumnName(col);
                        var cell = _spreadsheet.GetReadOnlyCell(row, colName);

                        var cellVm = new CellViewModel
                        {
                            Row = row,
                            Column = col,
                            Width = _spreadsheet.GetColumnWidth(colName),
                            Height = _spreadsheet.GetRowHeight(row),
                            EditText = cell.Input
                        };

                        UpdateCellDisplay(cellVm, cell);
                        rowVm.Cells.Add(cellVm);
                    }
                    Rows.Add(rowVm);
                }

                OnPropertyChanged(nameof(RowCount));
                OnPropertyChanged(nameof(ColumnCount));

                if (_spreadsheet.RowCount > 100 || _spreadsheet.ColumnCount > 50)
                {
                    StatusText = $"Показано {maxVisibleRows}×{maxVisibleCols} комірок з {_spreadsheet.RowCount}×{_spreadsheet.ColumnCount}";
                }
            }
            finally
            {
                Mouse.OverrideCursor = null;
            }
        }

        private void UpdateCellInput(int row, int col, string input)
        {
            string colName = SpreadsheetUtils.ToColumnName(col);
            _spreadsheet.SetCellInput(row, colName, input);
            RecalculateAll();
        }

        private void RecalculateAll()
        {
            _spreadsheet.RecalculateAllCells();
            UpdateAllCellsDisplay();
        }

        private void UpdateAllCellsDisplay()
        {
            for (int row = 0; row < Rows.Count; row++)
            {
                for (int col = 0; col < Rows[row].Cells.Count; col++)
                {
                    string colName = SpreadsheetUtils.ToColumnName(col);
                    var cell = _spreadsheet.GetReadOnlyCell(row, colName);
                    var cellVm = Rows[row].Cells[col];
                    UpdateCellDisplay(cellVm, cell);
                }
            }
        }

        private void UpdateCellDisplay(CellViewModel cellVm, Cell cell)
        {
            cellVm.EditText = cell.Input;

            if (ShowFormulas && !string.IsNullOrEmpty(cell.Input))
            {
                cellVm.DisplayText = cell.Input;
            }
            else
            {
                if (cell.CalculatedValue != null)
                {
                    cellVm.DisplayText = cell.CalculatedValue.ToString() ?? string.Empty;
                }
                else
                {
                    cellVm.DisplayText = string.Empty;
                }
            }

            ApplyCellFormat(cellVm, cell.Format);
        }

        private void ApplyCellFormat(CellViewModel cellVm, CellFormat format)
        {
            cellVm.FontWeight = format.IsBold ? FontWeights.Bold : FontWeights.Normal;
            cellVm.FontStyle = format.IsItalic ? FontStyles.Italic : FontStyles.Normal;
            cellVm.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(format.TextColor));

            if (!cellVm.IsSelected)
            {
                cellVm.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(format.BackgroundColor));
            }

            if (format.IsUnderline)
            {
                cellVm.TextDecorations = TextDecorations.Underline;
            }
            else
            {
                cellVm.TextDecorations = null;
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
