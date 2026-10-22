using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace CyberKeyboard.Native
{
    public class ScreenInfo : System.ComponentModel.INotifyPropertyChanged
    {
        public int Index { get; set; }
        public string DeviceName { get; set; } = string.Empty;
        public int Left { get; set; }
        public int Top { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public bool IsPrimary { get; set; }

        private bool _isActive;
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive != value)
                {
                    _isActive = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ActiveBadgeVisibility));
                }
            }
        }

        public Visibility ActiveBadgeVisibility => IsActive ? Visibility.Visible : Visibility.Collapsed;
        public string ActiveBadgeText => $"● {Services.LocalizationService.Instance.Get("ActiveBadge")}";

        public string Title => $"MONITOR {Index + 1}";
        public string ResolutionString => $"{Width} × {Height}";
        public string PositionString => $"X: {Left}, Y: {Top}";
        public string DeviceTag => IsPrimary 
            ? Services.LocalizationService.Instance.Get("PrimaryBadge") 
            : Services.LocalizationService.Instance.Get("ExternalTouchBadge");

        public string DisplayName => $"Monitor {Index + 1} {(IsPrimary ? "(Primary)" : "(External/Touch)")} - {Width}x{Height}";

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));
        }
    }

    public static class DisplayManager
    {
        public static List<ScreenInfo> GetScreens()
        {
            var screens = new List<ScreenInfo>();
            int index = 0;

            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                (IntPtr hMonitor, IntPtr hdcMonitor, ref NativeMethods.RECT lprcMonitor, IntPtr dwData) =>
                {
                    var mi = new NativeMethods.MONITORINFOEX();
                    mi.cbSize = Marshal.SizeOf(typeof(NativeMethods.MONITORINFOEX));
                    if (NativeMethods.GetMonitorInfo(hMonitor, ref mi))
                    {
                        screens.Add(new ScreenInfo
                        {
                            Index = index++,
                            DeviceName = mi.szDevice ?? $"Screen {index}",
                            Left = mi.rcMonitor.Left,
                            Top = mi.rcMonitor.Top,
                            Width = mi.rcMonitor.Width,
                            Height = mi.rcMonitor.Height,
                            IsPrimary = (mi.dwFlags & NativeMethods.MONITORINFOF_PRIMARY) != 0
                        });
                    }
                    return true;
                }, IntPtr.Zero);

            return screens;
        }

        public static void MoveWindowToScreen(Window window, ScreenInfo screen, bool fullscreen = true)
        {
            if (window == null || screen == null) return;

            // Get DPI factor of current window or system
            var source = PresentationSource.FromVisual(window);
            double dpiX = 1.0;
            double dpiY = 1.0;

            if (source?.CompositionTarget != null)
            {
                dpiX = source.CompositionTarget.TransformToDevice.M11;
                dpiY = source.CompositionTarget.TransformToDevice.M22;
            }

            // Convert physical pixels to WPF DIPs
            double leftDip = screen.Left / dpiX;
            double topDip = screen.Top / dpiY;
            double widthDip = screen.Width / dpiX;
            double heightDip = screen.Height / dpiY;

            window.WindowState = WindowState.Normal;
            window.Left = leftDip;
            window.Top = topDip;
            window.Width = widthDip;
            window.Height = heightDip;

            if (fullscreen)
            {
                window.WindowStyle = WindowStyle.None;
                window.ResizeMode = ResizeMode.CanMinimize;
            }
        }
    }
}
