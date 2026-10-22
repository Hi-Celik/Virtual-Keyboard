using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace CyberKeyboard.Models
{
    public class FileTreeItem : INotifyPropertyChanged
    {
        private static readonly string[] AudioExtensions =
            { ".mp3", ".wav", ".flac", ".m4a", ".aac", ".wma", ".ogg" };

        public string Name { get; set; } = string.Empty;
        public string FullPath { get; set; } = string.Empty;
        public bool IsDirectory { get; set; }
        public bool IsAudioFile { get; set; }
        public string Icon { get; set; } = "📁";

        public System.Windows.Visibility AddFolderButtonVisibility => IsDirectory ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
        public System.Windows.Visibility AudioFileButtonVisibility => IsAudioFile ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;

        public ObservableCollection<FileTreeItem> Children { get; set; } = new();

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                    if (_isExpanded)
                    {
                        LoadChildren();
                    }
                }
            }
        }

        private bool _hasLoadedChildren = false;

        public void AddDummyChild()
        {
            Children.Add(new FileTreeItem { Name = "Loading..." });
        }

        public void LoadChildren()
        {
            if (_hasLoadedChildren || !IsDirectory) return;
            _hasLoadedChildren = true;

            Children.Clear();

            try
            {
                var dirInfo = new DirectoryInfo(FullPath);

                // Add Subdirectories
                foreach (var dir in dirInfo.GetDirectories().OrderBy(d => d.Name))
                {
                    // Skip hidden or system folders
                    if ((dir.Attributes & FileAttributes.Hidden) != 0 || (dir.Attributes & FileAttributes.System) != 0)
                        continue;

                    var item = new FileTreeItem
                    {
                        Name = dir.Name,
                        FullPath = dir.FullName,
                        IsDirectory = true,
                        Icon = "📁"
                    };
                    item.AddDummyChild();
                    Children.Add(item);
                }

                // Add Audio Files
                foreach (var file in dirInfo.GetFiles().OrderBy(f => f.Name))
                {
                    string ext = file.Extension.ToLowerInvariant();
                    if (AudioExtensions.Contains(ext))
                    {
                        Children.Add(new FileTreeItem
                        {
                            Name = file.Name,
                            FullPath = file.FullName,
                            IsDirectory = false,
                            IsAudioFile = true,
                            Icon = "🎵"
                        });
                    }
                }
            }
            catch
            {
                // Silent fail for unauthorized or inaccessible folders
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
