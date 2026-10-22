using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CyberKeyboard.Models
{
    public enum KeyType
    {
        Standard,
        Modifier,
        Action,
        Navigation,
        Space,
        Function
    }

    public class KeyModel : INotifyPropertyChanged
    {
        public string NormalText { get; set; } = string.Empty;
        public string ShiftText { get; set; } = string.Empty;
        public string AltGrText { get; set; } = string.Empty;
        public byte? VkCode { get; set; }
        public string? SpecialAction { get; set; }
        public double WidthRatio { get; set; } = 1.0;
        public KeyType Type { get; set; } = KeyType.Standard;

        private string _currentDisplay = string.Empty;
        public string CurrentDisplay
        {
            get => _currentDisplay;
            set { _currentDisplay = value; OnPropertyChanged(); }
        }

        private string _cornerShiftText = string.Empty;
        public string CornerShiftText
        {
            get => _cornerShiftText;
            set { _cornerShiftText = value; OnPropertyChanged(); }
        }

        private string _cornerAltGrText = string.Empty;
        public string CornerAltGrText
        {
            get => _cornerAltGrText;
            set { _cornerAltGrText = value; OnPropertyChanged(); }
        }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set { _isActive = value; OnPropertyChanged(); }
        }

        public void RefreshState(bool isShift, bool isCaps, bool isAltGr)
        {
            if (Type == KeyType.Modifier || Type == KeyType.Action || Type == KeyType.Navigation || Type == KeyType.Function)
            {
                CurrentDisplay = NormalText;
                CornerShiftText = string.Empty;
                CornerAltGrText = string.Empty;
                return;
            }

            bool upper = isShift ^ isCaps;

            if (isAltGr && !string.IsNullOrEmpty(AltGrText))
            {
                CurrentDisplay = AltGrText;
            }
            else if (isShift && !string.IsNullOrEmpty(ShiftText))
            {
                CurrentDisplay = ShiftText;
            }
            else
            {
                CurrentDisplay = upper ? NormalText.ToUpperInvariant() : NormalText.ToLowerInvariant();
            }

            CornerShiftText = ShiftText != NormalText ? ShiftText : string.Empty;
            CornerAltGrText = AltGrText;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
