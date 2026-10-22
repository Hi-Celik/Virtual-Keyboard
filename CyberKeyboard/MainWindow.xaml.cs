using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using CyberKeyboard.Controls;
using CyberKeyboard.Graphics;
using CyberKeyboard.Models;
using CyberKeyboard.Native;
using CyberKeyboard.Services;

namespace CyberKeyboard
{
    public partial class MainWindow : Window
    {
        private IntPtr _hwnd;
        private readonly LanguageService _languageService;
        private readonly AudioService _audioService;
        private readonly InputService _inputService;
        private readonly AudioPlayerService _audioPlayer;

        private readonly List<CyberKeyControl> _allKeyControls = new();
        private readonly List<KeyModel> _allKeyModels = new();
        private readonly ObservableCollection<FileTreeItem> _fileTreeRoots = new();
        private readonly ObservableCollection<ScreenInfo> _screensList = new();

        private List<ScreenInfo> _screens = new();
        private int _currentScreenIndex = 0;
        private bool _isFullscreen = false;
        private bool _isNumpadVisible = true;
        private double _rowHeight = 68.0;
        private bool _isDockBottom = true;
        private bool _isPlayerVisible = false;
        private bool _isUserSeeking = false;

        // Live stream buffer for telemetry
        private readonly StringBuilder _liveBuffer = new();
        private int _totalKeysPressed = 0;

        // Digital Chrono Clock & Sizing
        private readonly DispatcherTimer _clockTimer;
        private readonly DateTime _sessionStartTime = DateTime.Now;
        private bool _clockColonVisible = true;
        private bool _isUpdatingClockSizeDropdown = false;
        private string _currentClockSize = "Medium";

        // Cyber Month Calendar
        private DateTime _displayedCalMonth = DateTime.Today;
        private DateTime _lastRenderedToday = DateTime.MinValue;

        // Typing Language Dropdown
        private bool _isUpdatingTypingLangDropdown = false;

        public MainWindow()
        {
            InitializeComponent();

            _audioService = new AudioService();
            _inputService = new InputService(_audioService);
            _languageService = new LanguageService();
            _audioPlayer = new AudioPlayerService();

            _clockTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _clockTimer.Tick += (s, e) => UpdateClockDisplay();
            _clockTimer.Start();

            _inputService.ModifiersChanged += OnModifiersChanged;
            _languageService.LanguageChanged += OnSystemLanguageChanged;
            _languageService.ActiveApplicationChanged += OnActiveApplicationChanged;
            LocalizationService.Instance.LanguageChanged += OnUiLanguageChanged;
            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
            SettingsService.Instance.ProfileApplied += OnSettingsProfileApplied;

            Loaded += OnWindowLoaded;
        }

        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            _screens = DisplayManager.GetScreens();

            // Look for external (non-primary) screen first
            for (int i = 0; i < _screens.Count; i++)
            {
                if (!_screens[i].IsPrimary)
                {
                    _currentScreenIndex = i;
                    break;
                }
            }

            IcMonitors.ItemsSource = _screensList;
            RefreshMonitorsList();

            IcProfiles.ItemsSource = SettingsService.Instance.Profiles;

            InitAudioPlayerUi();
            InitFileTree();
            TvComputerFiles.AddHandler(TreeViewItem.ExpandedEvent, new RoutedEventHandler(OnTreeViewItemExpanded));
            InitializeCalendar();
            PopulateTypingLangDropdown();

            // Load and restore saved active settings from previous run
            var savedSettings = SettingsService.Instance.ActiveSettings;
            if (savedSettings != null)
            {
                ApplySettings(savedSettings);
            }
            else
            {
                BtnKeyHeight.Content = $"↕ {(int)_rowHeight}px";
                RebuildAllGrids();
                UpdateLanguageBadges(SupportedLanguage.Turkish);
                ApplyCurrentTheme();
                ApplyLocalization();
            }

            // Set dynamic cyber deck taskbar icon
            try
            {
                Icon = CreateCyberDeckIcon();
            }
            catch { }

            StateChanged += OnWindowStateChanged;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            _hwnd = helper.Handle;

            // Set WS_EX_NOACTIVATE | WS_EX_TOPMOST | WS_EX_APPWINDOW (strip WS_EX_TOOLWINDOW so it appears on the taskbar)
            IntPtr exStyle = NativeMethods.GetWindowLong(_hwnd, NativeMethods.GWL_EXSTYLE);
            long newExStyleLong = (exStyle.ToInt64() & ~NativeMethods.WS_EX_TOOLWINDOW)
                                | NativeMethods.WS_EX_NOACTIVATE
                                | NativeMethods.WS_EX_TOPMOST
                                | NativeMethods.WS_EX_APPWINDOW;
            NativeMethods.SetWindowLong(_hwnd, NativeMethods.GWL_EXSTYLE, (IntPtr)newExStyleLong);

            try
            {
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CyberKeyboard", "app_debug.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] OnSourceInitialized: HWND=0x{_hwnd.ToInt64():X} ExStyle=0x{newExStyleLong:X8} WS_EX_APPWINDOW={(newExStyleLong & NativeMethods.WS_EX_APPWINDOW) != 0} WS_EX_TOOLWINDOW={(newExStyleLong & NativeMethods.WS_EX_TOOLWINDOW) != 0}\n");
            }
            catch { }

            // Notify Windows shell to register window on taskbar immediately
            NativeMethods.SetWindowPos(_hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_FRAMECHANGED);

            // Hook window messages to intercept WM_MOUSEACTIVATE
            var source = HwndSource.FromHwnd(_hwnd);
            source?.AddHook(WndProc);

            _languageService.SetOwnHandle(_hwnd);
            _languageService.Start();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == NativeMethods.WM_MOUSEACTIVATE)
            {
                if (ProfileManagerPopup != null && ProfileManagerPopup.Visibility == Visibility.Visible)
                {
                    handled = false;
                    return IntPtr.Zero;
                }

                handled = true;
                return (IntPtr)NativeMethods.MA_NOACTIVATE;
            }
            return IntPtr.Zero;
        }

        private void EnableWindowActivation(bool enable)
        {
            if (_hwnd == IntPtr.Zero) return;
            try
            {
                IntPtr exStyle = NativeMethods.GetWindowLong(_hwnd, NativeMethods.GWL_EXSTYLE);
                long current = exStyle.ToInt64();
                long updated = enable
                    ? (current & ~NativeMethods.WS_EX_NOACTIVATE)
                    : (current | NativeMethods.WS_EX_NOACTIVATE);

                if (updated != current)
                {
                    NativeMethods.SetWindowLong(_hwnd, NativeMethods.GWL_EXSTYLE, (IntPtr)updated);
                    NativeMethods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0,
                        NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOZORDER | NativeMethods.SWP_FRAMECHANGED);
                }
            }
            catch { }
        }

        #region Audio Player & Media Deck Initialization

        private void InitAudioPlayerUi()
        {
            LstPlaylist.ItemsSource = _audioPlayer.Playlist;
            TvComputerFiles.ItemsSource = _fileTreeRoots;

            _audioPlayer.PropertyChanged += (s, e) =>
            {
                Dispatcher.Invoke(() =>
                {
                    TxtTrackTitle.Text = _audioPlayer.CurrentTrackTitle.ToUpperInvariant();
                    BtnPlayPause.Content = _audioPlayer.IsPlaying ? "⏸" : "▶";
                    AudioVisualizer.IsActive = _audioPlayer.IsPlaying;
                    TxtCurrentTime.Text = _audioPlayer.PositionString;
                    TxtTotalTime.Text = _audioPlayer.DurationString;

                    if (!_isUserSeeking)
                    {
                        SeekSlider.Value = _audioPlayer.ProgressRatio * 100.0;
                    }

                    TxtTabPlaylist.Text = $"📑 LİSTE ({_audioPlayer.Playlist.Count})";
                    TxtEmptyPlaylistCue.Visibility = _audioPlayer.Playlist.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
                    BtnShuffle.Foreground = _audioPlayer.IsShuffle ? ThemeManager.Instance.AccentBrush : Brushes.Gray;
                    BtnRepeat.Foreground = _audioPlayer.IsRepeat ? ThemeManager.Instance.AccentBrush : Brushes.Gray;
                });
            };
        }

        private void InitFileTree()
        {
            _fileTreeRoots.Clear();

            try
            {
                var loc = LocalizationService.Instance;
                // 1. Quick User Folders (Music, Downloads, Desktop)
                AddSpecialFolderQuickRoot(Environment.SpecialFolder.MyMusic, loc.Get("MyMusic"));

                string downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
                if (Directory.Exists(downloadsPath))
                {
                    var dlItem = new FileTreeItem
                    {
                        Name = loc.Get("Downloads"),
                        FullPath = downloadsPath,
                        IsDirectory = true,
                        Icon = "📥"
                    };
                    dlItem.AddDummyChild();
                    _fileTreeRoots.Add(dlItem);
                }

                AddSpecialFolderQuickRoot(Environment.SpecialFolder.Desktop, loc.Get("Desktop"));

                // 2. Drives (C:\, D:\, etc.)
                foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
                {
                    string label = string.IsNullOrEmpty(drive.VolumeLabel) ? loc.Get("LocalDisk") : drive.VolumeLabel;
                    var driveItem = new FileTreeItem
                    {
                        Name = $"💽 {drive.Name} ({label})",
                        FullPath = drive.RootDirectory.FullName,
                        IsDirectory = true,
                        Icon = "💽"
                    };
                    driveItem.AddDummyChild();
                    _fileTreeRoots.Add(driveItem);
                }
            }
            catch
            {
                // Fallback
            }
        }

        private void AddSpecialFolderQuickRoot(Environment.SpecialFolder specialFolder, string displayName)
        {
            try
            {
                string path = Environment.GetFolderPath(specialFolder);
                if (Directory.Exists(path))
                {
                    var item = new FileTreeItem
                    {
                        Name = displayName,
                        FullPath = path,
                        IsDirectory = true,
                        Icon = displayName.Length >= 2 ? displayName.Substring(0, 2) : "📁"
                    };
                    item.AddDummyChild();
                    _fileTreeRoots.Add(item);
                }
            }
            catch { }
        }

        private void OnTreeViewItemExpanded(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeViewItem tvi && tvi.DataContext is FileTreeItem item)
            {
                item.LoadChildren();
            }
        }

        #endregion

        #region Audio Player Controls & Drag and Drop

        private void OnAudioDeckDragOver(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
                e.Handled = true;
            }
        }

        private void OnAudioDeckDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    _audioPlayer.AddFiles(files, autoPlayIfEmpty: true);
                    ShowTabPlaylist();
                }
                e.Handled = true;
            }
        }

        private void OnPlayerPlayPauseClick(object sender, RoutedEventArgs e)
        {
            _audioPlayer.TogglePlayPause();
        }

        private void OnPlayerStopClick(object sender, RoutedEventArgs e)
        {
            _audioPlayer.Stop();
        }

        private void OnPlayerNextClick(object sender, RoutedEventArgs e)
        {
            _audioPlayer.Next();
        }

        private void OnPlayerPrevClick(object sender, RoutedEventArgs e)
        {
            _audioPlayer.Previous();
        }

        private void OnPlayerShuffleClick(object sender, RoutedEventArgs e)
        {
            _audioPlayer.IsShuffle = !_audioPlayer.IsShuffle;
            BtnShuffle.Foreground = _audioPlayer.IsShuffle ? ThemeManager.Instance.AccentBrush : Brushes.Gray;
        }

        private void OnPlayerRepeatClick(object sender, RoutedEventArgs e)
        {
            _audioPlayer.IsRepeat = !_audioPlayer.IsRepeat;
            BtnRepeat.Foreground = _audioPlayer.IsRepeat ? ThemeManager.Instance.AccentBrush : Brushes.Gray;
        }

        private void OnVolSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_audioPlayer != null)
            {
                _audioPlayer.Volume = VolSlider.Value;
            }
        }

        private void OnSeekSliderMouseDown(object sender, MouseButtonEventArgs e) => _isUserSeeking = true;
        private void OnSeekSliderTouchDown(object sender, TouchEventArgs e) => _isUserSeeking = true;

        private void OnSeekSliderMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isUserSeeking = false;
            _audioPlayer.Seek(SeekSlider.Value / 100.0);
        }

        private void OnSeekSliderTouchUp(object sender, TouchEventArgs e)
        {
            _isUserSeeking = false;
            _audioPlayer.Seek(SeekSlider.Value / 100.0);
        }

        private void OnSeekSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUserSeeking)
            {
                _audioPlayer.Seek(SeekSlider.Value / 100.0);
            }
        }

        private void OnPlaylistItemPlayClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TrackItem track)
            {
                if (track == _audioPlayer.CurrentTrack)
                {
                    _audioPlayer.TogglePlayPause();
                }
                else
                {
                    _audioPlayer.PlayTrack(track);
                }
            }
        }

        private void OnPlaylistItemRemoveClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is TrackItem track)
            {
                _audioPlayer.RemoveTrack(track);
            }
        }

        private void OnPlaylistTrackDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (LstPlaylist.SelectedItem is TrackItem track)
            {
                _audioPlayer.PlayTrack(track);
            }
        }

        private void OnTreeSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (e.NewValue is FileTreeItem item)
            {
                BtnAddSelectedTreeFolder.Visibility = item.IsDirectory ? Visibility.Visible : Visibility.Collapsed;
                BtnPlaySelectedTreeItem.Visibility = item.IsAudioFile ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                BtnAddSelectedTreeFolder.Visibility = Visibility.Collapsed;
                BtnPlaySelectedTreeItem.Visibility = Visibility.Collapsed;
            }
        }

        private void OnPlaySelectedTreeItemClick(object sender, RoutedEventArgs e)
        {
            if (TvComputerFiles.SelectedItem is FileTreeItem item && item.IsAudioFile)
            {
                PlayFileTreeAudio(item);
            }
        }

        private void OnTreeItemDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (TvComputerFiles.SelectedItem is FileTreeItem item)
            {
                if (item.IsAudioFile)
                {
                    PlayFileTreeAudio(item);
                }
                else if (item.IsDirectory)
                {
                    item.IsExpanded = !item.IsExpanded;
                }
            }
        }

        private void PlayFileTreeAudio(FileTreeItem item)
        {
            var existing = _audioPlayer.Playlist.FirstOrDefault(t => string.Equals(t.FilePath, item.FullPath, StringComparison.OrdinalIgnoreCase));
            if (existing != null)
            {
                _audioPlayer.PlayTrack(existing);
            }
            else
            {
                var track = new TrackItem
                {
                    Title = Path.GetFileNameWithoutExtension(item.FullPath),
                    FilePath = item.FullPath,
                    Index = _audioPlayer.Playlist.Count + 1
                };
                _audioPlayer.Playlist.Add(track);
                _audioPlayer.PlayTrack(track);
            }
            ShowTabPlaylist();
        }

        private void OnTreeFolderAddClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FileTreeItem item && item.IsDirectory)
            {
                _audioService.PlayKeySound(true);
                _audioPlayer.AddFolder(item.FullPath, autoPlayIfEmpty: true);
                ShowTabPlaylist();
            }
        }

        private void OnTreeAudioPlayClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FileTreeItem item && item.IsAudioFile)
            {
                _audioService.PlayKeySound(true);
                PlayFileTreeAudio(item);
            }
        }

        private void OnTreeAudioAddClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is FileTreeItem item && item.IsAudioFile)
            {
                _audioService.PlayKeySound(true);
                var existing = _audioPlayer.Playlist.FirstOrDefault(t => string.Equals(t.FilePath, item.FullPath, StringComparison.OrdinalIgnoreCase));
                if (existing == null)
                {
                    var track = new TrackItem
                    {
                        Title = Path.GetFileNameWithoutExtension(item.FullPath),
                        FilePath = item.FullPath,
                        Index = _audioPlayer.Playlist.Count + 1
                    };
                    _audioPlayer.Playlist.Add(track);
                }
                ShowTabPlaylist();
            }
        }

        private void OnTreeItemTextClick(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is FileTreeItem item)
            {
                if (item.IsDirectory)
                {
                    item.IsExpanded = !item.IsExpanded;
                }
                else if (item.IsAudioFile)
                {
                    _audioService.PlayKeySound(true);
                    PlayFileTreeAudio(item);
                }
            }
        }

        private void OnTreeItemTextTouch(object sender, TouchEventArgs e)
        {
            if (sender is FrameworkElement fe && fe.DataContext is FileTreeItem item)
            {
                if (item.IsDirectory)
                {
                    item.IsExpanded = !item.IsExpanded;
                }
                else if (item.IsAudioFile)
                {
                    _audioService.PlayKeySound(true);
                    PlayFileTreeAudio(item);
                }
            }
        }

        private void OnAddAudioFilesClick(object sender, RoutedEventArgs e)
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Multiselect = true,
                Filter = "Ses Dosyaları (*.mp3;*.wav;*.flac;*.m4a;*.aac;*.wma;*.ogg)|*.mp3;*.wav;*.flac;*.m4a;*.aac;*.wma;*.ogg|Tüm Dosyalar (*.*)|*.*",
                Title = "Ses Dosyaları Seçin"
            };

            if (ofd.ShowDialog() == true)
            {
                _audioPlayer.AddFiles(ofd.FileNames, autoPlayIfEmpty: true);
                ShowTabPlaylist();
            }
        }

        private void OnAddAudioFolderClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFolderDialog
                {
                    Title = "Ses Dosyaları İçeren Klasör Seçin"
                };

                if (dialog.ShowDialog() == true)
                {
                    _audioPlayer.AddFolder(dialog.FolderName, autoPlayIfEmpty: true);
                    ShowTabPlaylist();
                }
            }
            catch
            {
                // Fallback if dialog unavailable
            }
        }

        private void OnClearPlaylistClick(object sender, RoutedEventArgs e)
        {
            _audioPlayer.ClearPlaylist();
        }

        private void OnAddSelectedTreeFolderClick(object sender, RoutedEventArgs e)
        {
            if (TvComputerFiles.SelectedItem is FileTreeItem item && item.IsDirectory)
            {
                _audioPlayer.AddFolder(item.FullPath, autoPlayIfEmpty: true);
                ShowTabPlaylist();
            }
        }

        private void OnTabPlaylistClick(object sender, MouseButtonEventArgs e) => ShowTabPlaylist();
        private void OnTabPlaylistTouch(object sender, TouchEventArgs e) => ShowTabPlaylist();

        private void OnTabFileTreeClick(object sender, MouseButtonEventArgs e) => ShowTabFileTree();
        private void OnTabFileTreeTouch(object sender, TouchEventArgs e) => ShowTabFileTree();

        private void ShowTabPlaylist()
        {
            TabPlaylistContent.Visibility = Visibility.Visible;
            TabFileTreeContent.Visibility = Visibility.Collapsed;
            BtnAddSelectedTreeFolder.Visibility = Visibility.Collapsed;
            BtnPlaySelectedTreeItem.Visibility = Visibility.Collapsed;

            var theme = ThemeManager.Instance;
            TabPlaylistHeader.Background = new SolidColorBrush(Color.FromArgb(50, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
            TabPlaylistHeader.BorderBrush = theme.AccentBrush;
            TabPlaylistHeader.BorderThickness = new Thickness(1.0);
            TxtTabPlaylist.Foreground = Brushes.White;

            TabFileTreeHeader.Background = new SolidColorBrush(Color.FromArgb(20, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
            TabFileTreeHeader.BorderBrush = new SolidColorBrush(Color.FromArgb(40, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
            TabFileTreeHeader.BorderThickness = new Thickness(1.0);
            TxtTabFileTree.Foreground = new SolidColorBrush(Color.FromRgb(160, 200, 235));
        }

        private void ShowTabFileTree()
        {
            TabPlaylistContent.Visibility = Visibility.Collapsed;
            TabFileTreeContent.Visibility = Visibility.Visible;

            if (TvComputerFiles.SelectedItem is FileTreeItem item)
            {
                BtnAddSelectedTreeFolder.Visibility = item.IsDirectory ? Visibility.Visible : Visibility.Collapsed;
                BtnPlaySelectedTreeItem.Visibility = item.IsAudioFile ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                BtnAddSelectedTreeFolder.Visibility = Visibility.Collapsed;
                BtnPlaySelectedTreeItem.Visibility = Visibility.Collapsed;
            }

            var theme = ThemeManager.Instance;
            TabFileTreeHeader.Background = new SolidColorBrush(Color.FromArgb(50, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
            TabFileTreeHeader.BorderBrush = theme.AccentBrush;
            TabFileTreeHeader.BorderThickness = new Thickness(1.0);
            TxtTabFileTree.Foreground = Brushes.White;

            TabPlaylistHeader.Background = new SolidColorBrush(Color.FromArgb(20, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
            TabPlaylistHeader.BorderBrush = new SolidColorBrush(Color.FromArgb(40, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
            TabPlaylistHeader.BorderThickness = new Thickness(1.0);
            TxtTabPlaylist.Foreground = new SolidColorBrush(Color.FromRgb(160, 200, 235));
        }

        #region Digital Chrono & Calendar HUD

        private void InitializeCalendar()
        {
            _displayedCalMonth = DateTime.Today;
            _lastRenderedToday = DateTime.Today;
            RenderCalendar(_displayedCalMonth);
        }

        private void OnCalPrevMonthClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            _displayedCalMonth = _displayedCalMonth.AddMonths(-1);
            RenderCalendar(_displayedCalMonth);
        }

        private void OnCalNextMonthClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            _displayedCalMonth = _displayedCalMonth.AddMonths(1);
            RenderCalendar(_displayedCalMonth);
        }

        private void OnCalMonthTitleClick(object sender, MouseButtonEventArgs e)
        {
            _audioService.PlayKeySound(true);
            _displayedCalMonth = DateTime.Today;
            _lastRenderedToday = DateTime.Today;
            RenderCalendar(_displayedCalMonth);
        }

        private void RenderCalendar(DateTime targetMonth)
        {
            if (CalWeekdayGrid == null || CalDaysGrid == null || TxtCalMonthYear == null) return;

            var loc = LocalizationService.Instance;
            var culture = loc.CurrentLanguage switch
            {
                UiLanguage.Turkish => new System.Globalization.CultureInfo("tr-TR"),
                UiLanguage.English => new System.Globalization.CultureInfo("en-US"),
                UiLanguage.French => new System.Globalization.CultureInfo("fr-FR"),
                UiLanguage.German => new System.Globalization.CultureInfo("de-DE"),
                _ => new System.Globalization.CultureInfo("tr-TR")
            };

            // Month & Year Title
            TxtCalMonthYear.Text = targetMonth.ToString("MMMM yyyy", culture).ToUpper(culture);
            TxtCalMonthYear.ToolTip = $"{loc.Get("CalendarToday")} ({DateTime.Today.ToString("d MMMM yyyy", culture)})";
            if (BtnCalPrevMonth != null) BtnCalPrevMonth.ToolTip = loc.Get("CalendarPrevMonth");
            if (BtnCalNextMonth != null) BtnCalNextMonth.ToolTip = loc.Get("CalendarNextMonth");

            // Render Weekdays (7 columns)
            CalWeekdayGrid.Children.Clear();
            var firstDayOfWeek = culture.DateTimeFormat.FirstDayOfWeek;
            var theme = ThemeManager.Instance;
            for (int i = 0; i < 7; i++)
            {
                var dow = (DayOfWeek)(((int)firstDayOfWeek + i) % 7);
                string rawAbbr = culture.DateTimeFormat.GetAbbreviatedDayName(dow);
                string shortAbbr = rawAbbr.Length > 2 ? rawAbbr.Substring(0, 2) : rawAbbr;
                bool isWeekend = (dow == DayOfWeek.Saturday || dow == DayOfWeek.Sunday);

                var tb = new TextBlock
                {
                    Text = shortAbbr.ToUpper(culture),
                    FontSize = 9.5,
                    FontWeight = isWeekend ? FontWeights.Bold : FontWeights.SemiBold,
                    FontFamily = new FontFamily("Consolas, Segoe UI"),
                    Foreground = isWeekend
                        ? (theme.Is80sTheme || theme.IsGlitchTheme ? theme.SecondaryBrush : new SolidColorBrush(Color.FromRgb(255, 115, 160)))
                        : new SolidColorBrush(Color.FromRgb(115, 175, 215)),
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                CalWeekdayGrid.Children.Add(tb);
            }

            // Render Days Grid (7 columns)
            CalDaysGrid.Children.Clear();
            var firstOfMonth = new DateTime(targetMonth.Year, targetMonth.Month, 1);
            int daysInMonth = DateTime.DaysInMonth(targetMonth.Year, targetMonth.Month);
            int offset = ((int)firstOfMonth.DayOfWeek - (int)firstDayOfWeek + 7) % 7;

            // Previous month trailing days
            var prevMonth = targetMonth.AddMonths(-1);
            int daysInPrevMonth = DateTime.DaysInMonth(prevMonth.Year, prevMonth.Month);
            for (int i = offset - 1; i >= 0; i--)
            {
                int dayNum = daysInPrevMonth - i;
                var cellDate = new DateTime(prevMonth.Year, prevMonth.Month, dayNum);
                bool isWeekend = (cellDate.DayOfWeek == DayOfWeek.Saturday || cellDate.DayOfWeek == DayOfWeek.Sunday);
                CalDaysGrid.Children.Add(CreateDayCell(dayNum, isCurrentMonth: false, isToday: false, isWeekend: isWeekend));
            }

            // Current month days
            var today = DateTime.Today;
            for (int d = 1; d <= daysInMonth; d++)
            {
                bool isToday = (today.Year == targetMonth.Year && today.Month == targetMonth.Month && today.Day == d);
                var cellDate = new DateTime(targetMonth.Year, targetMonth.Month, d);
                bool isWeekend = (cellDate.DayOfWeek == DayOfWeek.Saturday || cellDate.DayOfWeek == DayOfWeek.Sunday);
                CalDaysGrid.Children.Add(CreateDayCell(d, isCurrentMonth: true, isToday: isToday, isWeekend: isWeekend));
            }

            // Next month leading days to complete grid (multiples of 7)
            var nextMonth = targetMonth.AddMonths(1);
            int totalCells = CalDaysGrid.Children.Count;
            int remainder = (7 - (totalCells % 7)) % 7;
            for (int n = 1; n <= remainder; n++)
            {
                var cellDate = new DateTime(nextMonth.Year, nextMonth.Month, n);
                bool isWeekend = (cellDate.DayOfWeek == DayOfWeek.Saturday || cellDate.DayOfWeek == DayOfWeek.Sunday);
                CalDaysGrid.Children.Add(CreateDayCell(n, isCurrentMonth: false, isToday: false, isWeekend: isWeekend));
            }
        }

        private UIElement CreateDayCell(int dayNumber, bool isCurrentMonth, bool isToday, bool isWeekend)
        {
            var theme = ThemeManager.Instance;

            var border = new Border
            {
                CornerRadius = new CornerRadius(3),
                Height = 17,
                Margin = new Thickness(0.5),
                Focusable = false
            };

            var tb = new TextBlock
            {
                Text = dayNumber.ToString(),
                FontSize = 10.5,
                FontFamily = new FontFamily("Consolas, Segoe UI"),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            if (isToday)
            {
                // Hafifçe belirt: subtle accent border, glowing background fill, white bold text
                border.BorderBrush = theme.AccentBrush;
                border.BorderThickness = new Thickness(1);
                border.Background = new SolidColorBrush(Color.FromArgb(55, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
                tb.FontWeight = FontWeights.Bold;
                tb.Foreground = Brushes.White;

                if (theme.Is80sTheme || theme.IsGlitchTheme)
                {
                    border.BorderBrush = theme.SecondaryBrush;
                    tb.Foreground = theme.AccentBrush;
                }
            }
            else if (isCurrentMonth)
            {
                border.Background = Brushes.Transparent;
                border.BorderThickness = new Thickness(0);
                if (isWeekend)
                {
                    tb.FontWeight = FontWeights.Bold;
                    // Hafta sonları özel canlı neon pembe/mercan renk
                    tb.Foreground = (theme.Is80sTheme || theme.IsGlitchTheme)
                        ? theme.SecondaryBrush
                        : new SolidColorBrush(Color.FromRgb(255, 115, 160));
                }
                else
                {
                    tb.FontWeight = FontWeights.Normal;
                    tb.Foreground = new SolidColorBrush(Color.FromRgb(220, 240, 255));
                }
            }
            else
            {
                border.Background = Brushes.Transparent;
                border.BorderThickness = new Thickness(0);
                tb.FontWeight = FontWeights.Normal;
                if (isWeekend)
                {
                    tb.Foreground = new SolidColorBrush(Color.FromArgb(80, 255, 120, 165));
                }
                else
                {
                    tb.Foreground = new SolidColorBrush(Color.FromArgb(70, 110, 150, 185));
                }
            }

            border.Child = tb;
            return border;
        }

        #region Clock Sizing

        private void PopulateClockSizeDropdown()
        {
            if (CmbClockSize == null) return;

            _isUpdatingClockSizeDropdown = true;
            try
            {
                var loc = LocalizationService.Instance;
                CmbClockSize.Items.Clear();

                var sizes = new (string Key, string DisplayText)[]
                {
                    ("Small", loc.Get("ClockSizeSmall")),
                    ("Medium", loc.Get("ClockSizeMedium")),
                    ("Large", loc.Get("ClockSizeLarge")),
                    ("ExtraLarge", loc.Get("ClockSizeExtraLarge"))
                };

                int selectedIdx = 1; // Medium by default
                for (int i = 0; i < sizes.Length; i++)
                {
                    var item = new ComboBoxItem
                    {
                        Content = sizes[i].DisplayText,
                        Tag = sizes[i].Key,
                        Focusable = false
                    };
                    CmbClockSize.Items.Add(item);

                    if (string.Equals(sizes[i].Key, _currentClockSize, StringComparison.OrdinalIgnoreCase))
                    {
                        selectedIdx = i;
                    }
                }

                CmbClockSize.SelectedIndex = selectedIdx;
            }
            finally
            {
                _isUpdatingClockSizeDropdown = false;
            }
        }

        private void OnClockSizeChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingClockSizeDropdown) return;
            if (CmbClockSize?.SelectedItem is ComboBoxItem item && item.Tag is string sizeKey)
            {
                _audioService.PlayKeySound(true);
                _currentClockSize = sizeKey;
                ApplyClockSize(_currentClockSize);
                SaveCurrentSettingsSilently();
            }
        }

        private void ApplyClockSize(string size)
        {
            if (TxtClockHoursMinutes == null || TxtClockSeconds == null) return;

            // Gün ismi ve tarih bilgisi kullanıcı isteği doğrultusunda sabit boyutta kalır
            if (TxtClockDayOfWeek != null) TxtClockDayOfWeek.FontSize = 11;
            if (TxtClockFullDate != null) TxtClockFullDate.FontSize = 11.5;
            if (ClockDayBadge != null) ClockDayBadge.Padding = new Thickness(6, 2, 6, 2);

            switch (size?.ToLowerInvariant())
            {
                case "small":
                    TxtClockHoursMinutes.FontSize = 75;
                    TxtClockSeconds.FontSize = 34;
                    TxtClockAmPm.FontSize = 15;
                    break;

                case "large":
                    TxtClockHoursMinutes.FontSize = 115;
                    TxtClockSeconds.FontSize = 50;
                    TxtClockAmPm.FontSize = 21;
                    break;

                case "extralarge":
                    TxtClockHoursMinutes.FontSize = 135;
                    TxtClockSeconds.FontSize = 58;
                    TxtClockAmPm.FontSize = 24;
                    break;

                case "medium":
                default:
                    TxtClockHoursMinutes.FontSize = 95;
                    TxtClockSeconds.FontSize = 42;
                    TxtClockAmPm.FontSize = 18;
                    break;
            }
        }

        #endregion

        private void UpdateClockDisplay()
        {
            var now = DateTime.Now;
            _clockColonVisible = !_clockColonVisible;

            string colon = _clockColonVisible ? ":" : " ";
            TxtClockHoursMinutes.Text = $"{now.Hour:D2}{colon}{now.Minute:D2}";
            TxtClockSeconds.Text = $":{now.Second:D2}";

            var loc = LocalizationService.Instance;
            var culture = loc.CurrentLanguage switch
            {
                UiLanguage.Turkish => new System.Globalization.CultureInfo("tr-TR"),
                UiLanguage.English => new System.Globalization.CultureInfo("en-US"),
                UiLanguage.French => new System.Globalization.CultureInfo("fr-FR"),
                UiLanguage.German => new System.Globalization.CultureInfo("de-DE"),
                _ => new System.Globalization.CultureInfo("tr-TR")
            };

            // Day in active UI language
            TxtClockDayOfWeek.Text = now.ToString("dddd", culture).ToUpper(culture);

            // Full Date
            TxtClockFullDate.Text = now.ToString("d MMMM yyyy", culture).ToUpper(culture);

            // Timezone
            var tz = TimeZoneInfo.Local;
            var offset = tz.GetUtcOffset(now);
            string offsetStr = $"UTC{(offset >= TimeSpan.Zero ? "+" : "-")}{Math.Abs((int)offset.TotalHours)}";
            TxtClockTimezone.Text = $"{offsetStr} // {loc.Get("LocalTime")}";

            // Week of Year & Day of Year
            var cal = culture.Calendar;
            int weekNum = cal.GetWeekOfYear(now, System.Globalization.CalendarWeekRule.FirstFourDayWeek, DayOfWeek.Monday);
            TxtClockWeek.Text = $"{loc.Get("Week")} {weekNum}";
            TxtClockDayOfYear.Text = $"{loc.Get("Day")} {now.DayOfYear}/{(DateTime.IsLeapYear(now.Year) ? 366 : 365)}";

            // Live Session Uptime
            var uptime = DateTime.Now - _sessionStartTime;
            TxtClockUptime.Text = $"{loc.Get("Uptime")} {(int)uptime.TotalHours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";

            // Date rollover check to update calendar today highlight at midnight
            if (DateTime.Today != _lastRenderedToday)
            {
                _lastRenderedToday = DateTime.Today;
                RenderCalendar(_displayedCalMonth);
            }

            // 80s Theme Specific Blinking HUD Indicators (Yanıp sönen terminal imleci & carrier beat)
            var theme = ThemeManager.Instance;
            if (theme.Is80sTheme)
            {
                TxtBrandCursor.Visibility = _clockColonVisible ? Visibility.Visible : Visibility.Hidden;
                CarrierLedDot.Fill = _clockColonVisible ? theme.SecondaryBrush : theme.AccentBrush;
                ClockPulseDot.Fill = _clockColonVisible ? theme.AccentBrush : theme.SecondaryBrush;
            }
            else
            {
                TxtBrandCursor.Visibility = Visibility.Collapsed;
                BadgeCarrierBpm.Visibility = Visibility.Collapsed;
                ClockPulseDot.Fill = new SolidColorBrush(Color.FromRgb(0, 255, 136));
            }
        }

        #endregion

        private void OnTogglePlayerClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            _isPlayerVisible = !_isPlayerVisible;
            AudioPlayerDeck.Visibility = _isPlayerVisible ? Visibility.Visible : Visibility.Collapsed;
            BtnPlayerToggle.Foreground = _isPlayerVisible ? ThemeManager.Instance.AccentBrush : Brushes.Gray;

            if (!_isFullscreen)
            {
                Height = _isPlayerVisible
                    ? _rowHeight * 5 + FunctionRowGrid.Height + 340
                    : _rowHeight * 5 + FunctionRowGrid.Height + 130;
            }
            SaveCurrentSettingsSilently();
        }

        #endregion

        #region Keyboard Layout Builders

        private void RebuildAllGrids()
        {
            _allKeyControls.Clear();
            _allKeyModels.Clear();

            FunctionRowGrid.Height = _rowHeight * 0.58;

            BuildFunctionRow();
            BuildNavigationCluster();
            BuildNumpad();
            SwitchLayout(_languageService.CurrentLanguage);
        }

        private void BuildFunctionRow()
        {
            FunctionRowGrid.Children.Clear();
            FunctionRowGrid.ColumnDefinitions.Clear();

            var funcKeys = KeyboardLayouts.CreateFunctionRow();
            for (int i = 0; i < funcKeys.Count; i++)
            {
                FunctionRowGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(funcKeys[i].WidthRatio, GridUnitType.Star)
                });

                var ctrl = CreateKeyControl(funcKeys[i]);
                Grid.SetColumn(ctrl, i);
                FunctionRowGrid.Children.Add(ctrl);
            }
        }

        private void BuildNavigationCluster()
        {
            NavClusterGrid.Children.Clear();
            NavClusterGrid.RowDefinitions.Clear();

            var rows = KeyboardLayouts.CreateNavigationCluster();
            for (int r = 0; r < rows.Count; r++)
            {
                NavClusterGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_rowHeight, GridUnitType.Pixel) });

                var rowGrid = new Grid();
                for (int c = 0; c < rows[r].Keys.Count; c++)
                {
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(rows[r].Keys[c].WidthRatio, GridUnitType.Star)
                    });

                    var key = rows[r].Keys[c];
                    if (!string.IsNullOrEmpty(key.NormalText))
                    {
                        var ctrl = CreateKeyControl(key);
                        Grid.SetColumn(ctrl, c);
                        rowGrid.Children.Add(ctrl);
                    }
                }

                Grid.SetRow(rowGrid, r);
                NavClusterGrid.Children.Add(rowGrid);
            }
        }

        private void BuildNumpad()
        {
            NumpadGrid.Children.Clear();
            NumpadGrid.RowDefinitions.Clear();

            var rows = KeyboardLayouts.CreateNumpad();
            for (int r = 0; r < rows.Count; r++)
            {
                NumpadGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_rowHeight, GridUnitType.Pixel) });

                var rowGrid = new Grid();
                for (int c = 0; c < rows[r].Keys.Count; c++)
                {
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(rows[r].Keys[c].WidthRatio, GridUnitType.Star)
                    });

                    var ctrl = CreateKeyControl(rows[r].Keys[c]);
                    Grid.SetColumn(ctrl, c);
                    rowGrid.Children.Add(ctrl);
                }

                Grid.SetRow(rowGrid, r);
                NumpadGrid.Children.Add(rowGrid);
            }
        }

        private void SwitchLayout(SupportedLanguage lang)
        {
            MainDeckGrid.Children.Clear();
            MainDeckGrid.RowDefinitions.Clear();

            _allKeyControls.RemoveAll(k => k.Parent == MainDeckGrid || (k.Parent is Grid p && p.Parent == MainDeckGrid));

            var rows = KeyboardLayouts.GetLayout(lang);

            for (int r = 0; r < rows.Count; r++)
            {
                MainDeckGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(_rowHeight, GridUnitType.Pixel) });

                var rowGrid = new Grid();
                for (int c = 0; c < rows[r].Keys.Count; c++)
                {
                    rowGrid.ColumnDefinitions.Add(new ColumnDefinition
                    {
                        Width = new GridLength(rows[r].Keys[c].WidthRatio, GridUnitType.Star)
                    });

                    var ctrl = CreateKeyControl(rows[r].Keys[c]);
                    Grid.SetColumn(ctrl, c);
                    rowGrid.Children.Add(ctrl);
                }

                Grid.SetRow(rowGrid, r);
                MainDeckGrid.Children.Add(rowGrid);
            }

            RefreshAllKeys();
        }

        private CyberKeyControl CreateKeyControl(KeyModel model)
        {
            _allKeyModels.Add(model);

            var ctrl = new CyberKeyControl
            {
                KeyModel = model
            };

            ctrl.KeyTriggered = (key, _) =>
            {
                HandleKeyInput(key);
            };

            _allKeyControls.Add(ctrl);
            return ctrl;
        }

        #endregion

        #region Key Input Processing

        private void HandleKeyInput(KeyModel key)
        {
            _totalKeysPressed++;

            if (ProfileManagerPopup != null && ProfileManagerPopup.Visibility == Visibility.Visible)
            {
                if (HandleVirtualKeyForProfileInput(key))
                {
                    return;
                }
            }

            if (!string.IsNullOrEmpty(key.SpecialAction))
            {
                switch (key.SpecialAction.ToLowerInvariant())
                {
                    case "shift":
                        _inputService.ToggleShift();
                        break;
                    case "caps":
                        _inputService.ToggleCapsLock();
                        break;
                    case "altgr":
                        _inputService.ToggleAltGr();
                        break;
                    case "ctrl":
                        _inputService.ToggleCtrl();
                        break;
                    case "alt":
                        _inputService.ToggleAlt();
                        break;
                    case "win":
                        _inputService.ToggleWin();
                        break;
                    case "backspace":
                        _inputService.SendVirtualKey(NativeMethods.VK_BACK);
                        if (_liveBuffer.Length > 0) _liveBuffer.Remove(_liveBuffer.Length - 1, 1);
                        UpdateLiveStream();
                        break;
                    case "enter":
                        _inputService.SendVirtualKey(NativeMethods.VK_RETURN);
                        _liveBuffer.Append(' ');
                        UpdateLiveStream();
                        break;
                    case "tab":
                        _inputService.SendVirtualKey(NativeMethods.VK_TAB);
                        _liveBuffer.Append("    ");
                        UpdateLiveStream();
                        break;
                    case "space":
                        _inputService.SendVirtualKey(NativeMethods.VK_SPACE);
                        _liveBuffer.Append(' ');
                        UpdateLiveStream();
                        break;
                    case "esc":
                        _inputService.SendVirtualKey(NativeMethods.VK_ESCAPE);
                        break;
                }
            }
            else if (key.VkCode.HasValue)
            {
                _inputService.SendVirtualKey(key.VkCode.Value);
            }
            else
            {
                string text = key.CurrentDisplay;
                if (!string.IsNullOrEmpty(text))
                {
                    char c = text[0];
                    _inputService.SendChar(c);

                    if (_liveBuffer.Length > 60) _liveBuffer.Remove(0, 15);
                    _liveBuffer.Append(c);
                    UpdateLiveStream();
                }
            }

            RefreshAllKeys();
        }

        private void UpdateLiveStream()
        {
            var loc = LocalizationService.Instance;
            TxtLiveStream.Text = _liveBuffer.Length > 0 ? _liveBuffer.ToString() : loc.Get("LiveStreamReady");
            TxtStats.Text = $"{loc.Get("Keys")} {_totalKeysPressed}";
        }

        private void OnModifiersChanged()
        {
            Dispatcher.Invoke(RefreshAllKeys);
        }

        private void RefreshAllKeys()
        {
            bool shift = _inputService.IsShiftActive;
            bool caps = _inputService.IsCapsLockActive;
            bool altGr = _inputService.IsAltGrActive;
            bool ctrl = _inputService.IsCtrlActive;
            bool alt = _inputService.IsAltActive;
            bool win = _inputService.IsWinActive;

            foreach (var key in _allKeyModels)
            {
                key.RefreshState(shift, caps, altGr);

                if (key.SpecialAction == "shift") key.IsActive = shift;
                else if (key.SpecialAction == "caps") key.IsActive = caps;
                else if (key.SpecialAction == "altgr") key.IsActive = altGr;
                else if (key.SpecialAction == "ctrl") key.IsActive = ctrl;
                else if (key.SpecialAction == "alt") key.IsActive = alt;
                else if (key.SpecialAction == "win") key.IsActive = win;
            }

            foreach (var ctrlKey in _allKeyControls)
            {
                ctrlKey.UpdateVisuals();
            }
        }

        #endregion

        #region Language Switching & Active App Detection

        private void OnSystemLanguageChanged(SupportedLanguage newLang)
        {
            Dispatcher.Invoke(() =>
            {
                SwitchLayout(newLang);
                UpdateLanguageBadges(newLang);
            });
        }

        private void OnActiveApplicationChanged(string appTitle)
        {
            Dispatcher.Invoke(() =>
            {
                var loc = LocalizationService.Instance;
                TxtTargetApp.Text = $"{loc.Get("Target")}: {appTitle.ToUpperInvariant()}";
            });
        }

        private void PopulateTypingLangDropdown()
        {
            if (CmbTypingLang == null) return;

            _isUpdatingTypingLangDropdown = true;
            try
            {
                CmbTypingLang.Items.Clear();

                var langs = new (SupportedLanguage Lang, string DisplayText)[]
                {
                    (SupportedLanguage.Turkish, "🇹🇷 TR-Q"),
                    (SupportedLanguage.English, "🇺🇸 EN-US"),
                    (SupportedLanguage.FrenchAzerty, "🇫🇷 FR-AZ"),
                    (SupportedLanguage.FrenchQwerty, "🇫🇷 FR-Q"),
                    (SupportedLanguage.German, "🇩🇪 DE-QZ")
                };

                int selectedIdx = 0;
                for (int i = 0; i < langs.Length; i++)
                {
                    var item = new ComboBoxItem
                    {
                        Content = langs[i].DisplayText,
                        Tag = langs[i].Lang,
                        Focusable = false
                    };
                    CmbTypingLang.Items.Add(item);

                    if (langs[i].Lang == _languageService.CurrentLanguage)
                    {
                        selectedIdx = i;
                    }
                }

                CmbTypingLang.SelectedIndex = selectedIdx;
            }
            finally
            {
                _isUpdatingTypingLangDropdown = false;
            }
        }

        private void OnTypingLangSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_isUpdatingTypingLangDropdown) return;
            if (CmbTypingLang?.SelectedItem is ComboBoxItem item && item.Tag is SupportedLanguage lang)
            {
                SelectLanguage(lang);
            }
        }

        private void UpdateLanguageBadges(SupportedLanguage activeLang)
        {
            if (CmbTypingLang == null) return;
            _isUpdatingTypingLangDropdown = true;
            try
            {
                for (int i = 0; i < CmbTypingLang.Items.Count; i++)
                {
                    if (CmbTypingLang.Items[i] is ComboBoxItem item && item.Tag is SupportedLanguage lang && lang == activeLang)
                    {
                        CmbTypingLang.SelectedIndex = i;
                        break;
                    }
                }
            }
            finally
            {
                _isUpdatingTypingLangDropdown = false;
            }
        }

        private void SelectLanguage(SupportedLanguage lang)
        {
            _audioService.PlayKeySound(true);
            _languageService.SetLanguageManual(lang);
            SwitchLayout(lang);
            UpdateLanguageBadges(lang);
            SaveCurrentSettingsSilently();
        }

        #endregion

        #region HUD Toolbar Actions

        private void OnCutClick(object sender, RoutedEventArgs e) => _inputService.SendShortcut("cut");
        private void OnCopyClick(object sender, RoutedEventArgs e) => _inputService.SendShortcut("copy");
        private void OnPasteClick(object sender, RoutedEventArgs e) => _inputService.SendShortcut("paste");
        private void OnSelectAllClick(object sender, RoutedEventArgs e) => _inputService.SendShortcut("selectall");
        private void OnUndoClick(object sender, RoutedEventArgs e) => _inputService.SendShortcut("undo");

        private void OnCycleKeyHeightClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            _rowHeight = _rowHeight switch
            {
                56.0 => 68.0,
                68.0 => 82.0,
                82.0 => 94.0,
                94.0 => 108.0,
                108.0 => 56.0,
                _ => 68.0
            };

            BtnKeyHeight.Content = $"↕ {(int)_rowHeight}px";
            RebuildAllGrids();

            if (!_isFullscreen)
            {
                Height = _isPlayerVisible
                    ? _rowHeight * 5 + FunctionRowGrid.Height + 340
                    : _rowHeight * 5 + FunctionRowGrid.Height + 130;
            }
            SaveCurrentSettingsSilently();
        }


        #region Monitor Selection Matrix Overlay

        private void OnToggleMonitorSelectorClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            if (MonitorSelectorPopup.Visibility == Visibility.Visible)
            {
                MonitorSelectorPopup.Visibility = Visibility.Collapsed;
            }
            else
            {
                RefreshMonitorsList();
                MonitorSelectorPopup.Visibility = Visibility.Visible;
            }
        }

        private void OnCloseMonitorSelectorClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            MonitorSelectorPopup.Visibility = Visibility.Collapsed;
        }

        private void OnRefreshScreensClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            RefreshMonitorsList();
        }

        private void OnMonitorCardClick(object sender, MouseButtonEventArgs e)
        {
            var screen = (sender as FrameworkElement)?.DataContext as ScreenInfo;
            if (screen != null)
            {
                SelectScreen(screen);
            }
        }

        private void OnMonitorCardTouch(object sender, TouchEventArgs e)
        {
            var screen = (sender as FrameworkElement)?.DataContext as ScreenInfo;
            if (screen != null)
            {
                SelectScreen(screen);
            }
        }

        private void SelectScreen(ScreenInfo targetScreen)
        {
            _audioService.PlayKeySound(true);
            _currentScreenIndex = targetScreen.Index;
            foreach (var s in _screensList)
            {
                s.IsActive = (s.Index == _currentScreenIndex);
            }
            DisplayManager.MoveWindowToScreen(this, targetScreen, _isFullscreen);
            MonitorSelectorPopup.Visibility = Visibility.Collapsed;
            UpdateScreenButtonLabel();
            SaveCurrentSettingsSilently();
        }

        private void RefreshMonitorsList()
        {
            _screens = DisplayManager.GetScreens();
            _screensList.Clear();
            for (int i = 0; i < _screens.Count; i++)
            {
                _screens[i].IsActive = (i == _currentScreenIndex);
                _screensList.Add(_screens[i]);
            }
            UpdateScreenButtonLabel();
        }

        private void UpdateScreenButtonLabel()
        {
            string prefix = LocalizationService.Instance.Get("Screen");
            int total = _screens.Count > 0 ? _screens.Count : 1;
            BtnScreen.Content = $"{prefix}: {_currentScreenIndex + 1}/{total}";
        }

        private void OnCycleScreenClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            if (_screens.Count == 0) _screens = DisplayManager.GetScreens();
            if (_screens.Count == 0) return;

            _currentScreenIndex = (_currentScreenIndex + 1) % _screens.Count;
            var targetScreen = _screens[_currentScreenIndex];
            SelectScreen(targetScreen);
        }

        #endregion

        #region UI Interface Language (TR, EN, FR, DE)

        private void OnCycleUiLanguageClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            LocalizationService.Instance.CycleLanguage();
            SaveCurrentSettingsSilently();
        }

        private void OnUiLanguageChanged(UiLanguage lang)
        {
            Dispatcher.Invoke(ApplyLocalization);
        }

        private void ApplyLocalization()
        {
            var loc = LocalizationService.Instance;

            BtnUiLang.Content = $"🌐 UI: {loc.LanguageCode}";

            // Top Bar Typing Language & Actions
            TxtTypingLangLabel.Text = loc.Get("TypingLang");
            PopulateTypingLangDropdown();

            BtnCut.Content = loc.Get("Cut");
            BtnCopy.Content = loc.Get("Copy");
            BtnPaste.Content = loc.Get("Paste");
            BtnSelectAll.Content = loc.Get("SelectAll");
            BtnUndo.Content = loc.Get("Undo");

            // HUD Toolbar Right Actions
            UpdateScreenButtonLabel();
            BtnProfileManager.Content = loc.Get("Profiles");
            BtnPlayerToggle.Content = loc.Get("Player");
            BtnKeyHeight.Content = $"↕ {(int)_rowHeight}px";
            BtnFullscreen.Content = _isFullscreen ? loc.Get("Restore") : loc.Get("Fullscreen");
            BtnNumpadToggle.Content = loc.Get("Numpad");
            string sfx = loc.Get("Sound");
            BtnSoundToggle.Content = _audioService.IsSoundEnabled ? $"🔊 {sfx}" : $"🔇 {sfx}";

            // Monitor Selector Dialog
            TxtMonitorSelectorTitle.Text = loc.Get("MonitorSelectorTitle");
            TxtMonitorSelectorSubtitle.Text = loc.Get("MonitorSelectorSubtitle");
            BtnRefreshScreens.Content = loc.Get("RefreshDisplays");

            // Profile & Settings Manager Dialog
            TxtProfileManagerTitle.Text = loc.Get("ProfileManagerTitle");
            TxtActiveProfileLabel.Text = loc.Get("ActiveProfile");
            TxtAutoSavedHint.Text = loc.Get("AutoSavedHint");
            BtnSaveNewProfile.Content = loc.Get("SaveNewProfile");
            BtnUpdateCurrentProfile.Content = loc.Get("UpdateCurrentProfile");
            TxtSavedProfilesTitle.Text = loc.Get("SavedProfiles");
            TxtQuickPresetsLabel.Text = loc.Get("QuickPresets");

            // Media Deck
            if (_audioPlayer.CurrentTrack == null)
            {
                TxtTrackTitle.Text = loc.Get("NoTrackLoaded");
            }
            TxtTabPlaylist.Text = $"{loc.Get("PlaylistTab")} ({_audioPlayer.Playlist.Count})";
            TxtTabFileTree.Text = loc.Get("TreeTab");
            TxtDragHint.Text = loc.Get("DragHint");
            TxtEmptyPlaylistCue.Text = loc.Get("EmptyPlaylistCue");
            BtnAddFiles.Content = loc.Get("AddFile");
            BtnAddFolder.Content = loc.Get("AddFolder");
            BtnClearPlaylist.Content = loc.Get("ClearPlaylist");
            BtnPlaySelectedTreeItem.Content = loc.Get("Play");
            BtnAddSelectedTreeFolder.Content = loc.Get("Add");

            // Active App Detection
            if (string.IsNullOrWhiteSpace(_languageService.ActiveAppName))
            {
                TxtTargetApp.Text = loc.Get("TargetDetecting");
            }
            else
            {
                TxtTargetApp.Text = $"{loc.Get("Target")}: {_languageService.ActiveAppName.ToUpperInvariant()}";
            }

            // Live stream
            UpdateLiveStream();

            // Refresh File Tree & Screen displays with localized text
            InitFileTree();
            RefreshMonitorsList();

            // Refresh Clock Size Dropdown & Calendar
            PopulateClockSizeDropdown();
            RenderCalendar(_displayedCalMonth);

            // Refresh Digital Chrono
            UpdateClockDisplay();
        }

        #endregion

        #region Profile & Settings Manager

        private bool HandleVirtualKeyForProfileInput(KeyModel key)
        {
            if (ProfileManagerPopup == null || ProfileManagerPopup.Visibility != Visibility.Visible) return false;

            if (!string.IsNullOrEmpty(key.SpecialAction))
            {
                switch (key.SpecialAction.ToLowerInvariant())
                {
                    case "shift":
                        _inputService.ToggleShift();
                        RefreshAllKeys();
                        return true;
                    case "caps":
                        _inputService.ToggleCapsLock();
                        RefreshAllKeys();
                        return true;
                    case "altgr":
                        _inputService.ToggleAltGr();
                        RefreshAllKeys();
                        return true;
                    case "backspace":
                        _audioService.PlayKeySound(false);
                        int selStart = TxtNewProfileName.SelectionStart;
                        int selLen = TxtNewProfileName.SelectionLength;
                        if (selLen > 0)
                        {
                            TxtNewProfileName.Text = TxtNewProfileName.Text.Remove(selStart, selLen);
                            TxtNewProfileName.SelectionStart = selStart;
                        }
                        else if (selStart > 0 && TxtNewProfileName.Text.Length > 0)
                        {
                            TxtNewProfileName.Text = TxtNewProfileName.Text.Remove(selStart - 1, 1);
                            TxtNewProfileName.SelectionStart = selStart - 1;
                        }
                        TxtNewProfileName.Focus();
                        return true;
                    case "space":
                        _audioService.PlayKeySound(false);
                        InsertTextIntoProfileInput(" ");
                        return true;
                    case "enter":
                        _audioService.PlayKeySound(false);
                        OnSaveNewProfileClick(this, new RoutedEventArgs());
                        return true;
                    case "esc":
                        _audioService.PlayKeySound(false);
                        OnCloseProfileManagerClick(this, new RoutedEventArgs());
                        return true;
                    case "tab":
                        return true;
                    default:
                        return false;
                }
            }

            string charText = key.CurrentDisplay;
            if (!string.IsNullOrEmpty(charText))
            {
                _audioService.PlayKeySound(false);
                InsertTextIntoProfileInput(charText);
                return true;
            }

            return false;
        }

        private void InsertTextIntoProfileInput(string text)
        {
            int selStart = TxtNewProfileName.SelectionStart;
            int selLen = TxtNewProfileName.SelectionLength;
            if (selLen > 0)
            {
                TxtNewProfileName.Text = TxtNewProfileName.Text.Remove(selStart, selLen).Insert(selStart, text);
            }
            else
            {
                TxtNewProfileName.Text = TxtNewProfileName.Text.Insert(selStart, text);
            }
            TxtNewProfileName.SelectionStart = selStart + text.Length;
            TxtNewProfileName.Focus();
        }

        private void OnProfileNameTextBoxMouseDown(object sender, MouseButtonEventArgs e)
        {
            EnableWindowActivation(true);
            NativeMethods.SetForegroundWindow(_hwnd);
            Activate();
            TxtNewProfileName.Focus();
        }

        private void OnProfileNameTextBoxTouchDown(object sender, TouchEventArgs e)
        {
            EnableWindowActivation(true);
            NativeMethods.SetForegroundWindow(_hwnd);
            Activate();
            TxtNewProfileName.Focus();
        }

        private void OnProfileNameTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            EnableWindowActivation(true);
        }

        private void OnProfileNameTextBoxKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                OnSaveNewProfileClick(sender, e);
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                OnCloseProfileManagerClick(sender, e);
                e.Handled = true;
            }
        }

        private void OnProfileNameTextBoxTextChanged(object sender, TextChangedEventArgs e)
        {
            if (TxtNewProfileNamePlaceholder != null)
            {
                TxtNewProfileNamePlaceholder.Visibility = string.IsNullOrEmpty(TxtNewProfileName.Text)
                    ? Visibility.Visible
                    : Visibility.Collapsed;
            }
        }

        private void OnToggleProfileManagerClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            if (ProfileManagerPopup.Visibility == Visibility.Visible)
            {
                ProfileManagerPopup.Visibility = Visibility.Collapsed;
                EnableWindowActivation(false);
            }
            else
            {
                MonitorSelectorPopup.Visibility = Visibility.Collapsed;
                TxtProfileStatusFeedback.Visibility = Visibility.Collapsed;
                ProfileManagerPopup.Visibility = Visibility.Visible;
                EnableWindowActivation(true);
                NativeMethods.SetForegroundWindow(_hwnd);
                Activate();
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    TxtNewProfileName.Focus();
                    Keyboard.Focus(TxtNewProfileName);
                    TxtNewProfileName.SelectAll();
                }), System.Windows.Threading.DispatcherPriority.Input);
            }
        }

        private void OnCloseProfileManagerClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            ProfileManagerPopup.Visibility = Visibility.Collapsed;
            EnableWindowActivation(false);
        }

        private void OnQuickNameChipClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is string name)
            {
                _audioService.PlayKeySound(true);
                TxtNewProfileName.Text = name;
                TxtNewProfileName.Focus();
                TxtNewProfileName.SelectAll();
            }
        }

        private void OnSaveNewProfileClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            string name = TxtNewProfileName.Text?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = $"Özel Profil {DateTime.Now:HH:mm}";
            }

            var current = CaptureCurrentState(name);
            SettingsService.Instance.SaveProfile(name, current);
            TxtActiveProfileDisplay.Text = name;
            TxtNewProfileName.Text = string.Empty;

            ShowProfileFeedback(LocalizationService.Instance.Get("ProfileSavedFeedback"));
        }

        private void OnUpdateCurrentProfileClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            string currentName = TxtActiveProfileDisplay.Text;
            if (string.IsNullOrWhiteSpace(currentName)) currentName = "Özel Profil";

            var current = CaptureCurrentState(currentName);
            SettingsService.Instance.SaveProfile(currentName, current);

            ShowProfileFeedback(LocalizationService.Instance.Get("ProfileSavedFeedback"));
        }

        private void OnLoadProfileClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is KeyboardProfile profile)
            {
                _audioService.PlayKeySound(true);
                SettingsService.Instance.ApplyProfile(profile);
                ShowProfileFeedback(LocalizationService.Instance.Get("ProfileLoadedFeedback"));
            }
        }

        private void OnOverwriteProfileClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is KeyboardProfile profile)
            {
                _audioService.PlayKeySound(true);
                var current = CaptureCurrentState(profile.Name);
                SettingsService.Instance.SaveProfile(profile.Name, current);
                TxtActiveProfileDisplay.Text = profile.Name;
                ShowProfileFeedback(LocalizationService.Instance.Get("ProfileSavedFeedback"));
            }
        }

        private void OnDeleteProfileClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is KeyboardProfile profile)
            {
                _audioService.PlayKeySound(true);
                SettingsService.Instance.DeleteProfile(profile);
                ShowProfileFeedback(LocalizationService.Instance.Get("ProfileDeletedFeedback"));
            }
        }

        private void OnSettingsProfileApplied(KeyboardProfile profile)
        {
            Dispatcher.Invoke(() =>
            {
                ApplySettings(profile);
            });
        }

        private void ShowProfileFeedback(string message)
        {
            TxtProfileStatusFeedback.Text = message;
            TxtProfileStatusFeedback.Visibility = Visibility.Visible;
        }

        private KeyboardProfile CaptureCurrentState(string profileName = "Varsayılan")
        {
            var currentScreen = (_currentScreenIndex >= 0 && _currentScreenIndex < _screens.Count)
                ? _screens[_currentScreenIndex]
                : null;

            var playlistFiles = _audioPlayer.Playlist
                .Where(t => !string.IsNullOrEmpty(t.FilePath))
                .Select(t => t.FilePath)
                .ToList();

            return new KeyboardProfile
            {
                Name = profileName,
                LastModified = DateTime.Now,
                ScreenIndex = _currentScreenIndex,
                ScreenDeviceName = currentScreen?.DeviceName ?? string.Empty,
                IsFullscreen = _isFullscreen,
                RowHeight = _rowHeight,
                IsDockBottom = _isDockBottom,
                IsNumpadVisible = _isNumpadVisible,
                IsPlayerVisible = _isPlayerVisible,
                WindowWidth = ActualWidth > 400 ? ActualWidth : Width,
                WindowHeight = ActualHeight > 200 ? ActualHeight : Height,
                WindowLeft = Left,
                WindowTop = Top,
                Theme = ThemeManager.Instance.CurrentTheme,
                UiLanguage = LocalizationService.Instance.CurrentLanguage,
                TypingLanguage = _languageService.CurrentLanguage,
                IsSoundEnabled = _audioService.IsSoundEnabled,
                Volume = _audioPlayer.Volume,
                IsShuffle = _audioPlayer.IsShuffle,
                IsRepeat = _audioPlayer.IsRepeat,
                PlaylistFiles = playlistFiles,
                ClockSize = _currentClockSize
            };
        }

        public void ApplySettings(KeyboardProfile profile)
        {
            if (profile == null) return;

            // 1. Theme
            ThemeManager.Instance.CurrentTheme = profile.Theme;

            // 2. UI Language
            LocalizationService.Instance.CurrentLanguage = profile.UiLanguage;

            // 3. Typing Language
            SelectLanguage(profile.TypingLanguage);

            // 4. Row Height / Key Size
            _rowHeight = profile.RowHeight;
            BtnKeyHeight.Content = $"↕ {(int)_rowHeight}px";
            RebuildAllGrids();

            // 5. Sound
            _audioService.IsSoundEnabled = profile.IsSoundEnabled;
            string sfx = LocalizationService.Instance.Get("Sound");
            BtnSoundToggle.Content = _audioService.IsSoundEnabled ? $"🔊 {sfx}" : $"🔇 {sfx}";
            BtnSoundToggle.Foreground = _audioService.IsSoundEnabled ? ThemeManager.Instance.AccentBrush : Brushes.Gray;

            // 6. Dock Bottom
            _isDockBottom = profile.IsDockBottom;
            KeyboardDeckContainer.VerticalAlignment = _isDockBottom ? VerticalAlignment.Bottom : VerticalAlignment.Center;

            // 7. Numpad
            _isNumpadVisible = profile.IsNumpadVisible;
            SideDeckGrid.Visibility = _isNumpadVisible ? Visibility.Visible : Visibility.Collapsed;
            BtnNumpadToggle.Foreground = _isNumpadVisible ? ThemeManager.Instance.AccentBrush : Brushes.Gray;

            // 8. Player Deck
            _isPlayerVisible = profile.IsPlayerVisible;
            AudioPlayerDeck.Visibility = _isPlayerVisible ? Visibility.Visible : Visibility.Collapsed;
            BtnPlayerToggle.Foreground = _isPlayerVisible ? ThemeManager.Instance.AccentBrush : Brushes.Gray;

            // 9. Audio Player settings
            _audioPlayer.Volume = profile.Volume;
            VolSlider.Value = profile.Volume;
            _audioPlayer.IsShuffle = profile.IsShuffle;
            BtnShuffle.Foreground = _audioPlayer.IsShuffle ? ThemeManager.Instance.AccentBrush : Brushes.Gray;
            _audioPlayer.IsRepeat = profile.IsRepeat;
            BtnRepeat.Foreground = _audioPlayer.IsRepeat ? ThemeManager.Instance.AccentBrush : Brushes.Gray;

            // 10. Clock Sizing
            _currentClockSize = !string.IsNullOrEmpty(profile.ClockSize) ? profile.ClockSize : "Medium";
            ApplyClockSize(_currentClockSize);
            PopulateClockSizeDropdown();

            // Restore playlist tracks
            if (_audioPlayer.Playlist.Count == 0 && profile.PlaylistFiles != null && profile.PlaylistFiles.Count > 0)
            {
                var validFiles = profile.PlaylistFiles.Where(File.Exists).ToArray();
                if (validFiles.Length > 0)
                {
                    _audioPlayer.AddFiles(validFiles, autoPlayIfEmpty: false);
                }
            }

            // 10. Screen & Fullscreen
            if (_screens.Count == 0) _screens = DisplayManager.GetScreens();

            ScreenInfo? targetScreen = null;
            if (!string.IsNullOrEmpty(profile.ScreenDeviceName))
            {
                targetScreen = _screens.FirstOrDefault(s => string.Equals(s.DeviceName, profile.ScreenDeviceName, StringComparison.OrdinalIgnoreCase));
            }
            if (targetScreen == null && profile.ScreenIndex >= 0 && profile.ScreenIndex < _screens.Count)
            {
                targetScreen = _screens[profile.ScreenIndex];
            }
            targetScreen ??= (_screens.Count > 0 ? _screens[0] : null);

            if (targetScreen != null)
            {
                _currentScreenIndex = targetScreen.Index;
                foreach (var s in _screensList)
                {
                    s.IsActive = (s.Index == _currentScreenIndex);
                }
                UpdateScreenButtonLabel();

                _isFullscreen = profile.IsFullscreen;
                if (_isFullscreen)
                {
                    DisplayManager.MoveWindowToScreen(this, targetScreen, true);
                    BtnFullscreen.Content = LocalizationService.Instance.Get("Restore");
                }
                else
                {
                    WindowStyle = WindowStyle.None;
                    ResizeMode = ResizeMode.CanResize;
                    Width = profile.WindowWidth > 400 ? profile.WindowWidth : 1240;
                    Height = _isPlayerVisible
                        ? _rowHeight * 5 + FunctionRowGrid.Height + 340
                        : _rowHeight * 5 + FunctionRowGrid.Height + 130;
                    Left = profile.WindowLeft != 0 ? profile.WindowLeft : targetScreen.Left + (targetScreen.Width - Width) / 2.0;
                    Top = profile.WindowTop != 0 ? profile.WindowTop : targetScreen.Top + (targetScreen.Height - Height) / 2.0;
                    BtnFullscreen.Content = LocalizationService.Instance.Get("Fullscreen");
                }
            }

            ApplyCurrentTheme();
            ApplyLocalization();

            if (!string.IsNullOrEmpty(profile.Name))
            {
                TxtActiveProfileDisplay.Text = profile.Name;
            }
        }

        private void SaveCurrentSettingsSilently()
        {
            try
            {
                string activeName = TxtActiveProfileDisplay?.Text ?? "Aktif Profil";
                if (string.IsNullOrWhiteSpace(activeName)) activeName = "Aktif Profil";
                var current = CaptureCurrentState(activeName);
                SettingsService.Instance.SaveActiveSettings(current);
            }
            catch
            {
                // Silent fail
            }
        }

        #endregion

        private void OnToggleFullscreenClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            _isFullscreen = !_isFullscreen;

            if (_screens.Count == 0) _screens = DisplayManager.GetScreens();
            var screen = (_currentScreenIndex < _screens.Count) ? _screens[_currentScreenIndex] : _screens[0];

            if (_isFullscreen)
            {
                _isPlayerVisible = true;
                AudioPlayerDeck.Visibility = Visibility.Visible;
                BtnPlayerToggle.Foreground = ThemeManager.Instance.AccentBrush;

                DisplayManager.MoveWindowToScreen(this, screen, true);
                BtnFullscreen.Content = LocalizationService.Instance.Get("Restore");
            }
            else
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.CanResize;
                Width = 1240;
                Height = _isPlayerVisible
                    ? _rowHeight * 5 + FunctionRowGrid.Height + 340
                    : _rowHeight * 5 + FunctionRowGrid.Height + 130;
                Left = screen.Left + (screen.Width - 1240) / 2.0;
                Top = screen.Top + (screen.Height - Height) / 2.0;
                BtnFullscreen.Content = LocalizationService.Instance.Get("Fullscreen");
            }
            SaveCurrentSettingsSilently();
        }

        private void OnToggleNumpadClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            _isNumpadVisible = !_isNumpadVisible;
            SideDeckGrid.Visibility = _isNumpadVisible ? Visibility.Visible : Visibility.Collapsed;
            BtnNumpadToggle.Foreground = _isNumpadVisible ? ThemeManager.Instance.AccentBrush : Brushes.Gray;
            SaveCurrentSettingsSilently();
        }

        private void OnCycleThemeClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            ThemeManager.Instance.CycleTheme();
            SaveCurrentSettingsSilently();
        }

        private void OnToggleSoundClick(object sender, RoutedEventArgs e)
        {
            _audioService.IsSoundEnabled = !_audioService.IsSoundEnabled;
            string sfx = LocalizationService.Instance.Get("Sound");
            BtnSoundToggle.Content = _audioService.IsSoundEnabled ? $"🔊 {sfx}" : $"🔇 {sfx}";
            BtnSoundToggle.Foreground = _audioService.IsSoundEnabled ? ThemeManager.Instance.AccentBrush : Brushes.Gray;
            SaveCurrentSettingsSilently();
        }

        private void OnTopBarMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && !_isFullscreen)
            {
                DragMove();
            }
        }

        private void OnMinimizeClick(object sender, RoutedEventArgs e)
        {
            _audioService.PlayKeySound(true);
            SaveCurrentSettingsSilently();
            WindowState = WindowState.Minimized;
        }

        private void OnWindowStateChanged(object? sender, EventArgs e)
        {
            try
            {
                string logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "CyberKeyboard", "app_debug.log");
                File.AppendAllText(logPath, $"[{DateTime.Now}] OnWindowStateChanged: State={WindowState}\n");
            }
            catch { }

            if (WindowState == WindowState.Normal)
            {
                if (_screens == null || _screens.Count == 0) _screens = DisplayManager.GetScreens();
                var screen = (_screens != null && _currentScreenIndex < _screens.Count)
                    ? _screens[_currentScreenIndex]
                    : (_screens?.Count > 0 ? _screens[0] : null);

                if (screen != null && _isFullscreen)
                {
                    DisplayManager.MoveWindowToScreen(this, screen, true);
                }

                if (_hwnd != IntPtr.Zero)
                {
                    NativeMethods.SetWindowPos(_hwnd, NativeMethods.HWND_TOPMOST, 0, 0, 0, 0,
                        NativeMethods.SWP_NOMOVE | NativeMethods.SWP_NOSIZE | NativeMethods.SWP_NOACTIVATE | NativeMethods.SWP_SHOWWINDOW);
                }
            }
            else if (WindowState == WindowState.Minimized)
            {
                SaveCurrentSettingsSilently();
            }
        }

        private ImageSource CreateCyberDeckIcon()
        {
            int size = 64;
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                // Dark neon cyber background
                dc.DrawRoundedRectangle(
                    new SolidColorBrush(Color.FromRgb(8, 13, 24)),
                    new Pen(new SolidColorBrush(Color.FromRgb(0, 240, 255)), 2.5),
                    new Rect(2, 2, size - 4, size - 4),
                    10, 10);

                // Cyber keyboard grid / key slots
                var keyBrush = new SolidColorBrush(Color.FromArgb(200, 0, 240, 255));
                var accentBrush = new SolidColorBrush(Color.FromRgb(255, 0, 128));

                // Draw 3 rows of mini cyber keys
                int keyW = 8;
                int keyH = 7;
                int gap = 3;
                int startX = 10;
                int startY = 16;

                for (int r = 0; r < 3; r++)
                {
                    int cols = (r == 2) ? 4 : 5;
                    int y = startY + r * (keyH + gap);
                    for (int c = 0; c < cols; c++)
                    {
                        int x = startX + c * (keyW + gap);
                        var brush = (r == 1 && c == 2) ? accentBrush : keyBrush;
                        dc.DrawRoundedRectangle(brush, null, new Rect(x, y, keyW, keyH), 1.5, 1.5);
                    }
                }

                // Draw spacebar
                dc.DrawRoundedRectangle(keyBrush, null, new Rect(startX + 10, startY + 3 * (keyH + gap), 24, keyH), 1.5, 1.5);

                // Draw glowing LED power indicator
                dc.DrawEllipse(new SolidColorBrush(Color.FromRgb(0, 255, 136)), null, new Point(size - 12, 12), 2.5, 2.5);
            }

            var rtb = new RenderTargetBitmap(size, size, 96, 96, PixelFormats.Pbgra32);
            rtb.Render(visual);
            rtb.Freeze();
            return rtb;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            SaveCurrentSettingsSilently();
            base.OnClosing(e);
        }

        private void OnCloseClick(object sender, RoutedEventArgs e)
        {
            SaveCurrentSettingsSilently();
            _clockTimer.Stop();
            _audioPlayer.Stop();
            _languageService.Stop();
            Close();
        }

        #endregion

        #region Theme & Appearance

        private void OnThemeChanged()
        {
            Dispatcher.Invoke(() =>
            {
                ApplyCurrentTheme();
                UpdateLanguageBadges(_languageService.CurrentLanguage);
            });
        }

        private void ApplyCurrentTheme()
        {
            var theme = ThemeManager.Instance;
            bool isGlitch = theme.IsGlitchTheme;

            BtnTheme.Content = $"🎨 {theme.GetThemeDisplayName()}";

            if (isGlitch)
            {
                TxtBrandTitle.Text = "CYBER_DECK";
                TxtBrandCursor.Visibility = Visibility.Collapsed;
                BadgeCarrierBpm.Visibility = Visibility.Collapsed;
                MainShell.Effect = null;
                AudioPlayerDeck.Effect = null;

                // Retro-Futurist Glitch Theme styling: Raw pixelated monospace, high contrast Cyan vs Magenta
                MainShell.BorderBrush = theme.SecondaryBrush; // Jarring Neon Magenta
                MainShell.CornerRadius = new CornerRadius(4); // Sharper raw edges
                AudioPlayerDeck.Background = new SolidColorBrush(Color.FromArgb(248, 8, 6, 22));
                AudioPlayerDeck.BorderBrush = theme.SecondaryBrush;

                TxtClockHoursMinutes.Foreground = theme.AccentBrush; // Electric Cyan
                TxtClockHoursMinutes.FontFamily = new FontFamily("Consolas, Courier New, monospace");
                TxtClockSeconds.Foreground = theme.SecondaryBrush; // Neon Magenta
                TxtClockSeconds.FontFamily = new FontFamily("Consolas, Courier New, monospace");
                TxtClockDayOfWeek.Foreground = theme.AccentBrush;
                ClockDayBadge.BorderBrush = theme.SecondaryBrush;

                if (GlitchCrtOverlay != null)
                {
                    GlitchCrtOverlay.Visibility = Visibility.Visible;
                    GlitchCrtOverlay.CornerRadius = new CornerRadius(4);
                    GlitchCrtOverlay.Opacity = 1.0;
                }

                if (CyberCalendarContainer != null)
                {
                    CyberCalendarContainer.BorderBrush = theme.SecondaryBrush;
                    CyberCalendarContainer.CornerRadius = new CornerRadius(3);
                }
            }
            else if (theme.Is80sTheme)
            {
                // 80s Retro Arcade / Synthwave styling (Matching reference style: #00F0FF vs #FF0055)
                TxtBrandTitle.Text = "SYS://80S_SYNTH.DECK";
                TxtBrandCursor.Visibility = Visibility.Visible;
                BadgeCarrierBpm.Visibility = Visibility.Visible;

                MainShell.BorderBrush = theme.AccentBrush; // High-Voltage Neon Cyan
                MainShell.CornerRadius = new CornerRadius(6);
                MainShell.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(255, 0, 85),
                    Direction = 315,
                    ShadowDepth = 4.0,
                    BlurRadius = 0,
                    Opacity = 0.95
                };

                AudioPlayerDeck.Background = new SolidColorBrush(Color.FromArgb(248, 6, 8, 20));
                AudioPlayerDeck.BorderBrush = theme.SecondaryBrush;
                AudioPlayerDeck.CornerRadius = new CornerRadius(6);
                AudioPlayerDeck.Effect = new DropShadowEffect
                {
                    Color = Color.FromRgb(0, 240, 255),
                    Direction = 315,
                    ShadowDepth = 3.5,
                    BlurRadius = 0,
                    Opacity = 0.90
                };

                TxtClockHoursMinutes.Foreground = theme.AccentBrush; // Neon Cyan
                TxtClockHoursMinutes.FontFamily = new FontFamily("Consolas, Cascadia Code, Segoe UI Semibold, monospace");
                TxtClockSeconds.Foreground = theme.SecondaryBrush; // Arcade Hot Pink
                TxtClockSeconds.FontFamily = new FontFamily("Consolas, Cascadia Code, Segoe UI Semibold, monospace");
                TxtClockDayOfWeek.Foreground = theme.AccentBrush;
                ClockDayBadge.BorderBrush = theme.SecondaryBrush;

                if (GlitchCrtOverlay != null)
                {
                    GlitchCrtOverlay.Visibility = Visibility.Visible;
                    GlitchCrtOverlay.CornerRadius = new CornerRadius(6);
                    GlitchCrtOverlay.Opacity = 0.25;
                }

                if (CyberCalendarContainer != null)
                {
                    CyberCalendarContainer.BorderBrush = theme.SecondaryBrush;
                    CyberCalendarContainer.CornerRadius = new CornerRadius(5);
                }
            }
            else
            {
                // Standard Themes (100% untouched original styling)
                TxtBrandTitle.Text = "CYBER_DECK";
                TxtBrandCursor.Visibility = Visibility.Collapsed;
                BadgeCarrierBpm.Visibility = Visibility.Collapsed;
                MainShell.Effect = null;
                AudioPlayerDeck.Effect = null;

                MainShell.BorderBrush = theme.AccentBrush;
                MainShell.CornerRadius = new CornerRadius(12);
                AudioPlayerDeck.Background = new SolidColorBrush(Color.FromArgb(242, 6, 10, 18));
                AudioPlayerDeck.BorderBrush = new SolidColorBrush(Color.FromArgb(40, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));

                TxtClockHoursMinutes.Foreground = theme.AccentBrush;
                TxtClockHoursMinutes.FontFamily = new FontFamily("Consolas, Segoe UI");
                TxtClockSeconds.Foreground = new SolidColorBrush(Color.FromRgb(0, 255, 136));
                TxtClockSeconds.FontFamily = new FontFamily("Consolas, Segoe UI");
                TxtClockDayOfWeek.Foreground = Brushes.White;
                ClockDayBadge.BorderBrush = theme.AccentBrush;

                if (GlitchCrtOverlay != null)
                {
                    GlitchCrtOverlay.Visibility = Visibility.Collapsed;
                }

                if (CyberCalendarContainer != null)
                {
                    CyberCalendarContainer.BorderBrush = new SolidColorBrush(Color.FromArgb(40, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
                    CyberCalendarContainer.CornerRadius = new CornerRadius(5);
                }
            }

            RenderCalendar(_displayedCalMonth);

            foreach (var ctrlKey in _allKeyControls)
            {
                ctrlKey.UpdateVisuals();
            }
        }

        #endregion
    }
}