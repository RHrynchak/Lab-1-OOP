using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
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

namespace Lab_1
{
    /// <summary>
    /// Interaction logic for GoogleDriveDialog.xaml
    /// </summary>
    public partial class GoogleDriveDialog : Window
    {
        private readonly GoogleDriveService _driveService;
        private List<GoogleDriveFile> _files = new();
        private GoogleDriveFile? _selectedFile;

        public enum DialogMode
        {
            Open,
            Save
        }

        public DialogMode Mode { get; set; }
        public string? SelectedFileId { get; private set; }
        public string? LocalFilePath { get; set; }

        public GoogleDriveDialog(DialogMode mode)
        {
            InitializeComponent();
            _driveService = new GoogleDriveService();
            Mode = mode;

            Title = mode == DialogMode.Open
                ? "Відкрити з Google Drive"
                : "Зберегти на Google Drive";

            if (mode == DialogMode.Open)
            {
                UploadButton.Visibility = Visibility.Collapsed;
                DownloadButton.Content = "⬇ Відкрити";
            }
            else
            {
                DownloadButton.Visibility = Visibility.Collapsed;
                UploadButton.Content = "⬆ Зберегти";
            }
        }

        private async void AuthButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                AuthButton.IsEnabled = false;

                bool success = await _driveService.AuthenticateAsync();

                if (success)
                {
                    StatusTextBlock.Text = "✅ Підключено до Google Drive";
                    AuthButton.Content = "Від'єднатися";
                    EnableButtons(true);
                    await LoadFilesAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Помилка підключення до Google Drive:\n\n{ex.Message}\n\n" +
                    "Переконайтеся що файл credentials.json знаходиться в каталозі програми.",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                AuthButton.IsEnabled = true;
            }
        }

        private async void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await LoadFilesAsync();
        }

        private async void UploadButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(LocalFilePath) || !File.Exists(LocalFilePath))
            {
                MessageBox.Show("Спочатку збережіть файл локально",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ShowLoading(true);

                string fileName = Path.GetFileName(LocalFilePath);

                var existingFile = _files.FirstOrDefault(f => f.Name == fileName);

                if (existingFile != null)
                {
                    var result = MessageBox.Show(
                        $"Файл '{fileName}' вже існує на Google Drive. Перезаписати?",
                        "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                    {
                        await _driveService.UpdateFileAsync(existingFile.Id, LocalFilePath);
                        MessageBox.Show("Файл успішно оновлено на Google Drive",
                            "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                else
                {
                    string fileId = await _driveService.UploadFileAsync(LocalFilePath, fileName);
                    MessageBox.Show("Файл успішно завантажено на Google Drive",
                        "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);
                }

                await LoadFilesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження файлу:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                ShowLoading(false);
            }
        }

        private async void DownloadButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFile == null)
            {
                MessageBox.Show("Виберіть файл зі списку",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ShowLoading(true);

                string tempPath = Path.Combine(Path.GetTempPath(), _selectedFile.Name);
                await _driveService.DownloadFileAsync(_selectedFile.Id, tempPath);

                LocalFilePath = tempPath;
                SelectedFileId = _selectedFile.Id;

                DialogResult = true;
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження файлу:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                ShowLoading(false);
            }
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (_selectedFile == null)
            {
                MessageBox.Show("Виберіть файл зі списку",
                    "Увага", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show(
                $"Видалити файл '{_selectedFile.Name}' з Google Drive?",
                "Підтвердження", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                Mouse.OverrideCursor = Cursors.Wait;
                ShowLoading(true);

                await _driveService.DeleteFileAsync(_selectedFile.Id);
                MessageBox.Show("Файл успішно видалено",
                    "Успіх", MessageBoxButton.OK, MessageBoxImage.Information);

                await LoadFilesAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка видалення файлу:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                Mouse.OverrideCursor = null;
                ShowLoading(false);
            }
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (Mode == DialogMode.Open)
            {
                DownloadButton_Click(sender, e);
            }
            else
            {
                UploadButton_Click(sender, e);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void FilesDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _selectedFile = FilesDataGrid.SelectedItem as GoogleDriveFile;

            bool hasSelection = _selectedFile != null;
            DownloadButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
            OkButton.IsEnabled = hasSelection || Mode == DialogMode.Save;
        }

        private async System.Threading.Tasks.Task LoadFilesAsync()
        {
            try
            {
                ShowLoading(true);
                _files = await _driveService.ListFilesAsync();
                FilesDataGrid.ItemsSource = _files;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Помилка завантаження списку файлів:\n{ex.Message}",
                    "Помилка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                ShowLoading(false);
            }
        }

        private void EnableButtons(bool enabled)
        {
            RefreshButton.IsEnabled = enabled;
            UploadButton.IsEnabled = enabled;
            DeleteButton.IsEnabled = false;
            DownloadButton.IsEnabled = false;
        }

        private void ShowLoading(bool show)
        {
            LoadingPanel.Visibility = show ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    public class CountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count == 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
