using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Lab_1.ViewModels
{
    public class CellViewModel : INotifyPropertyChanged
    {
        private string _displayText = string.Empty;
        private string _editText = string.Empty;
        private bool _isEditing = false;
        private bool _isSelected = false;
        private double _width = 80;
        private double _height = 20;
        private Brush _backgroundActual = Brushes.White;
        private Brush _background = Brushes.White;
        private Brush _foreground = Brushes.Black;
        private FontWeight _fontWeight = FontWeights.Normal;
        private FontStyle _fontStyle = FontStyles.Normal;
        private TextDecorationCollection? _textDecorations;

        public int Row { get; set; }
        public int Column { get; set; }

        public string DisplayText
        {
            get => _displayText;
            set
            {
                if (_displayText != value)
                {
                    _displayText = value;
                    OnPropertyChanged();
                }
            }
        }

        public string EditText
        {
            get => _editText;
            set
            {
                if (_editText != value)
                {
                    _editText = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditing
        {
            get => _isEditing;
            set
            {
                if (_isEditing != value)
                {
                    _isEditing = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                    UpdateSelectionBackground();
                }
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnPropertyChanged();
                }
            }
        }

        public Brush Foreground
        {
            get => _foreground;
            set
            {
                if (_foreground != value)
                {
                    _foreground = value;
                    OnPropertyChanged();
                }
            }
        }

        public FontWeight FontWeight
        {
            get => _fontWeight;
            set
            {
                if (_fontWeight != value)
                {
                    _fontWeight = value;
                    OnPropertyChanged();
                }
            }
        }

        public FontStyle FontStyle
        {
            get => _fontStyle;
            set
            {
                if (_fontStyle != value)
                {
                    _fontStyle = value;
                    OnPropertyChanged();
                }
            }
        }

        public TextDecorationCollection? TextDecorations
        {
            get => _textDecorations;
            set
            {
                if (_textDecorations != value)
                {
                    _textDecorations = value;
                    OnPropertyChanged();
                }
            }
        }

        private void UpdateSelectionBackground()
        {
            if (IsSelected)
            {
                _backgroundActual = Background;
                Background = new SolidColorBrush(Color.FromRgb(200, 220, 240));
            }
            else
            {
                Background = _backgroundActual;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RowViewModel
    {
        public ObservableCollection<CellViewModel> Cells { get; } = new();
    }

    public class ColumnHeaderViewModel : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private double _width = 80;

        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class RowHeaderViewModel : INotifyPropertyChanged
    {
        private int _number;
        private double _height = 20;

        public int Number
        {
            get => _number;
            set
            {
                if (_number != value)
                {
                    _number = value;
                    OnPropertyChanged();
                }
            }
        }

        public double Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
