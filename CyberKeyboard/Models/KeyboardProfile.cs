using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CyberKeyboard.Graphics;
using CyberKeyboard.Services;

namespace CyberKeyboard.Models
{
    public class KeyboardProfile : INotifyPropertyChanged
    {
        private string _name = "Varsayılan";
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

        public DateTime LastModified { get; set; } = DateTime.Now;
        public bool IsBuiltIn { get; set; } = false;

        // Monitor & Display
        public int ScreenIndex { get; set; } = 0;
        public string ScreenDeviceName { get; set; } = string.Empty;
        public bool IsFullscreen { get; set; } = false;
        public double RowHeight { get; set; } = 68.0;
        public bool IsDockBottom { get; set; } = true;
        public bool IsNumpadVisible { get; set; } = true;
        public bool IsPlayerVisible { get; set; } = false;

        // Window geometry (for windowed mode)
        public double WindowWidth { get; set; } = 1240;
        public double WindowHeight { get; set; } = 550;
        public double WindowLeft { get; set; } = 100;
        public double WindowTop { get; set; } = 100;

        // Themes & Languages
        public CyberTheme Theme { get; set; } = CyberTheme.CyberCyan;
        public UiLanguage UiLanguage { get; set; } = UiLanguage.Turkish;
        public SupportedLanguage TypingLanguage { get; set; } = SupportedLanguage.Turkish;

        // Sound & Media
        public bool IsSoundEnabled { get; set; } = true;
        public double Volume { get; set; } = 0.75;
        public bool IsShuffle { get; set; } = false;
        public bool IsRepeat { get; set; } = false;

        // Playlist file paths
        public List<string> PlaylistFiles { get; set; } = new();

        // Clock Sizing
        public string ClockSize { get; set; } = "Medium";

        // Computed display properties for UI data binding
        public string DisplayDetails => $"{Theme} • {(int)RowHeight}px • {UiLanguage.ToString().Substring(0, 2).ToUpper()} • {(IsFullscreen ? "FULLSCREEN" : "WINDOWED")}";
        public string FormattedDate => LastModified.ToString("dd.MM.yyyy HH:mm");

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public KeyboardProfile Clone(string newName)
        {
            return new KeyboardProfile
            {
                Name = newName,
                LastModified = DateTime.Now,
                IsBuiltIn = false,
                ScreenIndex = ScreenIndex,
                ScreenDeviceName = ScreenDeviceName,
                IsFullscreen = IsFullscreen,
                RowHeight = RowHeight,
                IsDockBottom = IsDockBottom,
                IsNumpadVisible = IsNumpadVisible,
                IsPlayerVisible = IsPlayerVisible,
                WindowWidth = WindowWidth,
                WindowHeight = WindowHeight,
                WindowLeft = WindowLeft,
                WindowTop = WindowTop,
                Theme = Theme,
                UiLanguage = UiLanguage,
                TypingLanguage = TypingLanguage,
                IsSoundEnabled = IsSoundEnabled,
                Volume = Volume,
                IsShuffle = IsShuffle,
                IsRepeat = IsRepeat,
                PlaylistFiles = new List<string>(PlaylistFiles),
                ClockSize = ClockSize
            };
        }
    }
}
