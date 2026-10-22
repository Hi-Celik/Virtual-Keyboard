using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace CyberKeyboard.Models
{
    public class TrackItem : INotifyPropertyChanged
    {
        public string Title { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }

        public string DurationString => Duration.TotalSeconds > 0
            ? $"{(int)Duration.TotalMinutes}:{Duration.Seconds:D2}"
            : "--:--";

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set { _isPlaying = value; OnPropertyChanged(); }
        }

        private int _index;
        public int Index
        {
            get => _index;
            set { _index = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
