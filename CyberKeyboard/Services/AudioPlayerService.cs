using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows.Threading;
using CyberKeyboard.Models;

namespace CyberKeyboard.Services
{
    public class AudioPlayerService : INotifyPropertyChanged
    {
        private static readonly string[] AudioExtensions =
            { ".mp3", ".wav", ".flac", ".m4a", ".aac", ".wma", ".ogg" };

        private readonly MediaPlayer _player;
        private readonly DispatcherTimer _positionTimer;
        private readonly Random _rand = new();

        public ObservableCollection<TrackItem> Playlist { get; } = new();

        private TrackItem? _currentTrack;
        public TrackItem? CurrentTrack
        {
            get => _currentTrack;
            private set
            {
                if (_currentTrack != null) _currentTrack.IsPlaying = false;
                _currentTrack = value;
                if (_currentTrack != null) _currentTrack.IsPlaying = _isPlaying;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CurrentTrackTitle));
            }
        }

        public string CurrentTrackTitle => CurrentTrack != null ? CurrentTrack.Title : "NO TRACK SELECTED";

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            private set
            {
                _isPlaying = value;
                if (CurrentTrack != null) CurrentTrack.IsPlaying = value;
                OnPropertyChanged();
            }
        }

        private TimeSpan _currentPosition = TimeSpan.Zero;
        public TimeSpan CurrentPosition
        {
            get => _currentPosition;
            private set { _currentPosition = value; OnPropertyChanged(); OnPropertyChanged(nameof(PositionString)); }
        }

        private TimeSpan _totalDuration = TimeSpan.Zero;
        public TimeSpan TotalDuration
        {
            get => _totalDuration;
            private set { _totalDuration = value; OnPropertyChanged(); OnPropertyChanged(nameof(DurationString)); }
        }

        public string PositionString => $"{(int)CurrentPosition.TotalMinutes}:{CurrentPosition.Seconds:D2}";
        public string DurationString => $"{(int)TotalDuration.TotalMinutes}:{TotalDuration.Seconds:D2}";

        private double _progressRatio;
        public double ProgressRatio
        {
            get => _progressRatio;
            set
            {
                _progressRatio = Math.Clamp(value, 0.0, 1.0);
                OnPropertyChanged();
            }
        }

        private double _volume = 0.75;
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = Math.Clamp(value, 0.0, 1.0);
                _player.Volume = _volume;
                OnPropertyChanged();
            }
        }

        public bool IsShuffle { get; set; } = false;
        public bool IsRepeat { get; set; } = false;

        public event Action? PlaybackStateChanged;

        public AudioPlayerService()
        {
            _player = new MediaPlayer();
            _player.Volume = _volume;

            _player.MediaOpened += OnMediaOpened;
            _player.MediaEnded += OnMediaEnded;
            _player.MediaFailed += OnMediaFailed;

            _positionTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(200)
            };
            _positionTimer.Tick += OnPositionTimerTick;
        }

        private void OnMediaOpened(object? sender, EventArgs e)
        {
            if (_player.NaturalDuration.HasTimeSpan)
            {
                TotalDuration = _player.NaturalDuration.TimeSpan;
                if (CurrentTrack != null)
                {
                    CurrentTrack.Duration = TotalDuration;
                }
            }
        }

        private void OnMediaEnded(object? sender, EventArgs e)
        {
            if (IsRepeat && CurrentTrack != null)
            {
                _player.Position = TimeSpan.Zero;
                _player.Play();
            }
            else
            {
                Next();
            }
        }

        private void OnMediaFailed(object? sender, ExceptionEventArgs e)
        {
            Next();
        }

        private void OnPositionTimerTick(object? sender, EventArgs e)
        {
            if (_isPlaying && TotalDuration.TotalSeconds > 0)
            {
                CurrentPosition = _player.Position;
                ProgressRatio = CurrentPosition.TotalSeconds / TotalDuration.TotalSeconds;
            }
        }

        public void PlayTrack(TrackItem track)
        {
            try
            {
                CurrentTrack = track;
                _player.Open(new Uri(track.FilePath));
                _player.Play();
                IsPlaying = true;
                _positionTimer.Start();
                PlaybackStateChanged?.Invoke();
            }
            catch
            {
                // File open error
            }
        }

        public void TogglePlayPause()
        {
            if (CurrentTrack == null)
            {
                if (Playlist.Count > 0)
                {
                    PlayTrack(Playlist[0]);
                }
                return;
            }

            if (IsPlaying)
            {
                _player.Pause();
                IsPlaying = false;
                _positionTimer.Stop();
            }
            else
            {
                _player.Play();
                IsPlaying = true;
                _positionTimer.Start();
            }
            PlaybackStateChanged?.Invoke();
        }

        public void Stop()
        {
            _player.Stop();
            IsPlaying = false;
            CurrentPosition = TimeSpan.Zero;
            ProgressRatio = 0;
            _positionTimer.Stop();
            PlaybackStateChanged?.Invoke();
        }

        public void Next()
        {
            if (Playlist.Count == 0) return;

            if (IsShuffle && Playlist.Count > 1)
            {
                int nextIdx = _rand.Next(Playlist.Count);
                PlayTrack(Playlist[nextIdx]);
                return;
            }

            int currentIdx = CurrentTrack != null ? Playlist.IndexOf(CurrentTrack) : -1;
            int nextIndex = (currentIdx + 1) % Playlist.Count;
            PlayTrack(Playlist[nextIndex]);
        }

        public void Previous()
        {
            if (Playlist.Count == 0) return;

            if (CurrentPosition.TotalSeconds > 3)
            {
                Seek(0);
                return;
            }

            int currentIdx = CurrentTrack != null ? Playlist.IndexOf(CurrentTrack) : 0;
            int prevIndex = (currentIdx - 1 + Playlist.Count) % Playlist.Count;
            PlayTrack(Playlist[prevIndex]);
        }

        public void Seek(double ratio)
        {
            ratio = Math.Clamp(ratio, 0.0, 1.0);
            if (TotalDuration.TotalSeconds > 0)
            {
                var target = TimeSpan.FromSeconds(TotalDuration.TotalSeconds * ratio);
                _player.Position = target;
                CurrentPosition = target;
                ProgressRatio = ratio;
            }
        }

        public void AddFiles(IEnumerable<string> filePaths, bool autoPlayIfEmpty = false)
        {
            bool wasEmpty = Playlist.Count == 0;

            foreach (var path in filePaths)
            {
                if (File.Exists(path))
                {
                    string ext = Path.GetExtension(path).ToLowerInvariant();
                    if (AudioExtensions.Contains(ext))
                    {
                        var track = new TrackItem
                        {
                            Title = Path.GetFileNameWithoutExtension(path),
                            FilePath = path,
                            Index = Playlist.Count + 1
                        };
                        Playlist.Add(track);
                    }
                }
                else if (Directory.Exists(path))
                {
                    AddFolder(path, false);
                }
            }

            RenumberPlaylist();

            if (wasEmpty && autoPlayIfEmpty && Playlist.Count > 0)
            {
                PlayTrack(Playlist[0]);
            }
        }

        public void AddFolder(string folderPath, bool autoPlayIfEmpty = false)
        {
            if (!Directory.Exists(folderPath)) return;
            bool wasEmpty = Playlist.Count == 0;

            try
            {
                var dir = new DirectoryInfo(folderPath);
                var files = dir.GetFiles("*.*", SearchOption.AllDirectories)
                               .Where(f => AudioExtensions.Contains(f.Extension.ToLowerInvariant()))
                               .OrderBy(f => f.Name);

                foreach (var file in files)
                {
                    Playlist.Add(new TrackItem
                    {
                        Title = Path.GetFileNameWithoutExtension(file.Name),
                        FilePath = file.FullName,
                        Index = Playlist.Count + 1
                    });
                }

                RenumberPlaylist();

                if (wasEmpty && autoPlayIfEmpty && Playlist.Count > 0)
                {
                    PlayTrack(Playlist[0]);
                }
            }
            catch
            {
                // Directory scan access error
            }
        }

        public void RemoveTrack(TrackItem track)
        {
            bool isCurrent = track == CurrentTrack;
            Playlist.Remove(track);
            RenumberPlaylist();

            if (isCurrent)
            {
                if (Playlist.Count > 0) Next();
                else Stop();
            }
        }

        public void ClearPlaylist()
        {
            Stop();
            CurrentTrack = null;
            Playlist.Clear();
        }

        private void RenumberPlaylist()
        {
            for (int i = 0; i < Playlist.Count; i++)
            {
                Playlist[i].Index = i + 1;
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
