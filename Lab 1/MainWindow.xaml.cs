using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Lab_1.Models;
using Lab_1.ViewModels;
using Microsoft.Win32;

namespace Lab_1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            var vm = new MainViewModel();
            DataContext = vm;
            InputBindings.Add(new KeyBinding(vm.NewCommand, Key.N, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(vm.OpenCommand, Key.O, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(vm.SaveCommand, Key.S, ModifierKeys.Control));
            InputBindings.Add(new KeyBinding(vm.CutCommand, Key.K, ModifierKeys.Control));
        }

        private void CellTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            var textBox = sender as TextBox;
            if (textBox == null) return;
            
            var cellVm = textBox.DataContext as CellViewModel;
            if (cellVm == null) return;

            var viewModel = DataContext as MainViewModel;
            if (viewModel == null) return;

            if (e.Key == Key.Enter)
            {
                cellVm.IsEditing = false;

                string colName = SpreadsheetUtils.ToColumnName(cellVm.Column);
                viewModel.FormulaBarText = cellVm.EditText;

                int nextRow = cellVm.Row + 1;
                if (nextRow < viewModel.RowCount)
                {
                    var nextCell = viewModel.Rows[nextRow].Cells[cellVm.Column];
                    viewModel.SelectCellCommand.Execute(nextCell);
                }

                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                cellVm.IsEditing = false;
                e.Handled = true;
            }
            else if (e.Key == Key.Tab)
            {
                cellVm.IsEditing = false;

                string colName = SpreadsheetUtils.ToColumnName(cellVm.Column);
                viewModel.FormulaBarText = cellVm.EditText;

                int nextCol = cellVm.Column + 1;
                if (nextCol < viewModel.ColumnCount)
                {
                    var nextCell = viewModel.Rows[cellVm.Row].Cells[nextCol];
                    viewModel.SelectCellCommand.Execute(nextCell);
                }

                e.Handled = true;
            }
        }
    }
}