using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using CyberKeyboard.Graphics;
using CyberKeyboard.Models;

namespace CyberKeyboard.Controls
{
    public class CyberKeyControl : ContentControl
    {
        public static readonly DependencyProperty KeyModelProperty =
            DependencyProperty.Register(nameof(KeyModel), typeof(KeyModel), typeof(CyberKeyControl),
                new PropertyMetadata(null, OnKeyModelChanged));

        public KeyModel? KeyModel
        {
            get => (KeyModel?)GetValue(KeyModelProperty);
            set => SetValue(KeyModelProperty, value);
        }

        public Action<KeyModel, Point>? KeyTriggered;

        private Border? _outerBorder;
        private Ellipse? _ledIndicator;
        private TextBlock? _mainText;
        private TextBlock? _shiftText;
        private TextBlock? _altGrText;
        private long _lastTouchTimestamp = 0;
        private bool _isPressed = false;

        private static readonly DropShadowEffect Shadow80sPink = new DropShadowEffect
        {
            Color = Color.FromRgb(255, 0, 85),
            Direction = 315,
            ShadowDepth = 3.5,
            BlurRadius = 0,
            Opacity = 1.0
        };

        private static readonly DropShadowEffect Shadow80sCyan = new DropShadowEffect
        {
            Color = Color.FromRgb(0, 240, 255),
            Direction = 315,
            ShadowDepth = 3.5,
            BlurRadius = 0,
            Opacity = 1.0
        };

        static CyberKeyControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CyberKeyControl),
                new FrameworkPropertyMetadata(typeof(CyberKeyControl)));
            Shadow80sPink.Freeze();
            Shadow80sCyan.Freeze();
        }

        public CyberKeyControl()
        {
            Focusable = false;
            IsTabStop = false;
            Stylus.SetIsPressAndHoldEnabled(this, false);
            Stylus.SetIsFlicksEnabled(this, false);

            PreviewTouchDown += OnPreviewTouchDown;
            PreviewTouchUp += OnPreviewTouchUp;
            PreviewMouseDown += OnPreviewMouseDown;
            PreviewMouseUp += OnPreviewMouseUp;
            MouseLeave += OnMouseLeave;
            TouchLeave += OnTouchLeave;

            ThemeManager.Instance.ThemeChanged += OnThemeChanged;
        }

        private static void OnKeyModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CyberKeyControl control && e.NewValue is KeyModel)
            {
                control.UpdateVisuals();
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _outerBorder = GetTemplateChild("PART_OuterBorder") as Border;
            _ledIndicator = GetTemplateChild("PART_LedIndicator") as Ellipse;
            _mainText = GetTemplateChild("PART_MainText") as TextBlock;
            _shiftText = GetTemplateChild("PART_ShiftText") as TextBlock;
            _altGrText = GetTemplateChild("PART_AltGrText") as TextBlock;

            UpdateVisuals();
        }

        private void OnThemeChanged()
        {
            UpdateVisuals();
        }

        public void UpdateVisuals()
        {
            if (KeyModel == null || _outerBorder == null) return;

            var theme = ThemeManager.Instance;
            bool isGlitch = theme.IsGlitchTheme;

            bool isAccentKey = KeyModel.Type == KeyType.Action || KeyModel.SpecialAction == "enter" || KeyModel.SpecialAction == "backspace";
            bool isModifier = KeyModel.Type == KeyType.Modifier;

            if (isGlitch)
            {
                _outerBorder.Effect = null;
                // Retro-Futurist Glitch Art styling: Raw pixelated monospace, sharp edges, jarring Cyan vs Magenta
                _outerBorder.CornerRadius = new CornerRadius(3);

                var monoFont = new FontFamily("Consolas, Courier New, monospace");
                if (_mainText != null) _mainText.FontFamily = monoFont;
                if (_shiftText != null)
                {
                    _shiftText.FontFamily = monoFont;
                    _shiftText.Foreground = theme.AccentBrush; // Electric Cyan
                }
                if (_altGrText != null)
                {
                    _altGrText.FontFamily = monoFont;
                    _altGrText.Foreground = theme.SecondaryBrush; // Jarring Magenta
                }

                if (_isPressed)
                {
                    _outerBorder.Background = Brushes.White;
                    _outerBorder.BorderBrush = theme.SecondaryBrush;
                    if (_mainText != null) _mainText.Foreground = Brushes.Black;
                }
                else if (KeyModel.IsActive)
                {
                    _outerBorder.Background = new SolidColorBrush(Color.FromArgb(220, 0, 240, 255)); // Electric Cyan
                    _outerBorder.BorderBrush = theme.SecondaryBrush;
                    if (_mainText != null) _mainText.Foreground = Brushes.Black;
                }
                else if (isAccentKey)
                {
                    _outerBorder.Background = new SolidColorBrush(Color.FromArgb(190, 255, 0, 128)); // Vivid Neon Magenta
                    _outerBorder.BorderBrush = theme.AccentBrush; // Cyan Edge
                    if (_mainText != null) _mainText.Foreground = Brushes.White;
                }
                else if (isModifier)
                {
                    _outerBorder.Background = new SolidColorBrush(Color.FromArgb(220, 20, 10, 32)); // Dark Obsidian
                    _outerBorder.BorderBrush = theme.AccentBrush; // Cyan Edge
                    if (_mainText != null) _mainText.Foreground = Brushes.White;
                }
                else
                {
                    _outerBorder.Background = theme.KeyBackgroundBrush;
                    _outerBorder.BorderBrush = theme.KeyBorderBrush; // Magenta Edge
                    if (_mainText != null) _mainText.Foreground = Brushes.White;
                }

                if (_ledIndicator != null)
                {
                    _ledIndicator.Visibility = (KeyModel.SpecialAction == "caps" || KeyModel.SpecialAction == "shift")
                        ? Visibility.Visible : Visibility.Collapsed;
                    _ledIndicator.Fill = KeyModel.IsActive ? theme.AccentBrush : theme.SecondaryBrush;
                }
            }
            else if (theme.Is80sTheme)
            {
                // 80s Retro Arcade / Synthwave Keycaps (Matching reference image: #00F0FF vs #FF0055 with 3D offset shadow)
                _outerBorder.CornerRadius = new CornerRadius(3);
                _outerBorder.BorderThickness = new Thickness(1.5);

                var retroFont = new FontFamily("Consolas, Cascadia Code, Segoe UI, monospace");
                if (_mainText != null)
                {
                    _mainText.FontFamily = retroFont;
                    _mainText.FontWeight = FontWeights.Bold;
                }
                if (_shiftText != null)
                {
                    _shiftText.FontFamily = retroFont;
                    _shiftText.Foreground = new SolidColorBrush(Color.FromRgb(0, 240, 255)); // Neon Cyan
                }
                if (_altGrText != null)
                {
                    _altGrText.FontFamily = retroFont;
                    _altGrText.Foreground = new SolidColorBrush(Color.FromRgb(255, 0, 85)); // Arcade Hot Pink
                }

                bool is80sAccent = isAccentKey || KeyModel.Type == KeyType.Space;
                bool is80sModifier = isModifier || KeyModel.Type == KeyType.Navigation || KeyModel.Type == KeyType.Function;

                if (_isPressed)
                {
                    // Depressed 3D state: plunges into socket (shadow collapses, key shifts down-right)
                    _outerBorder.Effect = null;
                    _outerBorder.RenderTransform = new TranslateTransform(2.5, 2.5);
                    _outerBorder.Background = Brushes.White;
                    _outerBorder.BorderBrush = theme.SecondaryBrush;
                    if (_mainText != null) _mainText.Foreground = Brushes.Black;
                }
                else
                {
                    _outerBorder.RenderTransform = Transform.Identity;

                    if (KeyModel.IsActive)
                    {
                        // Active Toggled Key (Caps Lock / Shift on): High-voltage Cyan with Black text (matching PLAY button in reference)
                        _outerBorder.Background = new SolidColorBrush(Color.FromRgb(0, 240, 255));
                        _outerBorder.BorderBrush = theme.SecondaryBrush;
                        if (_mainText != null) _mainText.Foreground = Brushes.Black;
                        _outerBorder.Effect = Shadow80sPink;
                    }
                    else if (is80sAccent)
                    {
                        // Accent Key (Enter, Space, Backspace): Intense Hot Pink fill with Cyan border & Cyan 3D shadow
                        _outerBorder.Background = new SolidColorBrush(Color.FromRgb(255, 0, 85));
                        _outerBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 240, 255));
                        if (_mainText != null) _mainText.Foreground = Brushes.White;
                        _outerBorder.Effect = Shadow80sCyan;
                    }
                    else if (is80sModifier)
                    {
                        // Modifier Key (Ctrl, Alt, Win, Nav, Fn): Deep Obsidian Blue with Hot Pink border & Cyan 3D shadow
                        _outerBorder.Background = new SolidColorBrush(Color.FromRgb(12, 16, 28));
                        _outerBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(255, 0, 85));
                        if (_mainText != null) _mainText.Foreground = new SolidColorBrush(Color.FromRgb(0, 240, 255));
                        _outerBorder.Effect = Shadow80sCyan;
                    }
                    else
                    {
                        // Standard Key (letters & numbers): Pitch Matte Void fill with Neon Cyan border & Hot Pink 3D shadow
                        _outerBorder.Background = new SolidColorBrush(Color.FromRgb(8, 10, 18));
                        _outerBorder.BorderBrush = new SolidColorBrush(Color.FromRgb(0, 240, 255));
                        if (_mainText != null) _mainText.Foreground = Brushes.White;
                        _outerBorder.Effect = Shadow80sPink;
                    }
                }

                if (_ledIndicator != null)
                {
                    _ledIndicator.Visibility = (KeyModel.SpecialAction == "caps" || KeyModel.SpecialAction == "shift")
                        ? Visibility.Visible : Visibility.Collapsed;
                    _ledIndicator.Fill = KeyModel.IsActive ? theme.AccentBrush : theme.SecondaryBrush;
                }
            }
            else
            {
                // Standard Themes (100% untouched original styling)
                _outerBorder.Effect = null;
                _outerBorder.BorderThickness = new Thickness(1.5);
                _outerBorder.CornerRadius = new CornerRadius(8);

                var defaultFont = new FontFamily("Segoe UI, Consolas, sans-serif");
                if (_mainText != null)
                {
                    _mainText.FontFamily = defaultFont;
                    _mainText.Foreground = Brushes.White;
                }
                if (_shiftText != null)
                {
                    _shiftText.FontFamily = defaultFont;
                    _shiftText.Foreground = new SolidColorBrush(Color.FromRgb(156, 213, 255));
                }
                if (_altGrText != null)
                {
                    _altGrText.FontFamily = defaultFont;
                    _altGrText.Foreground = new SolidColorBrush(Color.FromRgb(255, 170, 0));
                }

                if (_isPressed)
                {
                    _outerBorder.Background = new SolidColorBrush(Color.FromArgb(220, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
                    _outerBorder.BorderBrush = Brushes.White;
                }
                else if (KeyModel.IsActive)
                {
                    _outerBorder.Background = new SolidColorBrush(Color.FromArgb(170, theme.AccentColor.R, theme.AccentColor.G, theme.AccentColor.B));
                    _outerBorder.BorderBrush = theme.AccentBrush;
                }
                else if (isAccentKey)
                {
                    _outerBorder.Background = new SolidColorBrush(Color.FromArgb(190, 16, 34, 60));
                    _outerBorder.BorderBrush = theme.SecondaryBrush;
                }
                else if (isModifier)
                {
                    _outerBorder.Background = new SolidColorBrush(Color.FromArgb(180, 20, 28, 48));
                    _outerBorder.BorderBrush = theme.KeyBorderBrush;
                }
                else
                {
                    _outerBorder.Background = theme.KeyBackgroundBrush;
                    _outerBorder.BorderBrush = theme.KeyBorderBrush;
                }

                if (_ledIndicator != null)
                {
                    _ledIndicator.Visibility = (KeyModel.SpecialAction == "caps" || KeyModel.SpecialAction == "shift")
                        ? Visibility.Visible : Visibility.Collapsed;
                    _ledIndicator.Fill = KeyModel.IsActive ? theme.AccentBrush : new SolidColorBrush(Color.FromArgb(60, 120, 140, 160));
                }
            }
        }

        private void OnPreviewTouchDown(object? sender, TouchEventArgs e)
        {
            e.Handled = true;
            _lastTouchTimestamp = Environment.TickCount64;

            _isPressed = true;
            PressVisual();

            if (KeyModel != null)
            {
                KeyTriggered?.Invoke(KeyModel, default);
            }
        }

        private void OnPreviewTouchUp(object? sender, TouchEventArgs e)
        {
            e.Handled = true;
            _isPressed = false;
            ReleaseVisual();
        }

        private void OnTouchLeave(object? sender, TouchEventArgs e)
        {
            _isPressed = false;
            ReleaseVisual();
        }

        private void OnPreviewMouseDown(object? sender, MouseButtonEventArgs e)
        {
            // CRUCIAL: Ignore mouse events generated by touch/stylus emulation to prevent double-press!
            if (e.StylusDevice != null)
            {
                e.Handled = true;
                return;
            }

            // Also debounce if a real touch occurred within 350ms
            if (Environment.TickCount64 - _lastTouchTimestamp < 350)
            {
                e.Handled = true;
                return;
            }

            e.Handled = true;
            _isPressed = true;
            PressVisual();

            if (KeyModel != null)
            {
                KeyTriggered?.Invoke(KeyModel, default);
            }
        }

        private void OnPreviewMouseUp(object? sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
            _isPressed = false;
            ReleaseVisual();
        }

        private void OnMouseLeave(object? sender, MouseEventArgs e)
        {
            _isPressed = false;
            ReleaseVisual();
        }

        private void PressVisual()
        {
            if (_outerBorder == null) return;
            var theme = ThemeManager.Instance;
            if (theme.Is80sTheme)
            {
                // Physical 3D keycap downward depression into socket
                _outerBorder.BorderThickness = new Thickness(1.5);
                _outerBorder.RenderTransform = new TranslateTransform(2.5, 2.5);
            }
            else
            {
                _outerBorder.BorderThickness = new Thickness(1.5);
                _outerBorder.RenderTransform = new ScaleTransform(0.95, 0.95, ActualWidth / 2, ActualHeight / 2);
            }
            _outerBorder.BorderBrush = theme.AccentBrush;
            UpdateVisuals();
        }

        private void ReleaseVisual()
        {
            if (_outerBorder == null) return;
            var theme = ThemeManager.Instance;
            _outerBorder.RenderTransform = Transform.Identity;
            _outerBorder.BorderThickness = new Thickness(1.5);
            UpdateVisuals();
        }
    }
}
